using System;
using System.Collections.Generic;

namespace Celeste.Mod.MultiheartHelper.UI
{
    public abstract class InventoryTab
    {
        static Dictionary<string, Type> tabs = [];
        protected InventoryUI ui;
        public virtual string Name => GetType().Name;
        public abstract void Setup();
        public abstract void Render();

        public InventoryTab(InventoryUI ui)
        {
            this.ui = ui;
            tabs[Name] = GetType();
        }

        public static InventoryTab Create(InventoryUI ui, string name)
        {
            if (!tabs.TryGetValue(name, out Type type))
                return null;
            return (InventoryTab)Activator.CreateInstance(type, ui);
        }

        public virtual void Update()
        {

        }

        public virtual void Focus()
        {
            
        }

        public virtual bool RegisterDirection(int x, int y)
        {
            return false;
        }
    }
}