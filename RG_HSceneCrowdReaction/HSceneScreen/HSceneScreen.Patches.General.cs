using BepInEx.Logging;
using HarmonyLib;
using RG.Scene;
using UnityEngine;
using RG.Scene.Action.Core;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using HSceneCrowdReaction.InfoList;
using HSceneCrowdReaction.BackgroundHAnimation;

namespace HSceneCrowdReaction.HSceneScreen
{
    internal partial class Patches
    {
        internal class General
        {
            private static ManualLogSource Log = HSceneCrowdReactionPlugin.Log;

            internal static void InitHScene(HScene hScene)
            {
                if (ActionScene.Instance != null)
                {
                    StateManager.Instance.HSceneSetup = false;
                    StateManager.Instance.HasRemoveClothes = false;
                    StateManager.Instance.HotKeyState = new StateManager.HotKeyData();
                    StateManager.Instance.CustomAnimationParameter = new Dictionary<int, CustomAnimation.CustomAnimationData>();
                    StateManager.Instance.CurrentHSceneInstance = hScene;

                    StateManager.Instance.ForceActiveInstanceID = new List<int>();
                    StateManager.Instance.HActionUpdateTimerList = new List<Timer>();
                    StateManager.Instance.ActorHAnimationList = new Dictionary<int, HAnimation.ActorHAnimData>();

                    StateManager.Instance.ActorHAnimNextUpdateProcessing = new Dictionary<int, bool>();
                    foreach (var actor in ActionScene.Instance._actors)
                    {
                        StateManager.Instance.ActorHAnimNextUpdateProcessing.Add(actor.Chara.GetInstanceID(), false);
                    }
                    StateManager.Instance.ActorHAnimNextUpdateTimeDictionary = new Dictionary<int, long>();

                    StateManager.Instance.CharacterCollisionCtrlDictionary = new Dictionary<int, CollisionCtrl>();
                    StateManager.Instance.CharacterHitObjectCtrlDictionary = new Dictionary<int, HitObjectCtrl>();
                    StateManager.Instance.CharacterYureCtrlDictionary = new Dictionary<int, YureCtrl>();
                    StateManager.Instance.CharacterDynamicBoneCtrlDictionary = new Dictionary<int, DynamicBoneReferenceCtrl>();
                    StateManager.Instance.CharacterHLayerCtrlDictionary = new Dictionary<int, HLayerCtrl>();
                    StateManager.Instance.CharacterHItemCtrlDictionary = new Dictionary<int, HItemCtrl>();
                    StateManager.Instance.CharacterHPointDictionary = new Dictionary<int, HPoint>();


                    StateManager.Instance.ForceBlowJobTarget = new Dictionary<int, Transform>();

                    StateManager.Instance.ActorBackUpData = new Dictionary<int, StateManager.BackUpInformation>();
                    StateManager.Instance.ActorHGroupDictionary = new Dictionary<int, HAnimationGroup>();

                    StateManager.Instance.HAnimationGroupsList = new List<HAnimationGroup>();

                    StateManager.Instance.HSceneDropDownSelectedToggle = null;
                    StateManager.Instance.HSceneDropDownSelectedCharaText = null;
                    StateManager.Instance.ToggleIDCharacterList = new Dictionary<int, Chara.ChaControl>();

                }
            }

            internal static void RestoreActorsLookingDirection(HScene hScene)
            {
                if (ActionScene.Instance != null)
                {
                    var actorList = Util.GetActorsNotInvolvedInH(ActionScene.Instance, hScene);
                    foreach (var actor in actorList)
                    {
                        if (StateManager.Instance.ActorBackUpData.ContainsKey(actor.GetInstanceID()))
                        {
                            var bkInfo = StateManager.Instance.ActorBackUpData[actor.GetInstanceID()];
                            actor.Chara.ChangeLookEyesTarget(bkInfo.lookEyePtn, bkInfo.lookEyeTarget);
                            actor.Chara.ChangeLookNeckTarget(bkInfo.lookNeckPtn, bkInfo.lookNeckTarget);

                            actor.Chara.ChangeLookEyesPtn(bkInfo.lookEyePtn);
                            actor.Chara.ChangeLookNeckPtn(bkInfo.lookNeckPtn);
                        }

                    }
                }
            }

            internal static void UpdateNonHActorsLookAt(HScene hScene)
            {
                if (ActionScene.Instance != null)
                {
                    DestroyTempObject();

                    List<Actor> actorList = Util.GetActorsNotInvolvedInH(ActionScene.Instance, hScene);
                    foreach (var actor in actorList)
                    {
                        if (!StateManager.Instance.ActorHAnimationList.ContainsKey(actor.GetInstanceID()))
                            ChangeActorLookingAtHScene(actor);
                    }
                }
            }

            internal static void DestroyTempObject()
            {
                if (StateManager.Instance.HeadDownLookTarget != null)
                {
                    GameObject.Destroy(StateManager.Instance.HeadDownLookTarget);
                    StateManager.Instance.HeadDownLookTarget = null;
                }

            }

            internal static void RemoveStateManagerValue()
            {
                StateManager.Instance.CurrentHSceneInstance = null;
                StateManager.Instance.HSceneDropDownSelectedToggle = null;
                StateManager.Instance.HSceneDropDownSelectedCharaText = null;
            }

            internal static void DestroyStateManagerList()
            {

                if (StateManager.Instance.HActionUpdateTimerList != null)
                {
                    foreach (var timer in StateManager.Instance.HActionUpdateTimerList)
                    {
                        if (timer != null)
                        {
                            timer.Stop();
                            timer.Dispose();
                        }
                    }
                    StateManager.Instance.HActionUpdateTimerList.Clear();
                    StateManager.Instance.HActionUpdateTimerList = null;
                }

                if (StateManager.Instance.ActorHAnimationList != null)
                {
                    StateManager.Instance.ActorHAnimationList.Clear();
                    StateManager.Instance.ActorHAnimationList = null;
                }
                if (StateManager.Instance.ForceActiveInstanceID != null)
                {
                    StateManager.Instance.ForceActiveInstanceID.Clear();
                    StateManager.Instance.ForceActiveInstanceID = null;
                }



                if (StateManager.Instance.ActorHAnimNextUpdateTimeDictionary != null)
                {
                    StateManager.Instance.ActorHAnimNextUpdateTimeDictionary.Clear();
                    StateManager.Instance.ActorHAnimNextUpdateTimeDictionary = null;
                }
                if (StateManager.Instance.ActorHAnimNextUpdateProcessing != null)
                {
                    StateManager.Instance.ActorHAnimNextUpdateProcessing.Clear();
                    StateManager.Instance.ActorHAnimNextUpdateProcessing = null;
                }
                if (StateManager.Instance.ActorClothesState != null)
                {
                    StateManager.Instance.ActorClothesState.Clear();
                }

                if (StateManager.Instance.ForceBlowJobTarget != null)
                {
                    StateManager.Instance.ForceBlowJobTarget.Clear();
                    StateManager.Instance.ForceBlowJobTarget = null;
                }

                if (StateManager.Instance.CharacterCollisionCtrlDictionary != null)
                {
                    StateManager.Instance.CharacterCollisionCtrlDictionary.Clear();
                    StateManager.Instance.CharacterCollisionCtrlDictionary = null;
                }

                if (StateManager.Instance.CharacterHitObjectCtrlDictionary != null)
                {
                    StateManager.Instance.CharacterHitObjectCtrlDictionary.Clear();
                    StateManager.Instance.CharacterHitObjectCtrlDictionary = null;
                }

                if (StateManager.Instance.CharacterYureCtrlDictionary != null)
                {
                    StateManager.Instance.CharacterYureCtrlDictionary.Clear();
                    StateManager.Instance.CharacterYureCtrlDictionary = null;
                }

                if (StateManager.Instance.CharacterDynamicBoneCtrlDictionary != null)
                {
                    StateManager.Instance.CharacterDynamicBoneCtrlDictionary.Clear();
                    StateManager.Instance.CharacterDynamicBoneCtrlDictionary = null;
                }

                if (StateManager.Instance.CharacterHLayerCtrlDictionary != null)
                {
                    StateManager.Instance.CharacterHLayerCtrlDictionary.Clear();
                    StateManager.Instance.CharacterHLayerCtrlDictionary = null;
                }

                if (StateManager.Instance.CharacterHItemCtrlDictionary != null)
                {
                    foreach (var kvp in StateManager.Instance.CharacterHItemCtrlDictionary)
                    {
                        kvp.Value.MapReleaseItem();
                    }
                    StateManager.Instance.CharacterHItemCtrlDictionary.Clear();
                    StateManager.Instance.CharacterHItemCtrlDictionary = null;
                }

                if (StateManager.Instance.CharacterHPointDictionary != null)
                {
                    StateManager.Instance.CharacterHPointDictionary.Clear();
                    StateManager.Instance.CharacterHPointDictionary = null;
                }

                if (StateManager.Instance.ActorBackUpData != null)
                {
                    StateManager.Instance.ActorBackUpData.Clear();
                    StateManager.Instance.ActorBackUpData = null;
                }

                if (StateManager.Instance.ActorHGroupDictionary != null)
                {
                    StateManager.Instance.ActorHGroupDictionary.Clear();
                    StateManager.Instance.ActorHGroupDictionary = null;
                }

                if (StateManager.Instance.HAnimationGroupsList != null)
                {
                    StateManager.Instance.HAnimationGroupsList.Clear();
                    StateManager.Instance.HAnimationGroupsList = null;
                }

                if (StateManager.Instance.ToggleIDCharacterList != null)
                {
                    StateManager.Instance.ToggleIDCharacterList.Clear();
                    StateManager.Instance.ToggleIDCharacterList = null;
                }

            }



            internal static void ChangeActorsAnimation(HScene hScene)
            {
                if (!StateManager.Instance.HSceneSetup)
                {

                    if (ActionScene.Instance != null)
                    {
                        StateManager.Instance.HSceneSetup = true;

                        //Find out all the characters that are not involved in H
                        List<Actor> charList = Util.GetActorsNotInvolvedInH(ActionScene.Instance, hScene);
                        List<Actor> hCharList = Util.GetActorsInvolvedInH(ActionScene.Instance, hScene);

                        foreach (var group in StateManager.Instance.HAnimationGroupsList)
                        {
                            //Recover the clothes state as all actors are forced to take off the clothes before enter H scene in order to fix a rendering issue
                            HAnim.RecoverClothesState(group);

                            HAnim.StartHAnimation(group);
                            if (group.male1 != null)
                                charList.Remove(group.male1);
                            if (group.male2 != null)
                                charList.Remove(group.male2);
                            if (group.female1 != null)
                                charList.Remove(group.female1);
                            if (group.female2 != null)
                                charList.Remove(group.female2);
                        }

                        foreach (Actor actor in charList)
                        {
                            //Recover the clothes state as all actors are forced to take off the clothes before enter H scene in order to fix a rendering issue
                            HAnim.RecoverClothesState(actor.Chara);

                            if (StateManager.Instance.ActorHAnimationList.ContainsKey(actor.GetInstanceID()))
                                continue;


                            //Otherwise set other single reaction

                            int animType = Util.GetCurrentAnimationType(ActionScene.Instance.MapID, actor.Sex, actor.Animation._param.ID);

                            //Get possible reactions list
                            List<int> possibleReaction = DecidePossibleReaction(actor, hCharList);

                            //Decide the reaction by random number
                            int chosenReaction = -1;
                            if (possibleReaction.Count > 0)
                            {
                                System.Random rnd = new System.Random();
                                int rndResult = rnd.Next(possibleReaction.Count);
                                chosenReaction = possibleReaction[rndResult];
                            }

                            //set the animation to the character
                            var reactionParam = GetCustomAnimationData(chosenReaction, animType, actor.Sex);


                            //Move to a standing position if the animation is a standing one and the actor is sitting
                            if (reactionParam.requireStanding && animType != Constant.AnimType.Standing)
                            {
                                if (!RelocateActor(actor))
                                {
                                    //Force the character to use default anim(awkward) if no such position is found
                                    reactionParam = CustomAnimation.Common.NoChange;
                                }
                            }


                            //Assign the animation to the actor
                            if (reactionParam.animationParameter != null)
                            {
                                if (reactionParam.assetBundle != null && reactionParam.assetName != null)
                                {
                                    var rac = actor.Chara.LoadAnimation(reactionParam.assetBundle, reactionParam.assetName);
                                    actor.Animation.SetAnimatorController(rac);
                                }
                                actor.PlayAnimOnce(reactionParam.animationParameter);
                            }

                            StateManager.Instance.CustomAnimationParameter.Add(actor.GetInstanceID(), reactionParam);

                            ChangeActorLookingAtHScene(actor);
                        }


                    }

                }
            }

            internal static List<int> DecidePossibleReaction(Actor actor, List<Actor> hCharList)
            {
                List<int> possibleReaction = new List<int>();

                switch (Util.GetCharacterType(actor))
                {
                    case Constant.CharacterType.Honesty:
                        if (Config.EnableSeriousAwkward)
                            possibleReaction.Add(Constant.PossibleReactionType.Awkward);
                        if (Config.EnableSeriousAngry)
                            possibleReaction.Add(Constant.PossibleReactionType.Angry);
                        break;
                    case Constant.CharacterType.Naughty:
                        if (Config.EnablePlayfulHurray)
                            possibleReaction.Add(Constant.PossibleReactionType.Hurray);
                        if (Config.EnablePlayfulExcited)
                            possibleReaction.Add(Constant.PossibleReactionType.Excited);
                        break;
                    case Constant.CharacterType.Unique:
                        if (Config.EnableUniqueWorry)
                            possibleReaction.Add(Constant.PossibleReactionType.Worry);
                        if (Config.EnableUniqueHappy)
                            possibleReaction.Add(Constant.PossibleReactionType.Happy);
                        break;
                }

                //Allow masturbation if female and sex desire is high enough
                if (actor.Sex == 1 && actor._status.Parameters[(int)RG.Define.Action.StatusCategory.Libido] >= Config.MasturbationLibidoThreshold)
                {
                    if (Config.EnableStandardMasturbation)
                        possibleReaction.Add(Constant.PossibleReactionType.Libido);
                }

                //Check if any partner involved in H
                foreach (var target in hCharList)
                {
                    if (ActionScene.IsGTERelationState(actor, target, RG.Define.Action.RelationType.Partner) || !Config.StandardCryPrerequisite)
                    {
                        if (Config.EnableStandardCry)
                            possibleReaction.Add(Constant.PossibleReactionType.Cry);
                    }
                }

                return possibleReaction;
            }

            internal static bool RelocateActor(Actor actor)
            {
                bool isPosChanged = false;

                int i = 0;
                while (i < actor.OccupiedActionPoint._destinationToTalk.Count)
                {
                    //Check if the point is occupied
                    bool isOccupied = false;
                    foreach (var a in ActionScene.Instance._actors)
                    {
                        if (a.transform.position == actor.OccupiedActionPoint._destinationToTalk[i].position)
                        {
                            isOccupied = true;
                            break;
                        }
                    }

                    //Move if there is a non occupied position
                    if (!isOccupied)
                    {
                        actor.transform.position = actor.OccupiedActionPoint._destinationToTalk[i].position;
                        isPosChanged = true;
                        break;
                    }

                    i++;
                }

                return isPosChanged;
            }

            internal static void ChangeActorLookingAtHScene(Actor actor)
            {
                if (StateManager.Instance.CustomAnimationParameter.ContainsKey(actor.GetInstanceID()))
                {
                    var reactionParam = StateManager.Instance.CustomAnimationParameter[actor.GetInstanceID()];
                    var hSceneFirstFemaleChar = StateManager.Instance.CurrentHSceneInstance._chaFemales[0];

                    //Turn the body to face the scene if necessary
                    if (reactionParam.requireBodyMove)
                    {
                        actor.Chara.transform.LookAt(hSceneFirstFemaleChar.transform);
                    }


                    //Set staring at the female
                    if (reactionParam.isStaring)
                    {
                        actor.Chara.ChangeLookEyesTarget(1, hSceneFirstFemaleChar.ObjEyesLookTarget.transform);

                        actor.Chara.ChangeLookEyesPtn(1);


                        if (reactionParam.isHeadDown)
                        {
                            if (StateManager.Instance.HeadDownLookTarget == null)
                            {
                                //Turn the head a bit down
                                StateManager.Instance.HeadDownLookTarget = UnityEngine.Object.Instantiate(hSceneFirstFemaleChar.ObjNeckLookTarget);
                                StateManager.Instance.HeadDownLookTarget.transform.position = hSceneFirstFemaleChar.ObjNeckLookTarget.transform.position + new Vector3(6, -12, 0);

                            }

                            actor.Chara.ChangeLookNeckTarget(1, StateManager.Instance.HeadDownLookTarget.transform);
                        }
                        else
                        {
                            actor.Chara.ChangeLookNeckTarget(1, hSceneFirstFemaleChar.ObjNeckLookTarget.transform);
                        }
                        actor.Chara.ChangeLookNeckPtn(1);
                    }
                }
            }

            //Get the animation parameters based on the reaction type etc
            internal static CustomAnimation.CustomAnimationData GetCustomAnimationData(int reactionType, int animType, byte sex)
            {
                if (reactionType == Constant.PossibleReactionType.Angry)
                {
                    return CustomAnimation.Common.NotImpressed;
                }
                else if (reactionType == Constant.PossibleReactionType.Happy)
                {
                    return CustomAnimation.Common.Happy;
                }
                else if (reactionType == Constant.PossibleReactionType.Worry)
                {
                    if (sex == 0)
                    {
                        if (animType == Constant.AnimType.Standing)
                            return CustomAnimation.Male.StandingWorry;
                        else
                            return CustomAnimation.Male.SittingWorry;
                    }
                    else
                    {
                        if (animType == Constant.AnimType.Standing)
                            return CustomAnimation.Female.StandingWorry;
                        else
                            return CustomAnimation.Female.SittingWorry;
                    }
                }
                else if (reactionType == Constant.PossibleReactionType.Libido)
                {
                    if (animType == Constant.AnimType.SittingWithTable)
                        return CustomAnimation.Female.TableSeatMasturbation;
                    if (animType == Constant.AnimType.Sitting)
                        return CustomAnimation.Female.SeatMasturbation;
                    else
                        return CustomAnimation.Female.StandingMasturbation;

                }
                else if (reactionType == Constant.PossibleReactionType.Cry)
                {
                    return CustomAnimation.Common.StandingCrying;
                }
                else if (reactionType == Constant.PossibleReactionType.Excited)
                {
                    return CustomAnimation.Common.Excited;
                }
                else if (reactionType == Constant.PossibleReactionType.Hurray)
                {
                    return CustomAnimation.Common.Hurray;
                }
                else
                {
                    //Awkward
                    return CustomAnimation.Common.NoChange;
                }
            }





            ////internal static List<Actor> GetActorsInvolvedInH(ActionScene actionScene, HScene hScene)
            ////{
            ////    List<Actor> result = GetActorsNotInvolvedInH(actionScene, hScene);

            ////    foreach (var actor in actionScene._actors)
            ////    {
            ////        if (result.Where(a => a.GetInstanceID() == actor.GetInstanceID()) == null)
            ////            result.Add(actor);
            ////    }

            ////    return result;
            ////}

            internal static void BackupCharacterLookInfo(HScene hScene)
            {
                if (ActionScene.Instance != null)
                {
                    var actorList = Util.GetActorsNotInvolvedInH(ActionScene.Instance, hScene);
                    foreach (var actor in actorList)
                    {
                        StateManager.BackUpInformation info = new StateManager.BackUpInformation();
                        info.lookEyePtn = actor.Chara.GetLookEyesPtn();
                        info.lookNeckPtn = actor.Chara.GetLookNeckPtn();
                        info.lookEyeTarget = actor.Chara.EyeLookCtrl.target;
                        info.lookNeckTarget = actor.Chara.NeckLookCtrl.target;

                        StateManager.Instance.ActorBackUpData.Add(actor.GetInstanceID(), info);
                    }
                }

            }

        }
    }
}
