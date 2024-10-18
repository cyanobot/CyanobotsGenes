using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;
using UnityEngine;
using RimWorld;

namespace CyanobotsGenes
{
    public class BodyTypeOffset
    {
        public BodyTypeDef bodyType;
        public Vector3 offset;

        public override string ToString()
        {
            return bodyType + ": (" + offset.x + "," + offset.y + "," + offset.z + ")";
        }

        public BodyTypeOffset Flip()
        {
            BodyTypeOffset newOffset = new BodyTypeOffset
            {
                bodyType = this.bodyType,
                offset = new Vector3(-1f * this.offset.x, this.offset.y, this.offset.z)
            };
            return newOffset;
        }
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "bodyType", xmlRoot.Name);
            offset = ParseHelper.FromString<Vector3>(xmlRoot.FirstChild.Value);
        }
    }

    public class PawnRenderNodeProperties_OffsetByBodyType : PawnRenderNodeProperties
    {
        public List<BodyTypeOffset> bodyTypeOffsetsEast = new List<BodyTypeOffset>();
        public List<BodyTypeOffset> bodyTypeOffsetsWest = new List<BodyTypeOffset>();
        public List<BodyTypeOffset> bodyTypeOffsetsNorth = new List<BodyTypeOffset>();
        public List<BodyTypeOffset> bodyTypeOffsetsSouth = new List<BodyTypeOffset>();

        public PawnRenderNodeProperties_OffsetByBodyType()
        {
            nodeClass = typeof(PawnRenderNode_OffsetByBodyType);
            subworkerClasses = new List<Type>() { typeof(PawnRenderSubWorker_OffsetByBodyType) };
        }
    }
}
