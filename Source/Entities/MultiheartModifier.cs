using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.MultiheartHelper.Entities {
    [CustomEntity("MultiheartHelper/MultiheartModifier")]
    [Tracked(true)]
    public class MultiheartModifier: Entity {
        int heartID { get; set; }
        string heartName { get; set; }
        bool endLevel { get; set; }
        string heartTexture { get; set; }

        public MultiheartModifier(EntityData data, Vector2 offset): base(data.Position + offset) {
            heartID = data.Int("heartID");
            heartName = data.Attr("heartName");
            heartTexture = data.Attr("heartTexture");
            endLevel = data.Bool("endLevel");
        }

        public static void Hook() {
            On.Celeste.HeartGem.Collect += HeartGemCollectHook;
            On.Celeste.HeartGem.RegisterAsCollected += HeartGemRegisterCollectHook;
            On.Celeste.HeartGem.Awake += HeartGemAwake;
            IL.Celeste.Poem.ctor += IL_Poem_ctor;
        }

        private static void HeartGemRegisterCollectHook(On.Celeste.HeartGem.orig_RegisterAsCollected orig, HeartGem self, Level level, string poemID)
        {
            orig(self, level, poemID);
            if(TryGetMatchingMultiheart(self, out var _))
                level.Session.HeartGem = false;
        }

        private static void IL_Poem_ctor(ILContext il)
        {
            ILCursor c = new(il);
            
        }

        private static void HeartGemAwake(On.Celeste.HeartGem.orig_Awake orig, HeartGem self, Scene scene)
        {
            if(TryGetMatchingMultiheart(self, out var multiheart)) {
                if(MultiheartHelperModule.Session.collectedHearts.Contains(multiheart.heartName)) {
                    self.RemoveSelf();
                }
            }
            orig(self, scene);
        }

        private static void HeartGemCollectHook(On.Celeste.HeartGem.orig_Collect orig, HeartGem self, Player player)
        {
            orig(self, player);
            if(TryGetMatchingMultiheart(self, out var multiheart)) {
                MultiheartHelperModule.SaveData.RegisterHeartCollect(self.SceneAs<Level>().Session.Area.ID, multiheart.heartName);
                MultiheartHelperModule.Session.collectedHearts.Add(multiheart.heartName);
            }
        }

        private static bool TryGetMatchingMultiheart(HeartGem self, out MultiheartModifier multiheart) {
            foreach(MultiheartModifier modifier in self.Scene.Tracker.GetEntities<MultiheartModifier>()) {
                if(modifier.heartID == self.entityID.ID) {
                    multiheart = modifier;
                    return true;
                }
            }
            multiheart = null;
            return false;
        }
    }
}