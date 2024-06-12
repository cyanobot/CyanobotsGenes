using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;

//modeled off VRE Lycanthrope, thank you to the VE team

namespace CyanobotsGenes
{
    //[HarmonyPatch(typeof(GeneDefGenerator), "ImpliedGeneDefs")]
    public static class ImpliedGeneDefs_Patch
    {
        public static IEnumerable<GeneDef> Postfix(IEnumerable<GeneDef> results, bool hotReload)
        {
            //Log.Message("Calling ImpliedGeneDefs_Patch, original results: " + results.ToStringSafeEnumerable());
            //first yield all geneDefs produced by the main method
            foreach (GeneDef geneDef in results) yield return geneDef;

            //then add our new
            List<XenotypeDef> allXenotypes = DefDatabase<XenotypeDef>.AllDefsListForReading;
            List<XenotypeGeneTemplateDef> xenotypeTemplates = DefDatabase<XenotypeGeneTemplateDef>.AllDefsListForReading;

            foreach (XenotypeGeneTemplateDef template in xenotypeTemplates)
            {
                //Log.Message("Trying to read template: " + template.defName);
                if (template.geneClass == null || !typeof(Gene).IsAssignableFrom(template.geneClass))
                {
                    Log.Error("[Cyanobot's Genes] Attempted to create xenotype-linked genes from template: " + template.defName
                        + ", but geneClass (" + template.geneClass.FullName + ") does not inherit from type Gene.");
                    continue;
                }

                foreach (XenotypeDef xenotype in allXenotypes)
                {
                    //Log.Message("Trying to generate gene for xenotype: " + xenotype);
                    yield return NewGeneFromTemplate(template,xenotype, hotReload);
                }
            }

        }

        public static GeneDef NewGeneFromTemplate(XenotypeGeneTemplateDef template, XenotypeDef xenotype, bool hotReload)
        {
            GeneDef geneDef = new GeneDef
            {
                defName = template.defName + "_" + xenotype.defName,
                geneClass = template.geneClass,
                label = template.label.Formatted(xenotype.label),
                description = template.description.Formatted(xenotype.label),
                iconPath = xenotype.iconPath,
                selectionWeight = template.selectionWeight,
                biostatCpx = template.biostatCpx,
                biostatMet = template.biostatMet,
                biostatArc = template.biostatArc,
                displayCategory = template.displayCategory,
                displayOrderInCategory = template.displayOrderInCategory + xenotype.displayPriority,
                minAgeActive = template.minAgeActive,
                modContentPack = template.modContentPack,
                generated = true,
                modExtensions = new List<DefModExtension>
                {
                    new GeneExtension_Xenotype
                    {
                        xenotype = xenotype
                    }
                },
                descriptionHyperlinks = new List<DefHyperlink>
                {
                    new DefHyperlink
                    {
                        def = xenotype
                    }
                },
                exclusionTags = template.exclusionTags,
                abilities = template.abilities?.ToList() ?? new List<AbilityDef>()
            };
            if (!template.modExtensions.NullOrEmpty())
            {
                geneDef.modExtensions.AddRange(template.modExtensions);
            }
            if (!template.abilityTemplates.NullOrEmpty())
            {
                foreach (AbilityTemplateDef abilityTemplate in template.abilityTemplates)
                {
                    AbilityDef newAbility = NewAbilityFromTemplate(abilityTemplate, xenotype);
                    DefGenerator.AddImpliedDef(newAbility, hotReload);
                    geneDef.abilities.Add(newAbility);
                }
            }
            return geneDef;
        }

        public static AbilityDef NewAbilityFromTemplate(AbilityTemplateDef template, XenotypeDef xenotype)
        {
            AbilityDef abilityDef = new AbilityDef
            {
                defName = template.defName + "_" + xenotype.defName,
                label = template.label.Formatted(xenotype.label),
                description = template.description.Formatted(xenotype.label),
                abilityClass = template.abilityClass,
                gizmoClass = template.gizmoClass,
                comps = template.comps,
                category = template.category,
                displayOrder = template.displayOrder,
                statBases = template.statBases,
                verbProperties = template.verbProperties,
                hotKey = template.hotKey,
                jobDef = template.jobDef,
                warmupMote = template.warmupMote,
                warmupEffecter = template.warmupEffecter,
                emittedFleck = template.emittedFleck,
                emissionInterval = template.emissionInterval,
                warmupMoteSocialSymbol = template.warmupMoteSocialSymbol,
                warmupStartSound = template.warmupStartSound,
                warmupSound = template.warmupSound,
                warmupPreEndSound = template.warmupPreEndSound,
                warmupPreEndSoundSeconds = template.warmupPreEndSoundSeconds,
                moteDrawOffset = template.moteDrawOffset,
                moteOffsetAmountTowardsTarget = template.moteOffsetAmountTowardsTarget,
                canUseAoeToGetTargets = template.canUseAoeToGetTargets,
                useAverageTargetPositionForWarmupEffecter = template.useAverageTargetPositionForWarmupEffecter,
                targetRequired = template.targetRequired,
                targetWorldCell = template.targetWorldCell,
                showGizmoOnWorldView = template.showGizmoOnWorldView,
                aiCanUse = template.aiCanUse,
                ai_SearchAOEForTargets = template.ai_SearchAOEForTargets,
                ai_IsOffensive = template.ai_IsOffensive,
                ai_IsIncendiary = template.ai_IsIncendiary,
                groupAbility = template.groupAbility,
                level = template.level,
                cooldownTicksRange = template.cooldownTicksRange,
                cooldownPerCharge = template.cooldownPerCharge,
                hasExternallyHandledCooldown = template.hasExternallyHandledCooldown,
                charges = template.charges,
                groupDef = template.groupDef,
                overrideGroupCooldown = template.overrideGroupCooldown,
                requiredMemes = template.requiredMemes,
                sendLetterOnCooldownComplete = template.sendLetterOnCooldownComplete,
                sendMessageOnCooldownComplete = template.sendMessageOnCooldownComplete,
                displayGizmoWhileUndrafted = template.displayGizmoWhileUndrafted,
                disableGizmoWhileUndrafted = template.disableGizmoWhileUndrafted,
                writeCombatLog = template.writeCombatLog,
                stunTargetWhileCasting = template.stunTargetWhileCasting,
                showPsycastEffects = template.showPsycastEffects,
                showCastingProgressBar = template.showCastingProgressBar,
                detectionChanceOverride = template.detectionChanceOverride,
                uiOrder = template.uiOrder,
                waitForJobEnd = template.waitForJobEnd,
                showWhenDrafted = template.showWhenDrafted,
                showOnCharacterCard = template.showOnCharacterCard,
                hostile = template.hostile,
                casterMustBeCapableOfViolence = template.casterMustBeCapableOfViolence,
                confirmationDialogText = template.confirmationDialogText,
                iconPath = xenotype.iconPath,
                descriptionHyperlinks = new List<DefHyperlink>
                {
                    new DefHyperlink
                    {
                        def = xenotype
                    }
                },
                modContentPack = template.modContentPack,
                generated = true
            };
            return abilityDef;
        }
    }
}
