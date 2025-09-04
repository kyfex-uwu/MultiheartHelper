using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.MultiheartHelper.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.MultiheartHelper.UI
{
    public class ItemsTab : InventoryTab
    {
        List<ItemInfo> collected = [];
        List<Texture2D> textures = [];
        public const int COLUMNS = 6;
        public const int ITEM_PADDING = 20;
        public const int SELECTION_PADDING = 10;
        public static int ItemAreaWidth => Engine.Width / 2 - InventoryUI.MARGIN;
        public static int ItemSize = ItemAreaWidth / COLUMNS;
        public int selectedX, selectedY;
        public Vector2 selectionOffset;
        Wiggler selectionWiggler;
        public override string Name => "Misc";

        public ItemsTab(InventoryUI ui) : base(ui)
        {
        }

        public override void Render()
        {
            RenderItemArea();
            RenderTextArea();
        }

        private void RenderItemArea()
        {
            Draw.HollowRect(Engine.Width / 2, InventoryUI.MARGIN, ItemAreaWidth, Engine.Height - InventoryUI.MARGIN * 2, ui.PrimaryColor);
            int i = 0;
            foreach (ItemInfo item in collected)
            {
                int x = i % COLUMNS;
                int y = i / COLUMNS;

                Texture2D texture = textures[i];
                if (texture == null)
                    continue;
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);
                InventoryUI.DrawSpriteInRect(Draw.SpriteBatch, texture, new Rectangle(InventoryUI.MARGIN + x * ItemSize + ITEM_PADDING, InventoryUI.MARGIN + y * ItemSize + ITEM_PADDING, ItemSize - 2 * ITEM_PADDING, ItemSize - 2 * ITEM_PADDING), Color.White);
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin();
                i++;
            }
        }

        private void RenderTextArea()
        {
            float height = ActiveFont.HeightOf("ABCDEFGHIJKLMNOPQRSTUVWXYZ") * 1.2f;
            height += ITEM_PADDING;
            Draw.HollowRect(Engine.Width / 2, InventoryUI.MARGIN, Engine.Width / 2 - InventoryUI.MARGIN, height, ui.PrimaryColor);
            if (!ui.TabFocused)
                return;
            int selected = selectedY * COLUMNS + selectedX;
            if (selected >= collected.Count || selected < 0)
                return;
            ItemInfo selectedItem = collected[selected];
            Vector2 Origin = new Vector2(Engine.Width / 2, InventoryUI.MARGIN);
            ActiveFont.DrawEdgeOutline(Dialog.Clean($"{(ui.Scene as Level).Session.Area.SID}_item_{selectedItem.Name}"), Origin + new Vector2(ITEM_PADDING), Vector2.Zero, Vector2.One * 1.2f, ui.PrimaryColor, 4f, Color.Black);
            InventoryUI.DrawTextWrapped(Dialog.Clean($"{(ui.Scene as Level).Session.Area.SID}_item_{selectedItem.Name}_desc"), Origin + new Vector2(ITEM_PADDING, height + ITEM_PADDING), Engine.Width / 2 - InventoryUI.MARGIN - ITEM_PADDING * 2, 0.6f, ui.PrimaryColor);
        }

        public override void Setup()
        {
            Session session = (ui.Scene as Level)?.Session;
            if (session == null)
                return;
            AreaItemMetadata data = MultiheartHelperModule.itemData.GetValueOrDefault(session.MapData.ModeData.Path);
            if (data == null)
                return;

            foreach (ItemInfo item in data.Items)
            {
                if (item.TabName == Name && MultiheartHelperModule.Session.collectedItems.Contains(item.Name))
                {
                    collected.Add(item);
                    MTexture texture = (item.Gameplay ? GFX.Game : GFX.Gui).textures.GetValueOrDefault(item.Texture);
                    textures.Add(texture?.GetSubtextureCopy());
                }
            }
            ui.Add(selectionWiggler = Wiggler.Create(0.4f, 4f, v =>
            {
                ui.selectionWobble = v * 5;
            }));
        }

        public override void Focus()
        {
            ui.TargetRect = new(InventoryUI.MARGIN + selectedX * ItemSize + SELECTION_PADDING, InventoryUI.MARGIN + selectedY * ItemSize + SELECTION_PADDING, ItemSize - 2 * SELECTION_PADDING, ItemSize - 2 * SELECTION_PADDING);
        }

        public override bool RegisterDirection(int x, int y)
        {
            if (!IsInBounds(selectedX + x, selectedY + y))
            {
                selectionWiggler?.Start();
                return false;
            }
            selectedX += x;
            selectedY += y;
            ui.TargetRect = new(InventoryUI.MARGIN + selectedX * ItemSize + SELECTION_PADDING, InventoryUI.MARGIN + selectedY * ItemSize + SELECTION_PADDING, ItemSize - 2 * SELECTION_PADDING, ItemSize - 2 * SELECTION_PADDING);
            
            return true;
        }

        private bool IsInBounds(int x, int y)
        {
            int i = y * COLUMNS + x;
            return x >= 0 && y >= 0 && 0 <= i && i < collected.Count;
        }
    }
}