using RimWorld;
using RimWorld.QuestGen;
using Verse;
using HarmonyLib;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(QuestNode_Root_RefugeePodCrash_Baby),nameof(QuestNode_Root_RefugeePodCrash_Baby.SendLetter))]
    public static class RefugeePodCrash_Baby_SendLetter_Patch
    {
        public static bool Prefix(Quest quest, Pawn pawn)
        {
            if (pawn.HasActiveGene(CG_DefOf.CYB_Precocious))
            {
                //TaggedString title = "PRECOCIOUS";
                TaggedString title = "LetterLabelRefugeePodCrash".Translate();
                TaggedString letterText = "CYB_LetterText_RefugeePodCrashPrecociousBaby".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);

                if (QuestGen.slate.Get("hasParent", defaultValue: false))
                {
                    letterText += "\n\n" + "RefugeePodCrashBabyHasParent".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
                }
                QuestNode_Root_WandererJoin_WalkIn.AppendCharityInfoToLetter("JoinerCharityInfo".Translate(pawn), ref letterText);
                PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref letterText, ref title, pawn);
                Find.LetterStack.ReceiveLetter(title, letterText, LetterDefOf.NeutralEvent, new TargetInfo(pawn));

                return false;
            }
            return true;
        }
    }
}
