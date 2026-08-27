local Messages = require("./Messages")
local Controller = {}

function Controller.initialize()
    ui.BuildBoard()
    Controller.restart()
end

function Controller.makeMove(index)
    local result = game.TryMove(index)
    if not result.Accepted then return end
    ui.SetCell(index, result.Player)

    if result.HasWinner then
        ui.SetStatus(Messages.winner(result.Player))
        ui.MarkWinner(result.WinningLine)
        ui.DisableBoard()
        return
    end
    if result.IsDraw then
        ui.SetStatus(Messages.draw)
        ui.DisableBoard()
        return
    end
    ui.SetStatus(Messages.turn(result.NextPlayer))
end

function Controller.restart()
    game.Reset()
    ui.ResetBoard()
    ui.SetStatus(Messages.turn(game.CurrentPlayer))
end

function Controller.onReload()
    Controller.restart()
end

return Controller
