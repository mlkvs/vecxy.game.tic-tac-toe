local Rules = require("./Rules")
local Controller = {}

local board = {}
local currentPlayer = "X"
local gameOver = false

function Controller.initialize()
    ui.BuildBoard()
    Controller.restart()
end

function Controller.makeMove(index)
    if gameOver or board[index + 1] ~= "" then return end
    board[index + 1] = currentPlayer
    ui.SetCell(index, currentPlayer)

    local winningLine = Rules.findWinningLine(board, currentPlayer)
    if winningLine ~= nil then
        gameOver = true
        ui.SetStatus(`Победил {currentPlayer}!`)
        ui.MarkWinner(winningLine)
        ui.DisableBoard()
        return
    end

    for _, cell in board do
        if cell == "" then
            currentPlayer = if currentPlayer == "X" then "O" else "X"
            ui.SetStatus(`Ходит {currentPlayer}`)
            return
        end
    end
    gameOver = true
    ui.SetStatus("Ничья!")
    ui.DisableBoard()
end

function Controller.restart()
    board = table.create(9, "")
    currentPlayer = "X"
    gameOver = false
    ui.ResetBoard()
    ui.SetStatus("Ходит X")
end

function Controller.onReload()
    Controller.restart()
end

return Controller
