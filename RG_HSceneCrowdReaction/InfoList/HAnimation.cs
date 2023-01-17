using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSceneCrowdReaction.InfoList
{
    internal class HAnimation
    {

        internal const float DefaultMaleEyeOpenMax = 1;
        internal const float DefaultFemaleEyeOpenMax = .9f;
        internal const int DefaultMaleEyePtn = 0;
        internal const int DefaultFemaleEyePtn = 0;
        internal const string DefaultMaleEyebrowPtn = "5;5;6";
        internal const string DefaultFemaleEyebrowPtn = "5;6;6";

        internal static int[] ValidHPointTypeMF = { 0, 1, 2, 3, 4, 5, 6, 7, 9, 10 };
        internal static int[] ValidHPointTypeFF = { 0, 1, 2, 3, 5 };
        internal static int[] ValidHPointTypeFFM = { 8 };
        internal static int[] ValidHPointTypeMMF = { 8 };

        //H animation excluded due to technical issue (eg. unable to control the mouth)
        internal static List<(int, int)> ExcludeList;
        internal static Dictionary<(int, int), ExtraHAnimationData> ExtraHAnimationDataDictionary;

        public HAnimation()
        {

        }


        public static void Init()
        {
            ExcludeList = new List<(int, int)>();
            InitExtraHAnimationDataDictionary();
        }

        private static void InitExtraHAnimationDataDictionary()
        {
            if (ExtraHAnimationDataDictionary == null)
            {
                ExtraHAnimationDataDictionary = new Dictionary<(int, int), ExtraHAnimationData>();
                ReadCSVFile();
            }
        }

        private static void ReadCSVFile()
        {
            string[] csvHAnimation = Resources.ListResources.HAnimation.Split('\n');
            for (int i = 1; i < csvHAnimation.Length; i++)
            {
                if (csvHAnimation[i] == "") continue;      //in case empty row
                string[] rowData = csvHAnimation[i].Split(',');

                int categoryID = int.Parse(rowData[2]);
                int animID = int.Parse(rowData[3]);

                if (rowData[27] == "1")
                {
                    //Include in game
                    Vector3 offsetVector = Vector3.zeroVector;
                    if (rowData[26] != "")
                    {
                        string[] strOffset = rowData[26].Split(';');
                        offsetVector.x = float.Parse(strOffset[0]);
                        offsetVector.y = float.Parse(strOffset[1]);
                        offsetVector.z = float.Parse(strOffset[2]);
                    }

#pragma warning disable CS8605
#pragma warning disable CS8602
                    SituationType situationType = (SituationType)Enum.Parse(typeof(SituationType), rowData[1]);
                    HVoice.HVoiceType female1VoiceType = (HVoice.HVoiceType)Enum.Parse(typeof(HVoice.HVoiceType), rowData[4]);
                    HVoice.HVoiceType female2VoiceType = (HVoice.HVoiceType)Enum.Parse(typeof(HVoice.HVoiceType), rowData[5]);
                    
#pragma warning restore CS8602
#pragma warning restore CS8605

                    ExtraHAnimationData dataInfo;
                    if (ExtraHAnimationDataDictionary.ContainsKey((categoryID, animID)))
                    {
                        dataInfo = ExtraHAnimationDataDictionary[(categoryID, animID)];
                    }
                    else
                    {
                        dataInfo = new ExtraHAnimationData();
                        ExtraHAnimationDataDictionary.Add((categoryID, animID), dataInfo);
                    }

                    dataInfo.situationType = situationType;

                    dataInfo.female1VoiceType = female1VoiceType;
                    dataInfo.female2VoiceType = female2VoiceType;
                    
                    dataInfo.isMale1Inverse = rowData[6] == "1";
                    dataInfo.isMale2Inverse = rowData[7] == "1";
                    dataInfo.isFemale1Inverse = rowData[8] == "1";
                    dataInfo.isFemale2Inverse = rowData[9] == "1";

                    dataInfo.mouthTypeMale1 = ParseMouthType(rowData[10]);
                    dataInfo.mouthTypeMale2 = ParseMouthType(rowData[11]);
                    dataInfo.mouthTypeFemale1 = ParseMouthType(rowData[12]);
                    dataInfo.mouthTypeFemale2 = ParseMouthType(rowData[13]);

                    dataInfo.eyeOpenMaxMale1 = ParseEyeOpenMax(rowData[14], true);
                    dataInfo.eyeOpenMaxMale2 = ParseEyeOpenMax(rowData[15], true);
                    dataInfo.eyeOpenMaxFemale1 = ParseEyeOpenMax(rowData[16], false);
                    dataInfo.eyeOpenMaxFemale2 = ParseEyeOpenMax(rowData[17], false);

                    dataInfo.eyePtnMale1 = ParseEyePtn(rowData[18], true);
                    dataInfo.eyePtnMale2 = ParseEyePtn(rowData[19], true);
                    dataInfo.eyePtnFemale1 = ParseEyePtn(rowData[20], false);
                    dataInfo.eyePtnFemale2 = ParseEyePtn(rowData[21], false);

                    dataInfo.eyebrowPtnMale1 = ParseEyebrowPtn(rowData[22], true);
                    dataInfo.eyebrowPtnMale2 = ParseEyebrowPtn(rowData[23], true);
                    dataInfo.eyebrowPtnFemale1 = ParseEyebrowPtn(rowData[24], false);
                    dataInfo.eyebrowPtnFemale2 = ParseEyebrowPtn(rowData[25], false);

                    dataInfo.offsetVector = offsetVector;
                }
                else
                {
                    //Excluded in game
                    ExcludeList.Add((categoryID, animID));
                }

            }
        }

        private static Dictionary<string, MouthType> ParseMouthType(string rowData)
        {
            Dictionary<string, MouthType> result = new Dictionary<string, MouthType>();
            result.Add(HAnimationClipType.WLoop, MouthType.Common);
            result.Add(HAnimationClipType.SLoop, MouthType.Common);
            result.Add(HAnimationClipType.OLoop, MouthType.Common);

            if (rowData != "")
            {
                var rowDataSplit = rowData.Split(';');
                result[HAnimationClipType.WLoop] = GetMouthType(rowDataSplit[0]);
                result[HAnimationClipType.SLoop] = GetMouthType(rowDataSplit[1]);
                result[HAnimationClipType.OLoop] = GetMouthType(rowDataSplit[2]);
            }

            return result;
        }

        private static MouthType GetMouthType(string input)
        {
            if (input == CSVMouthType.Lick)
                return MouthType.Lick;
            else if (input == CSVMouthType.BlowJob)
                return MouthType.BlowJob;
            else if (input == CSVMouthType.Kiss)
                return MouthType.Kiss;
            return MouthType.Common;
        }

        private static Dictionary<string, float> ParseEyeOpenMax(string rowData, bool isMale)
        {
            Dictionary<string, float> result = new Dictionary<string, float>();
            float defaultValue = isMale ? DefaultMaleEyeOpenMax : DefaultFemaleEyeOpenMax;
            result.Add(HAnimationClipType.WLoop, defaultValue);
            result.Add(HAnimationClipType.SLoop, defaultValue);
            result.Add(HAnimationClipType.OLoop, defaultValue);

            if (rowData != "")
            {
                var rowDataSplit = rowData.Split(';');
                result[HAnimationClipType.WLoop] = float.Parse(rowDataSplit[0]);
                result[HAnimationClipType.SLoop] = float.Parse(rowDataSplit[1]);
                result[HAnimationClipType.OLoop] = float.Parse(rowDataSplit[2]);
            }

            return result;
        }

        private static Dictionary<string, int> ParseEyePtn(string rowData, bool isMale)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            int defaultValue = isMale ? DefaultMaleEyePtn : DefaultFemaleEyePtn;
            result.Add(HAnimationClipType.WLoop, defaultValue);
            result.Add(HAnimationClipType.SLoop, defaultValue);
            result.Add(HAnimationClipType.OLoop, defaultValue);

            if (rowData != "")
            {
                var rowDataSplit = rowData.Split(';');
                result[HAnimationClipType.WLoop] = int.Parse(rowDataSplit[0]);
                result[HAnimationClipType.SLoop] = int.Parse(rowDataSplit[1]);
                result[HAnimationClipType.OLoop] = int.Parse(rowDataSplit[2]);
            }

            return result;
        }

        private static Dictionary<string, int> ParseEyebrowPtn(string rowData, bool isMale)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            string dataToParse = rowData;
            if (dataToParse == "")
                dataToParse = isMale ? DefaultMaleEyebrowPtn : DefaultFemaleEyebrowPtn;

            var rowDataSplit = dataToParse.Split(';');
            result.Add(HAnimationClipType.WLoop, int.Parse(rowDataSplit[0]));
            result.Add(HAnimationClipType.SLoop, int.Parse(rowDataSplit[1]));
            result.Add(HAnimationClipType.OLoop, int.Parse(rowDataSplit[2]));

            return result;
        }

        public static bool IsInExcludeList(int category, int id)
        {
            if (ExcludeList.Contains((category, id)))
                return true;
            return false;
        }

        public static string GetClipTypeByName(string clipName)
        {
            if (clipName.Contains(HAnimationClipType.SLoop))
                return HAnimationClipType.SLoop;
            else if (clipName.Contains(HAnimationClipType.WLoop))
                return HAnimationClipType.WLoop;
            else if (clipName.Contains(HAnimationClipType.OLoop))
                return HAnimationClipType.OLoop;
            else
                return HAnimationClipType.Unknown;
        }


        internal enum SituationType
        {
            F,
            MF,
            FF,
            MMF,
            FFM
        }

        internal class HAnimationClipType
        {
            public const string Idle = "Idle";
            public const string WLoop = "WLoop";
            public const string SLoop = "SLoop";
            public const string OLoop = "OLoop";
            public const string Unknown = "";
        }

        internal enum MouthType
        {
            BlowJob = 13,
            Kiss = 12,
            Lick = 10,
            Pain = 25,
            Joy = 24,
            Sad = 3,
            Common = 0,
        }

        internal class CSVMouthType
        {
            public const string BlowJob = "B";
            public const string Kiss = "K";
            public const string Lick = "L";
            public const string Pain = "P";
            public const string Joy = "J";
            public const string Sad = "S";
            public const string Common = "C";
        }

        internal class ExtraHAnimationData
        {
            internal SituationType situationType;

            internal bool isMale1Inverse = false;
            internal bool isMale2Inverse = false;
            internal bool isFemale1Inverse = false;
            internal bool isFemale2Inverse = false;

            internal Dictionary<string, MouthType> mouthTypeMale1;
            internal Dictionary<string, MouthType> mouthTypeMale2;
            internal Dictionary<string, MouthType> mouthTypeFemale1;
            internal Dictionary<string, MouthType> mouthTypeFemale2;

            internal Dictionary<string, float> eyeOpenMaxMale1;
            internal Dictionary<string, float> eyeOpenMaxMale2;
            internal Dictionary<string, float> eyeOpenMaxFemale1;
            internal Dictionary<string, float> eyeOpenMaxFemale2;

            internal Dictionary<string, int> eyePtnMale1;
            internal Dictionary<string, int> eyePtnMale2;
            internal Dictionary<string, int> eyePtnFemale1;
            internal Dictionary<string, int> eyePtnFemale2;

            internal Dictionary<string, int> eyebrowPtnMale1;
            internal Dictionary<string, int> eyebrowPtnMale2;
            internal Dictionary<string, int> eyebrowPtnFemale1;
            internal Dictionary<string, int> eyebrowPtnFemale2;

            internal HVoice.HVoiceType female1VoiceType;
            internal HVoice.HVoiceType female2VoiceType;

            internal Vector3 offsetVector = Vector3.zero;

        }

        internal class ActorHAnimData
        {
            internal SituationType situationType;
            internal Constant.HCharacterType characterType;
            internal HScene.AnimationListInfo animationListInfo;

            internal string clipType;
            internal float height;
            internal float breast;
            internal float speed;
            internal float motion;

            internal int changePositionFactor;
        }
    }
}
