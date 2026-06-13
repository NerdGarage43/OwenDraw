using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Owendraw.Projectiles;

namespace Owendraw.Items.Weapons.Classless
{
    public class Guidance : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.DamageType = DamageClass.Default;
            Item.damage = GuidanceHelper.GetScaledDamage();
            Item.knockBack = 3f;
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Blue;
            Item.autoReuse = true;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<GuidanceFoxHead>();
            Item.shootSpeed = 10f;
        }

        public override void UpdateInventory(Player player)
        {
            Item.damage = GuidanceHelper.GetScaledDamage();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var gp = player.GetModPlayer<GuidancePlayer>();
            gp.HoldingGuidance = true;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == ModContent.ProjectileType<GuidanceFoxHead>())
                {
                    return false;
                }
            }

            Projectile.NewProjectile(source, player.Center, velocity,
                ModContent.ProjectileType<GuidanceFoxHead>(),
                damage, knockback, player.whoAmI);

            return false;
        }

        public override void HoldItem(Player player)
        {
            if (player.channel)
            {
                var gp = player.GetModPlayer<GuidancePlayer>();
                gp.HoldingGuidance = true;
            }
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "GuidanceDesc", "Hold to summon a fox spirit that hunts nearby enemies"));

            if (GuidanceHelper.HasTrail)
                tooltips.Add(new TooltipLine(Mod, "GuidanceTrail", "Leaves behind slow homing embers"));

            if (GuidanceHelper.HasFrontShot)
                tooltips.Add(new TooltipLine(Mod, "GuidanceShot",
                    $"Fires {GuidanceHelper.FrontShotCount} homing projectiles per second"));
        }
    }
    public class GuidancePlayer : ModPlayer
    {
        public bool HoldingGuidance = false;
 
        public override void ResetEffects()
        {
            if (!HoldingGuidance)
                KillFox();
 
            HoldingGuidance = false;
        }
 
        private void KillFox()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != Player.whoAmI) continue;
 
                if (p.type == ModContent.ProjectileType<Projectiles.GuidanceFoxHead>() ||
                    p.type == ModContent.ProjectileType<Projectiles.GuidanceFoxBody>() ||
                    p.type == ModContent.ProjectileType<Projectiles.GuidanceFoxTail>())
                {
                    p.Kill();
                }
            }
        }
    }
    public static class GuidanceHelper
    {
        public static int GetScaledDamage()
        {
            int damage = 8;
 
            if (NPC.downedSlimeKing)      damage += 2;   // 10
            if (NPC.downedBoss1)          damage += 1;   // 11  EoC
            if (NPC.downedBoss2)          damage += 4;   // 15  Evils
            if (NPC.downedQueenBee)       damage += 5;   // 20
            if (NPC.downedDeerclops)      damage += 5;   // 25
            if (NPC.downedBoss3)          damage += 5;   // 30  Skeletron
            if (Main.hardMode)            damage += 5;   // 35  WoF
            if (NPC.downedQueenSlime)     damage += 5;   // 40
            if (NPC.downedMechBoss2)      damage += 5;   // 45  Twins
            if (NPC.downedMechBoss1)      damage += 5;   // 50  Destroyer
            if (NPC.downedMechBoss3)      damage += 5;   // 55  Prime
            if (NPC.downedPlantBoss)      damage += 10;  // 65  Plantera
            if (NPC.downedGolemBoss)      damage += 15;  // 80  Golem
            if (NPC.downedEmpressOfLight) damage += 5;   // 85
            if (NPC.downedFishron)        damage += 10;  // 95  Duke
            if (NPC.downedAncientCultist) damage += 15;  // 110
            if (NPC.downedMoonlord)       damage += 60;  // 170
 
            return damage;
        }

        public static int GetSegmentCount()
        {
            int segments = 1;
 
            if (NPC.downedSlimeKing)      segments++;
            if (NPC.downedBoss1)          segments++;
            if (NPC.downedBoss2)          segments++;
            if (NPC.downedQueenBee)       segments++;
            if (NPC.downedDeerclops)      segments++;
            if (NPC.downedBoss3)          segments++;
            if (Main.hardMode)            segments++;
            if (NPC.downedQueenSlime)     segments++;
            if (NPC.downedMechBoss2)      segments++;
            if (NPC.downedMechBoss1)      segments++;
            if (NPC.downedMechBoss3)      segments++;
            if (NPC.downedPlantBoss)      segments++;
            if (NPC.downedGolemBoss)      segments++;
            if (NPC.downedEmpressOfLight) segments++;
            if (NPC.downedFishron)        segments++;
            if (NPC.downedAncientCultist) segments++;
            if (NPC.downedMoonlord)       segments += 2;
 
            return segments;
        }
 
        public static bool HasTrail     => Main.hardMode;
        public static bool HasFrontShot => NPC.downedPlantBoss;
        public static int FrontShotCount => NPC.downedMoonlord ? 4 : 2;
    }
}