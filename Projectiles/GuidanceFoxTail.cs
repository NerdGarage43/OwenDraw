using Terraria;
using Terraria.ModLoader;

namespace Owendraw.Projectiles
{
    public class GuidanceFoxTail : ModProjectile
    {
        public override string Texture => "Owendraw/Projectiles/GuidanceFoxTail";

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = int.MaxValue;
        }
    }
}