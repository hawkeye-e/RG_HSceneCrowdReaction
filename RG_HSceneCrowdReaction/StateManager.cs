using UnityEngine;
using System.Collections.Generic;
using System.Timers;
using HSceneCrowdReaction.InfoList;

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


        internal bool isprint = false;
        internal List<int> isrotated= new List<int>();
        internal Transform left = null;
        internal Transform right = null;

        internal List<int> animCtrlIDs = new List<int>();
    }
}