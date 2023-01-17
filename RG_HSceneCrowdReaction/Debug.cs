using BepInEx.Logging;
using RG.Scene.Action.Core;
using RG.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace HSceneCrowdReaction
{
    internal class Debug
    {
        private static ManualLogSource Log = HSceneCrowdReactionPlugin.Log;

        public static void PrintDetail(object a)
        {
            if (a == null) return;

            foreach (var prop in a.GetType().GetProperties())
            {
                try
                {

                    object value = prop.GetValue(a, null);
                    if (value != null)
                    {
                        if (value is Transform)
                            Log.Log(LogLevel.Info, prop.Name + "=" + value + ", name: " + ((Transform)value).name);
                        else if (value is GameObject)
                            Log.Log(LogLevel.Info, prop.Name + "=" + value + ", name: " + ((GameObject)value).name);
                        else
                            Log.Log(LogLevel.Info, prop.Name + "=" + value);
                    }
                    else
                        Log.Log(LogLevel.Info, prop.Name + " is null!!");

                }
                catch { }
            }
            Log.LogInfo("================");
        }

        public static void PrintCustomTable(int category)
        {
            var a = Manager.Character.Instance.CustomTableData._instance[category];
            foreach (var b in a)
            {

                Log.LogInfo("b.Key: " + b.Key);
                foreach (var c in b.Value)
                {
                    Log.LogInfo("b.Key: " + b.Key + ", c.ID: " + c.ID + ", c.Category: " + c.Category);

                    foreach (var d in c._dic)
                    {
                        Log.LogInfo("b.Key: " + b.Key + ", c.ID: " + c.ID + ", c.Category: " + c.Category + ", d.Key: " + d.Key);

                        foreach (var e in d.Value)
                        {
                            Log.LogInfo("b.Key: " + b.Key + ", c.ID: " + c.ID + ", c.Category: " + c.Category + ", d.Key: " + d.Key + ", e: " + e);


                        }

                    }

                }
            }
        }

        public static void PrintAnimationTable(int category)
        {
            var a = Manager.Game.ActionCache.AnimationTableData._instance[category];
            foreach (var b in a)
            {

                Log.LogInfo("b.Key: " + b.Key);
                foreach (var c in b.Value)
                {
                    Log.LogInfo("b.Key: " + b.Key + ", c.ID: " + c.ID + ", c count: " + c._dic.Count);

                    foreach (var d in c._dic)
                    {
                        Log.LogInfo("b.Key: " + b.Key + ", c.ID: " + c.ID + ", d.Key: " + d.Key + ", d count: " + d.Value.Count);

                        foreach (var e in d.Value)
                        {
                            Log.LogInfo("b.Key: " + b.Key + ", c.ID: " + c.ID + ", d.Key: " + d.Key + ", e: " + e);


                        }

                    }

                }
            }
        }

        public static void PrintAnimationTable()
        {
            if (Manager.Game.ActionCache != null)
            {
                Log.LogInfo("FilterCommands _dicCommonAnimationParameters:");

                for (int i = 0; i < Manager.Game.ActionCache._dicCommonAnimationParameters.Count; i++)
                {
                    Log.LogInfo("sex: " + i);
                    foreach (var kvp in Manager.Game.ActionCache._dicCommonAnimationParameters[i])
                    {
                        Log.LogInfo("sex: " + i + ", Key: " + kvp.Key + ", name: " + kvp.Value.Name
                             + ", asset: " + kvp.Value.Asset
                             + ", assetBundle: " + kvp.Value.AssetBundle
                            );
                    }
                }

                Log.LogInfo("==========================");

                Log.LogInfo("_dicAnimationParameters:");

                for (int i = 0; i < Manager.Game.ActionCache._dicAnimationParameters.Count; i++)
                {
                    Log.LogInfo("Sex: " + i);
                    foreach (var kvp in Manager.Game.ActionCache._dicAnimationParameters[i])
                    {

                        foreach (var kvp2 in kvp.Value.Private)
                            Log.LogInfo("sex: " + i + ", jobid: " + kvp.Key + ", Private, kvp2 key: " + kvp2.Key + ", name: "
                                + kvp2.Value.Name
                                                             + ", asset: " + kvp2.Value.Asset
                             + ", assetBundle: " + kvp2.Value.AssetBundle);
                        foreach (var kvp2 in kvp.Value.Workplace)
                            Log.LogInfo("sex: " + i + ", jobid: " + kvp.Key + ", Workplace, kvp2 key: " + kvp2.Key + ", name: " + kvp2.Value.Name
                                + ", asset: " + kvp2.Value.Asset
                             + ", assetBundle: " + kvp2.Value.AssetBundle);
                    }
                }

                Log.LogInfo("==========================");


                Log.LogInfo("_dicADVAnimationParameters:");

                for (int i = 0; i < Manager.Game.ActionCache._dicADVAnimationParameters.Count; i++)
                {
                    Log.LogInfo("sex: " + i);
                    foreach (var kvp in Manager.Game.ActionCache._dicADVAnimationParameters[i])
                    {
                        Log.LogInfo("sex: " + i + ", Key: " + kvp.Key + ", name: " + kvp.Value.Name
                            + ", asset: " + kvp.Value.Asset
                         + ", assetBundle: " + kvp.Value.AssetBundle);
                    }
                }

                Log.LogInfo("==========================");

                Log.LogInfo("_mobAnimationParameters:");

                for (int i = 0; i < Manager.Game.ActionCache._mobAnimationParameters.Count; i++)
                {
                    Log.LogInfo("i: " + i);
                    foreach (var kvp in Manager.Game.ActionCache._mobAnimationParameters[i])
                    {
                        Log.LogInfo("i: " + i + ", Key: " + kvp.Key + ", name: " + kvp.Value.Name
                            + ", asset: " + kvp.Value.Asset
                         + ", assetBundle: " + kvp.Value.AssetBundle);
                    }
                }

                Log.LogInfo("==========================");

            }
        }

        public static void PrintFullAnimationTable()
        {
            foreach (var a in Manager.Game.ActionCache.AnimationTableData._instance)
            {
                Log.LogInfo("a.Key: " + a.Key + ", b count: " + a.Value.Count);
                foreach (var b in a.Value)
                {

                    Log.LogInfo("b.Key: " + b.Key);
                    foreach (var c in b.Value)
                    {
                        Log.LogInfo("b.Key: " + b.Key + ", c.ID: " + c.ID + ", c count: " + c._dic.Count);

                        foreach (var d in c._dic)
                        {
                            if (d.Key == 5 || d.Key == 2)
                            {
                                Log.LogInfo("b.Key: " + b.Key + ", c.ID: " + c.ID + ", d.Key: " + d.Key + ", d count: " + d.Value.Count);

                                foreach (var e in d.Value)
                                {

                                    Log.LogInfo("b.Key: " + b.Key + ", c.ID: " + c.ID + ", d.Key: " + d.Key + ", e: " + e);


                                }
                            }
                        }

                    }
                }
            }
        }

        public static void PrintHAnimationList(HScene hScene)
        {
            for (int i = 0; i < hScene._lstAnimInfo.Count; i++)
            {
                Log.LogInfo("_lstAnimInfo[" + i + "] Count: " + hScene._lstAnimInfo[i].Count);
                foreach (var item in hScene._lstAnimInfo[i])
                {
                    Debug.PrintDetail(item);
                    for (int k = 0; k < item.LstPositons.Count; k++)
                        Log.LogInfo("LstPositons[" + k + "]: " + item.LstPositons[k]);
                    for (int k = 0; k < item.LstOffset.Count; k++)
                        Log.LogInfo("LstOffset[" + k + "]: " + item.LstOffset[k]);
                    for (int k = 0; k < item.VisiblePointMapObj.Count; k++)
                        Log.LogInfo("VisiblePointMapObj[" + k + "]: " + item.VisiblePointMapObj[k]);
                    for (int k = 0; k < item.LimitMap.Count; k++)
                        Log.LogInfo("LimitMap[" + k + "]: " + item.LimitMap[k]);
                    Log.LogInfo("%%%%%%%%%%%%%%%%%%%");
                }

            }
        }

        public static void PrintAnimationParameter(AnimationParameter param)
        {
            Debug.PrintDetail(param);
            if (param.States != null)
                for (int i = 0; i < param.States.Count; i++)
                {
                    for (int j = 0; j < param.States[i].Count; j++)
                        Log.LogInfo("States[" + i + "][" + j + "]: " + param.States[i][j]);
                }
            if (param.StateHashes != null)
                for (int i = 0; i < param.StateHashes.Count; i++)
                {
                    for (int j = 0; j < param.StateHashes[i].Count; j++)
                    {
                        Log.LogInfo("StateHashes[" + i + "][" + j + "]: " + param.StateHashes[i][j]);
                    }
                }
            if (param.SpecifiedLayers != null)
                for (int i = 0; i < param.SpecifiedLayers.Count; i++)
                {
                    Log.LogInfo("SpecifiedLayers[" + i + "]: " + param.SpecifiedLayers[i]);

                }
        }

        public static void PrintCharacterParameter(Actor a)
        {
            Log.LogInfo("Name: " + a.Status.FullName);
            for (int i = 0; i < a.Status.Parameters.Count; i++)
            {
                Log.LogInfo("a.Status.Parameters[" + i + "]: " + a.Status.Parameters[i]);
            }
        }

        internal static void PrintRenderer(Transform t, string currentPath)
        {
            if (t != null)
            {
                Log.LogInfo("Path: " + currentPath);
                Log.LogInfo("Name: " + t.name);
                Log.LogInfo("Active: " + t.gameObject.active + ", activeInHierarchy: " + t.gameObject.activeInHierarchy + ", activeSelf: " + t.gameObject.activeSelf);

                var r = t.GetComponent<SkinnedMeshRenderer>();
                if (r != null)
                {

                    PrintDetail(r);
                    //r.updateWhenOffscreen = true;
                    Log.LogInfo("rootBone: " + r.rootBone.position + ", local: " + r.rootBone.localPosition);

                    for (int i = 0; i < r.bones.Count; i++)
                        Log.LogInfo("bones[" + i + "]: " + r.bones[i].position + ", local: " + r.bones[i].localPosition);
                }

                for (int i = 0; i < t.GetChildCount(); i++)
                {
                    Log.LogInfo("Visiting the child of [" + t.name + "]");
                    PrintRenderer(t.GetChild(i), currentPath + ".[" + t.name + "]");
                }
            }
        }

        internal static void PrintTransformTreeUpward(Transform t, string currentPath, string stopAt = null)
        {
            GetComponentTypes(t);

            Log.LogInfo("Visiting the parent of [" + t.name + "]" + ", position: " + t.position);
            if (t.parent != null && (stopAt == null || t.name != stopAt))
                PrintTransformTreeUpward(t.parent, "[" + t.name + "]." + currentPath);
        }

        internal static string GetChaControl(Transform t)
        {

            var ctrl = t.GetComponent<Chara.ChaControl>();
            if (ctrl != null)
            {
                return ctrl.FileParam.fullname;
            }

            if (t.parent != null)
                return GetChaControl(t.parent);
            else
                return "";


        }

        internal static void PrintTransformTreeNameOnly(Transform t)
        {
            if (t != null)
            {
                Log.LogInfo(t.gameObject.name);

                for (int i = 0; i < t.GetChildCount(); i++)
                {
                    PrintTransformTreeNameOnly(t.GetChild(i));
                }
            }
        }



        internal static void PrintTransformTree(Transform t, string currentPath)
        {
            if (t != null)
            {
                Log.LogInfo("Path: " + currentPath);
                Log.LogInfo("Name: " + t.name);
                Log.LogInfo("Active: " + t.gameObject.active);
                //PrintDetail(t.gameObject);
                //PrintDetail(t);
                GetComponentTypes(t);

                var mono = t.GetComponent<MonoBehaviour>();
                if (mono != null)
                {
                    Log.LogInfo("GetScriptClassName: " + mono.GetScriptClassName());
                }

                Log.LogInfo("Position: " + t.position);
                Log.LogInfo("LocalPosition: " + t.localPosition);
                var r = t.GetComponent<RectTransform>();
                if (r != null)
                {
                    Log.LogInfo("Width: " + r.rect.width + ", height: " + r.rect.height);
                    Log.LogInfo("bottom: " + r.rect.bottom + ", top: " + r.rect.top);
                }
                Log.LogInfo("Child Count: " + t.childCount);

                Log.LogInfo("");
                for (int i = 0; i < t.GetChildCount(); i++)
                {
                    Log.LogInfo("Visiting the child of [" + t.name + "]");
                    PrintTransformTree(t.GetChild(i), currentPath + ".[" + t.name + "]");
                }
                Log.LogInfo("");
            }
        }

        internal static void PrintVoiceList(HScene hScene)
        {
            if (hScene.CtrlVoice._voiceList != null)
            {
                Log.LogInfo("_voicelist not null ");
                if (hScene.CtrlVoice._voiceList.DicDicDicDicVoice != null)
                {
                    Log.LogInfo("DicDicDicDicVoice Count: " + hScene.CtrlVoice._voiceList.DicDicDicDicVoice.Count);
                    for (int i = 0; i < hScene.CtrlVoice._voiceList.DicDicDicDicVoice.Count; i++)
                    {
                        Log.LogInfo("i: " + i + ", item Count : " + hScene.CtrlVoice._voiceList.DicDicDicDicVoice[i].Count);

                        foreach (var kvp in hScene.CtrlVoice._voiceList.DicDicDicDicVoice[i])
                        {
                            Log.LogInfo("i: " + i + ", kvp.Key: " + kvp.Key + ", item count: " + kvp.Value.Count);
                            foreach (var kvp2 in kvp.Value)
                            {
                                Log.LogInfo("i: " + i + ", kvp.Key: " + kvp.Key + ", kvp2.Key: " + kvp2.Key + ", item count: " + kvp2.Value.Count);

                                foreach (var kvp3 in kvp2.Value)
                                {
                                    Log.LogInfo("i: " + i + ", kvp.Key: " + kvp.Key + ", kvp2.Key: " + kvp2.Key + ", kvp3.Key: " + kvp3.Key + ", item count: " + kvp3.Value.Count);

                                    foreach (var kvp4 in kvp3.Value)
                                    {
                                        Log.LogInfo("i: " + i + ", kvp.Key: " + kvp.Key + ", kvp2.Key: " + kvp2.Key + ", kvp3.Key: " + kvp3.Key + ", kvp4.Key: " + kvp4.Key);
                                        Debug.PrintDetail(kvp4.Value);
                                    }
                                }
                            }
                        }
                    }


                }

            }
        }

        internal static void PrintAllHPoint()
        {
            var list = StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst;
            foreach (var kvp in list)
            {
                Log.LogInfo("HPoint kvp.Key: " + kvp.Key);
                foreach (var item in kvp.Value.HPoints)
                {
                    Log.LogInfo("HPoint ID: " + item.ID + ", name: " + item.name + ", position: " + item.transform.position + ", rotation: " + item.transform.rotation.eulerAngles + ", now using? " + item.NowUsing + ", instanceID: " + item.GetInstanceID());
                    
                }

            }
            
        }

        internal static HScene.AnimationListInfo GetAnimFromTable(int cat, int id)
        {
            foreach (var item in Manager.HSceneManager.HResourceTables.LstAnimInfo[cat])
            {
                if (item.ID == id)
                    return item;
            }
            return null;
        }

        internal enum HPointType
        {
            OfficeConferenceTable,
            OfficeConferenceSeat,
            OfficeWall,
            OfficeFloor,
            OfficeToilet,
            OfficeCounter,
            OfficeSeatNoBack,
            OfficeDesk,

            ClinicsPatientSeat,
            ClinicsSeatBack,
            ClinicsSeat,
            ClinicsBed,
            ClinicsDeliveryTable,
            ClinicsHospitalBed,

            SeminarCounter,
            SeminarStudentSeat,
            SeminarInstructorSeat,

            LivehouseMirror,
            LivehouseRoundSeat,
            LivehouseSeatBack,

            CasinoPole,
            CasinoTable,
            CasinoSeat,

            LivingRoomSofa,
            LivingRoomCounter,
            LivingRoomBathTub,
            LivingRoomSeat,
            ParkLongChair
        }

        internal static HPoint GetHPoint(HPointType type)
        {
            switch (type)
            {
                case HPointType.OfficeConferenceTable:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[5].HPoints[7];       //conference table
                case HPointType.OfficeConferenceSeat:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[3].HPoints[5];       //conference seat
                case HPointType.OfficeSeatNoBack:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[3].HPoints[7];       //seat no back
                case HPointType.OfficeWall:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[2].HPoints[1];       //wall
                case HPointType.OfficeFloor:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[0].HPoints[2];       //floor
                case HPointType.OfficeToilet:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[9].HPoints[0];       //toilet
                case HPointType.OfficeCounter:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[7].HPoints[0];       //counter
                case HPointType.OfficeDesk:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[5].HPoints[0];       //desk
                case HPointType.SeminarCounter:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[7].HPoints[0];       //counter
                case HPointType.SeminarStudentSeat:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[3].HPoints[0];       //student seat
                case HPointType.SeminarInstructorSeat:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[3].HPoints[3];       //instructor seat
                case HPointType.ClinicsPatientSeat:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[3].HPoints[0];       //patient seat
                case HPointType.ClinicsSeat:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[3].HPoints[2];       //seat no back
                case HPointType.ClinicsSeatBack:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[3].HPoints[1];       //seat with back
                case HPointType.ClinicsBed:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[9].HPoints[3];       //patient bed 
                case HPointType.ClinicsDeliveryTable:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[9].HPoints[1];       //delivery table 
                case HPointType.ClinicsHospitalBed:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[9].HPoints[2];       //hospital bed
                case HPointType.LivehouseMirror:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[9].HPoints[2];       //in front of mirror
                case HPointType.LivehouseRoundSeat:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[3].HPoints[2];       //round seat in front of mirror
                case HPointType.LivehouseSeatBack:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[3].HPoints[0];       //seat with back
                case HPointType.CasinoPole:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[9].HPoints[1];       //pole
                case HPointType.CasinoTable:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[5].HPoints[0];       //gamble table
                case HPointType.CasinoSeat:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[3].HPoints[12];       //seat
                case HPointType.LivingRoomSofa:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[6].HPoints[0];       //sofa
                case HPointType.LivingRoomCounter:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[7].HPoints[0];       //Counter
                case HPointType.LivingRoomBathTub:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[9].HPoints[1];       //bath tub
                case HPointType.LivingRoomSeat:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[3].HPoints[0];       //Seat
                case HPointType.ParkLongChair:
                    return StateManager.Instance.CurrentHSceneInstance.HPointCtrl.HPointList.Lst[4].HPoints[0];       //Long Chair
                default:
                    return null;
            }

        }

        internal static void PrintAllABAssetName(string abPath)
        {
            string path = Util.GetAssetBundleBasePath() + abPath;
            AssetBundle ab = AssetBundle.LoadFromFile(path);

            if (ab != null)
            {

                foreach (var s in ab.AllAssetNames())
                {
                    Log.LogInfo(abPath + ": " + s);
                }
            }
            ab.Unload(false);
        }

        private static void GetComponentTypes(Transform t)
        {
            var c1 = t.GetComponent<Renderer>(); if (c1 != null) Log.LogInfo("has Renderer");
            var c2 = t.GetComponent<MeshFilter>(); if (c2 != null) Log.LogInfo("has MeshFilter");
            var c3 = t.GetComponent<LODGroup>(); if (c3 != null) Log.LogInfo("has LODGroup");
            var c4 = t.GetComponent<Behaviour>(); if (c4 != null) Log.LogInfo("has Behaviour");

            var c5 = t.GetComponent<Transform>(); if (c5 != null) Log.LogInfo("has Transform");
            var c6 = t.GetComponent<CanvasRenderer>(); if (c6 != null) Log.LogInfo("has CanvasRenderer");
            var c7 = t.GetComponent<Component>(); if (c7 != null) Log.LogInfo("has Component");
            var c8 = t.GetComponent<RectTransform>(); if (c8 != null) Log.LogInfo("has RectTransform");

            var c9 = t.GetComponent<BillboardRenderer>(); if (c9 != null) Log.LogInfo("has BillboardRenderer");
            var c10 = t.GetComponent<LineRenderer>(); if (c10 != null) Log.LogInfo("has LineRenderer");
            var c11 = t.GetComponent<SkinnedMeshRenderer>(); if (c11 != null) Log.LogInfo("has SkinnedMeshRenderer");
            var c12 = t.GetComponent<MeshRenderer>(); if (c12 != null) Log.LogInfo("has MeshRenderer");
            var c13 = t.GetComponent<SpriteRenderer>(); if (c13 != null) Log.LogInfo("has SpriteRenderer");
            var c14 = t.GetComponent<Animator>(); if (c14 != null) Log.LogInfo("has Animator");
            var c15 = t.GetComponent<MonoBehaviour>(); if (c15 != null) Log.LogInfo("has MonoBehaviour");

            var c17 = t.GetComponent<VerticalLayoutGroup>(); if (c17 != null) Log.LogInfo("has VerticalLayoutGroup");
            var c18 = t.GetComponent<HorizontalLayoutGroup>(); if (c18 != null) Log.LogInfo("has HorizontalLayoutGroup");
            var c19 = t.GetComponent<LayoutGroup>(); if (c19 != null) Log.LogInfo("has LayoutGroup");
            var c20 = t.GetComponent<GridLayoutGroup>(); if (c20 != null) Log.LogInfo("has GridLayoutGroup");
            var c21 = t.GetComponent<ContentSizeFitter>(); if (c21 != null) Log.LogInfo("has ContentSizeFitter");
            var c22 = t.GetComponent<Canvas>(); if (c22 != null) Log.LogInfo("has Canvas");
            var c23 = t.GetComponent<ContentSizeFitter>(); if (c23 != null) Log.LogInfo("has ContentSizeFitter");
        }
    }
}
