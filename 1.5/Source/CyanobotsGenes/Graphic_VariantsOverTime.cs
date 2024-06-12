using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CyanobotsGenes
{
    public class Graphic_VariantsOverTime : Graphic
    {
        private Dictionary<Rot4,List<Graphic>> subGraphics = new Dictionary<Rot4, List<Graphic>> 
        {
            { Rot4.North, new List<Graphic>() },
            { Rot4.East, new List<Graphic>() },
            { Rot4.South, new List<Graphic>() },
            { Rot4.West, new List<Graphic>() }
        };
        private Dictionary<Rot4, List<string>> texPaths = new Dictionary<Rot4, List<string>>()
        {
            { Rot4.North, new List<string>() },
            { Rot4.East, new List<string>() },
            { Rot4.South, new List<string>() },
            { Rot4.West, new List<string>() }
        };

        public Shader shader;
        public List<ShaderParameter> shaderParameters;

        public int interval = 60;

        public Dictionary<Rot4, List<string>> TexPaths
        {
            get 
            { 
                return texPaths; 
            }
            set
            {
                texPaths = value;
                UpdateSubGraphics();
            }
        }

        public override void TryInsertIntoAtlas(TextureAtlasGroup groupKey)
        {
            foreach (Rot4 rot4 in subGraphics.Keys)
            {
                List<Graphic> rotGraphics = subGraphics[rot4];
                foreach (Graphic graphic in rotGraphics)
                {
                    graphic.TryInsertIntoAtlas(groupKey);
                }
            }
        }

        public override void Init(GraphicRequest req)
        {
            data = req.graphicData;
            path = req.path;
            maskPath = req.maskPath;
            color = req.color;
            colorTwo = req.colorTwo;
            drawSize = req.drawSize;
            shader = req.shader;
            shaderParameters = req.shaderParameters;

            UpdateSubGraphics();
        }

        public void UpdateSubGraphics()
        {
            foreach (Rot4 rot4 in texPaths.Keys)
            {
                subGraphics[rot4].Clear();
                List<string> rotTexPaths = texPaths[rot4];
                foreach(string texPath in rotTexPaths)
                {
                    Graphic newGraphic = GraphicDatabase.Get(typeof(Graphic_Single), texPath, shader, drawSize, color, colorTwo, data, shaderParameters);
                    subGraphics[rot4].Add(newGraphic);
                }
            }
        }

        public Graphic SubGraphicAt(Rot4 rot4, int index)
        {
            if (!Rot4.AllRotations.Contains(rot4)) return null;
            List<Graphic> graphics = subGraphics[rot4];
            if (index < 0 || index >= graphics.Count)
            {
                Log.Error("Tried to retrieve index: " + index + " from graphics list of length: " + graphics.Count);
                return null;
            }
            return subGraphics[rot4][index];
        }
    }
}
