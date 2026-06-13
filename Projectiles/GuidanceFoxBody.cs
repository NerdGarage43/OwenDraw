using Terraria;
using Terraria.ModLoader;

namespace Owendraw.Projectiles
{
    public class GuidanceFoxBody : ModProjectile
    {
        public override string Texture => "Owendraw/Projectiles/GuidanceFoxBody";

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = int.MaxValue;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
    }
}