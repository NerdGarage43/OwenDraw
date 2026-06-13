using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Owendraw.Items.Weapons.Classless.SoulLantern;

namespace Owendraw.Projectiles
{
    public class SoulLanternSentry : ModProjectile
    {
        private const int FIRE_RATE = 20;
        private const float HOVER_HEIGHT = 56f;
        private const float BOB_AMPLITUDE = 4f;
        private const float BOB_SPEED = 0.05f;

        private ref float FireTimer => ref Projectile.ai[0];
        private ref float BobTimer => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 32;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = int.MaxValue;
            Projectile.light = 0.8f;
            Projectile.DamageType = DamageClass.Default;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            // Despawn if the owner is not actively holding the Soul Lantern
            if (owner.HeldItem.type != ModContent.ItemType<Items.Weapons.Classless.SoulLantern>())
            {
                Projectile.Kill();
                return;
            }

            Projectile.damage = SoulLanternHelper.GetScaledDamage();

            BobTimer += BOB_SPEED;
            float bobOffset = (float)Math.Sin(BobTimer) * BOB_AMPLITUDE;
            Vector2 targetPos = owner.Top - new Vector2(0f, HOVER_HEIGHT + bobOffset);

            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.2f);
            Projectile.velocity = Vector2.Zero;

            Lighting.AddLight(Projectile.Center, 1.0f, 0.85f, 0.4f);

            if (Main.rand.NextBool(6))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.GoldFlame, 0f, -0.5f, 150, default, 0.6f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.3f;
            }

            FireTimer++;
            if (FireTimer >= FIRE_RATE)
            {
                FireTimer = 0;
                TryFireProjectile(owner);
            }
        }

        private void TryFireProjectile(Player owner)
        {
            NPC target = FindNearestEnemy();
            if (target == null) return;

            bool planteraDead = NPC.downedPlantBoss;
            bool mlDead = NPC.downedMoonlord;

            int shots = 1;

            if (planteraDead)
            {
                float extraChance = mlDead ? 0.50f : 0.20f;
                if (Main.rand.NextFloat() < extraChance)
                    shots += Main.rand.Next(1, 3);
            }

            for (int i = 0; i < shots; i++)
            {
                float spread = i == 0 ? 0f : Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 toTarget = target.Center - Projectile.Center;
                toTarget.Normalize();
                toTarget = toTarget.RotatedBy(spread) * 10f;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    Projectile.Center,
                    toTarget,
                    ModContent.ProjectileType<SoulLanternBolt>(),
                    Projectile.damage,
                    0f,
                    owner.whoAmI
                );
            }
        }

        private NPC FindNearestEnemy()
        {
            NPC nearest = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    nearest = npc;
                }
            }

            return nearest;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.GoldFlame, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f),
                    100, default, 0.7f);
            }
        }
    }
}