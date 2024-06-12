using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(Pawn_AbilityTracker),nameof(Pawn_AbilityTracker.Notify_TemporaryAbilitiesChanged))]
    public static class Notify_TemporaryAbilitiesChanged_Patch
    {
        public static void Postfix(Pawn ___pawn)
        {
            List<Ability_ImplantXenotype> implanterAbilities = ___pawn.abilities.AllAbilitiesForReading
                .Where(x => x.GetType() == typeof(Ability_ImplantXenotype))
                .Select< Ability, Ability_ImplantXenotype>(a => (Ability_ImplantXenotype)a)
                .ToList();

            if (implanterAbilities.NullOrEmpty()) return;
            foreach (Ability_ImplantXenotype ability in implanterAbilities)
            {
                ability.xenotype = null;
            }
        }
    }
}
