using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MultiheartHelper.Triggers
{
    [CustomEntity("MultiheartHelper/AcceleratePlayer")]
    public class AcceleratePlayer : Trigger
    {
        float acceleration { get; set; }
        float terminalVelocity { get; set; }
        Vector2 target { get; set; }
        public AcceleratePlayer(EntityData data, Vector2 offset) : base(data, offset)
        {
            acceleration = data.Float(nameof(acceleration));
            terminalVelocity = data.Float(nameof(terminalVelocity));
            target = data.Nodes[0] + offset;
        }

        public override void OnStay(Player player)
        {
            player.Speed += (target - player.Center).SafeNormalize() * acceleration;
            if (terminalVelocity > 0 && player.Speed.LengthSquared() >= terminalVelocity * terminalVelocity)
            {
                player.Speed = player.Speed.SafeNormalize() * terminalVelocity;
            }
        }
    }
}