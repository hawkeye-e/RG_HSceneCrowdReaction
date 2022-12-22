﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.IL2CPP;


namespace HSceneCrowdReaction
{
    [BepInPlugin(GUID, PluginName, Version)]
    public class HSceneCrowdReactionPlugin : BasePlugin
    {
        public const string PluginName = "HSceneCrowdReaction";
        public const string GUID = "hawk.RG.HSceneCrowdReaction";
        public const string Version = "0.1";

        internal static new ManualLogSource Log;

        public override void Load()
        {
            Log = base.Log;

            HSceneCrowdReaction.Config.Init(this);

            if (HSceneCrowdReaction.Config.Enabled)
            {
                Harmony.CreateAndPatchAll(typeof(HSceneScreen.Hook), GUID);
            }

            StateManager.Instance = new StateManager();
        }


    }
}