using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Owendraw.Projectiles
{
    public class SoulLanternBolt : ModProjectile
    {
        private const float HOME_STRENGTH = 0.14f;
        private const float MAX_SPEED = 11f;
        private const float HOME_RANGE = 50f * 16f;

        private ref float AgeTimer => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.light = 0.4f;
        }

        public override void AI()
        {
            AgeTimer++;

            if (Main.rand.NextBool(2))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.GoldFlame, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f,
                    120, default, 0.7f);
                Main.dust[dust].noGravity = true;
            }

            if (AgeTimer > 10f)
                HomeToNearestEnemy();

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Lighting.AddLight(Projectile.Center, 0.6f, 0.5f, 0.2f);
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
            if (Main.hardMode)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    Projectile.Center,
                    Vector2.Zero,
                    ProjectileID.DaybreakExplosion,
                    (int)(Projectile.damage * 0.6f),
                    0f,
                    Projectile.owner
                );
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.GoldFlame, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f),
                    100, default, 0.7f);
            }
        }
    }
}