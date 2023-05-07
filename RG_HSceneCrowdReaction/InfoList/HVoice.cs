using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace HSceneCrowdReaction.InfoList
{
    internal class HVoice
    {
        private static ManualLogSource Log = HSceneCrowdReactionPlugin.Log;

        //Key: (personality, HPositionType, VoicePaceType)
        internal static Dictionary<(int, HVoiceType, string), List<HVoiceAssetData>> HVoiceDictionary;

        internal static void Init()
        {
            if(HVoiceDictionary == null)
            {
                HVoiceDictionary = new Dictionary<(int, HVoiceType, string), List<HVoiceAssetData>>();                
                ReadCSVFile();
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
                HVoiceType voiceType = (HVoiceType)Enum.Parse(typeof(HVoiceType), rowData[0]);
#pragma warning restore CS8602
#pragma warning restore CS8605
                string clipType = rowData[1];
                int assetNumber = int.Parse(rowData[2]);

                for(int personalityType = 0; personalityType < Constant.TotalPersonalityCount; personalityType++)
                {
                    string assetName = string.Format(Settings.HVoiceAssetNameFormat, personalityType, assetNumber);

                    List<HVoiceAssetData> dataList;
                    if (HVoiceDictionary.ContainsKey((personalityType, voiceType, clipType)))
                    {
                        dataList = HVoiceDictionary[(personalityType, voiceType, clipType)];
                    }
                    else
                    {
                        dataList = new List<HVoiceAssetData>();
                        HVoiceDictionary.Add((personalityType, voiceType, clipType), dataList);
                    }

                    dataList.Add(new HVoiceAssetData(personalityType, assetName));
                }

                
            }
        }

        internal class HVoiceAssetData
        {
            internal string assetBundle = "";
            internal string asset = "";

            public HVoiceAssetData(int personalityType, string asset)
            {
                string abFormat;
                if (personalityType < 12)
                    abFormat = Settings.HVoiceAssetBundleFormat;
                else
                    abFormat = Settings.HVoiceAssetBundleExpansionFormat;

                this.assetBundle = string.Format(abFormat, personalityType);
                this.asset = asset;
            }


        }

        internal enum HVoiceType
        {
            BlowJob,
            BlowJobIntercourse,
            BlowJobExp,
            ForceBlowJob,
            HandAndLick,
            Kiss,
            Leading,
            LickAndBlow,
            LickLeading,
            LickJob,
            NA,
            OnPhone,
            Pain,
            Receiving,
            Service,
            SixNine,
            SixNineLeading,
            Tentacles

        }
    }
}
