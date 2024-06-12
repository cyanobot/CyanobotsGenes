using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace CyanobotsGenes
{
    public class Gene_Precocious : Gene
    {
        public LifeStageAge lsa0;
        public LifeStageAge lsaChild;
        public int indexChild = -1;

        public LifeStageAge Lsa0
        {
            get
            {
                if (lsa0 == null)
                {
                    lsa0 = pawn.RaceProps.lifeStageAges[0];
                }
                return lsa0;
            }
        }
        public LifeStageAge LsaChild
        {
            get
            {
                if (lsaChild == null)
                {
                    lsaChild = pawn.RaceProps.lifeStageAges.Find(lsa => lsa.def == LifeStageDefOf.HumanlikeChild);
                }
                return lsaChild;
            }
        }
        public int IndexChild
        {
            get 
            {
                if (indexChild == -1)
                {
                    indexChild = pawn.RaceProps.lifeStageAges.IndexOf(LsaChild);
                }
                return indexChild;
            }
        }

        public bool CurrentlyPrecocious => pawn.ageTracker.AgeBiologicalYears < LsaChild.minAge;

        public void ApplyPrecociousEffects()
        {
            //skip if we've already done
            if (pawn.ageTracker.lockedLifeStageIndex == IndexChild) return;

            //BodyTypeDef bodyType = pawn.story.bodyType;
            pawn.ageTracker.LockCurrentLifeStageIndex(IndexChild);
            //pawn.story.bodyType = bodyType;
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        public void RemovePrecociousEffects()
        {
            if (pawn.ageTracker.lockedLifeStageIndex == IndexChild)
            {
                pawn.ageTracker.LockCurrentLifeStageIndex(-1);
                //pawn.story.bodyType = PawnGenerator.GetBodyTypeFor(pawn);
                pawn.Drawer.renderer.SetAllGraphicsDirty();
            }
        }

        public override void PostAdd()
        {
            base.PostAdd();
            if (Active && CurrentlyPrecocious)
            {
                ApplyPrecociousEffects();
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (pawn.IsHashIntervalTick(60))
            {
                if (Active && CurrentlyPrecocious) ApplyPrecociousEffects();
                else RemovePrecociousEffects();
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            RemovePrecociousEffects();
        }
    }
}
