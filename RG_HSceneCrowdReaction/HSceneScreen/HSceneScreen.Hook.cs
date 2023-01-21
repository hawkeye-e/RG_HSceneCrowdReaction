using BepInEx.Logging;
using HarmonyLib;
using RG.Scene;
using RG.Scene.Action.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnhollowerBaseLib;


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
            StateManager.Instance.CurrentHSceneInstance = __instance;
            Patches.General.InitHScene(__instance);
            Patches.General.BackupCharacterLookInfo(__instance);
        }


        //Restore the look/neck target when scene end
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.OnDestroy))]
        private static void OnDestroyPre(HScene __instance)
        {
            Patches.HAnim.RecoverAllClothesState(ActionScene.Instance);
            Patches.HAnim.RecoverActorBody(ActionScene.Instance);
            Patches.General.RestoreActorsLookingDirection(__instance);
            Patches.General.DestroyTempObject();
            Patches.General.DestroyStateManagerList();
        }

        //Reset the looking direction when H point moved
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetMovePositionPoint))]
        private static void SetMovePositionPointPost(HScene __instance, Transform trans, Vector3 offsetpos, Vector3 offsetrot, bool isWorld)
        {
            Patches.General.UpdateNonHActorsLookAt(__instance);
        }

        //Reset the looking direction when H point moved
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetPosition), new[] { typeof(Transform), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool) })]
        private static void SetPosition1Post(HScene __instance, Transform _trans, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart, bool _isWorld)
        {
            Patches.General.UpdateNonHActorsLookAt(__instance);
        }

        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetPosition), new[] { typeof(Vector3), typeof(Quaternion), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool) })]
        private static void SetPosition2Post(HScene __instance, Vector3 pos, Quaternion rot, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart, bool isWorld)
        {
            //Change the animation of actors not involved in H
            Patches.General.ChangeActorsAnimation(__instance);
            //Reset the looking direction when H point moved
            Patches.General.UpdateNonHActorsLookAt(__instance);
        }

        ////Remove the clothes of the actors not involved in H in order to fix a body rendering issue
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartPointSelect))]
        private static void StartPointSelectPre(HScene __instance, int hpointLen, Il2CppReferenceArray<HPoint> hPoints, int checkCategory, HScene.AnimationListInfo info)
        {
            Patches.HAnim.RemoveAllClothesState(__instance);
        }

        //Change the animation of actors not involved in HHScene.StartPointSelectPre
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.LateUpdateForce))]
        private static void LateUpdateForcePost(Chara.ChaControl __instance)
        {
            Patches.HAnim.CheckUpdateHAnim(__instance);
            Patches.HAnim.ForceBlowJob(__instance);
            Patches.HAnim.HandleHAnimationCtrlsUpdate(__instance);
        }


        //Force showing the penis of the male characters if flag is set
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameObject), nameof(GameObject.SetActive))]
        private static void SetActivePost(GameObject __instance, bool value)
        {
            if (StateManager.Instance.CurrentHSceneInstance != null && StateManager.Instance.ForceActiveInstanceID != null)
            {
                if (StateManager.Instance.ForceActiveInstanceID.Contains(__instance.GetInstanceID()))
                    __instance.active = true;
            }
        }

        //Remove the HPoint occupied by groups
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HPointCtrl), nameof(HPointCtrl.SetMarker), new[] { typeof(Il2CppSystem.Collections.Generic.List<HPoint>), typeof(int), typeof(Il2CppSystem.Collections.Generic.List<HScene.AnimationListInfo>), typeof(bool) })]
        private static void SetMarkerPre(Il2CppSystem.Collections.Generic.List<HPoint> points)
        {
            Patches.HAnim.RemoveOccupiedHPoints(points);
        }

    }
}
