using UnityEngine;
using System.Collections.Generic;
using System.Timers;
using HSceneCrowdReaction.InfoList;
using HSceneCrowdReaction.BackgroundHAnimation;
using RG.Scene.Action.Core;

namespace HSceneCrowdReaction
{
    internal class StateManager
    {
        public StateManager()
        {
            ActorClothesState = new Dictionary<int, byte[]>();
        }

        internal static StateManager Instance;

        internal bool HSceneSetup = false;
        internal Dictionary<int, CustomAnimation.CustomAnimationData> CustomAnimationParameter = null;
        internal HScene CurrentHSceneInstance = null;
        internal GameObject HeadDownLookTarget = null;

        internal List<int> ForceActiveInstanceID = null;
        internal List<Timer> HActionUpdateTimerList = null;
        internal Dictionary<int, HAnimation.ActorHAnimData> ActorHAnimationList = null;
        internal Dictionary<int, long> ActorHAnimNextUpdateTimeDictionary = null;
        internal Dictionary<int, bool> ActorHAnimNextUpdateProcessing = null;

        internal Dictionary<int, byte[]> ActorClothesState = null;
        

        internal Dictionary<int, CollisionCtrl> CharacterCollisionCtrlDictionary = null;
        internal Dictionary<int, HitObjectCtrl> CharacterHitObjectCtrlDictionary = null;
        internal Dictionary<int, YureCtrl> CharacterYureCtrlDictionary = null;
        internal Dictionary<int, DynamicBoneReferenceCtrl> CharacterDynamicBoneCtrlDictionary = null;
        internal Dictionary<int, HLayerCtrl> CharacterHLayerCtrlDictionary = null;
        internal Dictionary<int, HItemCtrl> CharacterHItemCtrlDictionary = null;
        internal Dictionary<int, HPoint> CharacterHPointDictionary = null;

        internal Dictionary<int, bool> CharacterCtrlInitFinishedDictionary = null;

        internal Dictionary<int, Transform> ForceBlowJobTarget = null;

        internal List<int> HSceneParticipantActorIDList = null;

        internal List<int> HSceneOccupiedHPointIDList = null;

        internal Dictionary<int, BackUpInformation> ActorBackUpData = null;
        internal Dictionary<int, HAnimationGroup> ActorHGroupDictionary = null;


        internal class BackUpInformation
        {
            internal int lookEyePtn;
            internal int lookNeckPtn;
            internal Transform lookEyeTarget;
            internal Transform lookNeckTarget;
        }

        internal static void UpdateHGroupDictionary(HAnimationGroup group)
        {
            UpdateHGroupDictionary(group.male1, group);
            UpdateHGroupDictionary(group.male2, group);
            UpdateHGroupDictionary(group.female1, group);
            UpdateHGroupDictionary(group.female2, group);
        }

        internal static void UpdateHGroupDictionary(Actor actor, HAnimationGroup group)
        {
            if (actor == null) return;

            if (Instance.ActorHGroupDictionary.ContainsKey(actor.GetInstanceID()))
                Instance.ActorHGroupDictionary[actor.GetInstanceID()] = group;
            else
                Instance.ActorHGroupDictionary.Add(actor.GetInstanceID(), group);
        }
    }
}