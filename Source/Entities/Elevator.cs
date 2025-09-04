using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.MultiheartHelper.Entities
{
    [CustomEntity("MultiheartHelper/Elevator")]
    [Tracked]
    public class Elevator : Entity
    {
        Texture2D frameTex, leftDoorTex, rightDoorTex, topTex, bottomTex;
        float openAmount = 0;
        float descendAmount = 0;
        TalkComponent talk;
        Entity bgEntity;
        Fadeout fadeout;
        public string myID, targetID;
        string targetRoom;
        int down = 1;
        SoundSource moveSfx;
        bool playSound = false;
        string flag = "";
        Wiggler doorWiggler;
        Vector2 doorWobble;

        public Elevator(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            string path = data.Attr("textureFolder");
            targetRoom = data.Attr("targetRoom");
            targetID = data.Attr("targetID");
            myID = data.Attr("elevatorID");
            down = data.Bool("down", true) ? 1 : -1;
            flag = data.Attr("flag");
            MTexture bgTex = GFX.Game.textures.GetValueOrDefault(path + "bg");
            frameTex = GFX.Game.textures.GetValueOrDefault(path + "frame")?.GetSubtextureCopy();
            leftDoorTex = GFX.Game.textures.GetValueOrDefault(path + "leftDoor")?.GetSubtextureCopy();
            rightDoorTex = GFX.Game.textures.GetValueOrDefault(path + "rightDoor")?.GetSubtextureCopy();
            topTex = GFX.Game.textures.GetValueOrDefault(path + "top")?.GetSubtextureCopy();
            bottomTex = GFX.Game.textures.GetValueOrDefault(path + "bottom")?.GetSubtextureCopy();
            Add(talk = new TalkComponent(new Rectangle(-1, 24, 1 * 8, 4 * 8), new Vector2(3, 20f), OnInteract));
            Add(doorWiggler = Wiggler.Create(0.5f, 4f, v =>
            {
                doorWobble = new Vector2(v, MathF.Sin(3 * v));
            }));
            talk.PlayerMustBeFacing = false;
            bgEntity = new(Position + new Vector2(8, 8));
            bgEntity.Depth = Depth + 5;
            Sprite bgSprite;
            bgEntity.Add(bgSprite = new Sprite(GFX.Game, path));
            bgSprite.AddLoop("idle", 0.08f, bgTex);
            bgSprite.Play("idle");
            fadeout = new();
            moveSfx = new();
            bgEntity.Add(moveSfx);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(bgEntity);
            scene.Add(fadeout);
        }

        private void OnInteract(Player player)
        {
            if (flag != "" && !(Scene as Level).Session.GetFlag(flag))
            {
                doorWiggler.Start();
                return;
            }

            player.DummyBegin();
            Tween tween = AddDoorTween(false, (f) =>
            {
                if ((int)openAmount == 8)
                {
                    Add(new Coroutine(player.DummyWalkToExact((int)X + 24, false, 0.5f), true));
                }
            });
            tween.OnComplete = (f) =>
            {
                Add(new Coroutine(WaitForWalkRoutine(player, () =>
                {
                    player.Depth = Depth + 3;
                    Tween closeTween = AddDoorTween(true);
                    closeTween.OnComplete = (f) =>
                    {
                        player.Collidable = false;
                        player.DummyGravity = false;
                        player.DummyAutoAnimate = false;
                        Audio.Play("event:/game/04_cliffside/arrowblock_activate", Position);
                        Tween descendTween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineIn, 2.5f, true);
                        descendTween.OnUpdate = (g) =>
                        {
                            descendAmount = down * 16 * 8 * g.Eased;
                            bgEntity.Position = Position + new Vector2(8, 8 + descendAmount);
                            player.Y = Y + 56 + descendAmount;
                            if (g.Eased > 0.5f)
                            {
                                fadeout.opacity = (g.Eased - 0.5f) * 2;
                            }
                        };
                        descendTween.OnComplete = (g) => Teleport(player);
                        Add(descendTween);
                    };
                    Add(closeTween);
                }), true));
            };
            Add(tween);
        }

        Tween AddDoorTween(bool close = false, Action<Tween> ExtraOnUpdate = null)
        {
            Audio.Play(close ? "event:/game/03_resort/door_metal_close" : "event:/game/03_resort/door_metal_open", Position);
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, 1f, true);
            Add(tween);
            tween.OnUpdate = (f) =>
            {
                openAmount = close ? (1 - f.Eased) * 16 : f.Eased * 16;
                ExtraOnUpdate?.Invoke(f);
            };
            return tween;
        }

        // Parts are copied from vivhelper's perfectteleport: https://github.com/vivianlonging/VivHelper/blob/master/_Code/Module%2C%20Extensions%2C%20Etc/TeleporterFunctions.cs#L125
        void Teleport(Player player) {
            Level level = Scene as Level;
            MultiheartHelperModule.Session.BeforeUpdateNextFrame.Add(() =>
            {
                LevelData targetData = level.Session.MapData.Get(targetRoom);
                EntityData targetElevator = null;
                foreach (EntityData entity in targetData.Entities)
                {
                    if (entity.Name == "MultiheartHelper/Elevator")
                    {
                        if (entity.Attr("elevatorID") == targetID)
                        {
                            targetElevator = entity;
                        }
                    }
                }

                if (targetElevator == null)
                {
                    return;
                }

                Vector2 pos = player.Position;
                Facings facing = player.Facing;
                int dashes = player.Dashes;
                Vector2 cameraDiff = level.Camera.position - player.CameraTarget;
                player.CleanUpTriggers();

                string prevRoom = level.Session.Level;

                Leader leader = player.Leader;
                for (int i = 0; i < leader.PastPoints.Count; i++)
                {
                    leader.PastPoints[i] -= pos;
                }
                foreach (Follower follower in leader.Followers)
                {
                    if (follower.Entity != null)
                    {
                        follower.Entity.Position -= pos;
                        follower.Entity.AddTag(Tags.Global);
                        if (follower.ParentEntityID.ID != -1)
                            level.Session.DoNotLoad.Add(follower.ParentEntityID);
                    }
                }

                List<Component> transitionOut = new List<Component>();
                List<Component> transitionIn = new List<Component>();
                if (level.Tracker.Components.TryGetValue(typeof(TransitionListener), out var u))
                {
                    transitionOut = [.. u];
                }

                Vector2 targetPos = targetData.Position + targetElevator.Position;


                level.Remove(player);
                // level.Entities.Remove(level.Entities);
                level.Displacement.Clear();
                level.Particles.Clear();
                level.ParticlesBG.Clear();
                level.ParticlesFG.Clear();
                TrailManager.Clear();

                level.UnloadLevel();
                level.Session.Level = targetRoom;
                level.Session.RespawnPoint = level.Session.GetSpawnPoint(targetPos);
                level.Session.FirstLevel = false;
                level.Add(player);
                level.LoadLevel(Player.IntroTypes.Transition);

                player.Position = targetPos;
                player.Hair.MoveHairBy(targetPos - pos);
                player.Facing = facing;
                player.Dashes = dashes;

                foreach (Follower follower in leader.Followers)
                {
                    if (follower.Entity != null)
                    {
                        follower.Entity.Position += player.TopLeft;
                        follower.Entity.RemoveTag(Tags.Global);
                        if (follower.ParentEntityID.ID != -1 && !level.Session.Keys.Contains(follower.ParentEntityID))
                            level.Session.DoNotLoad.Remove(follower.ParentEntityID);
                    }
                }
                for (int i = 0; i < leader.PastPoints.Count; i++)
                {
                    leader.PastPoints[i] += player.Position;
                }

                if (level.Tracker.Components.TryGetValue(typeof(TransitionListener), out var w))
                {
                    transitionIn = [.. w];
                    transitionIn.RemoveAll((Component c) => transitionOut.Contains(c));
                    foreach (TransitionListener item in transitionOut)
                    {
                        item?.OnOutBegin?.Invoke();
                        item?.OnOut?.Invoke(1f); // We want to instantly set to the values in
                    }
                    foreach (TransitionListener item in transitionIn)
                    {
                        item?.OnInBegin?.Invoke();
                        item?.OnIn?.Invoke(1f);
                        item?.OnInEnd?.Invoke();
                    }
                }

                leader.TransferFollowers();
                level.Camera.Position = targetPos + new Vector2(24, 32) - new Vector2(level.Camera.Viewport.Width, level.Camera.Viewport.Height)/2;
                ClampCamera(level);

                foreach (var e in level.Tracker.GetEntities<Elevator>())
                {
                    if (e is Elevator elevator && elevator.myID == targetID)
                    {
                        elevator.DoRoomEntry(player);
                        break;
                    }
                }
            });
        }

        void ClampCamera(Level level)
        {
            if (level.Camera.Bottom > level.Bounds.Bottom)
            {
                level.Camera.Top = level.Bounds.Bottom - level.Camera.Viewport.Height;
            }
            else if (level.Camera.Top < level.Bounds.Top)
            {
                level.Camera.Top = level.Bounds.Top;
            }
            if (level.Camera.Left < level.Bounds.Left)
            {
                level.Camera.Left = level.Bounds.Left;
            }
            else if (level.Camera.Right > level.Bounds.Right)
            {
                level.Camera.Left = level.Bounds.Right - level.Camera.Viewport.Width;
            }
        }

        public void DoRoomEntry(Player player)
        {
            Tween enterTween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, 2.5f, true);
            moveSfx.Play("event:/game/04_cliffside/arrowblock_move");
            moveSfx.Param("arrow_stop", 0f);
            enterTween.OnUpdate = (f) =>
            {
                if (f.Eased < 0.5f)
                    fadeout.opacity = (0.5f - f.Eased) * 2;
                else
                {
                    fadeout.opacity = 0;
                }

                descendAmount = down * 16 * 8 * (1 - f.Eased);
                bgEntity.Position = Position + new Vector2(8, 8 + descendAmount);
                player.Y = Y + 56 + descendAmount;
                player.X = X + 24;
            };

            enterTween.OnComplete = _ =>
            {
                player.DummyGravity = true;
                player.Collidable = true;
                player.DummyAutoAnimate = true;
                Tween openTween = AddDoorTween();
                moveSfx.Stop();
                openTween.OnComplete = (f) =>
                {
                    player.Depth = 0;
                    Tween closeTween = AddDoorTween(true);
                    player.StateMachine.State = Player.StNormal;
                };
            };
            Add(enterTween);
        }
        

        public IEnumerator WaitForWalkRoutine(Player player, Action OnFinished)
        {
            while (player.DummyMoving)
            {
                yield return null;
            }
            OnFinished();
        }

        public override void Render()
        {
            if (topTex != null)
                Draw.SpriteBatch.Draw(topTex, Position + new Vector2(8, 0), new Rectangle(0, topTex.Height - 9 - (int)descendAmount, topTex.Width, 9 + (int)descendAmount), Color.White);
            if (bottomTex != null)
                Draw.SpriteBatch.Draw(bottomTex, Position + new Vector2(8, 55 + (int)descendAmount), new Rectangle(0, 0, topTex.Width, topTex.Height - (int)descendAmount), Color.White);
            if (leftDoorTex != null)
                Draw.SpriteBatch.Draw(leftDoorTex, Position + new Vector2(8, 8) - doorWobble, new Rectangle((int)openAmount, 0, leftDoorTex.Width - (int)openAmount, leftDoorTex.Height), Color.White);
            if (rightDoorTex != null)
                Draw.SpriteBatch.Draw(rightDoorTex, Position + new Vector2(8 + openAmount, 8) + doorWobble, new Rectangle(0, 0, rightDoorTex.Width - (int)openAmount, rightDoorTex.Height), Color.White);
            if (frameTex != null)
                Draw.SpriteBatch.Draw(frameTex, Position - new Vector2(1, 1), Color.White);
        }
    }

    public class Fadeout : Entity
    {
        public float opacity = 0;
        public Fadeout()
        {
            Tag = Tags.PauseUpdate | Tags.HUD;
            Depth = Depths.FGTerrain - 2;
        }

        public override void Render()
        {
            Draw.Rect(0, 0, Engine.Width+2, Engine.Height+2, Color.Black * opacity);
        }
    }
}