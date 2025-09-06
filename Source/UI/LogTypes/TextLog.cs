using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MultiheartHelper.UI.LogTypes
{
    public class TextLog : LogUI
    {
        public override string Name => "text";
        public TextLog(string dialog, InventoryUI ui) : base(dialog, ui)
        {
        }

        public override float Render(Vector2 origin, Vector2 size, Color color)
        {
            float currentY = 0;
            foreach (TerminalTab.TerminalLine line in lines)
            {
                InventoryUI.DrawTextWrapped(line.text, origin + new Vector2(0, currentY), size.X, line.scale, line.color ?? ui.PrimaryColor, out float height, line.outlineColor, line.outlineWidth);
                currentY += height;
            }
            return currentY;
        }

        public override void ParseText(string text)
        {
            lines = [];
            base.ParseText(text);
        }
    }
}