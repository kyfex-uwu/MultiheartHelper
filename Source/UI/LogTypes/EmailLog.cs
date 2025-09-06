using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MultiheartHelper.UI.LogTypes
{
    public class EmailLog : LogUI
    {
        string from;
        List<string> to = [];
        string date;
        string subject;
        public override string Name => "email";
        public EmailLog(string dialog, InventoryUI ui) : base(dialog, ui)
        {
        }

        public override float Render(Vector2 origin, Vector2 size, Color color)
        {
            float currentY = 0;
            float lastHeight = 0;
            int i = 0;
            foreach (TerminalTab.TerminalLine line in lines)
            {
                // Draw subject (0) and sender (1) normally
                if (i == 2)
                {
                    // Date aligned to right, same height as sender
                    currentY -= lastHeight;
                    ActiveFont.Draw(line.text, new Vector2(size.X, currentY) + origin, new Vector2(1, 0), Vector2.One * line.scale, line.color ?? ui.PrimaryColor);
                    currentY += MathF.Max(lastHeight, ActiveFont.HeightOf(line.text) * line.scale);
                    i++;
                    continue;
                }
                else if (i == 3)
                {
                    // Draw a line before recipients, then draw normally
                    currentY += 5;
                    Draw.Line(new Vector2(0, currentY) + origin, new Vector2(size.X, currentY) + origin, ui.PrimaryColor);
                    currentY += 5;
                }
                else if (i == 4)
                {
                    // Add extra padding below header
                    currentY += 25;
                }
                InventoryUI.DrawTextWrapped(line.text, origin + new Vector2(0, currentY), size.X, line.scale, line.color ?? ui.PrimaryColor, out float height, line.outlineColor, line.outlineWidth);
                lastHeight = height;
                currentY += height;
                i++;
            }
            return currentY;
        }

        public override void ParseText(string text)
        {
            base.ParseText(text);
            lines.Insert(0, new TerminalTab.TerminalLine(Dialog.Clean("MULTIHEARTHELPER_TO") + " " + string.Join(", ", to), 0.3f, ui.PrimaryColor));
            lines.Insert(0, new TerminalTab.TerminalLine(date, 0.3f, ui.PrimaryColor));
            lines.Insert(0, new TerminalTab.TerminalLine(Dialog.Clean("MULTIHEARTHELPER_FROM") + " " + from, 0.3f, ui.PrimaryColor));
            lines.Insert(0, new TerminalTab.TerminalLine(subject, 1f, ui.PrimaryColor, Color.Black, 2));
        }

        public override void ParseCommand(string command, params string[] arguments)
        {
            base.ParseCommand(command, arguments);
            if (command == "from" && arguments.Length > 0)
                from = arguments[0];
            else if (command == "to")
                to = arguments.ToList();
            else if (command == "date" && arguments.Length > 0)
                date = arguments[0];
            else if (command == "subject")
                subject = string.Join(' ', arguments);
        }
    }
}