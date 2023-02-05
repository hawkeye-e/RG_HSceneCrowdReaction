using BepInEx.Logging;
using RG.Scene;
using HSceneCrowdReaction.InfoList;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HSceneCrowdReaction.BackgroundHAnimation
{
    internal class GroupSelectionControl
    {
        private static ManualLogSource Log = HSceneCrowdReactionPlugin.Log;

        internal Canvas canvas = null;
        internal Text groupName = null;
        internal int selectedIndex = -1;
        internal HScene hScene;
        internal List<HAnimationGroup> lstAnimationGroups;
        internal StateManager stateManager;

        internal HAnimationGroup SelectedGroup
        {
            get
            {
                if (selectedIndex >= 0)
                    return lstAnimationGroups[selectedIndex];
                return null;
            }
        }

        public GroupSelectionControl(HScene hScene, List<HAnimationGroup> groups, StateManager stateManager)
        {
            this.hScene = hScene;
            this.lstAnimationGroups = groups;
            this.stateManager = stateManager;
        }


        public void InitControl(Transform parentTransform)
        {
            if (ActionScene.Instance != null)
            {
                if (stateManager.GroupSelectionAB == null)
                {
                    stateManager.GroupSelectionAB = AssetBundle.LoadFromMemory(Resources.Layout.ReactionLayout);

                }
                canvas = Util.InstantiateFromBundle(stateManager.GroupSelectionAB, "assets/prefab/cvsgroup.prefab").GetComponent<Canvas>();
                canvas.gameObject.SetActive(false);
                canvas.transform.SetParent(parentTransform, false);

                //Initiate the group value
                groupName = canvas.transform.Find("lblName").GetComponent<Text>();
                groupName.text = hScene._chaFemales[0].FileParam.fullname;
                selectedIndex = -1;

                var btnLeft = canvas.transform.Find("btnLeft").GetComponent<Button>();
                btnLeft.onClick = new Button.ButtonClickedEvent();
                btnLeft.onClick.AddListener((UnityAction)SwitchToPreviousGroup);

                var btnRight = canvas.transform.Find("btnRight").GetComponent<Button>();
                btnRight.onClick = new Button.ButtonClickedEvent();
                btnRight.onClick.AddListener((UnityAction)SwitchToNextGroup);

                canvas.transform.position = new Vector3(90, Screen.height - 35, 0);

                //Move the sex position panel lower
                var objTaii = hScene._sprite.ObjTaii;
                objTaii.transform.position = new Vector3(
                    objTaii.transform.position.x,
                    objTaii.transform.position.y - 50,
                    objTaii.transform.position.z
                    );
            }
        }

        public void SwitchToPreviousGroup()
        {
            if (canvas != null)
            {
                selectedIndex--;
                if (selectedIndex < -1)
                    selectedIndex = lstAnimationGroups.Count - 1;

                UpdateGroupSelection();
                UpdateUI();
            }
        }

        public void SwitchToNextGroup()
        {
            if (canvas != null)
            {
                selectedIndex++;
                if (selectedIndex >= lstAnimationGroups.Count)
                    selectedIndex = -1;

                UpdateGroupSelection();
                UpdateUI();
            }
        }

        private void UpdateGroupSelection()
        {
            if (selectedIndex < 0)
                groupName.text = hScene._chaFemales[0].FileParam.fullname;
            else
                groupName.text = lstAnimationGroups[selectedIndex].female1.Status.FullName;
        }

        public void UpdateUI()
        {
            if (hScene._sprite._ctrlFlag.IsPointMoving)
            {
                //Call twice to restart the move button flow
                hScene._sprite.OnClickMoveBt();
                hScene._sprite.OnClickMoveBt();
            }

            if (hScene._sprite.ObjTaii.isFadeIn)
            {
                HScene.AnimationListInfo animInfo;
                if (SelectedGroup != null)
                    animInfo = stateManager.ActorHAnimationList[SelectedGroup.female1.GetInstanceID()].animationListInfo;
                else
                    animInfo = stateManager.CurrentHSceneInstance.CtrlFlag.NowAnimationInfo;

                int animGroup = Util.GetHAnimationGroup(animInfo);

                int targetCategoryValue = HAnimation.GetIconValueByCategory(HAnimation.ExtraHAnimationDataDictionary[(animGroup, animInfo.ID)].iconCategory);
                hScene._sprite.CategoryMain.SetNowIcon(targetCategoryValue);
                string targetIconName = HAnimation.GetIconObjectNameByCategory(HAnimation.ExtraHAnimationDataDictionary[(animGroup, animInfo.ID)].iconCategory);

                //Call onclick twice to ensure the UI refreshed
                if (targetIconName == HAnimation.IconName.FemaleLeading)
                {
                    hScene._sprite.OnClickMotion(0);
                    hScene._sprite.OnClickMotionFemale();
                }
                else
                {
                    int spoofCategory = targetCategoryValue == 0 ? 1 : 0;
                    hScene._sprite.OnClickMotion(spoofCategory);
                    hScene._sprite.OnClickMotion(targetCategoryValue);
                }
                
            }
        }
    }
}
