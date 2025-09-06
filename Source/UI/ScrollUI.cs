using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;



namespace Celeste.Mod.MultiheartHelper.UI {
    public class ScrollUI
    {
        static int scrollUICount = 0;
        public RenderTarget2D viewport;
        public Rectangle bounds;
        float scrollX, scrollY;
        float targetX, targetY;

        public float Right => scrollX + bounds.Width;
        public float Bottom => scrollY + bounds.Height;

        public Vector2 TotalSize { get; set; }
        public bool Clamped { get; set; } = false;

        InventoryUI ui;

        public event Action<float, float> OnScroll;

        public ScrollUI(InventoryUI ui, int x, int y, int width, int height, Action render)
        {
            viewport = VirtualContent.CreateRenderTarget($"scrollUI_{scrollUICount}", width, height);
            bounds = new(x, y, width, height);
            scrollUICount++;
            this.ui = ui;
            this.ui.Add(new BeforeRenderHook(() =>
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(viewport);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Draw.SpriteBatch.Begin();
                render();
                Draw.SpriteBatch.End();
            }));
        }

        public Vector2 Translated(Vector2 vec)
        {
            return vec - new Vector2(scrollX, scrollY);
        }

        public Vector2 Translated(float x, float y) => Translated(new Vector2(x, y));

        public Vector2 TranslatedByTarget(Vector2 vec)
        {
            return vec - new Vector2(targetX, targetY);
        }

        public Vector2 TranslatedByTarget(float x, float y) => TranslatedByTarget(new Vector2(x, y));

        public void Render()
        {
            Draw.SpriteBatch.Draw(viewport, new Vector2(bounds.X, bounds.Y), Color.White);
        }

        public void Scroll(float x, float y, bool smooth = true)
        {
            if (Clamped)
            {
                if (x < 0)
                {
                    x = 0;
                }
                if (y < 0)
                {
                    y = 0;
                }
                if (TotalSize.X > 0 && x + bounds.Width > TotalSize.X)
                {
                    x = TotalSize.X - bounds.Width;
                }
                if (TotalSize.X <= bounds.Width)
                {
                    x = 0;
                }
                if (TotalSize.Y > 0 && y + bounds.Height > TotalSize.Y)
                {
                    y = TotalSize.Y - bounds.Height;
                }
                if (TotalSize.Y <= bounds.Height)
                {
                    y = 0;
                }
            }

            targetX = x;
            targetY = y;
            if (!smooth)
            {
                scrollX = x;
                scrollY = y;
                OnScroll?.Invoke(scrollX, scrollY);
            }
            ui.Add(new Coroutine(ScrollRoutine(), true));
        }

        public void ScrollToX(float x, bool smooth = true) => Scroll(x, targetY, smooth);
        public void ScrollToY(float y, bool smooth = true) => Scroll(targetX, y, smooth);
        public void ScrollByX(float x, bool smooth = true) => Scroll(targetX + x, targetY, smooth);
        public void ScrollByY(float y, bool smooth = true) => Scroll(targetX, targetY + y, smooth);
        public void ScrollToBottom(bool smooth = true) => Scroll(targetX, float.MaxValue, smooth);

        public IEnumerator ScrollRoutine()
        {
            while (new Vector2(scrollX - targetX, scrollY - targetY).LengthSquared() > 0.01f)
            {
                scrollX = float.Lerp(scrollX, targetX, 0.7f);
                scrollY = float.Lerp(scrollY, targetY, 0.7f);
                OnScroll?.Invoke(scrollX, scrollY);
                yield return null;
            }

            scrollX = targetX;
            scrollY = targetY;
        }
    }
}