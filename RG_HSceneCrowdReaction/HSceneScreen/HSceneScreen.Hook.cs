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

        
        //Initialize when H scene start
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CharaInit))]
        private static void CharaInitPost(HScene __instance)
        {
            StateManager.Instance.CurrentHSceneInstance = __instance;
            Log.LogInfo("CharaInitPost");
            Patches.InitHScene(__instance);
        }
        
        //Restore the look/neck target when scene end
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.OnDestroy))]
        private static void OnDestroyPre(HScene __instance)
        {
            Patches.RestoreActorsLookingDirection(__instance);
            Patches.DestroyTempObject();
            Patches.DestroyStateManagerList();
        }

        //Reset the looking direction when H point moved
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetMovePositionPoint))]
        private static void SetMovePositionPoint(HScene __instance, Transform trans, Vector3 offsetpos, Vector3 offsetrot, bool isWorld)
        {
            Log.LogInfo("SetMovePositionPoint");
            Patches.UpdateNonHActorsLookAt(__instance);
        }

        //Reset the looking direction when H point moved
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetPosition), new[] { typeof(Transform), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool) })]
        private static void SetPosition1(HScene __instance, Transform _trans, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart, bool _isWorld)
        {
            Log.LogInfo("SetPosition1");
            Patches.UpdateNonHActorsLookAt(__instance);
        }

        //Reset the looking direction when H point moved
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetPosition), new[] { typeof(Vector3), typeof(Quaternion), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool) })]
        private static void SetPosition2(HScene __instance, Vector3 pos, Quaternion rot, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart, bool isWorld)
        {
            Log.LogInfo("SetPosition2");
            Patches.UpdateNonHActorsLookAt(__instance);
        }

        ////////Change the animation of actors not involved in H
        //////[HarmonyPostfix]
        //////[HarmonyPatch(typeof(HScene), nameof(HScene.StartPointSelect))]
        //////private static void StartPointSelect(HScene __instance, int hpointLen, UnhollowerBaseLib.Il2CppReferenceArray<HPoint> hPoints, int checkCategory, HScene.AnimationListInfo info)
        //////{
        //////    Log.LogInfo("StartPointSelect");
        //////    //Patches.ChangeActorsAnimation(__instance);
        //////}

        //Change the animation of actors not involved in H
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetStartAnimationInfo))]
        private static void SetStartAnimationInfo(HScene __instance)
        {
            Log.LogInfo("SetStartAnimationInfo");
            
            Patches.ChangeActorsAnimation(__instance);
            
        }
        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.Start))]
        private static void Start()
        {
            Log.LogInfo("HScene.Start");
            //Patches.ChangeActorsAnimation(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartAnim))]
        private static void StartAnim(HScene.AnimationListInfo StartAnimInfo)
        {
            Log.LogInfo("HScene.StartAnim");
            //Patches.ChangeActorsAnimation(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.StartAnimDef))]
        private static void StartAnimDef()
        {
            Log.LogInfo("HScene.StartAnimDef");
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
                //__instance.Chara.VisibleSon = !__instance.Chara.VisibleSon;
                Log.LogInfo("FilterCommands Name: " + __instance.Status.FullName);
                //Debug.PrintTransformTree(__instance.Chara.CmpBody.targetEtc.objMNPB.transform, "");
                //Debug.PrintDetail(__instance.Chara.CmpBody.targetEtc.objDanTop);

                if (__instance.CharaFileName == "default3")
                {
                    //Debug.PrintRenderer(__instance.Chara.CmpBody.targetEtc.objDanTop.transform, "");
                    /*
                    StateManager.Instance.dantama = __instance.Chara.CmpBody.targetEtc.objDanTama;
                    StateManager.Instance.dansao = __instance.Chara.CmpBody.targetEtc.objDanSao;
                    StateManager.Instance.dantop = __instance.Chara.CmpBody.targetEtc.objDanTop;
                    */

                    HPoint hPoint = __instance.OccupiedActionPoint.HPointLink[0];
                    Log.LogInfo("hPoint: " + hPoint.name + ", hpointlink count: " + __instance.OccupiedActionPoint.HPointLink.Count);
                    if (hPoint._moveObjects != null)
                    {
                        for (int i = 0; i < hPoint._moveObjects.Count; i++)
                            Log.LogInfo("_moveObjects[" + i + "]: " + hPoint._moveObjects[i].MoveObjName);
                    }
                    hPoint.ChangeHideProcBefore();
                    hPoint.ChangeHideProc(1);
                    
                    //hPoint.ChangeHideProcAll();
                    hPoint.HpointObjVisibleChange(true);
                }
            }

            Log.LogInfo("Name: " + __instance.Status.FullName + ", OccupyiedPt: " + __instance.OccupiedActionPoint?.name);
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
                if (StateManager.Instance.ForceActiveInstanceID.Contains(__instance.GetInstanceID()))
                    __instance.active = true;
            }

        }




        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.OnClickConfig))]
        private static void OnClickConfig(HSceneSprite __instance)
        {
            Log.LogInfo("OnClickConfig");

            //Manager.Game.ActionMap.APTContainer.ActionPoints[0].HPointLink

            //StateManager.Instance.CurrentHSceneInstance.


            /*
            for (int i=0; i< Manager.Game.ActionMap.APTContainer._actionPoints.Count; i++)
            {
                var pt = Manager.Game.ActionMap.APTContainer.ActionPoints[i];
                Log.LogInfo("Action pt id: " + pt.UniqueID + " name: " + pt.name + ", HPoint link count: " + pt.HPointLink?.Count);

                if(pt.HPointLink != null)
                {
                    for (int j = 0; j < pt.HPointLink.Count; j++)
                    {
                        var hpoint = pt.HPointLink[j];
                        Log.LogInfo("Hpoint id: " + hpoint.ID + " name: " + hpoint.name + ", move obj count: " + hpoint._moveObjects?.Count);
                        if (hpoint._moveObjects != null)
                        {
                            foreach (var item in hpoint._moveObjects)
                            {
                                Log.LogInfo("Hpoint name: " + hpoint.name + ", move obj name: " + item.MoveObjName);
                            }
                        }

                        if(hpoint.ID == 29)
                        {
                            hpoint.ChangeHideProcBefore();
                            //hpoint.ChangeHideProc(1);
                            hpoint.ChangeHideProcAll();
                            hpoint.HpointObjVisibleChange(true);

                            foreach (var item in hpoint._moveObjects)
                            {
                                Log.LogInfo("Hpoint name: " + hpoint.name + ", move obj name: " + item.MoveObjName);
                                for(int k=0; k< item.OffSetInfos.Count; k++)
                                {

                                    Log.LogInfo("item.OffSetInfos[" + k + "] pos: " + item.OffSetInfos[k].Pos + ", ang: " + item.OffSetInfos[k].Ang
                                         + ", UsePos: " + item.OffSetInfos[k].UsePos
                                          + ", UseAng: " + item.OffSetInfos[k].UseAng
                                        );
                                }
                                item.SetOffset(0);
                            }
                        }
                    }

                        

                }
                
            }
            */


            /*
            Log.LogInfo("%%%%%%%%%");
            if (HPoint._animationLists != null)
            {
                Log.LogInfo("HPoint._animationLists Count: " + HPoint._animationLists.Count);
                for (int i = 0; i < HPoint._animationLists.Count; i++)
                {
                    Log.LogInfo("HPoint._animationLists[" + i + "] list count: " + HPoint._animationLists[i].Count);
                    foreach (var item in HPoint._animationLists[i])
                    {
                        Debug.PrintDetail(item);
                    }
                }
            }
            Log.LogInfo("$$$$$$$$$$");
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
                var list = Patches.GetActorsInvolvedInH(ActionScene.Instance, StateManager.Instance.CurrentHSceneInstance);
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
        [HarmonyPostfix]
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

        [HarmonyPostfix]
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

        [HarmonyPostfix]
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
            /*
            //need to rectify the position of the hanimation crowd
            if(StateManager.Instance.CurrentHSceneInstance != null && ActionScene.Instance != null)
            {
                foreach (var a in ActionScene.Instance._actors)
                {
                    if (StateManager.Instance.HActionActorList.ContainsKey(a.GetInstanceID()))
                    {
                        var hpoint = StateManager.Instance.HActionActorList[a.GetInstanceID()];
                        Patches.SetActorToHPoint(a, StateManager.Instance.HActionActorList[a.GetInstanceID()]);
                        //Log.LogInfo("HPoint pos: " + hpoint.transform.position + ", localpos: " + hpoint.transform.localPosition);
                        //Debug.PrintDetail(a.Chara.transform);
                        
                    }
                }
            }
            */
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.SetPosition), new[] {typeof(Vector3)} )]
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.SetVoiceTransform))]
        private static void SetVoiceTransform(Chara.ChaControl __instance, AudioSource voice)
        {
            Log.LogInfo("Chara.ChaControl.SetVoiceTransform Name: " + __instance.FileParam.fullname
                + ", voice: " + voice.name
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Sound), nameof(Manager.Sound.Create))]
        private static void Create(Manager.Sound.Type type, bool isCache)
        {
            Log.LogInfo("Manager.Sound.Create type: " + type
                + ", isCache: " + isCache
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Sound), nameof(Manager.Sound.CreateCache), new[] {typeof(Manager.Sound.Type), typeof(AssetBundleData) } )]
        private static void CreateCache1(Manager.Sound.Type type, AssetBundleData data)
        {
            Log.LogInfo("Manager.Sound.CreateCache1 type: " + type
                + ", data: " + data.Bundle + " | " + data.Asset
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Sound), nameof(Manager.Sound.CreateCache), new[] { typeof(Manager.Sound.Type), typeof(AssetBundleManifestData) })]
        private static void CreateCache2(Manager.Sound.Type type, AssetBundleManifestData data)
        {
            Log.LogInfo("Manager.Sound.CreateCache2 type: " + type
                + ", data: " + data.Bundle + " | " + data.Asset + " | " + data.Manifest
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.Sound), nameof(Manager.Sound.CreateCache), new[] { typeof(Manager.Sound.Type), typeof(string), typeof(string), typeof(string) })]
        private static void CreateCache3(Manager.Sound.Type type, string bundle, string asset, string manifest)
        {
            Log.LogInfo("Manager.Sound.CreateCache3 type: " + type
                + ", bundle: " + bundle
                + ", asset: " + asset
                + ", manifest: " + manifest
                );
        }

        /*

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.BackUpPosition))]
        private static void BackUpPosition(HPoint __instance)
        {
            Log.LogInfo("BackUpPosition name: " + __instance.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.ChangeHideProc))]
        private static void ChangeHideProc(HPoint __instance, int hideUseProc)
        {
            Log.LogInfo("ChangeHideProc name: " + __instance.name + ", hideUseProc: " + hideUseProc);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.ChangeHideProcAll))]
        private static void ChangeHideProcAll(HPoint __instance)
        {
            Log.LogInfo("ChangeHideProcAll name: " + __instance.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.ChangeHideProcBefore))]
        private static void ChangeHideProcBefore(HPoint __instance)
        {
            Log.LogInfo("ChangeHideProcBefore name: " + __instance.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.ChangeShowerVisible))]
        private static void ChangeShowerVisible(HPoint __instance)
        {
            Log.LogInfo("ChangeShowerVisible name: " + __instance.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.CheckVisible))]
        private static void CheckVisible(HPoint __instance, GameObject obj)
        {
            Log.LogInfo("CheckVisible name: " + __instance.name + ",  obj: " + obj.name);
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.FindObjsMove))]
        private static void FindObjsMove(HPoint __instance, Transform root, UnhollowerBaseLib.Il2CppReferenceArray<HPoint.MoveObjectSet> move)
        {
            Log.LogInfo("FindObjsMove name: " + __instance.name + ",  root: " + root.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.HpointObjVisibleChange))]
        private static void HpointObjVisibleChange(HPoint __instance, bool val)
        {
            Log.LogInfo("HpointObjVisibleChange name: " + __instance.name + ",  val: " + val);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.Init))]
        private static void Init(HPoint __instance, GameObject MapObj)
        {
            Log.LogInfo("HPoint.Init name: " + __instance?.name + ", MapObj: " + MapObj?.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.MoveBackUpPos))]
        private static void MoveBackUpPos(HPoint __instance)
        {
            Log.LogInfo("MoveBackUpPos name: " + __instance.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.MoveBackUpPosAll))]
        private static void MoveBackUpPosAll(HPoint __instance)
        {
            Log.LogInfo("MoveBackUpPosAll name: " + __instance.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.MoveOffset))]
        private static void MoveOffset(HPoint __instance)
        {
            Log.LogInfo("MoveOffset name: " + __instance.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.MoveOffsetAll))]
        private static void MoveOffsetAll(HPoint __instance)
        {
            Log.LogInfo("MoveOffsetAll name: " + __instance.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.ReInit))]
        private static void ReInit(HPoint __instance)
        {
            Log.LogInfo("ReInit name: " + __instance.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.ResetOffset))]
        private static void ResetOffset(HPoint __instance)
        {
            Log.LogInfo("ResetOffset name: " + __instance.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.SaveOffsetAng))]
        private static void SaveOffsetAng(HPoint __instance)
        {
            Log.LogInfo("SaveOffsetAng name: " + __instance.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.SaveOffsetPos))]
        private static void SaveOffsetPos(HPoint __instance)
        {
            Log.LogInfo("SaveOffsetPos name: " + __instance.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.SetOffset), new System.Type[] { })]
        private static void SetOffset(HPoint __instance)
        {
            Log.LogInfo("SetOffset name: " + __instance.name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.SetOffset), new[] { typeof(int) })]
        private static void SetOffset(HPoint __instance, int ptn)
        {
            Log.LogInfo("SetOffset name: " + __instance.name + ", ptn: " + ptn);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.SetShowerMasturbationFlag))]
        private static void SetShowerMasturbationFlag(HPoint __instance, bool val)
        {
            Log.LogInfo("SetShowerMasturbationFlag name: " + __instance.name + ", val: " + val);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint), nameof(HPoint.VisibleObj))]
        private static void VisibleObj(HPoint __instance, List<int> visible)
        {
            Log.LogInfo("VisibleObj name: " + __instance.name + ",  visible Count: " + visible.Count);
            foreach (var i in visible) Log.LogInfo("Visible: " + i);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint.MoveObjectSet), nameof(HPoint.MoveObjectSet.ResetPosRot))]
        private static void ResetPosRot(HPoint.MoveObjectSet __instance)
        {
            Log.LogInfo("MoveObjectSet.ResetPosRot name: " + __instance?.MoveObjName);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint.MoveObjectSet), nameof(HPoint.MoveObjectSet.SetBasePosAng))]
        private static void SetBasePosAng(HPoint.MoveObjectSet __instance)
        {
            Log.LogInfo("MoveObjectSet.SetBasePosAng name: " + __instance?.MoveObjName);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HPoint.MoveObjectSet), nameof(HPoint.MoveObjectSet.SetOffset))]
        private static void SetOffset(HPoint.MoveObjectSet __instance, int ptn)
        {
            Log.LogInfo("MoveObjectSet.SetBasePosAng name: " + __instance?.MoveObjName + ", ptn: " + ptn);
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(RootmotionOffset), nameof(RootmotionOffset.OffsetBlend))]
        private static void OffsetBlend(RootmotionOffset __instance, RootmotionOffset.Info info)
        {
            Log.LogInfo("RootmotionOffset.OffsetBlend name: " + __instance?.Chara?.FileParam.fullname);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RootmotionOffset), nameof(RootmotionOffset.OffsetInit))]
        private static void OffsetInit(RootmotionOffset __instance, string _file)
        {
            Log.LogInfo("RootmotionOffset.OffsetInit name: " + __instance?.Chara?.FileParam.fullname + ", file: " + _file);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RootmotionOffset), nameof(RootmotionOffset.Set), new[] { typeof(string) })]
        private static void Set1(RootmotionOffset __instance, string _state)
        {
            Log.LogInfo("RootmotionOffset.Set1 name: " + __instance?.Chara?.FileParam.fullname + ", _state: " + _state);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RootmotionOffset), nameof(RootmotionOffset.Set), new[] { typeof(int) })]
        private static void Set2(RootmotionOffset __instance, int _state)
        {
            Log.LogInfo("RootmotionOffset.Set2 name: " + __instance?.Chara?.FileParam.fullname + ", _state: " + _state);
        }

        //Reset the looking direction when H point moved
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetPosition), new[] { typeof(Transform), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool) })]
        private static void SetPositiona(HScene __instance, Transform _trans, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart, bool _isWorld)
        {
            Log.LogInfo("SetPositiona trans name: " + _trans.gameObject.name
                + "+ offsetpos: " + offsetpos
                + "+ offsetrot: " + offsetrot
                );
        }

        //Reset the looking direction when H point moved
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetPosition), new[] { typeof(Vector3), typeof(Quaternion), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool) })]
        private static void SetPositionb(HScene __instance, Vector3 pos, Quaternion rot, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart, bool isWorld)
        {
            Log.LogInfo("SetPositionb "
                + "+ offsetpos: " + offsetpos
                + "+ offsetrot: " + offsetrot
                                + "+ pos: " + pos
                + "+ rot: " + rot
                );
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.EndPloc))]
        private static void EndPlocPre()
        {
            Log.LogInfo("HitObjectCtrl.EndPlocPre");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.EndPloc))]
        private static void EndPlocPost()
        {
            Log.LogInfo("HitObjectCtrl.EndPlocPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.GetObjParent))]
        private static void GetObjParentPre(Transform objTop, string name)
        {
            Log.LogInfo("HitObjectCtrl.GetObjParentPre objTop: " + objTop.gameObject.name + ", name: " + name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.GetObjParent))]
        private static void GetObjParentPost(Transform objTop, string name, GameObject __result)
        {
            Log.LogInfo("HitObjectCtrl.GetObjParentPost objTop: " + objTop.gameObject.name + ", name: " + name + ", result: " + __result?.name);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.HitObjInit))]
        private static void HitObjInitPre(int Sex, GameObject _objBody, Chara.ChaControl _custom)
        {
            Log.LogInfo("HitObjectCtrl.HitObjInitPre sex: " + Sex + ", objbody: " + _objBody.name + ", custom: " + _custom?.FileParam?.fullname);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.HitObjInit))]
        private static void HitObjInitPost(int Sex, GameObject _objBody, Chara.ChaControl _custom, Il2CppSystem.Collections.IEnumerator __result)
        {
            Log.LogInfo("HitObjectCtrl.HitObjInitPost sex: " + Sex + ", objbody: " + _objBody.name + ", custom: " + _custom?.FileParam?.fullname);

        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.HitObjLoadExcel))]
        private static void HitObjLoadExcelPre(string _file)
        {
            Log.LogInfo("HitObjectCtrl.HitObjLoadExcelPre file: " + _file);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.HitObjLoadExcel))]
        private static void HitObjLoadExcelPost(string _file)
        {
            Log.LogInfo("HitObjectCtrl.HitObjLoadExcelPost file: " + _file);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.PreEndPloc))]
        private static void PreEndPlocPre()
        {
            Log.LogInfo("HitObjectCtrl.PreEndPlocPre");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.PreEndPloc))]
        private static void PreEndPlocPost()
        {
            Log.LogInfo("HitObjectCtrl.PreEndPlocPost");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.Proc), new[] { typeof(string) })]
        private static void Proc1Pre(string _nextAnim)
        {
            Log.LogInfo("HitObjectCtrl.Proc1Pre _nextAnim: " + _nextAnim);

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.Proc), new[] { typeof(string) })]
        private static void Proc1Post(string _nextAnim)
        {
            Log.LogInfo("HitObjectCtrl.Proc1Post _nextAnim: " + _nextAnim);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.Proc), new[] { typeof(int) })]
        private static void Proc2Pre(int _nextAnimHash)
        {
            Log.LogInfo("HitObjectCtrl.Proc2Pre _nextAnimHash: " + _nextAnimHash);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.Proc), new[] { typeof(int) })]
        private static void Proc2Post(int _nextAnimHash)
        {
            Log.LogInfo("HitObjectCtrl.Proc2Post _nextAnimHash: " + _nextAnimHash);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.Visible))]
        private static void VisiblePre(bool _visible)
        {
            Log.LogInfo("HitObjectCtrl.VisiblePre _visible: " + _visible);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.Visible))]
        private static void VisiblePost(bool _visible)
        {
            Log.LogInfo("HitObjectCtrl.VisiblePost _visible: " + _visible);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.SetActiveObject))]
        private static void SetActiveObjectPre(bool val)
        {
            Log.LogInfo("HitObjectCtrl.SetActiveObjectPre val: " + val);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HitObjectCtrl), nameof(HitObjectCtrl.SetActiveObject))]
        private static void SetActiveObjectPost(bool val)
        {
            Log.LogInfo("HitObjectCtrl.SetActiveObjectPost val: " + val);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.LoadHitObject))]
        private static void LoadHitObject(Chara.ChaControl __instance)
        {
            Log.LogInfo("Chara.ChaControl.LoadHitObject name: " + __instance.FileParam.fullname);
        }
        */

        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.SetShapeBodyValue))]
        private static void SetShapeBodyValue(Chara.ChaControl __instance, int index, float value)
        {
            Log.LogInfo("Chara.ChaControl.SetShapeBodyValue, name: " + __instance?.FileParam.fullname + ", index: " + index + ", value: " + value);
        }
        */
    }
}
;