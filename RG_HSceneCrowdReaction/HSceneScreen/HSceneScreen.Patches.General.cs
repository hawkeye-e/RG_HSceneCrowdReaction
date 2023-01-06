using BepInEx.Logging;
using HarmonyLib;
using RG.Scene;
using UnityEngine;
using RG.Scene.Action.Core;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using HSceneCrowdReaction.InfoList;

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
                    StateManager.Instance.CustomAnimationParameter = new Dictionary<int, CustomAnimation.CustomAnimationData>();
                    StateManager.Instance.CurrentHSceneInstance = hScene;

                    StateManager.Instance.ForceActiveInstanceID = new List<int>();
                    StateManager.Instance.HActionUpdateTimerList = new List<Timer>();
                    //StateManager.Instance.HActionActorList = new List<int>();
                    StateManager.Instance.ActorHAnimationList = new Dictionary<int, HAnimation.ActorHAnimData>();
                    
                    StateManager.Instance.ActorHAnimNextUpdateProcessing = new Dictionary<int, bool>();
                    foreach(var actor in ActionScene.Instance._actors)
                    {
                        StateManager.Instance.ActorHAnimNextUpdateProcessing.Add(actor.Chara.GetInstanceID(), false);
                    }
                    StateManager.Instance.ActorHAnimNextUpdateTimeDictionary = new Dictionary<int, long>();

                    
                }
            }

            internal static void RestoreActorsLookingDirection(HScene hScene)
            {
                if (ActionScene.Instance != null)
                {
                    var actorList = GetActorsNotInvolvedInH(ActionScene.Instance, hScene);
                    foreach (var actor in actorList)
                    {
                        actor.Chara.ChangeLookEyesPtn(0);
                        actor.Chara.ChangeLookNeckPtn(0);
                    }
                }
            }

            internal static void UpdateNonHActorsLookAt(HScene hScene)
            {
                if (ActionScene.Instance != null)
                {
                    DestroyTempObject();

                    List<Actor> actorList = GetActorsNotInvolvedInH(ActionScene.Instance, hScene);
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
                if(StateManager.Instance.ActorHAnimNextUpdateProcessing != null)
                {
                    StateManager.Instance.ActorHAnimNextUpdateProcessing.Clear();
                    StateManager.Instance.ActorHAnimNextUpdateProcessing = null;
                }
                if (StateManager.Instance.ActorClothesState != null)
                {
                    StateManager.Instance.ActorClothesState.Clear();
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
                        List<Actor> charList = GetActorsNotInvolvedInH(ActionScene.Instance, hScene);
                        List<Actor> hCharList = GetActorsInvolvedInH(ActionScene.Instance, hScene);
                        foreach (Actor actor in charList)
                        {
                            if (StateManager.Instance.ActorHAnimationList.ContainsKey(actor.GetInstanceID()))
                                continue;

                            if (HAnim.IsHActionPossible(actor))
                            {
                                Log.LogInfo("Start H Animation: " + actor.Status.FullName + " and " + actor.Partner.Status.FullName);
                                HAnim.StartHAnimation(actor);
                            }
                            else
                            {

                                int animType = Util.GetCurrentAnimationType(ActionScene.Instance.MapID, actor.Sex, actor.Animation._param.ID);

                                //Get possible reactions list
                                List<int> possibleReaction = DecidePossibleReaction(actor, hCharList);

                                //Decide the reaction by random number
                                System.Random rnd = new System.Random();
                                int rndResult = rnd.Next(possibleReaction.Count);
                                var chosenReaction = possibleReaction[rndResult];

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

                                    //Log.LogInfo("=============");
                                    //Debug.PrintDetail(reactionParam.animationParameter);
                                    //if(reactionParam.animationParameter.States != null)
                                    //    for(int i=0; i< reactionParam.animationParameter.States.Count; i++)
                                    //    {
                                    //        for (int j = 0; j < reactionParam.animationParameter.States[i].Count; j++)
                                    //            Log.LogInfo("States[" + i + "][" + j +  "]: " + reactionParam.animationParameter.States[i][j]);
                                    //    }
                                    //if (reactionParam.animationParameter.StateHashes != null)
                                    //    for (int i = 0; i < reactionParam.animationParameter.StateHashes.Count; i++)
                                    //    {
                                    //        for (int j = 0; j < reactionParam.animationParameter.StateHashes[i].Count; j++)
                                    //        {
                                    //            Log.LogInfo("StateHashes[" + i + "][" + j + "]: " + reactionParam.animationParameter.StateHashes[i][j]);
                                    //        }
                                    //    }
                                    //if (reactionParam.animationParameter.SpecifiedLayers != null)
                                    //    for (int i = 0; i < reactionParam.animationParameter.SpecifiedLayers.Count; i++)
                                    //    {
                                    //        Log.LogInfo("SpecifiedLayers[" + i + "]: " + reactionParam.animationParameter.SpecifiedLayers[i]);

                                    //    }

                                    //Log.LogInfo("=============");
                                }

                                StateManager.Instance.CustomAnimationParameter.Add(actor.GetInstanceID(), reactionParam);

                                ChangeActorLookingAtHScene(actor);
                            }
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
                        possibleReaction.Add(Constant.PossibleReactionType.Awkward);
                        possibleReaction.Add(Constant.PossibleReactionType.Angry);
                        break;
                    case Constant.CharacterType.Naughty:
                        possibleReaction.Add(Constant.PossibleReactionType.Hurray);
                        possibleReaction.Add(Constant.PossibleReactionType.Excited);
                        break;
                    case Constant.CharacterType.Unique:
                        possibleReaction.Add(Constant.PossibleReactionType.Worry);
                        possibleReaction.Add(Constant.PossibleReactionType.Happy);
                        break;
                }

                //Allow masturbation if female and sex desire is high enough
                if (actor.Sex == 1 && actor._status.Parameters[(int)RG.Define.Action.StatusCategory.Libido] > Settings.LibidoThreshold)
                {
                    possibleReaction.Add(Constant.PossibleReactionType.Libido);
                }

                //Check if any partner involved in H
                foreach (var target in hCharList)
                {
                    if (ActionScene.IsGTERelationState(actor, target, RG.Define.Action.RelationType.Partner))
                    {
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
                    Log.LogInfo("ChangeActorLookingAtHScene: " + actor.Status.FullName);

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



            internal static List<Actor> GetActorsNotInvolvedInH(ActionScene actionScene, HScene hScene)
            {
                Log.LogInfo("GetActorsNotInvolvedInH");
                List<Actor> result = new List<Actor>();
                List<int> hCharList = new List<int>();
                if (hScene._chaFemales != null)
                {
                    for (int i = 0; i < hScene._chaFemales.Count; i++)
                    {
                        if (hScene._chaFemales[i] != null)
                            hCharList.Add(hScene._chaFemales[i].GetInstanceID());
                    }
                }
                Log.LogInfo("GetActorsNotInvolvedInH pt1");

                if (hScene._chaMales != null)
                {
                    for (int i = 0; i < hScene._chaMales.Count; i++)
                    {
                        if (hScene._chaMales[i] != null)
                            hCharList.Add(hScene._chaMales[i].GetInstanceID());
                    }
                }
                Log.LogInfo("GetActorsNotInvolvedInH pt2");
                foreach (var actor in actionScene._actors)
                {
                    if (!hCharList.Contains(actor.Chara.GetInstanceID()))
                        result.Add(actor);
                }
                Log.LogInfo("GetActorsNotInvolvedInH pt3, result Count: " + result);

                return result;
            }

            internal static List<Actor> GetActorsInvolvedInH(ActionScene actionScene, HScene hScene)
            {
                Log.LogInfo("GetActorsInvolvedInH");
                List<Actor> result = new List<Actor>();
                List<int> hCharList = new List<int>();
                if (hScene._chaFemales != null)
                {
                    for (int i = 0; i < hScene._chaFemales.Count; i++)
                    {
                        if (hScene._chaFemales[i] != null)
                            hCharList.Add(hScene._chaFemales[i].GetInstanceID());
                    }
                }

                if (hScene._chaMales != null)
                {
                    for (int i = 0; i < hScene._chaMales.Count; i++)
                    {
                        if (hScene._chaMales[i] != null)
                            hCharList.Add(hScene._chaMales[i].GetInstanceID());
                    }
                }

                return result;
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




        }
    }
}
