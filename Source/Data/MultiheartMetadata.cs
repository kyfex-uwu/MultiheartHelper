using System.Collections.Generic;

namespace Celeste.Mod.MultiheartHelper.Data {
    public class MultiheartMetadata {
        public int MaxHearts { get; set; } = 1;
        public float Spacing { get; set; } = -32;
        public List<HeartInfo> Hearts { get; set; } = [];
    }
}