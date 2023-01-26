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
        internal class MenuItems
        {
            private static ManualLogSource Log = HSceneCrowdReactionPlugin.Log;

            internal static void AddMainHSceneToggleToState(HScene hScene)
            {
                if(ActionScene.Instance != null && hScene != null)
                {
                    StateManager.Instance.ToggleIDCharacterList.Add(hScene._sprite.CharaChoice.tglCharas[0].GetInstanceID(), hScene._chaFemales[0]);
                    if(hScene._chaFemales[1] != null)
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

                Il2CppReferenceArray<Toggle> toggles = new Il2CppReferenceArray<Toggle>(chaChoice.tglCharas.Count + 4 * groupCount);
                for (int i = 0; i < 4; i++)
                    toggles[i] = chaChoice.tglCharas[i];
                chaChoice.tglCharas = toggles;

                Il2CppReferenceArray<Chara.ChaControl> females = new Il2CppReferenceArray<Chara.ChaControl>(chaChoice.females.Count + 2 * groupCount);
                for (int i = 0; i < 2; i++)
                    females[i] = chaChoice.females[i];
                chaChoice.females = females;

                Il2CppReferenceArray<Chara.ChaControl> males = new Il2CppReferenceArray<Chara.ChaControl>(chaChoice.males.Count + 2 * groupCount);
                for (int i = 0; i < 2; i++)
                    males[i] = chaChoice.males[i];
                chaChoice.males = males;

                Il2CppStructArray<bool> femaleActives = new Il2CppStructArray<bool>(chaChoice.femaleActive.Count + 2 * groupCount);
                for (int i = 0; i < chaChoice.femaleActive.Count; i++)
                    femaleActives[i] = chaChoice.femaleActive[i];
                chaChoice.femaleActive = femaleActives;

                Il2CppStructArray<bool> maleActives = new Il2CppStructArray<bool>(chaChoice.maleActive.Count + 2 * groupCount);
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
                    toggleName = Constant.CharaChoiceTogglePrefixFemale + (3 + 2 * groupIndex + offsetBySex);
                }
                else if (characterType == Constant.HCharacterType.Female2)
                {
                    offsetMixed = 1;
                    offsetBySex = 1;
                    toggleName = Constant.CharaChoiceTogglePrefixFemale + (3 + 2 * groupIndex + offsetBySex);
                }
                else if (characterType == Constant.HCharacterType.Male1)
                {
                    offsetMixed = 2;
                    offsetBySex = 0;
                    toggleName = Constant.CharaChoiceTogglePrefixMale + (3 + 2 * groupIndex + offsetBySex);
                }
                else if (characterType == Constant.HCharacterType.Male2)
                {
                    offsetMixed = 3;
                    offsetBySex = 1;
                    toggleName = Constant.CharaChoiceTogglePrefixMale + (3 + 2 * groupIndex + offsetBySex);
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

                

                chaChoice.tglCharas[4 + 4 * groupIndex + offsetMixed] = newToggle;

                if (character.Sex == 1)
                {
                    chaChoice.females[2 + 2 * groupIndex + offsetBySex] = character;
                    chaChoice.femaleActive[2 + 2 * groupIndex + offsetBySex] = false;
                }
                else
                {
                    chaChoice.males[2 + 2 * groupIndex + offsetBySex] = character;
                    chaChoice.maleActive[2 + 2 * groupIndex + offsetBySex] = false;
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
                var vp = Util.GetParentTransformByName(StateManager.Instance.HSceneDropDownSelectedToggle.transform, "Viewport");
                var rt = vp.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y + Settings.ExtendCharaChoiceDropDownViewportHeight);
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
                                StateManager.Instance.HSceneDropDownSelectedCharaText.text = toggle.transform.Find("Label").GetComponent<Text>().text;
                                StateManager.Instance.HSceneDropDownSelectedToggle = t;
                                t.Set(true);

                                if (StateManager.Instance.CurrentHSceneInstance._sprite.ClothMode == 0)
                                {
                                    UpdateClothesStateButtons(StateManager.Instance.ToggleIDCharacterList[t.GetInstanceID()]);
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
                        if (selectedToggle.name.StartsWith(Constant.CharaChoiceTogglePrefixMale))
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
        }
    }
}
