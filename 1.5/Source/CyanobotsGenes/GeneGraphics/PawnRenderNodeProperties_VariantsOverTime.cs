using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CyanobotsGenes
{
    public class IntervalModifier
    {
        public float factor;
        public virtual float ModifierFor(Pawn pawn) { return 1f; }

        public override string ToString()
        {
            return this.GetType() + " [" + factor + "]";
        }
    }
    public class IntervalModifier_Sleep : IntervalModifier
    {
        public override float ModifierFor(Pawn pawn)
        {
            if (pawn.Awake()) return 1f;
            else return factor;
        }
    }
    public class IntervalModifier_Hediff : IntervalModifier
    {
        public HediffDef hediff;
        public override float ModifierFor(Pawn pawn)
        {
            if (pawn.health?.hediffSet?.HasHediff(hediff) ?? false) return factor;
            return 1f;
        }
        public override string ToString()
        {
            return this.GetType() + " [" + hediff + ", " + factor + "]";
        }
    }

    public class PawnRenderNodeProperties_VariantsOverTime : PawnRenderNodeProperties_OffsetByBodyType
    {
        public List<string> variantPathsNorth;
        public List<string> variantPathsEast;
        public List<string> variantPathsSouth;
        public List<string> variantPathsWest;
        public int baseInterval;
        public bool northSouthMirror = true;
        public List<IntervalModifier> intervalModifiers;

        public PawnRenderNodeProperties_VariantsOverTime()
        {
            workerClass = typeof(PawnRenderNodeWorker_VariantsOverTime);
            nodeClass = typeof(PawnRenderNode_VariantsOverTime);
            subworkerClasses = new List<Type>() { typeof(PawnRenderSubWorker_OffsetByBodyType) };
        }
    }
}
