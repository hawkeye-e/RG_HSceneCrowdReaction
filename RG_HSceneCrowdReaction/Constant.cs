

namespace HSceneCrowdReaction
{
    internal class Constant
    {
        internal const int TotalPersonalityCount = 16;
        internal const string AnimatorParamHeight = "height";
        internal const string AnimatorParamSpeed = "speed";
        internal const string AnimatorParamMotion = "motion";
        internal const string AnimatorParamBreast = "breast";
        internal const int ThreesomeHPointIndex = 8;
        internal const int ClothesPartCount = 8;

        internal const string CharaChoiceTogglePrefixMale = "m";
        internal const string CharaChoiceTogglePrefixFemale = "f";

        internal const int characterCountInHGroup = 5;
        internal const int femaleCountInHGroup = 2;
        internal const int maleCountInHGroup = 3;

        internal class AnimType
        {
            internal const int NotDetermined = -1;
            internal const int Standing = 0;
            internal const int Sitting = 1;
            internal const int SittingWithTable = 2;
            internal const int Bed = 3;
        }

        internal class AnimFacingType
        {
            internal const int NoChange = 0;
            internal const int StaringHeadUp = 1;
            internal const int StaringHeadDown = 2;
            internal const int WholeBodyFacing = 3;
        }

        internal class MapType
        {
            internal const int OfficeWorkplace = 0;
            internal const int OfficePrivate = 1;
            internal const int ClinicsWorkplace = 2;
            internal const int ClinicsPrivate = 3;
            internal const int SeminarWorkplace = 4;
            internal const int SeminarPrivate = 5;
            internal const int LivehouseWorkplace = 6;
            internal const int LivehousePrivate = 7;
            internal const int CasinoWorkplace = 8;
            internal const int CasinoPrivate = 9;
            internal const int HomeWorkplace = 10;
            internal const int HomePrivate = 11;

            internal const int Cafe = 12;
            internal const int Restaurant = 13;
            internal const int Park = 14;
            internal const int Hotel = 15;

            internal const int AlleyWorkplace = 20;
            internal const int AlleyPrivate = 21;
            internal const int HotSpringWorkplace = 22;
            internal const int HotSpringPrivate = 23;
            internal const int SMRoom = 24;
        }

        internal enum CharacterType
        {
            Honesty,
            Naughty,
            Unique,
        }

        internal class PossibleReactionType
        {
            internal const int Awkward = 0;
            internal const int Angry = 1;
            internal const int Happy = 2;
            internal const int Worry = 3;
            internal const int Libido = 4;
            internal const int Cry = 5;
            internal const int Excited = 6;
            internal const int Hurray = 7;
        }

        internal enum HCharacterType
        {
            NA = -1,
            Female1 = 0,
            Female2 = 1,
            Male1 = 2,
            Male2 = 3,
            Male3 = 4,
        }

        internal static class ClothesPart
        {
            internal const int Top = 0;
            internal const int Bottom = 1;
            internal const int InnerTop = 2;
            internal const int InnerBottom = 3;
            internal const int Gloves = 4;
            internal const int PantyHose = 5;
            internal const int Socks = 6;
            internal const int Shoes = 7;
        }

        internal static class GeneralClothesStates
        {
            internal const int Full = 0;
            internal const int Half = 1;
            internal const int Nude = 2;
        }

        internal static class TwoStateClothesStates
        {
            internal const int Full = 0;
            internal const int Nude = 1;
        }
    }
}
