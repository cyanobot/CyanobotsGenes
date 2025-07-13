using Verse;
using UnityEngine;
using System.Threading.Tasks;

namespace CyanobotsGenes
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatching
    {
        static HarmonyPatching()
        {
            CG_Mod.harmony.PatchAll();
        }
    }

}
