using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.MultiheartHelper
{
    [CustomEntity("MultiheartHelper/LineMirror")]
    public class LineMirror : Entity
    {
        public bool left, right, up, down;
        public string flag;
        public int w, h;
        int ID = 0;
        static float playerYScale = 1;
        RenderTarget2D mirrorVertical, mirrorHorizontal;
        Entity reflection;
        PlayerSprite reflectionSprite;
        PlayerHair reflectionHair;
        public LineMirror(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(new BeforeRenderHook(BeforeRender));
            w = data.Width;
            h = data.Height;
            ID = data.ID;
            left = data.Bool("vertical");
            right = data.Bool("horizontal");
            up = data.Bool("up");
            down = data.Bool("down");
            left = data.Bool("left");
            right = data.Bool("right");
            flag = data.Attr("flag");
            Depth = data.Int("depth");
            Visible = true;
        }

        public static void Hook()
        {
            IL.Celeste.Player.Render += Hook_Render;
            // On.Celeste.PlayerHair.Render += CustomHairRender;
        }

        public static void Unhook()
        {
            IL.Celeste.Player.Render -= Hook_Render;
            // On.Celeste.PlayerHair.Render -= CustomHairRender;
        }

        private static void CustomHairRender(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self)
        {
            Player player = self.Scene?.Tracker?.GetEntity<Player>();
            if (self.Entity.GetType() != typeof(Entity) || player == null)
            {
                orig(self);
                return;
            }

            Entity original = self.Entity;
            self.Entity = player;
            orig(self);
            self.Entity = original;
        }

        private static void Hook_Render(ILContext il)
        {
            ILCursor c = new(il);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Player player) =>
            {
                playerYScale = player.Sprite.Scale.Y;
            });
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            mirrorVertical = VirtualContent.CreateRenderTarget($"line_mirror_{ID}_v", w, h);
            mirrorHorizontal = VirtualContent.CreateRenderTarget($"line_mirror_{ID}_h", w, h);
            reflectionSprite = new PlayerSprite(PlayerSpriteMode.Badeline);
            reflection = [reflectionHair = new PlayerHair(reflectionSprite), reflectionSprite];
            reflectionHair.Border = Color.Black;
            reflectionHair.Color = BadelineOldsite.HairColor;
            reflectionHair.Start();
        }

        public override void Update()
        {
            base.Update();
            reflection.Update();
            reflectionHair.Facing = (Facings)Math.Sign(reflectionSprite.Scale.X);
            reflectionHair.AfterUpdate();
        }

        private void BeforeRender()
        {
            if (mirrorVertical == null || mirrorHorizontal == null)
                return;



            Player player = Scene?.Tracker?.GetEntity<Player>();
            if (player == null)
                return;

            reflectionSprite.Scale.X = (float)player.Facing * Math.Abs(player.Sprite.Scale.X);
            reflectionSprite.Scale.Y = player.Sprite.Scale.Y;
            if (reflectionSprite.CurrentAnimationID != player.Sprite.CurrentAnimationID && player.Sprite.CurrentAnimationID != null && reflectionSprite.Has(player.Sprite.CurrentAnimationID))
            {
                reflectionSprite.Play(player.Sprite.CurrentAnimationID);
            }
            if (up || down)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
                Engine.Graphics.GraphicsDevice.SetRenderTarget(mirrorVertical);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                if(up)
                    DrawVertical(player, true);
                if(down)
                    DrawVertical(player, false);
                Draw.SpriteBatch.End();
            }
            if (left || right)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
                Engine.Graphics.GraphicsDevice.SetRenderTarget(mirrorHorizontal);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                if(left)
                    DrawHorizontal(player, true);
                if(right)
                    DrawHorizontal(player, false);
                Draw.SpriteBatch.End();
            }
        }

        private void DrawVertical(Player player, bool belowPlayer)
        {
            Vector2 pos = reflection.Position;
            reflection.Position = player.Position + new Vector2(0, h * (belowPlayer? 1: -1)) - Position;

            if (playerYScale < 0)
            {
                // grelper
                float reflectedY = Height - reflection.Position.Y;
                reflectedY -= reflection.Height;
                reflection.Y = reflectedY;
            }

            Vector2 vector = reflection.Position - pos;
            for (int i = 0; i < reflectionHair.Nodes.Count; i++)
            {
                reflectionHair.Nodes[i] += vector;
            }
            reflection.Render();
        }

        private void DrawHorizontal(Player player, bool rightOfPlayer)
        {
            Vector2 pos = reflection.Position;
            reflection.Position = player.Position + new Vector2(w * (rightOfPlayer? 1: -1), 0) - Position;
            Vector2 vector = reflection.Position - pos;
            for (int i = 0; i < reflectionHair.Nodes.Count; i++)
            {
                reflectionHair.Nodes[i] += vector;
            }
            reflection.Render();
        }

        public override void Render()
        {
            if (flag != "" && !SceneAs<Level>().Session.GetFlag(flag))
                return;
            if (mirrorVertical != null)
            {
                Draw.SpriteBatch.Draw(mirrorVertical, Position, null, Color.White, 0, Vector2.Zero, 1, playerYScale >= 0? SpriteEffects.FlipVertically: SpriteEffects.None, 0);
            }
            if (mirrorHorizontal != null)
            {
                Draw.SpriteBatch.Draw(mirrorHorizontal, Position, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally | (playerYScale < 0? SpriteEffects.FlipVertically: SpriteEffects.None), 0);
            }
        }
    }
}