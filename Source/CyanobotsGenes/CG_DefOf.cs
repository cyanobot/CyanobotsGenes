using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CyanobotsGenes
{
    [DefOf]
    static class CG_DefOf
    {
        public static GeneDef Asocial;
        public static GeneDef Carnivore;
        public static GeneDef Hypercarnivore;
        public static GeneDef Herbivore;
        public static GeneDef StrictHerbivore;
        public static GeneDef EasilyBored;
        public static GeneDef RarelyBored;
        public static GeneDef NeverBored;
        public static GeneDef LightFur;
        public static GeneDef Tabby;
        public static GeneDef PackBond;
        public static GeneDef PreyDrive;
        public static GeneDef Psychopathic;
        public static GeneDef Stealthy;

        public static HediffDef DietaryIndigestion;

        public static StatDef JoyFallRateFactor;
        public static StatDef VegetableNutritionFactor;
        public static StatDef AnimalNutritionFactor;

        public static ThoughtDef WetFur;
        public static ThoughtDef AtePlantCarnivore;
        public static ThoughtDef AteAnimalProductHerbivore;
        public static ThoughtDef AteMeatHerbivore;
        public static ThoughtDef AteCorpseHypercarnivore;

        public static TraitDef Unaffected;

        public static XenotypeDef Biodrone;
        public static XenotypeDef Kitlin;
    }
}
