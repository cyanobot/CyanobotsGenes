using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static CyanobotsGenes.CG_Mod;
using static CyanobotsGenes.OffspringUtility;

namespace CyanobotsGenes
{
#if RW_1_5
    [HarmonyPatch(typeof(PregnancyUtility), nameof(PregnancyUtility.ApplyBirthOutcome_NewTemp), new Type[]
#else
    [HarmonyPatch(typeof(PregnancyUtility), nameof(PregnancyUtility.ApplyBirthOutcome), new Type[]
#endif
            {
                typeof(RitualOutcomePossibility),
                typeof(float),
                typeof(Precept_Ritual),
                typeof(List<GeneDef>),
                typeof(Pawn),
                typeof(Thing),
                typeof(Pawn),
                typeof(Pawn),
                typeof(LordJob_Ritual),
                typeof(RitualRoleAssignments),
                typeof(bool)
            })]
    [HarmonyBefore(new string[] { "RedMattis.BetterPrerequisites" })]
    static public class BigAndSmall_MultipleBabies_Patch
    {
        static Type t_PregnancyPatches = bigAndSmallFrameworkLoaded ? AccessTools.TypeByName("BigAndSmall.PregnancyPatches") : null;
        static FieldInfo f_newBabyGenes = bigAndSmallFrameworkLoaded ? AccessTools.Field(t_PregnancyPatches, "newBabyGenes") : null;
        static FieldInfo f_disableBirthPatch = bigAndSmallFrameworkLoaded ? AccessTools.Field(t_PregnancyPatches, "disableBirthPatch") : null;

        public static bool Prepare()
        {
            return CG_Mod.bigAndSmallFrameworkLoaded;
        }

        public static void Prefix(ref List<GeneDef> genes, Pawn geneticMother, Pawn father)
        {
            bool disableBirthPatch = (bool)f_disableBirthPatch.GetValue(null);

            LogUtil.DebugLog("Fired BigAndSmall_MultipleBabies_Patch.Prefix - disableBirthPatch: " + disableBirthPatch);

            if (disableBirthPatch)
            {
                List<GeneDef> newGenes = PregnancyUtility.GetInheritedGenes(father, geneticMother);
                genes = newGenes;
                f_newBabyGenes.SetValue(null, null);
            }

        }
    }

    /*
    [HarmonyPatch(
            typeof(PawnGenerator),
            nameof(PawnGenerator.GeneratePawn),
            new Type[] { typeof(PawnGenerationRequest) })
            ]
    [HarmonyBefore(new string[] { "RedMattis.BetterPrerequisites" })]
    static public class BigAndSmall_MultipleBabies_Patch
    {

        static Type t_PregnancyPatches = bigAndSmallFrameworkLoaded ? AccessTools.TypeByName("BigAndSmall.PregnancyPatches") : null;
        static FieldInfo f_newBabyGenes = bigAndSmallFrameworkLoaded ? AccessTools.Field(t_PregnancyPatches, "newBabyGenes") : null;

        public static bool Prepare()
        {
            return CG_Mod.bigAndSmallFrameworkLoaded;
        }



    }
    */
}
