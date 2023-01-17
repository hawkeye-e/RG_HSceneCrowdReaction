using RG.Scene.Action.Core;
using System.Timers;

namespace HSceneCrowdReaction
{
    internal class CustomTimer : Timer
    {
        public Actor Actor1;
        public Actor Actor2;

        public CustomTimer(Actor actor1, Actor actor2, double interval) : base(interval)
        {
            this.Actor1 = actor1;
            this.Actor2 = actor2;
        }

        public CustomTimer() { }
    }
}
