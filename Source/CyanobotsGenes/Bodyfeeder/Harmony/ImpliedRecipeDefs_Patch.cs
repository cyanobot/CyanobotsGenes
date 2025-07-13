using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyanobotsGenes
{

    //[HarmonyPatch(typeof(RecipeDefGenerator),nameof(RecipeDefGenerator.ImpliedRecipeDefs))]
    class ImpliedRecipeDefs_Patch
    {
        public static void Postfix(ref IEnumerable<RecipeDef> __result, bool hotReload)
        {
            //Log.Message("Postfix ImpliedRecipeDefs");
            __result = __result.Concat(GenerateMeatAdministerDefs.MeatAdministerDefs(hotReload));
        }
    }
}
