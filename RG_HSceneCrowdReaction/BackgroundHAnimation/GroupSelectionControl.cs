using BepInEx.Logging;
using RG.Scene;
using RG.Scene.Action.Core;
using HSceneCrowdReaction.InfoList;
using System;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
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
                stateManager.CurrentSelectedGroup = null;

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

                //Call twice to restart the move button flow
                hScene._sprite.OnClickMoveBt();
                hScene._sprite.OnClickMoveBt();
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

                //Call twice to restart the move button flow
                hScene._sprite.OnClickMoveBt();
                hScene._sprite.OnClickMoveBt();
            }
        }

        private void UpdateGroupSelection()
        {
            if (selectedIndex < 0)
            {
                stateManager.CurrentSelectedGroup = null;
                groupName.text = hScene._chaFemales[0].FileParam.fullname;
            }
            else
            {
                stateManager.CurrentSelectedGroup = lstAnimationGroups[selectedIndex];
                groupName.text = lstAnimationGroups[selectedIndex].female1.Status.FullName;
            }
        }
    }
}
