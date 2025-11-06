local trigger = {}


trigger.name = "MultiheartHelper/AcceleratePlayer"
trigger.nodeLimits = {1, 1}
trigger.placements = {
    name = "default",
    data = {
        acceleration = 8,
        terminalVelocity = 0
    }
}


return trigger