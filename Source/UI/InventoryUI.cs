using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.MultiheartHelper.UI
{
    [Tracked]
    public class InventoryUI : Entity
    {
        public const int MARGIN = 100;
        public const int BORDER = 5;

        Texture2D leftArrow, rightArrow;
        Wiggler leftWiggler, rightWiggler;
        float leftScale = 1, rightScale = 1;
        List<InventoryTab> tabs = [];
        int currentIndex = 0;

        static InventoryUI()
        {
            new ItemsTab(null);
            new EquipmentTab(null);
            new LogsTab(null);
            new TerminalTab(null);
        }

        Rectangle selectionRect = new Rectangle();
        Rectangle targetRect = new Rectangle();
        private Coroutine moveCoroutine;

        RenderTarget2D screen;

        static int inventoryUICount = 0;

        public Rectangle TargetRect
        {
            set
            {
                targetRect = value;
                if (moveCoroutine != null)
                    moveCoroutine.RemoveSelf();
                Add(moveCoroutine = new Coroutine(MoveSelectionRoutine(), true));
            }
            get
            {
                return targetRect;
            }
        }
        public float selectionWobble = 0;
        public InventoryTab currentTab => tabs[currentIndex];
        public bool AcceptInput { get; set; } = true;

        public float OpenPercentage { get; set; } = 1;


        bool tabFocused = true;
        public bool TabFocused
        {
            get
            {
                return tabFocused;
            }
            set
            {
                if (value)
                    currentTab?.Focus();
                tabFocused = value;
            }
        }

        public Color PrimaryColor;
        public InventoryUI(Color primaryColor, params string[] tabs)
        {
            foreach (string tab in tabs)
            {
                this.tabs.Add(InventoryTab.Create(this, tab));
                if (this.tabs[^1] == null)
                {
                    this.tabs.RemoveAt(this.tabs.Count - 1);
                    continue;
                }
            }

            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 0.4f, true);
            tween.OnUpdate = f =>
            {
                OpenPercentage = f.Eased;
            };
            Add(tween);

            Tag = Tags.PauseUpdate | Tags.HUD;
            Depth = Depths.FGTerrain - 2;
            PrimaryColor = primaryColor;
            leftArrow = GFX.Gui.textures["controls/directions/-1x0"].GetSubtextureCopy();
            rightArrow = GFX.Gui.textures["controls/directions/1x0"].GetSubtextureCopy();
            screen = VirtualContent.CreateRenderTarget($"inventoryUI_{inventoryUICount}", Engine.Width, Engine.Height);
            inventoryUICount++;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            foreach (InventoryTab tab in tabs)
            {
                tab.Setup();
            }
            currentTab?.Focus();
            Add(new BeforeRenderHook(RenderScreen));
            SetLocked(true);
            Add(leftWiggler = Wiggler.Create(0.2f, 4f, v =>
            {
                leftScale = 1 + 0.2f * v;
            }));
            Add(rightWiggler = Wiggler.Create(0.2f, 4f, v =>
            {
                rightScale = 1 + 0.2f * v;
            }));
        }

        public void RenderScreen()
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(screen);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin();
            RenderBackground();
            RenderForeground();
            Draw.SpriteBatch.End();
        }

        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Draw(screen, new Vector2(0, (1-OpenPercentage) * Engine.Height/2), new Rectangle(0, (int)((1-OpenPercentage) * Engine.Height/2), Engine.Width, (int)(Engine.Height * OpenPercentage)), Color.White);
        }

        void RenderBackground()
        {
            Draw.Rect(0, 0, Engine.Width, Engine.Height, Color.Black * 0.5f);
        }

        void RenderForeground()
        {
            if (currentTab != null)
                ActiveFont.DrawEdgeOutline(Dialog.Clean($"{(Scene as Level)?.Session?.Area.SID}_tab_{currentTab?.Name}"), new Vector2(Engine.Width / 2, MARGIN / 2), new Vector2(0.5f, 0.5f), Vector2.One * 1.2f, PrimaryColor, 4f, Color.Black);
            DrawSpriteCentered(Draw.SpriteBatch, leftArrow, new Rectangle(MARGIN, 0, MARGIN, MARGIN), PrimaryColor, leftScale);
            DrawSpriteCentered(Draw.SpriteBatch, rightArrow, new Rectangle(Engine.Width - 2 * MARGIN, 0, MARGIN, MARGIN), PrimaryColor, rightScale);
            Draw.HollowRect(MARGIN, MARGIN, Engine.Width - MARGIN * 2, Engine.Height - MARGIN * 2, PrimaryColor);
            currentTab?.Render();
            Draw.HollowRect(ExpandedRect(selectionRect, selectionWobble), PrimaryColor);
        }

        public override void Update()
        {
            base.Update();

            if (!AcceptInput)
                return;
            bool? cancelDefaultDirection = TabFocused;
            int x = 0, y = 0;
            if (Input.MenuUp.Pressed)
            {
                y += -1;
            }
            if (Input.MenuDown.Pressed)
            {
                y += 1;
            }
            if (Input.MenuLeft.Pressed)
            {
                x += -1;
            }
            if (Input.MenuRight.Pressed)
            {
                x += 1;
            }

            if ((x != 0 || y != 0) && TabFocused)
            {
                cancelDefaultDirection = currentTab?.RegisterDirection(x, y);
            }

            if (Input.ESC.Pressed)
            {
                Input.ESC.ConsumePress();
                Close();
            }

            if (cancelDefaultDirection == false)
            {
                HandleDefaultDirectionInput(x, y);
            }
        }

        public void Close()
        {
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 0.4f, true);
            tween.OnUpdate = f =>
            {
                OpenPercentage = 1-f.Eased;
            };
            tween.OnComplete = f =>
            {
                RemoveSelf();
            };
            Add(tween);
        }

        private void HandleDefaultDirectionInput(int x, int y)
        {
            if (y == -1 && TabFocused)
            {
                TabFocused = false;
                TargetRect = new(MARGIN, 0, Engine.Width - MARGIN * 2, MARGIN);
            }
            if (y == 1 && !TabFocused)
            {
                TabFocused = true;
            }
            if (x != 0 && !TabFocused)
            {
                currentIndex += x;
                if (currentIndex < 0)
                {
                    currentIndex += tabs.Count;
                }
                currentIndex %= tabs.Count;
                (x == 1 ? rightWiggler : leftWiggler).Start();
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            SetLocked(false);
        }

        public static void DrawTextWrapped(string text, Vector2 position, float width, float scale, Color color)
        {
            text = text.Replace("\\n", "\n");
            List<string> lines = [];
            float currentWidth = 0;
            string currentLine = "";
            foreach (string w in text.Split(' '))
            {
                string word = w + " ";
                currentWidth += ActiveFont.Measure(word).X * scale;
                if (currentWidth > width)
                {
                    lines.Add(currentLine);
                    currentLine = "";
                    currentWidth = ActiveFont.Measure(word).X * scale;
                }
                currentLine += word;
            }
            lines.Add(currentLine);
            ActiveFont.Draw(string.Join('\n', lines), position, Vector2.Zero, Vector2.One * scale, color);
        }

        public static void DrawSpriteInRect(SpriteBatch spriteBatch, Texture2D texture, Rectangle rectangle, Color color)
        {
            float textureAspect = (float)texture.Width / texture.Height;
            float rectAspect = (float)rectangle.Width / rectangle.Height;

            float scale;
            if (textureAspect > rectAspect)
            {
                scale = (float)rectangle.Width / texture.Width;
            }
            else
            {
                scale = (float)rectangle.Height / texture.Height;
            }

            int drawWidth = (int)(texture.Width * scale);
            int drawHeight = (int)(texture.Height * scale);

            int drawX = rectangle.X + (rectangle.Width - drawWidth) / 2;
            int drawY = rectangle.Y + (rectangle.Height - drawHeight) / 2;
            spriteBatch.Draw(texture, new Rectangle(drawX, drawY, drawWidth, drawHeight), color);
        }

        public static void DrawSpriteCentered(SpriteBatch spriteBatch, Texture2D texture, Rectangle rectangle, Color color, float scale = 1)
        {
            int textureWidth = (int)(texture.Width * scale);
            int textureHeight = (int)(texture.Height * scale);
            Rectangle dest = new(rectangle.X + rectangle.Width / 2 - textureWidth / 2, rectangle.Y + rectangle.Height / 2 - textureHeight / 2, textureWidth, textureHeight);
            DrawSpriteInRect(spriteBatch, texture, dest, color);
        }

        // Copied from collabutils2 LobbyMapUI: https://github.com/EverestAPI/CelesteCollabUtils2/blob/master/UI/LobbyMapUI.cs#L1146
        public static void SetLocked(bool locked)
        {
            Level level = Engine.Scene as Level;
            Player player = level?.Tracker.GetEntity<Player>();
            if (level == null || player == null)
                return;

            level.CanRetry = !locked;
            level.PauseLock = locked;
            player.Speed = Vector2.Zero;
            player.DummyGravity = !locked;
            player.StateMachine.State = locked ? Player.StDummy : Player.StNormal;

            player.DummyAutoAnimate = !locked || player.OnGround();
        }

        private IEnumerator MoveSelectionRoutine()
        {
            while (RectDistance(selectionRect, TargetRect) >= float.Epsilon)
            {
                selectionRect = RectLerp(selectionRect, targetRect, 0.7f);
                yield return null;
            }
            selectionRect = targetRect;
        }

        private static float RectDistance(Rectangle a, Rectangle b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y).LengthSquared() + new Vector2(b.Width - a.Width, b.Height - a.Height).LengthSquared();
        }

        private static Rectangle RectLerp(Rectangle a, Rectangle b, float value)
        {
            Rectangle c = new();
            Vector2 pos = new Vector2(a.X, a.Y);
            pos = Vector2.Lerp(pos, new Vector2(b.X, b.Y), value);
            c.X = (int)pos.X;
            c.Y = (int)pos.Y;

            Vector2 size = new(a.Width, a.Height);
            size = Vector2.Lerp(size, new(b.Width, b.Height), value);
            c.Width = (int)size.X;
            c.Height = (int)size.Y;
            return c;
        }

        private static Rectangle ExpandedRect(Rectangle a, float wobble)
        {
            return new(a.X - (int)wobble, a.Y - (int)wobble, a.Width + (int)wobble * 2, a.Height + (int)wobble * 2);
        }
    }
}