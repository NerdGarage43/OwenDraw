using Terraria;
using Terraria.ModLoader;

namespace Owendraw.Items.Vanity
{
    public class OwendrawPlayer : ModPlayer
    {
        public bool vanityEquipped;

        public override void ResetEffects()
        {
            vanityEquipped = false;
        }

        public override void FrameEffects()
        {
            if (vanityEquipped)
            {
                int head = EquipLoader.GetEquipSlot(Mod, "Owendraw", EquipType.Head);
                int body = EquipLoader.GetEquipSlot(Mod, "Owendraw", EquipType.Body);
                int legs = EquipLoader.GetEquipSlot(Mod, "Owendraw", EquipType.Legs);

                Player.head = head;
                Player.body = body;
                Player.legs = legs;

                if (ModContent.GetInstance<Config>().DisableDyes)
                {
                    Player.cHead = 0;
                    Player.cBody = 0;
                    Player.cLegs = 0;
                }
            }
        }
    }
}