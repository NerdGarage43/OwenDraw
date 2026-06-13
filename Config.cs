using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Owendraw
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [LabelKey("$Mods.Owendraw.Config.DisableDyes")]
        [DefaultValue(true)]
        public bool DisableDyes { get; set; }
    }
}