using BepInEx.Logging;
using HarmonyLib;
using RG.Scene;
using RG.Scene.Action.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;



namespace HSceneCrowdReaction.HSceneScreen
{
    internal class Hook
    {
        private static ManualLogSource Log = HSceneCrowdReactionPlugin.Log;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CharaInit))]
        private static void CharaInitPre(HScene __instance)
        {
            Log.LogInfo("CharaInitPre");
        }

        //Initialize when H scene start
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CharaInit))]
        private static void CharaInitPost(HScene __instance)
        {
            //TODO: need remove this
            StateManager.Instance.CurrentHSceneInstance = __instance;
            Log.LogInfo("CharaInitPost");
            //Patches.General.InitHScene(__instance);
        }


        //Restore the look/neck target when scene end
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.OnDestroy))]
        private static void OnDestroyPre(HScene __instance)
        {
            Patches.HAnim.RecoverAllClothesState(ActionScene.Instance);
            Patches.General.RestoreActorsLookingDirection(__instance);
            Patches.General.DestroyTempObject();
            Patches.General.DestroyStateManagerList();

        }

        //Reset the looking direction when H point moved
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetMovePositionPoint))]
        private static void SetMovePositionPointPost(HScene __instance, Transform trans, Vector3 offsetpos, Vector3 offsetrot, bool isWorld)
        {
            Log.LogInfo("SetMovePositionPointPost");
            //Patches.General.UpdateNonHActorsLookAt(__instance);
        }

        //Reset the looking direction when H point moved
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetPosition), new[] { typeof(Transform), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool) })]
        private static void SetPosition1Post(HScene __instance, Transform _trans, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart, bool _isWorld)
        {
            Log.LogInfo("SetPosition1Post");
            //Patches.General.UpdateNonHActorsLookAt(__instance);
        }

        //Reset the looking direction when H point moved
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetPosition), new[] { typeof(Vector3), typeof(Quaternion), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool) })]
        private static void SetPosition2Post(HScene __instance, Vector3 pos, Quaternion rot, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart, bool isWorld)
        {
            Log.LogInfo("SetPosition2Post");
            //Patches.General.UpdateNonHActorsLookAt(__instance);
        }

        ////////Change the animation of actors not involved in H
        //////[HarmonyPostfix]
        //////[HarmonyPatch(typeof(HScene), nameof(HScene.StartPointSelect))]
        //////private static void StartPointSelectPost(HScene __instance, int hpointLen, UnhollowerBaseLib.Il2CppReferenceArray<HPoint> hPoints, int checkCategory, HScene.AnimationListInfo info)
        //////{
        //////    Log.LogInfo("StartPointSelectPost");
        //////    //Patches.ChangeActorsAnimation(__instance);
        //////}


        /*
        //Change the animation of actors not involved in H
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetStartAnimationInfo))]
        private static void SetStartAnimationInfoPost(HScene __instance)
        {
            Log.LogInfo("SetStartAnimationInfoPost");
            //Patches.General.InitHScene(__instance);
            //Patches.General.ChangeActorsAnimation(__instance);

        }
        */

        //To Fix CTD?
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetClothStateStartMotion))]
        private static void SetClothStateStartMotionPost(HScene __instance)
        {
            Log.LogInfo("SetClothStateStartMotionPost");

            Patches.General.InitHScene(__instance);
            Patches.General.ChangeActorsAnimation(__instance);
        }

        //Change the animation of actors not involved in HHScene.StartPointSelectPre
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.LateUpdateForce))]
        private static void LateUpdateForcePost(Chara.ChaControl __instance)
        {
            Patches.HAnim.CheckUpdateHAnim(__instance);
        }





        //Set all character not involved in H scene to nude in order to avoid transparent surface of body parts
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ActionScene), nameof(ActionScene.ChangeToHScene))]
        private static void ChangeToHScenePre(Actor target, Actor partner, Actor sub)
        {
            Log.LogInfo("ChangeToHScenePre");
            foreach (var actor in ActionScene.Instance._actors)
            {
                if (target != null)
                    if (actor.GetInstanceID() == target.GetInstanceID())
                        continue;
                if (partner != null)
                    if (actor.GetInstanceID() == partner.GetInstanceID())
                        continue;
                if (sub != null)
                    if (actor.GetInstanceID() == sub.GetInstanceID())
                        continue;

                //TODO: remember the clothes state and recover later
                byte[] clothesState = new byte[8];
                for(int i=0; i< Math.Min(actor.Chara.FileStatus.clothesState.Length, 8); i++)
                {
                    clothesState[i] = actor.Chara.FileStatus.clothesState[i];
                    actor.Chara.FileStatus.clothesState[i] = 2;
                }
                StateManager.Instance.ActorClothesState.Add(actor.Chara.GetInstanceID(), clothesState);

            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ActionScene), nameof(ActionScene.ChangeToHScene))]
        private static void ChangeToHScenePost(Actor target, Actor partner, Actor sub)
        {
            Log.LogInfo("ChangeToHScenePost");
        }

        [HarmonyFinalizer]
        [HarmonyPatch(typeof(ActionScene), nameof(ActionScene.ChangeToHScene))]
        private static Exception CatchLoadErrors(Exception __exception)
        {
            if (__exception != null)
            {
                //__result = false;
                Log.LogWarning("exception in ActionScene.ChangeToHScene");
                Log.LogInfo(__exception.Message);
            }
            return null;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ActionScene), nameof(ActionScene.BackFromADVScene))]
        private static void BackFromADVScenePre()
        {
            Log.LogInfo("BackFromADVScenePre");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ActionScene), nameof(ActionScene.BackFromADVScene))]
        private static void BackFromADVScenePost()
        {
            Log.LogInfo("BackFromADVScenePost");
        }



        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CharaInit))]
        private static void CharaInitPre(HScene __instance)
        {
            Log.LogInfo("CharaInitPre");
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetMovePositionPoint))]
        private static void SetMovePositionPointPre(HScene __instance, Transform trans, Vector3 offsetpos, Vector3 offsetrot, bool isWorld)
        {
            Log.LogInfo("SetMovePositionPointPre");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetPosition), new[] { typeof(Transform), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool) })]
        private static void SetPosition1Pre(HScene __instance, Transform _trans, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart, bool _isWorld)
        {
            Log.LogInfo("SetPosition1Pre");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetPosition), new[] { typeof(Vector3), typeof(Quaternion), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool) })]
        private static void SetPosition2Pre(HScene __instance, Vector3 pos, Quaternion rot, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart, bool isWorld)
        {
            Log.LogInfo("SetPosition2Pre");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetStartAnimationInfo))]
        private static void SetStartAnimationInfoPre(HScene __instance)
        {
            Log.LogInfo("SetStartAnimationInfoPre");

        }



        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CanSpeakChangeMotion))]
        private static void CanSpeakChangeMotionPre(Animator _anim)
        {
            Log.LogInfo("CanSpeakChangeMotionPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CanSpeakChangeMotion))]
        private static void CanSpeakChangeMotion(Animator _anim)
        {
            Log.LogInfo("CanSpeakChangeMotionPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeAnimation))]
        private static void ChangeAnimationPre(HScene.AnimationListInfo _info, bool _isForceResetCamera, bool _isForceLoopAction, bool _UseFade, bool isLoadFirst)
        {
            Log.LogInfo("ChangeAnimationPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeAnimation))]
        private static void ChangeAnimationPost(HScene.AnimationListInfo _info, bool _isForceResetCamera, bool _isForceLoopAction, bool _UseFade, bool isLoadFirst)
        {
            Log.LogInfo("ChangeAnimationPost");
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeAnimPos))]
        private static void ChangeAnimPosPre(HScene.AnimationListInfo _info, Vector3 pos, Vector3 rot, bool isLoadFirst)
        {
            Log.LogInfo("ChangeAnimPosPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeAnimPos) )]
        private static void ChangeAnimPosPost(HScene.AnimationListInfo _info, Vector3 pos, Vector3 rot, bool isLoadFirst)
        {
            Log.LogInfo("ChangeAnimPosPost");
        }
        */

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeAnimPosAndCamera))]
        private static void ChangeAnimPosAndCameraPre(HScene.AnimationListInfo _info, bool _isForceResetCamera, bool isLoadFirst)
        {
            Log.LogInfo("ChangeAnimPosAndCameraPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeAnimPosAndCamera))]
        private static void ChangeAnimPosAndCameraPost(HScene.AnimationListInfo _info, bool _isForceResetCamera, bool isLoadFirst)
        {
            Log.LogInfo("ChangeAnimPosAndCameraPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeItem))]
        private static void ChangeItemPre()
        {
            Log.LogInfo("ChangeItemPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeItem))]
        private static void ChangeItemPost()
        {
            Log.LogInfo("ChangeItemPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeModeCtrl))]
        private static void ChangeModeCtrlPre(HScene.AnimationListInfo _info)
        {
            Log.LogInfo("ChangeModeCtrlPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeModeCtrl))]
        private static void ChangeModeCtrlPost(HScene.AnimationListInfo _info)
        {
            Log.LogInfo("ChangeModeCtrlPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeMotionForPointProc))]
        private static void ChangeMotionForPointProcPre()
        {
            Log.LogInfo("ChangeMotionForPointProcPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeMotionForPointProc))]
        private static void ChangeMotionForPointProcPost()
        {
            Log.LogInfo("ChangeMotionForPointProcPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeOnly))]
        private static void ChangeOnlyPre(HScene.AnimationListInfo select)
        {
            Log.LogInfo("ChangeOnlyPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeOnly))]
        private static void ChangeOnlyPost(HScene.AnimationListInfo select)
        {
            Log.LogInfo("ChangeOnlyPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeReverb))]
        private static void ChangeReverbPre(bool val)
        {
            Log.LogInfo("ChangeReverbPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeReverb))]
        private static void ChangeReverbPost(bool val)
        {
            Log.LogInfo("ChangeReverbPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeWet))]
        private static void ChangeWetPre(HPoint p)
        {
            Log.LogInfo("ChangeWetPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeWet))]
        private static void ChangeWetPost(HPoint p)
        {
            Log.LogInfo("ChangeWetPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CharaInitF))]
        private static void CharaInitFPre()
        {
            Log.LogInfo("CharaInitFPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CharaInitF))]
        private static void CharaInitFPost()
        {
            Log.LogInfo("CharaInitFPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CharaInitM))]
        private static void CharaInitMPre()
        {
            Log.LogInfo("CharaInitMPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CharaInitM))]
        private static void CharaInitMPost()
        {
            Log.LogInfo("CharaInitMPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckEvent), new[] { typeof(int), typeof(UnhollowerBaseLib.Il2CppReferenceArray<HPoint>) })]
        private static void CheckEventPre(int eventID, UnhollowerBaseLib.Il2CppReferenceArray<HPoint> hPoints)
        {
            Log.LogInfo("CheckEventPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckEvent), new[] { typeof(int), typeof(UnhollowerBaseLib.Il2CppReferenceArray<HPoint>) })]
        private static void CheckEventPost(int eventID, UnhollowerBaseLib.Il2CppReferenceArray<HPoint> hPoints)
        {
            Log.LogInfo("CheckEventPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckEvent), new[] { typeof(Il2CppSystem.Collections.Generic.List<int>), typeof(int) })]
        private static void CheckEventPre2(Il2CppSystem.Collections.Generic.List<int> infoEvents, int eventID)
        {
            Log.LogInfo("CheckEventPre2");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckEvent), new[] {typeof(Il2CppSystem.Collections.Generic.List<int>), typeof(int)} )]
        private static void CheckEventPost2(Il2CppSystem.Collections.Generic.List<int> infoEvents, int eventID)
        {
            Log.LogInfo("CheckEventPost2");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckFaintness))]
        private static void CheckFaintnessPre(HScene.AnimationListInfo info)
        {
            Log.LogInfo("CheckFaintnessPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckFaintness))]
        private static void CheckFaintnessPost(HScene.AnimationListInfo info)
        {
            Log.LogInfo("CheckFaintnessPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckHpoint))]
        private static void CheckHpointPre(UnhollowerBaseLib.Il2CppReferenceArray<HPoint> hPoints)
        {
            Log.LogInfo("CheckHpointPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckHpoint))]
        private static void CheckHpointPost(UnhollowerBaseLib.Il2CppReferenceArray<HPoint> hPoints)
        {
            Log.LogInfo("CheckHpointPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckRelationFinish), new System.Type[] {})]
        private static void CheckRelationFinishPre()
        {
            Log.LogInfo("CheckRelationFinishPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckRelationFinish), new System.Type[] { })]
        private static void CheckRelationFinishPost()
        {
            Log.LogInfo("CheckRelationFinishPost");
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckRelationFinish), new [] { typeof(Actor), typeof(Il2CppSystem.Collections.Generic.List<Actor>), typeof(int), typeof(int), typeof(int) }, new ArgumentType[] {ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref})]
        private static void CheckRelationFinishPre2(Actor main, Il2CppSystem.Collections.Generic.List<Actor> subs, int length, int set, int setSub)
        {
            Log.LogInfo("CheckRelationFinishPre2");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckRelationFinish), new[] { typeof(Actor), typeof(Il2CppSystem.Collections.Generic.List<Actor>), typeof(int), typeof(int), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref })]
        private static void CheckRelationFinishPost2(Actor main, Il2CppSystem.Collections.Generic.List<Actor> subs, int length, int set, int setSub)
        {
            Log.LogInfo("CheckRelationFinishPost2");
        }
        */

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckSpeek))]
        private static void CheckSpeekPre()
        {
            Log.LogInfo("CheckSpeekPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckSpeek))]
        private static void CheckSpeekPost()
        {
            Log.LogInfo("CheckSpeekPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckState))]
        private static void CheckStatePre(int mode, int state)
        {
            Log.LogInfo("CheckStatePre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CheckState))]
        private static void CheckStatePost(int mode, int state)
        {
            Log.LogInfo("CheckStatePost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ConfigEnd))]
        private static void ConfigEndPre()
        {
            Log.LogInfo("ConfigEndPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ConfigEnd))]
        private static void ConfigEndPost()
        {
            Log.LogInfo("ConfigEndPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CreateListAnimationFileName))]
        private static void CreateListAnimationFileNamePre()
        {
            Log.LogInfo("CreateListAnimationFileNamePre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CreateListAnimationFileName))]
        private static void CreateListAnimationFileNamePost()
        {
            Log.LogInfo("CreateListAnimationFileNamePost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.EndParamNotPlayChara), new System.Type[] {} )]
        private static void EndParamNotPlayCharaPre1()
        {
            Log.LogInfo("EndParamNotPlayCharaPre1");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.EndParamNotPlayChara), new System.Type[] { })]
        private static void EndParamNotPlayCharaPost1()
        {
            Log.LogInfo("EndParamNotPlayCharaPost1");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.EndParamNotPlayChara), new [] { typeof(RG.User.Status) })]
        private static void EndParamNotPlayCharaPre2(RG.User.Status status)
        {
            Log.LogInfo("EndParamNotPlayCharaPre2");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.EndParamNotPlayChara), new[] { typeof(RG.User.Status) })]
        private static void EndParamNotPlayCharaPost2(RG.User.Status status)
        {
            Log.LogInfo("EndParamNotPlayCharaPost2");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.EndProc))]
        private static void EndProc()
        {
            Log.LogInfo("EndProcPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.EndProc))]
        private static void EndProcPost()
        {
            Log.LogInfo("EndProcPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.EndProcADV))]
        private static void EndProcADVPre()
        {
            Log.LogInfo("EndProcADVPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.EndProcADV))]
        private static void EndProcADVPost()
        {
            Log.LogInfo("EndProcADVPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.FemaleEndproc))]
        private static void FemaleEndprocPre()
        {
            Log.LogInfo("FemaleEndprocPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.FemaleEndproc))]
        private static void FemaleEndprocPrePost()
        {
            Log.LogInfo("FemaleEndprocPrePost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.GetAnimationListModeFromSelectInfo))]
        private static void GetAnimationListModeFromSelectInfoPre(HScene.AnimationListInfo _info)
        {
            Log.LogInfo("GetAnimationListModeFromSelectInfoPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.GetAnimationListModeFromSelectInfo))]
        private static void GetAnimationListModeFromSelectInfoPost(HScene.AnimationListInfo _info)
        {
            Log.LogInfo("GetAnimationListModeFromSelectInfoPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.GetAnimInfo))]
        private static void GetAnimInfoPre(int category, int id)
        {
            Log.LogInfo("GetAnimInfoPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.GetAnimInfo))]
        private static void GetAnimInfoPost(int category, int id)
        {
            Log.LogInfo("GetAnimInfoPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.GetAutoAnimation))]
        private static void GetAutoAnimationPre(bool _isFirst)
        {
            Log.LogInfo("GetAutoAnimationPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.GetAutoAnimation))]
        private static void GetAutoAnimationPost(bool _isFirst)
        {
            Log.LogInfo("GetAutoAnimationPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.GetConfigCloseProc))]
        private static void GetConfigCloseProcPre()
        {
            Log.LogInfo("GetConfigCloseProcPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.GetConfigCloseProc))]
        private static void GetConfigCloseProcPost()
        {
            Log.LogInfo("GetConfigCloseProcPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.GetFemales))]
        private static void GetFemalesPre()
        {
            Log.LogInfo("GetFemalesPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.GetFemales))]
        private static void GetFemalesPost()
        {
            Log.LogInfo("GetFemalesPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.GetMales))]
        private static void GetMalesPre()
        {
            Log.LogInfo("GetMalesPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.GetMales))]
        private static void GetMalesPost()
        {
            Log.LogInfo("GetMalesPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.GetPointIDMin))]
        private static void GetPointIDMinPre(Dictionary<int, List<HScene.AnimationListInfo>> tmpDicLst)
        {
            Log.LogInfo("GetPointIDMinPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.GetPointIDMin))]
        private static void GetPointIDMinPost(Dictionary<int, List<HScene.AnimationListInfo>> tmpDicLst)
        {
            Log.LogInfo("GetPointIDMinPost");
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.GetProcBase))]
        private static void GetProcBasePre()
        {
            Log.LogInfo("GetProcBasePre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.GetProcBase))]
        private static void GetProcBasePost()
        {
            Log.LogInfo("GetProcBasePost");
        }
        */

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.IsAfterIdle))]
        private static void IsAfterIdlePre(Animator _anim)
        {
            Log.LogInfo("IsAfterIdlePre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.IsAfterIdle))]
        private static void IsAfterIdlePost(Animator _anim)
        {
            Log.LogInfo("IsAfterIdlePost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.IsBathCheck))]
        private static void IsBathCheckPre()
        {
            Log.LogInfo("IsBathCheckPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.IsBathCheck))]
        private static void IsBathCheckPost()
        {
            Log.LogInfo("IsBathCheckPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.IsIdle))]
        private static void IsIdlePre(Animator _anim)
        {
            Log.LogInfo("IsIdlePre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.IsIdle))]
        private static void IsIdlePost(Animator _anim)
        {
            Log.LogInfo("IsIdlePost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.IsOloop))]
        private static void IsOloopPre(Animator _anim)
        {
            Log.LogInfo("IsOloopPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.IsOloop))]
        private static void IsOloopPost(Animator _anim)
        {
            Log.LogInfo("IsOloopPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ItemVisible))]
        private static void ItemVisiblePre(bool val)
        {
            Log.LogInfo("ItemVisiblePre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ItemVisible))]
        private static void ItemVisiblePost(bool val)
        {
            Log.LogInfo("ItemVisiblePost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.LimitInitiative))]
        private static void LimitInitiativePre()
        {
            Log.LogInfo("LimitInitiativePre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.LimitInitiative))]
        private static void LimitInitiativePost()
        {
            Log.LogInfo("LimitInitiativePost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.MetaClear))]
        private static void MetaClearPre()
        {
            Log.LogInfo("MetaClearPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.MetaClear))]
        private static void MetaClearPost()
        {
            Log.LogInfo("MetaClearPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.Method_Internal_Static_Boolean_Actor_0))]
        private static void Method_Internal_Static_Boolean_Actor_0Pre(Actor actor)
        {
            Log.LogInfo("Method_Internal_Static_Boolean_Actor_0Pre"); 

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.Method_Internal_Static_Boolean_Actor_0))]
        private static void Method_Internal_Static_Boolean_Actor_0Post(Actor actor)
        {
            Log.LogInfo("Method_Internal_Static_Boolean_Actor_0Post");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ParamContain))]
        private static void ParamContainPre()
        {
            Log.LogInfo("ParamContainPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ParamContain))]
        private static void ParamContainPost()
        {
            Log.LogInfo("ParamContainPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.PositionShift))]
        private static void PositionShiftPre()
        {
            Log.LogInfo("PositionShiftPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.PositionShift))]
        private static void PositionShiftPost()
        {
            Log.LogInfo("PositionShiftPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.PositionShiftMouseUp))]
        private static void PositionShiftMouseUpPre()
        {
            Log.LogInfo("PositionShiftMouseUpPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.PositionShiftMouseUp))]
        private static void PositionShiftMouseUpPost()
        {
            Log.LogInfo("PositionShiftMouseUpPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.RelationSet))]
        private static void RelationSetPre()
        {
            Log.LogInfo("RelationSetPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.RelationSet))]
        private static void RelationSetPost()
        {
            Log.LogInfo("RelationSetPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ReturnFromMovePoint))]
        private static void ReturnFromMovePointPre()
        {
            Log.LogInfo("ReturnFromMovePointPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ReturnFromMovePoint))]
        private static void ReturnFromMovePointPost()
        {
            Log.LogInfo("ReturnFromMovePointPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetCameraLoad))]
        private static void SetCameraLoadPre()
        {
            Log.LogInfo("SetCameraLoadPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetCameraLoad))]
        private static void SetCameraLoadPost()
        {
            Log.LogInfo("SetCameraLoadPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetClothStateStartMotion))]
        private static void SetClothStateStartMotionPre()
        {
            Log.LogInfo("SetClothStateStartMotionPre");

        }

        
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetOldAnimatorInfo))]
        private static void SetOldAnimatorInfoPre()
        {
            Log.LogInfo("SetOldAnimatorInfoPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetOldAnimatorInfo))]
        private static void SetOldAnimatorInfoPost()
        {
            Log.LogInfo("SetOldAnimatorInfoPost");
        }
        */

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetStartVoice))]
        private static void SetStartVoicePre()
        {
            Log.LogInfo("SetStartVoicePre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetStartVoice))]
        private static void SetStartVoicePost()
        {
            Log.LogInfo("SetStartVoicePost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetStateHash))]
        private static void SetStateHashPre()
        {
            Log.LogInfo("SetStateHashPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetStateHash))]
        private static void SetStateHashPost()
        {
            Log.LogInfo("SetStateHashPost");
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ShortcutKey))]
        private static void ShortcutKeyPre()
        {
            Log.LogInfo("ShortcutKeyPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ShortcutKey))]
        private static void ShortcutKeyPost()
        {
            Log.LogInfo("ShortcutKeyPost");
        }
        */

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ShortcutKeyIsMoveScene))]
        private static void ShortcutKeyIsMoveScenePre()
        {
            Log.LogInfo("ShortcutKeyIsMoveScenePre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ShortcutKeyIsMoveScene))]
        private static void ShortcutKeyIsMoveScenePost()
        {
            Log.LogInfo("ShortcutKeyIsMoveScenePost");
        }
        */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.Start))]
        private static void HSceneStartPre()
        {
            Log.LogInfo("HScene.StartPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.Start))]
        private static void HSceneStartPost()
        {
            Log.LogInfo("HScene.StartPost");
        }
        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartAnim))]
        private static void HSceneStartAnimPre()
        {
            Log.LogInfo("HScene.StartAnimPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartAnim))]
        private static void HSceneStartAnimPost()
        {
            Log.LogInfo("HScene.StartAnimPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartAnimDef))]
        private static void StartAnimDefPre()
        {
            Log.LogInfo("HScene.StartAnimDefPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartAnimDef))]
        private static void StartAnimDefPost()
        {
            Log.LogInfo("HScene.StartAnimDefPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartFaintnessCheck), new System.Type[] { })]
        private static void StartFaintnessCheckPre1()
        {
            Log.LogInfo("HScene.StartFaintnessCheckPre1");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartFaintnessCheck), new System.Type[] {} )]
        private static void StartFaintnessCheckPost1()
        {
            Log.LogInfo("HScene.StartFaintnessCheckPost1");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartFaintnessCheck), new [] { typeof(Actor), typeof(int) })]
        private static void StartFaintnessCheckPre2(Actor actor, int f)
        {
            Log.LogInfo("HScene.StartFaintnessCheckPre2");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartFaintnessCheck), new [] { typeof(Actor), typeof(int)})]
        private static void StartFaintnessCheckPost2(Actor actor, int f)
        {
            Log.LogInfo("HScene.StartFaintnessCheckPost2");
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartPointSelect))]
        private static void StartPointSelectPre()
        {
            Log.LogInfo("HScene.StartPointSelectPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartPointSelect))]
        private static void StartPointSelectPost()
        {
            Log.LogInfo("HScene.StartPointSelectPost");
        }
        */

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartPointSelect))]
        private static void StartPointSelectPre()
        {
            Log.LogInfo("HScene.StartPointSelectPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartPointSelect))]
        private static void StartPointSelectPost()
        {
            Log.LogInfo("HScene.StartPointSelectPost");
        }
        */














        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.LoadHScene))]
        private static void LoadHScenePre()
        {
            Log.LogInfo("Manager.HSceneManager.LoadHScenePre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.LoadHScene))]
        private static void LoadHScenePost()
        {
            Log.LogInfo("Manager.HSceneManager.LoadHScenePost");

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.Start))]
        private static void StartPre()
        {
            Log.LogInfo("Manager.HSceneManager.StartPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.Start))]
        private static void StartPost()
        {
            Log.LogInfo("Manager.HSceneManager.StartPost");

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.GetStartPoint))]
        private static void GetStartPointPre()
        {
            Log.LogInfo("Manager.HSceneManager.GetStartPointPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.GetStartPoint))]
        private static void GetStartPointPost()
        {
            Log.LogInfo("Manager.HSceneManager.GetStartPointPost");

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.SetHFlag))]
        private static void SetHFlagPre()
        {
            Log.LogInfo("Manager.HSceneManager.SetHFlagPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.SetHFlag))]
        private static void SetHFlagPost()
        {
            Log.LogInfo("Manager.HSceneManager.SetHFlagPost");

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.SetStartPoint))]
        private static void SetStartPointPre(HPoint point)
        {
            Log.LogInfo("Manager.HSceneManager.SetStartPointPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.SetStartPoint))]
        private static void SetStartPointPost(HPoint point)
        {
            Log.LogInfo("Manager.HSceneManager.SetStartPointPost");

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.ChangeHEventID))]
        private static void ChangeHEventIDPre()
        {
            Log.LogInfo("Manager.HSceneManager.ChangeHEventIDPre");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.ChangeHEventID))]
        private static void ChangeHEventIDPost()
        {
            Log.LogInfo("Manager.HSceneManager.ChangeHEventIDPost");
        }
        */

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.CheckHChara))]
        private static void CheckHCharaPre(Chara.ChaControl cha)
        {
            Log.LogInfo("Manager.HSceneManager.CheckHCharaPre cha: " + cha.FileParam.fullname);
        }

        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.CheckHChara))]
        private static void CheckHCharaPost(Chara.ChaControl cha)
        {
            Log.LogInfo("Manager.HSceneManager.CheckHCharaPost cha: " + cha.FileParam.fullname);
        }
        */

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.MobAnimSet))]
        private static void MobAnimSetPre()
        {
            Log.LogInfo("Manager.HSceneManager.MobAnimSetPre");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.MobAnimSet))]
        private static void MobAnimSetPost()
        {
            Log.LogInfo("Manager.HSceneManager.MobAnimSetPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.MoveForwardProgress))]
        private static void MoveForwardProgressPre()
        {
            Log.LogInfo("Manager.HSceneManager.MoveForwardProgressPre");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.MoveForwardProgress))]
        private static void MoveForwardProgressPost()
        {
            Log.LogInfo("Manager.HSceneManager.MoveForwardProgressPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.MoveForwardProgressTotal))]
        private static void MoveForwardProgressTotalPre()
        {
            Log.LogInfo("Manager.HSceneManager.MoveForwardProgressTotalPre");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.MoveForwardProgressTotal))]
        private static void MoveForwardProgressTotalPost()
        {
            Log.LogInfo("Manager.HSceneManager.MoveForwardProgressTotalPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.PropensitySetValid))]
        private static void PropensitySetValidPre()
        {
            Log.LogInfo("Manager.HSceneManager.PropensitySetValidPre");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.HSceneManager), nameof(Manager.HSceneManager.PropensitySetValid))]
        private static void PropensitySetValidPost()
        {
            Log.LogInfo("Manager.HSceneManager.PropensitySetValidPost");
        }
        */






        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.Start))]
        private static void Start()
        {
            Log.LogInfo("HScene.Start");
            //Patches.ChangeActorsAnimation(__instance);
        }


        */
        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartFaintnessCheck), new System.Type[] {})]
        private static void StartFaintnessCheck()
        {
            Log.LogInfo("HScene.StartFaintnessCheck");
            //Patches.ChangeActorsAnimation(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartFaintnessCheck), new System.Type[] { typeof(Actor), typeof(int)})]
        private static void StartFaintnessCheck2(Actor actor, int f)
        {
            Log.LogInfo("HScene.StartFaintnessCheck2");
            //Patches.ChangeActorsAnimation(__instance);
        }
        */
        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.LimitInitiative))]
        private static void LimitInitiative(Dictionary<int, List<HScene.AnimationListInfo>> check, Dictionary<int, List<int>> map, int numInitiativeFemale)
        {
            Log.LogInfo("LimitInitiative.LimitInitiative");
            //Patches.ChangeActorsAnimation(__instance);
        }
        */

        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.Init))]
        private static void Init()
        {
            Log.LogInfo("HSceneSprite.Init");
            //Patches.ChangeActorsAnimation(__instance);
        }
        */


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Actor), nameof(Actor.FilterCommands))]
        private static void FilterCommands(Actor __instance, IReadOnlyList<RG.Scripts.ActionCommand> commands, List<RG.Scripts.ActionCommand> dest, bool errorOnly)
        {
            //Debug.PrintDetail(__instance.Chara);


            if (__instance.Sex == 1)
            {

                Log.LogInfo("FilterCommands Name: " + __instance.Status.FullName + ", personality: " + __instance.Chara.FileParam.personality);
                //Debug.PrintTransformTree(__instance.Chara.CmpBody.targetEtc.objMNPB.transform, "");
                //Debug.PrintDetail(__instance.Chara.CmpBody.targetEtc.objDanTop);

                //__instance.Chara.SetClothesStateAll(2);


                /*
                var lefthand = __instance.Partner.Chara.CmpBoneBody.targetEtc.trf_k_handL_00.parent;
                var righthand = __instance.Partner.Chara.CmpBoneBody.targetEtc.trf_k_handR_00.parent;


                //lefthand.gameObject.active = false;
                //lefthand.position += new Vector3(5f, 5f, 5f);
                //righthand.position += new Vector3(2f, 2f, 2f);cf_J_Hand_s_L

                var lefthandmiddle = __instance.Partner.Chara.transform.Find("BodyTop/p_cf_anim/cf_J_Root/cf_N_height/cf_J_Hips/cf_J_Spine01/cf_J_Spine02/cf_J_Spine03/cf_J_ShoulderIK_L/cf_J_Shoulder_L/cf_J_ArmUp00_L/cf_J_ArmLow01_L/cf_J_Hand_L/cf_J_Hand_s_L/cf_J_Hand_Middle01_L");
                var lefthandlittle = __instance.Partner.Chara.transform.Find("BodyTop/p_cf_anim/cf_J_Root/cf_N_height/cf_J_Hips/cf_J_Spine01/cf_J_Spine02/cf_J_Spine03/cf_J_ShoulderIK_L/cf_J_Shoulder_L/cf_J_ArmUp00_L/cf_J_ArmLow01_L/cf_J_Hand_L/cf_J_Hand_s_L/cf_J_Hand_Little01_L");

                var righthandmiddle = __instance.Partner.Chara.transform.Find("BodyTop/p_cf_anim/cf_J_Root/cf_N_height/cf_J_Hips/cf_J_Spine01/cf_J_Spine02/cf_J_Spine03/cf_J_ShoulderIK_R/cf_J_Shoulder_R/cf_J_ArmUp00_R/cf_J_ArmLow01_R/cf_J_Hand_R/cf_J_Hand_s_R/cf_J_Hand_Middle01_R");
                var righthandlittle = __instance.Partner.Chara.transform.Find("BodyTop/p_cf_anim/cf_J_Root/cf_N_height/cf_J_Hips/cf_J_Spine01/cf_J_Spine02/cf_J_Spine03/cf_J_ShoulderIK_R/cf_J_Shoulder_R/cf_J_ArmUp00_R/cf_J_ArmLow01_R/cf_J_Hand_R/cf_J_Hand_s_R/cf_J_Hand_Little01_R");

                var lefthandtiptest = __instance.Partner.Chara.transform.Find("BodyTop/p_cf_anim/cf_J_Root/cf_N_height/cf_J_Hips/cf_J_Spine01/cf_J_Spine02/cf_J_Spine03/cf_J_ShoulderIK_L/cf_J_Shoulder_L/cf_J_ArmUp00_L/cf_J_ArmLow01_L/cf_J_Hand_L/cf_J_Hand_s_L/cf_J_Hand_Middle01_L/N_Middle_L");
                var righthandtiptest = __instance.Partner.Chara.transform.Find("BodyTop/p_cf_anim/cf_J_Root/cf_N_height/cf_J_Hips/cf_J_Spine01/cf_J_Spine02/cf_J_Spine03/cf_J_ShoulderIK_R/cf_J_Shoulder_R/cf_J_ArmUp00_R/cf_J_ArmLow01_R/cf_J_Hand_R/cf_J_Hand_s_R/cf_J_Hand_Middle01_R/N_Middle_R");

                if (StateManager.Instance.left == null)
                    StateManager.Instance.left = lefthandtiptest;
                if (StateManager.Instance.right == null)
                    StateManager.Instance.right = righthandtiptest;

                //lefthandmiddle.gameObject.active = false;
                //righthandlittle.gameObject.active = false;

                var leftbreast = __instance.Chara.transform.Find("BodyTop/p_cf_anim/cf_J_Root/cf_N_height/cf_J_Hips/cf_J_Spine01/cf_J_Spine02/cf_J_Spine03/cf_J_Mune00/cf_J_Mune00_t_L");
                var rightbreast = __instance.Chara.transform.Find("BodyTop/p_cf_anim/cf_J_Root/cf_N_height/cf_J_Hips/cf_J_Spine01/cf_J_Spine02/cf_J_Spine03/cf_J_Mune00/cf_J_Mune00_t_R");

                var leftNipple = __instance.Chara.transform.Find("BodyTop/p_cf_anim/cf_J_Root/cf_N_height/cf_J_Hips/cf_J_Spine01/cf_J_Spine02/cf_J_Spine03/cf_J_Mune00/cf_J_Mune00_t_L/cf_J_Mune00_L/cf_J_Mune00_s_L/cf_J_Mune00_d_L/cf_J_Mune01_L/cf_J_Mune01_s_L/cf_J_Mune01_t_L/cf_J_Mune02_L/cf_J_Mune02_s_L/cf_J_Mune02_t_L/cf_J_Mune03_L/cf_J_Mune03_s_L/cf_J_Mune04_s_L/cf_J_Mune_Nip01_L/cf_J_Mune_Nip01_s_L/cf_J_Mune_Nip02_L/cf_J_Mune_Nipacs01_L/N_Tikubi_L");
                var rightNipple = __instance.Chara.transform.Find("BodyTop/p_cf_anim/cf_J_Root/cf_N_height/cf_J_Hips/cf_J_Spine01/cf_J_Spine02/cf_J_Spine03/cf_J_Mune00/cf_J_Mune00_t_R/cf_J_Mune00_R/cf_J_Mune00_s_R/cf_J_Mune00_d_R/cf_J_Mune01_R/cf_J_Mune01_s_R/cf_J_Mune01_t_R/cf_J_Mune02_R/cf_J_Mune02_s_R/cf_J_Mune02_t_R/cf_J_Mune03_R/cf_J_Mune03_s_R/cf_J_Mune04_s_R/cf_J_Mune_Nip01_R/cf_J_Mune_Nip01_s_R/cf_J_Mune_Nip02_R/cf_J_Mune_Nipacs01_R/N_Tikubi_R");
                //var leftNipple = __instance.Chara.transform.Find("BodyTop/p_cf_anim/cf_J_Root/cf_N_height/cf_J_Hips/cf_J_Spine01/cf_J_Spine02/cf_J_Spine03/cf_J_Mune00/cf_J_Mune00_t_L/cf_J_Mune00_L/cf_J_Mune00_s_L/cf_J_Mune00_d_L/cf_J_Mune01_L/cf_J_Mune01_s_L/cf_J_Mune01_t_L/cf_J_Mune02_L/cf_J_Mune02_s_L/cf_J_Mune02_t_L/cf_J_Mune03_L/cf_J_Mune03_s_L/cf_J_Mune04_s_L/cf_J_Mune_Nip01_L/cf_J_Mune_Nip01_s_L");
                //var rightNipple = __instance.Chara.transform.Find("BodyTop/p_cf_anim/cf_J_Root/cf_N_height/cf_J_Hips/cf_J_Spine01/cf_J_Spine02/cf_J_Spine03/cf_J_Mune00/cf_J_Mune00_t_R/cf_J_Mune00_R/cf_J_Mune00_s_R/cf_J_Mune00_d_R/cf_J_Mune01_R/cf_J_Mune01_s_R/cf_J_Mune01_t_R/cf_J_Mune02_R/cf_J_Mune02_s_R/cf_J_Mune02_t_R/cf_J_Mune03_R/cf_J_Mune03_s_R/cf_J_Mune04_s_R/cf_J_Mune_Nip01_R/cf_J_Mune_Nip01_s_R");

                var leftdiff = rightNipple.position - StateManager.Instance.left.position;
                var rightdiff = leftNipple.position - StateManager.Instance.right.position;

                var tkb = __instance.Chara.CmpBoneBody.targetAccessory.acs_Tikubi_L;

                __instance.Chara.SetClothesStateAll(2);
                leftNipple.gameObject.active = false;
                rightNipple.gameObject.active = false;
                
                lefthand.position = rightNipple.position + leftdiff;
                righthand.position = leftNipple.position + rightdiff;
                

                //__instance.Partner.Chara.SetPosition(leftNipple.position);
                __instance.Chara.SetPosition(lefthand.position);

                Log.LogInfo("lefthand: " + lefthand.position
                    + ", righthand: " + righthand.position
                    + ", lefthandmiddle: " + lefthandmiddle.position
                    + ", lefthandlittle: " + lefthandlittle.position
                    + ", lefthandtiptest: " + lefthandtiptest.position
                    );
                Log.LogInfo("leftbreast: " + leftbreast.position
                    + ", rightbreast: " + rightbreast.position
                    + ", leftNipple: " + leftNipple.position
                    + ", rightNipple: " + rightNipple.position

                    + ", leftdiff: " + leftdiff
                    + ", rightdiff: " + rightdiff
                    );
                Log.LogInfo("tkb left: " + tkb.position + ", name: " + tkb.name);
                */

                //leftbreast.gameObject.active = false;
                //rightbreast.gameObject.active = false;

                if (__instance.CharaFileName == "default3")
                {
                    /*
                    StateManager.Instance.ActorHAnimationList = new Dictionary<int, InfoList.HAnimation.ActorHAnimData>();
                    Patches.HAnim.StartHAnimation(__instance);
                    */







                    /*
                    var lefthand = __instance.Chara.CmpBoneBody.targetEtc.trf_k_handL_00.parent.parent;
                    lefthand.position += new Vector3(.5f, .5f, .5f);
                    */
                    //__instance.Chara.CmpBoneBody.targetEtc.trfHeadParent.transform.position += new Vector3(-10, -20, -30);
                    //__instance.Chara.CmpBoneBody.targetEtc.trf_k_handL_00.transform.position += new Vector3(10, 20, 30);
                    //__instance.Chara.CmpBoneBody.targetEtc.trf_k_shoulderR_00.transform.position += new Vector3(10, 20, 30);

                    /*
                    if (Manager.HSceneManager.HResourceTables != null)
                    {
                        Log.LogInfo("Manager.HSceneManager.HResourceTables not null");
                        if(Manager.HSceneManager.HResourceTables.LstAnimInfo != null)
                        {
                            Log.LogInfo("Manager.HSceneManager.HResourceTables.LstAnimInfo not null, count: " + Manager.HSceneManager.HResourceTables.LstAnimInfo.Count);
                        }
                        
                    }
                    */

                    /*
                    //__instance.Chara.CmpBoneBody.targetEtc.trfHeadParent.transform.position += new Vector3(10, 20, 30);
                    __instance.Chara.CmpBoneBody.targetEtc.trfNeckLookTarget.transform.position += new Vector3(10, 20, 30);

                    //Debug.PrintRenderer(__instance.Chara.CmpBody.targetEtc.objDanTop.transform, "");
                    Log.LogInfo("==========");
                    Log.LogInfo("__instance.Chara.CmpBody: " + __instance.Chara.CmpBody.gameObject.name);
                    Debug.PrintDetail(__instance.Chara.CmpBody.targetEtc);
                    Log.LogInfo("==========");
                    Log.LogInfo("__instance.Chara.CmpBoneBody: " + __instance.Chara.CmpBoneBody.gameObject.name);
                    Debug.PrintDetail(__instance.Chara.CmpBoneBody.targetEtc);
                    Log.LogInfo("==========");
                    Log.LogInfo("__instance.Chara.CmpFace: " + __instance.Chara.CmpFace.gameObject.name);
                    Debug.PrintDetail(__instance.Chara.CmpFace.targetEtc);
                    Log.LogInfo("==========");
                    Log.LogInfo("__instance.Chara.CmpSimpleBody: " + __instance.Chara.CmpSimpleBody.gameObject.name);
                    Debug.PrintDetail(__instance.Chara.CmpSimpleBody.targetEtc);
                    Log.LogInfo("==========");
                    Log.LogInfo("__instance.Chara.CmpBoneHead: " + __instance.Chara.CmpBoneHead.gameObject.name);
                    Debug.PrintDetail(__instance.Chara.CmpBoneHead.targetEtc);
                    Log.LogInfo("==========");

                    Debug.PrintDetail(__instance.Chara);
                    */
                }

            }

            //Log.LogInfo("Name: " + __instance.Status.FullName + ", OccupyiedPt: " + __instance.OccupiedActionPoint?.name);
            /*
            Log.LogInfo("%%%%%%%%%");
            Log.LogInfo("Name: " + __instance.Status.FullName);
            Debug.PrintDetail(__instance);
            if (__instance.Partner != null)
                Log.LogInfo("Name: " + __instance.Status.FullName + ", Partner: " + __instance.Partner.Status.FullName);
            else
                Log.LogInfo("Name: " + __instance.Status.FullName  + ", Partner null");
            Log.LogInfo("%%%%%%%%%");
            */

        }

        //Force showing the penis of the male characters if flag is set
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameObject), nameof(GameObject.SetActive))]
        private static void SetActivePost(GameObject __instance, bool value)
        {
            if (StateManager.Instance.CurrentHSceneInstance != null && StateManager.Instance.ForceActiveInstanceID != null)
            {
                //Log.LogInfo("SetActivePost");
                if (StateManager.Instance.ForceActiveInstanceID.Contains(__instance.GetInstanceID()))
                    __instance.active = true;
            }

        }




        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.OnClickConfig))]
        private static void OnClickConfig(HSceneSprite __instance)
        {
            Log.LogInfo("OnClickConfig");

            var male = StateManager.Instance.CurrentHSceneInstance._chaMales[0];
            var female = StateManager.Instance.CurrentHSceneInstance._chaFemales[0];

            var animInfo = Manager.HSceneManager.HResourceTables.LstAnimInfo[4][0];
            for (int i = 0; i < animInfo.FemaleLowerCloths.Count; i++)
            {
                Log.LogInfo("animInfo.FemaleLowerCloths[" + i + "]: " + animInfo.FemaleLowerCloths[i]);
            }
            for (int i = 0; i < animInfo.FemaleUpperCloths.Count; i++)
            {
                Log.LogInfo("animInfo.FemaleUpperCloths[" + i + "]: " + animInfo.FemaleUpperCloths[i]);
            }
            var lefthand = male.CmpBoneBody.targetEtc.trf_k_handL_00.parent;
            lefthand.position += new Vector3(10, 10, 10);

            






            /*
            if (male.NeckLookCtrl != null)
            {
                if (male.NeckLookCtrl.neckLookScript != null)
                {
                    Debug.PrintDetail(male.NeckLookCtrl.neckLookScript);
                    //Debug.PrintTransformTreeUpward(male.NeckLookCtrl.neckLookScript.boneCalcAngle, "");
                }
            }
            if (female.NeckLookCtrl != null)
            {
                if (female.NeckLookCtrl.neckLookScript != null)
                {
                    Debug.PrintDetail(female.NeckLookCtrl.neckLookScript);
                    //Debug.PrintTransformTreeUpward(female.NeckLookCtrl.neckLookScript.boneCalcAngle, "");
                }
            }
            */


            /*
            var character = StateManager.Instance.CurrentHSceneInstance._chaFemales[0];
            Log.LogInfo("character.CmpBoneHead.name: " + character.CmpBoneHead.name);
            Log.LogInfo("character.ObjHead.name: " + character.ObjHead.name);
            Log.LogInfo("character.ObjHeadBone.name: " + character.ObjHeadBone.name);
            Log.LogInfo("character.ObjHitHead.name: " + character.ObjHitHead.name);
            Log.LogInfo("character.CmpBoneHead.targetEtc.trfHairParent.name: " + character.CmpBoneHead.targetEtc.trfHairParent.name);
            Log.LogInfo("character.CmpBoneHead.targetEtc.trfMouthAdjustWidth.name: " + character.CmpBoneHead.targetEtc.trfMouthAdjustWidth.name);

            Debug.PrintDetail(character.CmpBoneBody.targetEtc);
            Debug.PrintDetail(character.CmpBody.targetEtc);
            */

            //Debug.PrintDetail(character);

            //Debug.PrintTransformTree(character.transform, "");
            /*
            if (Manager.Voice._transTable != null)
            {
                Log.LogInfo("_transTable Count: " + Manager.Voice._transTable.Count);
                foreach (var kvp in Manager.Voice._transTable)
                {
                    Log.LogInfo("_transTable kvp.Key: " + kvp.Key + ", transform: " + kvp.Value.name);
                    Debug.PrintTransformTree(kvp.Value, "");
                }

            }
            */

        }

        internal static void PrintHitObjectCtrl(HitObjectCtrl ctrl)
        {
            if (ctrl != null)
            {
                Log.LogInfo("PrintHitObjectCtrl name: " + ctrl._chaControl?.FileParam.fullname);
                Debug.PrintDetail(ctrl);

                if (ctrl._atariName != null)
                    foreach (var s in ctrl._atariName)
                        Log.LogInfo("_atariName: " + s);
                if (ctrl._lstInfo != null)
                    foreach (var s in ctrl._lstInfo)
                    {
                        Log.LogInfo("_lstInfo: ");
                        Debug.PrintDetail(s);
                        for (int i = 0; i < s.LstIsActive.Count; i++)
                            Log.LogInfo("LstIsActive[" + i + "]: " + s.LstIsActive[i]);
                    }
                if (ctrl._lstObject != null)
                    foreach (var s in ctrl._lstObject)
                        Log.LogInfo("_lstObject: " + s.Name + ", object name: " + s.Obj?.name);
                if (ctrl._tmpDic != null)
                {
                    foreach (var kvp in ctrl._tmpDic)
                    {
                        Log.LogInfo("_tmpDic kvp.Key: " + kvp.Key + ", value count: " + kvp.Value.Count);
                        foreach (var kvp2 in kvp.Value)
                        {
                            Log.LogInfo("_tmpDic kvp.Key: " + kvp.Key + ", kvp2.Key: " + kvp2.Key + ", Value name: " + kvp2.Value.name);
                        }
                    }
                }
                if (ctrl._tmpLst != null)
                {
                    foreach (var kvp in ctrl._tmpLst)
                    {
                        Log.LogInfo("_tmpLst kvp.Key: " + kvp.Key + ", value name: " + kvp.Value.name);
                    }
                }
                if (ctrl.getChild != null)
                {
                    for (int i = 0; i < ctrl.getChild.Count; i++)
                        Log.LogInfo("getChild[" + i + "]: " + ctrl.getChild[i].gameObject.name);
                }
                if (ctrl.row != null)
                    foreach (var s in ctrl.row)
                        Log.LogInfo("row: " + s);
                if (HitObjectCtrl.lstHitObject != null)
                {
                    foreach (var s in HitObjectCtrl.lstHitObject)
                    {
                        Log.LogInfo("lstHitObject: " + s);
                    }
                }
                Log.LogInfo("=======");
            }
        }

        internal static void RecurAddTransformToList(List<Transform> list, Transform t)
        {
            if (t != null)
            {
                list.Add(t);

                for (int i = 0; i < t.GetChildCount(); i++)
                {

                    //Log.LogInfo("Visiting the parent of [" + t.name + "]");
                    RecurAddTransformToList(list, t.GetChild(i));
                }
            }
        }



        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.OnClickTaiiCategory))]
        private static void OnClickTaiiCategory(HSceneSprite __instance)
        {
            Log.LogInfo("OnClickTaiiCategory");




            if (ActionScene.Instance != null)
            {
                Actor male = null;
                Actor female = null;
                var list = Patches.General.GetActorsInvolvedInH(ActionScene.Instance, StateManager.Instance.CurrentHSceneInstance);
                foreach (var actor in list)
                {
                    Log.LogInfo("Name: " + actor.Status.FullName);
                    //Debug.PrintAnimationParameter(actor.Animation.Param);




                    if (actor.Sex == 0)
                    {
                        male = actor;
                        male.Chara.VisibleSon = true;

                        //Debug.PrintTransformTree(actor.Chara.CmpBody.targetEtc.objMNPB.transform, "");
                        //Debug.PrintDetail(actor.Chara.CmpBody.targetEtc.objDanTop);
                    }
                    else
                        female = actor;
                }

                if (StateManager.Instance.CurrentHSceneInstance != null)
                {



                    if (male != null && female != null)
                    {


                        //y.lstInfo
                        //y.info

                        /*
                        Debug.PrintDetail(yureFemale);
                        foreach (var item in yureFemale.lstInfo)
                        {
                            Log.LogInfo("%%%%%%%%");
                            Debug.PrintDetail(item);
                            Log.LogInfo("%%%%%%%%");
                        }
                        Log.LogInfo("============================!!!!");
                        */


                        //male.Chara.confSon = true;

                        male.Chara.LoadHitObject();
                        female.Chara.LoadHitObject();

                        male.Chara.confSon = true;
                        male.Chara.VisibleSon = true;
                        //male.Chara.ResetDynamicBoneALL();

                        //male.Chara.CmpBody.targetEtc.objMNPB.active = true;
                        male.Chara.CmpBody.targetEtc.objDanSao.active = true;
                        male.Chara.CmpBody.targetEtc.objDanTama.active = true;
                        male.Chara.CmpBody.targetEtc.objDanTop.active = true;

                        StateManager.Instance.ForceActiveInstanceID.Add(male.Chara.CmpBody.targetEtc.objDanSao.GetInstanceID());
                        StateManager.Instance.ForceActiveInstanceID.Add(male.Chara.CmpBody.targetEtc.objDanTama.GetInstanceID());
                        StateManager.Instance.ForceActiveInstanceID.Add(male.Chara.CmpBody.targetEtc.objDanTop.GetInstanceID());



                        //var racFemale = female.Chara.LoadAnimation("animator/h/female/00/sonyu.unity3d", "rgs_f_13");
                        //var racFemale = female.Chara.LoadAnimation("animator/h/female/00/sonyu.unity3d", "rgs_f_56");


                        var racFemale = female.Chara.LoadAnimation("animator/h/female/00/houshi.unity3d", "rgh_f_20");

                        //var racFemaleNeck = female.Chara.LoadAnimation("animator/h/female/00/houshi.unity3d", "neck_rgh_f_06");
                        //female.Animation.SetAnimatorController(racFemaleNeck);


                        HMotionEyeNeckFemale femaleNeckMotion = new HMotionEyeNeckFemale();
                        femaleNeckMotion.Init(female.Chara, 0, StateManager.Instance.CurrentHSceneInstance);
                        femaleNeckMotion.Load("list/h/neckcontrol/", "neck_rgh_f_19");
                        femaleNeckMotion.SetPartnerMaleObj(male.Chara._objBodyBone, null);
                        femaleNeckMotion.SetPartnerFemaleObj(null);
                        femaleNeckMotion.SetPartner(male.Chara._objBodyBone, null, null);

                        HVoiceCtrl.FaceInfo faceInfo = new HVoiceCtrl.FaceInfo();
                        faceInfo.OpenEye = female.Chara.EyesCtrl.openRate;
                        faceInfo.OpenMouthMin = female.Chara.GetMouthOpenMin();
                        faceInfo.OpenMouthMax = female.Chara.GetMouthOpenMax();
                        faceInfo.EyeBlow = female.Chara.GetEyebrowPtn();
                        faceInfo.Eye = female.Chara.GetEyesPtn();
                        faceInfo.Mouth = female.Chara.GetMouthPtn();
                        faceInfo.Tear = female.Chara.TearsRate;
                        faceInfo.Cheek = female.Chara.FileFace.makeup.cheekGloss;
                        faceInfo.Highlight = !female.Chara.FileStatus.hideEyesHighlight;
                        faceInfo.Blink = female.Chara.FileStatus.eyesBlink;
                        faceInfo.BehaviorEyeLine = 1;
                        faceInfo.BehaviorNeckLine = 1;
                        faceInfo.TargetNeckLine = 1;
                        faceInfo.TargetEyeLine = 1;
                        femaleNeckMotion.Proc(female.Chara.AnimBody.GetCurrentAnimatorStateInfo(0), faceInfo, 0);



                        HMotionEyeNeckMale maleNeckMotion = new HMotionEyeNeckMale();
                        maleNeckMotion.Init(male.Chara, 0);
                        maleNeckMotion.Load("list/h/neckcontrol/", "neck_rgh_m_19");
                        maleNeckMotion.SetPartnerMaleObj(null);
                        maleNeckMotion.SetPartnerFemaleObj(female.Chara._objBodyBone, null);
                        maleNeckMotion.SetPartner(female.Chara._objBodyBone, null, null);
                        maleNeckMotion.Proc(male.Chara.AnimBody.GetCurrentAnimatorStateInfo(0));


                        //var racMale = male.Chara.LoadAnimation("animator/h/male/00/sonyu.unity3d", "rgs_m_13");

                        //var racMale = male.Chara.LoadAnimation("animator/h/male/00/sonyu.unity3d", "rgs_m_56");
                        var racMale = male.Chara.LoadAnimation("animator/h/male/00/houshi.unity3d", "rgh_m_20");
                        female.Animation.SetRegularAnimator(racFemale);
                        male.Animation.SetRegularAnimator(racMale);

                        female.Chara.PlaySync("M_WLoop1", -1, 0);
                        male.Chara.PlaySync("M_WLoop1", -1, 0);
                        //
                        Debug.PrintDetail(female.Chara);
                        try
                        {
                            Log.LogInfo("female speed: " + female.Chara.AnimBody.speed + ", male speed: " + male.Chara.AnimBody.speed);
                            female.Chara.AnimBody.speed = 3f;
                            male.Chara.AnimBody.speed = 3f;


                        }
                        catch { }

                        HitObjectCtrl hitObjectCtrlMale = new HitObjectCtrl();
                        hitObjectCtrlMale._sex = 0;
                        hitObjectCtrlMale._chaControl = male.Chara;
                        hitObjectCtrlMale.IsInit = true;
                        hitObjectCtrlMale.Id = 0;
                        var a = hitObjectCtrlMale.HitObjInit(0, male.Chara.ObjBodyBone, male.Chara);
                        List<Transform> maleTransformList = new List<Transform>();
                        RecurAddTransformToList(maleTransformList, male.Chara.ObjBodyBone.transform);
                        UnhollowerBaseLib.Il2CppReferenceArray<Transform> maleTransformArray = new UnhollowerBaseLib.Il2CppReferenceArray<Transform>(maleTransformList.Count);
                        int counter = 0;
                        foreach (var t in maleTransformList)
                        {
                            maleTransformArray[counter] = t;
                            counter++;
                        }
                        hitObjectCtrlMale.getChild = maleTransformArray;

                        hitObjectCtrlMale.SetActiveObject(false);
                        hitObjectCtrlMale.HitObjLoadExcel("rgh_m_19");
                        hitObjectCtrlMale.Proc("WLoop");

                        //PrintHitObjectCtrl(hitObjectCtrlMale);

                        HitObjectCtrl hitObjectCtrlFemale = new HitObjectCtrl();
                        hitObjectCtrlFemale._sex = 1;
                        hitObjectCtrlFemale._chaControl = female.Chara;
                        hitObjectCtrlFemale.IsInit = true;
                        hitObjectCtrlFemale.Id = 0;
                        hitObjectCtrlFemale.HitObjInit(1, female.Chara.ObjBodyBone, female.Chara);

                        List<Transform> femaleTransformList = new List<Transform>();
                        RecurAddTransformToList(femaleTransformList, female.Chara.ObjBodyBone.transform);
                        UnhollowerBaseLib.Il2CppReferenceArray<Transform> femaleTransformArray = new UnhollowerBaseLib.Il2CppReferenceArray<Transform>(femaleTransformList.Count);
                        counter = 0;
                        foreach (var t in femaleTransformList)
                        {
                            femaleTransformArray[counter] = t;
                            counter++;
                        }
                        hitObjectCtrlFemale.getChild = femaleTransformArray;
                        hitObjectCtrlFemale.SetActiveObject(false);
                        hitObjectCtrlFemale.HitObjLoadExcel("rgh_f_19");
                        hitObjectCtrlFemale.Proc("WLoop");



                        if (female.OccupiedActionPoint != null)
                        {



                            var position = female.OccupiedActionPoint.HPointLink[0].transform.position;

                            female.OccupiedActionPoint.HPointLink[0].ChangeHideProcBefore();
                            female.OccupiedActionPoint.HPointLink[0].ChangeHideProc(1);
                            //female.OccupiedActionPoint.HPointLink[0].VisibleObj(new Il2CppSystem.Collections.Generic.List<int>());
                            //female.OccupiedActionPoint.HPointLink[0].SetOffset(0);
                            //female.OccupiedActionPoint.HPointLink[0].SetOffset();
                            female.OccupiedActionPoint.HPointLink[0].HpointObjVisibleChange(true);

                            /*
                            foreach(var a in AssetBundle.GetAllLoadedAssetBundles_Native())
                            {
                                Log.LogInfo(a.name);
                            }
                            */

                            for (int i = 0; i < female.OccupiedActionPoint.HPointLink[0]._moveObjects.Count; i++)
                            {
                                var moveobj = female.OccupiedActionPoint.HPointLink[0]._moveObjects[i];
                                Log.LogInfo("i: " + i + ", Name: " + moveobj.MoveObjName + ", base pos: " + moveobj.BasePos + ", base rot: " + moveobj.BaseRot);
                            }


                            foreach (var item in female.OccupiedActionPoint.HPointLink[0].MotionChairIDs)
                            {


                                var abinfo = Manager.HSceneManager.HResourceTables.DicDicMapDependItemInfo[item];
                                var fullpath = System.IO.Path.Combine(Application.dataPath.Replace("RoomGirl_Data", "abdata"), abinfo.assetbundle);

                                var ab = AssetBundle.LoadFromFile(fullpath);
                                //var loadedab = AssetBundleManager.LoadAssetBundle(fullpath, abinfo.manifest);
                                /*
                                if (loadedab != null)
                                {
                                    var ab2 = loadedab.Bundle;
                                    Log.LogInfo("loadedab not null");
                                    if(ab2 != null)
                                        Log.LogInfo("loadedab.Bundle not null");
                                    else
                                        Log.LogInfo("loadedab.Bundle is null");
                                }
                                else
                                    Log.LogInfo("loadedab is null");
                                */

                                if (ab != null)
                                {
                                    GameObject obj = Util.InstantiateFromBundle(ab, abinfo.asset);

                                    obj.transform.position = position;
                                    obj.transform.localPosition = female.OccupiedActionPoint.HPointLink[0].transform.localPosition;
                                    obj.transform.rotation = female.OccupiedActionPoint.HPointLink[0].transform.rotation;
                                    obj.transform.localRotation = female.OccupiedActionPoint.HPointLink[0].transform.localRotation;

                                    obj.transform.parent = StateManager.Instance.CurrentHSceneInstance.gameObject.transform;
                                    //Debug.PrintTransformTree(obj.transform, "");
                                }


                                ab.Unload(false);


                            }


                            female.transform.position = position;
                            female.transform.localPosition = female.OccupiedActionPoint.HPointLink[0].transform.localPosition;
                            female.transform.rotation = female.OccupiedActionPoint.HPointLink[0].transform.rotation;

                            /*
                            female.transform.rotation = new Quaternion(female.OccupiedActionPoint.HPointLink[0].transform.rotation.x,
                                female.OccupiedActionPoint.HPointLink[0].transform.rotation.y,
                                female.OccupiedActionPoint.HPointLink[0].transform.rotation.z,
                                female.OccupiedActionPoint.HPointLink[0].transform.rotation.w
                                );
                            */
                            female.transform.localRotation = female.OccupiedActionPoint.HPointLink[0].transform.localRotation;
                            male.transform.position = position;
                            male.transform.localPosition = female.OccupiedActionPoint.HPointLink[0].transform.localPosition;
                            male.transform.rotation = female.OccupiedActionPoint.HPointLink[0].transform.rotation;
                            /*
                            male.transform.rotation = new Quaternion(female.OccupiedActionPoint.HPointLink[0].transform.rotation.x,
                                female.OccupiedActionPoint.HPointLink[0].transform.rotation.y,
                                female.OccupiedActionPoint.HPointLink[0].transform.rotation.z,
                                female.OccupiedActionPoint.HPointLink[0].transform.rotation.w
                                );
                            */
                            male.transform.localRotation = female.OccupiedActionPoint.HPointLink[0].transform.localRotation;


                            male.transform.position = male.transform.position;// + new Vector3(0, 0.2f, 0);
                            Log.LogInfo("male original transform: " + male.Chara.transform.position + ", local: " + male.Chara.transform.localPosition);
                            Log.LogInfo("female original transform: " + female.Chara.transform.position + ", local: " + female.Chara.transform.localPosition);
                            //male.transform.rotation *= Quaternion.Inverse(male.transform.rotation);
                            //female.transform.rotation *= Quaternion.Inverse(female.transform.rotation);

                            male.Chara.SetClothesStateAll(2);
                            female.Chara.SetClothesStateAll(2);
                            //male.transform.rotation = Quaternion.Inverse(male.transform.rotation);

                            //male.transform.RotateAround(position, Vector3.up, 180f);


                            //var targetAngles = female.OccupiedActionPoint.HPointLink[0].transform.eulerAngles + 180f * Vector3.up;
                            //male.transform.eulerAngles = Vector3.Lerp(female.OccupiedActionPoint.HPointLink[0].transform.eulerAngles, targetAngles, 0);
                            //male.transform.position = position + new Vector3(10f, 20, 30);
                            //male.transform.localPosition = female.OccupiedActionPoint.HPointLink[0].transform.localPosition;
                            //male.Chara.transform.LookAt(female.transform.position);
                            //male.Chara.SetRotation(rot);HMotionEyeNeckFemale
                            //male.Chara.transform.forward = rot;

                            Log.LogInfo("pt2");





                            Debug.PrintDetail(female.OccupiedActionPoint.HPointLink[0]);

                            //ActionScene.
                        }

                    }

                }

            }
        }

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.SetPartner))]
        private static void SetPartnerPre(HMotionEyeNeckFemale __instance, GameObject _objMale1Bone, GameObject _objMale2Bone, GameObject _objFemale1Bone)
        {
            Log.LogInfo("HMotionEyeNeckFemale.SetPartnerPre name: " + __instance.chaFemale?.FileParam.fullname
                + ", _objMale1Bone: " + _objMale1Bone?.name
                + ", _objMale2Bone: " + _objMale2Bone?.name
                + ", _objFemale1Bone: " + _objFemale1Bone?.name
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.SetPartner))]
        private static void SetPartnerPost(HMotionEyeNeckFemale __instance, GameObject _objMale1Bone, GameObject _objMale2Bone, GameObject _objFemale1Bone)
        {
            Log.LogInfo("HMotionEyeNeckFemale.SetPartnerPost name: " + __instance.chaFemale?.FileParam.fullname);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.SetPartnerFemaleObj))]
        private static void SetPartnerFemaleObjPre(HMotionEyeNeckFemale __instance, GameObject _objFemale1Bone)
        {
            Log.LogInfo("HMotionEyeNeckFemale.SetPartnerFemaleObjPre name: " + __instance.chaFemale?.FileParam.fullname
                + ", _objFemale1Bone: " + _objFemale1Bone?.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.SetPartnerFemaleObj))]
        private static void SetPartnerFemaleObjPost(HMotionEyeNeckFemale __instance, GameObject _objFemale1Bone)
        {
            Log.LogInfo("HMotionEyeNeckFemale.SetPartnerFemaleObjPost name: " + __instance.chaFemale?.FileParam.fullname);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.SetPartnerMaleObj))]
        private static void SetPartnerMaleObjPre(HMotionEyeNeckFemale __instance, GameObject _objMale1Bone, GameObject _objMale2Bone)
        {
            Log.LogInfo("HMotionEyeNeckFemale.SetPartnerMaleObjPre name: " + __instance.chaFemale?.FileParam.fullname
                + ", _objMale1Bone: " + _objMale1Bone?.name
                + ", _objMale2Bone: " + _objMale2Bone?.name
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.SetPartnerMaleObj))]
        private static void SetPartnerMaleObjPost(HMotionEyeNeckFemale __instance, GameObject _objMale1Bone, GameObject _objMale2Bone)
        {
            Log.LogInfo("HMotionEyeNeckFemale.SetPartnerMaleObjPost name: " + __instance.chaFemale?.FileParam.fullname);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.ChangeAnimSet))]
        private static void ChangeAnimSetPre(bool neck, bool eye)
        {
            Log.LogInfo("HMotionEyeNeckFemale.ChangeAnimSetPre neck: " + neck
                + ", eye: " + eye
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.ChangeAnimSet))]
        private static void ChangeAnimSetPost(bool neck , bool eye )
        {
            Log.LogInfo("HMotionEyeNeckFemale.ChangeAnimSetPost neck: " + neck
                + ", eye: " + eye
                );
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.EyeCalc))]
        private static void EyeCalcPre(Vector3 targetEyeRot)
        {
            Log.LogInfo("HMotionEyeNeckFemale.EyeCalcPre targetEyeRot: " + targetEyeRot
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.EyeCalc))]
        private static void EyeCalcPost(Vector3 targetEyeRot)
        {
            Log.LogInfo("HMotionEyeNeckFemale.EyeCalcPost targetEyeRot: " + targetEyeRot
                );
        }
        */
        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.EyeNeckCalc))]
        private static void EyeNeckCalcPre()
        {
            Log.LogInfo("HMotionEyeNeckFemale.EyeNeckCalcPre"
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.EyeNeckCalc))]
        private static void EyeNeckCalcPost()
        {
            Log.LogInfo("HMotionEyeNeckFemale.EyeNeckCalcPost"
                );
        }
        */
        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.GetObjectName))]
        private static void GetObjectNamePre(Transform top, string name)
        {
            Log.LogInfo("HMotionEyeNeckFemale.GetObjectNamePre top: " + top + ", name: " + name
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.GetObjectName))]
        private static void GetObjectNamePost(Transform top, string name)
        {
            Log.LogInfo("HMotionEyeNeckFemale.GetObjectNamePost top: " + top + ", name: " + name
                );
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.Init))]
        private static void InitPre(Chara.ChaControl _female, int id, HScene hScene)
        {
            Log.LogInfo("HMotionEyeNeckFemale.InitPre name: " + _female?.FileParam.fullname + ", id: " + id + ", hscene: " + hScene?.name
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.Init))]
        private static void InitPost(Chara.ChaControl _female, int id, HScene hScene)
        {
            Log.LogInfo("HMotionEyeNeckFemale.InitPost name: " + _female?.FileParam.fullname + ", id: " + id + ", hscene: " + hScene?.name
                );
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.Load))]
        private static void LoadPre(string _assetpath, string _file)
        {
            Log.LogInfo("HMotionEyeNeckFemale.LoadPre _assetpath: " + _assetpath + ", _file: " + _file
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.Load))]
        private static void LoadPost(string _assetpath, string _file)
        {
            Log.LogInfo("HMotionEyeNeckFemale.LoadPost _assetpath: " + _assetpath + ", _file: " + _file
                );
        }
        */
        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.NeckCalc))]
        private static void NeckCalcPre(Vector3 targetNeckRot, Vector3 targetHeadRot)
        {
            Log.LogInfo("HMotionEyeNeckFemale.NeckCalcPre targetNeckRot: " + targetNeckRot + ", targetHeadRot: " + targetHeadRot
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.NeckCalc))]
        private static void NeckCalcPost(Vector3 targetNeckRot, Vector3 targetHeadRot)
        {
            Log.LogInfo("HMotionEyeNeckFemale.NeckCalcPost targetNeckRot: " + targetNeckRot + ", targetHeadRot: " + targetHeadRot
                );
        }
        */

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.Proc))]
        private static void ProcPre(AnimatorStateInfo _ai, HVoiceCtrl.FaceInfo _faceVoice, int _main)
        {
            Log.LogInfo("HMotionEyeNeckFemale.ProcPre _ai: " + _ai.m_Name + ", _faceVoice: " + _faceVoice + ", _main: " + _main
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.Proc))]
        private static void ProcPost(AnimatorStateInfo _ai, HVoiceCtrl.FaceInfo _faceVoice, int _main)
        {
            Log.LogInfo("HMotionEyeNeckFemale.ProcPost _ai: " + _ai.m_Name + ", _faceVoice: " + _faceVoice + ", _main: " + _main
                );
        }
        */

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.SetBehaviourEyes))]
        private static void SetBehaviourEyesPre(int _behaviour)
        {
            Log.LogInfo("HMotionEyeNeckFemale.SetBehaviourEyesPre _behaviour: " + _behaviour
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.SetBehaviourEyes))]
        private static void SetBehaviourEyesPost(int _behaviour)
        {
            Log.LogInfo("HMotionEyeNeckFemale.SetBehaviourEyesPost _behaviour: " + _behaviour
                );
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.SetBehaviourNeck))]
        private static void SetBehaviourNeckPre(int _behaviour)
        {
            Log.LogInfo("HMotionEyeNeckFemale.SetBehaviourNeckPre _behaviour: " + _behaviour
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.SetBehaviourNeck))]
        private static void SetBehaviourNeckPost(int _behaviour)
        {
            Log.LogInfo("HMotionEyeNeckFemale.SetBehaviourNeckPost _behaviour: " + _behaviour
                );
        }
        */
        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.SetConfigBehaviour))]
        private static void SetConfigBehaviourPre(AnimatorStateInfo _ai, bool neck, bool eye)
        {
            Log.LogInfo("HMotionEyeNeckFemale.SetBehaviourNeckPre _ai: " + _ai.m_Name + ", neck: " + neck + ", eye: " + eye
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.SetConfigBehaviour))]
        private static void SetConfigBehaviourPost(AnimatorStateInfo _ai, bool neck, bool eye)
        {
            Log.LogInfo("HMotionEyeNeckFemale.SetBehaviourNeckPost _ai: " + _ai.m_Name + ", neck: " + neck + ", eye: " + eye
                );
        }
        */
        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.SetEyesTarget))]
        private static void SetEyesTargetPre(int _tag)
        {
            Log.LogInfo("HMotionEyeNeckFemale.SetEyesTargetPre _tag: " + _tag
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.SetEyesTarget))]
        private static void SetEyesTargetPost(int _tag)
        {
            Log.LogInfo("HMotionEyeNeckFemale.SetEyesTargetPost _tag: " + _tag
                );
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.SetNeckTarget))]
        private static void SetNeckTargetPre(int _tag)
        {
            Log.LogInfo("HMotionEyeNeckFemale.SetNeckTargetPre _tag: " + _tag
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HMotionEyeNeckFemale), nameof(HMotionEyeNeckFemale.SetNeckTarget))]
        private static void SetNeckTargetPost(int _tag)
        {
            Log.LogInfo("HMotionEyeNeckFemale.SetNeckTargetPost _tag: " + _tag
                );
        }
        */
        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeAnimation))]
        private static void ChangeAnimationPre(HScene __instance, HScene.AnimationListInfo _info, bool _isForceResetCamera, bool _isForceLoopAction, bool _UseFade, bool isLoadFirst)
        {
            Log.LogInfo("ChangeAnimationPre info: " + _info.NameAnimation
                + ", _isForceResetCamera: " + _isForceResetCamera
                + ", _isForceLoopAction: " + _isForceLoopAction
                + ", _UseFade: " + _UseFade
                + ", isLoadFirst: " + isLoadFirst
                );
            Log.LogInfo("Name: " + __instance._chaMales[0]?.FileParam.fullname + ", visibleson: " + __instance._chaMales[0]?.VisibleSon);

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeAnimation))]
        private static void ChangeAnimationPost(HScene __instance, HScene.AnimationListInfo _info, bool _isForceResetCamera, bool _isForceLoopAction, bool _UseFade, bool isLoadFirst)
        {
            Log.LogInfo("ChangeAnimationPost info: " + _info.NameAnimation
                + ", _isForceResetCamera: " + _isForceResetCamera
                + ", _isForceLoopAction: " + _isForceLoopAction
                + ", _UseFade: " + _UseFade
                + ", isLoadFirst: " + isLoadFirst
                );
            Log.LogInfo("Name: " + __instance._chaMales[0]?.FileParam.fullname + ", visibleson: " + __instance._chaMales[0]?.VisibleSon);
        }



        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(typeof(ProcBase), new[] { typeof(DeliveryMember) })]
        private static void ProcBaseContructor(DeliveryMember _delivery)
        {
            Log.LogInfo("ProcBase contructor");
        }

        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(typeof(ProcBase), new Type[] { })]
        private static void ProcBaseContructor2()
        {
            Log.LogInfo("ProcBase contructor2");
        }

        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(typeof(Houshi), new[] { typeof(DeliveryMember) })]
        private static void HoushiContructor(DeliveryMember _delivery)
        {
            Log.LogInfo("Houshi contructor");
        }

        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(typeof(Houshi), new Type[] { })]
        private static void HoushiContructor2()
        {
            Log.LogInfo("Houshi contructor2");
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(ProcBase), nameof(ProcBase.FeelProc))]
        private static void FeelProc(bool female)
        {
            Log.LogInfo("ProcBase FeelProc female: " + female);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ProcBase), nameof(ProcBase.PlayAtariParticle))]
        private static void PlayAtariParticle()
        {
            Log.LogInfo("ProcBase PlayAtariParticle");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ProcBase), nameof(ProcBase.ReInitPlay))]
        private static void ReInitPlay(int _playAnimationHash, float _normalizetime)
        {
            Log.LogInfo("ProcBase ReInitPlay _playAnimationHash: " + _playAnimationHash + ", _normalizetime: " + _normalizetime);
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(ProcBase), nameof(ProcBase.SetPlay), new[] { typeof(string), typeof(bool) })]
        private static void SetPlayPost(ProcBase __instance, string _playAnimation, bool _isFade)
        {
            Log.LogInfo("ProcBase SetPlayPost, _playAnimation: " + _playAnimation + ", _isFade: " + _isFade);
            Log.LogInfo("Name: " + __instance._chaMales[0]?.FileParam.fullname + ", visibleson: " + __instance._chaMales[0]?.VisibleSon);

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcBase), nameof(ProcBase.SetPlay), new[] { typeof(string), typeof(bool) })]
        private static void SetPlayPre(ProcBase __instance, string _playAnimation, bool _isFade)
        {
            Log.LogInfo("ProcBase SetPlayPre, _playAnimation: " + _playAnimation + ", _isFade: " + _isFade);
            Log.LogInfo("Name: " + __instance._chaMales[0]?.FileParam.fullname + ", visibleson: " + __instance._chaMales[0]?.VisibleSon);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ProcBase), nameof(ProcBase.SetPlay), new[] { typeof(string), typeof(float), typeof(bool) })]
        private static void SetPlay2(string _playAnimation, float _normalizetime, bool _isFade)
        {
            Log.LogInfo("ProcBase SetPlay2, _playAnimation: " + _playAnimation + ", _normalizetime: " + _normalizetime + ", _isFade: " + _isFade);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ProcBase), nameof(ProcBase.SetPlay), new[] { typeof(int), typeof(float), typeof(bool) })]
        private static void SetPlay3(int _playAnimationHash, float _normalizetime, bool _isFade)
        {
            Log.LogInfo("ProcBase SetPlay3, _playAnimationHash: " + _playAnimationHash + ", _normalizetime: " + _normalizetime + ", _isFade: " + _isFade);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ProcBase), nameof(ProcBase.SetRecoverTaii))]
        private static void SetRecoverTaii()
        {
            Log.LogInfo("ProcBase SetRecoverTaii ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.Play))]
        private static void PlayPre(string _strAnmName, int _nLayer)
        {
            Log.LogInfo("ChaControl.PlayPre _strAnmName: " + _strAnmName + ", _nLayer: " + _nLayer);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.Play))]
        private static void PlayPost(string _strAnmName, int _nLayer)
        {
            Log.LogInfo("ChaControl.PlayPost _strAnmName: " + _strAnmName + ", _nLayer: " + _nLayer);
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.OnValuePositionMoveSpeed))]
        private static void OnValuePositionMoveSpeed(float _value)
        {
            Log.LogInfo("OnValuePositionMoveSpeed _value: " + _value);
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(AnimationController), nameof(AnimationController.Play))]
        private static void PlayPre(Animator animator, int stateNameHash, int layer, float normalizedTime)
        {
            Log.LogInfo("AnimationController.PlayPre stateNameHash: " + stateNameHash + ", layer: " + layer + ", normalizedTime: " + normalizedTime);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AnimationController), nameof(AnimationController.Play))]
        private static void PlayPost(Animator animator, int stateNameHash, int layer, float normalizedTime)
        {
            Log.LogInfo("AnimationController.PlayPost stateNameHash: " + stateNameHash + ", layer: " + layer + ", normalizedTime: " + normalizedTime);
        }
        */

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AnimationController), nameof(AnimationController.PlayAnim))]
        private static void PlayAnimPre(int blendMode, float blendTime, float fadeoutTime, bool useOffset, bool useRandomSpeed, UnhollowerBaseLib.Il2CppStructArray<int> layers,
            UnhollowerBaseLib.Il2CppReferenceArray<RG.Scripts.ActionItem> items, float preCalcOffset, float preCalcSpeed)
        {
            Log.LogInfo("AnimationController.PlayAnimPre blendMode: " + blendMode
                + ", blendTime: " + blendTime
                + ", fadeoutTime: " + fadeoutTime
                + ", useOffset: " + useOffset
                + ", useRandomSpeed: " + useRandomSpeed
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AnimationController), nameof(AnimationController.PlayAnim))]
        private static void PlayAnimPost(int blendMode, float blendTime, float fadeoutTime, bool useOffset, bool useRandomSpeed, UnhollowerBaseLib.Il2CppStructArray<int> layers,
            UnhollowerBaseLib.Il2CppReferenceArray<RG.Scripts.ActionItem> items, float preCalcOffset, float preCalcSpeed)
        {
            Log.LogInfo("AnimationController.PlayAnimPost blendMode: " + blendMode
                + ", blendTime: " + blendTime
                + ", fadeoutTime: " + fadeoutTime
                );
        }
        */
        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AnimationController), nameof(AnimationController.PlayAnimPrimitive))]
        private static void PlayAnimPrimitivePre(int stateNameHash, int blendMode, float blendTime, bool useOffset, bool useRandomSpeed, UnhollowerBaseLib.Il2CppStructArray<int> layers,
            UnhollowerBaseLib.Il2CppReferenceArray<RG.Scripts.ActionItem> items)
        {
            Log.LogInfo("AnimationController.PlayAnimPrimitivePre blendMode: " + blendMode + ", blendTime: " + blendTime
                + ", useOffset: " + useOffset
                + ", useRandomSpeed: " + useRandomSpeed
                );
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AnimationController), nameof(AnimationController.PlayAnimPrimitive))]
        private static void PlayAnimPrimitivePost(int stateNameHash, int blendMode, float blendTime, bool useOffset, bool useRandomSpeed, UnhollowerBaseLib.Il2CppStructArray<int> layers,
            UnhollowerBaseLib.Il2CppReferenceArray<RG.Scripts.ActionItem> items)
        {
            Log.LogInfo("AnimationController.PlayAnimPrimitivePost blendMode: " + blendMode + ", blendTime: " + blendTime + ", useOffset: " + useOffset);
        }
        */
        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AnimationController), nameof(AnimationController.SetAnimatorController))]
        private static void SetAnimatorController(AnimationController __instance, RuntimeAnimatorController rac)
        {
            Log.LogInfo("AnimationController.SetAnimatorController Name: " + __instance.Actor?.Status.FullName);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AnimationController), nameof(AnimationController.SetRegularAnimator))]
        private static void SetRegularAnimator(AnimationController __instance, RuntimeAnimatorController rac)
        {
            Log.LogInfo("AnimationController.SetRegularAnimator Name: " + __instance.Actor?.Status.FullName);
        }
        **/

        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.SetPosition), new[] { typeof(Vector3) })]
        private static void SetPosition1(Chara.ChaControl __instance, Vector3 pos)
        {
            Log.LogInfo("Chara.ChaControl.SetPosition1 Name: " + __instance.FileParam.fullname + ", pos: " + pos);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.SetPosition), new[] { typeof(float), typeof(float), typeof(float) })]
        private static void SetPosition2(Chara.ChaControl __instance, float x, float y, float z)
        {
            Log.LogInfo("Chara.ChaControl.SetPosition2 Name: " + __instance.FileParam.fullname
                + ", x: " + x
                + ", y: " + y
                + ", z: " + z
                );
        }
        */

        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.SetVoiceTransform))]
        private static void SetVoiceTransform(Chara.ChaControl __instance, AudioSource voice)
        {
            Log.LogInfo("Chara.ChaControl.SetVoiceTransform Name: " + __instance.FileParam.fullname
                + ", voice: " + voice?.name
                
                ); ;
        }
        */









        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.ChangeMouthPtn))]
        private static void ChangeMouthPtn(Chara.ChaControl __instance, int ptn, bool blend)
        {
            Log.LogInfo("ChangeMouthPtn name: " + __instance.FileParam.fullname + ", ptn: " + ptn + ", blend: " + blend);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharaCustom.CustomBase), nameof(CharaCustom.CustomBase.ChangeMouthPtnNo))]
        private static void ChangeMouthPtnNo(int no)
        {
            Log.LogInfo("ChangeMouthPtnNo no: " + no);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharaCustom.CustomBase), nameof(CharaCustom.CustomBase.ChangeMouthPtnNext))]
        private static void ChangeMouthPtnNext(int next)
        {
            Log.LogInfo("ChangeMouthPtnNext next: " + next);
        }
        */




        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.Create))]
        private static void CreatePre(Transform vt)
        {
            Log.LogInfo("Manager.Voice.CreatePre type: " + vt.name);
            //Debug.PrintDetail(__result);

        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.Create))]
        private static void CreatePost(Transform vt, AudioSource __result)
        {
            Log.LogInfo("Manager.Voice.CreatePost type: " + vt.name + ", result: " + __result.name);
            //Debug.PrintDetail(__result);

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.CreateCache), new[] { typeof(int), typeof(AssetBundleData) })]
        private static void CreateCache1(int voiceNo, AssetBundleData data)
        {
            Log.LogInfo("Manager.Voice.CreateCache1 voiceNo: " + voiceNo
                + ", data: " + data.Bundle + " | " + data.Asset
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.CreateCache), new[] { typeof(int), typeof(AssetBundleManifestData) })]
        private static void CreateCache2(int voiceNo, AssetBundleManifestData data)
        {
            Log.LogInfo("Manager.Voice.CreateCache2 voiceNo: " + voiceNo
                + ", data: " + data.Bundle + " | " + data.Asset + " | " + data.Manifest
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.CreateCache), new[] { typeof(int), typeof(string), typeof(string), typeof(string) })]
        private static void CreateCache3(int voiceNo, string bundle, string asset, string manifest)
        {
            Log.LogInfo("Manager.Voice.CreateCache3 voiceNo: " + voiceNo
                + ", bundle: " + bundle
                + ", asset: " + asset
                + ", manifest: " + manifest
                );
        }
        





        [HarmonyPrefix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.OncePlay), new[] { typeof(Manager.Voice.Loader) })]
        private static void OncePlayPre(Manager.Voice.Loader loader)
        {
            Log.LogInfo("Manager.Voice.OncePlayPre"
                );

            //Debug.PrintDetail(loader);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.OncePlay), new[] { typeof(Manager.Voice.Loader) })]
        private static void OncePlayPost(Manager.Voice.Loader loader)
        {
            Log.LogInfo("Manager.Voice.OncePlayPost"
                );

            //Debug.PrintDetail(loader);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.OncePlayChara), new[] { typeof(Manager.Voice.Loader) })]
        private static void OncePlayCharaPre(Manager.Voice.Loader loader)
        {
            Log.LogInfo("Manager.Voice.OncePlayCharaPre loader.bundle: " + loader?.Bundle + ", loader.asset: " + loader?.Asset
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.OncePlayChara), new[] { typeof(Manager.Voice.Loader) })]
        private static void OncePlayCharaPost(Manager.Voice.Loader loader)
        {
            Log.LogInfo("Manager.Voice.OncePlayCharaPost loader.bundle: " + loader?.Bundle + ", loader.asset: " + loader?.Asset
                );
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.Play), new[] { typeof(Manager.Voice.Loader) })]
        private static void PlayPre(Manager.Voice.Loader loader)
        {
            Log.LogInfo("========");
            Log.LogInfo("Manager.Voice.PlayPre loader bundle: " + loader?.Bundle + ", asset: " + loader?.Asset + ", voicetrans name: " + loader?.VoiceTrans?.name
                );
            //Debug.PrintDetail(loader);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.Play), new[] { typeof(Manager.Voice.Loader) })]
        private static void PlayPost(Manager.Voice.Loader loader)
        {
            Log.LogInfo("Manager.Voice.PlayPost loader bundle: " + loader?.Bundle + ", asset: " + loader?.Asset + ", voicetrans name: " + loader?.VoiceTrans?.name
                );
            //Debug.PrintDetail(loader);
        }
        */

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.PlayStandby))]
        private static void PlayStandbyPre(AudioSource audioSource, Manager.Voice.Loader loader)
        {
            Log.LogInfo("Manager.Voice.PlayStandbyPre audioSource: " + audioSource.name + ", loader.bundle: " + loader?.Bundle + ", loader.asset: " + loader?.Asset);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.PlayStandby))]
        private static void PlayStandbyPost(AudioSource audioSource, Manager.Voice.Loader loader)
        {
            Log.LogInfo("Manager.Voice.PlayStandbyPost audioSource: " + audioSource.name + ", loader.bundle: " + loader?.Bundle + ", loader.asset: " + loader?.Asset);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.ReleaseCache))]
        private static void ReleaseCache(int voiceNo, string bundle, string asset, string manifest)
        {
            Log.LogInfo("Manager.Voice.ReleaseCache voiceNo: " + voiceNo + ", bundle: " + bundle + ", asset: " + asset + ", manifest: " + manifest);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.ResetConfig))]
        private static void ResetConfig()
        {
            Log.LogInfo("Manager.Voice.ResetConfig");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.SetParent))]
        private static void SetParent(int no, Transform t)
        {
            Log.LogInfo("Manager.Voice.SetParent no: " + no + ", t: " + t.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.Stop), new[] {typeof(int)} )]
        private static void Stop1(int no)
        {
            Log.LogInfo("Manager.Voice.Stop1 no: " + no);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.Stop), new[] { typeof(Transform) })]
        private static void Stop2(Transform voiceTrans)
        {
            Log.LogInfo("Manager.Voice.Stop2 voiceTrans: " + voiceTrans.name);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.Stop), new[] { typeof(int), typeof(Transform) })]
        private static void Stop3Pre(int no, Transform voiceTrans)
        {
            Log.LogInfo("Manager.Voice.Stop3Pre no: " + no + ", voiceTrans: " + voiceTrans.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.Stop), new[] { typeof(int), typeof(Transform) })]
        private static void Stop3Post(int no, Transform voiceTrans)
        {
            Log.LogInfo("Manager.Voice.Stop3Post no: " + no + ", voiceTrans: " + voiceTrans.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.StopAll))]
        private static void StopAll(bool isLoopStop)
        {
            Log.LogInfo("Manager.Voice.StopAll isLoopStop: " + isLoopStop );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Voice), nameof(Manager.Voice.ToPlaying))]
        private static void ToPlaying(int no)
        {
            Log.LogInfo("Manager.Voice.Stop3 ToPlaying: " + no);
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.PlaySync), new[] {typeof(string), typeof(int), typeof(float)})]
        private static void PlaySync1(Chara.ChaControl __instance, string _strameHash, int _nLayer, float _fnormalizedTime)
        {
            Log.LogInfo("Chara.ChaControl.PlaySync1 name: " + __instance.FileParam.fullname + ", _strameHash: " + _strameHash);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.PlaySync), new[] { typeof(int), typeof(int), typeof(float) })]
        private static void PlaySync2(Chara.ChaControl __instance, int _nameHash, int _nLayer, float _fnormalizedTime)
        {
            Log.LogInfo("Chara.ChaControl.PlaySync2 name: " + __instance.FileParam.fullname + ", _nameHash: " + _nameHash);
        }
        */


        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HVoiceCtrl), nameof(HVoiceCtrl.ShortBreathProc))]
        private static void ShortBreathProc(Chara.ChaControl _female, int _main)
        {
            Log.LogInfo("HVoiceCtrl.ShortBreathProc name: " + _female.FileParam.fullname + ", main: " + _main);
        }
        */


        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.ChangeLookNeckTarget))]
        private static void ChangeLookNeckTarget(Chara.ChaControl __instance, int targetType, Transform trfTarg, float rate, float rotDeg, float range, float dis)
        {
            Log.LogInfo("Chara.ChaControl.ChangeLookNeckTarget name: " + __instance.FileParam.fullname + ", targetType: " + targetType
                 + ", rate: " + rate
                  + ", rotDeg: " + rotDeg
                   + ", range: " + range
                    + ", dis: " + dis
                );
            if (trfTarg != null)
                Log.LogInfo("trfTarg: " + trfTarg.name);
            Log.LogInfo("__instance.NeckLookCtrl.target.name: " + __instance.NeckLookCtrl?.target?.name);
            Log.LogInfo("__instance.EyeLookCtrl.target.name: " + __instance.EyeLookCtrl?.target?.name);
            
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.ChangeLookNeckPtn))]
        private static void ChangeLookNeckPtn(Chara.ChaControl __instance, int ptn, float rate)
        {
            Log.LogInfo("Chara.ChaControl.ChangeLookNeckPtn name: " + __instance.FileParam.fullname + ", ptn: " + ptn
                 + ", rate: " + rate
                );
        }
        */

        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.ChangeLookEyesTarget))]
        private static void ChangeLookEyesTarget(Chara.ChaControl __instance, int targetType, Transform trfTarg, float rate, float rotDeg, float range, float dis)
        {
            Log.LogInfo("Chara.ChaControl.ChangeLookEyesTarget name: " + __instance.FileParam.fullname + ", targetType: " + targetType
                 + ", rate: " + rate
                  + ", rotDeg: " + rotDeg
                   + ", range: " + range
                    + ", dis: " + dis
                );
            if (trfTarg != null)
                Log.LogInfo("trfTarg: " + trfTarg.name);
            
            
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.ChangeLookEyesPtn))]
        private static void ChangeLookEyesPtn(Chara.ChaControl __instance, int ptn)
        {
            Log.LogInfo("Chara.ChaControl.ChangeLookEyesPtn name: " + __instance.FileParam.fullname + ", ptn: " + ptn
                );
        }
        */

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.Setup))]
        private static void MotionIK_SetupPre(List<Chara.ChaControl> infos)
        {
            Log.LogInfo("MotionIK.SetupPre ");
            for (int i = 0; i < infos.Count; i++)
            {
                Log.LogInfo(infos[i].FileParam.fullname);
                //infos[i].FullBodyIK
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.Setup))]
        private static void MotionIK_SetupPost(List<Chara.ChaControl> infos)
        {
            Log.LogInfo("MotionIK.SetupPost ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetData))]
        private static void MotionIK_SetDataPre(MotionIKData data)
        {
            Log.LogInfo("MotionIK_SetDataPre dataname:" + data?.dataName);

        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetData))]
        private static void MotionIK_SetDataPost(MotionIKData data)
        {
            Log.LogInfo("MotionIK_SetDataPost dataname:" + data?.dataName);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.LoadData), new[] { typeof(TextAsset) })]
        private static void MotionIK_LoadDataPre(MotionIK __instance, TextAsset ta)
        {
            Log.LogInfo("MotionIK_LoadDataPre name: " + __instance.Info.FileParam.fullname);
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.LoadData), new[] { typeof(TextAsset) })]
        private static void MotionIK_LoadDataPost(MotionIK __instance, TextAsset ta)
        {
            Log.LogInfo("MotionIK_LoadDataPost name: " + __instance.Info.FileParam.fullname);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.LoadData), new[] { typeof(string) })]
        private static void MotionIK_LoadDataPre2(string path)
        {
            Log.LogInfo("MotionIK_LoadDataPre2 path:" + path);

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.LoadData), new[] { typeof(string) })]
        private static void MotionIK_LoadDataPost2(string path)
        {
            Log.LogInfo("MotionIK_LoadDataPost2 path:" + path);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.AddDicStateContain), new[] { typeof(Il2CppSystem.Collections.Generic.List<MotionIKData.State>), typeof(UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State>) })]
        private static void AddDicStateContainPre1(Il2CppSystem.Collections.Generic.List<MotionIKData.State> setList, UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State> dicState)
        {
            Log.LogInfo("AddDicStateContainPre1");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.AddDicStateContain), new[] { typeof(Il2CppSystem.Collections.Generic.List<MotionIKData.State>), typeof(UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State>) })]
        private static void AddDicStateContainPost1(Il2CppSystem.Collections.Generic.List<MotionIKData.State> setList, UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State> dicState)
        {
            Log.LogInfo("AddDicStateContainPost1");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.AddDicStateContain), new[] { typeof(Il2CppSystem.Collections.Generic.List<MotionIKData.State>), typeof(MotionIKData.State) })]
        private static void AddDicStateContainPre2(Il2CppSystem.Collections.Generic.List<MotionIKData.State> setList, MotionIKData.State state)
        {
            Log.LogInfo("AddDicStateContainPre1");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.AddDicStateContain), new[] { typeof(Il2CppSystem.Collections.Generic.List<MotionIKData.State>), typeof(MotionIKData.State) })]
        private static void AddDicStateContainPost2(Il2CppSystem.Collections.Generic.List<MotionIKData.State> setList, MotionIKData.State state)
        {
            Log.LogInfo("AddDicStateContainPost1");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.BlendAllDispose))]
        private static void BlendAllDisposePre()
        {
            Log.LogInfo("BlendAllDisposePre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.BlendAllDispose))]
        private static void BlendAllDisposePost()
        {
            Log.LogInfo("BlendAllDisposePost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.BlendAnimDispose))]
        private static void BlendAnimDisposePre(bool overrideNext, int overridehash)
        {
            Log.LogInfo("BlendAnimDisposePre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.BlendAnimDispose))]
        private static void BlendAnimDisposePost(bool overrideNext, int overridehash)
        {
            Log.LogInfo("BlendAnimDisposePost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.Calc), new[] { typeof(string) })]
        private static void CalcPre(MotionIK __instance, string stateName)
        {
            Log.LogInfo("CalcPre name: " + __instance.Info.FileParam.fullname + ", stateName: " + stateName);

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.Calc), new[] { typeof(string) })]
        private static void CalcPost(MotionIK __instance, string stateName)
        {
            Log.LogInfo("CalcPost name: " + __instance.Info.FileParam.fullname + ", stateName: " + stateName);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.Calc), new[] { typeof(int), typeof(int) })]
        private static void Calc2Pre(int hashName, int fkID)
        {
            Log.LogInfo("Calc2Pre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.Calc), new[] { typeof(int), typeof(int) })]
        private static void Calc2Post(int hashName, int fkID)
        {
            Log.LogInfo("Calc2Post");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.CalcAnimBlend))]
        private static void CalcAnimBlendPre(int hashName, float blendTime, int fkID)
        {
            Log.LogInfo("CalcAnimBlendPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.CalcAnimBlend))]
        private static void CalcAnimBlendPost(int hashName, float blendTime, int fkID)
        {
            Log.LogInfo("CalcAnimBlendPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.CalcBlendAnimProc))]
        private static void CalcBlendAnimProcPre()
        {
            Log.LogInfo("CalcBlendAnimProcPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.CalcBlendAnimProc))]
        private static void CalcBlendAnimProcPost()
        {
            Log.LogInfo("CalcBlendAnimProcPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.ChangeWeight))]
        private static void ChangeWeightPre(int nameHash, int fkID)
        {
            Log.LogInfo("ChangeWeightPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.ChangeWeight))]
        private static void ChangeWeightPost()
        {
            Log.LogInfo("ChangeWeightPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.ChangeWeightDispose))]
        private static void ChangeWeightDisposePre(int idx)
        {
            Log.LogInfo("ChangeWeightDisposePre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.ChangeWeightDispose))]
        private static void ChangeWeightDisposePost()
        {
            Log.LogInfo("ChangeWeightDisposePost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.ChangeWeightFK))]
        private static void ChangeWeightFKPre(int nameHash, float normalizedTime)
        {
            Log.LogInfo("ChangeWeightFKPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.ChangeWeightFK))]
        private static void ChangeWeightFKPost(int nameHash, float normalizedTime)
        {
            Log.LogInfo("ChangeWeightFKPost");
        }
        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.ChangeWeightIK))]
        private static void ChangeWeightIKPre(int nameHash, float normalizedTime)
        {
            Log.LogInfo("ChangeWeightIKPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.ChangeWeightIK))]
        private static void ChangeWeightIKPost(int nameHash, float normalizedTime)
        {
            Log.LogInfo("ChangeWeightIKPost");
        }
        */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.GetNowFKState))]
        private static void GetNowFKStatePre(int ID)
        {
            Log.LogInfo("GetNowFKStatePre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.GetNowFKState))]
        private static void GetNowFKStatePost(int ID)
        {
            Log.LogInfo("GetNowFKStatePost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.IKBaseOverride))]
        private static void IKBaseOverridePre(MotionIK __instance, bool val)
        {
            Log.LogInfo("IKBaseOverridePre name: " + __instance.Info.FileParam.fullname + ", val: " + val);

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.IKBaseOverride))]
        private static void IKBaseOverridePost(MotionIK __instance, bool val)
        {
            Log.LogInfo("IKBaseOverridePost name: " + __instance.Info.FileParam.fullname + ", val: " + val);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.InitFrameCalc))]
        private static void InitFrameCalcPre(MotionIK __instance, bool changeFKEnable)
        {
            Log.LogInfo("InitFrameCalcPre name: " + __instance.Info.FileParam.fullname + ", changeFKEnable: " + changeFKEnable);

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.InitFrameCalc))]
        private static void InitFrameCalcPost(MotionIK __instance, bool changeFKEnable)
        {
            Log.LogInfo("InitFrameCalcPost name: " + __instance.Info.FileParam.fullname + ", changeFKEnable: " + changeFKEnable);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.InitState))]
        private static void InitStatePre(string stateName)
        {
            Log.LogInfo("InitStatePre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.InitState))]
        private static void InitStatePost(string stateName)
        {
            Log.LogInfo("InitStatePost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.LinkIK))]
        private static void LinkIKPre(int index, MotionIKData.State state, MotionIK.IKTargetPair pair, float initNormalizeTime, bool forceLateUpdate)
        {
            Log.LogInfo("LinkIKPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.LinkIK))]
        private static void LinkIKPost(int index, MotionIKData.State state, MotionIK.IKTargetPair pair, float initNormalizeTime, bool forceLateUpdate)
        {
            Log.LogInfo("LinkIKPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.LinkIKAnimBlend))]
        private static void LinkIKAnimBlendPre(int index, UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State> state, MotionIK.IKTargetPair pair, float startTime, float blendTime)
        {
            Log.LogInfo("LinkIKAnimBlendPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.LinkIKAnimBlend))]
        private static void LinkIKAnimBlendPost(int index, UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State> state, MotionIK.IKTargetPair pair, float startTime, float blendTime)
        {
            Log.LogInfo("LinkIKAnimBlendPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.Release))]
        private static void ReleasePre(MotionIK __instance)
        {
            Log.LogInfo("ReleasePre name: " + __instance.Info.FileParam.fullname);

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.Release))]
        private static void ReleasePost(MotionIK __instance)
        {
            Log.LogInfo("ReleasePost name: " + __instance.Info.FileParam.fullname);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.Reset))]
        private static void ResetPre(MotionIK __instance)
        {
            Log.LogInfo("ResetPre name: " + __instance.Info.FileParam.fullname);

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.Reset))]
        private static void ResetPost(MotionIK __instance)
        {
            Log.LogInfo("ResetPost name: " + __instance.Info.FileParam.fullname);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.ReSetPartner))]
        private static void ReSetPartnerPre()
        {
            Log.LogInfo("ReSetPartnerPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.ReSetPartner))]
        private static void ReSetPartnerPost()
        {
            Log.LogInfo("ReSetPartnerPost");
        }
        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.Set), new[] { typeof(Illusion.Component.Correct.BaseData), typeof(UnhollowerBaseLib.Il2CppStructArray<Vector3>) })]
        private static void Set1Pre(Illusion.Component.Correct.BaseData data, UnhollowerBaseLib.Il2CppStructArray<Vector3> dirs)
        {
            Log.LogInfo("Set1Pre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.Set), new[] { typeof(Illusion.Component.Correct.BaseData), typeof(UnhollowerBaseLib.Il2CppStructArray<Vector3>) })]
        private static void Set1Post(Illusion.Component.Correct.BaseData data, UnhollowerBaseLib.Il2CppStructArray<Vector3> dirs)
        {
            Log.LogInfo("Set1Post");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.Set), new[] { typeof(Illusion.Component.Correct.BaseData), typeof(Il2CppSystem.ValueTuple<Vector3, Vector3>) })]
        private static void Set2Pre(Illusion.Component.Correct.BaseData data, Il2CppSystem.ValueTuple<Vector3, Vector3> vt)
        {
            Log.LogInfo("Set2Pre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.Set), new[] { typeof(Illusion.Component.Correct.BaseData), typeof(Il2CppSystem.ValueTuple<Vector3, Vector3>) })]
        private static void Set2Post(Illusion.Component.Correct.BaseData data, Il2CppSystem.ValueTuple<Vector3, Vector3> vt)
        {
            Log.LogInfo("Set2Post");
        }
        */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetActorSex))]
        private static void SetActorSexPre(Actor actor)
        {
            Log.LogInfo("SetActorSexPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetActorSex))]
        private static void SetActorSexPost(Actor actor)
        {
            Log.LogInfo("SetActorSexPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetData))]
        private static void SetDataPre(MotionIKData data)
        {
            Log.LogInfo("SetDataPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetData))]
        private static void SetDataPost(MotionIKData data)
        {
            Log.LogInfo("SetDataPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetFK))]
        private static void SetFKPre(int hashName, float initTime, bool forceLateUpdate)
        {
            Log.LogInfo("SetFKPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetFK))]
        private static void SetFKPost(int hashName, float initTime, bool forceLateUpdate)
        {
            Log.LogInfo("SetFKPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetFKBlend))]
        private static void SetFKBlendPre(int nowHash, int nextHash, float start, float blendTime)
        {
            Log.LogInfo("SetFKBlendPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetFKBlend))]
        private static void SetFKBlendPost(int nowHash, int nextHash, float start, float blendTime)
        {
            Log.LogInfo("SetFKBlendPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetItems))]
        private static void SetItemsPre(MotionIK __instance, UnhollowerBaseLib.Il2CppReferenceArray<GameObject> items)
        {
            Log.LogInfo("SetItemsPre name: " + __instance.Info.FileParam.fullname);
            if (items != null) Log.LogInfo("item count: " + items.Count);

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetItems))]
        private static void SetItemsPost(MotionIK __instance, UnhollowerBaseLib.Il2CppReferenceArray<GameObject> items)
        {
            Log.LogInfo("SetItemsPost name: " + __instance.Info.FileParam.fullname);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetMapFK))]
        private static void SetMapFKPre(string AnimatorName)
        {
            Log.LogInfo("SetMapFKPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetMapFK))]
        private static void SetMapFKPost(string AnimatorName)
        {
            Log.LogInfo("SetMapFKPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetMapIK))]
        private static bool SetMapIKPre(MotionIK __instance, string AnimatorName)
        {
            Log.LogInfo("SetMapIKPre name: " + __instance.Info.FileParam.fullname + ", animatorname: " + AnimatorName + ", id: " + __instance.Controller.GetInstanceID());
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetMapIK))]
        private static void SetMapIKPost(string AnimatorName)
        {
            Log.LogInfo("SetMapIKPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetNotBlendHashList))]
        private static void SetNotBlendHashListPre(string assetName, UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State> states)
        {
            Log.LogInfo("SetNotBlendHashListPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetNotBlendHashList))]
        private static void SetNotBlendHashListPost(string assetName, UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State> states)
        {
            Log.LogInfo("SetNotBlendHashListPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetPartnerAdd))]
        private static void SetPartnerAddPre(MotionIK add)
        {
            Log.LogInfo("SetPartnerAddPre");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetPartnerAdd))]
        private static void SetPartnerAddPost(MotionIK add)
        {
            Log.LogInfo("SetPartnerAddPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetPartners), new[] { typeof(Il2CppSystem.Collections.Generic.IEnumerable<MotionIK>) })]
        private static void SetPartnersPre1(Il2CppSystem.Collections.Generic.IEnumerable<MotionIK> partners)
        {
            Log.LogInfo("SetPartnersPre1");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetPartners), new[] { typeof(Il2CppSystem.Collections.Generic.IEnumerable<MotionIK>) })]
        private static void SetPartnersPost1(Il2CppSystem.Collections.Generic.IEnumerable<MotionIK> partners)
        {
            Log.LogInfo("SetPartnersPost1");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetPartners), new[] { typeof(Il2CppSystem.Collections.Generic.IReadOnlyCollection<Il2CppSystem.ValueTuple<int, int, MotionIK>>) })]
        private static void SetPartnersPre2(MotionIK __instance, Il2CppSystem.Collections.Generic.IReadOnlyCollection<Il2CppSystem.ValueTuple<int, int, MotionIK>> partners)
        {
            Log.LogInfo("SetPartnersPre2 name: " + __instance.Info.FileParam.fullname);
            if (partners != null)
            {
                Log.LogInfo("Partner count: " + partners.Count);
            }

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetPartners), new[] { typeof(Il2CppSystem.Collections.Generic.IReadOnlyCollection<Il2CppSystem.ValueTuple<int, int, MotionIK>>) })]
        private static void SetPartnersPost2(MotionIK __instance, Il2CppSystem.Collections.Generic.IReadOnlyCollection<Il2CppSystem.ValueTuple<int, int, MotionIK>> partners)
        {
            Log.LogInfo("SetPartnersPost2 name: " + __instance.Info.FileParam.fullname);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetPartners), new[] { typeof(UnhollowerBaseLib.Il2CppReferenceArray<MotionIK>) })]
        private static void SetPartnersPre3(MotionIK __instance, UnhollowerBaseLib.Il2CppReferenceArray<MotionIK> partners)
        {
            Log.LogInfo("SetPartnersPre3 name: " + __instance.Info.FileParam.fullname);
            if (partners != null)
            {
                Log.LogInfo("Partner count: " + partners.Count);
                foreach (var p in partners)
                {
                    Log.LogInfo("partners: " + p.Info.FileParam.fullname);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.SetPartners), new[] { typeof(UnhollowerBaseLib.Il2CppReferenceArray<MotionIK>) })]
        private static void SetPartnersPost3(MotionIK __instance, UnhollowerBaseLib.Il2CppReferenceArray<MotionIK> partners)
        {
            Log.LogInfo("SetPartnersPost3 name: " + __instance.Info.FileParam.fullname);
        }

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.BlendAnimLinkFK))]
        private static void BlendAnimLinkFKPre(MotionIK ik, float blendTime, float startTime)
        {
            Log.LogInfo("BlendAnimLinkFKPre name: " + ik.Info.FileParam.fullname);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.BlendAnimLinkFK))]
        private static void BlendAnimLinkFKPost(MotionIK ik, float blendTime, float startTime)
        {
            Log.LogInfo("BlendAnimLinkFKPost name: " + ik.Info.FileParam.fullname);
        }
        */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.BlendAnimLinkFKNextProc))]
        private static void BlendAnimLinkFKNextProcPre(MotionIK ik)
        {
            Log.LogInfo("BlendAnimLinkFKNextProcPre name: " + ik.Info.FileParam.fullname);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.BlendAnimLinkFKNextProc))]
        private static void BlendAnimLinkFKNextProcPost(MotionIK ik)
        {
            Log.LogInfo("BlendAnimLinkFKNextProcPost name: " + ik.Info.FileParam.fullname + ", id: " + ik.Controller.GetInstanceID());
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.BlendAnimLinkIK))]
        private static void BlendAnimLinkIKPre(MotionIK ik, float blendTime, UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State> state, float startTime)
        {
            Log.LogInfo("BlendAnimLinkIKPre name: " + ik.Info.FileParam.fullname);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.BlendAnimLinkIK))]
        private static void BlendAnimLinkIKPost(MotionIK ik, float blendTime, UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State> state, float startTime)
        {
            Log.LogInfo("BlendAnimLinkIKPost name: " + ik.Info.FileParam.fullname);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.BlendAnimLinkIKNextProc))]
        private static void BlendAnimLinkIKNextProcPre(MotionIK ik, UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State> state)
        {
            Log.LogInfo("BlendAnimLinkIKNextProcPre name: " + ik.Info.FileParam.fullname);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.BlendAnimLinkIKNextProc))]
        private static void BlendAnimLinkIKNextProcPost(MotionIK ik, UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State> state)
        {
            Log.LogInfo("BlendAnimLinkIKNextProcPost name: " + ik.Info.FileParam.fullname);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.WeightSet))]
        private static void WeightSetPre(int pattern, float start, float end, float key)
        {
            Log.LogInfo("WeightSetPre ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.WeightSet))]
        private static void WeightSetPost(int pattern, float start, float end, float key)
        {
            Log.LogInfo("WeightSetPost ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.CalcBlend))]
        private static void CalcBlendPre()
        {
            Log.LogInfo("CalcBlendPre ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.CalcBlend))]
        private static void CalcBlendPost()
        {
            Log.LogInfo("CalcBlendPost ");
        }
        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.Copy), new System.Type[] {} )]
        private static void CopyPre1()
        {
            Log.LogInfo("CopyPre1 ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.Copy), new System.Type[] { })]
        private static void CopyPost1()
        {
            Log.LogInfo("CalcBlendPost ");
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.Copy), new []{ typeof(MotionIKData) })]
        private static void CopyPre2(MotionIKData src)
        {
            Log.LogInfo("CopyPre2 ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.Copy), new[] { typeof(MotionIKData) })]
        private static void CopyPost2(MotionIKData src)
        {
            Log.LogInfo("CopyPost2 ");
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.Copy), new[] { typeof(UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State>) })]
        private static void CopyPre3(UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State> srcArray)
        {
            Log.LogInfo("CopyPre3 ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.Copy), new[] { typeof(UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State>) })]
        private static void CopyPost3(UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State> srcArray)
        {
            Log.LogInfo("CopyPost3 ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.InitFrame))]
        private static void InitFramePre(MotionIKData.State state)
        {
            Log.LogInfo("InitFramePre ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.InitFrame))]
        private static void InitFramePost(MotionIKData.State state)
        {
            Log.LogInfo("InitFramePost ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.InitShape))]
        private static void InitShapePre(MotionIKData.Frame frame)
        {
            Log.LogInfo("InitShapePre ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.InitShape))]
        private static void InitShapePost(MotionIKData.Frame frame)
        {
            Log.LogInfo("InitShapePost ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.InitState))]
        private static void MotionIKData_InitStatePre(string stateName)
        {
            Log.LogInfo("MotionIKData_InitStatePre ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.InitState))]
        private static void MotionIKData_InitStatePost(string stateName)
        {
            Log.LogInfo("MotionIKData_InitStatePost ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.Read), new[] {typeof(TextAsset)} )]
        private static void ReadPre1(TextAsset ta)
        {
            Log.LogInfo("ReadPre1 ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.Read), new[] { typeof(TextAsset) })]
        private static void ReadPost1(TextAsset ta)
        {
            Log.LogInfo("ReadPost1 ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.Read), new[] { typeof(string) })]
        private static void ReadPre2(string path)
        {
            Log.LogInfo("ReadPre2 ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.Read), new[] { typeof(string) })]
        private static void ReadPost2(string path)
        {
            Log.LogInfo("ReadPost2 ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.Read), new[] { typeof(Il2CppSystem.IO.Stream) })]
        private static void ReadPre3(Il2CppSystem.IO.Stream stream)
        {
            Log.LogInfo("ReadPre3 ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.Read), new[] { typeof(Il2CppSystem.IO.Stream) })]
        private static void ReadPost3(Il2CppSystem.IO.Stream stream)
        {
            Log.LogInfo("ReadPost3 ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.ReadBlend))]
        private static void ReadBlendPre(Il2CppSystem.IO.BinaryReader r)
        {
            Log.LogInfo("ReadBlendPre ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.ReadBlend))]
        private static void ReadBlendPost(Il2CppSystem.IO.BinaryReader r)
        {
            Log.LogInfo("ReadBlendPost ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.ReadBlendFK))]
        private static void ReadBlendFKPre(Il2CppSystem.IO.BinaryReader r)
        {
            Log.LogInfo("ReadBlendFKPre ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.ReadBlendFK))]
        private static void ReadBlendFKPost(Il2CppSystem.IO.BinaryReader r)
        {
            Log.LogInfo("ReadBlendFKPost ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.ReadFK))]
        private static void ReadFKPre(Il2CppSystem.IO.Stream stream)
        {
            Log.LogInfo("ReadFKPre ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.ReadFK))]
        private static void ReadFKPost(Il2CppSystem.IO.Stream stream)
        {
            Log.LogInfo("ReadFKPost ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.ReadNormal))]
        private static void ReadNormalPre(Il2CppSystem.IO.BinaryReader r)
        {
            Log.LogInfo("ReadNormalPre ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.ReadNormal))]
        private static void ReadNormalPost(Il2CppSystem.IO.BinaryReader r)
        {
            Log.LogInfo("ReadNormalPost ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.ReadOut), new[] { typeof(TextAsset) })]
        private static void ReadOutPre1(TextAsset ta)
        {
            Log.LogInfo("ReadOutPre1 ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.ReadOut), new[] {typeof(TextAsset) } )]
        private static void ReadOutPost1(TextAsset ta)
        {
            Log.LogInfo("ReadOutPost1 ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.ReadOut), new[] { typeof(Il2CppSystem.IO.Stream) })]
        private static void ReadOutPre2(Il2CppSystem.IO.Stream stream)
        {
            Log.LogInfo("ReadOutPre2 ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.ReadOut), new[] { typeof(Il2CppSystem.IO.Stream) })]
        private static void ReadOutPost2(Il2CppSystem.IO.Stream stream)
        {
            Log.LogInfo("ReadOutPost2 ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.ReadOutFK), new[] { typeof(TextAsset) })]
        private static void ReadOutFKPre1(TextAsset ta)
        {
            Log.LogInfo("ReadOutFKPre1 ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.ReadOutFK), new[] { typeof(TextAsset) })]
        private static void ReadOutFKPost1(TextAsset ta)
        {
            Log.LogInfo("ReadOutFKPost1 ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.ReadOutFK), new[] { typeof(Il2CppSystem.IO.Stream) })]
        private static void ReadOutFKPre2(Il2CppSystem.IO.Stream stream)
        {
            Log.LogInfo("ReadOutFKPre2 ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.ReadOutFK), new[] { typeof(Il2CppSystem.IO.Stream) })]
        private static void ReadOutFKPost2(Il2CppSystem.IO.Stream stream)
        {
            Log.LogInfo("ReadOutFKPost2 ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.Release))]
        private static void ReleasePre()
        {
            Log.LogInfo("ReleasePre ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.Release))]
        private static void ReleasePost()
        {
            Log.LogInfo("ReleasePost ");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.SetStates))]
        private static void SetStatesPre(UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State> states)
        {
            Log.LogInfo("SetStatesPre ");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIKData), nameof(MotionIKData.SetStates))]
        private static void SetStatesPost(UnhollowerBaseLib.Il2CppReferenceArray<MotionIKData.State> states)
        {
            Log.LogInfo("SetStatesPost ");
        }
        */






        [HarmonyPrefix]
        [HarmonyPatch(typeof(AnimationController), nameof(AnimationController.MapIKCalc))]
        private static bool MapIKCalcPre(AnimationController __instance, int stateNameHash, float blendTime)
        {
            Log.LogInfo("MapIKCalcPre id: " + __instance.GetInstanceID() + ", stateNameHash: " + stateNameHash + ", blendTime: " + blendTime);
            /*
            if (StateManager.Instance.animCtrlIDs.Contains(__instance.GetInstanceID()))
            {
                blendTime = -1;
            }*/
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AnimationController), nameof(AnimationController.MapIKCalc))]
        private static void MapIKCalcPost(AnimationController __instance, int stateNameHash, float blendTime)
        {
            Log.LogInfo("MapIKCalcPost id: " + __instance.GetInstanceID() + ", stateNameHash: " + stateNameHash + ", blendTime: " + blendTime);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AnimationController), nameof(AnimationController.MapIKCalc))]
        private static void MapIKInitPre(AnimationController __instance)
        {
            Log.LogInfo("MapIKInitPre id: " + __instance.GetInstanceID());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AnimationController), nameof(AnimationController.MapIKCalc))]
        private static void MapIKInitPost(AnimationController __instance)
        {
            Log.LogInfo("MapIKInitPost id: " + __instance.GetInstanceID());
        }




        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AssetBundle), nameof(AssetBundle.LoadAsset), new[] {typeof(string) , typeof(Il2CppSystem.Type) } )]
        private static void LoadAsset1(AssetBundle __instance, string name, Il2CppSystem.Type type)
        {
            Log.LogInfo("LoadAsset1 name:" + name + ", type: " + type.FullName + ", assetBundle: " + __instance.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AssetBundle), nameof(AssetBundle.LoadAsset), new[] { typeof(string) })]
        private static void LoadAsset2(string name)
        {
            Log.LogInfo("LoadAsset2 name:" + name);
        }
        */
    }
}
