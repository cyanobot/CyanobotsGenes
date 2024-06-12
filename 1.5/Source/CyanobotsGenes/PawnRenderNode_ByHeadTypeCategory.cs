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
    public enum HeadTypeCategory
    {
        Undefined,
        Male,
        Female,
        MaleNarrow,
        FemaleNarrow,
        HeavyJaw
    }

    public class PawnRenderNode_ByHeadTypeCategory : PawnRenderNode
    {
        public new PawnRenderNodeProperties_ByHeadTypeCategory Props => (PawnRenderNodeProperties_ByHeadTypeCategory)props;

        public Dictionary<Rot4, List<HeadTypeCategoryData>> categoryData = new Dictionary<Rot4, List<HeadTypeCategoryData>>()
        {
            { Rot4.North, null },
            { Rot4.East, null },
            { Rot4.South, null },
            { Rot4.West, null }
        };

        public PawnRenderNode_ByHeadTypeCategory(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
            categoryData[Rot4.North] = Props.headTypeCategoryDataNorth;
            categoryData[Rot4.East] = Props.headTypeCategoryDataEast;
            categoryData[Rot4.South] = Props.headTypeCategoryDataSouth;
            categoryData[Rot4.West] = Props.headTypeCategoryDataWest;

            
            if (categoryData[Rot4.North].NullOrEmpty())
            {
                if (!categoryData[Rot4.South].NullOrEmpty()) categoryData[Rot4.North] = categoryData[Rot4.South];
            }
            if (categoryData[Rot4.East].NullOrEmpty())
            {
                if (!categoryData[Rot4.West].NullOrEmpty()) categoryData[Rot4.East] = categoryData[Rot4.West];
            }
            if (categoryData[Rot4.West].NullOrEmpty())
            {
                if (!categoryData[Rot4.East].NullOrEmpty())
                {
                    categoryData[Rot4.West] = new List<HeadTypeCategoryData>();
                    foreach (HeadTypeCategoryData data in categoryData[Rot4.East])
                    {
                        categoryData[Rot4.West].Add(data.Flip());
                    }
                }
            }
            /*
            foreach (Rot4 rot4 in Rot4.AllRotations)
            {
                Log.Message("Node loaded categorydata[" + rot4 + "]: " + categoryData[rot4].ToStringSafeEnumerable());
            }
            */
        }

        public HeadTypeCategoryData DataFor(HeadTypeCategory cat, Rot4 rot4)
        {
            //Log.Message("DataFor - cat: " + cat + ", rot4: " + rot4 + ", AllRotations.Contains: " + Rot4.AllRotations.Contains(rot4));
            if (cat == HeadTypeCategory.Undefined || !Rot4.AllRotations.Contains(rot4)) return null;
            List<HeadTypeCategoryData> dataList = categoryData[rot4];
            //Log.Message("dataList: " + dataList.ToStringSafeEnumerable());
            HeadTypeCategoryData data = dataList.Find(x => x.category == cat);
            //Log.Message("data: " + data);
            return data;
        }
    }
}
