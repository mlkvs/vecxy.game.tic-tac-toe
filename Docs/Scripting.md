# Luau scripting в Vecxy

Vecxy загружает `.luau` как обычные ассеты. Скрипт может описывать UI, анимации или всю игру; C# остаётся равноправным вариантом. Runtime использует нативную Luau VM через NuLua и не требует Node.js.

## Минимальный запуск

Создайте `Assets/Scripts/Game.luau`:

```luau
local Game = {}

function Game.initialize()
    ui.SetStatus("Игра запущена")
end

return Game
```

Передайте скрипту только нужные C#-объекты:

```csharp
var context = new ScriptContext()
    .Add("ui", boardView)
    .Add("game", model);

var controller = scripts.Create(Assets.Scripts.Game, context);
controller.Invoke("initialize");
```

Каждый public-метод и читаемое public-свойство capability доступны в таблице Luau. Примитивы, массивы и простые DTO автоматически преобразуются в обе стороны. CLR целиком скрипту не открыт.

## Модули и require

Обычный модуль возвращает таблицу:

```luau
local Rules = {}

function Rules.isValid(index)
    return index >= 0 and index < 9
end

return Rules
```

Импорт пишется в начале использующего файла привычным для Luau способом:

```luau
local Rules = require("./Rules")
local Messages = require("Scripts/Messages")
```

Расширение можно опустить: `./Rules` означает `./Rules.luau`. Относительный путь считается от текущего модуля, а путь без `.` — от корня ассетов. URL, абсолютные файловые пути, выход через `..` и импорт файлов другого типа запрещены. Результат модуля кешируется внутри одной VM.

Asset pipeline находит статические вызовы `require("...")`, записывает их в `Assets.manifest` и добавляет зависимые скрипты в VPack. Поэтому production-сборке не нужны loose-файлы. Для зависимости из другого пакета пакет должен быть загружен и явно указан в `dependencies` владельца.

## Lifecycle

Entry-модуль возвращает таблицу с любыми функциями, которые вызывает C#:

```luau
local Controller = {}

function Controller.initialize() end
function Controller.onReload() end
function Controller.dispose() end

return Controller
```

- `initialize` приложение вызывает после создания instance.
- `onReload` runtime вызывает на новой VM перед атомарной заменой старой.
- `dispose` вызывается при освобождении instance.
- Остальные функции доступны через `Invoke` и `InvokeOptional`.

Runtime отслеживает версии entry-файла и всех транзитивных `require`. При изменении любого модуля граф загружается заново. Если компиляция или `onReload` завершились ошибкой, рабочая VM сохраняется; повторная попытка произойдёт после следующего изменения ассета.

## C# и Luau вместе

Граница интеграции строится через capability:

```csharp
public sealed class GameApi
{
    public string CurrentPlayer { get; private set; } = "X";
    public MoveResult TryMove(int index) => /* ... */;
    public void Reset() => /* ... */;
}
```

```luau
local result = game.TryMove(index)
if result.Accepted then
    ui.SetCell(index, result.Player)
end
```

Это позволяет выбрать архитектуру на уровне проекта: чистый C#, полностью Luau или гибрид с типизированной моделью на C# и изменяемым контроллером на Luau.

## Примеры

- `TicTacToe.Game` — исходный чистый C# подход.
- `TicTacToe.Scripted` — правила, состояние и контроллер на Luau; C# предоставляет UI capability.
- `TicTacToe.Hybrid` — модель на C#, UI-flow и сообщения на Luau.

## Сетевой код и безопасность

Упаковка и загрузка скрипта через сеть не означают, что ему можно доверять. Перед production-запуском недоверенного контента нужны подпись пакета, лимиты памяти и времени/инструкций, разрешённый список capability, квоты на ресурсы и политика версий API. Текущая capability-модель уже не открывает CLR, но сама по себе не является полной sandbox-границей для произвольного сетевого кода.
