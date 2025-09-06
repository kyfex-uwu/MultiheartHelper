using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MultiheartHelper.UI
{
    public class TerminalTab : InventoryTab
    {
        public const int ITEM_PADDING = 20;
        public float scrollSpeed = 0.2f;
        public Color? textColor = null;
        public class TerminalLine(string text, float scale, Color? color = null, Color? outlineColor = null, float outlineWidth = 0)
        {
            public string text = text;
            public float scale = scale;
            public Color? color = color;
            public Color outlineColor = outlineColor ?? Color.Transparent;
            public float outlineWidth = outlineWidth;
        }

        public static Queue<TerminalLine> Lines => MultiheartHelperModule.Session.terminalLines;

        Queue<TerminalLine> Destination = null;

        Action<TerminalLine> OnPrintLine = null;

        public override string Name => "Terminal";
        ScrollUI terminal;

        public void PrintLine(string text, float scale, Color? outlineColor = null, float outlineWidth = 0)
        {
            TerminalLine line = new TerminalLine(text, scale, textColor ?? ui.PrimaryColor, outlineColor, outlineWidth);
            (Destination ?? Lines).Enqueue(line);
            OnPrintLine?.Invoke(line);
        }

        public IEnumerator PrintDialog(string key, params string[] parameters)
        {
            string dialog = Dialog.Clean(key);
            for (int i = 0; i < parameters.Length; i++)
            {
                dialog = dialog.Replace($"${i}$", parameters[i]);
            }
            foreach (string line in dialog.Split("\n"))
            {
                yield return ParseLine(line);
            }
        }

        public IEnumerator ParseLine(string line)
        {
            if (line.StartsWith("$"))
            {
                List<string> split = line.Substring(1).Split(" ").ToList();
                yield return ParseCommand(split[0], split.Slice(1, split.Count - 1).ToArray());
            }
            else
            {
                PrintLine(line, 0.6f);
                yield return scrollSpeed;
            }
        }

        public IEnumerator ParseCommand(string command, params string[] arguments)
        {
            if (command == "wait" && arguments.Length > 0 && float.TryParse(arguments[0], out float t))
            {
                yield return t;
            }
            else if (command == "speed" && arguments.Length > 0 && float.TryParse(arguments[0], out float t2))
            {
                scrollSpeed = t2;
            }
            else if (command == "color")
            {
                if (arguments.Length == 0 || arguments[0].Length != 6)
                {
                    textColor = null;
                }
                else
                {
                    textColor = Calc.HexToColor(arguments[0]);
                }
            }
            else if (command == "heading")
            {
                string text = string.Join(' ', arguments);
                PrintLine(text, 1f, Color.Black, 2f);
                yield return scrollSpeed;
            }
            else if (command == "append")
            {
                string text = string.Join(' ', arguments);
                Destination = [];
                OnPrintLine = line =>
                {
                    if (Lines.Count == 0)
                        return;
                    Lines.Last().text += line.text;
                };
                yield return ParseLine(text);
                Destination = null;
                OnPrintLine = null;

                yield return scrollSpeed;
            }
            else if (command == "type")
            {
                string text = string.Join(' ', arguments);
                float speed = scrollSpeed;
                if (float.TryParse(arguments[0], out speed) && arguments.Length > 1) {
                    text = text.Substring(arguments[0].Length + 1);
                }
                foreach (char c in text)
                {
                    Lines.Last().text += c;
                    yield return speed;
                }
            }
        }

        public TerminalTab(InventoryUI ui) : base(ui)
        {
        }

        public override void Render()
        {
            terminal.Render();
        }

        public override void Setup()
        {
            terminal = new(ui, InventoryUI.MARGIN + ITEM_PADDING, InventoryUI.MARGIN + ITEM_PADDING, Engine.Width - 2 * (InventoryUI.MARGIN + ITEM_PADDING), Engine.Height - 2 * (InventoryUI.MARGIN + ITEM_PADDING), RenderTerminal);
            terminal.Clamped = true;
        }

        private void RenderTerminal()
        {
            float currentY = 0;
            foreach (TerminalLine line in Lines)
            {
                ActiveFont.DrawOutline(line.text, terminal.Translated(0, currentY), Vector2.Zero, Vector2.One * line.scale, line.color ?? ui.PrimaryColor, line.outlineWidth, line.outlineColor);
                currentY += ActiveFont.HeightOf(line.text) * line.scale;
            }
            terminal.TotalSize = new(0, currentY);
            terminal.ScrollToBottom();
        }
    }
}