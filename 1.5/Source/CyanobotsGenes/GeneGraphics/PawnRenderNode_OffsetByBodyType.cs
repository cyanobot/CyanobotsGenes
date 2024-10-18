using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;

namespace CyanobotsGenes
{
    public class PawnRenderNode_OffsetByBodyType : PawnRenderNode
    {
        public new PawnRenderNodeProperties_OffsetByBodyType Props => (PawnRenderNodeProperties_OffsetByBodyType)props;

        public Dictionary<Rot4, List<BodyTypeOffset>> bodyTypeOffsets = new Dictionary<Rot4, List<BodyTypeOffset>>()
        {
            { Rot4.North, null },
            { Rot4.East, null },
            { Rot4.South, null },
            { Rot4.West, null }
        };

        public PawnRenderNode_OffsetByBodyType(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {

            bodyTypeOffsets[Rot4.North] = Props.bodyTypeOffsetsNorth;
            bodyTypeOffsets[Rot4.East] = Props.bodyTypeOffsetsEast;
            bodyTypeOffsets[Rot4.South] = Props.bodyTypeOffsetsSouth;
            bodyTypeOffsets[Rot4.West] = Props.bodyTypeOffsetsWest;


            if (bodyTypeOffsets[Rot4.North].NullOrEmpty())
            {
                if (!bodyTypeOffsets[Rot4.South].NullOrEmpty()) bodyTypeOffsets[Rot4.North] = bodyTypeOffsets[Rot4.South];
            }
            if (bodyTypeOffsets[Rot4.East].NullOrEmpty())
            {
                if (!bodyTypeOffsets[Rot4.West].NullOrEmpty()) bodyTypeOffsets[Rot4.East] = bodyTypeOffsets[Rot4.West];
            }
            if (bodyTypeOffsets[Rot4.West].NullOrEmpty())
            {
                if (!bodyTypeOffsets[Rot4.East].NullOrEmpty())
                {
                    bodyTypeOffsets[Rot4.West] = new List<BodyTypeOffset>();
                    foreach (BodyTypeOffset bodyTypeOffset in bodyTypeOffsets[Rot4.East])
                    {
                        bodyTypeOffsets[Rot4.West].Add(bodyTypeOffset.Flip());
                    }
                }
            }
            /*
            foreach (Rot4 rot4 in Rot4.AllRotations)
            {
                Log.Message("Node loaded bodyTypeOffsets[" + rot4 + "]: " + bodyTypeOffsets[rot4].ToStringSafeEnumerable());
            }
            */
        }

        public Vector3 OffsetFor(BodyTypeDef bodyType, Rot4 rot4)
        {
            if (bodyType == null || !Rot4.AllRotations.Contains(rot4)) return Vector3.zero;
            List<BodyTypeOffset> offsetList = bodyTypeOffsets[rot4];
            BodyTypeOffset bodyTypeOffset = offsetList.Find(x => x.bodyType == bodyType);
            if (bodyTypeOffset == null) return Vector3.zero;
            return bodyTypeOffset.offset;
        }
    }
}
