using BepInEx.Logging;
using RG.Scene;
using RG.Scene.Action.Core;
using HSceneCrowdReaction.InfoList;
using System;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using UnityEngine;

namespace HSceneCrowdReaction.BackgroundHAnimation
{
    internal class HAnimationGroup
    {
        private static ManualLogSource Log = HSceneCrowdReactionPlugin.Log;

        internal Actor male1;
        internal Actor male2;
        internal Actor male3;
        internal Actor female1;
        internal Actor female2;
        internal HAnimation.SituationType situationType;
        internal HPoint hPoint;

        public HAnimationGroup()
        {
            male1 = null;
            male2 = null;
            male3 = null;
            female1 = null;
            female2 = null;
            situationType = HAnimation.SituationType.Unknown;
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

            List<int> processedActorIDs = new List<int>();
            List<HPoint> threesomePoints = GetAvailableThreesomeHPointList();
            int current3PCount = 0;
            var threesomeActors = actorList.Where(a => a.ThreesomeTarget != null && a.ThreesomeTarget.Partner != null);
            foreach (var actor in threesomeActors)
            {
                if (current3PCount >= threesomePoints.Count)
                    break;

                if (!processedActorIDs.Contains(actor.GetInstanceID()))
                {
                    if (IsHActionPossible(actor, actor.ThreesomeTarget, actor.ThreesomeTarget.Partner))
                    {
                        HAnimationGroup newGroup = new HAnimationGroup();
                        newGroup.AssignActor(actor.ThreesomeTarget);
                        newGroup.AssignActor(actor.ThreesomeTarget.Partner);
                        newGroup.AssignActor(actor);

                        newGroup.UpdateSituationType();

                        processedActorIDs.Add(actor.GetInstanceID());
                        processedActorIDs.Add(actor.ThreesomeTarget.GetInstanceID());
                        processedActorIDs.Add(actor.ThreesomeTarget.Partner.GetInstanceID());

                        result.Add(newGroup);

                        current3PCount++;
                    }
                }
            }

            var pairedActors = actorList.Where(a => a.Partner != null && !processedActorIDs.Contains(a.GetInstanceID()));
            foreach (var actor in pairedActors)
            {
                if (!processedActorIDs.Contains(actor.GetInstanceID()))
                {
                    if (IsHActionPossible(actor, actor.Partner))
                    {
                        HAnimationGroup newGroup = new HAnimationGroup();
                        newGroup.AssignActor(actor);
                        newGroup.AssignActor(actor.Partner);

                        newGroup.UpdateSituationType();

                        processedActorIDs.Add(actor.GetInstanceID());
                        processedActorIDs.Add(actor.Partner.GetInstanceID());

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
                if (nonZeroMatch.Count > 0)
                    minCount = nonZeroMatch.Min(dictItem => dictItem.Value.Count);

                if (minCount > 0)
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

            //try to assign the remaining actor to threesome
            List<HPoint> threesomePoints = GetAvailableThreesomeHPointList();
            int current3PCount = 0;
            List<Actor> notAssignedActors = actorList.Where(a => !processedActorID.Contains(a.GetInstanceID())).ToList();
            foreach (var actor in notAssignedActors)
            {
                if (current3PCount >= threesomePoints.Count)
                    break;

                var twoActorGroup = result.Where(r => r.situationType == HAnimation.SituationType.MF || r.situationType == HAnimation.SituationType.FF);
                foreach (var group in twoActorGroup)
                {
                    if (group.situationType == HAnimation.SituationType.MF)
                    {
                        if (IsHActionPossible(actor, group.male1, group.female1))
                        {
                            group.AssignActor(actor);
                            group.UpdateSituationType();
                            
                            current3PCount++;
                            break;
                        }
                    }
                    else if (group.situationType == HAnimation.SituationType.FF)
                    {
                        if (IsHActionPossible(actor, group.female1, group.female2))
                        {
                            group.AssignActor(actor);
                            group.UpdateSituationType();

                            current3PCount++;
                            break;
                        }
                    }
                }
            }


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
            if (female2 != null && male1 == null && male2 == null && male3 == null)
                situationType = HAnimation.SituationType.FF;
            else if (female2 != null && male1 != null && male2 == null && male3 == null)
                situationType = HAnimation.SituationType.FFM;
            else if (female2 == null && male1 != null && male2 != null && male3 == null)
                situationType = HAnimation.SituationType.MMF;
            else if (female2 == null && male1 != null && male2 == null && male3 == null)
                situationType = HAnimation.SituationType.MF;
            else if (female2 == null && male1 == null && male2 == null && male3 == null)
                situationType = HAnimation.SituationType.F;
            else if (female2 == null && male1 != null && male2 != null && male3 != null)
                situationType = HAnimation.SituationType.MMMF;
            else if (female2 != null && male1 != null && male2 != null && male3 == null)
                situationType = HAnimation.SituationType.MMFF;
            else
                situationType = HAnimation.SituationType.Unknown;
        }

        public List<Actor> GetActorList()
        {
            List<Actor> list = new List<Actor>();
            if (male1 != null) list.Add(male1);
            if (male2 != null) list.Add(male2);
            if (male3 != null) list.Add(male3);
            if (female1 != null) list.Add(female1);
            if (female2 != null) list.Add(female2);
            return list;
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


        internal static HPoint GetHPointForGroup(HAnimationGroup group)
        {
            return GetHPoint(group.female1, group.situationType);
        }

        private static HPoint GetHPoint(Actor actor, HAnimation.SituationType situationType)
        {
            ActionPoint targetAP = actor.OccupiedActionPoint == null ? actor.Partner?.OccupiedActionPoint : actor.OccupiedActionPoint;

            if (targetAP == null)
                targetAP = actor.PostedActionPoint;

            System.Random rnd = new System.Random();

            List<HPoint> availablePoint = new List<HPoint>();
            List<HPoint> occupyiedPoints = GetOccupiedHPointList();

            Il2CppReferenceArray<HPoint> pointArray = targetAP.HPointLink;
            if (situationType == HAnimation.SituationType.MMF || situationType == HAnimation.SituationType.FFM)
                pointArray = targetAP.HPoint3PLink;
            foreach (var hpoint in pointArray)
            {
                if (!hpoint.NowUsing && occupyiedPoints.FindAll(p => p.GetInstanceID() == hpoint.GetInstanceID()).Count == 0)
                    availablePoint.Add(hpoint);
            }

            if (availablePoint.Count == 0)
            {
                int[] validHPointTypes = null;
                if (situationType == HAnimation.SituationType.MF)
                    validHPointTypes = HAnimation.ValidHPointTypeMF;
                else if (situationType == HAnimation.SituationType.FF)
                    validHPointTypes = HAnimation.ValidHPointTypeFF;
                else if (situationType == HAnimation.SituationType.FFM)
                    validHPointTypes = HAnimation.ValidHPointTypeFFM;
                else if (situationType == HAnimation.SituationType.MMF)
                    validHPointTypes = HAnimation.ValidHPointTypeMMF;

                //the current linked hpoint is occupied, randomly choose a unused one
                foreach (var kvp in StateManager.Instance.FullHPointListInMap)
                {
                    if (Array.IndexOf(validHPointTypes, kvp.Key) > -1)
                    {
                        foreach (var hpoint in kvp.Value.HPoints)
                        {
                            if (!hpoint.NowUsing && occupyiedPoints.FindAll(p => p.GetInstanceID() == hpoint.GetInstanceID()).Count == 0)
                                availablePoint.Add(hpoint);
                        }
                    }
                }
            }

            int rndResult = rnd.Next(availablePoint.Count);
            return availablePoint[rndResult];
        }

        internal static List<HPoint> GetOccupiedHPointList(bool includeMainHScenePoint = true)
        {
            List<HPoint> lstOccupiedPoint = new List<HPoint>();

            if (ActionScene.Instance == null) return lstOccupiedPoint;

            List<Actor> charList = Util.GetActorsNotInvolvedInH(ActionScene.Instance, StateManager.Instance.CurrentHSceneInstance);

            //single actor
            foreach (var group in StateManager.Instance.ActorHGroupDictionary)
            {
                foreach (var bgHChar in group.Value.GetActorList())
                {
                    charList.RemoveAll(a => a.GetInstanceID() == bgHChar.GetInstanceID());
                }
            }

            foreach (var actor in charList)
            {
                ActionPoint targetAP = actor.OccupiedActionPoint == null ? actor.Partner?.OccupiedActionPoint : actor.OccupiedActionPoint;

                if (targetAP != null)
                    foreach (var point in targetAP.HPointLink)
                        if (lstOccupiedPoint.FindAll(p => p.GetInstanceID() == point.GetInstanceID()).Count == 0)
                            lstOccupiedPoint.Add(point);
                //foreach (var point in targetAP.HPoint3PLink)
                //    if (lstOccupiedPoint.FindAll(p => p.GetInstanceID() == point.GetInstanceID()).Count == 0)
                //        lstOccupiedPoint.Add(point);
            }

            //assigned background group
            foreach (var group in StateManager.Instance.ActorHGroupDictionary)
            {
                if (group.Value.hPoint != null)
                {
                    var toAdd = GetHPointsByPosition(group.Value.hPoint.transform.position);
                    foreach (var point in toAdd)
                        if (lstOccupiedPoint.FindAll(p => p.GetInstanceID() == point.GetInstanceID()).Count == 0)
                            lstOccupiedPoint.Add(point);
                }
            }

            //main h scene actors
            if (includeMainHScenePoint)
            {
                foreach (var lst in StateManager.Instance.FullHPointListInMap)
                {
                    foreach (var point in lst.Value.HPoints)
                        if (point.NowUsing)
                            lstOccupiedPoint.Add(point);
                }
            }

            //include the HPoint with same position in the list to avoid overlap due to pair and 3P difference
            foreach (var lst in StateManager.Instance.FullHPointListInMap)
                foreach (var point in lst.Value.HPoints)
                {
                    var toAdd = lstOccupiedPoint.FindAll(p => p.GetInstanceID() != point.GetInstanceID() && p.transform.position == point.transform.position).ToList();
                    lstOccupiedPoint.AddRange(toAdd);
                }



            return lstOccupiedPoint;
        }

        internal static List<HPoint> GetAvailableThreesomeHPointList()
        {
            List<HPoint> list = new List<HPoint>();

            var lstOccupiedPoint = GetOccupiedHPointList();
            foreach (var point in StateManager.Instance.FullHPointListInMap[Constant.ThreesomeHPointIndex].HPoints)
            {
                if (!lstOccupiedPoint.Any(p => p.GetInstanceID() == point.GetInstanceID()))
                {
                    list.Add(point);
                }
            }

            return list;
        }

        internal static List<HPoint> GetHPointsByPosition(Vector3 pos)
        {
            List<HPoint> hPoints = new List<HPoint>();
            foreach (var lst in StateManager.Instance.FullHPointListInMap)
                foreach (var point in lst.Value.HPoints)
                    if (point.transform.position == pos)
                        hPoints.Add(point);
            return hPoints;
        }
    }
}
