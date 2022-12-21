using UnityEngine;
using System.Collections.Generic;

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
    }
}