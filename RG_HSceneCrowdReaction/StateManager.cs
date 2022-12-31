using UnityEngine;
using System.Collections.Generic;
using System.Timers;

namespace HSceneCrowdReaction
{
    internal class StateManager
    {
        public StateManager()
        {

        }

        internal static StateManager Instance;

        internal bool HSceneSetup = false;
        internal Dictionary<int, CustomAnimation.CustomAnimationData> CustomAnimationParameter = null;
        internal HScene CurrentHSceneInstance = null;
        internal GameObject HeadDownLookTarget = null;

        internal List<int> ForceActiveInstanceID = null;
        internal List<Timer> HActionUpdateTimerList = null;
        internal Dictionary<int, HPoint> HActionActorList = null;

        internal bool isprint = false;

    }
}