using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
#if RW_1_5
namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    class FloatMenu_Patch_Diet
    {
        static void Postfix(ref List<FloatMenuOption> opts, Pawn pawn, Vector3 clickPos)
        {
            IntVec3 c = IntVec3.FromVector3(clickPos);
            foreach (Thing t in c.GetThingList(pawn.Map))
            {
                //not interested in non-foods or nonhuman pawns
                if (t.def.ingestible == null || !pawn.RaceProps.CanEverEat(t) || !t.IngestibleNow || !pawn.RaceProps.Humanlike || pawn.genes == null) continue;

                //not interested in anything this mod doesn't interfere with
                if (!GeneticDietUtility.DietForbids(t, pawn)) continue;

                //copied directly from source
                //this will allow us to identify the menu options related to consuming this object
                string text;
                if (t.def.ingestible.ingestCommandString.NullOrEmpty())
                {
                    text = "ConsumeThing".Translate(t.LabelShort, t);
                }
                else
                {
                    text = t.def.ingestible.ingestCommandString.Formatted(t.LabelShort);
                }

                //for hay we should replicate the vanilla behaviour of not showing a Consume option at all
                /*
                if (t.def == ThingDefOf.Hay)
                {
                    opts.RemoveAll(x => x.Label.Contains(text));
                }

                //for other foods, we should disable the float menu option and tell the player why
                else
                {
                */
                foreach (FloatMenuOption consume in opts.FindAll(x => x.Label.Contains(text)))
                {
                    //if it's already disabled, leave it alone
                    if (consume.Disabled) continue;
                    if (GeneticDietUtility.DietForbids(t, pawn))
                    {
                        consume.Label = text += " : " + "CYB_Inedible".Translate();
                        consume.Disabled = true;
                    }
                }
                //}

            }
        }
    }

}
#endif