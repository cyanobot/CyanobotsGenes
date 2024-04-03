using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using LudeonTK;
using UnityEngine;

namespace CyanobotsGenes
{
    class OffsetByBodyType : DefModExtension
    {
        OffsetByBodyType_Rot offsetsSouth;
        OffsetByBodyType_Rot offsetsNorth;
        OffsetByBodyType_Rot offsetsEast;
        OffsetByBodyType_Rot offsetsWest;

        public OffsetByBodyType_Rot OffsetByBodyTypeForRot(Rot4 rot)
        {
            if (rot == Rot4.South)
                return offsetsSouth;
            if (rot == Rot4.North)
                return offsetsNorth;
            if (rot == Rot4.East)
                return offsetsEast;
            if (rot == Rot4.West)
                return offsetsWest;

            return null;
        }

        public Vector3 Offset(BodyTypeDef bodyType, Rot4 rot)
        {
            OffsetByBodyType_Rot offsetByBodyType_Rot = OffsetByBodyTypeForRot(rot);
            if (offsetByBodyType_Rot == null) return Vector3.zero;
            return OffsetByBodyTypeForRot(rot).OffsetForBodyType(bodyType);
        }

        public Vector3 Offset(Pawn pawn)
        {
            BodyTypeDef bodyType = pawn.story?.bodyType;
            Rot4 rot = pawn.Rotation;
            return Offset(bodyType, rot);
        }
    }

    class OffsetByBodyType_Rot
    {
        Dictionary<BodyTypeDef, Vector3> bodyTypes;

        public Vector3 OffsetForBodyType(BodyTypeDef bodyType)
        {
            if (bodyTypes.NullOrEmpty() || !bodyTypes.ContainsKey(bodyType)) return Vector3.zero;
            return bodyTypes[bodyType];
        }
    }
}
