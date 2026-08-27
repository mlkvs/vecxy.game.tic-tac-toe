using Vecxy.Assets;
using Vecxy.Engine;
using Vecxy.Kernel;
using Vecxy.Platforms;
using Vecxy.Scripting;
using Vecxy.UI;

namespace TicTacToe.Scripted;

[Vecxy]
public sealed class App : AApp;

public sealed class ScriptedGameLayer(IUiManager ui, IScriptRuntime scripts) : AAppLayer
{
    private UiDocument? _document;
    private IScriptInstance? _controller;

    [AppLayerDef("scripted-game")]
    public sealed class Def : ADefinition<ScriptedGameLayer>;

    public override void OnInitialize()
    {
        _document = ui.Load(Assets.UI.Board);
        var view = new ScriptedBoardView(_document, Assets.UI.BoardSlot);
        var context = new ScriptContext().Add("ui", view);
        _controller = scripts.Create(Assets.Scripts.TicTacToe, context);
        view.Attach(_controller);
        _controller.Invoke("initialize");

        _document.Reloaded += document =>
        {
            view.Rebind(document);
            _controller.Invoke("initialize");
        };
    }

    public override void OnUnload()
    {
        _controller?.Dispose();
        _controller = null;
        if (_document is not null)
            ui.Unload(_document);
        _document = null;
    }
}

public sealed class ScriptedBoardView(UiDocument document, IAssetHandle slotAsset)
{
    private readonly UiButton[] _slots = new UiButton[9];
    private UiDocument _document = document;
    private UiText _status = null!;
    private IScriptInstance _controller = null!;

    public void Attach(IScriptInstance controller) => _controller = controller;

    public void Rebind(UiDocument document) => _document = document;

    public void BuildBoard()
    {
        var board = _document.GetElementById<UiPanel>("board");
        _status = _document.GetElementById<UiText>("status");
        for (var index = 0; index < _slots.Length; index++)
        {
            var slot = (UiButton)_document.Instantiate(
                slotAsset,
                board,
                new Dictionary<string, string> { ["index"] = index.ToString() });
            var capturedIndex = index;
            slot.Clicked += _ => _controller.Invoke("makeMove", capturedIndex);
            _slots[index] = slot;
        }
        _document.GetElementById<UiButton>("restart").Clicked +=
            _ => _controller.Invoke("restart");
    }

    public void SetStatus(string value) => _status.Value = value;

    public void SetCell(int index, string value)
    {
        var slot = _slots[index];
        slot.Label = value;
        slot.AddClass(value == "X" ? "cross" : "nought");
        slot.IsEnabled = false;
    }

    public void MarkWinner(object indices)
    {
        foreach (var index in ConvertIndices(indices))
            _slots[index].AddClass("winner");
    }

    public void DisableBoard()
    {
        foreach (var slot in _slots)
            slot.IsEnabled = false;
    }

    public void ResetBoard()
    {
        foreach (var slot in _slots)
        {
            slot.Label = string.Empty;
            slot.IsEnabled = true;
            slot.RemoveClass("cross");
            slot.RemoveClass("nought");
            slot.RemoveClass("winner");
        }
    }

    private static IEnumerable<int> ConvertIndices(object value)
    {
        if (value is object[] values)
            return values.Select(Convert.ToInt32);
        if (value is IEnumerable<object> items)
            return items.Select(Convert.ToInt32);
        throw new ArgumentException("Expected an array of board indices.", nameof(value));
    }
}
