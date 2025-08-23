local multiheartModifier = {
    name = "MultiheartHelper/MultiheartModifier",
    depth = 0,
    texture = "collectables/heartgem/3/00",
    placements = {
        name = "multiheartModifier",
        data = {
            heartID = 0,
            heartName = "",
            endLevel = false,
            heartTexture = "",
            setFlagOnCollect = "",
            poemHeartSprite = "",
            poemColor = "",
            heartIndex = ""
        }
    },
    fieldInformation = {
        poemColor = {
            fieldType = "color",
            allowEmpty = true
        },
        heartID = {
            fieldType = "integer"
        },
        heartIndex = {
            fieldType = "list",
            options = {
                "",
                "0",
                "1",
                "2"
            },
            editable = false
        }
    }
}

return multiheartModifier;