local Rules = {}

local winningLines = {
    { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 },
    { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 },
    { 0, 4, 8 }, { 2, 4, 6 },
}

function Rules.findWinningLine(board, player)
    for _, line in winningLines do
        if board[line[1] + 1] == player
            and board[line[2] + 1] == player
            and board[line[3] + 1] == player then
            return line
        end
    end
    return nil
end

return Rules
