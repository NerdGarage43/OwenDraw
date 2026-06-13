using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Owendraw.Projectiles
{
    public class ContractArrowMode2 : ModProjectile
    {
        public bool Homing = false;
        public int BonusPen = 0;
        public int AmmoType = ProjectileID.WoodenArrowFriendly;

        private const float HOME_RANGE = 28f * 16f;
        private const float HOME_STRENGTH = 0.10f;
        private const float MAX_SPEED = 14f;
        private const int SLASH_COUNT = 7;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.arrow = true;
            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            Projectile.ArmorPenetration = BonusPen;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(3))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.PurpleTorch, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f,
                    150, default, 0.8f);
                Main.dust[dust].noGravity = true;
            }

            if (Homing)
                HomeToNearestEnemy();
        }

        private void HomeToNearestEnemy()
        {
            NPC target = null;
            float bestDist = HOME_RANGE;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(Projectile)) continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    target = npc;
                }
            }

            if (target == null) return;

            Vector2 toTarget = target.Center - Projectile.Center;
            toTarget.Normalize();
            toTarget *= MAX_SPEED;

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget, HOME_STRENGTH);

            if (Projectile.velocity.Length() > MAX_SPEED)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= MAX_SPEED;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnSlashes(target.Center);
            TryHeal();
        }

        private void SpawnSlashes(Vector2 center)
        {
            int slashDamage = (int)Math.Max(1, Projectile.damage * 0.20f);

            float angleStep = MathHelper.TwoPi / SLASH_COUNT;

            for (int i = 0; i < SLASH_COUNT; i++)
            {
                float angle = i * angleStep;
                Vector2 slashVel = angle.ToRotationVector2() * 6f;

                var slash = Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromAI(),
                    center,
                    slashVel,
                    ModContent.ProjectileType<ContractSlash>(),
                    slashDamage,
                    0f,
                    Projectile.owner
                );

                slash.ArmorPenetration = BonusPen;
                slash.DamageType = DamageClass.Ranged;
            }
        }

        private void TryHeal()
        {
            if (Main.rand.NextFloat() < 0.01f)
            {
                Player owner = Main.player[Projectile.owner];
                owner.Heal(3);
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.PurpleTorch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f),
                    100, default, 0.6f);
            }
        }
    }
}