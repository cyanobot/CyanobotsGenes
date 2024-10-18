using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace CyanobotsGenes
{
    public class GeneExtension_BlocksInteractions : DefModExtension
    {
        public bool whitelistInitiator = false;
        public bool whitelistRecipient = false;
        public List<InteractionDef> interactionDefsInitiator = new List<InteractionDef>();
        public List<InteractionDef> interactionDefsRecipient = new List<InteractionDef>();

        public bool BlocksAsInitiator(InteractionDef def)
        {
            return interactionDefsInitiator.Contains(def) ^ whitelistInitiator;
        }

        public bool BlocksAsRecipient(InteractionDef def)
        {
            return interactionDefsRecipient.Contains(def) ^ whitelistRecipient;
        }
    }
}
