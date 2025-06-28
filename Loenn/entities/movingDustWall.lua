local dustSpreader = {}

dustSpreader.name = "MultiheartHelper/MovingDustWall"
dustSpreader.depth = 0
dustSpreader.texture = "@Internal@/rising_lava"
dustSpreader.nodeLimits = {1, 1}
dustSpreader.nodeLineRenderType = "line"
dustSpreader.placements = {
    name = "moving_dust_wall",
    data = {
        dustCount = 30,
        spreadTime = 0.05,
        maxDustAmount = 600,
        spreadCount = 3,
        smoothing = 500,
        flag = ""
    }
}

return dustSpreader