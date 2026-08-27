using Vecxy.Assets;
using Vecxy.Engine;
using Vecxy.Kernel;
using Vecxy.Platforms;
using Vecxy.Scripting;
using Vecxy.UI;

namespace TicTacToe.Hybrid;

[Vecxy]
public sealed class App : AApp;

public sealed class HybridGameLayer(IUiManager ui, IScriptRuntime scripts) : AAppLayer
{
    private UiDocument? _document;
    private IScriptInstance? _controller;

    [AppLayerDef("hybrid-game")]
    public sealed class Def : ADefinition<HybridGameLayer>;

    public override void OnInitialize()
    {
        _document = ui.Load(Assets.UI.Board);
        var view = new HybridBoardView(_document, Assets.UI.BoardSlot);
        var game = new TicTacToeModel();
        var context = new ScriptContext()
            .Add("ui", view)
            .Add("game", game);

        _controller = scripts.Create(Assets.Scripts.BoardController, context);
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

public sealed class TicTacToeModel
{
    private static readonly int[][] WinningLines =
    [
        [0, 1, 2], [3, 4, 5], [6, 7, 8],
        [0, 3, 6], [1, 4, 7], [2, 5, 8],
        [0, 4, 8], [2, 4, 6]
    ];

    private readonly char[] _board = new char[9];
    private bool _isGameOver;

    public string CurrentPlayer { get; private set; } = "X";

    public MoveResult TryMove(int index)
    {
        if (index is < 0 or >= 9 || _isGameOver || _board[index] != '\0')
            return MoveResult.Rejected(CurrentPlayer);

        var player = CurrentPlayer[0];
        _board[index] = player;
        var winningLine = WinningLines.FirstOrDefault(line =>
            line.All(cell => _board[cell] == player));
        var isDraw = winningLine is null && _board.All(cell => cell != '\0');

        if (winningLine is not null || isDraw)
            _isGameOver = true;
        else
            CurrentPlayer = player == 'X' ? "O" : "X";

        return new MoveResult(
            true,
            player.ToString(),
            CurrentPlayer,
            winningLine ?? [],
            isDraw);
    }

    public void Reset()
    {
        Array.Clear(_board);
        CurrentPlayer = "X";
        _isGameOver = false;
    }
}

public sealed record MoveResult(
    bool Accepted,
    string Player,
    string NextPlayer,
    int[] WinningLine,
    bool IsDraw)
{
    public bool HasWinner => WinningLine.Length > 0;

    public static MoveResult Rejected(string currentPlayer) =>
        new(false, string.Empty, currentPlayer, [], false);
}

public sealed class HybridBoardView(UiDocument document, IAssetHandle slotAsset)
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

    public void MarkWinner(int[] indices)
    {
        foreach (var index in indices)
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
}
