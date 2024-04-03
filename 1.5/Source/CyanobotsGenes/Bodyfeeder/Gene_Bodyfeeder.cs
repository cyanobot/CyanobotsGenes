using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CyanobotsGenes.BodyfeederUtility;

namespace CyanobotsGenes
{
    class Gene_Bodyfeeder : Gene
    {
        public override void Notify_IngestedThing(Thing thing, int numTaken)
        {
            //Log.Message("Fired Notify_IngestedThing - thing: " + thing + ", numTaken: " + numTaken);

            if (thing is Corpse) return;        //handling corpses in a separate method bc hemogen gain dependent on how much eaten

            if (IsStandardHemogenSource(thing)) return;     //standard hemogen sources (eg hemogen packs) are handled by Gene_Hemogenic, don't duplicate

            float nutrition = FoodUtility.NutritionForEater(pawn,thing) * numTaken;
            //Log.Message("numTaken: " + numTaken + ", nutrition: " + nutrition
            //    + ", hpn: " + HemogenPerNutrition(pawn, thing));
            float hemogen = nutrition * HemogenPerNutrition(pawn, thing) / BLOOD_FACTOR; //negating the 0.2 factor 
            
            if (hemogen != 0) GeneUtility.OffsetHemogen(pawn, hemogen);
        }

        public void Notify_IngestedCorpse(Corpse corpse, float nutritionEaten)
        {
            //Log.Message("Fired Notify_IngestedCorpse, nutritionEaten: " + nutritionEaten 
            //    + ", efficiency: " + HemogenPerNutrition(pawn, corpse));
            float hemogen = nutritionEaten * HemogenPerNutrition(pawn, corpse) / BLOOD_FACTOR; //negating the 0.2 factor 

            GeneUtility.OffsetHemogen(pawn, hemogen);
        }

        public void Notify_IngestedLivePawn(float nutritionEaten)
        {
            //Log.Message("Fired Notify_IngestedLivePawn");
            float hemogen = nutritionEaten * HemogenPerNutritionForLivePawn / BLOOD_FACTOR;

            GeneUtility.OffsetHemogen(pawn, hemogen);
        }
    }
}
