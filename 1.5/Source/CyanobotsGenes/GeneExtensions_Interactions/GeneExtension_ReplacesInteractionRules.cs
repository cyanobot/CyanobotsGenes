using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace CyanobotsGenes
{
    public enum InteractionRole
    {
        Recipient,
        Initiator,
        Either
    }

    public class InteractionReplacement
    {
        public InteractionRole role;
        public InteractionDef original;
        public InteractionDef replacement;
    }

    class GeneExtension_ReplacesInteractionRules : DefModExtension
    {
        public List<InteractionReplacement> interactionReplacements;

        private List<InteractionDef> cachedAffectedInteractions;
        public List<InteractionDef> AffectedInteractions
        {
            get
            {
                if (cachedAffectedInteractions == null)
                {
                    cachedAffectedInteractions = interactionReplacements
                        .Select<InteractionReplacement, InteractionDef>(x => x.original).ToList();
                    cachedAffectedInteractions.RemoveDuplicates();
                }
                return cachedAffectedInteractions;
            }
        }

        public InteractionDef ReplacementFor(InteractionDef def, bool isInitiator)
        {
            InteractionReplacement interactionReplacement = interactionReplacements.Find(
                x => x.original == def
                && (x.role == InteractionRole.Either
                || isInitiator ? x.role == InteractionRole.Initiator : x.role == InteractionRole.Recipient));

            if (interactionReplacement == null) return def;
            else return interactionReplacement.replacement;
        }
    }
}
