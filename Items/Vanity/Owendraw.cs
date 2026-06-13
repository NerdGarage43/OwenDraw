using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Owendraw.Items.Vanity
{
    public class Owendraw : ModItem
    {
        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                EquipLoader.AddEquipTexture(Mod, "Owendraw/Items/Vanity/Owendraw_Head", EquipType.Head, this);
                EquipLoader.AddEquipTexture(Mod, "Owendraw/Items/Vanity/Owendraw_Body", EquipType.Body, this);
                EquipLoader.AddEquipTexture(Mod, "Owendraw/Items/Vanity/Owendraw_Legs", EquipType.Legs, this);
            }
        }

        public override void SetStaticDefaults()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                int head = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
                ArmorIDs.Head.Sets.DrawHead[head] = false;

                int body = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
                ArmorIDs.Body.Sets.HidesTopSkin[body] = true;
                ArmorIDs.Body.Sets.HidesArms[body] = true;

                int legs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
                ArmorIDs.Legs.Sets.HidesBottomSkin[legs] = true;
            }
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 20;
            Item.accessory = true;
            Item.value = Item.buyPrice(0, 0, 0, 0);
            Item.vanity = true;
        }

        public override void UpdateVanity(Player player)
        {
            player.GetModPlayer<OwendrawPlayer>().vanityEquipped = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (!hideVisual)
                player.GetModPlayer<OwendrawPlayer>().vanityEquipped = true;
            
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Silk, 10)
                .AddIngredient(ItemID.LifeCrystal)
                .Register();
        }
    }
}