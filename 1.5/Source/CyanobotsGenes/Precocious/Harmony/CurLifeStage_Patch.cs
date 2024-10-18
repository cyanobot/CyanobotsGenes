using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static CyanobotsGenes.PrecociousUtil;
using System.Reflection.Emit;

namespace CyanobotsGenes
{
    [HarmonyPatch]
    public static class CurLifeStage_Patch
    {

        public static FieldInfo f_pawn = AccessTools.Field(typeof(Pawn_AgeTracker), "pawn");

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(LifeStageUtility), nameof(LifeStageUtility.PlayNearestLifestageSound));
            yield return AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.BodySize));
            yield return AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.HealthScale));
            yield return AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.HumanlikeBodyWidthForPawn));
            yield return AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.HumanlikeHeadWidthForPawn));
            yield return AccessTools.Method(typeof(PawnRenderer), "GetDrawParms");
            yield return AccessTools.Method(typeof(PawnRenderNodeWorker_Eye), nameof(PawnRenderNodeWorker_Eye.ScaleFor));
            yield return AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn));
            yield return AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeBeardSetForPawn));
            yield return AccessTools.Method(typeof(PawnTweener), "TweenedPosRoot");
        }

        public static LifeStageDef GetLifeStageDef(Pawn_AgeTracker ageTracker)
        {
            Pawn pawn = (Pawn)f_pawn.GetValue(ageTracker);
            if (!IsPrecociousBaby(pawn, out Gene_Precocious gene_Precocious)) return ageTracker.CurLifeStage;
            //Log.Message("returning life stage: " + gene_Precocious.Lsa0.def);
            return gene_Precocious.Lsa0.def;
        }
        public static MethodInfo m_get_CurLifeStage = AccessTools.PropertyGetter(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.CurLifeStage));
        public static MethodInfo m_getLifeStageDef = AccessTools.Method(typeof(CurLifeStage_Patch), nameof(CurLifeStage_Patch.GetLifeStageDef));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list_instructions = instructions.ToList();
            int count = list_instructions.Count;
            for (int i = 0; i < count; i++)
            {
                CodeInstruction cur = list_instructions[i];

                //if current line gets CurLifeStage
                //call custom method instead
                if (cur.Calls(m_get_CurLifeStage))
                {
                    //call function
                    yield return new CodeInstruction(OpCodes.Call, m_getLifeStageDef);
                    continue;
                }

                yield return cur;
            }
        }
    }
}
