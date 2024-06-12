using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace CyanobotsGenes
{
    public class AbilityTemplateDef : Def
    {

		public Type abilityClass = typeof(Ability);

		public Type gizmoClass = typeof(Command_Ability);

		public List<AbilityCompProperties> comps = new List<AbilityCompProperties>();

		public AbilityCategoryDef category;

		public int displayOrder;

		public List<StatModifier> statBases;

		public VerbProperties verbProperties;

		public KeyBindingDef hotKey;

		public JobDef jobDef;

		public ThingDef warmupMote;

		public EffecterDef warmupEffecter;

		public FleckDef emittedFleck;

		public int emissionInterval;

		public string warmupMoteSocialSymbol;

		public SoundDef warmupStartSound;

		public SoundDef warmupSound;

		public SoundDef warmupPreEndSound;

		public float warmupPreEndSoundSeconds;

		public Vector3 moteDrawOffset;

		public float moteOffsetAmountTowardsTarget;

		public bool canUseAoeToGetTargets = true;

		public bool useAverageTargetPositionForWarmupEffecter;

		public bool targetRequired = true;

		public bool targetWorldCell;

		public bool showGizmoOnWorldView;

		public bool aiCanUse;

		public bool ai_SearchAOEForTargets;

		public bool ai_IsOffensive = true;

		public bool ai_IsIncendiary = true;

		public bool groupAbility;

		public int level;

		public IntRange cooldownTicksRange;

		public bool cooldownPerCharge;

		public bool hasExternallyHandledCooldown;

		public int charges = -1;

		public AbilityGroupDef groupDef;

		public bool overrideGroupCooldown;

		public List<MemeDef> requiredMemes;

		public bool sendLetterOnCooldownComplete;

		public bool sendMessageOnCooldownComplete;

		public bool displayGizmoWhileUndrafted;

		public bool disableGizmoWhileUndrafted = true;

		public bool writeCombatLog;

		public bool stunTargetWhileCasting;

		public bool showPsycastEffects = true;

		public bool showCastingProgressBar;

		public float detectionChanceOverride = -1f;

		public float uiOrder;

		public bool waitForJobEnd;

		public bool showWhenDrafted = true;

		public bool showOnCharacterCard = true;

		public bool hostile = true;

		public bool casterMustBeCapableOfViolence = true;

		[MustTranslate]
		public string confirmationDialogText;

		[NoTranslate]
		public string iconPath;

	}
}
