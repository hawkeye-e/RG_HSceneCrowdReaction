using BepInEx.Logging;
using HarmonyLib;
using RG.Scene;
using UnityEngine;
using RG.Scene.Action.Core;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace HSceneCrowdReaction.HSceneScreen
{
    internal class Patches
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
                StateManager.Instance.HActionActorList = new Dictionary<int, HPoint>();
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
                    if(!StateManager.Instance.HActionActorList.ContainsKey(actor.GetInstanceID()))
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
            
            if(StateManager.Instance.HActionUpdateTimerList != null)
            {
                foreach(var timer in StateManager.Instance.HActionUpdateTimerList)
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
            
            if (StateManager.Instance.HActionActorList != null)
            {
                StateManager.Instance.HActionActorList.Clear();
                StateManager.Instance.HActionActorList = null;
            }
            if(StateManager.Instance.ForceActiveInstanceID != null)
            {
                StateManager.Instance.ForceActiveInstanceID.Clear();
                StateManager.Instance.ForceActiveInstanceID = null;
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
                        if (StateManager.Instance.HActionActorList.ContainsKey(actor.GetInstanceID()))
                            continue;

                        if (IsHActionPossible(actor))
                        {
                            Log.LogInfo("Start H Animation: " + actor.Status.FullName + " and " + actor.Partner.Status.FullName);
                            StartHAnimation(actor);
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






        internal static bool IsHActionPossible(Actor actor)
        {
            if(actor.Partner == null)                           //block if not in pair now
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
            actor.Chara.LoadHitObject();
            partnerActor.Chara.LoadHitObject();
            Log.LogInfo("pt2");
            //Get the HPoint
            HPoint hPoint = GetHPoint(actor);
            Log.LogInfo("pt3");
            Log.LogInfo("actor: " + actor.Status.FullName + " and " + partnerActor.Status.FullName + ", hPoint: " + hPoint.name);

            //Get the chosen animation
            var animInfo = GetHAnimation(hPoint);
             
            //Setup HPoint
            InitHPoint(hPoint, animInfo);
            Log.LogInfo("pt4");
            //Assign the actor to the HPoint
            SetActorToHPoint(actor, hPoint);
            SetActorToHPoint(partnerActor, hPoint);
            Log.LogInfo("Name: " + actor.Status.FullName);
            Debug.PrintDetail(actor.Chara.transform);
            Log.LogInfo("partnerActor: " + actor.Status.FullName);
            Debug.PrintDetail(partnerActor.Chara.transform);
            Log.LogInfo("pt5");
            //Set the animation of the actor
            Constant.HCharacterType actorType, partnerType;
            if (actor.Sex == 0)
            {
                actorType = Constant.HCharacterType.Male1;
                partnerType = Constant.HCharacterType.Female1;
            }else if(partnerActor.Sex == 0)
            {
                actorType = Constant.HCharacterType.Female1;
                partnerType = Constant.HCharacterType.Male1;
            }
            else
            {
                actorType = Constant.HCharacterType.Female1;
                partnerType = Constant.HCharacterType.Female2;
            }
            Log.LogInfo("actor: " + actor.Status.FullName + " and " + partnerActor.Status.FullName + ", anim: " + animInfo.NameAnimation);
            SetActorAnimation(actor, animInfo, actorType);
            SetActorAnimation(partnerActor, animInfo, partnerType);
            Log.LogInfo("pt6");

            //Play the animation
            string clipName = GetRandomAnimationClipName(actor);
            Log.LogInfo("ClipName: " + clipName);
            SetCharacterPlayAnimation(actor.Chara, clipName);
            SetCharacterPlayAnimation(partnerActor.Chara, clipName);
            Log.LogInfo("pt7");
            //Update the clothes state of the actor
            SetActorClothesState(actor.Chara);
            SetActorClothesState(partnerActor.Chara);
            Log.LogInfo("pt8");
            //Show the penis if necessary
            if (animInfo.MaleSon == 1)
            {
                ForcePenisVisible(actor.Chara);
                ForcePenisVisible(partnerActor.Chara);
            }
            Log.LogInfo("pt9");

            StateManager.Instance.HActionActorList.Add(actor.GetInstanceID(), hPoint);
            StateManager.Instance.HActionActorList.Add(actor.Partner.GetInstanceID(), hPoint);

            //Set timer to update the clip and speed at random interval
            System.Random rnd = new System.Random();
            int rndResult = rnd.Next(Settings.HActionRandomMilliSecond);
            CustomTimer timer = new CustomTimer(actor, partnerActor, Settings.HActionMinMilliSecond + rndResult);
            timer.Elapsed += OnHActionUpdateEvent;
            timer.Enabled = true;
            timer.AutoReset = true;
            StateManager.Instance.HActionUpdateTimerList.Add(timer);
            Log.LogInfo("pt10");
        }

        private static void OnHActionUpdateEvent(object sender, ElapsedEventArgs e)
        {
            Log.LogInfo("OnHActionUpdateEvent start!");
            Actor actor1 = ((CustomTimer)sender).Actor1;
            Actor actor2 = ((CustomTimer)sender).Actor2;

            //Update the animation clip
            string clipName = GetRandomAnimationClipName(actor1);
            SetCharacterPlayAnimation(actor1.Chara, clipName);
            SetCharacterPlayAnimation(actor2.Chara, clipName);

            //Update the animation speed
            System.Random rnd = new System.Random();
            float speed = 1 + rnd.Next(1000) / 1000f;
            actor1.Chara.AnimBody.speed = speed;
            actor2.Chara.AnimBody.speed = speed;


            int rndResult = rnd.Next(Settings.HActionRandomMilliSecond);
            ((CustomTimer)sender).Interval = Settings.HActionMinMilliSecond + rndResult;

            Log.LogInfo("OnHActionUpdateEvent end!");
        }
 

        private static HPoint GetHPoint(Actor actor)
        {
            ActionPoint targetAP = actor.OccupiedActionPoint == null ? actor.Partner.OccupiedActionPoint : actor.OccupiedActionPoint;
            System.Random rnd = new System.Random();
            int rndResult = rnd.Next(targetAP.HPointLink.Count);
            return targetAP.HPointLink[rndResult];
        }

        private static void InitHPoint(HPoint hPoint, HScene.AnimationListInfo animInfo)
        {
            hPoint.ChangeHideProcBefore();
            //hPoint.ChangeHideProc(1);
            hPoint.ChangeHideProcAll();
            hPoint.HpointObjVisibleChange(true);

            //move the object set
            if(hPoint._moveObjects != null)
            {
                foreach(var moveObj in hPoint._moveObjects)
                {
                    for (int i = 0; i < moveObj.OffSetInfos.Count; i++)
                        moveObj.SetOffset(i);
                }   
            }
            
            //populate the moved chair for the pose
            if (animInfo.IsNeedItem)
            {
                foreach (var item in hPoint.MotionChairIDs)
                {
                    if (item != 0)
                    {
                        var abinfo = Manager.HSceneManager.HResourceTables.DicDicMapDependItemInfo[item];
                        var fullpath = Util.GetAssetBundlePath(abinfo.assetbundle);

                        var ab = AssetBundle.LoadFromFile(fullpath);

                        if (ab != null)
                        {
                            GameObject obj = Util.InstantiateFromBundle(ab, abinfo.asset);

                            obj.transform.position = hPoint.transform.position;
                            obj.transform.localPosition = hPoint.transform.localPosition;
                            obj.transform.rotation = hPoint.transform.rotation;
                            obj.transform.localRotation = hPoint.transform.localRotation;

                            obj.transform.parent = StateManager.Instance.CurrentHSceneInstance.gameObject.transform;
                        }

                        ab.Unload(false);
                    }
                }
            }
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


        internal static void SetActorToHPoint(Actor actor, HPoint hPoint)
        //private static void AttachCharacterToHPoint(Chara.ChaControl character, HPoint hPoint)
        {
            //character.SetPosition(hPoint.transform.position);
            //character.SetRotation(hPoint.transform.rotation);
            actor.transform.position = hPoint.transform.position;
            actor.transform.localPosition = hPoint.transform.localPosition;
            //actor.transform.localPosition = Vector3.zero;
            actor.transform.rotation = hPoint.transform.rotation;
            actor.transform.localRotation = hPoint.transform.localRotation;

            if(hPoint.MotionChairIDs.Count > 0)
            {
                if (hPoint.MotionChairIDs[0]!= 0)
                {
                    actor.transform.rotation = Quaternion.Inverse(actor.transform.rotation);
                }
            }
            
            Log.LogInfo("Name: " + actor.Status.FullName + ", pos: " + actor.transform.position + ", hPoint pos: " + hPoint.transform.position);
            Log.LogInfo("Name: " + actor.Status.FullName + ", localpos: " + actor.transform.localPosition + ", hPoint localpos: " + hPoint.transform.localPosition);
            Log.LogInfo("Name: " + actor.Status.FullName + ", rot: " + actor.transform.rotation + ", hPoint rot: " + hPoint.transform.rotation);
            Log.LogInfo("Name: " + actor.Status.FullName + ", localrot: " + actor.transform.localRotation + ", hPoint localrot: " + hPoint.transform.localRotation);
        }

        private static void SetActorAnimation(Actor actor, HScene.AnimationListInfo animInfo, Constant.HCharacterType characterType)
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
            Log.LogInfo("Name: " + character.FileParam.fullname + ", clipname: " + fullClipName);
            character.PlaySync(fullClipName, -1, 0);
        }

        private static string GetRandomAnimationClipName(Actor actor)
        {
            List<string> list = new List<string>();
            
            foreach (var clip in actor.Chara.AnimBody.runtimeAnimatorController.animationClips)
            {
                if (clip.name.Contains(Constant.HAnimClipKeyword.Loop) )
                {
                    //Remove the height kind
                    string clipname = clip.name.Substring(2);
                    //Log.LogInfo("GetRandomAnimationClipName clipname: " + clip.name + ", substring: " + clipname);
                    if (!list.Contains(clipname))
                        list.Add(clipname);
                }
            }

            System.Random rnd = new System.Random();
            int rndResult = rnd.Next(list.Count);
            return list[rndResult];
        }

        

        private static void SetActorClothesState(Chara.ChaControl character)
        {
            //male.Chara.SetClothesStateAll(2);
            //female.Chara.SetClothesStateAll(2);
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
    }
}
