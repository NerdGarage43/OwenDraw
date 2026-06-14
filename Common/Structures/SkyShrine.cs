using StructureHelper.API;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace Owendraw.Common.Structures
{
    public class SkyShrineWorldGen : ModSystem
    {
        public static bool SkyShrinePlaced = false;

        public override void SaveWorldData(TagCompound tag)
        {
            tag["skyShrinePlaced"] = SkyShrinePlaced;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            SkyShrinePlaced = tag.GetBool("skyShrinePlaced");
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int index = tasks.FindIndex(t => t.Name == "Settle Liquids Again");
            if (index != -1)
                tasks.Insert(index + 1, new SkyShrineGenPass("Sky Shrine", 1f));
        }
    }

    public class SkyShrineGenPass : GenPass
    {
        public SkyShrineGenPass(string name, float weight) : base(name, weight) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration config)
        {
            if (SkyShrineWorldGen.SkyShrinePlaced)
                return;

            progress.Message = "Placing sky shrine";

            Point16 dims = Generator.GetStructureDimensions("Common/Structures/SkyShrine", ModContent.GetInstance<Owendraw>());

            int skyTop = 50;
            int skyBottom = (int)Main.worldSurface - 80;

            for (int attempts = 0; attempts < 10000; attempts++)
            {
                int x = WorldGen.genRand.Next(Main.maxTilesX / 10, Main.maxTilesX - Main.maxTilesX / 10 - dims.X);
                int y = WorldGen.genRand.Next(skyTop, skyBottom);

                Point16 pos = new Point16(x, y);

                if (!Generator.IsInBounds("Common/Structures/SkyShrine", ModContent.GetInstance<Owendraw>(), pos))
                    continue;

                Generator.GenerateStructure("Common/Structures/SkyShrine", pos, ModContent.GetInstance<Owendraw>());
                SkyShrineWorldGen.SkyShrinePlaced = true;
                return;
            }
        }
    }
}