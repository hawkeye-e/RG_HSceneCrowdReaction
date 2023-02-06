using BepInEx.Logging;
using HarmonyLib;
using HSceneCrowdReaction.InfoList;
using HSceneCrowdReaction.BackgroundHAnimation;
using RG.Scene;
using RG.Scene.Action.Core;
using System;
using System.Collections.Generic;
using UnhollowerBaseLib;
using UnityEngine;

namespace HSceneCrowdReaction.HSceneScreen
{
    internal partial class Patches
    {
        internal class HAnim
        {
            private static ManualLogSource Log = HSceneCrowdReactionPlugin.Log;

            internal static void SetupAnimationGroups(HScene hScene)
            {
                if (ActionScene.Instance != null)
                {
                    //Find out all the characters that are not involved in H
                    List<Actor> charList = Util.GetActorsNotInvolvedInH(ActionScene.Instance, hScene);

                    //Get the H groups and stored the state
                    var groups = HAnimationGroup.GetHAnimationGroups(charList);
                    foreach (var group in groups)
                        StateManager.UpdateHGroupDictionary(group);
                }
            }

            //Handle animation for group with >1 actors
            internal static void StartHAnimation(HAnimationGroup animGroup, bool requireInit = true, HScene.AnimationListInfo selectedAnimInfo = null)
            {
                HPoint hPoint;

                if (requireInit)
                {
                    InitBodyBoneForGroup(animGroup);

                    InitHAnimationCtrlForGroup(animGroup);
                    InitHLayerCtrlForGroup(animGroup);

                    InitHItemCtrlForGroup(animGroup);
                }
                //Get the HPoint
                if(!StateManager.Instance.CharacterHPointDictionary.ContainsKey(animGroup.female1.Chara.GetInstanceID()))
                    hPoint = HAnimationGroup.GetHPointForGroup(animGroup);
                else
                    hPoint = StateManager.Instance.CharacterHPointDictionary[animGroup.female1.Chara.GetInstanceID()];
                

                animGroup.hPoint = hPoint;

                //Get the chosen animation
                HScene.AnimationListInfo animInfo = selectedAnimInfo;
                if(animInfo == null)
                    animInfo = GetHAnimation(hPoint, animGroup.situationType);

                //Setup HPoint
                InitHPoint(hPoint);

                //Assign the actor to the HPoint
                SetActorToHPointForGroup(animGroup, hPoint, animInfo);

                //Set the animation of the actors
                SetActorAnimatorForGroup(animGroup, animInfo);

                //Add animation data to StateManager                
                AddActorHAnimationDataForGroup(animGroup, animInfo);

                //Play the animation
                PlayAnimationForGroup(animGroup);

                //Update the clothes state of the actor
                SetActorClothesStateForGroup(animGroup, animInfo);

                //Set timer for periodic update. Add only 1 actor to the next update list
                AddOrUpdateHAnimNextUpdateTime(animGroup.female1);
            }

            internal static void CheckUpdateHAnim(Chara.ChaControl character)
            {
                if (ActionScene.Instance != null && StateManager.Instance.ActorHAnimNextUpdateTimeDictionary != null)
                {
                    if (!StateManager.Instance.ActorHAnimNextUpdateProcessing.ContainsKey(character.GetInstanceID()))
                        return;
                    if (StateManager.Instance.ActorHAnimNextUpdateProcessing[character.GetInstanceID()])
                        return;

                    StateManager.Instance.ActorHAnimNextUpdateProcessing[character.GetInstanceID()] = true;

                    Actor actor = null;
                    foreach (var a in ActionScene.Instance._actors)
                    {
                        if (a.Chara.GetInstanceID() == character.GetInstanceID())
                        {
                            actor = a;
                            break;
                        }
                    }

                    if (actor != null)
                    {
                        if (StateManager.Instance.ActorHAnimNextUpdateTimeDictionary.ContainsKey(actor.GetInstanceID()))
                        {
                            if ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) > StateManager.Instance.ActorHAnimNextUpdateTimeDictionary[actor.GetInstanceID()])
                            {
                                AddOrUpdateHAnimNextUpdateTime(actor);
                                UpdateHAnimation(actor);
                            }
                        }
                    }

                    StateManager.Instance.ActorHAnimNextUpdateProcessing[character.GetInstanceID()] = false;
                }
            }

            internal static void UpdateHAnimation(Actor actor)
            {
                System.Random rnd = new System.Random();
                int rndSexPositionResult = rnd.Next(StateManager.Instance.ActorHAnimationList[actor.GetInstanceID()].changePositionFactor);
                if (rndSexPositionResult > Settings.HChangePositionThreshold && StateManager.Instance.ConfigRandomAnimationChange)
                {
                    StartHAnimation(StateManager.Instance.ActorHGroupDictionary[actor.GetInstanceID()], false);
                }
                else
                {
                    PlayAnimationForGroup(StateManager.Instance.ActorHGroupDictionary[actor.GetInstanceID()]);
                }
            }

            internal static void RecoverActorBody(ActionScene actionScene)
            {
                if (actionScene != null)
                    foreach (var actor in actionScene._actors)
                    {
                        actor.Chara.VisibleSon = false;
                        actor.Chara.confSon = false;
                    }

            }

            internal static void RecoverAllClothesState(ActionScene actionScene)
            {
                if (actionScene != null)
                    foreach (var actor in actionScene._actors)
                        RecoverClothesState(actor.Chara);
            }

            internal static void RecoverClothesState(HAnimationGroup group)
            {
                RecoverClothesState(group.male1?.Chara);
                RecoverClothesState(group.male2?.Chara);
                RecoverClothesState(group.female1?.Chara);
                RecoverClothesState(group.female2?.Chara);
            }

            internal static void RecoverClothesState(Chara.ChaControl character)
            {
                if (character == null) return;

                if (StateManager.Instance.ActorClothesState.ContainsKey(character.GetInstanceID()))
                {
                    byte[] originalClothesState = StateManager.Instance.ActorClothesState[character.GetInstanceID()];
                    for (int i = 0; i < Math.Min(character.FileStatus.clothesState.Length, 8); i++)
                    {
                        character.FileStatus.clothesState[i] = originalClothesState[i];
                    }
                }
            }

            internal static void RemoveAllClothesState(HScene hScene)
            {
                if (ActionScene.Instance != null && !StateManager.Instance.HasRemoveClothes)
                {
                    StateManager.Instance.HasRemoveClothes = true;

                    var actorList = Util.GetActorsNotInvolvedInH(ActionScene.Instance, hScene);

                    foreach (var actor in actorList)
                    {
                        byte[] clothesState = new byte[8];
                        for (int i = 0; i < Math.Min(actor.Chara.FileStatus.clothesState.Length, 8); i++)
                        {
                            clothesState[i] = actor.Chara.FileStatus.clothesState[i];
                            actor.Chara.FileStatus.clothesState[i] = 2;
                        }

                        StateManager.Instance.ActorClothesState.Add(actor.Chara.GetInstanceID(), clothesState);

                    }
                }
            }

            internal static void ForceBlowJob(Chara.ChaControl character)
            {
                if (StateManager.Instance.ForceBlowJobTarget != null)
                {
                    if (StateManager.Instance.ForceBlowJobTarget.ContainsKey(character.GetInstanceID()))
                    {
                        var trfGlans = character.CmpBoneBody.targetEtc.trfRoot.Find(Settings.MaleGlansPath);
                        trfGlans.transform.position = StateManager.Instance.ForceBlowJobTarget[character.GetInstanceID()].position;
                    }
                }
            }

            internal static void ResetHPointForGroup(ActionScene actionScene)
            {
                if (actionScene != null && StateManager.Instance.HAnimationGroupsList != null)
                {
                    foreach (var group in StateManager.Instance.HAnimationGroupsList)
                        group.hPoint.ReInit();
                    StateManager.Instance.CurrentHSceneInstance.CtrlFlag.NowHPoint.ReInit();
                }
            }

            //This function is following how the functions in the original code is called
            //This is supposed to be fixing the multiple layer animation problem but currently it is not working properly
            internal static void HandleHAnimationCtrlsUpdate(Chara.ChaControl character)
            {
                if (ActionScene.Instance != null && StateManager.Instance.CurrentHSceneInstance != null)
                {

                    if (StateManager.Instance.ActorHAnimationList != null)
                    {

                        Actor actor = Util.GetActorByChaControlID(character.GetInstanceID());
                        if (actor == null) return;

                        if (StateManager.Instance.ActorHAnimationList.ContainsKey(actor.GetInstanceID()))
                        {
                            var animInfo = StateManager.Instance.ActorHAnimationList[actor.GetInstanceID()];

                            ////This loop cause the layer animation not working
                            //for (int i = 0; i < character.CmpBoneBody.dynamicBonesBustAndHip.Count; i++)
                            //{
                            //    character.CmpBoneBody.EnableDynamicBonesBustAndHip(true, i);
                            //}


                            if (StateManager.Instance.CharacterHLayerCtrlDictionary != null)
                            {
                                if (StateManager.Instance.CharacterHLayerCtrlDictionary.ContainsKey(character.GetInstanceID()))
                                {
                                    var layerCtrl = StateManager.Instance.CharacterHLayerCtrlDictionary[character.GetInstanceID()];
                                    if (character.Sex == 1)
                                        layerCtrl.setLayer(layerCtrl.chaFemales, 1);
                                    else
                                        layerCtrl.setLayer(layerCtrl.chaMales, 0);
                                }
                            }


                            var extraLayer = GetExtraLayerID(character, animInfo.characterType, animInfo.clipType);
                            if (extraLayer != 0)
                            {
                                for (int i = 1; i < character._animBody.layerCount; i++)
                                {
                                    if (extraLayer != i)
                                        character.SetLayerWeight(0, i);
                                    else
                                        character.SetLayerWeight(1, i);
                                }
                                character.PlaySync(character.GetAnimatorStateInfo(0), extraLayer);
                            }

                            if (StateManager.Instance.CharacterCollisionCtrlDictionary != null)
                            {
                                if (StateManager.Instance.CharacterCollisionCtrlDictionary.ContainsKey(character.GetInstanceID()))
                                    StateManager.Instance.CharacterCollisionCtrlDictionary[character.GetInstanceID()].Proc(character.GetAnimatorStateInfo(0));
                            }

                            character.SetAnimatorParamFloat(Constant.AnimatorParamHeight, animInfo.height);
                            character.SetAnimatorParamFloat(Constant.AnimatorParamSpeed, animInfo.speed);
                            character.SetAnimatorParamFloat(Constant.AnimatorParamMotion, animInfo.motion);
                            character.SetAnimatorParamFloat(Constant.AnimatorParamBreast, animInfo.breast);
                            if (StateManager.Instance.CharacterDynamicBoneCtrlDictionary != null)
                            {
                                if (StateManager.Instance.CharacterDynamicBoneCtrlDictionary.ContainsKey(character.GetInstanceID()))
                                    StateManager.Instance.CharacterDynamicBoneCtrlDictionary[character.GetInstanceID()].Proc();
                            }
                        }


                    }

                }
            }

            internal static int GetExtraLayerID(Chara.ChaControl character, Constant.HCharacterType characterType, string clipType)
            {
                if (StateManager.Instance.CharacterHLayerCtrlDictionary != null)
                {
                    if (StateManager.Instance.CharacterHLayerCtrlDictionary.ContainsKey(character.GetInstanceID()))
                    {
                        int index = (characterType == Constant.HCharacterType.Female1 || characterType == Constant.HCharacterType.Male1) ? 0 : 1;
                        var layerCtrl = StateManager.Instance.CharacterHLayerCtrlDictionary[character.GetInstanceID()];

                        if (layerCtrl.LayerInfos.ContainsKey(character.Sex))
                        {
                            if (layerCtrl.LayerInfos[character.Sex][index].ContainsKey(clipType))
                            {
                                return layerCtrl.LayerInfos[character.Sex][index][clipType].LayerID;
                            }
                        }
                    }
                }

                return 0;
            }

            private static List<HPoint> GetRelatedHPoints(HPoint point)
            {
                List<HPoint> relatedHPoints = new List<HPoint>();
                foreach (var ap in Manager.Game.ActionMap.APTContainer._actionPoints)
                {
                    //if the HPoint hit any nonHPointLink of an Action Point, include all nonHPointLink to the result list
                    bool isHit = false;
                    for (int i = 0; i < ap._nonHPointLink.Count; i++)
                        if (ap._nonHPointLink[i].GetInstanceID() == point.GetInstanceID())
                        {
                            isHit = true;
                            break;
                        }
                    if (isHit)
                        for (int i = 0; i < ap._nonHPointLink.Count; i++)
                            relatedHPoints.Add(ap._nonHPointLink[i]);
                }
                if(!relatedHPoints.Contains(point))
                    relatedHPoints.Add(point);

                return relatedHPoints;
            }

            internal static void UpdateMobPoint()
            {
                if (ActionScene.Instance != null && StateManager.Instance.GroupSelection != null)
                {
                    StateManager.Instance.CurrentHSceneInstance.HPointCtrl.mobActorPoints.Clear();

                    //1. Exclude the HPoint occupied by mob NPC
                    if (Manager.Game.ActionMap.MobPTContainer != null)
                        if (Manager.Game.ActionMap.MobPTContainer._allMobPointDic != null)
                            foreach (var kvpMP in Manager.Game.ActionMap.MobPTContainer._allMobPointDic)
                                if (kvpMP.Value._mobChaControl != null)
                                    if (kvpMP.Value.NonHPointLink != null)
                                        for (int i = 0; i < kvpMP.Value.NonHPointLink.Count; i++)
                                            if (!StateManager.Instance.CurrentHSceneInstance.HPointCtrl.mobActorPoints.Contains(kvpMP.Value.NonHPointLink[i]))
                                                StateManager.Instance.CurrentHSceneInstance.HPointCtrl.mobActorPoints.Add(kvpMP.Value.NonHPointLink[i]);

                    //2. Exclude the HPoint occupied by main Scene if we are moving the H reaction group
                    if (StateManager.Instance.GroupSelection.SelectedGroup != null)
                    {
                        var pointsToAdd = GetRelatedHPoints(StateManager.Instance.MainSceneHPoint);
                        foreach (var point in pointsToAdd)
                            if (!StateManager.Instance.CurrentHSceneInstance.HPointCtrl.mobActorPoints.Contains(point))
                                StateManager.Instance.CurrentHSceneInstance.HPointCtrl.mobActorPoints.Add(point);
                    }

                    //3. Exclude the HPoint occupied by other non moving groups
                    foreach (var group in StateManager.Instance.ActorHGroupDictionary)
                        if (StateManager.Instance.GroupSelection.SelectedGroup != group.Value && group.Value.hPoint != null)
                        {
                            var pointsToAdd = GetRelatedHPoints(group.Value.hPoint);
                            foreach (var point in pointsToAdd)
                                if (!StateManager.Instance.CurrentHSceneInstance.HPointCtrl.mobActorPoints.Contains(point))
                                    StateManager.Instance.CurrentHSceneInstance.HPointCtrl.mobActorPoints.Add(point);
                        }

                    //4. Exclude the HPoint occupied by remaining single actor
                    foreach (var actor in StateManager.Instance.SingleActorList)
                    {
                        var ap = actor.OccupiedActionPoint;
                        if (ap == null)
                            ap = actor.Partner?.OccupiedActionPoint;
                        if (ap == null)
                            ap = actor.ThreesomeTarget?.OccupiedActionPoint;

                        if (ap != null)
                            foreach (var point in ap.NonHPointLink)
                                if (!StateManager.Instance.CurrentHSceneInstance.HPointCtrl.mobActorPoints.Contains(point))
                                    StateManager.Instance.CurrentHSceneInstance.HPointCtrl.mobActorPoints.Add(point);
                    }
                }
            }

            private static void InitHAnimationCtrlForGroup(HAnimationGroup group)
            {
                InitHAnimationCtrl(group.female1, null, Constant.HCharacterType.Female1, group.situationType);
                if (group.male1 != null)
                    InitHAnimationCtrl(group.male1, group.female1, Constant.HCharacterType.Male1, group.situationType);
                if (group.female2 != null)
                    InitHAnimationCtrl(group.female2, group.female1, Constant.HCharacterType.Female2, group.situationType);
                //TODOL need check the case for threesome
                if (group.male2 != null)
                    InitHAnimationCtrl(group.male2, group.female1, Constant.HCharacterType.Male2, group.situationType);
            }

            private static void InitHAnimationCtrl(Actor actor, Actor partnerActor, Constant.HCharacterType actorType, HAnimation.SituationType situationType)
            {
                Actor femaleActor = actor;
                if (actor.Sex == 0)
                    femaleActor = partnerActor;

                CollisionCtrl collisionCtrl = new CollisionCtrl();
                string hitHeadPath = string.Format(Settings.FemaleHitHeadPathFormat, femaleActor.Chara.FileFace.headId);

                //Based on the head object it seems the path name may changed with different character customization. To play safe locate the object by checking the format of the name
                var t = femaleActor.Chara.transform.Find(Settings.FemaleHitBodyTopPath);

                GameObject objFemaleHitBody = null;
                for (int i = 0; i < t.childCount; i++)
                {
                    var trfChild = t.GetChild(i);
                    if (trfChild.name.StartsWith(Settings.FemaleHitBodyPathSearchStart) && trfChild.name.EndsWith(Settings.FemaleHitBodyPathSearchEnd))
                    {
                        objFemaleHitBody = trfChild.gameObject;
                        break;
                    }
                }

                collisionCtrl.Init(femaleActor.Chara, femaleActor.Chara.transform.Find(hitHeadPath).gameObject, objFemaleHitBody);

                actor.Chara.ReSetupDynamicBone();

                HitObjectCtrl hitObjectCtrl = new HitObjectCtrl();
                hitObjectCtrl.HitObjInit(actor.Sex, actor.Chara.ObjBodyBone, actor.Chara);

                hitObjectCtrl.SetActiveObject(false);

                YureCtrl yureCtrl = new YureCtrl();
                yureCtrl.Init();

                yureCtrl.sex = actor.Sex;
                if (situationType == HAnimation.SituationType.MF)
                    yureCtrl.numID = 0;
                else
                    yureCtrl.numID = (int)actorType;
                yureCtrl.SetChaControl(actor.Chara);


                if (actor.Sex == 1)
                {
                    DynamicBoneReferenceCtrl boneCtrl = new DynamicBoneReferenceCtrl();
                    boneCtrl.Init(actor.Chara);
                    if(!StateManager.Instance.CharacterDynamicBoneCtrlDictionary.ContainsKey(actor.Chara.GetInstanceID()))
                        StateManager.Instance.CharacterDynamicBoneCtrlDictionary.Add(actor.Chara.GetInstanceID(), boneCtrl);
                    else
                        StateManager.Instance.CharacterDynamicBoneCtrlDictionary[actor.Chara.GetInstanceID()] = boneCtrl;
                }

                if(!StateManager.Instance.CharacterCollisionCtrlDictionary.ContainsKey(actor.Chara.GetInstanceID()))
                    StateManager.Instance.CharacterCollisionCtrlDictionary.Add(actor.Chara.GetInstanceID(), collisionCtrl);
                else
                    StateManager.Instance.CharacterCollisionCtrlDictionary[actor.Chara.GetInstanceID()] = collisionCtrl;

                if (!StateManager.Instance.CharacterHitObjectCtrlDictionary.ContainsKey(actor.Chara.GetInstanceID()))
                    StateManager.Instance.CharacterHitObjectCtrlDictionary.Add(actor.Chara.GetInstanceID(), hitObjectCtrl);
                else
                    StateManager.Instance.CharacterHitObjectCtrlDictionary[actor.Chara.GetInstanceID()] = hitObjectCtrl;

                if (!StateManager.Instance.CharacterYureCtrlDictionary.ContainsKey(actor.Chara.GetInstanceID()))
                    StateManager.Instance.CharacterYureCtrlDictionary.Add(actor.Chara.GetInstanceID(), yureCtrl);
                else
                    StateManager.Instance.CharacterYureCtrlDictionary[actor.Chara.GetInstanceID()] = yureCtrl;
                

            }

            private static void InitHItemCtrlForGroup(HAnimationGroup group)
            {
                HItemCtrl itemCtrl = new HItemCtrl();
                itemCtrl.HItemInit(Manager.HSceneManager.Instance.transform.Find(Settings.HItemPath));

                if (!StateManager.Instance.CharacterHItemCtrlDictionary.ContainsKey(group.female1.Chara.GetInstanceID()))
                    StateManager.Instance.CharacterHItemCtrlDictionary.Add(group.female1.Chara.GetInstanceID(), itemCtrl);
                else
                    StateManager.Instance.CharacterHItemCtrlDictionary[group.female1.Chara.GetInstanceID()] = itemCtrl;


            }

            private static void InitHLayerCtrlForGroup(HAnimationGroup group)
            {
                Il2CppReferenceArray<Chara.ChaControl> maleList = new Il2CppReferenceArray<Chara.ChaControl>(2);
                Il2CppReferenceArray<Chara.ChaControl> femaleList = new Il2CppReferenceArray<Chara.ChaControl>(2);
                AddCharacterToArrayListByType(group.male1?.Chara, maleList, femaleList, Constant.HCharacterType.Male1);
                AddCharacterToArrayListByType(group.male2?.Chara, maleList, femaleList, Constant.HCharacterType.Male2);
                AddCharacterToArrayListByType(group.female1?.Chara, maleList, femaleList, Constant.HCharacterType.Female1);
                AddCharacterToArrayListByType(group.female2?.Chara, maleList, femaleList, Constant.HCharacterType.Female2);

                HLayerCtrl layerCtrl = new HLayerCtrl();
                layerCtrl.Init(femaleList, maleList);
                layerCtrl.ctrlFlag = new HSceneFlagCtrl();
                layerCtrl.ctrlFlag.Motions = new Il2CppStructArray<float>(2);

                if(!StateManager.Instance.CharacterHLayerCtrlDictionary.ContainsKey(group.female1.Chara.GetInstanceID()))
                    StateManager.Instance.CharacterHLayerCtrlDictionary.Add(group.female1.Chara.GetInstanceID(), layerCtrl);
                else
                    StateManager.Instance.CharacterHLayerCtrlDictionary[group.female1.Chara.GetInstanceID()] = layerCtrl;
                if (group.male1 != null)
                    if (!StateManager.Instance.CharacterHLayerCtrlDictionary.ContainsKey(group.male1.Chara.GetInstanceID()))
                        StateManager.Instance.CharacterHLayerCtrlDictionary.Add(group.male1.Chara.GetInstanceID(), layerCtrl);
                    else
                        StateManager.Instance.CharacterHLayerCtrlDictionary[group.male1.Chara.GetInstanceID()] = layerCtrl;

                if (group.male2 != null)
                    if (!StateManager.Instance.CharacterHLayerCtrlDictionary.ContainsKey(group.male2.Chara.GetInstanceID()))
                        StateManager.Instance.CharacterHLayerCtrlDictionary.Add(group.male2.Chara.GetInstanceID(), layerCtrl);
                    else
                        StateManager.Instance.CharacterHLayerCtrlDictionary[group.male2.Chara.GetInstanceID()] = layerCtrl;
                
                if (group.female2 != null)
                    if (!StateManager.Instance.CharacterHLayerCtrlDictionary.ContainsKey(group.female2.Chara.GetInstanceID()))
                        StateManager.Instance.CharacterHLayerCtrlDictionary.Add(group.female2.Chara.GetInstanceID(), layerCtrl);
                    else
                        StateManager.Instance.CharacterHLayerCtrlDictionary[group.female2.Chara.GetInstanceID()] = layerCtrl;

                
            }

            private static void AddCharacterToArrayListByType(Chara.ChaControl character, Il2CppReferenceArray<Chara.ChaControl> maleList, Il2CppReferenceArray<Chara.ChaControl> femaleList, Constant.HCharacterType characterType)
            {
                if (characterType == Constant.HCharacterType.Female1)
                    femaleList[0] = character;
                else if (characterType == Constant.HCharacterType.Female2)
                    femaleList[1] = character;
                else if (characterType == Constant.HCharacterType.Male1)
                    maleList[0] = character;
                else if (characterType == Constant.HCharacterType.Male2)
                    maleList[1] = character;
            }

            private static void InitBodyBoneForGroup(HAnimationGroup group)
            {
                InitBodyBone(group.male1);
                InitBodyBone(group.male2);
                InitBodyBone(group.female1);
                InitBodyBone(group.female2);
            }

            private static void InitBodyBone(Actor actor)
            {
                if (actor != null)
                {
                    actor.Chara.ReSetupDynamicBone();
                    StateManager.Instance.CurrentHSceneInstance.DynamicBoneChangePtnBustAndHip(actor.Chara, 2);
                    actor.Chara.LoadHitObject();
                }
            }


            private static void SetBodyParts(Actor actor, Actor partnerActor, HAnimation.ActorHAnimData animData)
            {
                var animInfo = animData.animationListInfo;
                var characterType = animData.characterType;
                var clipType = animData.clipType;

                int animGroup = Util.GetHAnimationGroup(animInfo);
                var extraInfo = HAnimation.ExtraHAnimationDataDictionary[(animGroup, animInfo.ID)];

                //Show the penis if necessary
                ForcePenisVisible(actor.Chara, animInfo.MaleSon == 1);

                //reset state
                actor.Chara.ChangeTongueState(0);
                actor.Chara.ChangeMouthFixed(false);
                actor.Chara.ChangeMouthPtn((int)HAnimation.MouthType.Common);

                //facial expression
                HAnimation.MouthType mouthType = HAnimation.MouthType.Common;
                if (characterType == Constant.HCharacterType.Female1)
                {
                    mouthType = extraInfo.mouthTypeFemale1[clipType];
                    actor.Chara.ChangeEyebrowPtn(extraInfo.eyebrowPtnFemale1[clipType]);
                    actor.Chara.ChangeEyesOpenMax(extraInfo.eyeOpenMaxFemale1[clipType]);
                    actor.Chara.ChangeEyesPtn(extraInfo.eyePtnFemale1[clipType]);
                }
                else if (characterType == Constant.HCharacterType.Female2)
                {
                    mouthType = extraInfo.mouthTypeFemale2[clipType];
                    actor.Chara.ChangeEyebrowPtn(extraInfo.eyebrowPtnFemale2[clipType]);
                    actor.Chara.ChangeEyesOpenMax(extraInfo.eyeOpenMaxFemale2[clipType]);
                    actor.Chara.ChangeEyesPtn(extraInfo.eyePtnFemale2[clipType]);
                }
                else if (characterType == Constant.HCharacterType.Male1)
                {
                    mouthType = extraInfo.mouthTypeMale1[clipType];
                    actor.Chara.ChangeEyebrowPtn(extraInfo.eyebrowPtnMale1[clipType]);
                    actor.Chara.ChangeEyesOpenMax(extraInfo.eyeOpenMaxMale1[clipType]);
                    actor.Chara.ChangeEyesPtn(extraInfo.eyePtnMale1[clipType]);
                }
                else if (characterType == Constant.HCharacterType.Male2)
                {
                    mouthType = extraInfo.mouthTypeMale2[clipType];
                    actor.Chara.ChangeEyebrowPtn(extraInfo.eyebrowPtnMale2[clipType]);
                    actor.Chara.ChangeEyesOpenMax(extraInfo.eyeOpenMaxMale2[clipType]);
                    actor.Chara.ChangeEyesPtn(extraInfo.eyePtnMale2[clipType]);
                }

                //mouth
                if (mouthType == HAnimation.MouthType.BlowJob)
                {
                    actor.Chara.ChangeMouthPtn((int)HAnimation.MouthType.BlowJob);

                    //need to force updating the dick head location
                    if (!StateManager.Instance.ForceBlowJobTarget.ContainsKey(partnerActor.Chara.GetInstanceID()))
                        StateManager.Instance.ForceBlowJobTarget.Add(partnerActor.Chara.GetInstanceID(), actor.Chara.CmpBoneHead.targetEtc.trfMouthAdjustWidth.Find("cf_J_MouthLow"));
                }
                else
                {
                    StateManager.Instance.ForceBlowJobTarget.Remove(partnerActor.Chara.GetInstanceID());
                    actor.Chara.ChangeMouthPtn((int)mouthType);

                    if (mouthType == HAnimation.MouthType.Lick)
                    {
                        actor.Chara.ChangeTongueState(1);
                    }

                }

                //neck and eye direction
                var bkInfo = StateManager.Instance.ActorBackUpData[actor.GetInstanceID()];
                actor.Chara.ChangeLookEyesTarget(bkInfo.lookEyePtn, partnerActor.Chara.ObjEyesLookTarget.transform);
                actor.Chara.ChangeLookNeckTarget(bkInfo.lookNeckPtn, partnerActor.Chara.ObjNeckLookTarget.transform);

                actor.Chara.ChangeLookEyesPtn(bkInfo.lookEyePtn);
                actor.Chara.ChangeLookNeckPtn(bkInfo.lookNeckPtn);
            }

            //// Not using, the variable faceInfo and HScene is required. Need further study 
            //private static void SetEyeNeckCtrlMF(Actor maleActor, Actor femaleActor)
            //{
            //    HMotionEyeNeckFemale femaleNeckMotion = new HMotionEyeNeckFemale();
            //    femaleNeckMotion.Init(femaleActor.Chara, 0, StateManager.Instance.CurrentHSceneInstance);
            //    femaleNeckMotion.SetPartner(maleActor.Chara._objBodyBone, null, null);
            //    femaleNeckMotion.Load("list/h/neckcontrol/", "neck_rgh_f_20");

            //    HVoiceCtrl.FaceInfo faceInfo = new HVoiceCtrl.FaceInfo();
            //    faceInfo.OpenEye = 0.9f;
            //    faceInfo.OpenMouthMax = 1;
            //    faceInfo.OpenMouthMin = 1;
            //    faceInfo.EyeBlow = 5;
            //    faceInfo.Eye = 0;
            //    faceInfo.Mouth = 13;
            //    faceInfo.Tear = 0;
            //    faceInfo.Cheek = 0.2f;
            //    faceInfo.Highlight = true;
            //    faceInfo.Blink = true;
            //    faceInfo.BehaviorEyeLine = 0;
            //    faceInfo.BehaviorNeckLine = 0;
            //    faceInfo.TargetNeckLine = 0;
            //    faceInfo.TargetEyeLine = 0;
            //    faceInfo.NeckRot = new UnhollowerBaseLib.Il2CppStructArray<Vector3>(2);
            //    faceInfo.NeckRot[0] = Vector3.zero;
            //    faceInfo.NeckRot[1] = Vector3.zero;
            //    faceInfo.HeadRot = new UnhollowerBaseLib.Il2CppStructArray<Vector3>(2);
            //    faceInfo.HeadRot[0] = Vector3.zero;
            //    faceInfo.HeadRot[1] = Vector3.zero;
            //    faceInfo.EyeRot = new UnhollowerBaseLib.Il2CppStructArray<Vector3>(2);
            //    faceInfo.EyeRot[0] = Vector3.zero;
            //    faceInfo.EyeRot[1] = Vector3.zero;

            //    //femaleNeckMotion.

            //    femaleNeckMotion.Proc(femaleActor.Chara.GetAnimatorStateInfo(0), faceInfo, 0);
            //    //femaleNeckMotion.SetPartnerMaleObj(maleActor.Chara._objBodyBone, null);
            //    //femaleNeckMotion.SetPartnerFemaleObj(null);

            //}

            private static void SetMotionIKsSingle(Actor actor, HAnimationGroup group, Dictionary<int, MotionIK> dictMotionIK, TextAsset textAsset)
            {
                if (actor == null) return;

                MotionIK motionIK = dictMotionIK[actor.GetInstanceID()];

                var hAnimData = StateManager.Instance.ActorHAnimationList[actor.GetInstanceID()];

                motionIK.IKBaseOverride(hAnimData.animationListInfo.IKBaseOverride);

                Il2CppReferenceArray<MotionIK> dummy = new Il2CppReferenceArray<MotionIK>(0);
                motionIK.SetPartners(dummy);

                motionIK.InitFrameCalc(true);

                motionIK.Reset();
                motionIK.Release();

                Il2CppReferenceArray<MotionIK> arr = new Il2CppReferenceArray<MotionIK>(dictMotionIK.Count);
                int counter = 0;
                if (group.male1 != null)
                    arr[counter++] = dictMotionIK[group.male1.GetInstanceID()];
                if (group.male2 != null)
                    arr[counter++] = dictMotionIK[group.male2.GetInstanceID()];
                arr[counter++] = dictMotionIK[group.female1.GetInstanceID()];
                if (group.female2 != null)
                    arr[counter++] = dictMotionIK[group.female2.GetInstanceID()];

                motionIK.SetPartners(arr);

                motionIK.IKBaseOverride(hAnimData.animationListInfo.IKBaseOverride);

                motionIK.Reset();
                motionIK.Release();

                motionIK.LoadData(textAsset);

                var itemObjs = GetItemObjectsArrayForGroup(group);
                motionIK.SetItems(itemObjs);

                motionIK.Reset();

                motionIK.InfoSex = actor.Status.BodySex;
            }

            private static Dictionary<int, MotionIK> InitMotionIKDict(HAnimationGroup group)
            {
                Dictionary<int, MotionIK> result = new Dictionary<int, MotionIK>();

                MotionIK motionIKActorF1 = new MotionIK(group.female1.Chara);
                result.Add(group.female1.GetInstanceID(), motionIKActorF1);
                if (group.male1 != null)
                {
                    MotionIK motionIKActorM1 = new MotionIK(group.male1.Chara);
                    result.Add(group.male1.GetInstanceID(), motionIKActorM1);
                }
                if (group.male2 != null)
                {
                    MotionIK motionIKActorM2 = new MotionIK(group.male2.Chara);
                    result.Add(group.male2.GetInstanceID(), motionIKActorM2);
                }
                if (group.female2 != null)
                {
                    MotionIK motionIKActorF2 = new MotionIK(group.female2.Chara);
                    result.Add(group.female2.GetInstanceID(), motionIKActorF2);
                }

                return result;
            }

            private static void SetMotionIKsForGroup(HAnimationGroup group, string clipNameType)
            {
                var hAnimData = StateManager.Instance.ActorHAnimationList[group.female1.GetInstanceID()];

                var assetPathSplit = hAnimData.animationListInfo.AssetpathFemale.Split('/');

                string path = Util.GetAssetBundleBasePath() + Manager.HSceneManager.Instance.StrAssetIKListFolder + string.Format(Settings.HMotionIKAssetBundleFileName, assetPathSplit[assetPathSplit.Length - 2]);

                AssetBundle ab = AssetBundle.LoadFromFile(path);
                if (ab != null)
                {
                    string assetNameMale1 = GetMotionIKAssetName(hAnimData.animationListInfo, group.situationType, Constant.HCharacterType.Male1);
                    string assetNameMale2 = GetMotionIKAssetName(hAnimData.animationListInfo, group.situationType, Constant.HCharacterType.Male2);
                    string assetNameFemale1 = GetMotionIKAssetName(hAnimData.animationListInfo, group.situationType, Constant.HCharacterType.Female1);
                    string assetNameFemale2 = GetMotionIKAssetName(hAnimData.animationListInfo, group.situationType, Constant.HCharacterType.Female2);

                    TextAsset assetM1 = LoadMotionIKTextAsset(ab, assetNameMale1);
                    TextAsset assetM2 = LoadMotionIKTextAsset(ab, assetNameMale2);
                    TextAsset assetF1 = LoadMotionIKTextAsset(ab, assetNameFemale1);
                    TextAsset assetF2 = LoadMotionIKTextAsset(ab, assetNameFemale2);

                    var dictMotionIK = InitMotionIKDict(group);

                    if (hAnimData.situationType == HAnimation.SituationType.FF)
                    {
                        //Swap between F1 and F2. I dont know why but this work...
                        TextAsset temp = assetF1;
                        assetF1 = assetF2;
                        assetF2 = temp;
                    }

                    SetMotionIKsSingle(group.male1, group, dictMotionIK, assetM1);
                    SetMotionIKsSingle(group.male2, group, dictMotionIK, assetM2);
                    SetMotionIKsSingle(group.female1, group, dictMotionIK, assetF1);
                    SetMotionIKsSingle(group.female2, group, dictMotionIK, assetF2);

                    foreach (var kvpIK in dictMotionIK)
                    {
                        kvpIK.Value.Calc(clipNameType);
                    }

                    ab.Unload(false);
                }
            }

            internal static Il2CppReferenceArray<GameObject> GetItemObjectsArrayForGroup(HAnimationGroup group)
            {
                Actor targetActor = group.female1;

                Il2CppReferenceArray<GameObject> result = new Il2CppReferenceArray<GameObject>(StateManager.Instance.CharacterHItemCtrlDictionary[targetActor.Chara.GetInstanceID()].itemObj.Count);
                for (int i = 0; i < StateManager.Instance.CharacterHItemCtrlDictionary[targetActor.Chara.GetInstanceID()].itemObj.Count; i++)
                {
                    result[i] = StateManager.Instance.CharacterHItemCtrlDictionary[targetActor.Chara.GetInstanceID()].itemObj[i].Item1.gameObject;
                }
                return result;
            }

            //Not sure if required or not
            private static AnimationCurve HardcodeAnimationCurve()
            {
                AnimationCurve result = new AnimationCurve();

                Keyframe k1 = new Keyframe();
                k1.time = 0;
                k1.value = 0;
                k1.inTangent = 0.3333333f;
                k1.outTangent = 0.3333333f;
                k1.inWeight = 0;
                k1.outWeight = 0.5219253f;
                k1.weightedMode = WeightedMode.None;
                k1.tangentMode = 0;
                k1.tangentModeInternal = 0;

                Keyframe k2 = new Keyframe();
                k2.time = 0.4310961f;
                k2.value = 0.1676099f;
                k2.inTangent = 1.062808f;
                k2.outTangent = 1.062808f;
                k2.inWeight = 0.3333333f;
                k2.outWeight = 0.3333333f;
                k2.weightedMode = WeightedMode.None;
                k2.tangentMode = 0;
                k2.tangentModeInternal = 0;

                Keyframe k3 = new Keyframe();
                k3.time = 0.6520407f;
                k3.value = 0.7711853f;
                k3.inTangent = 1.965922f;
                k3.outTangent = 1.965922f;
                k3.inWeight = 0.3333333f;
                k3.outWeight = 0.3333333f;
                k3.weightedMode = WeightedMode.None;
                k3.tangentMode = 0;
                k3.tangentModeInternal = 0;

                Keyframe k4 = new Keyframe();
                k4.time = 1f;
                k4.value = 1f;
                k4.inTangent = 0f;
                k4.outTangent = 0f;
                k4.inWeight = 0f;
                k4.outWeight = 0f;
                k4.weightedMode = WeightedMode.None;
                k4.tangentMode = 0;
                k4.tangentModeInternal = 0;

                result.keys.AddItem(k1);
                result.keys.AddItem(k2);
                result.keys.AddItem(k3);
                result.keys.AddItem(k4);

                result.preWrapMode = WrapMode.ClampForever;
                result.postWrapMode = WrapMode.ClampForever;
                return result;
            }

            private static void SetupCtrl(Actor actor, Constant.HCharacterType characterType, string clipType)
            {
                if (actor == null) return;

                var hAnimData = StateManager.Instance.ActorHAnimationList[actor.GetInstanceID()];
                int animGroup = Util.GetHAnimationGroup(hAnimData.animationListInfo);

                var collisionCtrl = StateManager.Instance.CharacterCollisionCtrlDictionary[actor.Chara.GetInstanceID()];
                var hitObjCtrl = StateManager.Instance.CharacterHitObjectCtrlDictionary[actor.Chara.GetInstanceID()];
                var yureCtrl = StateManager.Instance.CharacterYureCtrlDictionary[actor.Chara.GetInstanceID()];
                DynamicBoneReferenceCtrl boneCtrl = null;
                if (actor.Sex == 1)
                    boneCtrl = StateManager.Instance.CharacterDynamicBoneCtrlDictionary[actor.Chara.GetInstanceID()];
                var layerCtrl = StateManager.Instance.CharacterHLayerCtrlDictionary[actor.Chara.GetInstanceID()];

                string fileName = "";
                if (characterType == Constant.HCharacterType.Female1)
                    fileName = hAnimData.animationListInfo.FileFemale;
                else if (characterType == Constant.HCharacterType.Female2)
                    fileName = hAnimData.animationListInfo.FileFemale2;
                else if (characterType == Constant.HCharacterType.Male1)
                    fileName = hAnimData.animationListInfo.FileMale;
                else if (characterType == Constant.HCharacterType.Male2)
                    fileName = hAnimData.animationListInfo.FileMale2;

                layerCtrl.LoadExcel(fileName, actor.Sex, (characterType == Constant.HCharacterType.Female1 || characterType == Constant.HCharacterType.Male1) ? 0 : 1);
                layerCtrl.ctrlFlag.NowAnimationInfo = hAnimData.animationListInfo;
                layerCtrl.ctrlFlag.ChangeMotionCurve = HardcodeAnimationCurve();
                layerCtrl.ctrlFlag.NowHPoint = StateManager.Instance.CharacterHPointDictionary[actor.Chara.GetInstanceID()];
                layerCtrl.ctrlFlag.Start();

                yureCtrl.Load(hAnimData.animationListInfo.ID, animGroup);
                collisionCtrl.LoadExcel(fileName);
                hitObjCtrl.HitObjLoadExcel(fileName);

                hitObjCtrl.Proc(clipType);
                yureCtrl.Proc(clipType);

                if (boneCtrl != null)
                {
                    boneCtrl.Load(Manager.HSceneManager.Instance.StrAssetDynamicBoneListFolder, fileName);
                }

                actor.Chara.CmpBoneBody.ResetDynamicBonesBustAndHip();
                actor.Chara.ResetDynamicBoneALL();
                actor.Chara.ReSetupDynamicBone();
            }

            private static void SetHLayerForGroup(HAnimationGroup group)
            {
                Il2CppReferenceArray<Chara.ChaControl> maleList = new Il2CppReferenceArray<Chara.ChaControl>(2);
                Il2CppReferenceArray<Chara.ChaControl> femaleList = new Il2CppReferenceArray<Chara.ChaControl>(2);
                AddCharacterToArrayListByType(group.male1?.Chara, maleList, femaleList, Constant.HCharacterType.Male1);
                AddCharacterToArrayListByType(group.male2?.Chara, maleList, femaleList, Constant.HCharacterType.Male2);
                AddCharacterToArrayListByType(group.female1?.Chara, maleList, femaleList, Constant.HCharacterType.Female1);
                AddCharacterToArrayListByType(group.female2?.Chara, maleList, femaleList, Constant.HCharacterType.Female2);

                var layerCtrl = StateManager.Instance.CharacterHLayerCtrlDictionary[group.female1.Chara.GetInstanceID()];
                layerCtrl.setLayer(maleList, 0);
                layerCtrl.setLayer(femaleList, 1);
            }

            private static TextAsset LoadMotionIKTextAsset(AssetBundle ab, string assetName)
            {
                if (assetName.Trim() != "")
                {
                    return ab.LoadAsset(assetName, UnhollowerRuntimeLib.Il2CppType.From(typeof(TextAsset))).Cast<TextAsset>();
                }

                return null;
            }

            private static string GetMotionIKAssetName(HScene.AnimationListInfo animInfo, HAnimation.SituationType situationType, Constant.HCharacterType characterType)
            {
                if (situationType == HAnimation.SituationType.MF || situationType == HAnimation.SituationType.FF)
                {
                    switch (characterType)
                    {
                        case Constant.HCharacterType.Female1:
                            return animInfo.FileFemale;
                        case Constant.HCharacterType.Female2:
                            return animInfo.FileFemale2;
                        case Constant.HCharacterType.Male1:
                            return animInfo.FileMale;
                    }
                }
                else if (situationType == HAnimation.SituationType.MMF)
                {
                    switch (characterType)
                    {
                        case Constant.HCharacterType.Female1:
                            return string.Format(Settings.HMotionIKAssetFormatThreesome.MMF_Female, animInfo.ID);
                        case Constant.HCharacterType.Male1:
                            return string.Format(Settings.HMotionIKAssetFormatThreesome.MMF_Male1, animInfo.ID);
                        case Constant.HCharacterType.Male2:
                            return string.Format(Settings.HMotionIKAssetFormatThreesome.MMF_Male2, animInfo.ID);
                    }
                }
                else if (situationType == HAnimation.SituationType.FFM)
                {
                    var nameSplit = animInfo.FileFemale.Split('_');
                    string nameNumber = nameSplit[nameSplit.Length - 1];

                    if (characterType == Constant.HCharacterType.Male1)
                    {
                        return string.Format(Settings.HMotionIKAssetFormatThreesome.FFM_Male, nameNumber, animInfo.ReverseTaii ? "02" : "01");

                    }
                    else if ((characterType == Constant.HCharacterType.Female1 && !animInfo.ReverseTaii) || (characterType == Constant.HCharacterType.Female2 && animInfo.ReverseTaii))
                    {
                        return string.Format(Settings.HMotionIKAssetFormatThreesome.FFM_Female1, nameNumber);
                    }
                    else if ((characterType == Constant.HCharacterType.Female2 && !animInfo.ReverseTaii) || (characterType == Constant.HCharacterType.Female1 && animInfo.ReverseTaii))
                    {
                        return string.Format(Settings.HMotionIKAssetFormatThreesome.FFM_Female2, nameNumber);
                    }
                }



                return "";
            }

            private static void AddActorHAnimationDataForGroup(HAnimationGroup group, HScene.AnimationListInfo animInfo)
            {
                AddActorHAnimationData(group.male1, animInfo, group.situationType, Constant.HCharacterType.Male1);
                AddActorHAnimationData(group.male2, animInfo, group.situationType, Constant.HCharacterType.Male2);
                AddActorHAnimationData(group.female1, animInfo, group.situationType, Constant.HCharacterType.Female1);
                AddActorHAnimationData(group.female2, animInfo, group.situationType, Constant.HCharacterType.Female2);
            }

            private static void AddActorHAnimationData(Actor actor, HScene.AnimationListInfo animInfo, HAnimation.SituationType situationType, Constant.HCharacterType characterType)
            {
                if (actor == null) return;

                HAnimation.ActorHAnimData data = new HAnimation.ActorHAnimData();
                data.animationListInfo = animInfo;
                data.situationType = situationType;
                data.characterType = characterType;
                data.changePositionFactor = 0;

                if (!StateManager.Instance.ActorHAnimationList.ContainsKey(actor.GetInstanceID()))
                    StateManager.Instance.ActorHAnimationList.Add(actor.GetInstanceID(), data);
                else
                    StateManager.Instance.ActorHAnimationList[actor.GetInstanceID()] = data;
            }


            private static void AddOrUpdateHAnimNextUpdateTime(Actor actor)
            {
                System.Random rnd = new System.Random();
                int rndResult = rnd.Next(Settings.HActionRandomMilliSecond);
                long nextUpdateTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) + (rndResult + StateManager.Instance.ConfigAnimationChangeInterval);

                if (StateManager.Instance.ActorHAnimNextUpdateTimeDictionary.ContainsKey(actor.GetInstanceID()))
                    StateManager.Instance.ActorHAnimNextUpdateTimeDictionary[actor.GetInstanceID()] = nextUpdateTime;
                else
                    StateManager.Instance.ActorHAnimNextUpdateTimeDictionary.Add(actor.GetInstanceID(), nextUpdateTime);
            }

            private static void PlayAnimationForGroup(HAnimationGroup group)
            {
                //Update the animation clip
                string clipNameType = GetRandomAnimationClipName();

                //Update the animation speed
                System.Random rnd = new System.Random();
                float speed = 1 + rnd.Next(1000) / 1000f;

                //var animInfo = StateManager.Instance.ActorHAnimationList[group.female1.GetInstanceID()];
                foreach (var kvpInfo in StateManager.Instance.ActorHAnimationList)
                    kvpInfo.Value.changePositionFactor++;


                SetupCtrlForGroup(group, clipNameType);

                SetHLayerForGroup(group);

                SetCharacterPlayAnimationForGroup(group, clipNameType);

                SetAnimInfoParamForGroup(group, clipNameType, speed);

                SetBodyPartsForGroup(group);

                SetItemCtrlForGroup(group, clipNameType);

                SetMotionIKsForGroup(group, clipNameType);

                SetVoiceToActorForGroup(group, clipNameType);

                //Handle the case of move point button is clicked
                if (StateManager.Instance.CurrentHSceneInstance.CtrlFlag.IsPointMoving)//is moving
                {
                    if (group == StateManager.Instance.GroupSelection.SelectedGroup)
                    {
                        //group moving, should be temporary not visible
                        MenuItems.SetAnimationGroupCharactersVisible(group, false);
                    }
                }

            }

            private static void SetupCtrlForGroup(HAnimationGroup group, string clipNameType)
            {
                SetupCtrl(group.male1, Constant.HCharacterType.Male1, clipNameType);
                SetupCtrl(group.male2, Constant.HCharacterType.Male2, clipNameType);
                SetupCtrl(group.female1, Constant.HCharacterType.Female1, clipNameType);
                SetupCtrl(group.female2, Constant.HCharacterType.Female1, clipNameType);
            }

            private static void SetCharacterPlayAnimationForGroup(HAnimationGroup group, string clipNameType)
            {
                SetCharacterPlayAnimation(group.male1?.Chara, clipNameType);
                SetCharacterPlayAnimation(group.male2?.Chara, clipNameType);
                SetCharacterPlayAnimation(group.female1?.Chara, clipNameType);
                SetCharacterPlayAnimation(group.female2?.Chara, clipNameType);
            }

            private static void SetAnimInfoParamForGroup(HAnimationGroup group, string clipNameType, float speed)
            {
                if (group.situationType == HAnimation.SituationType.MF)
                {
                    SetAnimInfoParam(StateManager.Instance.ActorHAnimationList[group.male1.GetInstanceID()], group.female1, speed, clipNameType);
                    SetAnimInfoParam(StateManager.Instance.ActorHAnimationList[group.female1.GetInstanceID()], group.female1, speed, clipNameType);
                }
                else if (group.situationType == HAnimation.SituationType.FF)
                {
                    SetAnimInfoParam(StateManager.Instance.ActorHAnimationList[group.female1.GetInstanceID()], group.female1, speed, clipNameType);
                    SetAnimInfoParam(StateManager.Instance.ActorHAnimationList[group.female2.GetInstanceID()], group.female2, speed, clipNameType);
                }
                else if (group.situationType == HAnimation.SituationType.FFM)
                {
                    var animInfo = StateManager.Instance.ActorHAnimationList[group.male1.GetInstanceID()].animationListInfo;
                    SetAnimInfoParam(StateManager.Instance.ActorHAnimationList[group.female1.GetInstanceID()], group.female1, speed, clipNameType);
                    SetAnimInfoParam(StateManager.Instance.ActorHAnimationList[group.female2.GetInstanceID()], group.female2, speed, clipNameType);
                    SetAnimInfoParam(StateManager.Instance.ActorHAnimationList[group.male1.GetInstanceID()], animInfo.ReverseTaii ? group.female2 : group.female1, speed, clipNameType);
                }
                else if (group.situationType == HAnimation.SituationType.MMF)
                {
                    SetAnimInfoParam(StateManager.Instance.ActorHAnimationList[group.male1.GetInstanceID()], group.female1, speed, clipNameType);
                    SetAnimInfoParam(StateManager.Instance.ActorHAnimationList[group.male2.GetInstanceID()], group.female1, speed, clipNameType);
                    SetAnimInfoParam(StateManager.Instance.ActorHAnimationList[group.female1.GetInstanceID()], group.female1, speed, clipNameType);
                }
            }

            private static void SetBodyPartsForGroup(HAnimationGroup group)
            {
                if (group.situationType == HAnimation.SituationType.MF)
                {
                    SetBodyParts(group.male1, group.female1, StateManager.Instance.ActorHAnimationList[group.male1.GetInstanceID()]);
                    SetBodyParts(group.female1, group.male1, StateManager.Instance.ActorHAnimationList[group.female1.GetInstanceID()]);
                }
                else if (group.situationType == HAnimation.SituationType.FF)
                {
                    SetBodyParts(group.female1, group.female2, StateManager.Instance.ActorHAnimationList[group.female1.GetInstanceID()]);
                    SetBodyParts(group.female2, group.female1, StateManager.Instance.ActorHAnimationList[group.female2.GetInstanceID()]);
                }
                else if (group.situationType == HAnimation.SituationType.FFM)
                {
                    var animInfo = StateManager.Instance.ActorHAnimationList[group.male1.GetInstanceID()].animationListInfo;
                    SetBodyParts(group.female1, group.male1, StateManager.Instance.ActorHAnimationList[group.female1.GetInstanceID()]);
                    SetBodyParts(group.female2, group.male1, StateManager.Instance.ActorHAnimationList[group.female2.GetInstanceID()]);
                    SetBodyParts(group.male1, animInfo.ReverseTaii ? group.female2 : group.female1, StateManager.Instance.ActorHAnimationList[group.male1.GetInstanceID()]);
                }
                else if (group.situationType == HAnimation.SituationType.MMF)
                {
                    var animInfo = StateManager.Instance.ActorHAnimationList[group.female1.GetInstanceID()].animationListInfo;
                    var clipType = StateManager.Instance.ActorHAnimationList[group.female1.GetInstanceID()].clipType;
                    var targetCharType = HAnimation.ExtraHAnimationDataDictionary[(HAnimation.CategoryMMF, animInfo.ID)].mmfFemale1Target[clipType];

                    //special handling for MMF case as the female have 2 blow job target
                    StateManager.Instance.ForceBlowJobTarget.Remove(group.male1.Chara.GetInstanceID());
                    StateManager.Instance.ForceBlowJobTarget.Remove(group.male2.Chara.GetInstanceID());

                    SetBodyParts(group.male1, group.female1, StateManager.Instance.ActorHAnimationList[group.male1.GetInstanceID()]);
                    SetBodyParts(group.male2, group.female1, StateManager.Instance.ActorHAnimationList[group.male2.GetInstanceID()]);
                    SetBodyParts(group.female1, targetCharType == Constant.HCharacterType.Male1 ? group.male1 : group.male2, StateManager.Instance.ActorHAnimationList[group.female1.GetInstanceID()]);
                }
            }

            private static void SetVoiceToActorForGroup(HAnimationGroup group, string clipNameType)
            {
                //only need to handle female
                SetVoiceToActor(group.female1, clipNameType);
                SetVoiceToActor(group.female2, clipNameType);
            }

            private static void SetAnimInfoParam(HAnimation.ActorHAnimData animInfo, Actor actor, float speed, string clipType)
            {
                animInfo.height = actor.Chara.GetShapeBodyValue((int)Chara.ChaFileDefine.BodyShapeIdx.Height);
                animInfo.breast = actor.Chara.GetShapeBodyValue((int)Chara.ChaFileDefine.BodyShapeIdx.BustSize);
                animInfo.motion = 1;
                animInfo.speed = speed;
                animInfo.clipType = clipType;
            }

            private static void SetItemCtrlForGroup(HAnimationGroup group, string clipNameType)
            {
                HItemCtrl itemCtrl = StateManager.Instance.CharacterHItemCtrlDictionary[group.female1.Chara.GetInstanceID()];
                HPoint hPoint = StateManager.Instance.CharacterHPointDictionary[group.female1.Chara.GetInstanceID()];

                var hAnimData = StateManager.Instance.ActorHAnimationList[group.female1.GetInstanceID()];
                var animInfo = hAnimData.animationListInfo;

                itemCtrl.SetTransform(hPoint.transform.position, hPoint.transform.rotation.eulerAngles);
                itemCtrl.ReleaseItem();
                itemCtrl._hFlag = StateManager.Instance.CharacterHLayerCtrlDictionary[group.female1.Chara.GetInstanceID()].ctrlFlag;

                if (hAnimData.animationListInfo.IsNeedItem)
                {
                    int animGroup = Util.GetHAnimationGroup(animInfo);
                    var extraInfo = HAnimation.ExtraHAnimationDataDictionary[(animGroup, animInfo.ID)];
                    itemCtrl.LoadItem(animGroup, animInfo.ID,
                        group.male1?.Chara.ObjBodyBone,
                        group.female1.Chara.ObjBodyBone,
                        group.male2?.Chara.ObjBodyBone,
                        group.female2?.Chara.ObjBodyBone,
                        GetBashoKind(hPoint)
                        );

                    //For unknown reason the object orientation for some position may be wrong, fix it here
                    if (extraInfo.isItemInverse)
                    {
                        foreach (var item in itemCtrl._dicItem)
                        {
                            item.Value.ObjItem.transform.rotation = item.Value.ObjItem.transform.rotation * Quaternion.AngleAxis(180, Vector3.up);
                        }
                    }
                    itemCtrl.SetAnimatorParamFloat(Constant.AnimatorParamHeight, hAnimData.height);
                    itemCtrl.SetAnimatorParamFloat(Constant.AnimatorParamSpeed, hAnimData.speed);
                }

                itemCtrl.SetPlay(clipNameType);


            }





            private static void InitHPoint(HPoint hPoint)
            {
                SetHPointObjectVisible(hPoint, true);
            }

            internal static void SetHPointObjectVisible(HPoint hPoint, bool isVisible)
            {
                hPoint.ChangeHideProcBefore();
                hPoint.ChangeHideProcAll();
                hPoint.HpointObjVisibleChange(true);

                if (isVisible)
                {
                    hPoint.SetOffset();
                }
                else
                {
                    hPoint.ReInit();
                }
            }

            private static int GetBashoKind(HPoint hPoint)
            {
                int result = -1;
                foreach (var kvp in StateManager.Instance.FullHPointListInMap)
                {
                    foreach (var point in kvp.Value.HPoints)
                    {
                        if (point.GetInstanceID() == hPoint.GetInstanceID())
                        {
                            result = kvp.Key;
                            break;
                        }
                    }
                }
                return result;
            }

            private static HScene.AnimationListInfo GetHAnimation(HPoint hPoint, HAnimation.SituationType situationType)
            {
                int hPointType = GetHPointType(hPoint);
                var possibleAnimList = GetAvailableHAnimationList(hPoint, hPointType, situationType);

                System.Random rnd = new System.Random();
                int rndResult = rnd.Next(possibleAnimList.Count);
                return possibleAnimList[rndResult];
            }

            internal static int GetHPointType(HPoint hPoint)
            {
                var listHPoint = StateManager.Instance.FullHPointListInMap;
                foreach (var kvp in listHPoint)
                {
                    foreach (var pt in kvp.Value.HPoints)
                    {
                        if (pt.GetInstanceID() == hPoint.GetInstanceID())
                            return kvp.Key;
                    }
                }    
                return -1;
            }

            internal static List<HScene.AnimationListInfo> GetAvailableHAnimationList(HPoint hPoint, int type, HAnimation.SituationType situationType)
            {
                List<HScene.AnimationListInfo> result = new List<HScene.AnimationListInfo>();

                bool isMainSceneGroup = true;
                if (StateManager.Instance.GroupSelection != null)
                    isMainSceneGroup = StateManager.Instance.GroupSelection.SelectedGroup == null;

                for (int i = 0; i < HPoint._animationLists.Count; i++)
                {
                    foreach (var info in HPoint._animationLists[i])
                    {
                        bool hPointExcludeMotion = false;
                        if (i < hPoint._limitData.Motion.Count)                                                                                 //the array only have length=6 while that of animation list is length=7...
                            hPointExcludeMotion = hPoint._limitData.Motion[i].MotionID.Contains(info.ID);

                        if (info.LstPositons.Contains(type)                                                                                     //check if the type of this HPoint is available for this animation
                            && (info.LimitMap[0] == -1 || info.LimitMap.Contains(ActionScene.Instance.MapID))                                   //check if the animation is available for current map
                            && (!HAnimation.IsInExcludeList(i, info.ID) || isMainSceneGroup)                                                    //check if the animation is excluded by this mod due to technical issue etc
                            && !hPointExcludeMotion                                                                                             //check if the animation is excluded by this HPoint)
                            && HAnimation.ExtraHAnimationDataDictionary[(i, info.ID)].situationType == situationType                            //check if the animation is available for current situation type 
                            && (!info.NotEventUnuse || (info.Event.Contains(StateManager.Instance.CurrentHSceneInstance._sprite._eventNo)))     //check if event animation allowed
                        )
                        {
                            result.Add(info);
                        }
                    }
                }

                return result;
            }

            private static void SetActorToHPointForGroup(HAnimationGroup group, HPoint hPoint, HScene.AnimationListInfo animInfo)
            {
                SetActorToHPoint(group.male1, hPoint, animInfo, Constant.HCharacterType.Male1);
                SetActorToHPoint(group.male2, hPoint, animInfo, Constant.HCharacterType.Male2);
                SetActorToHPoint(group.female1, hPoint, animInfo, Constant.HCharacterType.Female1);
                SetActorToHPoint(group.female2, hPoint, animInfo, Constant.HCharacterType.Female2);
            }

            private static void SetActorToHPoint(Actor actor, HPoint hPoint, HScene.AnimationListInfo animInfo, Constant.HCharacterType characterType)
            {
                if (actor == null) return;

                int animGroup = Util.GetHAnimationGroup(animInfo);
                var extraInfo = HAnimation.ExtraHAnimationDataDictionary[(animGroup, animInfo.ID)];

                actor.transform.position = hPoint.transform.position + extraInfo.offsetVector;
                actor.transform.localPosition = hPoint.transform.localPosition + extraInfo.offsetVector;
                actor.transform.rotation = hPoint.transform.rotation;
                actor.transform.localRotation = hPoint.transform.localRotation;

                actor.Chara.transform.localRotation = Quaternion.identity;
                actor.Chara.transform.localPosition = Vector3.zero;

                if ((characterType == Constant.HCharacterType.Female1 && extraInfo.isFemale1Inverse)
                    || (characterType == Constant.HCharacterType.Female2 && extraInfo.isFemale2Inverse)
                    || (characterType == Constant.HCharacterType.Male1 && extraInfo.isMale1Inverse)
                    || (characterType == Constant.HCharacterType.Male2 && extraInfo.isMale2Inverse)
                    )
                {
                    actor.transform.rotation = actor.transform.rotation * Quaternion.AngleAxis(180, Vector3.up);
                }

                if (!StateManager.Instance.CharacterHPointDictionary.ContainsKey(actor.Chara.GetInstanceID()))
                    StateManager.Instance.CharacterHPointDictionary.Add(actor.Chara.GetInstanceID(), hPoint);
                else
                    StateManager.Instance.CharacterHPointDictionary[actor.Chara.GetInstanceID()] = hPoint;
            }

            private static void SetActorAnimatorForGroup(HAnimationGroup group, HScene.AnimationListInfo animInfo)
            {
                SetActorAnimator(group.male1, animInfo, Constant.HCharacterType.Male1);
                SetActorAnimator(group.male2, animInfo, Constant.HCharacterType.Male2);
                SetActorAnimator(group.female1, animInfo, Constant.HCharacterType.Female1);
                SetActorAnimator(group.female2, animInfo, Constant.HCharacterType.Female2);
            }

            private static void SetActorAnimator(Actor actor, HScene.AnimationListInfo animInfo, Constant.HCharacterType characterType)
            {
                if (actor == null) return;

                Chara.ChaControl character = actor.Chara;
                string abName = "";
                string assetName = "";
                string baseABName = "";
                string baseAssetName = "";

                if (characterType == Constant.HCharacterType.Female1)
                {
                    abName = animInfo.AssetpathFemale;
                    assetName = animInfo.FileFemale;
                    baseABName = animInfo.AssetpathBaseF;
                    baseAssetName = animInfo.AssetBaseF;
                }
                else if (characterType == Constant.HCharacterType.Female2)
                {
                    abName = animInfo.AssetpathFemale2;
                    assetName = animInfo.FileFemale2;
                    baseABName = animInfo.AssetpathBaseF2;
                    baseAssetName = animInfo.AssetBaseF2;
                }
                else if (characterType == Constant.HCharacterType.Male1)
                {
                    abName = animInfo.AssetpathMale;
                    assetName = animInfo.FileMale;
                    baseABName = animInfo.AssetpathBaseM;
                    baseAssetName = animInfo.AssetBaseM;
                }
                else if (characterType == Constant.HCharacterType.Male2)
                {
                    abName = animInfo.AssetpathMale2;
                    assetName = animInfo.FileMale2;
                    baseABName = animInfo.AssetpathBaseM2;
                    baseAssetName = animInfo.AssetBaseM2;
                }

                //the path of base animator is always in "00" if not mentioned
                if (baseABName == "")
                {
                    string[] abNameSplit = abName.Split('/');
                    baseABName = abNameSplit[0];
                    for (int i = 1; i < abNameSplit.Length; i++)
                    {
                        if (i != abNameSplit.Length - 2)
                            baseABName += "/" + abNameSplit[i];
                        else
                            baseABName += "/00";
                    }
                }

                if (baseAssetName == "")
                {
                    baseAssetName = assetName.Substring(0, assetName.Length - 2) + "base";
                }

                var rac = character.LoadAnimation(abName, assetName);

                var racBase = character.LoadAnimation(baseABName, baseAssetName);
                var racResult = Illusion.Unity.Utils.Animator.SetupAnimatorOverrideController(racBase, rac);
                actor.Animation.SetAnimatorController(racResult);
            }

            private static void SetCharacterPlayAnimation(Chara.ChaControl character, string clipName)
            {
                if (character == null) return;

                character.PlaySync(clipName, 0, 0);
            }

            private static string GetRandomAnimationClipName()
            {
                System.Random rnd = new System.Random();
                int rndResult = rnd.Next(Settings.ValidPlayableHClipType.Length);
                return Settings.ValidPlayableHClipType[rndResult];
            }

            private static void SetActorClothesStateForGroup(HAnimationGroup group, HScene.AnimationListInfo animInfo)
            {
                SetActorClothesState(group.male1?.Chara, animInfo, Constant.HCharacterType.Male1);
                SetActorClothesState(group.male2?.Chara, animInfo, Constant.HCharacterType.Male2);
                SetActorClothesState(group.female1?.Chara, animInfo, Constant.HCharacterType.Female1);
                SetActorClothesState(group.female2?.Chara, animInfo, Constant.HCharacterType.Female2);
            }

            private static void SetActorClothesState(Chara.ChaControl character, HScene.AnimationListInfo animInfo, Constant.HCharacterType characterType)
            {
                if (character == null) return;

                //apply the clothes state per animation info
                int index = -1;
                if (characterType == Constant.HCharacterType.Female1)
                    index = 0;
                else if (characterType == Constant.HCharacterType.Female2)
                    index = 1;

                if (index != -1)
                {
                    character.FileStatus.clothesState[0] = Math.Max((byte)animInfo.FemaleUpperCloths[index], character.FileStatus.clothesState[0]);
                    character.FileStatus.clothesState[2] = Math.Max((byte)animInfo.FemaleUpperCloths[index], character.FileStatus.clothesState[2]);
                    character.FileStatus.clothesState[1] = Math.Max((byte)animInfo.FemaleLowerCloths[index], character.FileStatus.clothesState[1]);
                    character.FileStatus.clothesState[3] = Math.Max((byte)animInfo.FemaleLowerCloths[index], character.FileStatus.clothesState[3]);
                }
            }


            internal static void ForcePenisVisible(Chara.ChaControl character, bool isShowPenis)
            {
                if (character == null) return;

                if (character.Sex == 0 || character.FileParam.futanari)
                {
                    if (isShowPenis)
                    {

                        character.confSon = true;
                        character.VisibleSon = true;

                        character.CmpBody.targetEtc.objDanSao.active = true;
                        character.CmpBody.targetEtc.objDanTama.active = true;
                        character.CmpBody.targetEtc.objDanTop.active = true;

                        if (!StateManager.Instance.ForceActiveInstanceID.Contains(character.CmpBody.targetEtc.objDanSao.GetInstanceID()))
                            StateManager.Instance.ForceActiveInstanceID.Add(character.CmpBody.targetEtc.objDanSao.GetInstanceID());
                        if (!StateManager.Instance.ForceActiveInstanceID.Contains(character.CmpBody.targetEtc.objDanTama.GetInstanceID()))
                            StateManager.Instance.ForceActiveInstanceID.Add(character.CmpBody.targetEtc.objDanTama.GetInstanceID());
                        if (!StateManager.Instance.ForceActiveInstanceID.Contains(character.CmpBody.targetEtc.objDanTop.GetInstanceID()))
                            StateManager.Instance.ForceActiveInstanceID.Add(character.CmpBody.targetEtc.objDanTop.GetInstanceID());
                    }
                    else
                    {
                        StateManager.Instance.ForceActiveInstanceID.Remove(character.CmpBody.targetEtc.objDanSao.GetInstanceID());
                        StateManager.Instance.ForceActiveInstanceID.Remove(character.CmpBody.targetEtc.objDanTama.GetInstanceID());
                        StateManager.Instance.ForceActiveInstanceID.Remove(character.CmpBody.targetEtc.objDanTop.GetInstanceID());
                    }
                }
            }

            private static void SetVoiceToActor(Actor actor, string clipName)
            {
                if (actor == null) return;

                if (actor.Sex != 1)
                    return;

                var extraInfo = GetExtraHAnimationDataForActor(actor);
                var characterType = StateManager.Instance.ActorHAnimationList[actor.GetInstanceID()].characterType;
                HVoice.HVoiceType targetType;
                if (characterType == Constant.HCharacterType.Female1)
                    targetType = extraInfo.female1VoiceType;
                else
                    targetType = extraInfo.female2VoiceType;

                var voiceDataList = HVoice.HVoiceDictionary[(actor.Chara.FileParam.personality, targetType, clipName)];

                System.Random rnd = new System.Random();
                int rndResult = rnd.Next(voiceDataList.Count);

                Manager.Voice.Loader loader = new Manager.Voice.Loader();
                loader.Bundle = voiceDataList[rndResult].assetBundle;
                loader.Asset = voiceDataList[rndResult].asset;

                loader.No = actor.Chara.FileParam.personality;
                loader.Pitch = actor.Chara.FileParam.voicePitch;
                loader.SettingNo = -1;
                loader.VoiceTrans = actor.Chara.CmpBoneBody.targetEtc.trfHeadParent;

                var audioSource = Manager.Voice.Play(loader);
                audioSource.maxDistance = Settings.HVoiceMaxDistance;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.loop = true;
                if (actor.Chara.ASVoice != null)
                    actor.Chara.ASVoice.Stop();

                actor.Chara.SetVoiceTransform(audioSource);
            }

            private static HAnimation.ExtraHAnimationData GetExtraHAnimationDataForActor(Actor actor)
            {
                var animInfo = StateManager.Instance.ActorHAnimationList[actor.GetInstanceID()].animationListInfo;
                int animInfoGroup = Util.GetHAnimationGroup(animInfo);
                return HAnimation.ExtraHAnimationDataDictionary[(animInfoGroup, animInfo.ID)];
            }

            private static void GetHCharacterType(Actor actor1, Actor actor2, out Constant.HCharacterType actor1Type, out Constant.HCharacterType actor2Type)
            {
                if (actor1.Sex == 0)
                {
                    actor1Type = Constant.HCharacterType.Male1;
                    actor2Type = Constant.HCharacterType.Female1;
                }
                else if (actor2.Sex == 0)
                {
                    actor1Type = Constant.HCharacterType.Female1;
                    actor2Type = Constant.HCharacterType.Male1;
                }
                else
                {
                    actor1Type = Constant.HCharacterType.Female1;
                    actor2Type = Constant.HCharacterType.Female2;
                }
            }

            //index 0: male1, 1: male2, 2: female1, 3, female2
            private static Actor[] ConvertActorsToArray(Actor actor1, Actor actor2, Actor actor3 = null, Actor actor4 = null)
            {
                Actor[] arrActors = new Actor[4];
                AddActorToActorArray(arrActors, actor1);
                AddActorToActorArray(arrActors, actor2);
                AddActorToActorArray(arrActors, actor3);
                AddActorToActorArray(arrActors, actor4);

                return arrActors;
            }

            private static void AddActorToActorArray(Actor[] arr, Actor actor)
            {
                if (actor == null) return;

                if (actor.Sex == 0)
                {
                    if (arr[0] == null)
                        arr[0] = actor;
                    else
                        arr[1] = actor;
                }
                else
                {
                    if (arr[2] == null)
                        arr[2] = actor;
                    else
                        arr[3] = actor;
                }
            }

            private static HAnimation.SituationType GetSituationType(Actor actor1, Actor actor2)
            {
                if (actor1.Sex != actor2.Sex)
                    return HAnimation.SituationType.MF;
                else
                    return HAnimation.SituationType.FF;
            }


        }
    }
}
