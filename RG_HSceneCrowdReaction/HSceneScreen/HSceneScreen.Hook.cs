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
            Patches.HAnim.ResetHPointForGroup(ActionScene.Instance);
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

        //Abandoned as this will be handled by updating mob points
        ////Remove the HPoint occupied by groups
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HPointCtrl), nameof(HPointCtrl.SetMarker), new[] { typeof(Il2CppSystem.Collections.Generic.List<HPoint>), typeof(int), typeof(Il2CppSystem.Collections.Generic.List<HScene.AnimationListInfo>), typeof(bool) })]
        //private static void SetMarkerPre(Il2CppSystem.Collections.Generic.List<HPoint> points)
        //{
        //    Patches.HAnim.RemoveOccupiedHPoints(points);
        //}

        //Initialize the h reaction animation group
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.Init))]
        private static void HSceneSpriteInitPost(HSceneSprite __instance)
        {
            Patches.HAnim.SetupAnimationGroups(StateManager.Instance.CurrentHSceneInstance);
            Patches.MenuItems.InitGroupSelectionControl(__instance);

            if (StateManager.Instance.CurrentHSceneInstance != null)
                StateManager.Instance.MainSceneHPoint = StateManager.Instance.CurrentHSceneInstance.CtrlFlag.NowHPoint;
        }



        //Alter the drop down list of character selection
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSpriteChaChoice), nameof(HSceneSpriteChaChoice.Init))]
        private static void HSceneSpriteChaChoiceInitPost(HSceneSpriteChaChoice __instance)
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
            Patches.MenuItems.ShowGroupSelectionCanvas(false);
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

        //Set the state when user click move button
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.OnClickMoveBt))]
        private static void OnClickMoveBtPre()
        {
            Patches.MenuItems.PrepareHPointChange();
        }

        //Update the visibility of the correct group when move button clicked
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.OnClickMoveBt))]
        private static void OnClickMoveBtPost()
        {
            Patches.MenuItems.PrepareHPointChange2();
        }

        //Show the group selection canvas whenever HPoint location marker is shown
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HPointCtrl), nameof(HPointCtrl.SetMarker), new[] { typeof(int) })]
        private static void SetMarkerPre2(int kind)
        {
            Patches.MenuItems.ShowGroupSelectionCanvas(true);
        }

        //Hide the group selection canvas and reset the visibility whenever HPoint location marker is removed
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPointCtrl), nameof(HPointCtrl.DelMarker), new System.Type[] { })]
        private static void DelMarkerPost()
        {
            Patches.MenuItems.FinishMoveForGroup();
            Patches.MenuItems.ShowGroupSelectionCanvas(false);
        }

        //Spoof the motion count for the selected group if it is setting HPoint marker
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPointCtrl), nameof(HPointCtrl.CheckPointLimitMotion))]
        private static void CheckPointLimitMotionPost(int place, HPoint hPoint, List<HScene.AnimationListInfo> changeInfos, bool SetMarker, ref int __result)
        {
            __result = Patches.MenuItems.GetSpoofMotionCount(place, SetMarker, __result);
        }

        //Move the selected group
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetMovePositionPoint))]
        private static bool SetMovePositionPointPre(HScene __instance, Transform trans, Vector3 offsetpos, Vector3 offsetrot, bool isWorld)
        {
            bool isContinue = Patches.MenuItems.HandleSetPosition();
            return isContinue;
        }

        //Move the selected group
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetPosition), new[] { typeof(Transform), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool) })]
        private static bool SetPosition1Pre(HScene __instance, Transform _trans, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart, bool _isWorld)
        {
            bool isContinue = Patches.MenuItems.HandleSetPosition();
            return isContinue;
        }

        //Move the selected group
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetPosition), new[] { typeof(Vector3), typeof(Quaternion), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool) })]
        private static bool SetPosition2Pre(HScene __instance, Vector3 pos, Quaternion rot, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart, bool isWorld)
        {
            bool isContinue = Patches.MenuItems.HandleSetPosition();
            return isContinue;
        }

        //Set the states when user click a HPoint
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HPointCtrl), nameof(HPointCtrl.HitPlace))]
        private static void HitPlacePre(ref HPoint hpoint)
        {
            hpoint = Patches.MenuItems.HandleHPointClick(hpoint);
        }

        //Change the HPoint value to prevent incorrect motion changed for the main scene group
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HPointCtrl), nameof(HPointCtrl.ChangeMotion))]
        private static void ChangeMotionPre(int place, ref HPoint hPoint)
        {
            if (ActionScene.Instance != null && StateManager.Instance.GroupSelection != null && StateManager.Instance.GroupSelection.SelectedGroup != null)
                hPoint = StateManager.Instance.MainSceneHPoint;
        }

        //Move the selected group
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeAnimation))]
        private static bool ChangeAnimationPre(HScene.AnimationListInfo _info, bool _isForceResetCamera, bool _isForceLoopAction = false, bool _UseFade = true, bool isLoadFirst = false)
        {
            bool isContinue = Patches.MenuItems.HandleSetPosition();
            isContinue = isContinue && Patches.MenuItems.HandleSetAnimation(_info);
            return isContinue;
        }

        //Move the selected group
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeAnimPosAndCamera))]
        private static bool ChangeAnimPosAndCameraPre(HScene.AnimationListInfo _info, bool _isForceResetCamera, bool isLoadFirst)
        {
            bool isContinue = Patches.MenuItems.HandleSetPosition();
            isContinue = isContinue && Patches.MenuItems.HandleSetAnimation(_info);
            return isContinue;
        }

        //Rollback the HPoint value to prevent incorrect item loaded/removed for the main group
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HItemCtrl), nameof(HItemCtrl.LoadItem), new[] { typeof(int), typeof(int), typeof(GameObject), typeof(GameObject), typeof(GameObject), typeof(GameObject), typeof(int), typeof(GameObject) })]
        private static void LoadItemPre(int _mode, int _id, GameObject _boneMale, GameObject _boneFemale, GameObject _boneMale1, GameObject _boneFemale1, int basho, GameObject _boneMale2)
        {
            if (ActionScene.Instance != null && StateManager.Instance.MainSceneHPoint != null
                && StateManager.Instance.GroupSelection != null && StateManager.Instance.GroupSelection.SelectedGroup != null
                && StateManager.Instance.CurrentHSceneInstance.CtrlFlag.IsPointMoving)
                StateManager.Instance.CurrentHSceneInstance.CtrlFlag.NowHPoint = StateManager.Instance.MainSceneHPoint;
        }

        //Spoof the main group info to display the correct icons and motion list
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.OnClickMotion))]
        private static void OnClickMotionPre(int _motion)
        {
            if (StateManager.Instance.GroupSelection != null)
                Patches.MenuItems.UpdateSexPositionIconVisibility(StateManager.Instance.GroupSelection.SelectedGroup);
            Patches.MenuItems.SpoofGroupInfoForMotionClick();
        }

        //Recover the main group info and update the UI
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.OnClickMotion))]
        private static void OnClickMotionPost(int _motion)
        {
            Patches.MenuItems.RestoreGroupInfoForMotionClick();

            if (ActionScene.Instance != null)
            {
                Patches.MenuItems.HighlightSelectedMotion();
                StateManager.Instance.MotionChangeSelectedCategory = _motion;
            }
        }

        //Spoof the main group info to display the correct icons and motion list
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.OnClickMotionFemale))]
        private static void OnClickMotionFemalePre()
        {
            if (StateManager.Instance.GroupSelection != null)
                Patches.MenuItems.UpdateSexPositionIconVisibility(StateManager.Instance.GroupSelection.SelectedGroup);
            Patches.MenuItems.SpoofGroupInfoForMotionClick();
        }

        //Recover the main group info and update the UI
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.OnClickMotionFemale))]
        private static void OnClickMotionFemalePost()
        {
            Patches.MenuItems.RestoreGroupInfoForMotionClick();
            if (ActionScene.Instance != null)
            {
                Patches.MenuItems.HighlightSelectedMotion();
                StateManager.Instance.MotionChangeSelectedCategory = InfoList.HAnimation.IconCategoryValue.FemaleLeading;
            }
        }

        //Update the UI when change motion button is clicked
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.OnClickTaiiCategory))]
        private static void OnClickTaiiCategoryPost()
        {
            if (StateManager.Instance.CurrentHSceneInstance != null)
            {
                //Save the main scene animation info to state if it is never set
                if (StateManager.Instance.MainSceneAnimationInfo == null)
                    StateManager.Instance.MainSceneAnimationInfo = StateManager.Instance.CurrentHSceneInstance.CtrlFlag.NowAnimationInfo;

                Patches.MenuItems.ShowGroupSelectionCanvas(StateManager.Instance.CurrentHSceneInstance._sprite.ObjTaii.isFadeIn);
            }

            if (StateManager.Instance.GroupSelection != null)
                StateManager.Instance.GroupSelection.UpdateUI();

        }

        //Hide the group selection button
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.OnClickConfig))]
        private static void OnClickConfig()
        {
            Patches.MenuItems.ShowGroupSelectionCanvas(false);
        }

        //Hide the group selection button
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.OnClickLight))]
        private static void OnClickLightPost()
        {
            Patches.MenuItems.ShowGroupSelectionCanvas(false);
        }

        //Prevent the main group to change the posture and change the selected group instead
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckFaintness))]
        private static bool CheckFaintnessPre(HScene.AnimationListInfo info)
        {
            bool isContinue = Patches.MenuItems.HandleSetAnimation(info);
            return isContinue;
        }


        //Prevent the main group speaking if we are handling the H reaction group change
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckState))]
        private static bool CheckStatePre(int mode, int state)
        {
            if (ActionScene.Instance != null && StateManager.Instance.GroupSelection != null && StateManager.Instance.GroupSelection.SelectedGroup != null)
                return false;

            return true;
        }

        //Test code for swapping
        ////[HarmonyPostfix]
        ////[HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.OnClickConfig))]
        ////private static void OnClickConfig2()
        ////{
        ////    if(StateManager.Instance.HAnimationGroupsList != null)
        ////    {
        ////        if(StateManager.Instance.HAnimationGroupsList.Count >= 2)
        ////        {


        ////            
        ////            //////var tmp = StateManager.Instance.HAnimationGroupsList[0].female1;
        ////            //////StateManager.Instance.HAnimationGroupsList[0].female1 = StateManager.Instance.HAnimationGroupsList[1].female1;
        ////            //////StateManager.Instance.HAnimationGroupsList[1].female1 = tmp;

        ////            //////StateManager.Instance.CharacterHItemCtrlDictionary[StateManager.Instance.HAnimationGroupsList[0].female1.Chara.GetInstanceID()].ReleaseItem();
        ////            //////StateManager.Instance.CharacterHItemCtrlDictionary[StateManager.Instance.HAnimationGroupsList[1].female1.Chara.GetInstanceID()].ReleaseItem();



        ////            //////Patches.HAnim.StartHAnimation(StateManager.Instance.HAnimationGroupsList[0], true, StateManager.Instance.ActorHAnimationList[StateManager.Instance.HAnimationGroupsList[0].female1.GetInstanceID()].animationListInfo);
        ////            //////Patches.HAnim.StartHAnimation(StateManager.Instance.HAnimationGroupsList[1], true, StateManager.Instance.ActorHAnimationList[StateManager.Instance.HAnimationGroupsList[1].female1.GetInstanceID()].animationListInfo);

        ////        }
        ////    }
        ////}
    }
}
