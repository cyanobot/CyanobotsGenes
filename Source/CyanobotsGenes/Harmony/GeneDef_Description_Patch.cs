using RimWorld;
using Verse;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(GeneDef), "GetDescriptionFull")]
    public static class GeneDef_Description_Patch
    {
        public static string Postfix(string __result)
        {
            List<string> lines = __result.Split(new string[] { Environment.NewLine },StringSplitOptions.None).ToList();
            List<string> newLines = new List<string>();

            int thoughtCount = 0;
            string thoughtStart = "  - " + "Removes".Translate() + ": ";
            int insertionIndex = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                if (line.StartsWith(thoughtStart))
                {
                    thoughtCount++;
                    if (thoughtCount > 3)
                    {
                        continue;
                    }
                    else if (thoughtCount == 3)
                    {
                        insertionIndex = i+1;
                    }
                }
                newLines.Add(line);
            }

            if (thoughtCount > 3)
            {
                string newString = thoughtStart + (thoughtCount - 3) + " " + "MoreLower".Translate() + "...";
                newLines.Insert(insertionIndex, newString);
            }

            StringBuilder sb = new StringBuilder();
            foreach(string line in newLines)
            {
                sb.AppendLine(line);
            }

            return sb.ToString().TrimEndNewlines();
        }
    }

}
