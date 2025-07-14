using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;
using System.Reflection;
using System.Collections;
using static CyanobotsGenes.CG_Mod;

namespace CyanobotsGenes
{
    class ThoughtWorker_LightSensitivity : ThoughtWorker
    {
        public enum LightLevel
        {
            Dark,
            Dim,
            Bright
        }

        public const float AnyLightThreshold = 0.1f;
        public const float BrightLightThreshold = 0.5f;

        public static Type t_WorldTileInfo;
        public static MethodInfo m_WorldTileInfo_Get;
        public static PropertyInfo p_BiomeVariants;

        public static List<string> lightlessBiomes = new List<string>
        {
            "BMT_CrystalCaverns",
            "BMT_EarthenDepths",
            "BMT_FungalForest"
        };
        public static List<string> dimBiomes = new List<string>
        {
            "AB_RockyCrags"
        };
        public static List<string> lightlessBiomeVariants = new List<string>
        {
            "BMT_DesertShallows",
            "BMT_GlacialHollows",
            "BMT_ShallowCave"
        };

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (PawnUtility.IsBiologicallyOrArtificiallyBlind(p)) return ThoughtState.Inactive;

            LightLevel lightLevel = LightLevelFor(p);
            if (lightLevel == LightLevel.Bright) return ThoughtState.ActiveAtStage(1);
            if (lightLevel == LightLevel.Dim) return ThoughtState.ActiveAtStage(0);
            return ThoughtState.Inactive;
        }

        public static LightLevel LightLevelFor(Pawn pawn)
        {
            if (pawn.Spawned || (pawn.CarriedBy?.Spawned ?? false))
            {
                return LightLevelAt(pawn.MapHeld, pawn.PositionHeld);
            }
            Caravan caravan = CaravanUtility.GetCaravan(pawn);
            if (caravan != null) return LightLevelAt(caravan.Tile);

            return LightLevel.Dark;
        }

        public static LightLevel LightLevelAt(Map map, IntVec3 pos)
        {
            float groundGlow = map.glowGrid.GroundGlowAt(pos);
            if (groundGlow < AnyLightThreshold) return LightLevel.Dark;
            if (DarklightUtility.IsDarklightAt(pos, map) || groundGlow < BrightLightThreshold) return LightLevel.Dim;
            return LightLevel.Bright;
        }

#if RW_1_5
        public static LightLevel LightLevelAt(int tile)
        {
            string biome = Find.WorldGrid[tile].biome.defName;
            if (lightlessBiomes.Contains(biome)) return LightLevel.Dark;
            if (IsLightlessBiomeVariant(tile)) return LightLevel.Dark;
            float sunGlow = GenCelestial.CelestialSunGlow(tile, Find.TickManager.TicksAbs);
            if (sunGlow >= BrightLightThreshold)
            {
                if (dimBiomes.Contains(biome)) return LightLevel.Dim;
                return LightLevel.Bright;
            }
            if (sunGlow >= AnyLightThreshold) return LightLevel.Dim;
            return LightLevel.Dark;
        }
        
        public static bool IsLightlessBiomeVariant(int tile)
        {
            if (!geologicalLandformsLoaded) return false;
            //Log.Message("geologicalLandformsLoaded. t_WorldTileInfo: " + t_WorldTileInfo
            //    + ", m_WorldTileInfo_Get: " + m_WorldTileInfo_Get
            //    + ", p_BiomeVariants: " + p_BiomeVariants);

            object worldTileInfo = m_WorldTileInfo_Get.Invoke(null, new object[] { tile, true });
            object obj_BiomeVariants = p_BiomeVariants.GetValue(worldTileInfo);

            //Log.Message("worldTileInfo: " + worldTileInfo + ", obj_BiomeVariants: " + obj_BiomeVariants);

            IEnumerable ienum_BiomeVariants = (IEnumerable)obj_BiomeVariants;
            //Log.Message("ienum_BiomeVariants: " + ienum_BiomeVariants + ", TSSE: " + ienum_BiomeVariants.ToStringSafeEnumerable());
            if (ienum_BiomeVariants == null) return false;
            List<Def> ls_BiomeVariants = new List<Def>();
            foreach (Def variant in ienum_BiomeVariants)
            {
                ls_BiomeVariants.Add(variant);
            }
            //Log.Message("ls_BiomeVariants: " + ls_BiomeVariants.ToStringSafeEnumerable());
            if (ls_BiomeVariants.Any(v => lightlessBiomeVariants.Contains(v.defName)))
            {
                return true;
            }

            return false;
        }
#else
        public static LightLevel LightLevelAt(PlanetTile planetTile)
        {
            PlanetLayer planetLayer = planetTile.Layer;
            //use this for eg cave layers

            Tile tile = planetTile.Tile;
            BiomeDef biome = tile.PrimaryBiome;

            if (biome.biomeMapConditions.Any(gc => typeof(GameCondition_NoSunlight).IsAssignableFrom(gc.conditionClass)))
            {
                return LightLevel.Dark;
            }

            string biomeName = biome.defName;
            if (lightlessBiomes.Contains(biomeName)) return LightLevel.Dark;

            if (IsLightlessBiomeVariant(planetTile)) return LightLevel.Dark;
            float sunGlow = GenCelestial.CelestialSunGlow(planetTile, Find.TickManager.TicksAbs);
            if (sunGlow >= BrightLightThreshold)
            {
                if (dimBiomes.Contains(biomeName)) return LightLevel.Dim;
                return LightLevel.Bright;
            }
            if (sunGlow >= AnyLightThreshold) return LightLevel.Dim;
            return LightLevel.Dark;
        }

        public static bool IsLightlessBiomeVariant(PlanetTile tile)
        {
            if (!geologicalLandformsLoaded) return false;
            //Log.Message("geologicalLandformsLoaded. t_WorldTileInfo: " + t_WorldTileInfo
            //    + ", m_WorldTileInfo_Get: " + m_WorldTileInfo_Get
            //    + ", p_BiomeVariants: " + p_BiomeVariants);

            object worldTileInfo = m_WorldTileInfo_Get.Invoke(null, new object[] { tile, true });
            object obj_BiomeVariants = p_BiomeVariants.GetValue(worldTileInfo);

            //Log.Message("worldTileInfo: " + worldTileInfo + ", obj_BiomeVariants: " + obj_BiomeVariants);

            IEnumerable ienum_BiomeVariants = (IEnumerable)obj_BiomeVariants;
            //Log.Message("ienum_BiomeVariants: " + ienum_BiomeVariants + ", TSSE: " + ienum_BiomeVariants.ToStringSafeEnumerable());
            if (ienum_BiomeVariants == null) return false;
            List<Def> ls_BiomeVariants = new List<Def>();
            foreach (Def variant in ienum_BiomeVariants)
            {
                ls_BiomeVariants.Add(variant);
            }
            //Log.Message("ls_BiomeVariants: " + ls_BiomeVariants.ToStringSafeEnumerable());
            if (ls_BiomeVariants.Any(v => lightlessBiomeVariants.Contains(v.defName)))
            {
                return true;
            }

            return false;
        }

#endif
    }
}
