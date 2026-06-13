using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Owendraw.Projectiles
{
    public class ContractSlash : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 18;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Projectile.Opacity = Projectile.timeLeft / 18f;

            Projectile.velocity *= 0.92f;

            if (Main.rand.NextBool(2))
            {
                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.PurpleTorch,
                    Projectile.velocity.X * 0.3f,
                    Projectile.velocity.Y * 0.3f,
                    120, default, 0.7f
                );
                Main.dust[dust].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var texture = ModContent.Request<Texture2D>(Texture).Value;

            Color drawColor = new Color(180, 100, 255) * Projectile.Opacity;
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                drawColor,
                Projectile.rotation,
                origin,
                Projectile.scale * 1.2f,
                SpriteEffects.None,
                0
            );

            return false;
        }
    }
}