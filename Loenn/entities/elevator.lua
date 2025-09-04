local utils = require("utils")


local e = {
    name = "MultiheartHelper/Elevator",
    placements = {
        name = "default",
        data = {
            textureFolder = "",
            elevatorID = "",
            targetRoom = "",
            targetID = "",
            down = true,
            flag = ""
        }
    }
}

function e.rectangle(room, entity)
    return utils.rectangle(entity.x or 0, entity.y or 0, 4*8, 6*8)
end

return e