local dustPolygon = {}

dustPolygon.name = "MultiheartHelper/DustPolygon"
dustPolygon.depth = 0
dustPolygon.nodeLimits = {2, -1}
dustPolygon.nodeLineRenderType = "line"
dustPolygon.nodeVisibility = "always"
dustPolygon.placements = {
    name = "dust_polygon",
    data = {
        dustCount = 30,
        spreadTime = 0.05,
        maxDustAmount = 600,
        spreadCount = 3,
        smoothing = 20,
        flag = ""
    }
}

return dustPolygon