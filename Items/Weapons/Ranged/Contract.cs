using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Owendraw.Projectiles;
using Terraria.ModLoader.IO;

namespace Owendraw.Items.Weapons.Ranged
{
    public class Contract : ModItem
    {
        public const float SPREAD_RADIANS = MathHelper.Pi / 36f;

        private static int GetScaledDamage()
        {
            int damage = 8;

            if (NPC.downedSlimeKing)       damage += 2;   // 10
            if (NPC.downedBoss1)           damage += 1;   // 11  EoC
            if (NPC.downedBoss2)           damage += 4;   // 15  Evils (EoW/BoC)
            if (NPC.downedQueenBee)        damage += 5;   // 20
            if (NPC.downedDeerclops)       damage += 5;   // 25
            if (NPC.downedBoss3)           damage += 5;   // 30  Skeletron
            if (Main.hardMode)             damage += 5;   // 35  WoF
            if (NPC.downedQueenSlime)      damage += 5;   // 40
            if (NPC.downedMechBoss2)       damage += 5;   // 45  Twins
            if (NPC.downedMechBoss1)       damage += 5;   // 50  Destroyer
            if (NPC.downedMechBoss3)       damage += 5;   // 55  Prime
            if (NPC.downedPlantBoss)       damage += 10;  // 65  Plantera
            if (NPC.downedGolemBoss)       damage += 5;   // 70
            if (NPC.downedEmpressOfLight)  damage += 5;   // 75
            if (NPC.downedFishron)         damage += 5;   // 80  Duke Fishron
            if (NPC.downedAncientCultist)  damage += 30;  // 110
            if (NPC.downedMoonlord)        damage += 265; // 375

            return damage;
        }

        public override void SetDefaults()
        {
            Item.damage = 8;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 30;
            Item.height = 54;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item5;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 7f;
            Item.useAmmo = AmmoID.Arrow;
            Item.autoReuse = true;
        }

        public override void UpdateInventory(Player player)
        {
            Item.damage = GetScaledDamage();
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            bool hardmode = Main.hardMode;
            bool eocDead = NPC.downedBoss1;
            bool planteraDead = NPC.downedPlantBoss;
            bool cultistDead = NPC.downedAncientCultist;
            bool mlDead = NPC.downedMoonlord;

            var player = Main.LocalPlayer;
            var mp = player.GetModPlayer<ContractPlayer>();
            int mode = mp.ContractMode;

            string modeStr = mode == 0 ? "[Mode 1: Pierce & Home]" : "[Mode 2: Slash]";
            tooltips.Add(new TooltipLine(Mod, "ContractMode", modeStr));
            tooltips.Add(new TooltipLine(Mod, "ContractSwitch", "Right-click to switch modes"));
            tooltips.Add(new TooltipLine(Mod, "ContractHeal", "1% chance to heal 3 HP on hit"));

            if (mlDead)
                tooltips.Add(new TooltipLine(Mod, "ContractPen", "+7 armor penetration on all attacks"));

            if (!hardmode)
                tooltips.Add(new TooltipLine(Mod, "ContractLock", "[Mode 2 unlocks in Hardmode]") { OverrideColor = Color.Gray });
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                if (!Main.hardMode) return false;

                var mp = player.GetModPlayer<ContractPlayer>();
                mp.ContractMode = mp.ContractMode == 0 ? 1 : 0;

                SoundEngine.PlaySound(SoundID.Item4, player.Center);
                return false;
            }
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var mp = player.GetModPlayer<ContractPlayer>();
            int mode = mp.ContractMode;

            if (mode == 1 && !Main.hardMode) mode = 0;

            int arrowCount = GetArrowCount(mode);
            bool mlDead = NPC.downedMoonlord;
            int bonusPen = mlDead ? 7 : 0;

            if (arrowCount == 1)
            {
                SpawnArrow(source, position, velocity, type, damage, knockback, mode, bonusPen);
            }
            else
            {
                float baseAngle = velocity.ToRotation();
                float totalSpread = (arrowCount - 1) * SPREAD_RADIANS;
                float startAngle = baseAngle - totalSpread / 2f;

                for (int i = 0; i < arrowCount; i++)
                {
                    float angle = startAngle + i * SPREAD_RADIANS;
                    Vector2 spreadVel = angle.ToRotationVector2() * velocity.Length();
                    SpawnArrow(source, position, spreadVel, type, damage, knockback, mode, bonusPen);
                }
            }

            return false;
        }

        private void SpawnArrow(EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
            int ammoType, int damage, float knockback, int mode, int bonusPen)
        {
            bool cultistDead = NPC.downedAncientCultist;
            bool mlDead = NPC.downedMoonlord;

            if (mode == 0)
            {
                int pierce = mlDead ? 5 : 3;
                bool screenWideHome = mlDead;

                var proj = Projectile.NewProjectileDirect(source, position, velocity,
                    ModContent.ProjectileType<ContractArrowMode1>(), damage, knockback, Main.myPlayer);

                if (proj.ModProjectile is ContractArrowMode1 mp1)
                {
                    mp1.PierceLeft = pierce;
                    mp1.ScreenWideHoming = screenWideHome;
                    mp1.BonusPen = bonusPen;
                    mp1.AmmoType = ammoType;
                }
            }
            else
            {
                bool homing = cultistDead || mlDead;

                var proj = Projectile.NewProjectileDirect(source, position, velocity,
                    ModContent.ProjectileType<ContractArrowMode2>(), damage, knockback, Main.myPlayer);

                if (proj.ModProjectile is ContractArrowMode2 mp2)
                {
                    mp2.Homing = homing;
                    mp2.BonusPen = bonusPen;
                    mp2.AmmoType = ammoType;
                }
            }
        }

        private int GetArrowCount(int mode)
        {
            bool eocDead = NPC.downedBoss1;
            bool planteraDead = NPC.downedPlantBoss;
            bool cultistDead = NPC.downedAncientCultist;
            bool mlDead = NPC.downedMoonlord;

            if (mode == 0)
            {
                int count = 1;
                if (eocDead) count++;
                if (planteraDead) count++;
                if (mlDead) count += 2;
                return count;
            }
            else
            {
                int count = 1;
                if (planteraDead) count++;
                if (cultistDead) count++;
                return count;
            }
        }
    }

    public class ContractPlayer : ModPlayer
    {
        public int ContractMode = 0;

        public override void SaveData(TagCompound tag)
        {
            tag["ContractMode"] = ContractMode;
        }

        public override void LoadData(TagCompound tag)
        {
            ContractMode = tag.GetInt("ContractMode");
            if (ContractMode == 1 && !Main.hardMode)
                ContractMode = 0;
        }

        public override void ResetEffects()
        {
            if (ContractMode == 1 && !Main.hardMode)
                ContractMode = 0;
        }
    }
}