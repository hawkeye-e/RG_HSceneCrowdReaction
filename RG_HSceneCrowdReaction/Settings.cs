using UnityEngine;
using System.Collections.Generic;

namespace HSceneCrowdReaction
{
    internal class Settings
    {
        internal const int DefaultLibidoThreshold = 50;

        internal const int HActionMinMilliSecond = 10000;
        internal const int HActionRandomMilliSecond = 10000;

        internal const int HVoiceMaxDistance = 200;

        internal const int HChangePositionThreshold = 3;                //if the pair has accumulated value exceed this number, there is a chance the pair change sex position

        internal const int ExtendCharaChoiceDropDownViewportHeight = 200;

        internal static readonly string[] ValidPlayableHClipType =  
            { 
                InfoList.HAnimation.HAnimationClipType.WLoop,
                InfoList.HAnimation.HAnimationClipType.SLoop,
                InfoList.HAnimation.HAnimationClipType.OLoop,
            };

        internal const string HVoiceAssetBundleFormat = "sound/data/pcm/c{0:D2}/h/00.unity3d";
        internal const string HVoiceAssetNameFormat = "rg_hk_{0:D2}_{1:D3}";
        internal const string HMotionIKAssetBundleFileName = "{0:D2}.unity3d";
        
        
        internal class HMotionIKAssetFormatThreesome
        {
            internal const string MMF_Female = "rg3_m2f_f_{0:D2}";
            internal const string MMF_Male1 = "rg3_m2f_m1_{0:D2}";
            internal const string MMF_Male2 = "rg3_m2f_m2_{0:D2}";
            internal const string FFM_Male = "rg3_mf2_m_{0}_{1:D2}";
            internal const string FFM_Female1 = "rg3_mf2_f1_{0}";
            internal const string FFM_Female2 = "rg3_mf2_f2_{0}";
        }



        internal const string FemaleHitHeadPathFormat = "BodyTop/p_cf_head_0{0}_hit";
        //internal const string FemaleHitBodyPathFormat = "BodyTop/p_cf_body_00_hit_low";
        internal const string FemaleHitBodyTopPath = "BodyTop";
        internal const string FemaleHitBodyPathSearchStart = "p_cf_body_";
        internal const string FemaleHitBodyPathSearchEnd = "_hit_low";
        internal const string MaleGlansPath = "cf_J_Kosi01/cf_J_Kosi02/cm_J_dan_s/cm_J_dan_top/cm_J_dan100_00/cm_J_dan101_00/cm_J_dan109_00";
        internal const string HItemPath = "hsceneHDRP(Clone)/HSceneComponent/Place/HItem";

    }
}
