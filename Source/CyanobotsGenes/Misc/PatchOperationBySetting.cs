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
using System.Xml;
using static CyanobotsGenes.CG_Mod;

namespace CyanobotsGenes
{
    [StaticConstructorOnStartup]
    public static class PatchUtil
    {
        public static Dictionary<FieldInfo,List<PatchOperation>> patchesBySetting = new Dictionary<FieldInfo, List<PatchOperation>>();
        public static CG_Settings settings;

        static PatchUtil()
        {
            settings = LoadedModManager.GetMod<CG_Mod>().GetSettings<CG_Settings>();
        }
    }

    public class PatchOperationBySetting : PatchOperationPathed
    {
        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (string.IsNullOrEmpty(this.setting)) return false;

            //Log.Message("Calling PatchOperationBySetting ApplyWorker");

            object val = null;
            bool flag = true;
            FieldInfo f_setting = null;
            try
            {
                f_setting = PatchUtil.settings.GetType().GetField(setting, BindingFlags.Static | BindingFlags.Public);
                flag = (bool)f_setting.GetValue(PatchUtil.settings);
            }
            catch (Exception e)
            {
                if (e is NullReferenceException)
                {
                    Log.Warning("XML worker attempted to read setting " + setting + " but couldn't find any such setting.");
                }
                else if (e is InvalidCastException)
                {
                    Log.Warning("Only boolean settings can be used with PatchOperationBySetting. XML worker attempted to read setting " + setting + " but found type " + val.GetType().Name);
                }
                else
                {
                    Log.Warning("Exception caught while trying to apply PatchOperationBySetting with setting " + setting + ": " + e.Message);
                }
                return false;
            }

            if (f_setting == null) return false;

            if (!PatchUtil.patchesBySetting.ContainsKey(f_setting)) PatchUtil.patchesBySetting.Add(f_setting, new List<PatchOperation>());

            try
            {
                patchDict[setting].Add(new PatchWorker(setting, xml, this.on, this.off));
                //Log.Message("Try successful : " + setting);
                //Log.Message("list length " + CG_Mod.patchDict[setting].Count());
            }
            catch (KeyNotFoundException e)
            {
                //Log.Message("KeyNotFoundException caught: " + setting);
                patchDict.Add(setting, new List<PatchWorker>());
                patchDict[setting].Add(new PatchWorker(setting, xml, this.on, this.off));
            }
            
            if (flag)
            {
                if (this.on == null) return false;
                else return this.on.Apply(xml);
            }
            else
            {
                if (this.off == null) return false;
                else return this.off.Apply(xml);
            }
        }

        public string setting;
        public PatchOperation on;
        public PatchOperation off;
    }

    class PatchWorker
    {
        public PatchWorker(string setting, XmlDocument xml, PatchOperation patchOn, PatchOperation patchOff)
        {
            this.setting = setting;
            this.xml = xml;
            this.patchOn = patchOn;
            this.patchOff = patchOff;
        }

        public string setting;
        public XmlDocument xml;
        public PatchOperation patchOn;
        public PatchOperation patchOff;
    }

}
