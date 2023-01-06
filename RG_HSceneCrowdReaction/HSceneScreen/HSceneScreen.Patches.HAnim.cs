using BepInEx.Logging;
using HarmonyLib;
using RG.Scene;
using UnityEngine;
using RG.Scene.Action.Core;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using HSceneCrowdReaction.InfoList;
using System;

namespace HSceneCrowdReaction.HSceneScreen
{
    internal partial class Patches
    {
        internal class HAnim
        {
            private static ManualLogSource Log = HSceneCrowdReactionPlugin.Log;

            internal static bool IsHActionPossible(Actor actor)
            {
                if (actor.Partner == null)                           //block if not in pair now
                    return false;

                if (actor.Sex == 0 && actor.Partner.Sex == 0)       //block if both male
                    return false;
                /*
                if (!Util.CheckHasEverSex(actor, actor.Partner))    //block if the pair does not have sex before
                    return false;
                */
                return true;
            }


            //Only handle 2 person H at this time
            internal static void StartHAnimation(Actor actor)
            {
                Actor partnerActor = actor.Partner;
                Log.LogInfo("pt1");
                //actor.Chara.LoadHitObject();
                //partnerActor.Chara.LoadHitObject();
                Log.LogInfo("pt2");
                //Get the HPoint
                HPoint hPoint = GetHPoint(actor);
                Log.LogInfo("pt3");
                Log.LogInfo("actor: " + actor.Status.FullName + " and " + partnerActor.Status.FullName + ", hPoint: " + hPoint.name);

                //Get the chosen animation
                //TODO: uncomment this
                ////var animInfo = GetHAnimation(hPoint);

                //TODO: for test only!!
                //var animInfo = HPoint._animationLists[0][0];
                var animInfo = Manager.HSceneManager.HResourceTables.LstAnimInfo[0][3];

                //Setup HPoint
                InitHPoint(hPoint, animInfo);
                Log.LogInfo("pt4");
                //Assign the actor to the HPoint
                SetActorToHPoint(actor, hPoint);
                SetActorToHPoint(partnerActor, hPoint);
                Log.LogInfo("Name: " + actor.Status.FullName);

                Log.LogInfo("partnerActor: " + actor.Status.FullName);

                Log.LogInfo("pt5");
                //Set the animation of the actor

                /*
                
                if (actor.Sex == 0)
                {
                    actorType = Constant.HCharacterType.Male1;
                    partnerType = Constant.HCharacterType.Female1;
                }
                else if (partnerActor.Sex == 0)
                {
                    actorType = Constant.HCharacterType.Female1;
                    partnerType = Constant.HCharacterType.Male1;
                }
                else
                {
                    actorType = Constant.HCharacterType.Female1;
                    partnerType = Constant.HCharacterType.Female2;
                }*/
                Constant.HCharacterType actorType, partnerType;
                GetHCharacterType(actor, partnerActor, out actorType, out partnerType);
                Log.LogInfo("actor: " + actor.Status.FullName + " and " + partnerActor.Status.FullName + ", anim: " + animInfo.NameAnimation);
                SetActorAnimator(actor, animInfo, actorType);
                SetActorAnimator(partnerActor, animInfo, partnerType);
                Log.LogInfo("pt6");


                var situationType = GetSituationType(actor, partnerActor);
                AddActorHAnimationData(actor, animInfo, situationType);
                AddActorHAnimationData(actor.Partner, animInfo, situationType);

                //StateManager.Instance.ActorHAnimationList.Add(actor.GetInstanceID(), animInfo);
                //StateManager.Instance.ActorHAnimationList.Add(actor.Partner.GetInstanceID(), animInfo);



                //Play the animation
                PlayAnimationForActors(actor, partnerActor);
                /*
                 * string clipName = GetRandomAnimationClipName(actor);
                Log.LogInfo("ClipName: " + clipName);
                SetCharacterPlayAnimation(actor.Chara, clipName);
                SetCharacterPlayAnimation(partnerActor.Chara, clipName);
                */

                Log.LogInfo("pt7");
                //Update the clothes state of the actor

                SetActorClothesState(actor.Chara, animInfo, actorType);
                SetActorClothesState(partnerActor.Chara, animInfo, partnerType);
                Log.LogInfo("pt8");
                //Show the penis if necessary
                if (animInfo.MaleSon == 1)
                {
                    ForcePenisVisible(actor.Chara);
                    ForcePenisVisible(partnerActor.Chara);
                }
                Log.LogInfo("pt9");


                //Add only 1 actor to the next update list
                //TODO: uncomment this
                AddOrUpdateHAnimNextUpdateTime(actor);






                //Set timer to update the clip and speed at random interval

                /*
                System.Random rnd = new System.Random();
                int rndResult = rnd.Next(Settings.HActionRandomMilliSecond);
                long nextUpdateTime = DateTime.Now.ToFileTimeUtc() + (rndResult + Settings.HActionMinMilliSecond) * 100;
                

                StateManager.Instance.ActorHAnimNextUpdateTimeDictionary.Add(actor.GetInstanceID(), nextUpdateTime);
                */

                /*
                CustomTimer timer = new CustomTimer(actor, partnerActor, Settings.HActionMinMilliSecond + rndResult);
                timer.Elapsed += OnHActionUpdateEvent;
                timer.Enabled = true;
                timer.AutoReset = true;
                StateManager.Instance.HActionUpdateTimerList.Add(timer);
                */
                Log.LogInfo("pt10");
            }

            private static void motionik(Actor actor1, Actor actor2, string clipname)
            {
                Actor __instance = actor1;
                if (actor2.Sex == 1) __instance = actor2;

                string path = Application.dataPath.Replace("RoomGirl_Data", "abdata") + Manager.HSceneManager.Instance.StrAssetIKListFolder + Settings.HMotionIKAssetBundleFileName;
                AssetBundle ab = AssetBundle.LoadFromFile(path);
                if (ab != null)
                {


                    var assetF = ab.LoadAsset("rga_f_03", UnhollowerRuntimeLib.Il2CppType.From(typeof(TextAsset))).Cast<TextAsset>();
                    var assetM = ab.LoadAsset("rga_m_03", UnhollowerRuntimeLib.Il2CppType.From(typeof(TextAsset))).Cast<TextAsset>();

                    MotionIK motionIKFemale = new MotionIK(__instance.Chara);
                    MotionIK motionIKMale = new MotionIK(__instance.Partner.Chara);
                    //MotionIK motionIKFemale = __instance.Animation.MapIK;
                    //MotionIK motionIKMale = __instance.Animation.MapIK;

                    motionIKFemale.IKBaseOverride(false);
                    motionIKMale.IKBaseOverride(false);

                    UnhollowerBaseLib.Il2CppReferenceArray<MotionIK> dummy = new UnhollowerBaseLib.Il2CppReferenceArray<MotionIK>(0);
                    motionIKFemale.SetPartners(dummy);
                    motionIKMale.SetPartners(dummy);

                    motionIKFemale.InitFrameCalc(true);
                    motionIKMale.InitFrameCalc(true);

                    motionIKFemale.Reset();
                    motionIKMale.Reset();
                    motionIKFemale.Release();
                    motionIKMale.Release();

                    UnhollowerBaseLib.Il2CppReferenceArray<MotionIK> arr = new UnhollowerBaseLib.Il2CppReferenceArray<MotionIK>(2);
                    arr[0] = motionIKMale;
                    arr[1] = motionIKFemale;

                    motionIKFemale.SetPartners(arr);
                    motionIKMale.SetPartners(arr);

                    motionIKFemale.IKBaseOverride(false);
                    motionIKMale.IKBaseOverride(false);

                    motionIKFemale.Reset();
                    motionIKMale.Reset();
                    motionIKFemale.Release();
                    motionIKMale.Release();

                    motionIKFemale.LoadData(assetF);
                    motionIKMale.LoadData(assetM);

                    //motionIKMale.SetItems();
                    //motionIKFemale.SetItems();
                    motionIKFemale.Reset();
                    motionIKMale.Reset();

                    Log.LogInfo("Animation id: " + __instance.Partner.Animation.GetInstanceID());

                    Log.LogInfo("_animation id: " + __instance.Partner._animation.GetInstanceID());

                    motionIKMale.InfoSex = 0;
                    motionIKFemale.InfoSex = 1;

                    motionIKFemale.Calc(clipname.Substring(0, 5));
                    motionIKMale.Calc(clipname.Substring(0, 5));

                    //__instance.Chara.transform.position = __instance.Partner.Chara.transform.position + new Vector3(3, 3, 3);

                    Log.LogInfo("motionIKMale IK ID: " + motionIKMale.IK.GetInstanceID());
                    Log.LogInfo("__instance.Partner.Animation.IK ID: " + __instance.Partner.Animation.IK.GetInstanceID());
                    Log.LogInfo("clip name: " + clipname);

                    if (!StateManager.Instance.animCtrlIDs.Contains(__instance.Partner.Animation.GetInstanceID()))
                        StateManager.Instance.animCtrlIDs.Add(__instance.Partner.Animation.GetInstanceID());
                    /*
                    foreach(var s in motionIKFemale.StateNames)
                    {
                        Log.LogInfo("StateNames: " + s);
                    }
                    Log.LogInfo("====");
                    foreach (var s in motionIKMale.StateNames)
                    {
                        Log.LogInfo("StateNames: " + s);
                    }
                    Log.LogInfo("====");

                    foreach (var s in motionIKFemale._animatorNames)
                    {
                        Log.LogInfo("_animatorNames: " + s);
                    }
                    Log.LogInfo("====");
                    foreach (var s in motionIKMale._animatorNames)
                    {
                        Log.LogInfo("_animatorNames: " + s);
                    }
                    Log.LogInfo("====");

                    foreach (var s in motionIKFemale.Data.states)
                    {
                        Log.LogInfo("Data.states: name: " + s.name + ", nameid: " + s.nameID);
                    }
                    Log.LogInfo("====");
                    foreach (var s in motionIKMale.Data.states)
                    {
                        Log.LogInfo("Data.states: name: " + s.name + ", nameid: " + s.nameID);
                    }
                    Log.LogInfo("====");
                    */


                    //Debug.PrintDetail(__instance.Animation.StateInfo);
                    //Debug.PrintDetail(__instance.Partner.Animation);

                }
                else
                {
                    Log.LogInfo("ab null!!!");
                }
                ab.Unload(false);
            }

            internal static void CheckUpdateHAnim(Chara.ChaControl character)
            {
                if (ActionScene.Instance != null && StateManager.Instance.ActorHAnimNextUpdateTimeDictionary != null)
                {
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
                                Log.LogInfo("DateTime.Now: " + (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) + ", StateManager: " + StateManager.Instance.ActorHAnimNextUpdateTimeDictionary[actor.GetInstanceID()]);

                                AddOrUpdateHAnimNextUpdateTime(actor);
                                PlayAnimationForActors(actor, actor.Partner);
                            }
                        }
                    }


                    StateManager.Instance.ActorHAnimNextUpdateProcessing[character.GetInstanceID()] = false;
                }


            }


            private static void AddActorHAnimationData(Actor actor, HScene.AnimationListInfo animInfo, HAnimation.SituationType situationType)
            {
                HAnimation.ActorHAnimData data = new HAnimation.ActorHAnimData();
                data.animationListInfo = animInfo;
                data.situationType = situationType;
                StateManager.Instance.ActorHAnimationList.Add(actor.GetInstanceID(), data);
            }




            private static void OnHActionUpdateEvent(object sender, ElapsedEventArgs e)
            //private static void OnHActionUpdateEvent(Actor actor1, Actor actor2)
            {
                Log.LogInfo("OnHActionUpdateEvent start!");
                Actor actor1 = ((CustomTimer)sender).Actor1;
                Actor actor2 = ((CustomTimer)sender).Actor2;

                PlayAnimationForActors(actor1, actor2);

                System.Random rnd = new System.Random();
                int rndResult = rnd.Next(Settings.HActionRandomMilliSecond);
                long nextUpdateTime = DateTime.Now.ToFileTimeUtc() + rndResult;
                //Add only 1 actor to the next update list
                //StateManager.Instance.ActorHAnimNextUpdateTimeDictionary[actor1.GetInstanceID()] = nextUpdateTime;
                ((CustomTimer)sender).Interval = Settings.HActionMinMilliSecond + rndResult;

                Log.LogInfo("OnHActionUpdateEvent end!");
            }

            private static void AddOrUpdateHAnimNextUpdateTime(Actor actor)
            {
                System.Random rnd = new System.Random();
                int rndResult = rnd.Next(Settings.HActionRandomMilliSecond);
                long nextUpdateTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) + (rndResult + Settings.HActionMinMilliSecond);

                if (StateManager.Instance.ActorHAnimNextUpdateTimeDictionary.ContainsKey(actor.GetInstanceID()))
                    StateManager.Instance.ActorHAnimNextUpdateTimeDictionary[actor.GetInstanceID()] = nextUpdateTime;
                else
                    StateManager.Instance.ActorHAnimNextUpdateTimeDictionary.Add(actor.GetInstanceID(), nextUpdateTime);
            }

            private static void PlayAnimationForActors(Actor actor1, Actor actor2)
            {
                //TODO: allow change animation

                //Update the animation clip
                string clipName = GetRandomAnimationClipName(actor1);



                SetCharacterPlayAnimation(actor1.Chara, clipName);
                SetCharacterPlayAnimation(actor2.Chara, clipName);

                motionik(actor1, actor2, clipName);

                //Update the animation speed
                /*
                System.Random rnd = new System.Random();
                float speed = 1 + rnd.Next(1000) / 1000f;
                actor1.Chara.AnimBody.speed = speed;
                actor2.Chara.AnimBody.speed = speed;
                */

                //Fix the body parts to fit the animation

                /*
                Constant.HCharacterType actorType, partnerType;
                GetHCharacterType(actor1, actor2, out actorType, out partnerType);
                FixBodyParts(actor1, actor2, actorType);
                FixBodyParts(actor2, actor1, partnerType);
                */

                //TODO: uncomment this
                ////Set the voice
                //SetVoiceToActor(actor1, clipName);
                //SetVoiceToActor(actor2, clipName);
                ////SetVoiceToActor(actor1, StateManager.Instance.ActorHAnimationList[actor1.GetInstanceID()]);
                ////SetVoiceToActor(actor2, StateManager.Instance.ActorHAnimationList[actor2.GetInstanceID()]);
            }

            private static void FixBodyParts(Actor actor, Actor partnerActor, Constant.HCharacterType hCharacterType)
            {
                Log.LogInfo("FixBodyParts " + actor.Status.FullName + ", " + partnerActor.Status.FullName + ", " + hCharacterType);
                var extraInfo = GetExtraHAnimationDataForActor(actor);
                if (hCharacterType == Constant.HCharacterType.Female1)
                {
                    FixActorMouth(actor, partnerActor, extraInfo.female1.mouth);
                    FixActorHead(actor, partnerActor, extraInfo.female1.headMovement);

                    FixActorHand(actor, partnerActor, extraInfo.female1);
                }
                else if (hCharacterType == Constant.HCharacterType.Male1)
                {
                    FixActorMouth(actor, partnerActor, extraInfo.male1.mouth);
                    FixActorHead(actor, partnerActor, extraInfo.male1.headMovement);

                    FixActorHand(actor, partnerActor, extraInfo.male1);
                }
            }

            private static void FixActorHand(Actor actor, Actor partnerActor, HAnimation.ExtraHAnimationData.BodyConfig bodyConfig)
            {
                var dict = GetTargetLocationTransform(partnerActor);
                if (bodyConfig.leftHand != HAnimation.TargetLocationType.NoChange)
                {
                    if (dict.ContainsKey(bodyConfig.leftHand))
                        actor.Chara.CmpBoneBody.targetEtc.trf_k_handL_00.transform.position = dict[bodyConfig.leftHand].position;
                }

                if (bodyConfig.rightHand != HAnimation.TargetLocationType.NoChange)
                {
                    if (dict.ContainsKey(bodyConfig.rightHand))
                        actor.Chara.CmpBoneBody.targetEtc.trf_k_handR_00.transform.position = dict[bodyConfig.rightHand].position;
                }
            }

            private static Dictionary<HAnimation.TargetLocationType, Transform> GetTargetLocationTransform(Actor targetActor)
            {
                Dictionary<HAnimation.TargetLocationType, Transform> result = new Dictionary<HAnimation.TargetLocationType, Transform>();

                result.Add(HAnimation.TargetLocationType.ShoulderL, targetActor.Chara.CmpBoneBody.targetEtc.trf_k_shoulderL_00);
                result.Add(HAnimation.TargetLocationType.ShoulderR, targetActor.Chara.CmpBoneBody.targetEtc.trf_k_shoulderR_00);

                return result;
            }

            private static void FixActorMouth(Actor actor, Actor partnerActor, HAnimation.MouthType mouthType)
            {
                Log.LogInfo("FixActorMouth " + actor.Status.FullName + ", " + partnerActor.Status.FullName + ", " + mouthType);
                if (mouthType == HAnimation.MouthType.Common) return;

                if (mouthType == HAnimation.MouthType.Kiss)
                {
                    //actor.Chara.CmpFace.targetCustom.
                    actor.Chara.ChangeMouthPtn((int)mouthType);
                }
                else if (mouthType == HAnimation.MouthType.BlowJob)
                {
                    actor.Chara.ChangeMouthFixed(true);
                    actor.Chara.ChangeMouthPtn((int)mouthType);
                }
            }

            private static void FixActorHead(Actor actor, Actor partnerActor, HAnimation.TargetLocationType headMovement)
            {
                /*
                Log.LogInfo("FixActorHead " + actor.Status.FullName + ", " + partnerActor.Status.FullName + ", " + headMovement);
                Log.LogInfo("ObjEyesLookTarget : " + partnerActor.Chara.ObjEyesLookTarget.transform.name + ", " + partnerActor.Chara.ObjEyesLookTarget.transform.position + "\n"
                    + "ObjEyesLookTargetP : " + partnerActor.Chara.ObjEyesLookTargetP.transform.name + ", " + partnerActor.Chara.ObjEyesLookTargetP.transform.position + "\n"
                    + "ObjNeckLookTarget : " + partnerActor.Chara.ObjNeckLookTarget.transform.name + ", " + partnerActor.Chara.ObjNeckLookTarget.transform.position + "\n"
                    + "ObjNeckLookTargetP : " + partnerActor.Chara.ObjNeckLookTargetP.transform.name + ", " + partnerActor.Chara.ObjNeckLookTargetP.transform.position + "\n"
                    );

                if (actor.Sex == 0)
                {
                    actor.Chara.ChangeLookNeckTarget(1, partnerActor.Chara.ObjEyesLookTarget.transform);
                    actor.Chara.ChangeLookNeckPtn(1);
                }
                */
                if (headMovement == HAnimation.TargetLocationType.NoChange) return;

                if (headMovement == HAnimation.TargetLocationType.Kiss)
                {


                    //speedVector3 minorDisplacement = HAnimation.HAnimationMinorDisplacement;
                    //Vector3 mouthPosition = actor.Chara.CmpBoneHead.targetEtc.trfMouthAdjustWidth.transform.position;
                    //if (mouthPosition.x < 0) minorDisplacement.x *= -1;
                    //if (mouthPosition.y < 0) minorDisplacement.y *= -1;
                    //if (mouthPosition.z < 0) minorDisplacement.z *= -1;

                    //                male: (-0.2f, 0, 0.2f, 0.1f)
                    //female: (-0.2f, -0.3f, -0.1f, 0.1f)
                    if (actor.Sex == 0)
                    {
                        /*
                        actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.rotation = new Quaternion(
                            actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.rotation.x + -0.2f,
                            actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.rotation.y + 0,
                            actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.rotation.z + 0.2f,
                            actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.rotation.w + 0.1f);
                        */
                        Log.LogInfo("male head rotation: " + actor.Status.FullName + " " + actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.rotation);

                        if (!StateManager.Instance.isrotated.Contains(actor.GetInstanceID()))
                        {
                            actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.transform.RotateAround(actor.Chara.NeckLookCtrl.neckLookScript.boneCalcAngle.position, Vector3.forward, 15f);
                            //actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.transform.RotateAround(actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.transform.position, Vector3.left, 45f);
                            Log.LogInfo("female head rotation: " + actor.Status.FullName + " " + actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.rotation);
                            partnerActor.Chara.CmpBoneBody.targetEtc.trfHeadParent.transform.RotateAround(partnerActor.Chara.NeckLookCtrl.neckLookScript.boneCalcAngle.position, Vector3.forward, -12f);
                            StateManager.Instance.isrotated.Add(actor.GetInstanceID());

                            //actor.Chara.CmpBoneHead.targetEtc.trfMouthAdjustWidth.gameObject.active = false;
                            //partnerActor.Chara.CmpBoneHead.targetEtc.trfMouthAdjustWidth.gameObject.active = false;
                        }
                    }
                    else
                    {

                        /*
                        actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.rotation = new Quaternion(
                            actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.rotation.x + -0.2f,
                            actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.rotation.y + -0.3f,
                            actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.rotation.z + -0.1f,
                            actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.rotation.w + 0.1f);
                        */

                    }
                    /*
                    Vector3 displacement = partnerActor.Chara.CmpBoneHead.targetEtc.trfMouthAdjustWidth.transform.position - actor.Chara.CmpBoneHead.targetEtc.trfMouthAdjustWidth.transform.position;
                    Log.LogInfo("mouth transform name: " + actor.Chara.CmpBoneHead.targetEtc.trfMouthAdjustWidth.transform.name);
                    Log.LogInfo(
                        "mouthPosition: " + actor.Chara.CmpBoneHead.targetEtc.trfMouthAdjustWidth.transform.position
                        + ", partner mouth: " + partnerActor.Chara.CmpBoneHead.targetEtc.trfMouthAdjustWidth.transform.position
                        + ", displayment: " + displacement
                        + ", original neck: " + actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.transform.position
                        );
                    actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.transform.position += displacement;
                    Log.LogInfo(
                        "final neck: " + actor.Chara.CmpBoneBody.targetEtc.trfHeadParent.transform.position
                        );

                    Debug.PrintTransformTreeUpward(actor.Chara.CmpBoneHead.targetEtc.trfMouthAdjustWidth, "", "cf_J_MouthBase_tr");
                    Debug.PrintTransformTreeUpward(partnerActor.Chara.CmpBoneHead.targetEtc.trfMouthAdjustWidth, "", "cf_J_MouthBase_tr");
                    */
                }

            }


            private static HPoint GetHPoint(Actor actor)
            {
                ActionPoint targetAP = actor.OccupiedActionPoint == null ? actor.Partner.OccupiedActionPoint : actor.OccupiedActionPoint;
                System.Random rnd = new System.Random();
                int rndResult = rnd.Next(targetAP.HPointLink.Count);

                //TODO: uncomment
                //return targetAP.HPointLink[rndResult];
                return targetAP.HPointLink[0];
            }

            private static void InitHPoint(HPoint hPoint, HScene.AnimationListInfo animInfo)
            {
                hPoint.ChangeHideProcBefore();
                //hPoint.ChangeHideProc(1);
                hPoint.ChangeHideProcAll();
                hPoint.HpointObjVisibleChange(true);

                //move the object set
                if (hPoint._moveObjects != null)
                {
                    foreach (var moveObj in hPoint._moveObjects)
                    {
                        for (int i = 0; i < moveObj.OffSetInfos.Count; i++)
                            moveObj.SetOffset(i);
                    }
                }

                //TODO: uncomment this
                ////populate the moved chair for the pose
                //if (animInfo.IsNeedItem)
                //{
                //    foreach (var item in hPoint.MotionChairIDs)
                //    {
                //        if (item != 0)
                //        {
                //            var abinfo = Manager.HSceneManager.HResourceTables.DicDicMapDependItemInfo[item];
                //            var fullpath = Util.GetAssetBundlePath(abinfo.assetbundle);

                //            var ab = AssetBundle.LoadFromFile(fullpath);

                //            if (ab != null)
                //            {
                //                GameObject obj = Util.InstantiateFromBundle(ab, abinfo.asset);

                //                obj.transform.position = hPoint.transform.position;
                //                obj.transform.localPosition = hPoint.transform.localPosition;
                //                obj.transform.rotation = hPoint.transform.rotation;
                //                obj.transform.localRotation = hPoint.transform.localRotation;

                //                obj.transform.parent = StateManager.Instance.CurrentHSceneInstance.gameObject.transform;
                //            }

                //            ab.Unload(false);
                //        }
                //    }
                //}
            }

            private static HScene.AnimationListInfo GetHAnimation(HPoint hPoint)
            {
                //HPoint._animationLists
                int hPointType = GetHPointType(hPoint);
                var possibleAnimList = GetAvailableHAnimationList(hPointType);

                System.Random rnd = new System.Random();
                int rndResult = rnd.Next(possibleAnimList.Count);
                return possibleAnimList[rndResult];
            }

            private static int GetHPointType(HPoint hPoint)
            {
                var listHPoint = StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst;
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

            private static List<HScene.AnimationListInfo> GetAvailableHAnimationList(int type)
            {
                List<HScene.AnimationListInfo> result = new List<HScene.AnimationListInfo>();

                for (int i = 0; i < HPoint._animationLists.Count; i++)
                {
                    foreach (var info in HPoint._animationLists[i])
                    {
                        if (info.LstPositons.Contains(type) && !HAnimation.IsInExcludeList(i, info.ID))
                        {
                            result.Add(info);
                        }
                    }
                }

                return result;
            }

            private static int GetHAnimationGroup(HScene.AnimationListInfo animInfo)
            {

                int i = 0;

                ////TODO: uncomment
                //while(i < HPoint._animationLists.Count)
                //{
                //    foreach (var info in HPoint._animationLists[i])
                //    {
                //        if(info.ID == animInfo.ID && info.NameAnimation == animInfo.NameAnimation)
                //        {
                //            return i;
                //        }AddOrUpdateHAnimNextUpdateTime
                //    }
                //    i++;
                //}


                while (i < Manager.HSceneManager.HResourceTables.LstAnimInfo.Count)
                {
                    foreach (var info in Manager.HSceneManager.HResourceTables.LstAnimInfo[i])
                    {
                        if (info.ID == animInfo.ID && info.NameAnimation == animInfo.NameAnimation)
                        {
                            return i;
                        }
                    }
                    i++;
                }

                return -1;
            }


            internal static void SetActorToHPoint(Actor actor, HPoint hPoint)
            //private static void AttachCharacterToHPoint(Chara.ChaControl character, HPoint hPoint)
            {
                actor.transform.position = hPoint.transform.position;
                actor.transform.localPosition = hPoint.transform.localPosition;
                actor.transform.rotation = hPoint.transform.rotation;
                actor.transform.localRotation = hPoint.transform.localRotation;

                if (hPoint.MotionChairIDs.Count > 0)
                {
                    if (hPoint.MotionChairIDs[0] != 0)
                    {
                        actor.transform.rotation = Quaternion.Inverse(actor.transform.rotation);
                    }
                }
            }

            private static void SetActorAnimator(Actor actor, HScene.AnimationListInfo animInfo, Constant.HCharacterType characterType)
            {
                Chara.ChaControl character = actor.Chara;
                string abName = "";
                string assetName = "";


                if (characterType == Constant.HCharacterType.Female1)
                {
                    abName = animInfo.AssetpathFemale;
                    assetName = animInfo.FileFemale;
                }
                else if (characterType == Constant.HCharacterType.Female2)
                {
                    abName = animInfo.AssetpathFemale2;
                    assetName = animInfo.FileFemale2;
                }
                else if (characterType == Constant.HCharacterType.Male1)
                {
                    abName = animInfo.AssetpathMale;
                    assetName = animInfo.FileMale;
                }
                else if (characterType == Constant.HCharacterType.Male2)
                {
                    abName = animInfo.AssetpathMale2;
                    assetName = animInfo.FileMale2;
                }

                var rac = character.LoadAnimation(abName, assetName);
                actor.Animation.SetRegularAnimator(rac);
            }

            private static void SetCharacterPlayAnimation(Chara.ChaControl character, string clipName)
            {
                string prefix = Util.GetHeightKindAnimationPrefix(character.GetHeightKind());
                string fullClipName = prefix + clipName;
                //Log.LogInfo("Name: " + character.FileParam.fullname + ", clipname: " + fullClipName);
                character.PlaySync(fullClipName, -1, 0);
            }

            private static string GetRandomAnimationClipName(Actor actor)
            {
                List<string> list = new List<string>();

                foreach (var clip in actor.Chara.AnimBody.runtimeAnimatorController.animationClips)
                {
                    if (clip.name.Contains(Constant.HAnimClipKeyword.Loop))
                    {
                        //Remove the height kind
                        string clipname = clip.name.Substring(2);
                        //Log.LogInfo("GetRandomAnimationClipName clipname: " + clip.name + ", substring: " + clipname);
                        if (!list.Contains(clipname) && HAnimation.IsValidClipType(clipname))
                            list.Add(clipname);
                    }
                }

                System.Random rnd = new System.Random();
                int rndResult = rnd.Next(list.Count);
                return list[rndResult];
            }

            internal static void RecoverAllClothesState(ActionScene actionScene)
            {
                if(actionScene != null)
                    foreach(var actor in actionScene._actors)
                        RecoverClothesState(actor.Chara);
            }

            private static void RecoverClothesState(Chara.ChaControl character)
            {
                if (StateManager.Instance.ActorClothesState.ContainsKey(character.GetInstanceID()))
                {
                    byte[] originalClothesState = StateManager.Instance.ActorClothesState[character.GetInstanceID()];
                    for (int i = 0; i < Math.Min(character.FileStatus.clothesState.Length, 8); i++)
                    {
                        character.FileStatus.clothesState[i] = originalClothesState[i];
                    }
                }
            }

            private static void SetActorClothesState(Chara.ChaControl character, HScene.AnimationListInfo animInfo, Constant.HCharacterType characterType)
            {
                //recover the clothes state
                RecoverClothesState(character);

                //apply the clothes state per animation info
                int index = -1;
                if (characterType == Constant.HCharacterType.Female1)
                    index = 0;
                else if (characterType == Constant.HCharacterType.Female2)
                    index = 1;

                if (index != -1)
                {
                    character.FileStatus.clothesState[0] = (byte)animInfo.FemaleUpperCloths[index];
                    character.FileStatus.clothesState[2] = (byte)animInfo.FemaleUpperCloths[index];
                    character.FileStatus.clothesState[1] = (byte)animInfo.FemaleLowerCloths[index];
                    character.FileStatus.clothesState[3] = (byte)animInfo.FemaleLowerCloths[index];
                }
            }


            private static void ForcePenisVisible(Chara.ChaControl character)
            {
                if (character.Sex == 0)
                {
                    character.confSon = true;
                    character.VisibleSon = true;

                    character.CmpBody.targetEtc.objDanSao.active = true;
                    character.CmpBody.targetEtc.objDanTama.active = true;
                    character.CmpBody.targetEtc.objDanTop.active = true;

                    StateManager.Instance.ForceActiveInstanceID.Add(character.CmpBody.targetEtc.objDanSao.GetInstanceID());
                    StateManager.Instance.ForceActiveInstanceID.Add(character.CmpBody.targetEtc.objDanTama.GetInstanceID());
                    StateManager.Instance.ForceActiveInstanceID.Add(character.CmpBody.targetEtc.objDanTop.GetInstanceID());
                }
            }

            private static void SetVoiceToActor(Actor actor, string clipName)
            {

                if (actor.Sex != 1)
                    return;

                Log.LogInfo("SetVoiceToActor " + actor.Status.FullName);
                var extraInfo = GetExtraHAnimationDataForActor(actor);
                /*
                var hAnimData = StateManager.Instance.ActorHAnimationList[actor.GetInstanceID()];

                var animInfo = hAnimData.animationListInfo;
                int animInfoGroup = GetHAnimationGroup(animInfo);
                var extraInfo = HAnimation.ExtraHAnimationDataDictionary[(animInfoGroup, animInfo.ID)];
                */
                //Log.LogInfo("SetVoiceToActor currentAnimInfo.NameAnimation.Substring(2): " + clipName);
                var clipType = (HAnimation.HAnimationClipType)Enum.Parse(typeof(HAnimation.HAnimationClipType), clipName);
                //Log.LogInfo("SetVoiceToActor pt1, " + actor.Chara.FileParam.personality + ", " + extraInfo.hPosition + ", " + clipType);
                var voiceDataList = HVoice.HVoiceDictionary[(actor.Chara.FileParam.personality, extraInfo.hPosition, clipType)];
                //Log.LogInfo("SetVoiceToActor pt2");
                //if (voiceDataList == null)
                //Log.LogInfo("voiceDataList null");

                System.Random rnd = new System.Random();
                int rndResult = rnd.Next(voiceDataList.Count);
                //Log.LogInfo("SetVoiceToActor pt3");


                Manager.Voice.Loader loader = new Manager.Voice.Loader();
                loader.Bundle = voiceDataList[rndResult].assetBundle;
                loader.Asset = voiceDataList[rndResult].asset;
                //Log.LogInfo("SetVoiceToActor pt4");
                loader.No = actor.Chara.FileParam.personality;
                loader.Pitch = actor.Chara.FileParam.voicePitch;
                loader.SettingNo = -1;
                loader.VoiceTrans = actor.Chara.CmpBoneBody.targetEtc.trfHeadParent;
                //Log.LogInfo("SetVoiceToActor pt5");
                //var audioSource = Manager.Voice.OncePlay(loader);
                var audioSource = Manager.Voice.Play(loader);
                audioSource.maxDistance = Settings.HVoiceMaxDistance;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.loop = true;
                if (actor.Chara.ASVoice != null)
                    actor.Chara.ASVoice.Stop();
                //Log.LogInfo("SetVoiceToActor pt6");
                actor.Chara.SetVoiceTransform(audioSource);
                //Log.LogInfo("SetVoiceToActor pt7");
                /*
                Manager.Voice.Create(Manager.Voice._transTable[actor.Chara.FileParam.personality]);

                
                //if(actor.Chara.ASVoice != null)
                //    actor.Chara.ASVoice.Stop();
                

                Log.LogInfo("SetVoiceToActor pt5");

                
                var audioSource = Manager.Voice.OncePlay(loader);
                audioSource.maxDistance = Settings.HVoiceMaxDistance;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.loop = true;
                Log.LogInfo("SetVoiceToActor pt6");
                
                actor.Chara.SetVoiceTransform(audioSource);
                */
            }

            private static HAnimation.ExtraHAnimationData GetExtraHAnimationDataForActor(Actor actor)
            {
                var animInfo = StateManager.Instance.ActorHAnimationList[actor.GetInstanceID()].animationListInfo;
                var situationType = StateManager.Instance.ActorHAnimationList[actor.GetInstanceID()].situationType;
                int animInfoGroup = GetHAnimationGroup(animInfo);
                return HAnimation.ExtraHAnimationDataDictionary[(animInfoGroup, animInfo.ID, situationType)];
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
