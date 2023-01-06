using BepInEx.Logging;
using UnityEngine;
using RG.Scene.Action.Core;
using UnhollowerRuntimeLib;
using System.IO;
using HSceneCrowdReaction.InfoList;

namespace HSceneCrowdReaction
{
    internal class Util
    {
        private static ManualLogSource Log = HSceneCrowdReactionPlugin.Log;

        internal static Constant.CharacterType GetCharacterType(Actor actor)
        {
            float maxValue = actor.Status.Parameters[(int)RG.Define.Action.StatusCategory.Honesty];
            if (actor.Status.Parameters[(int)RG.Define.Action.StatusCategory.Honesty] > actor.Status.Parameters[(int)RG.Define.Action.StatusCategory.Naughty]
                && actor.Status.Parameters[(int)RG.Define.Action.StatusCategory.Honesty] > actor.Status.Parameters[(int)RG.Define.Action.StatusCategory.Unique])
            {
                return Constant.CharacterType.Honesty;
            }
            else if (actor.Status.Parameters[(int)RG.Define.Action.StatusCategory.Naughty] > actor.Status.Parameters[(int)RG.Define.Action.StatusCategory.Unique])
            {
                return Constant.CharacterType.Naughty;
            }
            else
            {
                return Constant.CharacterType.Unique;
            }
        }

        internal static int GetCurrentAnimationType(int MapID, byte sex, int animID)
        {
            int result = GetCurrentAnimationTypeCommon(sex, animID);

            if (result == Constant.AnimType.NotDetermined)
            {
                switch (MapID)
                {
                    case Constant.MapType.OfficeWorkplace:
                        return GetCurrentAnimationTypeOfficeWorkplace(sex, animID);
                    case Constant.MapType.OfficePrivate:
                        return GetCurrentAnimationTypeOfficePrivate(sex, animID);
                    case Constant.MapType.ClinicsWorkplace:
                        return GetCurrentAnimationTypeClinicsWorkplace(sex, animID);
                    case Constant.MapType.ClinicsPrivate:
                        return GetCurrentAnimationTypeClinicsPrivate(sex, animID);
                    case Constant.MapType.SeminarWorkplace:
                        return GetCurrentAnimationTypeSeminarWorkplace(sex, animID);
                    case Constant.MapType.SeminarPrivate:
                        return GetCurrentAnimationTypeSeminarPrivate(sex, animID);
                    case Constant.MapType.LivehouseWorkplace:
                        return GetCurrentAnimationTypeLivehouseWorkplace(sex, animID);
                    case Constant.MapType.LivehousePrivate:
                        return GetCurrentAnimationTypeLivehousePrivate(sex, animID);

                    case Constant.MapType.CasinoWorkplace:
                        return GetCurrentAnimationTypeCasinoWorkplace(sex, animID);
                    case Constant.MapType.CasinoPrivate:
                        return GetCurrentAnimationTypeCasinoPrivate(sex, animID);
                    case Constant.MapType.HomeWorkplace:
                        return GetCurrentAnimationTypeHomeWorkplace(sex, animID);
                    case Constant.MapType.HomePrivate:
                        return GetCurrentAnimationTypeHomePrivate(sex, animID);
                }
            }


            return result;
        }

        internal static int GetCurrentAnimationTypeOfficeWorkplace(byte sex, int animID)
        {
            if (sex == 1)
            {
                switch (animID)
                {
                    case 10031:
                        return Constant.AnimType.SittingWithTable;
                    case 10032:
                        return Constant.AnimType.Standing;
                }
            }
            else
            {
                switch (animID)
                {
                    case 10007:
                        return Constant.AnimType.SittingWithTable;
                    case 10008:
                        return Constant.AnimType.Standing;
                }
            }
            return Constant.AnimType.Standing;
        }

        internal static int GetCurrentAnimationTypeOfficePrivate(byte sex, int animID)
        {
            if (sex == 1)
            {
                switch (animID)
                {
                    case 10045:
                    case 10046:
                        return Constant.AnimType.Standing;
                }
            }
            else
            {
                switch (animID)
                {
                    case 10014:
                        return Constant.AnimType.Standing;
                }
            }
            return Constant.AnimType.Standing;
        }

        internal static int GetCurrentAnimationTypeClinicsWorkplace(byte sex, int animID)
        {
            if (sex == 1)
            {
                switch (animID)
                {
                    case 10017:
                        return Constant.AnimType.SittingWithTable;
                    case 10018:
                        return Constant.AnimType.Standing;
                    case 10019:
                        return Constant.AnimType.Sitting;
                }
            }
            else
            {
                switch (animID)
                {
                    case 10020:
                        return Constant.AnimType.SittingWithTable;
                    case 10018:
                        return Constant.AnimType.Standing;
                    case 10019:
                        return Constant.AnimType.Sitting;
                    case 10021:
                        return Constant.AnimType.Bed;
                }
            }
            return Constant.AnimType.Standing;
        }

        internal static int GetCurrentAnimationTypeClinicsPrivate(byte sex, int animID)
        {
            //no special handling
            return Constant.AnimType.Standing;
        }

        internal static int GetCurrentAnimationTypeSeminarWorkplace(byte sex, int animID)
        {
            if (sex == 1)
            {
                switch (animID)
                {
                    case 10005:
                        return Constant.AnimType.SittingWithTable;
                    case 10018:
                        return Constant.AnimType.Standing;
                }
            }
            else
            {
                switch (animID)
                {
                    case 10010:
                        return Constant.AnimType.SittingWithTable;
                    case 10013:
                        return Constant.AnimType.Standing;
                }
            }
            return Constant.AnimType.Standing;
        }

        internal static int GetCurrentAnimationTypeSeminarPrivate(byte sex, int animID)
        {
            //no special handling
            return Constant.AnimType.Standing;
        }

        internal static int GetCurrentAnimationTypeLivehouseWorkplace(byte sex, int animID)
        {
            if (sex == 1)
            {
                switch (animID)
                {
                    case 10003:
                        return Constant.AnimType.SittingWithTable;
                    case 10020:
                        return Constant.AnimType.Standing;
                }
            }
            else
            {
                switch (animID)
                {
                    case 10006:
                        return Constant.AnimType.SittingWithTable;
                    case 10007:
                    case 10019:
                        return Constant.AnimType.Standing;
                    case 10010:
                        return Constant.AnimType.Sitting;
                }
            }
            return Constant.AnimType.Standing;
        }

        internal static int GetCurrentAnimationTypeLivehousePrivate(byte sex, int animID)
        {
            if (sex == 1)
            {
                switch (animID)
                {
                    case 10052:
                    case 10053:
                        return Constant.AnimType.Standing;
                }
            }
            else
            {
                switch (animID)
                {
                    case 10012:
                        return Constant.AnimType.Standing;
                }
            }
            return Constant.AnimType.Standing;
        }

        internal static int GetCurrentAnimationTypeCasinoWorkplace(byte sex, int animID)
        {
            if (sex == 1)
            {
                switch (animID)
                {
                    case 10002:
                        return Constant.AnimType.SittingWithTable;
                    case 10003:
                        return Constant.AnimType.Standing;
                }
            }
            else
            {
                switch (animID)
                {
                    case 10007:
                        return Constant.AnimType.SittingWithTable;
                    case 10008:
                        return Constant.AnimType.Standing;
                }
            }
            return Constant.AnimType.Standing;
        }

        internal static int GetCurrentAnimationTypeCasinoPrivate(byte sex, int animID)
        {
            //no special handling
            return Constant.AnimType.Standing;
        }

        internal static int GetCurrentAnimationTypeHomeWorkplace(byte sex, int animID)
        {

            if (sex == 1)
            {
                //No special action for female ?

            }
            else
            {
                switch (animID)
                {
                    case 10016:
                        return Constant.AnimType.SittingWithTable;
                    case 10017:
                        return Constant.AnimType.Standing;
                }
            }
            return Constant.AnimType.Standing;
        }

        internal static int GetCurrentAnimationTypeHomePrivate(byte sex, int animID)
        {
            //no special handling
            return Constant.AnimType.Standing;
        }



        internal static int GetCurrentAnimationTypeCommon(byte sex, int animID)
        {
            if (sex == 1)
            {
                switch (animID)
                {
                    case 136:
                        return Constant.AnimType.SittingWithTable;
                    case 135:
                        return Constant.AnimType.Sitting;
                    case 134:
                        return Constant.AnimType.Standing;
                }
            }
            else
            {
                switch (animID)
                {
                    case 113:
                        return Constant.AnimType.SittingWithTable;
                    case 112:
                        return Constant.AnimType.Sitting;
                    case 111:
                        return Constant.AnimType.Standing;
                }
            }
            return Constant.AnimType.NotDetermined;
        }

        internal static GameObject InstantiateFromBundle(AssetBundle bundle, string assetName)
        {
            var asset = bundle.LoadAsset(assetName, Il2CppType.From(typeof(GameObject)));
            var obj = Object.Instantiate(asset);
            foreach (var rootGameObject in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (rootGameObject.GetInstanceID() == obj.GetInstanceID())
                {
                    rootGameObject.name = assetName;
                    return rootGameObject;
                }
            }
            throw new FileLoadException("Could not instantiate asset " + assetName);
        }

        internal static string GetHeightKindAnimationPrefix(int heightKind)
        {
            switch (heightKind)
            {
                case 2: return Constant.HeightKind.Large;
                case 1: return Constant.HeightKind.Medium;
                default: return Constant.HeightKind.Small;
            }
        }

        internal static HVoice.VoicePaceType GetVoicePaceTypeByAnimInfo(HScene.AnimationListInfo animInfo)
        {
            //General case: char[0]: height kind, char[1]: underscore, char[2]: the pace type
            switch (animInfo.NameAnimation[2])
            {
                case Constant.HAnimationClipNameKind.Normal: 
                    return HVoice.VoicePaceType.Normal;
                case Constant.HAnimationClipNameKind.Rapid: 
                    return HVoice.VoicePaceType.Rapid;
                case Constant.HAnimationClipNameKind.NearOrgasm: 
                    return HVoice.VoicePaceType.NearOrgasm;
                default: 
                    return HVoice.VoicePaceType.Idle;
            }
        }

        internal static string GetAssetBundlePath(string assetBundle)
        {
            //TODO: is there any place that storing the abdata path directly?
            return Path.Combine(Application.dataPath.Replace("RoomGirl_Data", "abdata"), assetBundle);
        }

        internal static bool CheckHasEverSex(Actor actor1, Actor actor2)
        {
            foreach (var dict in actor1.Status.RelationshipParameter)
            {
                if (dict.ContainsKey(actor2.CharaFileName))
                {
                    return dict[actor2.CharaFileName].HasEverSex;
                }
            }

            return false;
        }
    }
}
