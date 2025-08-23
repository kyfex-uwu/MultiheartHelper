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
        string flag { get; set; }
        string heartIndex { get; set; }
        string heartSprite { get; set; }
        Color poemColor { get; set; }

        static MultiheartModifier lastCollected;

        public MultiheartModifier(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            heartID = data.Int("heartID");
            heartName = data.Attr("heartName");
            heartTexture = data.Attr("heartTexture");
            endLevel = data.Bool("endLevel");
            flag = data.Attr("setFlagOnCollect");
            heartIndex = data.String("heartIndex");
            heartSprite = data.String("poemHeartSprite");
            if (heartSprite != null && !heartSprite.EndsWith("/"))
                heartSprite += "/";
            poemColor = data.HexColor("poemColor");
        }

        public static void Hook() {
            On.Celeste.HeartGem.Collect += HeartGemCollectHook;
            On.Celeste.HeartGem.RegisterAsCollected += HeartGemRegisterCollectHook;
            On.Celeste.HeartGem.Awake += HeartGemAwake;
            On.Celeste.Poem.ctor += PoemCtor;
        }

        private static void PoemCtor(On.Celeste.Poem.orig_ctor orig, Poem self, string text, int heartIndex, float heartAlpha)
        {
            if (lastCollected == null)
            {
                orig(self, text, heartIndex, heartAlpha);
                return;
            }
            Level level = lastCollected.Scene as Level;
            AreaKey area = level.Session.Area;
            string poemID = AreaData.Get(level).Mode[(int)area.Mode].PoemID;
            orig(self, Dialog.Clean("poem_" + poemID + "_" + lastCollected.heartName), lastCollected.heartIndex == null ? heartIndex : int.Parse(lastCollected.heartIndex), heartAlpha);
            if (lastCollected.heartSprite != null)
            {
                self.Heart = new Sprite(GFX.Gui, lastCollected.heartSprite);
                self.Heart.CenterOrigin();
                self.Heart.Justify = new Vector2(0.5f, 0.5f);
                self.Heart.AddLoop("spin", "spin", 0.08f);
                self.Heart.Play("spin");
                self.Heart.Position = new Vector2(1920, 1080) * 0.5f;
                self.Heart.Color = Color.White * heartAlpha;
            }
            if (lastCollected.poemColor != default)
                self.Color = lastCollected.poemColor;
            lastCollected = null;
        }

        private static void HeartGemRegisterCollectHook(On.Celeste.HeartGem.orig_RegisterAsCollected orig, HeartGem self, Level level, string poemID)
        {
            orig(self, level, poemID);
            if(TryGetMatchingMultiheart(self, out var _))
                level.Session.HeartGem = false;
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
            if (TryGetMatchingMultiheart(self, out var multiheart))
            {
                MultiheartHelperModule.SaveData.RegisterHeartCollect(self.SceneAs<Level>().Session.Area.ID, multiheart.heartName);
                MultiheartHelperModule.Session.collectedHearts.Add(multiheart.heartName);
                lastCollected = multiheart;
                if (multiheart.flag != "")
                {
                    self.SceneAs<Level>().Session.SetFlag(multiheart.flag, true);
                }
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