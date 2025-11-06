local e = {
    name = "MultiheartHelper/ItemCollectible",
    placements = {
        name = "default",
        data = {
            itemName = "",
            texture = "",
            gameplay = true
        }
    }
}

function e.texture(room, entity)
    return (entity.texture or "") .. "idle00"
end

return e