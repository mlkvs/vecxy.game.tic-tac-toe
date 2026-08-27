local Messages = {}

function Messages.turn(player)
    return `Ходит {player}`
end

function Messages.winner(player)
    return `Победил {player}!`
end

Messages.draw = "Ничья!"

return Messages
