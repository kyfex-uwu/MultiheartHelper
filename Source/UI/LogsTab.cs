using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.MultiheartHelper.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;


namespace Celeste.Mod.MultiheartHelper.UI {
    public class LogsTab: InventoryTab {
        public override string Name => "Logs";
        public const int ITEM_PADDING = 10;
        public static int ItemAreaWidth => Engine.Width / 2 - InventoryUI.MARGIN;
        float maxHeight = 0;
        int selectedY = 0;
        Wiggler selectionWiggler;

        ScrollUI logList;


        public override void Render()
        {
            RenderLogsArea();
            RenderTextArea();
        }

        private void RenderLogsArea()
        {
            Draw.HollowRect(Engine.Width / 2, InventoryUI.MARGIN, ItemAreaWidth, Engine.Height - InventoryUI.MARGIN * 2, ui.PrimaryColor);
            logList.Render();
        }

        private void RenderTextArea()
        {
            if (!ui.TabFocused)
                return;
            if (selectedY >= MultiheartHelperModule.Session.collectedLogs.Count || selectedY < 0)
                return;
            string selectedItem = MultiheartHelperModule.Session.collectedLogs[selectedY];
            Vector2 Origin = new Vector2(Engine.Width / 2, InventoryUI.MARGIN);
            InventoryUI.DrawTextWrapped(Dialog.Clean($"{selectedItem}_text"), Origin + new Vector2(ITEM_PADDING, ITEM_PADDING), Engine.Width / 2 - InventoryUI.MARGIN - ITEM_PADDING * 2, 0.6f, ui.PrimaryColor);
        }

        public LogsTab(InventoryUI ui): base(ui)
        {
            
        }

        public override void Setup()
        {
            ui.Add(selectionWiggler = Wiggler.Create(0.4f, 4f, v =>
            {
                ui.selectionWobble = v * 5;
            }));
            logList = new(ui, InventoryUI.MARGIN, InventoryUI.MARGIN, ItemAreaWidth, Engine.Height - InventoryUI.MARGIN * 2, RenderLogList);
        }

        public void RenderLogList()
        {
            maxHeight = 0;
            foreach (string logID in MultiheartHelperModule.Session.collectedLogs)
            {
                maxHeight = MathF.Max(maxHeight, ActiveFont.HeightOf(Dialog.Clean($"{logID}_name")));
            }
            logList.TotalSize = new(ItemAreaWidth, (maxHeight + 2 * ITEM_PADDING) * MultiheartHelperModule.Session.collectedLogs.Count);
            logList.Clamped = true;
            int i = 0;
            foreach (string logID in MultiheartHelperModule.Session.collectedLogs)
            {

                ActiveFont.DrawEdgeOutline(Dialog.Clean($"{logID}_name"), logList.Translated(ITEM_PADDING, ITEM_PADDING + i * (ITEM_PADDING * 2 + maxHeight)), Vector2.Zero, Vector2.One, ui.PrimaryColor, 4f, Color.Black);
                i++;
            }
        }

        public override void Focus() {            
            ui.TargetRect = new(InventoryUI.MARGIN, InventoryUI.MARGIN + (int)(selectedY * (maxHeight + ITEM_PADDING * 2)), ItemAreaWidth, (int)(maxHeight + ITEM_PADDING*2));
        }

        public override bool RegisterDirection(int x, int y)
        {
            if (selectedY + y < 0 || selectedY + y >= MultiheartHelperModule.Session.collectedLogs.Count)
            {
                selectionWiggler?.Start();
                return false;
            }
            selectedY += y;

            Vector2 selectionVector = logList.Translated(new Vector2(0, (int)(selectedY * (maxHeight + ITEM_PADDING * 2))));
            selectionVector += new Vector2(logList.bounds.X, logList.bounds.Y);
            if (y > 0 && selectionVector.Y > Engine.Height / 2)
            {
                logList.ScrollByY(maxHeight);
            }
            else if (y < 0 && selectionVector.Y < Engine.Height / 2)
            {
                logList.ScrollByY(-maxHeight);
            }
            selectionVector = logList.TranslatedByTarget(new Vector2(0, (int)(selectedY * (maxHeight + ITEM_PADDING * 2))));
            selectionVector += new Vector2(logList.bounds.X, logList.bounds.Y);

            ui.TargetRect = new((int)selectionVector.X, (int)selectionVector.Y, ItemAreaWidth, (int)(maxHeight + ITEM_PADDING*2));
            
            return true;
        }

    }
}