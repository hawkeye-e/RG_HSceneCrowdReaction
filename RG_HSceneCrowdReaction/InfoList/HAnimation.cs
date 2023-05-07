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

        internal const int CategoryCaress = 0;
        internal const int CategoryService = 1;
        internal const int CategoryInsert = 2;
        internal const int CategorySpecial = 3;
        internal const int CategoryLesbian = 4;
        internal const int CategoryFFM = 5;
        internal const int CategoryMMF = 6;

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

                if (rowData[29] != "1")
                {
                    //Excluded in H reaction group, still need to load the info to the dictionary for the icon category info
                    ExcludeList.Add((categoryID, animID));
                }
                
                Vector3 offsetVector = Vector3.zeroVector;
                if (rowData[28] != "")
                {
                    string[] strOffset = rowData[28].Split(';');
                    offsetVector.x = float.Parse(strOffset[0]);
                    offsetVector.y = float.Parse(strOffset[1]);
                    offsetVector.z = float.Parse(strOffset[2]);
                }


                SituationType situationType = (SituationType)Enum.Parse(typeof(SituationType), rowData[1]);
                HVoice.HVoiceType female1VoiceType = (HVoice.HVoiceType)Enum.Parse(typeof(HVoice.HVoiceType), rowData[4]);
                HVoice.HVoiceType female2VoiceType = (HVoice.HVoiceType)Enum.Parse(typeof(HVoice.HVoiceType), rowData[5]);

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
                dataInfo.isMale3Inverse = rowData[31] == "1";

                dataInfo.isItemInverse = rowData[27] == "1";

                dataInfo.mouthTypeMale1 = ParseMouthType(rowData[10]);
                dataInfo.mouthTypeMale2 = ParseMouthType(rowData[11]);
                dataInfo.mouthTypeFemale1 = ParseMouthType(rowData[12]);
                dataInfo.mouthTypeFemale2 = ParseMouthType(rowData[13]);
                dataInfo.mouthTypeMale3 = ParseMouthType(rowData[32]);

                dataInfo.eyeOpenMaxMale1 = ParseEyeOpenMax(rowData[14], true);
                dataInfo.eyeOpenMaxMale2 = ParseEyeOpenMax(rowData[15], true);
                dataInfo.eyeOpenMaxFemale1 = ParseEyeOpenMax(rowData[16], false);
                dataInfo.eyeOpenMaxFemale2 = ParseEyeOpenMax(rowData[17], false);
                dataInfo.eyeOpenMaxMale3 = ParseEyeOpenMax(rowData[33], true);

                dataInfo.eyePtnMale1 = ParseEyePtn(rowData[18], true);
                dataInfo.eyePtnMale2 = ParseEyePtn(rowData[19], true);
                dataInfo.eyePtnFemale1 = ParseEyePtn(rowData[20], false);
                dataInfo.eyePtnFemale2 = ParseEyePtn(rowData[21], false);
                dataInfo.eyePtnMale3 = ParseEyePtn(rowData[34], true);

                dataInfo.eyebrowPtnMale1 = ParseEyebrowPtn(rowData[22], true);
                dataInfo.eyebrowPtnMale2 = ParseEyebrowPtn(rowData[23], true);
                dataInfo.eyebrowPtnFemale1 = ParseEyebrowPtn(rowData[24], false);
                dataInfo.eyebrowPtnFemale2 = ParseEyebrowPtn(rowData[25], false);
                dataInfo.eyebrowPtnMale3 = ParseEyebrowPtn(rowData[35], true);

                if (rowData[26] != null)
                    dataInfo.mmfFemale1Target = ParseMMFTargetType(rowData[26]);
                if (rowData[36] != null)
                    dataInfo.orgyFemale2Target = ParseMMFTargetType(rowData[36]);

                dataInfo.offsetVector = offsetVector;

                dataInfo.iconCategory = rowData[30];
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

        private static Dictionary<string, Constant.HCharacterType> ParseMMFTargetType(string rowData)
        {
            Dictionary<string, Constant.HCharacterType> result = new Dictionary<string, Constant.HCharacterType>();
            result.Add(HAnimationClipType.WLoop, Constant.HCharacterType.NA);
            result.Add(HAnimationClipType.SLoop, Constant.HCharacterType.NA);
            result.Add(HAnimationClipType.OLoop, Constant.HCharacterType.NA);

            if (rowData != "")
            {
                var rowDataSplit = rowData.Split(';');
                result[HAnimationClipType.WLoop] = (Constant.HCharacterType)Enum.Parse(typeof(Constant.HCharacterType), rowDataSplit[0]);
                result[HAnimationClipType.SLoop] = (Constant.HCharacterType)Enum.Parse(typeof(Constant.HCharacterType), rowDataSplit[1]);
                result[HAnimationClipType.OLoop] = (Constant.HCharacterType)Enum.Parse(typeof(Constant.HCharacterType), rowDataSplit[2]);
            }

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

        internal static List<int> GetPlaceKindBySiuationType(SituationType type)
        {
            List<int> result = new List<int>();
            int[] places = null;
            switch (type)
            {
                case SituationType.FF:
                    places = ValidHPointTypeFF;
                    break;
                case SituationType.MF:
                    places = ValidHPointTypeMF;
                    break;
                case SituationType.MMF:
                    places = ValidHPointTypeMMF;
                    break;
                case SituationType.FFM:
                    places = ValidHPointTypeFFM;
                    break;
            }

            if (places != null)
                for (int i = 0; i < places.Length; i++)
                    result.Add(places[i]);
            return result;
        }

        internal static string GetIconObjectNameByCategory(string iconCategory)
        {
            if (iconCategory == IconCategory.Caress) return IconName.Caress;
            else if (iconCategory == IconCategory.Service) return IconName.Service;
            else if (iconCategory == IconCategory.Insert) return IconName.Insert;
            else if (iconCategory == IconCategory.FemaleLeading) return IconName.FemaleLeading;
            else if (iconCategory == IconCategory.lesbian) return IconName.lesbian;
            else if (iconCategory == IconCategory.FFM) return IconName.FFM;
            else if (iconCategory == IconCategory.MMF) return IconName.MMF;
            else if (iconCategory == IconCategory.Special) return IconName.Special;
            else if (iconCategory == IconCategory.MMMF) return IconName.MMMF;
            else if (iconCategory == IconCategory.TwoPair) return IconName.TwoPair;
            else
                return "";
        }

        internal static int GetIconValueByCategory(string iconCategory)
        {
            if (iconCategory == IconCategory.Caress) return IconCategoryValue.Caress;
            else if (iconCategory == IconCategory.Service) return IconCategoryValue.Service;
            else if (iconCategory == IconCategory.Insert) return IconCategoryValue.Insert;
            else if (iconCategory == IconCategory.FemaleLeading) return IconCategoryValue.FemaleLeading;
            else if (iconCategory == IconCategory.lesbian) return IconCategoryValue.lesbian;
            else if (iconCategory == IconCategory.FFM) return IconCategoryValue.FFM;
            else if (iconCategory == IconCategory.MMF) return IconCategoryValue.MMF;
            else if (iconCategory == IconCategory.Special) return IconCategoryValue.Special;
            else if (iconCategory == IconCategory.MMMF) return IconCategoryValue.MMMF;
            else if (iconCategory == IconCategory.TwoPair) return IconCategoryValue.TwoPair;
            else
                return -1;
        }

        internal static SituationType GetSituationType(HScene hScene)
        {
            int femaleCount = 0;
            int maleCount = 0;
            for(int i=0; i<hScene._chaFemales.Count; i++)
                if (hScene._chaFemales[i] != null)
                    femaleCount++;
            for (int i = 0; i < hScene._chaMales.Count; i++)
                if (hScene._chaMales[i] != null)
                    maleCount++;

            if (femaleCount == 1 && maleCount == 0)
                return SituationType.F;
            else if (femaleCount == 1 && maleCount == 1)
                return SituationType.MF;
            else if (femaleCount == 1 && maleCount == 2)
                return SituationType.MMF;
            else if (femaleCount == 2 && maleCount == 0)
                return SituationType.FF;
            else if (femaleCount == 2 && maleCount == 1)
                return SituationType.FFM;
            else if (femaleCount == 2 && maleCount == 2)
                return SituationType.MMFF;
            else if (femaleCount == 1 && maleCount == 3)
                return SituationType.MMMF;
            else
                return SituationType.Unknown;
        }


        internal enum SituationType
        {
            F,
            MF,
            FF,
            MMF,
            FFM,
            MMMF,
            MMFF,
            Unknown
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

        internal class IconCategory
        {
            public const string Caress = "Caress";
            public const string Service = "Service";
            public const string Insert = "Insert";
            public const string FemaleLeading = "FemaleLeading";
            public const string lesbian = "lesbian";
            public const string FFM = "FFM";
            public const string MMF = "MMF";
            public const string Special = "Special";

            public const string MMMF = "MMMF";
            public const string TwoPair = "TwoPair";
        }

        internal class IconName
        {
            public const string Caress = "aibu";
            public const string Service = "houshi";
            public const string Insert = "sonyu";
            public const string FemaleLeading = "LeavItToYou";
            public const string lesbian = "multiLes";
            public const string FFM = "multiF2";
            public const string MMF = "multiM2";
            public const string Special = "tokushu";

            public const string MMMF = "multiM3";
            public const string TwoPair = "multiW";
        }

        internal class IconCategoryValue
        {
            public const int Caress = 0;
            public const int Service = 1;
            public const int Insert = 2;
            public const int FemaleLeading = 7;
            public const int lesbian = 4;
            public const int FFM = 5;
            public const int MMF = 6;
            public const int Special = 3;

            public const int MMMF = 8;
            public const int TwoPair = 9;
        }

        internal class ExtraHAnimationData
        {
            internal SituationType situationType;

            internal bool isMale1Inverse = false;
            internal bool isMale2Inverse = false;
            internal bool isMale3Inverse = false;
            internal bool isFemale1Inverse = false;
            internal bool isFemale2Inverse = false;

            internal bool isItemInverse = false;

            internal Dictionary<string, MouthType> mouthTypeMale1;
            internal Dictionary<string, MouthType> mouthTypeMale2;
            internal Dictionary<string, MouthType> mouthTypeMale3;
            internal Dictionary<string, MouthType> mouthTypeFemale1;
            internal Dictionary<string, MouthType> mouthTypeFemale2;

            internal Dictionary<string, float> eyeOpenMaxMale1;
            internal Dictionary<string, float> eyeOpenMaxMale2;
            internal Dictionary<string, float> eyeOpenMaxMale3;
            internal Dictionary<string, float> eyeOpenMaxFemale1;
            internal Dictionary<string, float> eyeOpenMaxFemale2;

            internal Dictionary<string, int> eyePtnMale1;
            internal Dictionary<string, int> eyePtnMale2;
            internal Dictionary<string, int> eyePtnMale3;
            internal Dictionary<string, int> eyePtnFemale1;
            internal Dictionary<string, int> eyePtnFemale2;

            internal Dictionary<string, int> eyebrowPtnMale1;
            internal Dictionary<string, int> eyebrowPtnMale2;
            internal Dictionary<string, int> eyebrowPtnMale3;
            internal Dictionary<string, int> eyebrowPtnFemale1;
            internal Dictionary<string, int> eyebrowPtnFemale2;

            internal Dictionary<string, Constant.HCharacterType> mmfFemale1Target;
            internal Dictionary<string, Constant.HCharacterType> orgyFemale2Target;

            internal HVoice.HVoiceType female1VoiceType;
            internal HVoice.HVoiceType female2VoiceType;

            internal Vector3 offsetVector = Vector3.zero;

            internal string iconCategory;
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
