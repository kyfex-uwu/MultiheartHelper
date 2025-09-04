using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.MultiheartHelper.Data;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MultiheartHelper.Entities
{
    [CustomEntity("MultiheartHelper/ItemCollectible")]
    public class ItemCollectible : Entity
    {
        string itemName;
        string texture;
        Sprite sprite;
        private Wiggler wiggler;
	    private Wiggler rotateWiggler;
        private Follower Follower;
        private Vector2 start;
        float wobble = 0;
        float collectTimer = 0;
        bool collected = false;
        private bool IsFirstItem
        {
            get
            {
                for (int num = Follower.FollowIndex - 1; num >= 0; num--)
                {
                    if (Follower.Leader.Followers[num].Entity is ItemCollectible)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public ItemCollectible(EntityData data, Vector2 offset, EntityID gid) : base(data.Position + offset)
        {
            Depth = -100;
            itemName = data.String("itemName") ?? "";
            start = data.Position + offset;
            texture = data.String("texture") ?? GetItem(itemName)?.Texture ?? "";
            Add(sprite = new Sprite(GFX.Game, texture));
            sprite.AddLoop("idle", "idle", 0.08f);
            sprite.Add("collect", "collect", 0.08f);
            sprite.Play("idle");
            Collider = new Hitbox(14, 14, (sprite.width - 14)/2, (sprite.height - 14)/2);
            Add(new PlayerCollider(OnPlayer));
            Add(new MirrorReflection());
            Add(Follower = new(gid, null, OnLoseLeader));
        }

        public override void Update()
        {
            wobble += Engine.DeltaTime * 4f;
            Sprite obj = sprite;
            obj.Y = (float)Math.Sin(wobble) * 2f;
            int followIndex = Follower.FollowIndex;
            if (Follower.Leader != null && Follower.DelayTimer <= 0f && IsFirstItem)
            {
                Player player = Follower.Leader.Entity as Player;
                bool flag = false;
                if (player != null && player.Scene != null && !player.StrawberriesBlocked)
                {
                    if (player.OnSafeGround && (player.StateMachine.State != 13))
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    collectTimer += Engine.DeltaTime;
                    if (collectTimer > 0.15f)
                    {
                        OnCollect();
                    }
                }
                else
                {
                    collectTimer = Math.Min(collectTimer, 0f);
                }
            }
            else
            {
                if (followIndex > 0)
                {
                    collectTimer = -0.15f;
                }
            }
            base.Update();
        }

        private void OnCollect()
        {
            if (collected)
                return;
            collected = true;
            if (itemName != "")
                MultiheartHelperModule.Session.collectedItems.Add(itemName);
            if (Follower.Leader?.Entity is Player player)
                Add(new Coroutine(CollectRoutine(player.StrawberryCollectIndex)));
        }

        private IEnumerator CollectRoutine(int collectIndex)
        {
            Tag = Tags.TransitionUpdate;
            Depth = -2000010;
            sprite.Play("collect");
		    Audio.Play("event:/game/general/strawberry_get", Position, "colour", 0, "count", collectIndex);
            while (sprite.Animating)
                yield return null;

            RemoveSelf();
        }

        private void OnLoseLeader()
        {
            Alarm.Set(this, 0.15f, delegate
            {
                Vector2 vector = (start - Position).SafeNormalize();
                float num = Vector2.Distance(Position, start);
                float num2 = Calc.ClampedMap(num, 16f, 120f, 16f, 96f);
                Vector2 control = start + vector * 16f + vector.Perpendicular() * num2 * Calc.Random.Choose(1, -1);
                SimpleCurve curve = new SimpleCurve(Position, start, control);
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, MathHelper.Max(num / 100f, 0.4f), start: true);
                tween.OnUpdate = delegate(Tween f)
                {
                    Position = curve.GetPoint(f.Eased);
                };
                tween.OnComplete = delegate
                {
                    base.Depth = 0;
                };
                Add(tween);
            });
        }

        private void OnPlayer(Player player)
        {
            if (Follower.Leader != null)
                return;
            Audio.Play("event:/game/general/strawberry_touch", Position);
            player.Leader.GainFollower(Follower);
            wiggler.Start();
            Depth = -10000000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(sprite);
            Add(wiggler = Wiggler.Create(0.4f, 4f, delegate (float v)
            {
                sprite.Scale = Vector2.One * (1f + v * 0.35f);
            }));
            Add(rotateWiggler = Wiggler.Create(0.5f, 4f, delegate (float v)
            {
                sprite.Rotation = v * 30f * ((float)Math.PI / 180f);
            }));
        }

        AreaItemMetadata GetItems() => MultiheartHelperModule.itemData.GetValueOrDefault((Scene as Level)?.Session?.MapData.ModeData.Path ?? "");
        ItemInfo GetItem(string name) => GetItems()?.Items.FirstOrDefault(i => i.Name == name);
    }
}