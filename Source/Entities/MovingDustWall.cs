using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.MultiheartHelper.Entities {
    [CustomEntity("MultiheartHelper/MovingDustWall")]
    [Tracked()]
    public class MovingDustWall : Entity
    {
        Vector2 startPosition, endPosition;
        Vector2 moveOffset;
        bool waiting;
        float maxDelay = 0.05f;
        float delay = 0.5f;
        Level level;
        int dustCount = 30;
        int spreadCount = 3;
        int maxDustAmount = 600;
        int smoothing = 500;
        List<DustStaticSpinner> dusts = [];
        List<Vector2> spreadPoints = [];
        List<Vector2> newSpreadPoints = [];
        string flag = "";
        public MovingDustWall(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            maxDelay = data.Float("spreadTime", 0.05f);
            dustCount = data.Int("dustCount", 30);
            maxDustAmount = data.Int("maxDustAmount", 600);
            spreadCount = data.Int("spreadCount", 3);
            smoothing = data.Int("smoothing", 500);
            flag = data.Attr("flag", "");
            startPosition = data.Position;
            endPosition = data.Nodes[0];
            moveOffset = (endPosition - startPosition).Perpendicular().SafeNormalize();
        }

        public override void Added(Scene scene)
        {
            foreach(DustStaticSpinner spinner in dusts) {
                spinner.RemoveSelf();
            }
            dusts.Clear();
            for(int i = 0; i < dustCount; i++) {
                Vector2 spawnPos = startPosition + i * (endPosition - startPosition)/dustCount;
                Logger.Warn("dwedwed", spawnPos.ToString());
                level = scene as Level;
                spawnPos += new Vector2(level.Bounds.Left, level.Bounds.Top);
                level.Add(new DustStaticSpinner(spawnPos, false));
                spreadPoints.Add(spawnPos);
            }
        }

        public override void Update()
        {
		    Player entity = level.Tracker.GetEntity<Player>();
            base.Update();
            if(entity == null)
                return;
            if (!level.Session.GetFlag(flag))
            {
                return;
            }

            delay -= Engine.DeltaTime;
            if(delay <= 0) {
                delay = maxDelay/dustCount;
                for(int j = 0; j < spreadCount; j++) {
                    Vector2 spread = Calc.Random.Choose(spreadPoints);
                    spreadPoints.Remove(spread);
                    float angle = Calc.Random.Range(-MathF.PI/30, MathF.PI/30);
                    angle += moveOffset.Angle();
                    Vector2 trueOffset = new(MathF.Cos(angle), MathF.Sin(angle));
                    Vector2 spawnPos = spread + trueOffset * Calc.Random.Range(9, 11);
                    
                    var dust = new DustStaticSpinner(spread, false, true);
                    dust.Add(new MoveToTarget(true, false, spawnPos, smoothing, dust));
                    level.Add(dust);
                    dusts.Add(dust);
                    if(dusts.Count > maxDustAmount) {
                        dusts[0].RemoveSelf();
                        dusts.RemoveAt(0);
                    }
                    newSpreadPoints.Add(spawnPos);
                    if(spreadPoints.Count == 0) {
                        spreadPoints = [.. newSpreadPoints];
                        newSpreadPoints.Clear();
                    }
                }
            }
        }
    }


    public class MoveToTarget : Component
    {
        Vector2 startPos, targetPos;
        int moveTime;
        int currentTime = 0;
        public MoveToTarget(bool active, bool visible, Vector2 targetPos, int moveTime, Entity entity) : base(active, visible)
        {
            startPos = entity.Position;
            this.targetPos = targetPos;
            this.moveTime = moveTime;
        }

        public override void Update()
        {
            base.Entity.Position += currentTime * (targetPos - base.Entity.Position) * 1f/moveTime;
            currentTime++;    
        }
    }
}