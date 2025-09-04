using System.Collections.Generic;
using Monocle;

namespace Celeste.Mod.MultiheartHelper.Data
{
    public class AreaItemMetadata
    {
        public List<ItemInfo> Items { get; set; } = [];
    }

    public class ItemInfo
    {
        public string Name { get; set; }
        public string Texture { get; set; }
        public bool Gameplay { get; set; } = false;
        public string TabName { get; set; } = "Misc";
    }
}