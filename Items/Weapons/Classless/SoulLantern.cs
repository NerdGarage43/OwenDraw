using Microsoft.Xna.Framework;
using Owendraw.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Owendraw.Items.Weapons.Classless
{
    public class SoulLantern : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Blue;
            Item.DamageType = DamageClass.Default;
            Item.damage = SoulLanternHelper.GetScaledDamage();
            Item.knockBack = 0f;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<SoulLanternSentry>();
            Item.shootSpeed = 0f;
        }

        public override void UpdateInventory(Player player)
        {
            Item.damage = SoulLanternHelper.GetScaledDamage();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == ModContent.ProjectileType<Projectiles.SoulLanternSentry>())
                {
                    p.Kill();
                    break;
                }
            }

            Vector2 spawnPos = player.Top - new Vector2(0f, 48f);
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero,
                ModContent.ProjectileType<Projectiles.SoulLanternSentry>(),
                damage, knockback, player.whoAmI);

            return false;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            bool wofDead = Main.hardMode;
            bool planteraDead = NPC.downedPlantBoss;
            bool mlDead = NPC.downedMoonlord;

            tooltips.Add(new TooltipLine(Mod, "SLDesc", "Summons a lantern above your head that fires homing souls"));

            if (wofDead)
                tooltips.Add(new TooltipLine(Mod, "SLExplosion", "Projectiles explode on enemy hit"));

            if (planteraDead && !mlDead)
                tooltips.Add(new TooltipLine(Mod, "SLExtra", "20% chance to fire 1-2 extra projectiles"));

            if (mlDead)
                tooltips.Add(new TooltipLine(Mod, "SLExtra", "50% chance to fire 1-2 extra projectiles"));
        }

        public static class SoulLanternHelper
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
        }
    }
}