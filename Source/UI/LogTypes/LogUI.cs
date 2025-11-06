using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MultiheartHelper.UI.LogTypes
{
    public abstract class LogUI
    {
        static Dictionary<string, Type> logTypes = [];
        public virtual string Name => GetType().Name;
        internal List<TerminalTab.TerminalLine> lines;
        internal Color? drawColor = null;

        public InventoryUI ui;
        public LogUI(string dialog, InventoryUI ui)
        {
            logTypes[Name] = GetType();
            if (ui == null)
                return;
            lines = [];
            this.ui = ui;
            ParseText(Dialog.Clean(dialog));
        }


        public static LogUI Create(InventoryUI ui, string dialog, string name)
        {
            if (!logTypes.TryGetValue(name, out Type type))
                return null;
            return (LogUI)Activator.CreateInstance(type, dialog, ui);
        }

        public abstract float Render(Vector2 origin, Vector2 size, Color color);
        public virtual void ParseText(string text) {
            foreach (string line in text.Split('\n'))
            {
                ParseLine(line);
            }
        }

        public void ParseLine(string line)
        {
            if (line.StartsWith("$"))
            {
                List<string> split = line.Substring(1).Split(" ").ToList();
                ParseCommand(split[0], split.Slice(1, split.Count - 1).ToArray());
            }
            else
            {
                ParsePlainText(line);
            }
        }

        public virtual void ParsePlainText(string text)
        {
            lines.Add(new(text + "\\n", 0.6f, drawColor));
        }

        public virtual void ParseCommand(string command, params string[] arguments)
        {
            if (command == "heading")
            {
                string text = string.Join(' ', arguments);
                ParseLine(text);
                lines[^1].outlineColor = Color.Black;
                lines[^1].outlineWidth = 2;
                lines[^1].scale = 1f;
            }
            else if (command == "color")
            {
                if (arguments.Length == 0 || arguments[0].Length != 6)
                {
                    drawColor = null;
                }
                else
                {
                    drawColor = Calc.HexToColor(arguments[0]);
                }
            }
        }
    }
}