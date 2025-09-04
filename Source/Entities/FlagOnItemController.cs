using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MultiheartHelper.Entities
{
    [CustomEntity("MultiheartHelper/FlagOnItemController")]
    public class FlagOnItemController : Entity
    {
        string outputFlag;
        string itemName;
        bool invert;
        public FlagOnItemController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            outputFlag = data.Attr("flag");
            itemName = data.Attr("itemName");
            invert = data.Bool("invert");
        }

        public override void Update()
        {
            base.Update();

            Session session = (Scene as Level).Session;
            bool collected = MultiheartHelperModule.Session.collectedItems.Contains(itemName);
            session.SetFlag(outputFlag, invert? !collected: collected);
        }
    }
}