using RimWorld;
using HarmonyLib;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(GrowthUtility),nameof(GrowthUtility.IsGrowthBirthday))]
    public static class IsGrowthBirthday_Patch
    {
        public static bool Postfix(bool result, int age)
        {
            //Log.Message("IsGrowthBirthday_Patch, age: " + age);
            if (age == 3) return true;
            return result;
        }
    }
}
