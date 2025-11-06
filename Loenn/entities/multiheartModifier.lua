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
            options = {
                "",
                "0",
                "1",
                "2"
            },
            editable = false
        },
        heartTexture = {
            options = {
                "heartgem0",
                "heartgem1",
                "heartgem2"
            },
            editable = true
        },
        poemHeartSprite = {
            options = {
                "collectables/heartgem/0",
                "collectables/heartgem/1",
                "collectables/heartgem/2",
                "collectables/heartgem/3",
            },
            editable = true
        },
    }
}

return multiheartModifier;
