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
        public static GeneDef Bodyfeeder;
        public static GeneDef Carnivore;
        public static GeneDef Hypercarnivore;
        public static GeneDef Herbivore;
        public static GeneDef StrictHerbivore;
        public static GeneDef ObligateCannibal;
        public static GeneDef EasilyBored;
        public static GeneDef RarelyBored;
        public static GeneDef NeverBored;
        public static GeneDef LightFur;
        public static GeneDef Tabby;
        public static GeneDef PackBond;
        public static GeneDef PreyDrive;
        public static GeneDef Psychopathic;
        public static GeneDef Stealthy;

        public static HediffDef BodyfeederStarvation;
        public static HediffDef DietaryIndigestion;

        public static HistoryEventDef BodyfeederAteEnemy;
        public static HistoryEventDef BodyfeederAteOutsider;
        public static HistoryEventDef BodyfeederAteColonist;
        public static HistoryEventDef BodyfeederAteBodyPartEnemy;
        public static HistoryEventDef BodyfeederAteBodyPartOutsider;
        public static HistoryEventDef BodyfeederAteBodyPartColonist;

        public static JobDef IngestDowned;
        public static JobDef IngestForHemogen;

        public static StatDef JoyFallRateFactor;
        public static StatDef VegetableNutritionFactor;
        public static StatDef AnimalNutritionFactor;

        public static TaleDef TaleBodyfeederAtePerson;
        public static TaleDef TaleBodyfeederAteBodyPart;

        public static ThoughtDef WetFur;
        public static ThoughtDef AtePlantCarnivore;
        public static ThoughtDef AteAnimalProductHerbivore;
        public static ThoughtDef AteMeatHerbivore;
        public static ThoughtDef AteCorpseHypercarnivore;

        public static ThoughtDef BodyfeederVictim_BodyPartEaten_Opinion;
        public static ThoughtDef BodyfeederVictim_BodyPartEaten_Opinion_CRequiredStrong;
        public static ThoughtDef BodyfeederVictim_BodyPartEaten_Mood;
        public static ThoughtDef BodyfeederVictim_BodyPartEaten_Mood_CRequiredStrong;
        public static ThoughtDef Bodyfeeder_AteLivePerson;
        public static ThoughtDef Bodyfeeder_AteLivePerson_CAcceptable;
        public static ThoughtDef Bodyfeeder_AteLivePerson_CRequired;
        public static ThoughtDef Bodyfeeder_AteBodyPart;
        public static ThoughtDef Bodyfeeder_AteBodyPart_CAcceptable;
        public static ThoughtDef Bodyfeeder_AteBodyPart_CRequired;
        public static ThoughtDef Bodyfeeder_AteFriend;
        public static ThoughtDef Bodyfeeder_AteBodyPartFriend;

        public static ThoughtDef Bodyfeeder_Know_EnemyEaten;
        public static ThoughtDef Bodyfeeder_Know_OutsiderEaten;
        public static ThoughtDef Bodyfeeder_Know_ColonistEaten;
        public static ThoughtDef Bodyfeeder_Know_EnemyEaten_CAcceptable;
        public static ThoughtDef Bodyfeeder_Know_OutsiderEaten_CAcceptable;
        public static ThoughtDef Bodyfeeder_Know_ColonistEaten_CAcceptable;
        public static ThoughtDef Bodyfeeder_Know_EnemyEaten_CRequired;
        public static ThoughtDef Bodyfeeder_Know_OutsiderEaten_CRequired;
        public static ThoughtDef Bodyfeeder_Know_ColonistEaten_CRequired;
        public static ThoughtDef Bodyfeeder_Know_EnemyBodyPartEaten;
        public static ThoughtDef Bodyfeeder_Know_OutsiderBodyPartEaten;
        public static ThoughtDef Bodyfeeder_Know_ColonistBodyPartEaten;
        public static ThoughtDef Bodyfeeder_Know_EnemyBodyPartEaten_CAcceptable;
        public static ThoughtDef Bodyfeeder_Know_OutsiderBodyPartEaten_CAcceptable;
        public static ThoughtDef Bodyfeeder_Know_ColonistBodyPartEaten_CAcceptable;
        public static ThoughtDef Bodyfeeder_Know_EnemyBodyPartEaten_CRequired;
        public static ThoughtDef Bodyfeeder_Know_OutsiderBodyPartEaten_CRequired;
        public static ThoughtDef Bodyfeeder_Know_ColonistBodyPartEaten_CRequired;

        public static TraitDef Unaffected;

        public static XenotypeDef Biodrone;
        public static XenotypeDef Kitlin;
    }
}
