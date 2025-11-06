using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MultiheartHelper.Triggers
{
    [CustomEntity("MultiheartHelper/RemoveEntities")]
    public class RemoveEntities : Trigger
    {
        string entityName;
        public RemoveEntities(EntityData data, Vector2 offset) : base(data, offset)
        {
            entityName = data.String("entityName");
        }

        public override void OnEnter(Player player)
        {
            List<Entity> remove = [];
            foreach (Entity entity in Scene.Entities)
            {
                var attr = entity.GetType().GetCustomAttributes(false).FirstOrDefault(t => t is CustomEntityAttribute);
                if (attr != null)
                {
                    if ((attr as CustomEntityAttribute).IDs.Contains(entityName))
                    {
                        remove.Add(entity);
                    }
                }
            }

            foreach (Entity entity in remove)
            {
                Scene.Remove(entity);
            }
        }
    }
}