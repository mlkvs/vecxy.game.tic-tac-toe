# Крестики-нолики на Vecxy

Минимальный Get Started-проект для знакомства с игровым движком **Vecxy**.

Здесь небольшая законченная игра создана на чистом UI: без сцены, физических объектов и собственного игрового цикла. На её примере можно увидеть базовый путь от конфигурации приложения до интерактивного интерфейса:

- объявление приложения и игрового слоя;
- подключение слоя через YAML-конфигурацию;
- загрузка UI-документа из ассетов;
- создание переиспользуемых XML-компонентов;
- оформление интерфейса через CSS;
- поиск элементов и подписка на события;
- изменение текста, классов и доступности элементов из C#;
- горячая перезагрузка интерфейса;
- работа типизированных ссылок на ассеты.

Проект специально оставлен компактным. Вся игровая логика находится в одном C#-файле, а весь интерфейс — в трёх небольших UI-файлах.

## Что получится

Игра для двух игроков за одним устройством:

- игроки по очереди ставят `X` и `O`;
- занятая клетка становится недоступной;
- проверяются строки, столбцы и диагонали;
- победная комбинация подсвечивается;
- определяется ничья;
- кнопка «Новая игра» полностью сбрасывает поле.

## Требования

- Git с поддержкой сабмодулей;
- .NET SDK 10 или новее;
- Windows, Linux или macOS с графическим окружением.

Движок подключён как git-сабмодуль в `Vendors/Vecxy`, поэтому репозиторий нужно клонировать вместе с сабмодулями.

## Быстрый старт

### 1. Клонирование

```bash
git clone --recurse-submodules https://github.com/mlkvs/vecxy.game.tic-tac-toe.git
cd vecxy.game.tic-tac-toe
```

Если проект уже был клонирован без `--recurse-submodules`:

```bash
git submodule update --init --recursive
```

### 2. Сборка

```bash
dotnet build TicTacToe.slnx
```

При сборке Vecxy сканирует папку `Assets`, обновляет типизированные ссылки и проверяет зависимости ассетов.

### 3. Запуск

```bash
dotnet run --project TicTacToe.Game/TicTacToe.Game.csproj
```

Или через CLI-обёртку Vecxy:

```bash
./vecxy.sh build
```

На Windows ту же CLI можно вызвать напрямую:

```powershell
dotnet run --project Vendors/Vecxy/tools/Vecxy.Cli -- build
```

## Структура проекта

```text
.
├── TicTacToe.Game/
│   ├── Assets/
│   │   ├── Configs/Application.yaml   # окно, движок, ассеты и слои
│   │   ├── Fonts/Rubik-SemiBold.ttf   # шрифт интерфейса
│   │   └── UI/
│   │       ├── Board.xml              # главный UI-документ
│   │       ├── BoardSlot.xml          # шаблон одной клетки
│   │       └── BoardStyle.css         # оформление интерфейса
│   ├── Generated/Assets.g.cs          # типизированные ссылки на ассеты
│   ├── Assets.manifest                # стабильные идентификаторы ассетов
│   ├── Program.cs                     # приложение, слой и правила игры
│   └── TicTacToe.Game.csproj
├── Vendors/Vecxy/                     # движок как git-сабмодуль
├── TicTacToe.slnx
└── vecxy.sh                            # локальный запуск Vecxy CLI
```

## Как приложение запускается

Минимальное приложение Vecxy состоит из класса приложения, одного или нескольких слоёв и конфигурации.

### Точка входа

В `Program.cs` приложение объявляется двумя строками:

```csharp
[Vecxy]
public sealed class App : AApp;
```

`[Vecxy]` помечает класс, который движок должен запустить. `AApp` содержит стандартную настройку приложения на основе `Application.yaml`.

Обычный метод `Main` писать не требуется. В файле проекта включена генерация точки входа:

```xml
<VecxyGenerateEntryPoint>true</VecxyGenerateEntryPoint>
```

### Игровой слой

Слой — это модуль приложения с собственным жизненным циклом:

```csharp
public sealed class GameLayer(IUiManager ui) : AAppLayer
{
    [AppLayerDef("game")]
    public sealed class Def : ADefinition<GameLayer>;

    public override void OnInitialize()
    {
        // Инициализация игры.
    }
}
```

Что здесь происходит:

1. `GameLayer` наследуется от `AAppLayer`.
2. Необходимые сервисы движка передаются прямо в конструктор. Здесь нужен только `IUiManager`.
3. Вложенный `Def` регистрирует слой под идентификатором `game`.
4. `OnInitialize()` вызывается движком при запуске слоя.

Слой включается в `Assets/Configs/Application.yaml`:

```yaml
layers:
  - engine
  - game
```

Порядок имеет значение: сначала создаётся системный слой `engine`, затем слой игры `game`.

## Конфигурация Application.yaml

```yaml
application:
  title: Tic Tac Toe

window:
  width: 500
  height: 900

engine:
  targetFrameRate: 60
  showSplashScreen: false

assets:
  hotReload: true
  hotReloadDelayMilliseconds: 150

layers:
  - engine
  - game
```

Основные секции:

| Секция | Назначение |
| --- | --- |
| `application` | Общие данные приложения, например заголовок окна. |
| `window` | Начальные размеры окна. |
| `engine` | Частота кадров и системное поведение движка. |
| `assets` | Настройки ассетов и горячей перезагрузки. |
| `layers` | Слои, которые будут созданы при запуске. |

Для этого примера включена горячая перезагрузка. Изменения XML и CSS можно видеть в запущенной игре без перезапуска процесса.

## Загрузка UI

Главный документ загружается через `IUiManager`:

```csharp
var document = ui.Load(Assets.UI.Board);
```

`Assets.UI.Board` — типизированный handle, сгенерированный из файла `Assets/UI/Board.xml`. Это безопаснее строкового пути: переименование или удаление ассета обнаруживается инструментами Vecxy.

После загрузки к документу привязывается игровая логика:

```csharp
BindDocument(document);
document.Reloaded += BindDocument;
```

Обработчик `Reloaded` важен при разработке. После горячей перезагрузки дерево UI создаётся заново, поэтому ссылки на старые элементы и их обработчики больше не подходят. `BindDocument` повторно находит элементы, создаёт клетки и подписывает события.

## Главный UI-документ

`Board.xml` описывает статическую часть экрана:

```xml
<ui class="screen"
    styles="BoardStyle.css"
    scale-mode="fit"
    reference-width="1080"
    reference-height="1920">
    <panel class="game-card">
        <text class="title">КРЕСТИКИ-НОЛИКИ</text>
        <text id="status" class="status">Ходит X</text>
        <panel id="board" />
        <button id="restart">
            <text>НОВАЯ ИГРА</text>
        </button>
    </panel>
</ui>
```

В этом фрагменте используются четыре базовых элемента:

| XML | C#-тип | Для чего нужен |
| --- | --- | --- |
| `<ui>` | корень `UiDocument` | Настройки документа, масштаб и таблица стилей. |
| `<panel>` | `UiPanel` | Контейнер и компоновка дочерних элементов. |
| `<text>` | `UiText` | Отображение изменяемого текста. |
| `<button>` | `UiButton` | Интерактивный элемент с событиями. |

`scale-mode="fit"` масштабирует интерфейс относительно эталонного разрешения `1080 × 1920`, сохраняя пропорции на окнах другого размера.

### ID и классы

Как и в веб-интерфейсе, ID удобен для уникального элемента, а класс — для общей стилизации:

```csharp
var board = document.GetElementById<UiPanel>("board");
var status = document.GetElementById<UiText>("status");
```

`GetElementById<T>` одновременно проверяет ID и ожидаемый тип. Если разметка не соответствует коду, ошибка возникает сразу при привязке документа.

## Переиспользуемый компонент клетки

Девять одинаковых кнопок не нужно копировать в `Board.xml`. Одна клетка описана в `BoardSlot.xml`:

```xml
<button id="board-slot-{{index}}"
        class="board-slot"
        aria-label="Клетка {{index}}">
    <text></text>
</button>
```

`{{index}}` — параметр шаблона. Компонент создаётся из C# и добавляется в панель поля:

```csharp
var slot = (UiButton)document.Instantiate(
    Assets.UI.BoardSlot,
    board,
    new Dictionary<string, string> { ["index"] = index.ToString() });
```

Метод `Instantiate`:

1. получает handle XML-компонента;
2. создаёт новое дерево элементов;
3. заменяет параметры шаблона;
4. добавляет корневой элемент в указанный `parent`;
5. возвращает созданный элемент.

Такой подход подходит для карточек, строк списка, элементов инвентаря, диалогов и других повторяемых частей UI.

## События интерфейса

Клик по клетке передаёт её индекс в игровую логику:

```csharp
var capturedIndex = index;
slot.Clicked += _ => MakeMove(capturedIndex);
```

Кнопка перезапуска подписывается похожим образом:

```csharp
document.GetElementById<UiButton>("restart").Clicked += _ => ResetGame();
```

Интерфейс не опрашивается каждый кадр. Код реагирует только на событие пользователя, меняет состояние игры и обновляет затронутые элементы.

## Изменение UI из C#

Минимальный набор API, использованный в игре:

| API | Результат |
| --- | --- |
| `UiText.Value = "..."` | Меняет текстовый элемент. |
| `UiButton.Label = "X"` | Меняет текст внутри кнопки. |
| `element.AddClass("winner")` | Добавляет CSS-класс. |
| `element.RemoveClass("winner")` | Удаляет CSS-класс. |
| `element.IsEnabled = false` | Блокирует взаимодействие с элементом. |
| `element.Clicked += handler` | Подписывает обработчик клика. |

Например, после хода кнопка получает знак, цветовой класс и блокируется:

```csharp
_slots[index].Label = _currentPlayer.ToString();
_slots[index].AddClass(_currentPlayer == 'X' ? "cross" : "nought");
_slots[index].IsEnabled = false;
```

Игровая логика сообщает только смысловое состояние: `cross`, `nought`, `winner`. Конкретные цвета и оформление остаются в CSS.

## Стилизация через CSS

Таблица стилей подключается в корневом элементе:

```xml
<ui styles="BoardStyle.css">
```

Шрифт загружается как ассет:

```css
@font-face {
    font-family: "Rubik UI";
    src: url("../Fonts/Rubik-SemiBold.ttf");
}
```

Поле использует grid-компоновку:

```css
#board {
    width: 840px;
    height: 840px;
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 18px;
}
```

Состояния из C# оформляются отдельными селекторами:

```css
.board-slot.cross text {
    color: var(--cross);
}

.board-slot.nought text {
    color: var(--nought);
}

.board-slot.winner {
    background-color: var(--winner);
}
```

Для интерактивной обратной связи доступны псевдоклассы:

```css
.board-slot:hover {
    background-color: var(--cell-hover);
}

.board-slot:active {
    transform: scale(0.965);
}
```

## Состояние и правила игры

Состояние поля хранится отдельно от представления:

```csharp
private readonly char[] _board = new char[9];
private char _currentPlayer = 'X';
private bool _isGameOver;
```

`_board` — модель игры, а `_slots` — ссылки на соответствующие UI-кнопки. При клике сначала проверяется модель:

```csharp
if (_isGameOver || _board[index] != '\0')
    return;
```

После допустимого хода обновляются и модель, и представление. Затем проверяется победа, ничья или переход хода.

Победные линии представлены обычным массивом индексов:

```csharp
private static readonly int[][] WinningLines =
[
    [0, 1, 2], [3, 4, 5], [6, 7, 8],
    [0, 3, 6], [1, 4, 7], [2, 5, 8],
    [0, 4, 8], [2, 4, 6]
];
```

Это не специальный API движка. Vecxy отвечает за приложение, ассеты и интерфейс, а правила игры остаются простым C#-кодом.

## Ассеты и сгенерированные ссылки

Все файлы внутри `TicTacToe.Game/Assets` обрабатываются asset pipeline движка.

Два служебных файла должны храниться в Git:

- `Assets.manifest` сохраняет стабильные GUID и зависимости ассетов;
- `Generated/Assets.g.cs` предоставляет типизированные handles вроде `Assets.UI.Board`.

Не редактируйте их вручную. Для явного обновления можно использовать CLI:

```bash
./vecxy.sh assets scan
./vecxy.sh assets generate
./vecxy.sh assets validate
```

Полная сборка ассетов и проекта:

```bash
./vecxy.sh assets build
```

## Горячая перезагрузка

В `Application.yaml` она включена по умолчанию:

```yaml
assets:
  hotReload: true
  hotReloadDelayMilliseconds: 150
```

Практический цикл работы:

1. Запустите игру через `dotnet run`.
2. Измените `BoardStyle.css` или XML-разметку.
3. Сохраните файл.
4. Движок перечитает ассет и перестроит документ.
5. Обработчик `document.Reloaded` повторно подключит игровую логику.

При изменении C# приложение нужно пересобрать и перезапустить.

## Как экспериментировать дальше

Небольшие упражнения для знакомства с API:

1. Измените цвета `--cross` и `--nought` в `BoardStyle.css`.
2. Добавьте счёт побед X и O через два элемента `<text>`.
3. Сделайте кнопку очистки счёта.
4. Добавьте выбор первого игрока.
5. Реализуйте игру против компьютера, оставив текущий UI без изменений.
6. Вынесите правила из `GameLayer` в отдельный класс и покройте их тестами.

## Частые проблемы

### Сабмодуль Vecxy пустой

```bash
git submodule update --init --recursive
```

### Не найден подходящий .NET SDK

Проверьте установленные версии:

```bash
dotnet --list-sdks
```

Для проекта требуется .NET SDK 10 или новее.

### Изменился или добавился ассет

Обновите manifest и типизированные ссылки:

```bash
./vecxy.sh assets generate
./vecxy.sh assets validate
```

### UI изменился, но обработчики больше не работают

После горячей перезагрузки элементы создаются заново. Подпишитесь на `UiDocument.Reloaded` и повторите поиск элементов и регистрацию событий, как это сделано в `BindDocument`.

## Куда смотреть дальше

- `TicTacToe.Game/Program.cs` — минимальная логика слоя и управление UI;
- `TicTacToe.Game/Assets/UI` — декларативный интерфейс и стили;
- `Vendors/Vecxy/README.md` — обзор возможностей движка;
- `Vendors/Vecxy/tools/README.md` — команды asset pipeline и Vecxy CLI.

Этот проект намеренно показывает только фундамент: **приложение → слой → ассет → UI-документ → событие → обновление состояния**. Освоив этот путь, можно подключать остальные модули Vecxy по мере необходимости, не усложняя первый запуск.

## Варианты реализации игры

В solution находятся три совместимых примера:

| Проект | Правила игры | Управление UI | Назначение |
| --- | --- | --- | --- |
| `TicTacToe.Game` | C# | C# | Исходный минимальный подход без скриптов. |
| `TicTacToe.Scripted` | Luau | Luau через C# capability | Пример игры, логика которой загружается из ассета. |
| `TicTacToe.Hybrid` | C# | Luau | Совмещение типизированной модели и скриптового контроллера. |

Запуск конкретного варианта:

```bash
dotnet run --project TicTacToe.Game
dotnet run --project TicTacToe.Scripted
dotnet run --project TicTacToe.Hybrid
```

Архитектура scripting runtime, ограничения выполнения и границы безопасности описаны в [Docs/Scripting.md](Docs/Scripting.md).
