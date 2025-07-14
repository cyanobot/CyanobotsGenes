using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace CyanobotsGenes
{

    public class PawnRenderNode_VariantsOverTime : PawnRenderNode_OffsetByBodyType
    {
        public new PawnRenderNodeProperties_VariantsOverTime Props => (PawnRenderNodeProperties_VariantsOverTime)props;
        public int Interval 
        {
            get
            {
                int interval = Props.baseInterval;
                List<IntervalModifier> modifiers = Props.intervalModifiers;
                //Log.Message("baseInterval: " + Props.baseInterval + ", modifiers: " + modifiers.ToStringSafeEnumerable());
                if (modifiers.NullOrEmpty()) return interval;
                foreach (IntervalModifier modifier in modifiers)
                {
                    interval = Mathf.FloorToInt(interval * modifier.ModifierFor(pawn));
                }
                return interval;
            }
        }
        public bool NorthSouthMirror => Props.northSouthMirror;
        public Pawn pawn;
        public List<string> variantPathsDefault;
        public Dictionary<Rot4, List<string>> variantPaths = new Dictionary<Rot4, List<string>>
        {
            { Rot4.North, new List<string>() },
            { Rot4.East, new List<string>() },
            { Rot4.South, new List<string>() },
            { Rot4.West, new List<string>() },
        };
        //public List<string> variantPathsNorth;
        //public List<string> variantPathsEast;
        //public List<string> variantPathsSouth;
        //public List<string> variantPathsWest;

        public bool cachedFlip = false;
        public Dictionary<Rot4, int> cachedVariants = new Dictionary<Rot4, int>
        {
            { Rot4.North, 0 },
            { Rot4.East, 0 },
            { Rot4.South, 0 },
            { Rot4.West, 0 }
        };
        public int lastCachedTick = 0;

        public PawnRenderNode_VariantsOverTime(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
            this.pawn = pawn;

            variantPathsDefault = new List<string>() { Props.texPath };
            variantPaths[Rot4.North] = Props.variantPathsNorth;
            variantPaths[Rot4.East] = Props.variantPathsEast;
            variantPaths[Rot4.South] = Props.variantPathsSouth;
            variantPaths[Rot4.West] = Props.variantPathsWest;

            if (variantPaths[Rot4.North].NullOrEmpty()) variantPaths[Rot4.North] = variantPathsDefault;
            if (variantPaths[Rot4.East].NullOrEmpty())
            {
                if (!variantPaths[Rot4.West].NullOrEmpty()) variantPaths[Rot4.East] = variantPaths[Rot4.West];
                else variantPaths[Rot4.East] = variantPathsDefault;
            }
            if (variantPaths[Rot4.South].NullOrEmpty()) variantPaths[Rot4.South] = variantPathsDefault;
            if (variantPaths[Rot4.West].NullOrEmpty())
            {
                variantPaths[Rot4.West] = variantPaths[Rot4.East];
            }
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            string texPath = TexPathFor(pawn);
            if (texPath.NullOrEmpty())
            {
                return null;
            }
            Shader shader = ShaderFor(pawn);
            if (shader == null)
            {
                return null;
            }
            Graphic_VariantsOverTime graphic = (Graphic_VariantsOverTime)GraphicDatabase.Get<Graphic_VariantsOverTime>(texPath, shader, Vector2.one, ColorFor(pawn));
            graphic.interval = Interval;
            Dictionary<Rot4, List<string>> dict = new Dictionary<Rot4, List<string>>
            {
                { Rot4.North, variantPaths[Rot4.North] },
                { Rot4.East, variantPaths[Rot4.East] },
                { Rot4.South, variantPaths[Rot4.South] },
                { Rot4.West, variantPaths[Rot4.West] }
            };
            graphic.TexPaths = dict;

            return graphic;
        }

        public override Mesh GetMesh(PawnDrawParms parms)
        {
            //Log.Message("GetMesh");
            Mesh mesh = base.GetMesh(parms);
            if (cachedFlip && !parms.facing.IsHorizontal)
            {
#if RW_1_5
                mesh = MeshPool.GridPlaneFlip(MeshPool.SizeOf(mesh));
#else
                mesh = MeshPool.GridPlaneFlip(mesh);
#endif
            }
            return mesh;
        }

        public bool CanWiggle()
        {
            if (pawn.Dead || pawn.Suspended) return false;
            return true;
        }

        public int VariantFor(Rot4 rot4)
        {
            if (!CanWiggle()) return 0;
            UpdateCacheIfNeeded();
            return cachedVariants[rot4];
        }

        public void UpdateCacheIfNeeded()
        {
            if (!CanWiggle()) return;
            if (Find.TickManager.TicksGame > lastCachedTick + Interval)
            {
                RecalculateVariants();
                requestRecache = true;
                lastCachedTick = Find.TickManager.TicksGame;
            }
        }

        public void RecalculateVariants()
        {
            //int number = Math.Abs(pawn.HashOffsetTicks()) / Interval;
            foreach (Rot4 rot4 in variantPaths.Keys)
            {
                int count = variantPaths[rot4].Count;
                //int variant = number % count;

                int variant = 0;
                if (count > 1)
                {
                    List<int> options = Enumerable.Range(0, count).ToList();
                    options.Remove(cachedVariants[rot4]);
                    variant = options.RandomElement();
                }
                //Log.Message("rot4: " + rot4 + ", variant: " + variant);

                cachedVariants[rot4] = variant;
            }
            if (NorthSouthMirror) cachedFlip = Rand.Bool;
            //Log.Message("cachedFlip: " + cachedFlip);
        }
    }
}
