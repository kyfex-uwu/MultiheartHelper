local lineMirror = {
    name = "MultiheartHelper/LineMirror",
    depth = 0,
    fillColor = "#000000",
    borderColor = "#ffffff",
    placements = {
        name = "lineMirror",
        data = {
            width = 8,
            height = 8,
            up = true,
            down = true,
            left = true,
            right = true,
            flag = "",
            depth = 0
        }
    },
    fieldInformation = {
        depth = {
            fieldType = "integer"
        }
    }
}

function lineMirror:depth(room, entity, viewport)
    return entity.depth or 0
end

return lineMirror