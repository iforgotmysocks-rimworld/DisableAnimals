using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DisableAnimals
{
    public class Main : Mod
    {
        public Main(ModContentPack content) : base(content)
        {
            GetSettings<ModSettings>();
        }

        public override void DoSettingsWindowContents(UnityEngine.Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            GetSettings<ModSettings>().DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        }
    }
}
