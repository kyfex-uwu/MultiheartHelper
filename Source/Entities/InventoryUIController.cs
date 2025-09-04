using System.Collections.Generic;
using System.Diagnostics;
using Celeste.Mod.Entities;
using Celeste.Mod.MultiheartHelper.UI;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MultiheartHelper.Entities
{
    [CustomEntity("MultiheartHelper/InventoryUIController")]
    public class InventoryUIController : Entity
    {
        Color uiColor;
        string[] tabs = [];
        public InventoryUIController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            uiColor = data.HexColor("UIColor");
            tabs = data.String("tabs").Split(',');
        }

        public override void Update()
        {
            if (tabs.Length == 0)
                return;
            if (Scene is Level level && level.Tracker.GetEntity<InventoryUI>() == null && level.Tracker.GetEntity<Player>() is Player player)
            {
                if (MultiheartHelperModule.Settings.DisplayInventoryUI.Pressed && level.CanRetry && (player.StateMachine.State == Player.StNormal && player.OnGround() || player.StateMachine.State == Player.StSwim))
                {
                    level.Add(new InventoryUI(uiColor, tabs));
                }
            }
        }
    }
}