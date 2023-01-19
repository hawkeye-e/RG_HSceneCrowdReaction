using BepInEx.Logging;
using RG.Scene.Action.Core;
using System.Collections.Generic;
using System.Linq;

namespace HSceneCrowdReaction.BackgroundHAnimation
{
    internal class HAnimationGroup
    {
        private static ManualLogSource Log = HSceneCrowdReactionPlugin.Log;

        internal Actor male1;
        internal Actor male2;
        internal Actor female1;
        internal Actor female2;
        internal InfoList.HAnimation.SituationType situationType;

        public HAnimationGroup()
        {
            male1 = null;
            male2 = null;
            female1 = null;
            female2 = null;
            situationType = InfoList.HAnimation.SituationType.Unknown;
        }

        internal static List<HAnimationGroup> GetHAnimationGroups(List<Actor> actorList)
        {
            List<HAnimationGroup> result = new List<HAnimationGroup>();

            switch (Config.HAnimMatchType)
            {
                case Config.HAnimMatchingType.Default:
                    return GetHAnimationGroupsDefault(actorList);
                case Config.HAnimMatchingType.AutoMatch:
                    return GetHAnimationGroupsAutoMatch(actorList);
            }

            return result;
        }

        //For the default grouping, we check for each actor pair that is possible for H action
        private static List<HAnimationGroup> GetHAnimationGroupsDefault(List<Actor> actorList)
        {
            List<HAnimationGroup> result = new List<HAnimationGroup>();

            List<int> processedActorID = new List<int>();
            var threesomeActors = actorList.Where(a => a.ThreesomeTarget != null && a.ThreesomeTarget.Partner != null);
            foreach (var actor in threesomeActors)
            {
                if (!processedActorID.Contains(actor.GetInstanceID()))
                {
                    if (IsHActionPossible(actor, actor.ThreesomeTarget, actor.ThreesomeTarget.Partner))
                    {
                        HAnimationGroup newGroup = new HAnimationGroup();
                        newGroup.AssignActor(actor.ThreesomeTarget);
                        newGroup.AssignActor(actor.ThreesomeTarget.Partner);
                        newGroup.AssignActor(actor);

                        newGroup.UpdateSituationType();

                        processedActorID.Add(actor.GetInstanceID());
                        processedActorID.Add(actor.ThreesomeTarget.GetInstanceID());
                        processedActorID.Add(actor.ThreesomeTarget.Partner.GetInstanceID());

                        result.Add(newGroup);
                    }
                }
            }

            var pairedActors = actorList.Where(a => a.Partner != null && !processedActorID.Contains(a.GetInstanceID()));
            foreach (var actor in pairedActors)
            {
                if (!processedActorID.Contains(actor.GetInstanceID()))
                {
                    if (IsHActionPossible(actor, actor.Partner))
                    {
                        HAnimationGroup newGroup = new HAnimationGroup();
                        newGroup.AssignActor(actor);
                        newGroup.AssignActor(actor.Partner);

                        newGroup.UpdateSituationType();

                        processedActorID.Add(actor.GetInstanceID());
                        processedActorID.Add(actor.Partner.GetInstanceID());

                        result.Add(newGroup);
                    }
                }
            }

            return result;
        }

        //Auto match case
        private static List<HAnimationGroup> GetHAnimationGroupsAutoMatch(List<Actor> actorList)
        {
            List<HAnimationGroup> result = new List<HAnimationGroup>();
            List<int> processedActorID = new List<int>();

            //Key: Actor instance ID, value: list of possible match
            Dictionary<int, List<Actor>> dictPossibleMatch = new Dictionary<int, List<Actor>>();
            foreach (var actor in actorList.Where(a => a.Sex == 1))
            {
                //loop through female
                List<Actor> possibleActorList = new List<Actor>();
                foreach (var toBeMatched in actorList)
                {
                    if (actor.GetInstanceID() != toBeMatched.GetInstanceID() && IsHActionPossible(actor, toBeMatched))
                    {
                        possibleActorList.Add(toBeMatched);
                    }
                }
                dictPossibleMatch.Add(actor.GetInstanceID(), possibleActorList);
            }

            //now we scan the dictionary and pick the entry with least possible match count and start assigning group 
            while (dictPossibleMatch.Count > 0)
            {
                int minCount = 0;
                var nonZeroMatch = dictPossibleMatch.Where(dictItem => dictItem.Value.Count > 0).ToList();
                if(nonZeroMatch.Count > 0)
                    minCount = nonZeroMatch.Min(dictItem => dictItem.Value.Count);

                if(minCount > 0)
                {
                    var kvp = dictPossibleMatch.Where(a => a.Value.Count == minCount).First();
                    var actor = actorList.Where(a => a.GetInstanceID() == kvp.Key).First();

                    System.Random rnd = new System.Random();
                    int rndPick = rnd.Next(kvp.Value.Count);

                    HAnimationGroup newGroup = new HAnimationGroup();
                    newGroup.AssignActor(actor);
                    newGroup.AssignActor(kvp.Value[rndPick]);
                    newGroup.UpdateSituationType();

                    processedActorID.Add(actor.GetInstanceID());
                    processedActorID.Add(kvp.Value[rndPick].GetInstanceID());

                    //now we have to remove the assigned actor from the dictionary and possible match list
                    List<int> usedID = new List<int>();
                    usedID.Add(actor.GetInstanceID());
                    usedID.Add(kvp.Value[rndPick].GetInstanceID());

                    foreach (var id in usedID)
                    {
                        dictPossibleMatch.Remove(id);
                        foreach (var unassignedKVP in dictPossibleMatch)
                        {
                            unassignedKVP.Value.RemoveAll(a => a.GetInstanceID() == id);
                        }
                    }

                    result.Add(newGroup);
                }
                else
                {
                    //there is no possible match remaining in the dict, exit this loop
                    break;
                }
            }

            ////try to assign the remaining actor to threesome
            //List<Actor> notAssignedActors = actorList.Where(a => !processedActorID.Contains(a.GetInstanceID())).ToList();
            //foreach(var actor in notAssignedActors)
            //{
            //    var twoActorGroup = result.Where(r => r.situationType == InfoList.HAnimation.SituationType.MF || r.situationType == InfoList.HAnimation.SituationType.FF);
            //    foreach (var group in twoActorGroup)
            //    {
            //        if(group.situationType == InfoList.HAnimation.SituationType.MF)
            //        {
            //            if(IsHActionPossible(actor, group.male1, group.female1))
            //            {
            //                group.AssignActor(actor);
            //                group.UpdateSituationType();
            //            }   
            //        }else if(group.situationType == InfoList.HAnimation.SituationType.FF)
            //        {
            //            if (IsHActionPossible(actor, group.female1, group.female2))
            //            {
            //                group.AssignActor(actor);
            //                group.UpdateSituationType();
            //            }
            //        }
            //    }
            //}


            return result;
        }

        public void AssignActor(Actor actor)
        {
            if (actor.Sex == 0)
            {
                if (male1 == null)
                    male1 = actor;
                else
                    male2 = actor;
            }
            else
            {
                if (female1 == null)
                    female1 = actor;
                else
                    female2 = actor;
            }
        }

        public void UpdateSituationType()
        {
            if (female2 != null && male1 == null && male2 == null)
                situationType = InfoList.HAnimation.SituationType.FF;
            else if (female2 != null && male1 != null && male2 == null)
                situationType = InfoList.HAnimation.SituationType.FFM;
            else if (female2 == null && male1 != null && male2 != null)
                situationType = InfoList.HAnimation.SituationType.MMF;
            else if (female2 == null && male1 != null && male2 == null)
                situationType = InfoList.HAnimation.SituationType.MF;
            else if (female2 == null && male1 == null && male2 == null)
                situationType = InfoList.HAnimation.SituationType.F;
            else
                situationType = InfoList.HAnimation.SituationType.Unknown;
        }


        private static bool IsHActionPossible(Actor actor1, Actor actor2)
        {
            if (actor1.Sex == 0 && actor2.Sex == 0)       //block if both male
                return false;

            if (!Util.CheckHasEverSex(actor1, actor2) && Config.SexExpPrerequisite)    //block if the pair does not have sex before
                return false;

            return true;
        }

        private static bool IsHActionPossible(Actor actor1, Actor actor2, Actor actor3)
        {
            if (actor1.Sex == actor2.Sex && actor1.Sex == actor3.Sex)       //block if all same sex
                return false;

            HAnimationGroup tmpGroup = new HAnimationGroup();
            tmpGroup.AssignActor(actor1);
            tmpGroup.AssignActor(actor2);
            tmpGroup.AssignActor(actor3);

            if (Config.SexExpPrerequisite)
            {
                if (tmpGroup.male2 != null)
                {   //MMF, block if the female does not have sex with both male before
                    if (!Util.CheckHasEverSex(tmpGroup.male1, tmpGroup.female1) || !Util.CheckHasEverSex(tmpGroup.male2, tmpGroup.female1))
                        return false;
                }
                else
                {   //FFM, block if the male does not have sex with both female before
                    if (!Util.CheckHasEverSex(tmpGroup.male1, tmpGroup.female1) || !Util.CheckHasEverSex(tmpGroup.male1, tmpGroup.female2))
                        return false;
                }
            }

            return true;
        }

    }
}
