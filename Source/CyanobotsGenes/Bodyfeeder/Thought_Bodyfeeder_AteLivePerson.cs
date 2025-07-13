using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CyanobotsGenes
{
    class Thought_Bodyfeeder_AteLivePerson : Thought_Memory
    {
        private static List<ThoughtDef> removeByPawn = new List<ThoughtDef>();
        private static List<ThoughtDef> removeByTime = new List<ThoughtDef>();

        public virtual List<ThoughtDef> RemoveByPawn => removeByPawn;
        public virtual List<ThoughtDef> RemoveByTime => removeByTime;

        public override bool TryMergeWithExistingMemory(out bool showBubble)
        {
            //Log.Message("Thought_Bodyfeeder_AteFriend.TryMergeWithExistingMemory() - pawn: " + pawn + ", needs: " + pawn?.needs
            //   + ", mood: " + pawn?.needs?.mood +", thoughts: " + pawn?.needs?.mood?.thoughts 
            //    + ", memories: " + pawn?.needs?.mood?.thoughts?.memories + ", base: " + base.ToString());

            //remove other memories relating to the death
            //so as to have just one thought on the topic

            MemoryThoughtHandler memoryHandler = pawn.needs.mood.thoughts.memories;
            List<Thought_Memory> memories = memoryHandler.Memories.ListFullCopyOrNull();
            //Log.Message("memoryHandler: " + memoryHandler
            //    + ", memoryHandler.Memories: " + memoryHandler.Memories.ToStringSafeEnumerable()
            //    + ", memories: " + memories.ToStringSafeEnumerable());
            //Log.Message("removeByPawn: " + removeByPawn.ToStringSafeEnumerable());
            //Log.Message("removeByTime: " + removeByTime.ToStringSafeEnumerable());

            foreach (Thought_Memory memory in memories)
            {
                if (memory.otherPawn == otherPawn && RemoveByPawn.Contains(memory.def))
                {
                    memoryHandler.RemoveMemory(memory);
                    continue;
                }
                if (memory.age < 60 && RemoveByTime.Contains(memory.def))
                {
                    memoryHandler.RemoveMemory(memory);
                    continue;
                }
            }

            bool result = base.TryMergeWithExistingMemory(out showBubble);
            //Log.Message("result: " + result + ", showBubble: " + showBubble 
            //    + ", memoryHandler.Memories: " + memoryHandler.Memories.ToStringSafeEnumerable());
            return result;
        }
        
    }
}
