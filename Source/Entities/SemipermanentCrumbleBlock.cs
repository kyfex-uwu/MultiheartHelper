using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.MultiheartHelper.Entities {
    [CustomEntity("MultiheartHelper/SemipermanentCrumbleBlock")]
    [Tracked(true)]
    public class SemipermanentCrumbleBlock : CrumblePlatform
    {
        public int ID = 0;
        public bool Used {
            get { return SceneAs<Level>().Session.GetFlag($"semipermanentCrumbleBlock{ID}"); }
            set { SceneAs<Level>().Session.SetFlag($"semipermanentCrumbleBlock{ID}", value); }
        }
        public SemipermanentCrumbleBlock(EntityData data, Vector2 offset) : base(data, offset)
        {
            ID = data.ID;
        }

        public static void Hook() {
            On.Celeste.Player.Die += Hook_Player_Die;
        }

        public static void Unhook() {
            On.Celeste.Player.Die -= Hook_Player_Die;
        }

        private static PlayerDeadBody Hook_Player_Die(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            foreach(string flag in (self.Scene as Level).Session.Flags) {
                if(flag.StartsWith("semipermanentCrumbleBlock")) {
                    self.SceneAs<Level>().Session.SetFlag(flag, false);
                }
            }
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if(Used)
                RemoveSelf();

            // Copied from Maddie's Helping Hand
            DynData<CrumblePlatform> self = new DynData<CrumblePlatform>(this);
            var outlineFader = self.Get<Coroutine>("outlineFader");
            var falls = self.Get<List<Coroutine>>("falls");

            foreach(Component component in this) {
                if (component is Coroutine coroutine && coroutine != outlineFader && !falls.Contains(coroutine)) {
                    // this coroutine is the sequence! hijack it
                    coroutine.RemoveSelf();
                    Add(new Coroutine(modSequence()));
                    break;
                }
            }

            outlineFader.RemoveSelf();
        }

        private IEnumerator modSequence()
        {
            while (true)
            {
                bool onTop;
                if(Used) {
                    yield return null;
                    continue;
                }
                if (GetPlayerOnTop() != null)
                {
                    onTop = true;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }
                else
                {
                    if (GetPlayerClimbing() == null)
                    {
                        yield return null;
                        continue;
                    }
                    onTop = false;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }
                Audio.Play("event:/game/general/platform_disintegrate", base.Center);
                shaker.ShakeFor(onTop ? 0.6f : 1f, removeOnFinish: false);
                foreach (Image image in images)
                {
                    SceneAs<Level>().Particles.Emit(P_Crumble, 2, Position + image.Position + new Vector2(0f, 2f), Vector2.One * 3f);
                }
                for (int i = 0; i < (onTop ? 1 : 3); i++)
                {
                    yield return 0.2f;
                    foreach (Image image2 in images)
                    {
                        SceneAs<Level>().Particles.Emit(P_Crumble, 2, Position + image2.Position + new Vector2(0f, 2f), Vector2.One * 3f);
                    }
                }
                float timer = 0.4f;
                if (onTop)
                {
                    while (timer > 0f && GetPlayerOnTop() != null)
                    {
                        yield return null;
                        timer -= Engine.DeltaTime;
                    }
                }
                else
                {
                    while (timer > 0f)
                    {
                        yield return null;
                        timer -= Engine.DeltaTime;
                    }
                }
                occluder.Visible = false;
                Collidable = false;
                Used = true;
                float num = 0.05f;
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < images.Count; k++)
                    {
                        if (k % 4 - j == 0)
                        {
                            falls[k].Replace(TileOut(images[fallOrder[k]], num * (float)j));
                        }
                    }
                }
            }
        }
    }
}