local utils = require("utils")
local drawableNinePatch = require("structs.drawable_nine_patch")

local crumbleBlock = {}

local textures = {
    "default", "cliffside"
}

crumbleBlock.name = "MultiheartHelper/SemipermanentCrumbleBlock"
crumbleBlock.depth = 0
crumbleBlock.placements = {}

for _, texture in ipairs(textures) do
    table.insert(crumbleBlock.placements, {
        name = texture,
        data = {
            width = 8,
            texture = texture
        }
    })
end

crumbleBlock.fieldOrder = {"x", "y", "width", "texture"}

crumbleBlock.fieldInformation = {
    texture = {
        options = textures
    }
}

local ninePatchOptions = {
    mode = "fill",
    fillMode = "repeat",
    border = 0
}

function crumbleBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width = math.max(entity.width or 0, 8)

    local variant = entity.texture or "default"
    local texture = "objects/crumbleBlock/" .. variant
    local ninePatch = drawableNinePatch.fromTexture(texture, ninePatchOptions, x, y, width, 8)

    return ninePatch
end

function crumbleBlock.selection(room, entity)
    return utils.rectangle(entity.x or 0, entity.y or 0, math.max(entity.width or 0, 8), 8)
end

return crumbleBlock