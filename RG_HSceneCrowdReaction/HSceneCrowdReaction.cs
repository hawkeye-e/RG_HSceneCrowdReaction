using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.IL2CPP;

namespace HSceneCrowdReaction
{
    [BepInProcess("RoomGirl")]
    [BepInPlugin(GUID, PluginName, Version)]
    public class HSceneCrowdReactionPlugin : BasePlugin
    {
        public const string PluginName = "HSceneCrowdReaction";
        public const string GUID = "hawk.RG.HSceneCrowdReaction";
        public const string Version = "0.2";

        internal static new ManualLogSource Log;

        public override void Load()
        {
            Log = base.Log;

            HSceneCrowdReaction.Config.Init(this);

            if (HSceneCrowdReaction.Config.Enabled)
            {
                InfoList.HAnimation.Init();
                InfoList.HVoice.Init();
                Harmony.CreateAndPatchAll(typeof(HSceneScreen.Hook), GUID);
            }

            StateManager.Instance = new StateManager();
        }


    }
}
