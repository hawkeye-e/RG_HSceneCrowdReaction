using RG.Scripts;

namespace HSceneCrowdReaction.InfoList
{
    internal class CustomAnimation
    {
        internal class Common
        {
            internal static CustomAnimationData NoChange
            {
                get { return new CustomAnimationData(null, null, null); }
            }

            internal static CustomAnimationData StandingCrying //泣きながら立ってる
            {
                get { return new CustomAnimationData("animator/action/00_00/male/03.unity3d", "m_03_00", Manager.Game.ActionCache._dicAnimationParameters[0][3].Workplace[10019],
                    false, false, true, true); }
            }

            internal static CustomAnimationData Excited //motion: 立って喜ぶ
            {
                get { return new CustomAnimationData("animator/action/00_00/male/03.unity3d", "m_03_00", Manager.Game.ActionCache._dicAnimationParameters[0][3].Workplace[10002],
                    true, false, true, true); }
            }

            internal static CustomAnimationData NotImpressed //motion: ソファーを見下ろして怒る
            {
                get { return new CustomAnimationData("animator/action/00_00/male/05.unity3d", "m_05_00", Manager.Game.ActionCache._dicAnimationParameters[0][5].Workplace[10002],
                    true, false, true, true); }
            }

            internal static CustomAnimationData Hurray //motion: 立ってオタ芸　フワフワ
            {
                get { return new CustomAnimationData("animator/action/00_00/male/03.unity3d", "m_03_00", Manager.Game.ActionCache._dicAnimationParameters[0][3].Workplace[10017],
                    true, false, true, true); }
            }


            internal static CustomAnimationData Happy
            {
                get { return new CustomAnimationData("animator/adv/00_00.unity3d", "adv_m_00_00", Manager.Game.ActionCache._dicADVAnimationParameters[0][10003],
                    true, false, true, true); }
            }

            internal static CustomAnimationData Sad
            {
                get { return new CustomAnimationData("animator/adv/00_00.unity3d", "adv_f_00_00", Manager.Game.ActionCache._dicADVAnimationParameters[1][10004],
                    true, true, true, true); }
            }

            internal static CustomAnimationData Angry
            {
                get { return new CustomAnimationData("animator/adv/00_00.unity3d", "adv_f_00_00", Manager.Game.ActionCache._dicADVAnimationParameters[1][10005],
                    true, false, true, true); }
            }

            
        }

        internal class Female
        {
            
            internal static CustomAnimationData CrouchMasturbation
            {
                get{ return new CustomAnimationData(null, null, Manager.Game.ActionCache._dicCommonAnimationParameters[1][1026],
                    true, true, false, false); }
            }

            internal static CustomAnimationData SeatMasturbation
            {
                get { return new CustomAnimationData(null, null, Manager.Game.ActionCache._dicCommonAnimationParameters[1][1031],
                    true, true, false, false); }
            }

            internal static CustomAnimationData TableMasturbation
            {
                get { return new CustomAnimationData(null, null, Manager.Game.ActionCache._dicCommonAnimationParameters[1][1036],
                    true, true, false, false); }
            }

            internal static CustomAnimationData StandingMasturbation
            {
                get { return new CustomAnimationData(null, null, Manager.Game.ActionCache._dicCommonAnimationParameters[1][1056],
                    true, true, false, false); }
            }

            internal static CustomAnimationData TableSeatMasturbation
            {
                get { return new CustomAnimationData(null, null, Manager.Game.ActionCache._dicCommonAnimationParameters[1][1064],
                    true, true, false, false); }
            }

            internal static CustomAnimationData StandingWorry
            {
                get { return new CustomAnimationData(null, null, Manager.Game.ActionCache._dicCommonAnimationParameters[1][44],
                    true, false, true, false); }
            }

            internal static CustomAnimationData SittingWorry
            {
                get { return new CustomAnimationData(null, null, Manager.Game.ActionCache._dicCommonAnimationParameters[1][46],
                    true, false, false, false); }
            }
        }

        internal class Male
        {
            internal static CustomAnimationData StandingWorry
            {
                get { return new CustomAnimationData(null, null, Manager.Game.ActionCache._dicCommonAnimationParameters[0][14],
                    true, false, true, false); }
            }

            internal static CustomAnimationData SittingWorry
            {
                get { return new CustomAnimationData(null, null, Manager.Game.ActionCache._dicCommonAnimationParameters[0][16],
                    true, false, false, false); }
            }
        }
        


        internal class CustomAnimationData
        {
            public string assetBundle;
            public string assetName;
            public AnimationParameter animationParameter;
            public bool isStaring;
            public bool isHeadDown;
            public bool requireBodyMove;
            public bool requireStanding;
            internal CustomAnimationData(string assetBundle, string assetName, AnimationParameter animationParameter, 
                bool isStaring = false, bool isHeadDown = false, bool requireBodyMove = false, bool requireStanding = false)
            {
                this.assetBundle = assetBundle;
                this.assetName = assetName;
                this.animationParameter = animationParameter;
                this.isStaring = isStaring;
                this.isHeadDown = isHeadDown;
                this.requireBodyMove = requireBodyMove;
                this.requireStanding = requireStanding;
            }
        }
    }
}
