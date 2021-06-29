using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DisableAnimals
{
    class ModSettings : Verse.ModSettings
    {
        public static bool xmlOverride = false;
        public static List<string> disabledAnimalDefNames;
        private static List<ThingDef> animalDefs;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref xmlOverride, "xmlOverride", false);
            Scribe_Collections.Look(ref disabledAnimalDefNames, "disabledAnimalDefNames", LookMode.Value);
        }

        private Vector2 scrollPos;
        public void DoWindowContents(Rect rect)
        {
            var options = new Listing_Standard();
            options.Begin(rect);

            options.CheckboxLabeled("Override xml values", ref xmlOverride);
            options.Gap();

            var viewRect = new Rect(0f, 0f, rect.width - 150, animalDefs != null ? animalDefs.Count * (Text.LineHeight + 2f) + 100f : 1600f);
            options.BeginScrollView(rect, ref scrollPos, ref viewRect);

            if (animalDefs != null)
            {
                foreach (var def in animalDefs)
                {
                    var dis = disabledAnimalDefNames.Contains(def.defName);
                    options.CheckboxLabeled($"Disable {def.defName}", ref dis);
                    if (dis && !disabledAnimalDefNames.Contains(def.defName)) disabledAnimalDefNames.Add(def.defName);
                    if (!dis && disabledAnimalDefNames.Contains(def.defName)) disabledAnimalDefNames.Remove(def.defName);
                }
            }

            options.EndScrollView(ref viewRect);
            options.End();
        }

        public static void InitAndPatchAnimalDefs()
        {
            animalDefs = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x?.race?.Animal == true).ToList();
            if (disabledAnimalDefNames == null) disabledAnimalDefNames = new List<string>();
            
            if (!xmlOverride) return;
            
            foreach (var def in animalDefs)
            {
                if (!disabledAnimalDefNames.Contains(def.defName)) continue;
                if (def?.race?.wildBiomes != null) def.race.wildBiomes = new List<RimWorld.AnimalBiomeRecord>();
                if (def?.tradeTags != null) def.tradeTags = new List<string>();
            }

            foreach (var biomeDef in DefDatabase<BiomeDef>.AllDefsListForReading)
            {
                var animalListField = typeof(BiomeDef).GetField("wildAnimals", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(biomeDef) as List<BiomeAnimalRecord>;
                if (animalListField == null)
                {
                    Log.Message($"List for {biomeDef.defName} was null");
                    continue;
                }
                foreach (var entry in animalListField)
                {
                    if (!disabledAnimalDefNames.Contains(entry.animal.defName)) continue;
                    entry.commonality = 0;
                }
                typeof(BiomeDef).GetField("wildAnimals", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(biomeDef, animalListField);
            }
        }
    }

    [StaticConstructorOnStartup]
    class InitTentStartup { static InitTentStartup() => ModSettings.InitAndPatchAnimalDefs(); }
}
