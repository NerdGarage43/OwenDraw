using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Owendraw.Items.Weapons.Classless;

namespace Owendraw.Projectiles
{
    public class GuidanceFoxHead : BaseWormProjectile
    {
        // ── Segment setup ────────────────────────────────────────────────────────
        public override string Texture => "Owendraw/Projectiles/GuidanceFoxHead";
        public override List<string> SegmentTextures => new()
        {
            "Owendraw/Projectiles/GuidanceFoxBody",
            "Owendraw/Projectiles/GuidanceFoxTail"
        };

        public override int SegmentCount => GuidanceHelper.GetSegmentCount();

        // Position offsets so segments connect naturally at the sprite edges
        public override List<float> SegmentTypePositionOffsets => new()
        {
            22f, // Head half-height
            18f, // Body half-height
            18f  // Tail half-height
        };

        // ── AI slots ─────────────────────────────────────────────────────────────
        private ref float ShotTimer  => ref Projectile.ai[0];
        private ref float TrailTimer => ref Projectile.ai[1];
        private ref float StateTimer => ref Projectile.ai[2];

        // ── Constants ────────────────────────────────────────────────────────────
        private const float IDLE_SPEED    = 6f;
        private const float CHASE_SPEED   = 14f;
        private const float DASH_SPEED    = 22f;
        private const float CHASE_RANGE   = 40f * 16f;
        private const float DASH_RANGE    = 18f * 16f;
        private const float TURN_SPEED    = 0.12f;
        private const int   SHOT_RATE     = 60;
        private const int   TRAIL_RATE    = 8;
        private const int   DASH_DURATION = 18;

        private bool _isDashing = false;
        private int  _dashTimer = 0;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = int.MaxValue;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.MaxUpdates = 1;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            // Sync damage every frame
            Projectile.damage = GuidanceHelper.GetScaledDamage();

            // Keep segment list in sync with boss progression
            while (Segments.Count < SegmentCount)
                Segments.Add(new BaseWormSegment(this, 0));
            while (Segments.Count > SegmentCount)
                Segments.RemoveAt(Segments.Count - 1);

            // Last segment is always the tail type (index 1)
            if (Segments.Count > 0)
            {
                for (int i = 0; i < Segments.Count; i++)
                    Segments[i].segmentType = 0;
                Segments[Segments.Count - 1].segmentType = 1;
            }

            NPC target = FindNearestEnemy();
            UpdateMovement(owner, target);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Dust trail
            if (Main.rand.NextBool(3))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.OrangeTorch, -Projectile.velocity.X * 0.3f, -Projectile.velocity.Y * 0.3f,
                    100, default, 0.8f);
                Main.dust[dust].noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.9f, 0.5f, 0.1f);

            // Post-WoF trail embers
            if (GuidanceHelper.HasTrail && Main.myPlayer == Projectile.owner)
            {
                TrailTimer++;
                if (TrailTimer >= TRAIL_RATE)
                {
                    TrailTimer = 0;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromAI(),
                        Projectile.Center,
                        new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.5f, 0.5f)),
                        ModContent.ProjectileType<GuidanceFoxTrail>(),
                        (int)(Projectile.damage * 0.4f),
                        0f,
                        Projectile.owner
                    );
                }
            }

            // Post-Plantera front shots
            if (GuidanceHelper.HasFrontShot && Main.myPlayer == Projectile.owner && target != null)
            {
                ShotTimer++;
                if (ShotTimer >= SHOT_RATE)
                {
                    ShotTimer = 0;
                    FireFrontShots(target);
                }
            }

            UpdateSegments();
        }

        private void UpdateMovement(Player owner, NPC target)
        {
            if (_isDashing)
            {
                _dashTimer++;
                if (_dashTimer >= DASH_DURATION)
                {
                    _isDashing = false;
                    _dashTimer = 0;
                }
            }
            else if (target != null)
            {
                float dist = Vector2.Distance(Projectile.Center, target.Center);

                if (dist < DASH_RANGE)
                {
                    _isDashing = true;
                    _dashTimer = 0;
                    Projectile.velocity = Projectile.DirectionTo(target.Center) * DASH_SPEED;
                }
                else
                {
                    Vector2 toTarget = Projectile.DirectionTo(target.Center) * CHASE_SPEED;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget, TURN_SPEED);
                }
            }
            else
            {
                // Idle orbit around player
                Vector2 idleTarget = owner.Center + new Vector2(
                    (float)Math.Cos(Main.GameUpdateCount * 0.04f) * 80f,
                    (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 50f - 40f
                );
                Vector2 toIdle = Projectile.DirectionTo(idleTarget) * IDLE_SPEED;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle, 0.08f);
            }

            float maxSpeed = _isDashing ? DASH_SPEED : CHASE_SPEED;
            if (Projectile.velocity.Length() > maxSpeed)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= maxSpeed;
            }
        }

        private void FireFrontShots(NPC target)
        {
            int count = GuidanceHelper.FrontShotCount;
            float spreadStep = MathHelper.ToRadians(12f);
            float baseAngle  = Projectile.DirectionTo(target.Center).ToRotation();
            float startAngle = baseAngle - (count - 1) * spreadStep / 2f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + i * spreadStep;
                Vector2 vel = angle.ToRotationVector2() * 9f;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 20f,
                    vel,
                    ModContent.ProjectileType<GuidanceFoxShot>(),
                    (int)(Projectile.damage * 0.75f),
                    0f,
                    Projectile.owner
                );
            }
        }

        private NPC FindNearestEnemy()
        {
            NPC nearest  = null;
            float bestDist = CHASE_RANGE;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(Projectile)) continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    nearest  = npc;
                }
            }

            return nearest;
        }

        // Draw segments back-to-front, then head on top (matches VoidEaterMarionette pattern)
        public override bool PreDraw(ref Color lightColor)
        {
            for (int i = Segments.Count - 1; i >= 0; i--)
                DrawSegment(ref lightColor, Segments[i]);

            Texture2D headTex = ModContent.Request<Texture2D>(Texture).Value;
            Main.spriteBatch.Draw(
                headTex,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor * Projectile.Opacity,
                Projectile.rotation,
                headTex.Size() / 2f,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.OrangeTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f),
                    100, default, 0.9f);
            }
        }
    }
}