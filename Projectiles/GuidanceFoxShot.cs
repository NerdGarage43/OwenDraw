using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Owendraw.Projectiles
{    
    public class GuidanceFoxShot : ModProjectile
    {
        private const float HOME_STRENGTH = 0.16f;
        private const float MAX_SPEED     = 13f;
        private const float HOME_RANGE    = 45f * 16f;

        private ref float AgeTimer => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.light = 0.35f;
        }

        public override void AI()
        {
            AgeTimer++;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(2))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.OrangeTorch, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f,
                    120, default, 0.7f);
                Main.dust[dust].noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.7f, 0.4f, 0.1f);

            if (AgeTimer > 8f)
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

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.OrangeTorch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f),
                    100, default, 0.8f);
            }
        }
    }
}