using BepInEx.Logging;
using HarmonyLib;
using RG.Scene;
using RG.Scene.Action.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnhollowerBaseLib;
using UnityEngine.UI;


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
            Patches.General.InitHScene(__instance);
            Patches.General.BackupActorStatus(__instance);
        }


        //Restore the look/neck target when scene end
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.OnDestroy))]
        private static void OnDestroyPre(HScene __instance)
        {
            Patches.HotKey.RestoreHReactionMaleStates();
            Patches.HAnim.RecoverAllClothesState(ActionScene.Instance);
            Patches.HAnim.RecoverActorBody(ActionScene.Instance);
            Patches.General.RestoreActorsStatus(__instance);
            Patches.General.DestroyTempObject();
            Patches.General.DestroyStateManagerList();
            Patches.General.RemoveStateManagerValue();
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
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.UpdateForce))]
        private static void UpdateForcePost(Chara.ChaControl __instance)
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

        //Initialize the h reaction animation group
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.Init))]
        private static void InitPost(HSceneSprite __instance)
        {
            Patches.HAnim.SetupAnimationGroups(StateManager.Instance.CurrentHSceneInstance);
        }



        //Alter the drop down list of character selection
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSpriteChaChoice), nameof(HSceneSpriteChaChoice.Init))]
        private static void InitPost(HSceneSpriteChaChoice __instance)
        {
            if (ActionScene.Instance != null && StateManager.Instance.HAnimationGroupsList != null)
            {
                Patches.MenuItems.AddMainHSceneToggleToState(StateManager.Instance.CurrentHSceneInstance);
                Patches.MenuItems.ExpandCharaChoiceArrayPerGroup(StateManager.Instance.HAnimationGroupsList.Count);
                Patches.MenuItems.AddAnimationGroupsToCharaChoiceToggles(StateManager.Instance.HAnimationGroupsList);

                //Resize the drop down list
                Patches.MenuItems.ResizeCharaChoiceDropDownViewport();
            }
        }

        //Alter the checking result as the original coding does not process the extended toggles
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ToggleGroup), nameof(ToggleGroup.AnyTogglesOn))]
        private static void AnyTogglesOnPost(ToggleGroup __instance, ref bool __result)
        {
            if (StateManager.Instance.CurrentHSceneInstance != null)
                if (__instance.GetInstanceID() == StateManager.Instance.CurrentHSceneInstance._sprite.CharaChoice.tglGroup.GetInstanceID())
                    if (!__result && StateManager.Instance.HSceneDropDownSelectedToggle != null)
                        __result = true;
        }

        //Handles the click action on the added toggles
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Toggle), nameof(Toggle.OnPointerClick))]
        private static void OnPointerClick(Toggle __instance)
        {
            Patches.MenuItems.HandleToggleClick(__instance);
        }

        //Handle drop down reset when switching category
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.OnClickCloth))]
        private static void OnClickCloth(int mode)
        {
            Patches.MenuItems.HandleClickClothMenuButton(mode);
        }

        //Prevent the drop down selection reset to female 1 upon clicking
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Toggle), nameof(Toggle.Set))]
        private static void SetPre(Toggle __instance, ref bool value)
        {
            Patches.MenuItems.SpoofToggleSetValue(__instance, ref value);
        }

        //Backup the clothes states when change clothes state button clicked
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HSceneSpriteClothCondition), nameof(HSceneSpriteClothCondition.OnClickCloth))]
        private static void OnClickClothPre(int _cloth)
        {
            if (ActionScene.Instance != null && StateManager.Instance.HSceneDropDownSelectedToggle != null)
                Patches.MenuItems.BackupAllCharacterClothesStateToStateMananger();

        }

        //Apply clothes states change
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSpriteClothCondition), nameof(HSceneSpriteClothCondition.OnClickCloth))]
        private static void OnClickClothPost(int _cloth)
        {
            if (ActionScene.Instance != null && StateManager.Instance.HSceneDropDownSelectedToggle != null)
            {
                Patches.MenuItems.RestoreAllCharacterClothesStateFromStateMananger();
                Patches.MenuItems.ChangeClothesState(_cloth);
            }
        }

        //Backup the clothes states when change clothes state button clicked
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HSceneSpriteClothCondition), nameof(HSceneSpriteClothCondition.OnClickAllCloth))]
        private static void OnClickAllClothPre()
        {
            if (ActionScene.Instance != null && StateManager.Instance.HSceneDropDownSelectedToggle != null)
                Patches.MenuItems.BackupAllCharacterClothesStateToStateMananger();
        }

        //Apply clothes states change
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSpriteClothCondition), nameof(HSceneSpriteClothCondition.OnClickAllCloth))]
        private static void OnClickAllClothPost(HSceneSpriteClothCondition __instance)
        {
            if (ActionScene.Instance != null && StateManager.Instance.HSceneDropDownSelectedToggle != null)
            {
                Patches.MenuItems.RestoreAllCharacterClothesStateFromStateMananger();
                Patches.MenuItems.ChangeAllClothesState();
            }
        }

        //Allow to change the state of males in H reaction
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.Update))]
        private static bool Update()
        {
            bool isHotKeyPressed = Patches.HotKey.CheckHotKeyPressed();

            return isHotKeyPressed;
        }

        //Backup the accessory slot of the main female character and apply change to the correct character
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HSceneSpriteAccessoryCondition), nameof(HSceneSpriteAccessoryCondition.OnClickAccessory))]
        private static void OnClickAccessoryPre(HSceneSpriteAccessoryCondition __instance, int _accessory, ref Dictionary<int, bool> __state)
        {
            Patches.MenuItems.HandleAccessorySlotClickPre(__instance, _accessory, ref __state);
        }

        //Restore the accessory slot of the main female character
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSpriteAccessoryCondition), nameof(HSceneSpriteAccessoryCondition.OnClickAccessory))]
        private static void OnClickAccessoryPost(HSceneSpriteAccessoryCondition __instance, int _accessory, Dictionary<int, bool> __state)
        {
            Patches.MenuItems.HandleAccessorySlotClickPost(_accessory, __state);
        }

        //Backup the accessory slot of the main female character and apply change to the correct character
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HSceneSpriteAccessoryCondition), nameof(HSceneSpriteAccessoryCondition.OnClickAllAccessory))]
        private static void OnClickAllAccessoryPre(HSceneSpriteAccessoryCondition __instance, ref Dictionary<int, Dictionary<int, bool>> __state)
        {
            Patches.MenuItems.HandleAllAccessoryClickPre(__instance, ref __state);
        }

        //Restore the accessory slot of the main female character
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSpriteAccessoryCondition), nameof(HSceneSpriteAccessoryCondition.OnClickAllAccessory))]
        private static void OnClickAllAccessoryPost(Dictionary<int, Dictionary<int, bool>> __state)
        {
            Patches.MenuItems.HandleAllAccessoryClickPost(__state);
        }

        //Spoof the accessory list in order to populate the correct accessory list of the selected characters
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HSceneSpriteAccessoryCondition), nameof(HSceneSpriteAccessoryCondition.SetAccessoryCharacter))]
        private static void SetAccessoryCharacterPre(HSceneSpriteAccessoryCondition __instance, ref Dictionary<int, Il2CppReferenceArray<Chara.ListInfoBase>> __state)
        {
            Patches.MenuItems.SpoofCharacterAccessoryList(ref __state);
        }

        //Restore the accessory list of the main character
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSpriteAccessoryCondition), nameof(HSceneSpriteAccessoryCondition.SetAccessoryCharacter))]
        private static void SetAccessoryCharacterPost(HSceneSpriteAccessoryCondition __instance, Dictionary<int, Il2CppReferenceArray<Chara.ListInfoBase>> __state)
        {
            Patches.MenuItems.RestoreCharacterAccessoryList(__state);
            Patches.MenuItems.UpdateAccessoryToggleState();
        }

        //Change to default outfit for the selected character
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HSceneSpriteCoordinatesCard), nameof(HSceneSpriteCoordinatesCard.ChangeDefCoode))]
        private static bool ChangeDefCoodePre(HSceneSpriteCoordinatesCard __instance)
        {
            bool result = Patches.MenuItems.ChangeToDefaultOutfit();
            return result;
        }

        //Spoof the main character sex so that correct cooridnate list is populated
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HSceneSpriteCoordinatesCard), nameof(HSceneSpriteCoordinatesCard.SetCoordinatesCharacter))]
        private static void SetCoordinatesCharacterPre(HSceneSpriteCoordinatesCard __instance, ref bool __state)
        {
            __state = Patches.MenuItems.SpoofMainCharacterSex(__instance);
        }

        //Recover the sex info
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSpriteCoordinatesCard), nameof(HSceneSpriteCoordinatesCard.SetCoordinatesCharacter))]
        private static void SetCoordinatesCharacterPost(HSceneSpriteCoordinatesCard __instance, bool __state)
        {
            if (__state)
                Patches.MenuItems.RecoverMainCharacterSex(__instance);
        }

        //Change the outfit of the selected character
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Button), nameof(Button.Press))]
        private static bool PressPre(Button __instance)
        {
            bool isContinue = Patches.MenuItems.HandleChangeOutfitClick(__instance);

            return isContinue;
        }

    }
}
