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
using System.Linq;

namespace HSceneCrowdReaction.HSceneScreen
{
    internal partial class Patches
    {
        internal class MenuItems
        {
            private static ManualLogSource Log = HSceneCrowdReactionPlugin.Log;

            internal static void AddMainHSceneToggleToState(HScene hScene)
            {
                if (ActionScene.Instance != null && hScene != null)
                {
                    StateManager.Instance.ToggleIDCharacterList.Add(hScene._sprite.CharaChoice.tglCharas[0].GetInstanceID(), hScene._chaFemales[0]);
                    if (hScene._chaFemales[1] != null)
                        StateManager.Instance.ToggleIDCharacterList.Add(hScene._sprite.CharaChoice.tglCharas[1].GetInstanceID(), hScene._chaFemales[1]);
                    if (hScene._chaMales[0] != null)
                        StateManager.Instance.ToggleIDCharacterList.Add(hScene._sprite.CharaChoice.tglCharas[2].GetInstanceID(), hScene._chaMales[0]);
                    if (hScene._chaMales[1] != null)
                        StateManager.Instance.ToggleIDCharacterList.Add(hScene._sprite.CharaChoice.tglCharas[3].GetInstanceID(), hScene._chaMales[1]);

                }
            }

            internal static void ExpandCharaChoiceArrayPerGroup(int groupCount)
            {
                var chaChoice = StateManager.Instance.CurrentHSceneInstance._sprite.CharaChoice;
                Il2CppReferenceArray<Toggle> toggles = new Il2CppReferenceArray<Toggle>(chaChoice.tglCharas.Count + Constant.characterCountInHGroup * groupCount);
                for (int i = 0; i < chaChoice.tglCharas.Count; i++)
                    toggles[i] = chaChoice.tglCharas[i];
                chaChoice.tglCharas = toggles;

                Il2CppReferenceArray<Chara.ChaControl> females = new Il2CppReferenceArray<Chara.ChaControl>(chaChoice.females.Count + Constant.femaleCountInHGroup * groupCount);
                for (int i = 0; i < chaChoice.females.Count; i++)
                    females[i] = chaChoice.females[i];
                chaChoice.females = females;

                Il2CppReferenceArray<Chara.ChaControl> males = new Il2CppReferenceArray<Chara.ChaControl>(chaChoice.males.Count + Constant.maleCountInHGroup * groupCount);
                for (int i = 0; i < chaChoice.males.Count; i++)
                    males[i] = chaChoice.males[i];
                chaChoice.males = males;

                Il2CppStructArray<bool> femaleActives = new Il2CppStructArray<bool>(chaChoice.femaleActive.Count + Constant.femaleCountInHGroup * groupCount);
                for (int i = 0; i < chaChoice.femaleActive.Count; i++)
                    femaleActives[i] = chaChoice.femaleActive[i];
                chaChoice.femaleActive = femaleActives;

                Il2CppStructArray<bool> maleActives = new Il2CppStructArray<bool>(chaChoice.maleActive.Count + Constant.maleCountInHGroup * groupCount);
                for (int i = 0; i < chaChoice.maleActive.Count; i++)
                    maleActives[i] = chaChoice.maleActive[i];
                chaChoice.maleActive = maleActives;
            }

            internal static void AddCharaChoiceToggle(Chara.ChaControl character, int groupIndex, Constant.HCharacterType characterType)
            {
                if (character == null) return;

                int offsetMixed = 0;
                int offsetBySex = 0;
                string toggleName = "";
                if (characterType == Constant.HCharacterType.Female1)
                {
                    toggleName = Constant.CharaChoiceTogglePrefixFemale + (3 + Constant.femaleCountInHGroup * groupIndex + offsetBySex);
                }
                else if (characterType == Constant.HCharacterType.Female2)
                {
                    offsetMixed = 1;
                    offsetBySex = 1;
                    toggleName = Constant.CharaChoiceTogglePrefixFemale + (3 + Constant.femaleCountInHGroup * groupIndex + offsetBySex);
                }
                else if (characterType == Constant.HCharacterType.Male1)
                {
                    offsetMixed = 2;
                    offsetBySex = 0;
                    toggleName = Constant.CharaChoiceTogglePrefixMale + (3 + Constant.maleCountInHGroup * groupIndex + offsetBySex);
                }
                else if (characterType == Constant.HCharacterType.Male2)
                {
                    offsetMixed = 3;
                    offsetBySex = 1;
                    toggleName = Constant.CharaChoiceTogglePrefixMale + (3 + Constant.maleCountInHGroup * groupIndex + offsetBySex);
                }


                var arrCharas = StateManager.Instance.CurrentHSceneInstance._sprite.ObjCloth._hSceneSpriteChaChoice.tglCharas;

                GameObject newToggleObject = GameObject.Instantiate(arrCharas[0].gameObject);
                Toggle newToggle = newToggleObject.GetComponent<Toggle>();
                newToggle.name = toggleName;
                newToggle.transform.parent = arrCharas[0].transform.parent;
                newToggle.SetToggleGroup(arrCharas[0].m_Group, false);

                var text = newToggleObject.transform.Find("Label").GetComponent<Text>();
                text.text = character.FileParam.fullname;

                newToggle.transform.parent.Find("BottomImage").SetAsLastSibling();

                var chaChoice = StateManager.Instance.CurrentHSceneInstance._sprite.CharaChoice;



                chaChoice.tglCharas[Constant.characterCountInHGroup + Constant.characterCountInHGroup * groupIndex + offsetMixed] = newToggle;

                if (character.Sex == 1)
                {
                    chaChoice.females[Constant.femaleCountInHGroup + Constant.femaleCountInHGroup * groupIndex + offsetBySex] = character;
                    chaChoice.femaleActive[Constant.femaleCountInHGroup + Constant.femaleCountInHGroup * groupIndex + offsetBySex] = false;
                }
                else
                {
                    chaChoice.males[Constant.maleCountInHGroup + Constant.maleCountInHGroup * groupIndex + offsetBySex] = character;
                    chaChoice.maleActive[Constant.maleCountInHGroup + Constant.maleCountInHGroup * groupIndex + offsetBySex] = false;
                }

                StateManager.Instance.ToggleIDCharacterList.Add(newToggle.GetInstanceID(), character);

                StateManager.Instance.HSceneDropDownSelectedCharaText = Util.GetParentTransformByName(newToggleObject.transform, "slideRoot").Find("chara/Text").GetComponent<Text>();
                StateManager.Instance.HSceneDropDownSelectedToggle = arrCharas[0];
            }

            internal static void AddAnimationGroupsToCharaChoiceToggles(List<HAnimationGroup> groups)
            {
                int index = 0;
                foreach (var group in groups)
                {
                    Patches.MenuItems.AddCharaChoiceToggle(group.female1.Chara, index, Constant.HCharacterType.Female1);
                    Patches.MenuItems.AddCharaChoiceToggle(group.female2?.Chara, index, Constant.HCharacterType.Female2);
                    Patches.MenuItems.AddCharaChoiceToggle(group.male1?.Chara, index, Constant.HCharacterType.Male1);
                    Patches.MenuItems.AddCharaChoiceToggle(group.male2?.Chara, index, Constant.HCharacterType.Male2);

                    index++;
                }
            }

            internal static void ResizeCharaChoiceDropDownViewport()
            {
                if (StateManager.Instance.HSceneDropDownSelectedToggle != null)
                {
                    var vp = Util.GetParentTransformByName(StateManager.Instance.HSceneDropDownSelectedToggle.transform, "Viewport");
                    var rt = vp.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y + Settings.ExtendCharaChoiceDropDownViewportHeight);
                }
            }

            internal static void HandleToggleClick(Toggle toggle)
            {
                if (ActionScene.Instance != null && StateManager.Instance.CurrentHSceneInstance != null)
                {
                    foreach (var t in StateManager.Instance.CurrentHSceneInstance._sprite.CharaChoice.tglCharas)
                    {
                        if (t != null)
                        {
                            if (t.GetInstanceID() == toggle.GetInstanceID())
                            {
                                toggle.transform.parent.gameObject.active = false;
                                if (StateManager.Instance.HSceneDropDownSelectedCharaText != null)
                                    StateManager.Instance.HSceneDropDownSelectedCharaText.text = toggle.transform.Find("Label").GetComponent<Text>().text;
                                StateManager.Instance.HSceneDropDownSelectedToggle = t;
                                t.Set(true);

                                if (StateManager.Instance.CurrentHSceneInstance._sprite.ClothMode == 0)
                                {
                                    UpdateClothesStateButtons(StateManager.Instance.ToggleIDCharacterList[t.GetInstanceID()]);
                                }
                                else if (StateManager.Instance.CurrentHSceneInstance._sprite.ClothMode == 1)
                                {
                                    StateManager.Instance.CurrentHSceneInstance._sprite.ObjAccessory.SetAccessoryCharacter();
                                }
                                else if (StateManager.Instance.CurrentHSceneInstance._sprite.ClothMode == 2)
                                {
                                    StateManager.Instance.CurrentHSceneInstance._sprite.ObjClothCard.SetCoordinatesCharacter();
                                }
                            }
                            else
                            {
                                t.SetIsOnWithoutNotify(false);
                            }
                        }
                    }
                }
            }

            internal static void HandleClickClothMenuButton(int mode)
            {
                if (ActionScene.Instance != null)
                {
                    if (mode != 2)
                    {
                        SetMaleTogglesActive(false);

                        var selectedToggle = GetCharaChoiceSelectedToggle();
                        if (selectedToggle != null && selectedToggle.name.StartsWith(Constant.CharaChoiceTogglePrefixMale))
                        {
                            foreach (var t in StateManager.Instance.CurrentHSceneInstance._sprite.CharaChoice.tglCharas)
                            {
                                if (t != null && !t.name.StartsWith(Constant.CharaChoiceTogglePrefixMale))
                                {
                                    StateManager.Instance.HSceneDropDownSelectedToggle = t;
                                    t.Set(true);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        SetMaleTogglesActive(true);
                    }
                }
            }

            internal static void SpoofToggleSetValue(Toggle toggle, ref bool value)
            {
                if (StateManager.Instance.CurrentHSceneInstance != null && toggle != null)
                {
                    foreach (var t in StateManager.Instance.CurrentHSceneInstance._sprite.CharaChoice.tglCharas)
                    {

                        if (t != null && t.GetInstanceID() == toggle.GetInstanceID())
                        {
                            if (value)
                            {
                                var selectedToggle = GetCharaChoiceSelectedToggle();
                                if (selectedToggle != null && selectedToggle.GetInstanceID() != toggle.GetInstanceID())
                                {
                                    value = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }



            private static void UpdateClothesStateButtons(Chara.ChaControl character)
            {
                if (ActionScene.Instance != null && StateManager.Instance.CurrentHSceneInstance != null)
                {
                    var clothCondSprite = StateManager.Instance.CurrentHSceneInstance._sprite.ObjCloth;
                    ResetAllClothesStateButtons(clothCondSprite);

                    byte minState = 2;
                    for (int i = 0; i < Constant.ClothesPartCount; i++)
                    {
                        var dict = character.GetClothesStateKind(i);
                        if (dict != null)
                        {
                            int index = character.FileStatus.clothesState[i];
                            if (i == Constant.ClothesPart.Gloves || i == Constant.ClothesPart.Socks || i == Constant.ClothesPart.Shoes)
                            {
                                //special handling for panst, socks and shoes
                                if (character.FileStatus.clothesState[i] == Constant.GeneralClothesStates.Nude)
                                    index = Constant.TwoStateClothesStates.Nude;
                            }

                            minState = Math.Min(minState, character.FileStatus.clothesState[i]);
                            clothCondSprite._clothObjSets[i].Obj.buttons[index].gameObject.SetActive(true);
                            clothCondSprite._clothObjSets[i].Obj.gameObject.SetActive(true);
                        }
                        else
                        {
                            clothCondSprite._clothObjSets[i].Obj.gameObject.SetActive(false);
                        }
                    }
                    clothCondSprite._clothAllObjSet.Obj.buttons[minState].gameObject.SetActive(true);
                }
            }

            private static void SetMaleTogglesActive(bool isActive)
            {
                if (StateManager.Instance.CurrentHSceneInstance != null)
                    foreach (var t in StateManager.Instance.CurrentHSceneInstance._sprite.CharaChoice.tglCharas)
                    {
                        if (t != null)
                        {
                            if (StateManager.Instance.ToggleIDCharacterList.ContainsKey(t.GetInstanceID()))
                            {
                                if (t.name.StartsWith(Constant.CharaChoiceTogglePrefixMale) && StateManager.Instance.ToggleIDCharacterList[t.GetInstanceID()] != null)
                                {
                                    t.gameObject.SetActive(isActive);
                                }
                            }
                        }
                    }
            }

            private static Toggle GetCharaChoiceSelectedToggle()
            {
                return StateManager.Instance.HSceneDropDownSelectedToggle;
            }

            internal static void BackupAllCharacterClothesStateToStateMananger()
            {
                if (ActionScene.Instance != null)
                {
                    StateManager.Instance.CharacterClothesStateDictionary = new Dictionary<int, Dictionary<int, byte>>();

                    foreach (var actor in ActionScene.Instance._actors)
                    {
                        Dictionary<int, byte> clothesStateData = new Dictionary<int, byte>();

                        for (int i = 0; i < Constant.ClothesPartCount; i++)
                            clothesStateData.Add(i, actor.Chara.FileStatus.clothesState[i]);

                        StateManager.Instance.CharacterClothesStateDictionary.Add(actor.Chara.GetInstanceID(), clothesStateData);
                    }

                }
            }

            internal static void RestoreAllCharacterClothesStateFromStateMananger()
            {
                if (ActionScene.Instance != null)
                {
                    foreach (var actor in ActionScene.Instance._actors)
                    {
                        var clothesStateData = StateManager.Instance.CharacterClothesStateDictionary[actor.Chara.GetInstanceID()];

                        for (int i = 0; i < Constant.ClothesPartCount; i++)
                            actor.Chara.FileStatus.clothesState[i] = clothesStateData[i];
                    }

                }
            }

            internal static void ChangeClothesState(int clothKind)
            {
                if (StateManager.Instance.ToggleIDCharacterList.ContainsKey(StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()))
                {
                    var character = StateManager.Instance.ToggleIDCharacterList[StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()];

                    byte state = character.FileStatus.clothesState[clothKind];
                    if (clothKind == Constant.ClothesPart.Gloves || clothKind == Constant.ClothesPart.Socks || clothKind == Constant.ClothesPart.Shoes)
                        state += 2;
                    else
                        state++;
                    if (state > 2)
                        state = 0;

                    character.SetClothesState(clothKind, state);

                    UpdateClothesStateButtons(character);
                }
            }

            internal static void ChangeAllClothesState()
            {
                if (StateManager.Instance.ToggleIDCharacterList.ContainsKey(StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()))
                {
                    var character = StateManager.Instance.ToggleIDCharacterList[StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()];

                    byte minState = 2;
                    for (int i = 0; i < Constant.ClothesPartCount; i++)
                    {
                        var dict = character.GetClothesStateKind(i);
                        if (dict != null)
                            minState = Math.Min(minState, character.FileStatus.clothesState[i]);

                    }

                    minState++;
                    if (minState > 2)
                        minState = 0;
                    character.SetClothesStateAll(minState);

                    UpdateClothesStateButtons(character);
                }
            }

            private static void ResetAllClothesStateButtons(HSceneSpriteClothCondition clothCondSprite)
            {
                foreach (var objSet in clothCondSprite._clothObjSets)
                    foreach (var btn in objSet.Obj.buttons)
                        btn.gameObject.active = false;
                foreach (var btn in clothCondSprite._clothAllObjSet.Obj.buttons)
                    btn.gameObject.active = false;

            }

            internal static void SpoofCharacterAccessoryList(ref Dictionary<int, Il2CppReferenceArray<Chara.ListInfoBase>> dictBackup)
            {
                dictBackup = null;

                if (ActionScene.Instance != null && StateManager.Instance.HSceneDropDownSelectedToggle != null)
                {
                    if (StateManager.Instance.ToggleIDCharacterList.ContainsKey(StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()))
                    {
                        var character = StateManager.Instance.ToggleIDCharacterList[StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()];

                        if (!IsSelectedCharacterMainFemale())
                        {
                            //backup and replace info accessory of the selected character in order to populate the list we want
                            dictBackup = new Dictionary<int, Il2CppReferenceArray<Chara.ListInfoBase>>();
                            foreach (var chaMainF in StateManager.Instance.CurrentHSceneInstance._chaFemales)
                                if (chaMainF != null)
                                {
                                    dictBackup.Add(chaMainF.GetInstanceID(), chaMainF._infoAccessory);
                                    chaMainF._infoAccessory = character._infoAccessory;
                                }
                        }
                    }
                }
            }

            internal static void RestoreCharacterAccessoryList(Dictionary<int, Il2CppReferenceArray<Chara.ListInfoBase>> dictBackup)
            {
                if (ActionScene.Instance != null)
                {
                    if (dictBackup != null)
                    {
                        //Restore the info accessory
                        foreach (var chaMainF in StateManager.Instance.CurrentHSceneInstance._chaFemales)
                            if (chaMainF != null)
                                chaMainF._infoAccessory = dictBackup[chaMainF.GetInstanceID()];
                    }
                }


            }

            internal static void UpdateAccessoryToggleState()
            {
                if (ActionScene.Instance != null && StateManager.Instance.ToggleIDCharacterList != null && StateManager.Instance.HSceneDropDownSelectedToggle != null)
                {
                    if (StateManager.Instance.ToggleIDCharacterList.ContainsKey(StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()))
                    {
                        var character = StateManager.Instance.ToggleIDCharacterList[StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()];

                        //Set the state of the toggles depend on the accessory slot on/off status of the selected character
                        bool isAccessoryOn = false;
                        for (int i = 0; i < character.CmpAccessory.Count; i++)
                        {
                            if (character.CmpAccessory[i] != null)
                            {
                                isAccessoryOn = isAccessoryOn || character.CmpAccessory[i].IsVisible;
                                StateManager.Instance.CurrentHSceneInstance._sprite.ObjAccessory.AccessorySlots.SetCheck(character.CmpAccessory[i].isActiveAndEnabled, i);
                                StateManager.Instance.CurrentHSceneInstance._sprite.ObjAccessory._toggles[i].SetIsOnWithoutNotify(character.CmpAccessory[i].isActiveAndEnabled);
                            }

                        }
                        StateManager.Instance.CurrentHSceneInstance._sprite.ObjAccessory.AllChange.SetIsOnWithoutNotify(isAccessoryOn);
                        FixAccessoryToggleCheckmark();
                    }
                }
            }

            internal static void HandleAccessorySlotClickPre(HSceneSpriteAccessoryCondition instance, int slot, ref Dictionary<int, bool> dictBackup)
            {
                dictBackup = null;
                if (ActionScene.Instance != null && StateManager.Instance.HSceneDropDownSelectedToggle != null)
                {

                    if (StateManager.Instance.ToggleIDCharacterList.ContainsKey(StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()))
                    {
                        var character = StateManager.Instance.ToggleIDCharacterList[StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()];

                        if (!IsSelectedCharacterMainFemale())
                        {
                            //Not main female, backup the slot status of main character
                            dictBackup = new Dictionary<int, bool>();
                            foreach (var chaMainF in StateManager.Instance.CurrentHSceneInstance._chaFemales)
                                if (chaMainF != null && chaMainF.CmpAccessory[slot] != null)
                                    dictBackup.Add(chaMainF.GetInstanceID(), chaMainF.CmpAccessory[slot].IsVisible);

                            //Update the accessory status of the selected character
                            character.SetAccessoryState(slot, instance._toggles[slot].isOn);
                        }
                    }
                }
            }

            internal static void HandleAccessorySlotClickPost(int slot, Dictionary<int, bool> dictBackup)
            {
                //Restore the slot status if necessary
                if (dictBackup != null)
                {
                    foreach (var chaMainF in StateManager.Instance.CurrentHSceneInstance._chaFemales)
                        if (chaMainF != null && chaMainF.CmpAccessory[slot] != null)
                            chaMainF.SetAccessoryState(slot, dictBackup[chaMainF.GetInstanceID()]);
                    //chaMainF.ObjAccessory[_accessory].active = __state[chaMainF.GetInstanceID()];
                }

                FixAccessoryToggleCheckmark();
            }

            internal static void HandleAllAccessoryClickPre(HSceneSpriteAccessoryCondition instance, ref Dictionary<int, Dictionary<int, bool>> dictBackup)
            {
                dictBackup = null;

                if (ActionScene.Instance != null && StateManager.Instance.HSceneDropDownSelectedToggle != null)
                {

                    if (StateManager.Instance.ToggleIDCharacterList.ContainsKey(StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()))
                    {
                        var character = StateManager.Instance.ToggleIDCharacterList[StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()];

                        if (!IsSelectedCharacterMainFemale())
                        {
                            //Not main female, backup all slot status of main character
                            dictBackup = new Dictionary<int, Dictionary<int, bool>>();
                            foreach (var chaMainF in StateManager.Instance.CurrentHSceneInstance._chaFemales)
                                if (chaMainF != null)
                                {
                                    var dictState = new Dictionary<int, bool>();
                                    for (int i = 0; i < chaMainF.CmpAccessory.Count; i++)
                                        if (chaMainF.CmpAccessory[i] != null)
                                            dictState.Add(i, chaMainF.CmpAccessory[i].IsVisible);

                                    dictBackup.Add(chaMainF.GetInstanceID(), dictState);
                                }

                            character.SetAccessoryStateAll(instance.AllChange.isOn);
                        }
                    }
                }
            }

            internal static void HandleAllAccessoryClickPost(Dictionary<int, Dictionary<int, bool>> dictBackup)
            {
                if (dictBackup != null)
                {
                    foreach (var chaMainF in StateManager.Instance.CurrentHSceneInstance._chaFemales)
                        if (chaMainF != null)
                        {
                            for (int i = 0; i < chaMainF.CmpAccessory.Count; i++)
                                if (chaMainF.CmpAccessory[i] != null)
                                    chaMainF.SetAccessoryState(i, dictBackup[chaMainF.GetInstanceID()][i]);
                        }
                }

                FixAccessoryToggleCheckmark();
            }

            private static bool IsSelectedCharacterMainFemale()
            {
                bool result = false;
                if (StateManager.Instance.ToggleIDCharacterList.ContainsKey(StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()))
                {
                    var character = StateManager.Instance.ToggleIDCharacterList[StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()];

                    foreach (var chaMainF in StateManager.Instance.CurrentHSceneInstance._chaFemales)
                        if (chaMainF != null && chaMainF.GetInstanceID() == character.GetInstanceID())
                            return true;
                }
                return result;
            }

            //The checkmark of the toggle button may be wrong when switching between character and this function is attempt to fix it 
            private static void FixAccessoryToggleCheckmark()
            {
                if (ActionScene.Instance != null && StateManager.Instance.ToggleIDCharacterList != null && StateManager.Instance.HSceneDropDownSelectedToggle != null)
                {
                    if (StateManager.Instance.ToggleIDCharacterList.ContainsKey(StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()))
                    {
                        var character = StateManager.Instance.ToggleIDCharacterList[StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()];

                        for (int i = 0; i < StateManager.Instance.CurrentHSceneInstance._sprite.ObjAccessory._toggles.Count; i++)
                            if (character.CmpAccessory[i] != null)
                                StateManager.Instance.CurrentHSceneInstance._sprite.ObjAccessory._toggles[i].transform.Find("state/stateOn").gameObject.SetActive(StateManager.Instance.CurrentHSceneInstance._sprite.ObjAccessory._toggles[i].isOn);

                        ////The behaviour of the All accessory button is a bit strange. Since it is just layout problem, dont want to make things too complicated so leave it
                        //if (StateManager.Instance.CurrentHSceneInstance._sprite.ObjAccessory.AllChange.isOn)
                        //{
                        //    StateManager.Instance.CurrentHSceneInstance._sprite.ObjAccessory.AllChange.transform.Find("state/stateOn").gameObject.SetActive(StateManager.Instance.CurrentHSceneInstance._sprite.ObjAccessory.AllChange.isOn);
                        //}
                    }
                }
            }

            internal static bool ChangeToDefaultOutfit()
            {
                if (ActionScene.Instance != null && StateManager.Instance.HSceneDropDownSelectedToggle != null)
                {
                    if (StateManager.Instance.ToggleIDCharacterList.ContainsKey(StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()))
                    {
                        var character = StateManager.Instance.ToggleIDCharacterList[StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()];

                        character.ChangeNowCoordinate(true, true);
                        return false;
                    }
                }
                return true;
            }

            internal static bool SpoofMainCharacterSex(HSceneSpriteCoordinatesCard instance)
            {
                bool isSpoofed = false;
                if (ActionScene.Instance != null && StateManager.Instance.HSceneDropDownSelectedToggle != null)
                {
                    if (StateManager.Instance.ToggleIDCharacterList.ContainsKey(StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()))
                    {
                        var character = StateManager.Instance.ToggleIDCharacterList[StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()];

                        foreach (var charMainF in instance.females)
                            if (charMainF != null)
                                charMainF.FileParam.sex = character.Sex;
                        foreach (var charMainM in instance.males)
                            if (charMainM != null)
                                charMainM.FileParam.sex = character.Sex;
                        instance.ChangeTargetSex(character.Sex);
                        isSpoofed = true;
                    }
                }
                return isSpoofed;
            }

            internal static void RecoverMainCharacterSex(HSceneSpriteCoordinatesCard instance)
            {
                foreach (var charMainF in instance.females)
                    if (charMainF != null)
                        charMainF.FileParam.sex = 1;
                foreach (var charMainM in instance.males)
                    if (charMainM != null)
                        charMainM.FileParam.sex = 0;
            }

            internal static bool HandleChangeOutfitClick(Button button)
            {
                if (ActionScene.Instance != null && StateManager.Instance.CurrentHSceneInstance != null)
                {
                    if (StateManager.Instance.CurrentHSceneInstance._sprite.ObjClothCard.DecideCoode.GetInstanceID() == button.GetInstanceID())
                    {
                        if (StateManager.Instance.ToggleIDCharacterList != null && StateManager.Instance.HSceneDropDownSelectedToggle != null)
                        {
                            if (StateManager.Instance.ToggleIDCharacterList.ContainsKey(StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()))
                            {
                                var character = StateManager.Instance.ToggleIDCharacterList[StateManager.Instance.HSceneDropDownSelectedToggle.GetInstanceID()];

                                character.ChangeNowCoordinate(StateManager.Instance.CurrentHSceneInstance._sprite.ObjClothCard.filename, true, true,
                                    StateManager.Instance.CurrentHSceneInstance._sprite.ObjClothCard._toggleCloth.isOn,
                                    StateManager.Instance.CurrentHSceneInstance._sprite.ObjClothCard._toggleAcs.isOn,
                                    StateManager.Instance.CurrentHSceneInstance._sprite.ObjClothCard._toggleHair.isOn
                                    );
                                return false;
                            }
                        }
                    }

                }

                return true;
            }

            internal static void SetAnimationGroupCharactersVisible(HAnimationGroup group, bool isVisible)
            {
                if (group != null)
                {
                    group.female1.Chara.VisibleAll = isVisible;
                    if (group.female2 != null)
                        group.female2.Chara.VisibleAll = isVisible;
                    if (group.male1 != null)
                    {
                        group.male1.Chara.VisibleAll = isVisible;
                        if (StateManager.Instance.ActorHAnimationList[group.male1.GetInstanceID()].animationListInfo.MaleSon == 1)
                            Patches.HAnim.ForcePenisVisible(group.male1.Chara, isVisible);
                    }
                    if (group.male2 != null)
                    {
                        group.male2.Chara.VisibleAll = isVisible;
                        if (StateManager.Instance.ActorHAnimationList[group.male2.GetInstanceID()].animationListInfo.MaleSon == 1)
                            Patches.HAnim.ForcePenisVisible(group.male2.Chara, isVisible);
                    }

                    StateManager.Instance.CharacterHItemCtrlDictionary[group.female1.Chara.GetInstanceID()].SetVisible(isVisible);
                    Patches.HAnim.SetHPointObjectVisible(group.hPoint, isVisible);
                }

            }

            internal static void InitGroupSelectionControl(HSceneSprite instance)
            {
                if (ActionScene.Instance != null)
                {
                    if (StateManager.Instance.HAnimationGroupsList != null && StateManager.Instance.HAnimationGroupsList.Count > 0)
                    {
                        StateManager.Instance.GroupSelection = new GroupSelectionControl(StateManager.Instance.CurrentHSceneInstance, StateManager.Instance.HAnimationGroupsList, StateManager.Instance);
                        StateManager.Instance.GroupSelection.InitControl(instance.transform);
                    }
                }
            }

            internal static void PrepareHPointChange()
            {
                if (ActionScene.Instance != null && StateManager.Instance.GroupSelection != null)
                {
                    if (StateManager.Instance.GroupSelection.SelectedGroup != null)
                    {
                        StateManager.Instance.CurrentHSceneInstance.HPointCtrl._usePlace = new Il2CppSystem.Collections.Generic.List<int>();
                        var usePlaces = InfoList.HAnimation.GetPlaceKindBySiuationType(StateManager.Instance.GroupSelection.SelectedGroup.situationType);

                        foreach (var place in usePlaces)
                            StateManager.Instance.CurrentHSceneInstance.HPointCtrl._usePlace.Add(place);

                    }
                    else
                    {
                        StateManager.Instance.CurrentHSceneInstance.HPointCtrl._usePlace = StateManager.Instance.MainSceneUsePlaces;
                    }

                    HAnim.UpdateMobPoint();
                }
            }

            internal static void PrepareHPointChange2()
            {
                if (ActionScene.Instance != null && StateManager.Instance.GroupSelection != null)
                {
                    if (StateManager.Instance.CurrentHSceneInstance.CtrlFlag.IsPointMoving)
                        UpdateGroupVisibility(StateManager.Instance.GroupSelection.SelectedGroup);

                }
            }

            internal static void FinishMoveForGroup()
            {
                if (ActionScene.Instance != null)
                {
                    //Show all characters
                    ResetAllGroupVisibility();

                    StateManager.Instance.MovingToHPoint = null;
                }
            }

            private static void UpdateGroupVisibility(HAnimationGroup group)
            {
                foreach (var g in StateManager.Instance.HAnimationGroupsList)
                    SetAnimationGroupCharactersVisible(g, true);

                if (group != null)
                {
                    SetMainSceneCharactersVisible(true);
                    SetAnimationGroupCharactersVisible(group, false);
                }
                else
                {
                    SetMainSceneCharactersVisible(false);
                }
            }

            private static void ResetAllGroupVisibility()
            {
                SetMainSceneCharactersVisible(true);
                foreach (var g in StateManager.Instance.HAnimationGroupsList)
                    SetAnimationGroupCharactersVisible(g, true);
            }

            private static void SetMainSceneCharactersVisible(bool isVisible)
            {
                foreach (var character in StateManager.Instance.CurrentHSceneInstance._chaFemales)
                    if (character != null)
                        character.VisibleAll = isVisible;
                foreach (var character in StateManager.Instance.CurrentHSceneInstance._chaMales)
                    if (character != null)
                        character.VisibleAll = isVisible;

                StateManager.Instance.CurrentHSceneInstance._ctrlItem.SetVisible(isVisible);
                HAnim.SetHPointObjectVisible(StateManager.Instance.CurrentHSceneInstance.CtrlFlag.NowHPoint, isVisible);
            }

            internal static void ShowGroupSelectionCanvas(bool isCanvasVisible)
            {
                if (ActionScene.Instance != null && StateManager.Instance.GroupSelection != null)
                    if (StateManager.Instance.HAnimationGroupsList != null)
                        if (StateManager.Instance.HAnimationGroupsList.Count > 0)
                            StateManager.Instance.GroupSelection.canvas.gameObject.SetActive(isCanvasVisible);
            }

            internal static int GetSpoofMotionCount(int place, bool setMarker, int originalResult)
            {
                if (ActionScene.Instance != null && StateManager.Instance.GroupSelection != null && StateManager.Instance.GroupSelection.SelectedGroup != null && setMarker)
                {
                    bool isValidPoint = false;
                    if (StateManager.Instance.GroupSelection.SelectedGroup.situationType == InfoList.HAnimation.SituationType.MF && InfoList.HAnimation.ValidHPointTypeMF.Contains(place))
                        isValidPoint = true;
                    else if (StateManager.Instance.GroupSelection.SelectedGroup.situationType == InfoList.HAnimation.SituationType.FF && InfoList.HAnimation.ValidHPointTypeFF.Contains(place))
                        isValidPoint = true;
                    else if (StateManager.Instance.GroupSelection.SelectedGroup.situationType == InfoList.HAnimation.SituationType.FFM && InfoList.HAnimation.ValidHPointTypeFFM.Contains(place))
                        isValidPoint = true;
                    else if (StateManager.Instance.GroupSelection.SelectedGroup.situationType == InfoList.HAnimation.SituationType.MMF && InfoList.HAnimation.ValidHPointTypeMMF.Contains(place))
                        isValidPoint = true;

                    if (isValidPoint && originalResult == 0)
                        return 1;
                    if (!isValidPoint && originalResult > 0)
                        return 0;
                }
                return originalResult;
            }

            internal static bool HandleSetPosition()
            {
                if (ActionScene.Instance != null && StateManager.Instance.GroupSelection != null && StateManager.Instance.GroupSelection.SelectedGroup != null && StateManager.Instance.MovingToHPoint != null)
                {
                    MoveGroupToHPoint(StateManager.Instance.GroupSelection.SelectedGroup, StateManager.Instance.MovingToHPoint);
                    return false;
                }
                return true;
            }

            internal static bool HandleSetAnimation(HScene.AnimationListInfo animInfo)
            {
                if (ActionScene.Instance != null && StateManager.Instance.GroupSelection != null && StateManager.Instance.GroupSelection.SelectedGroup != null)
                {
                    HAnim.StartHAnimation(StateManager.Instance.GroupSelection.SelectedGroup, false, animInfo);
                    return false;
                }
                return true;
            }

            private static void MoveGroupToHPoint(HAnimationGroup group, HPoint hPoint)
            {
                if (ActionScene.Instance != null && group != null && hPoint != null)
                {
                    //Reset the hpoint before moving
                    HAnim.SetHPointObjectVisible(StateManager.Instance.CharacterHPointDictionary[group.female1.Chara.GetInstanceID()], false);

                    //Set to the new hpoint and reset the animation
                    StateManager.Instance.CharacterHPointDictionary[group.female1.Chara.GetInstanceID()] = hPoint;
                    if (group.female2 != null)
                        StateManager.Instance.CharacterHPointDictionary[group.female2.Chara.GetInstanceID()] = hPoint;
                    if (group.male1 != null)
                        StateManager.Instance.CharacterHPointDictionary[group.male1.Chara.GetInstanceID()] = hPoint;
                    if (group.male2 != null)
                        StateManager.Instance.CharacterHPointDictionary[group.male2.Chara.GetInstanceID()] = hPoint;
                    group.hPoint = hPoint;
                    HAnim.StartHAnimation(group, false);
                }
            }

            internal static HPoint HandleHPointClick(HPoint point)
            {
                if (ActionScene.Instance != null && StateManager.Instance.GroupSelection != null)
                {
                    if (StateManager.Instance.GroupSelection.SelectedGroup != null)
                    {
                        StateManager.Instance.MovingToHPoint = point;
                        point = StateManager.Instance.MainSceneHPoint;
                        StateManager.Instance.CurrentHSceneInstance.CtrlFlag.NowHPoint = StateManager.Instance.MainSceneHPoint;
                    }
                    else
                    {
                        //Update the StateManager HPoint that the main group i using
                        StateManager.Instance.MainSceneHPoint = point;
                    }
                }
                
                return point;
            }

            internal static void UpdateSexPositionIconVisibility(HAnimationGroup selectedGroup)
            {
                if (ActionScene.Instance != null && StateManager.Instance.GroupSelection != null)
                {
                    HPoint pointToCheck;
                    InfoList.HAnimation.SituationType situationType;
                    if (selectedGroup == null)
                    {
                        pointToCheck = StateManager.Instance.CurrentHSceneInstance.CtrlFlag.NowHPoint;
                        situationType = InfoList.HAnimation.GetSituationType(StateManager.Instance.CurrentHSceneInstance);
                    }
                    else
                    {
                        pointToCheck = selectedGroup.hPoint;
                        situationType = selectedGroup.situationType;
                    }
                    
                    //Get the available animation list for the HPoint
                    List<string> lstAvailableIcon = new List<string>();
                    int hPointType = Patches.HAnim.GetHPointType(pointToCheck);
                    var possibleAnimList = Patches.HAnim.GetAvailableHAnimationList(pointToCheck, hPointType, situationType);
                    
                    foreach (var animInfo in possibleAnimList)
                    {
                        int animGroup = Util.GetHAnimationGroup(animInfo);
                        var extraInfo = InfoList.HAnimation.ExtraHAnimationDataDictionary[(animGroup, animInfo.ID)];
                        
                        string iconName = InfoList.HAnimation.GetIconObjectNameByCategory(extraInfo.iconCategory);

                        if (!lstAvailableIcon.Contains(iconName))
                            lstAvailableIcon.Add(iconName);
                    }

                    foreach (var icon in StateManager.Instance.CurrentHSceneInstance._sprite.CategoryMain._categoryObjs)
                    {
                        if (lstAvailableIcon.Contains(icon.name))
                            icon.SetActive(true);
                        else
                            icon.SetActive(false);
                    }

                }
            }

            //Update the Sprite HAnimation List if a background group is selected
            internal static void SpoofHAnimationListForBackgroundGroup(HSceneSprite instance)
            {
                if (StateManager.Instance.GroupSelection != null)
                {
                    if (StateManager.Instance.GroupSelection.SelectedGroup != null)
                    {
                        var spoofList = new Il2CppReferenceArray<Il2CppSystem.Collections.Generic.List<HScene.AnimationListInfo>>(instance._lstAnimInfo.Length);
                        for (int i = 0; i < instance._lstAnimInfo.Length; i++)
                        {
                            Il2CppSystem.Collections.Generic.List<HScene.AnimationListInfo> lstSpoofInfo = new Il2CppSystem.Collections.Generic.List<HScene.AnimationListInfo>();
                            foreach (var info in instance._lstAnimInfo[i])
                            {
                                if (!InfoList.HAnimation.ExcludeList.Contains((i, info.ID)))
                                    lstSpoofInfo.Add(info);
                            }
                            spoofList[i] = lstSpoofInfo;
                        }
                        instance._lstAnimInfo = spoofList;
                    }
                }
            }

            internal static void SpoofGroupInfoForMotionClick()
            {
                if (ActionScene.Instance != null && StateManager.Instance.GroupSelection != null && StateManager.Instance.GroupSelection.SelectedGroup != null)
                {
                    StateManager.Instance.CurrentHSceneInstance.CtrlFlag.NowHPoint = StateManager.Instance.GroupSelection.SelectedGroup.hPoint;
                    StateManager.Instance.CurrentHSceneInstance.CtrlFlag.NowAnimationInfo = StateManager.Instance.ActorHAnimationList[StateManager.Instance.GroupSelection.SelectedGroup.female1.GetInstanceID()].animationListInfo;

                    //for the background group always spoof as no event
                    StateManager.Instance.MainSceneHEventID = StateManager.Instance.CurrentHSceneInstance._sprite._eventNo;
                    StateManager.Instance.CurrentHSceneInstance._sprite._eventNo = -1;
                }
            }

            internal static void RestoreGroupInfoForMotionClick()
            {
                if (ActionScene.Instance != null && StateManager.Instance.GroupSelection != null && StateManager.Instance.GroupSelection.SelectedGroup != null)
                {
                    StateManager.Instance.CurrentHSceneInstance.CtrlFlag.NowHPoint = StateManager.Instance.MainSceneHPoint;
                    StateManager.Instance.CurrentHSceneInstance.CtrlFlag.NowAnimationInfo = StateManager.Instance.MainSceneAnimationInfo;
                    StateManager.Instance.CurrentHSceneInstance._sprite._eventNo = StateManager.Instance.MainSceneHEventID;
                }
            }

            internal static void HighlightSelectedMotion()
            {
                if (ActionScene.Instance != null)
                {
                    HScene.AnimationListInfo animInfo;
                    if (StateManager.Instance.GroupSelection != null && StateManager.Instance.GroupSelection.SelectedGroup != null)
                        animInfo = StateManager.Instance.ActorHAnimationList[StateManager.Instance.GroupSelection.SelectedGroup.female1.GetInstanceID()].animationListInfo;
                    else
                        animInfo = StateManager.Instance.CurrentHSceneInstance.CtrlFlag.NowAnimationInfo;

                    foreach (var tgl in StateManager.Instance.CurrentHSceneInstance._sprite.tglMotions)
                        if (tgl.transform.Find("Label").GetComponent<Text>().text == animInfo.NameAnimation)
                            tgl.DoStateTransition(Selectable.SelectionState.Highlighted, true);
                }
            }
        }
    }
}
