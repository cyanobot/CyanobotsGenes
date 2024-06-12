using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace CyanobotsGenes
{
    public class HeadTypeCategoryData
    {
        public HeadTypeCategory category;
        public Vector3 offset = Vector3.zero;
        public Vector3 scale = Vector3.one;

        public override string ToString()
        {
            return category + " [offset: (" 
                + offset.x + ","  + offset.y + "," + offset.z
                + ", scale: (" 
                + scale.x + "," + scale.y + "," + scale.z 
                + ")]";
        }

        public HeadTypeCategoryData Flip()
        {
            HeadTypeCategoryData newData = new HeadTypeCategoryData {
                category = this.category,
                offset = new Vector3(-1f * this.offset.x, this.offset.y, this.offset.z),
                scale = this.scale
            };
            return newData;
        }
    }

    public class PawnRenderNodeProperties_ByHeadTypeCategory : PawnRenderNodeProperties
    {
        public List<HeadTypeCategoryData> headTypeCategoryDataEast = new List<HeadTypeCategoryData>();
        public List<HeadTypeCategoryData> headTypeCategoryDataWest = new List<HeadTypeCategoryData>();
        public List<HeadTypeCategoryData> headTypeCategoryDataNorth = new List<HeadTypeCategoryData>();
        public List<HeadTypeCategoryData> headTypeCategoryDataSouth = new List<HeadTypeCategoryData>();

        public PawnRenderNodeProperties_ByHeadTypeCategory()
        {
            workerClass = typeof(PawnRenderNodeWorker_ByHeadTypeCategory);
            nodeClass = typeof(PawnRenderNode_ByHeadTypeCategory);
        }
    }
}
