using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Owendraw.Projectiles
{
    public class GuidanceFoxTrail : ModProjectile
    {
        private const float HOME_STRENGTH = 0.04f;
        private const float MAX_SPEED     = 3.5f;
        private const float HOME_RANGE    = 25f * 16f;

        private ref float AgeTimer => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 200;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.light = 0.25f;
        }

        public override void AI()
        {
            AgeTimer++;

            Projectile.rotation += 0.12f;

            Projectile.Opacity = System.Math.Min(1f, AgeTimer / 15f);

            Lighting.AddLight(Projectile.Center, 0.5f, 0.28f, 0.05f);

            if (AgeTimer > 20f)
                SlowHomeToNearestEnemy();
        }

        private void SlowHomeToNearestEnemy()
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

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.OrangeTorch, Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f),
                    120, default, 0.6f);
            }
        }
    }
}