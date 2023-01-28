using BepInEx.Logging;
using HarmonyLib;
using HSceneCrowdReaction.BackgroundHAnimation;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using RG.Scene.Action.Core;
using RG.Scene;
using UnhollowerBaseLib;
using System.Collections.Generic;
using System;

namespace HSceneCrowdReaction.HSceneScreen
{
    internal partial class Patches
    {
        internal class HotKey
        {
            private static ManualLogSource Log = HSceneCrowdReactionPlugin.Log;

            internal static bool CheckHotKeyPressed()
            {
                bool isShiftPressing = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                if (ActionScene.Instance != null)
                {
                    //////TODO: The eye sight is probably working but need to currently restore the point when animation changed.
                    ////if (Input.GetKeyDown(KeyCode.Alpha1) && isShiftPressing)
                    ////{
                    ////    //eye sight
                    ////    StateManager.Instance.HotKeyState.HReactionFemaleEyeSightCameraState = !StateManager.Instance.HotKeyState.HReactionFemaleEyeSightCameraState;
                    ////    foreach (var group in StateManager.Instance.HAnimationGroupsList)
                    ////    {
                    ////        SetCharacterEyeSight(group.female1.Chara, StateManager.Instance.HotKeyState.HReactionFemaleEyeSightCameraState);
                    ////        SetCharacterEyeSight(group.female2?.Chara, StateManager.Instance.HotKeyState.HReactionFemaleEyeSightCameraState);
                    ////    }
                    ////    return false;
                    ////}

                    //////TODO: The face direction change is not working. Unable to override the animation setting
                    ////if (Input.GetKeyDown(KeyCode.Alpha2) && isShiftPressing)
                    ////{
                    ////    //face direction
                    ////    StateManager.Instance.HotKeyState.HReactionFemaleFaceDirectionCameraState = !StateManager.Instance.HotKeyState.HReactionFemaleFaceDirectionCameraState;
                    ////    foreach (var group in StateManager.Instance.HAnimationGroupsList)
                    ////    {
                    ////        SetCharacterFaceDirection(group.female1.Chara, StateManager.Instance.HotKeyState.HReactionFemaleFaceDirectionCameraState);
                    ////        SetCharacterFaceDirection(group.female2?.Chara, StateManager.Instance.HotKeyState.HReactionFemaleFaceDirectionCameraState);
                    ////    }
                    ////    return false;
                    ////}
                    
                    //UpdateSimpleBodyColor();

                    if (Input.GetKeyDown(KeyCode.Alpha3) && isShiftPressing)
                    {
                        StateManager.Instance.HotKeyState.HReactionMaleBodyVisibleState = !StateManager.Instance.HotKeyState.HReactionMaleBodyVisibleState;
                        HandleMaleBodyVisible(StateManager.Instance.HotKeyState.HReactionMaleBodyVisibleState);
                        return false;
                    }

                    if (Input.GetKeyDown(KeyCode.Alpha4) && isShiftPressing)
                    {
                        StateManager.Instance.HotKeyState.HReactionMalePenisVisibleState = !StateManager.Instance.HotKeyState.HReactionMalePenisVisibleState;
                        HandleMalePenisVisible(StateManager.Instance.HotKeyState.HReactionMalePenisVisibleState);
                        return false;
                    }

                    if (Input.GetKeyDown(KeyCode.Alpha5) && isShiftPressing)
                    {
                        StateManager.Instance.HotKeyState.HReactionMaleClothesVisibleState = !StateManager.Instance.HotKeyState.HReactionMaleClothesVisibleState;
                        HandleMaleClothesVisible(StateManager.Instance.HotKeyState.HReactionMaleClothesVisibleState);
                        return false;
                    }

                    if (Input.GetKeyDown(KeyCode.Alpha6) && isShiftPressing)
                    {
                        StateManager.Instance.HotKeyState.HReactionMaleAccessoryVisibleState = !StateManager.Instance.HotKeyState.HReactionMaleAccessoryVisibleState;
                        HandleMaleAccessoryVisible(StateManager.Instance.HotKeyState.HReactionMaleAccessoryVisibleState);
                        return false;
                    }

                    if (Input.GetKeyDown(KeyCode.Alpha7) && isShiftPressing)
                    {
                        StateManager.Instance.HotKeyState.HReactionMaleShoesVisibleState = !StateManager.Instance.HotKeyState.HReactionMaleShoesVisibleState;
                        HandleMaleShoesVisible(StateManager.Instance.HotKeyState.HReactionMaleShoesVisibleState);
                        return false;
                    }

                    if (Input.GetKeyDown(KeyCode.Alpha8) && isShiftPressing)
                    {
                        StateManager.Instance.HotKeyState.HReactionMaleSimpleBodyState = !StateManager.Instance.HotKeyState.HReactionMaleSimpleBodyState;
                        HandleMaleSimpleBodyVisible(StateManager.Instance.HotKeyState.HReactionMaleSimpleBodyState);
                        return false;
                    }
                }
                return true;
            }

            internal static void RestoreHReactionMaleStates()
            {
                if (ActionScene.Instance != null)
                {
                    StateManager.Instance.HotKeyState.HReactionMaleBodyVisibleState = true;
                    StateManager.Instance.HotKeyState.HReactionMaleSimpleBodyState = false;
                    HandleMaleBodyVisible(true);
                    HandleMalePenisVisible(true);
                    HandleMaleClothesVisible(true);
                    HandleMaleAccessoryVisible(true);
                    HandleMaleShoesVisible(true);
                    HandleMaleSimpleBodyVisible(false);
                }
            }

            private static void UpdateSimpleBodyColor()
            {
                foreach (var group in StateManager.Instance.HAnimationGroupsList)
                {
                    if (group.male1 != null)
                        group.male1.Chara.ChangeSimpleBodyColor(ProcBase._configH.SilhouetteColor);
                    if (group.male2 != null)
                        group.male2.Chara.ChangeSimpleBodyColor(ProcBase._configH.SilhouetteColor);
                }
            }

            private static void HandleMaleBodyVisible(bool isVisible)
            {
                foreach (var group in StateManager.Instance.HAnimationGroupsList)
                {
                    SetCharacterBodyVisible(group.male1?.Chara, isVisible);
                    SetCharacterBodyVisible(group.male2?.Chara, isVisible);
                }
            }

            private static void SetCharacterBodyVisible(Chara.ChaControl character, bool isVisible)
            {
                if (character == null) return;
                character._invisibleSet = !isVisible;
                UpdateCharacterBodyRenderState(character);
            }

            private static void HandleMalePenisVisible(bool isVisible)
            {
                foreach (var group in StateManager.Instance.HAnimationGroupsList)
                {
                    SetCharacterPenisVisible(group.male1?.Chara, isVisible);
                    SetCharacterPenisVisible(group.male2?.Chara, isVisible);
                }
            }

            private static void SetCharacterPenisVisible(Chara.ChaControl character, bool isVisible)
            {
                if (character == null) return;

                //character.VisibleSon = isVisible;
                var rend = character.GetComponentsInChildren<Renderer>();
                foreach (var r in rend)
                {
                    if (Settings.PenisRendererNameList.Contains(r.name))
                        r.enabled = isVisible;
                }
            }

            private static void HandleMaleClothesVisible(bool isVisible)
            {
                foreach (var group in StateManager.Instance.HAnimationGroupsList)
                {
                    SetMaleCharacterClothesVisible(group.male1?.Chara, isVisible);
                    SetMaleCharacterClothesVisible(group.male2?.Chara, isVisible);
                }
            }

            private static void SetMaleCharacterClothesVisible(Chara.ChaControl character, bool isVisible)
            {
                if (character == null) return;

                byte state = (byte)(isVisible ? Constant.GeneralClothesStates.Full : Constant.GeneralClothesStates.Nude);
                for (int i = 0; i < Constant.ClothesPartCount; i++)
                {
                    if (i != Constant.ClothesPart.Shoes)
                        character.FileStatus.clothesState[i] = state;
                }
            }

            private static void HandleMaleAccessoryVisible(bool isVisible)
            {
                foreach (var group in StateManager.Instance.HAnimationGroupsList)
                {
                    SetCharacterAccessoryVisible(group.male1?.Chara, isVisible);
                    SetCharacterAccessoryVisible(group.male2?.Chara, isVisible);
                }
            }

            private static void SetCharacterAccessoryVisible(Chara.ChaControl character, bool isVisible)
            {
                if (character == null) return;

                character.SetAccessoryStateAll(isVisible);
            }

            private static void HandleMaleShoesVisible(bool isVisible)
            {
                foreach (var group in StateManager.Instance.HAnimationGroupsList)
                {
                    SetMaleCharacterShoesVisible(group.male1?.Chara, isVisible);
                    SetMaleCharacterShoesVisible(group.male2?.Chara, isVisible);
                }
            }

            private static void SetMaleCharacterShoesVisible(Chara.ChaControl character, bool isVisible)
            {
                if (character == null) return;

                byte state = (byte)(isVisible ? Constant.GeneralClothesStates.Full : Constant.GeneralClothesStates.Nude);
                character.FileStatus.clothesState[Constant.ClothesPart.Shoes] = state;
            }

            private static void HandleMaleSimpleBodyVisible(bool isVisible)
            {
                foreach (var group in StateManager.Instance.HAnimationGroupsList)
                {
                    SetMaleCharacterSimpleBody(group.male1?.Chara, isVisible);
                    SetMaleCharacterSimpleBody(group.male2?.Chara, isVisible);
                }
            }

            private static void SetMaleCharacterSimpleBody(Chara.ChaControl character, bool isSimpleBody)
            {
                if (character == null) return;

                character.drawSimple = isSimpleBody;
                UpdateCharacterBodyRenderState(character);
            }

            //Not in use atm. The eye sight change correctly but need to fix it everytime the animation changed.
            private static void SetCharacterEyeSight(Chara.ChaControl character, bool isLookAtCamera)
            {
                if (character == null) return;

                StateManager.BackUpInformation backupInfo;
                if (StateManager.Instance.HotKeyState.HReactionFemaleLookState.ContainsKey(character.GetInstanceID()))
                    backupInfo = StateManager.Instance.HotKeyState.HReactionFemaleLookState[character.GetInstanceID()];
                else
                {
                    backupInfo = new StateManager.BackUpInformation();
                    StateManager.Instance.HotKeyState.HReactionFemaleLookState.Add(character.GetInstanceID(), backupInfo);
                }

                if (isLookAtCamera)
                {
                    //update the backupInfo
                    backupInfo.lookEyePtn = character.GetLookEyesPtn();
                    backupInfo.lookEyeTarget = character.EyeLookCtrl.target;

                    character.ChangeLookEyesTarget(backupInfo.lookEyePtn, StateManager.Instance.CurrentHSceneInstance.CtrlFlag.CameraCtrl.transform);
                    character.ChangeLookEyesPtn(backupInfo.lookEyePtn);

                }
                else
                {
                    character.ChangeLookEyesTarget(backupInfo.lookEyePtn, backupInfo.lookEyeTarget);
                    character.ChangeLookEyesPtn(backupInfo.lookEyePtn);
                }
            }

            //Not in use atm. The neck doesnt move. Need to call ChangeLookNeck in Update()?
            private static void SetCharacterFaceDirection(Chara.ChaControl character, bool isLookAtCamera)
            {
                if (character == null) return;

                StateManager.BackUpInformation backupInfo;
                if (StateManager.Instance.HotKeyState.HReactionFemaleLookState.ContainsKey(character.GetInstanceID()))
                    backupInfo = StateManager.Instance.HotKeyState.HReactionFemaleLookState[character.GetInstanceID()];
                else
                {
                    backupInfo = new StateManager.BackUpInformation();
                    StateManager.Instance.HotKeyState.HReactionFemaleLookState.Add(character.GetInstanceID(), backupInfo);
                }

                if (isLookAtCamera)
                {
                    //update the backupInfo
                    backupInfo.lookNeckPtn = character.GetLookNeckPtn();
                    backupInfo.lookNeckTarget = character.NeckLookCtrl.target;

                    character.NeckLookCtrl.neckLookScript.lookType = Illusion.Unity.Animations.NECK_LOOK_TYPE_VER2.TARGET;

                    character.ChangeLookNeckTarget(backupInfo.lookNeckPtn, StateManager.Instance.CurrentHSceneInstance.CtrlFlag.CameraCtrl.transform);
                    character.ChangeLookNeckPtn(backupInfo.lookNeckPtn);
                }
                else
                {
                    character.ChangeLookNeckTarget(backupInfo.lookNeckPtn, backupInfo.lookNeckTarget);
                    character.ChangeLookNeckPtn(backupInfo.lookNeckPtn);
                    character.NeckLookCtrl.neckLookScript.lookType = Illusion.Unity.Animations.NECK_LOOK_TYPE_VER2.ANIMATION;
                }
            }


            private static void UpdateCharacterBodyRenderState(Chara.ChaControl character)
            {
                Actor actor = Util.GetActorByChaControlID(character.GetInstanceID());
                var group = StateManager.Instance.ActorHGroupDictionary[actor.GetInstanceID()];
                Il2CppReferenceArray<GameObject> hItems = Patches.HAnim.GetItemObjectsArrayForGroup(group);

                //The hItems renderer to be excluded when changing renderer state
                List<string> excludedRendererName = new List<string>();
                foreach (var obj in hItems)
                {
                    var objRend = obj.GetComponent<Renderer>();
                    if (objRend != null)
                        excludedRendererName.Add(objRend.name);
                    var rends = obj.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in rends)
                        excludedRendererName.Add(renderer.name);
                }

                bool isRendererEnable;
                if (StateManager.Instance.HotKeyState.HReactionMaleBodyVisibleState)
                {
                    if (StateManager.Instance.HotKeyState.HReactionMaleSimpleBodyState)
                    {
                        //show simple body
                        isRendererEnable = false;

                        character.CmpSimpleBody.targetCustom.rendBody.enabled = true;
                        character.CmpSimpleBody.targetCustom.rendBody.gameObject.SetActive(true);
                        StateManager.Instance.ForceActiveInstanceID.Add(character.CmpSimpleBody.targetCustom.rendBody.gameObject.GetInstanceID());
                    }
                    else
                    {
                        //show normal body
                        isRendererEnable = true;
                        StateManager.Instance.ForceActiveInstanceID.Remove(character.CmpSimpleBody.targetCustom.rendBody.gameObject.GetInstanceID());
                    }
                }
                else
                {
                    //do not show both normal body and simply body
                    isRendererEnable = false;
                    StateManager.Instance.ForceActiveInstanceID.Remove(character.CmpSimpleBody.targetCustom.rendBody.gameObject.GetInstanceID());
                }

                var rend = character.GetComponentsInChildren<Renderer>();
                foreach (var r in rend)
                {
                    if (!Settings.PenisRendererNameList.Contains(r.name)
                        && !Settings.SimpleBodyRendereNameList.Contains(r.name)
                        && !excludedRendererName.Contains(r.name)
                        )
                        r.enabled = isRendererEnable;
                }
            }
        }
    }
}
