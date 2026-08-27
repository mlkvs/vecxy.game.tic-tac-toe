using Vecxy.Engine;
using Vecxy.Kernel;
using Vecxy.Platforms;
using Vecxy.UI;

namespace TicTacToe.Game;

[Vecxy]
public sealed class App : AApp;

public sealed class GameLayer(IUiManager ui) : AAppLayer
{
    private static readonly int[][] WinningLines =
    [
        [0, 1, 2], [3, 4, 5], [6, 7, 8],
        [0, 3, 6], [1, 4, 7], [2, 5, 8],
        [0, 4, 8], [2, 4, 6]
    ];

    private readonly char[] _board = new char[9];
    private readonly UiButton[] _slots = new UiButton[9];
    private UiText _status = null!;
    private char _currentPlayer = 'X';
    private bool _isGameOver;

    [AppLayerDef("game")]
    public sealed class Def : ADefinition<GameLayer>;

    public override void OnInitialize()
    {
        var document = ui.Load(Assets.UI.Board);
        BindDocument(document);
        document.Reloaded += BindDocument;
    }

    private void BindDocument(UiDocument document)
    {
        var board = document.GetElementById<UiPanel>("board");
        _status = document.GetElementById<UiText>("status");

        for (var index = 0; index < _slots.Length; index++)
        {
            var slot = (UiButton)document.Instantiate(
                Assets.UI.BoardSlot,
                board,
                new Dictionary<string, string> { ["index"] = index.ToString() });
            var capturedIndex = index;
            slot.Clicked += _ => MakeMove(capturedIndex);
            _slots[index] = slot;
        }

        document.GetElementById<UiButton>("restart").Clicked += _ => ResetGame();
        ResetGame();
    }

    private void MakeMove(int index)
    {
        if (_isGameOver || _board[index] != '\0')
            return;

        _board[index] = _currentPlayer;
        _slots[index].Label = _currentPlayer.ToString();
        _slots[index].AddClass(_currentPlayer == 'X' ? "cross" : "nought");
        _slots[index].IsEnabled = false;

        var winningLine = FindWinningLine(_currentPlayer);
        if (winningLine is not null)
        {
            _isGameOver = true;
            _status.Value = $"Победил {_currentPlayer}!";
            foreach (var winningIndex in winningLine)
                _slots[winningIndex].AddClass("winner");
            DisableBoard();
            return;
        }

        if (_board.All(cell => cell != '\0'))
        {
            _isGameOver = true;
            _status.Value = "Ничья!";
            DisableBoard();
            return;
        }

        _currentPlayer = _currentPlayer == 'X' ? 'O' : 'X';
        UpdateTurnStatus();
    }

    private int[]? FindWinningLine(char player) => WinningLines.FirstOrDefault(line =>
        line.All(index => _board[index] == player));

    private void DisableBoard()
    {
        foreach (var slot in _slots)
            slot.IsEnabled = false;
    }

    private void ResetGame()
    {
        Array.Clear(_board);
        _currentPlayer = 'X';
        _isGameOver = false;

        foreach (var slot in _slots)
        {
            slot.Label = string.Empty;
            slot.IsEnabled = true;
            slot.RemoveClass("cross");
            slot.RemoveClass("nought");
            slot.RemoveClass("winner");
        }

        UpdateTurnStatus();
    }

    private void UpdateTurnStatus() => _status.Value = $"Ходит {_currentPlayer}";
}
