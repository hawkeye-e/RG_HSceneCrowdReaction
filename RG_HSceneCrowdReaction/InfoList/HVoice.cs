using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace HSceneCrowdReaction.InfoList
{
    internal class HVoice
    {
        private static ManualLogSource Log = HSceneCrowdReactionPlugin.Log;

        //Key: (personality, HPositionType, VoicePaceType)
        internal static Dictionary<(int, HAnimation.HPositionType, HAnimation.HAnimationClipType), List<HVoiceAssetData>> HVoiceDictionary;

        internal static void Init()
        {
            if(HVoiceDictionary == null)
            {
                HVoiceDictionary = new Dictionary<(int, HAnimation.HPositionType, HAnimation.HAnimationClipType), List<HVoiceAssetData>>();

                ReadCSVFile();
                /*
                HVoiceDictionary.Add((Constant.PersonalityType.OfficeLadyTypeA, HAnimation.HPositionType.AibuKiss, VoicePaceType.Idle), 
                    new List<HVoiceAssetData> { 
                        new HVoiceAssetData(Constant.PersonalityType.OfficeLadyTypeA, "rg_hk_00_015") 
                    });
                HVoiceDictionary.Add((Constant.PersonalityType.OfficeLadyTypeA, HAnimation.HPositionType.AibuKiss, VoicePaceType.Normal),
                    new List<HVoiceAssetData> {
                        new HVoiceAssetData(Constant.PersonalityType.OfficeLadyTypeA, "rg_hk_00_040"),
                        new HVoiceAssetData(Constant.PersonalityType.OfficeLadyTypeA, "rg_hk_00_041"),
                        new HVoiceAssetData(Constant.PersonalityType.OfficeLadyTypeA, "rg_hk_00_042"),
                        new HVoiceAssetData(Constant.PersonalityType.OfficeLadyTypeA, "rg_hk_00_043"),  
                        new HVoiceAssetData(Constant.PersonalityType.OfficeLadyTypeA, "rg_hk_00_044"),
                        new HVoiceAssetData(Constant.PersonalityType.OfficeLadyTypeA, "rg_hk_00_045")
                    });
                HVoiceDictionary.Add((Constant.PersonalityType.OfficeLadyTypeA, HAnimation.HPositionType.AibuKiss, VoicePaceType.Rapid),
                    new List<HVoiceAssetData> {
                        new HVoiceAssetData(Constant.PersonalityType.OfficeLadyTypeA, "rg_hk_00_076"),
                        new HVoiceAssetData(Constant.PersonalityType.OfficeLadyTypeA, "rg_hk_00_077"),
                        new HVoiceAssetData(Constant.PersonalityType.OfficeLadyTypeA, "rg_hk_00_078"),

                        new HVoiceAssetData(Constant.PersonalityType.OfficeLadyTypeA, "rg_hk_00_079"),
                        new HVoiceAssetData(Constant.PersonalityType.OfficeLadyTypeA, "rg_hk_00_080"),
                        new HVoiceAssetData(Constant.PersonalityType.OfficeLadyTypeA, "rg_hk_00_081")
                    });

                HVoiceDictionary.Add((Constant.PersonalityType.OfficeLadyTypeA, HAnimation.HPositionType.AibuKiss, VoicePaceType.NearOrgasm),
                    new List<HVoiceAssetData> {
                        new HVoiceAssetData(Constant.PersonalityType.OfficeLadyTypeA, "rg_hk_00_107"),
                        new HVoiceAssetData(Constant.PersonalityType.OfficeLadyTypeA, "rg_hk_00_108"),
                        
                    });
                */
            }
        }



        private static void ReadCSVFile()
        {
            string[] csvHVoice = Resources.ListResources.HVoice.Split('\n');
            for (int i = 1; i < csvHVoice.Length; i++)
            {
                if(csvHVoice[i] == "") continue;      //in case empty row
                string[] rowData = csvHVoice[i].Split(',');
                
#pragma warning disable CS8605
#pragma warning disable CS8602
                int personalityType = (int)typeof(Constant.PersonalityType).GetField(rowData[0]).GetValue(null);
                HAnimation.HPositionType hPositionType = (HAnimation.HPositionType)Enum.Parse(typeof(HAnimation.HPositionType), rowData[1]);
                HAnimation.HAnimationClipType clipType = (HAnimation.HAnimationClipType)Enum.Parse(typeof(HAnimation.HAnimationClipType), rowData[2]);
#pragma warning restore CS8602
#pragma warning restore CS8605
                string asset = rowData[3];
                List<HVoiceAssetData> dataList;
                if (HVoiceDictionary.ContainsKey((personalityType, hPositionType, clipType)))
                {
                    dataList = HVoiceDictionary[(personalityType, hPositionType, clipType)];
                }
                else
                {
                    dataList = new List<HVoiceAssetData>();
                    HVoiceDictionary.Add((personalityType, hPositionType, clipType), dataList);
                }

                dataList.Add(new HVoiceAssetData(personalityType, asset));
            }
        }

        internal class HVoiceAssetData
        {
            internal string assetBundle = "";
            internal string asset = "";

            public HVoiceAssetData(int personalityType, string asset)
            {
                //this.assetBundle = assetBundle;
                this.assetBundle = string.Format(Settings.HVoiceAssetBundleFormat, personalityType);
                this.asset = asset;
            }


        }

        internal enum VoicePaceType
        {
            Idle,
            Normal,
            Rapid,
            NearOrgasm
        }


    }
}
