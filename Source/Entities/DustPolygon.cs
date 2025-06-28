using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MultiheartHelper.Entities {
    [CustomEntity("MultiheartHelper/DustPolygon")]
    public class DustPolygon: Entity {
        public List<Vector2> points = [];

        public DustPolygon(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            points.Add(data.Position + offset);
            foreach(Vector2 node in data.Nodes) {
                points.Add(node + offset);
            }
        }

        public override void Added(Scene scene)
        {
            foreach(Vector2 point in points) {
                scene.Add(new DustStaticSpinner(point, false, false));
            }
        }
    }
}