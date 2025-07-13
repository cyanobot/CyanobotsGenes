using HarmonyLib;
using Verse;
using RimWorld;
using static CyanobotsGenes.OffspringUtility;
using static CyanobotsGenes.CG_Mod;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(PawnUtility), "TrySpawnHatchedOrBornPawn")]
    public static class TrySpawnHatchedOrBornPawn_Patch
    {
        public static void Postfix(bool __result, Pawn pawn)
        {
            LogUtil.OffspringLog("TrySpawnHatchedOrBornPawn_Patch - result: " + __result
                + ", pawn: " + pawn + ", pawn.genes: " + pawn.genes + ", xenotype: " + pawn.genes?.Xenotype
                + ", relations: " + pawn.relations);
            //only run if a pawn was successfully spawned
            if (__result)
            {
                //only run if that pawn has genes and a relationtracker
                //also only run if xenotype is null
                //because TryGetInheritedXenotype_Patch sets xenotype to null if it needs to be calculated later
                //therefore if xenotype has already been set we don't need to worry about it
                if (pawn.genes != null && pawn.relations != null && (pawn.genes.Xenotype == null || pawn.genes.Xenotype == XenotypeDefOf.Baseliner))
                {
                    List<Pawn> parents = new List<Pawn>();
                    pawn.relations.GetDirectRelations(PawnRelationDefOf.Parent, ref parents);

                    //if we can't find any parents, give up
                    if (parents.Empty()) return;

                    LogUtil.OffspringLog("parents: " + parents.ToStringSafeEnumerable());

                    //mother should always be first in the list, father second
                    Pawn mother = parents[0];
                    Pawn father = parents.Count > 1 ? parents[1] : null;

                    //check if the parents have offspring genes, if not we don't need to continue (pawn is probably an actual baseliner)
                    if (HasActiveOffspringGene(mother) || HasActiveOffspringGene(father))
                    {
                        //we should only end up here because there are multiple offspring options
                        //so we need to figure out the xenotype by inspecting the baby's genes
                        List<XenotypeDef> potentialXenotypes = GetPotentialOffspringXenotypes(mother, father);
                        LogUtil.OffspringLog("potentialXenotypes: " + potentialXenotypes.ToStringSafeEnumerable());

                        //zero options returned when a dominant partner overrides offspring gene
                        //or potentially other edge cases
                        //in either case return to vanilla logic
                        if (potentialXenotypes.NullOrEmpty()) return;

                        //double check we've got more than one before entering potentially expensive logic
                        if (potentialXenotypes.Count < 2) 
                        {
                            Log.Message("[Cyanobot's Genes] Warning: TrySpawnHatchedOrBornPawn_Patch tried to find multiple offspring xenotype options for mother ("
                                + mother + ") and father: (" + father + ") but found only " + potentialXenotypes.Count + " potential xenotypes.");
                            return;
                        }

                        List<GeneDef> babyGenes = pawn.genes.GenesListForReading.Select(g => g.def).ToList();
                        LogUtil.DebugLog("babyGenes: " + babyGenes.ToStringSafeEnumerable());
                        //double check we actually found some genes
                        if (babyGenes.NullOrEmpty())
                        {
                            Log.Message("[Cyanobot's Genes] Warning: TrySpawnHatchedOrBornPawn_Patch tried to analyse genes of pawn (" + pawn + ") but couldn't find any.");
                            return;
                        }

                        foreach (XenotypeDef xenotype in potentialXenotypes)
                        {
                            List<GeneDef> xenotypeGenes = xenotype.AllGenes;
                            List<GeneDef> babyGenes_working = babyGenes.ToList();
                            List<GeneDef> xenotypeGenes_working = xenotypeGenes.ToList();

                            foreach (GeneDef geneDef in xenotypeGenes)
                            {
                                if (babyGenes_working.Remove(geneDef)) xenotypeGenes_working.Remove(geneDef);
                            }

                            //also remove skin/hair endogenes from baby or it won't match
                            babyGenes_working.RemoveAll(
                                g =>
                                g.endogeneCategory == EndogeneCategory.Melanin
                                || g.endogeneCategory == EndogeneCategory.HairColor
                                );

                            LogUtil.DebugLog("xenotypeGenes_working (culled): " + xenotypeGenes_working.ToStringSafeEnumerable());
                            LogUtil.DebugLog("babyGenes_working (culled): " + babyGenes_working.ToStringSafeEnumerable());

                            if (babyGenes_working.Empty() && xenotypeGenes_working.Empty())
                            {
                                //successfully found the right xenotype
                                LogUtil.DebugLog("found xenotype: " + xenotype);
                                pawn.genes.SetXenotypeDirect(xenotype);
                                return;
                            }
                        }

                        //fall-through - we didn't find a matching xenotype
                        Log.Message("[Cyanobot's Genes] Warning: TrySpawnHatchedOrBornPawn_Patch tried to find the appropriate xenotype for pawn ("
                            + pawn + ") but couldn't find a matching option from potentialOffspringXenotypes: ("
                            + potentialXenotypes.ToStringSafeEnumerable() + "). Leaving " + pawn + " as baseliner.");
                    }
                }
            }
        }
    }
}
