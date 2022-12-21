using BepInEx.Logging;
using HarmonyLib;
using RG.Scene;
using RG.Scene.Action.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HSceneCrowdReaction.HSceneScreen
{
    internal class Hook
    {
        private static ManualLogSource Log = HSceneCrowdReactionPlugin.Log;

        //Initialize when H scene start
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CharaInit))]
        private static void CharaInitPost(HScene __instance)
        {
            Patches.InitHScene(__instance);
        }

        //Restore the look/neck target when scene end
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.OnDestroy))]
        private static void OnDestroyPre(HScene __instance)
        {
            Patches.RestoreActorsLookingDirection(__instance);
            Patches.DestroyTempObject();
        }

        //Reset the looking direction when H point moved
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetMovePositionPoint))]
        private static void SetMovePositionPoint(HScene __instance, Transform trans, Vector3 offsetpos, Vector3 offsetrot, bool isWorld)
        {
            Patches.UpdateNonHActorsLookAt(__instance);
        }

        //Reset the looking direction when H point moved
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetPosition), new[] { typeof(Transform), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool) })]
        private static void SetPosition1(HScene __instance, Transform _trans, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart, bool _isWorld)
        {
            Patches.UpdateNonHActorsLookAt(__instance);
        }

        //Reset the looking direction when H point moved
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetPosition), new[] { typeof(Vector3), typeof(Quaternion), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool) })]
        private static void SetPosition2(HScene __instance, Vector3 pos, Quaternion rot, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart, bool isWorld)
        {
            Patches.UpdateNonHActorsLookAt(__instance);
        }

        //Change the animation of actors not involved in H
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartPointSelect))]
        private static void StartPointSelect(HScene __instance, int hpointLen, UnhollowerBaseLib.Il2CppReferenceArray<HPoint> hPoints, int checkCategory, HScene.AnimationListInfo info)
        {
            Patches.ChangeActorsAnimation(__instance);
        }

    }
}
