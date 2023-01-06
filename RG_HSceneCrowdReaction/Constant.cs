﻿

namespace HSceneCrowdReaction
{
    internal class Constant
    {
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

        internal class HeightKind
        {
            internal const string Small = "S_";
            internal const string Medium = "M_";
            internal const string Large = "L_";
        }

        internal class HAnimSpeed
        {
            internal const float Max = 2;
            internal const float Min = 1;
        }

        internal class HAnimClipKeyword
        {
            internal const string Loop = "Loop";
        }

        internal enum HCharacterType
        {
            Female1,
            Female2,
            Male1,
            Male2
        }

        internal class PersonalityType
        {
            public const int OfficeLadyTypeA = 0;
            public const int OfficeLadyTypeB = 1;
            public const int NurseTypeA = 2;
            public const int NurseTypeB = 3;

            public const int StudentTypeA = 4;
            public const int StudentTypeB = 5;
            public const int IdolTypeA = 6;
            public const int IdolTypeB = 7;
            public const int DealerTypeA = 8;
            public const int DealerTypeB = 9;
            public const int NeetTypeA = 10;
            public const int NeetTypeB = 11;
        }

        internal class HAnimationClipNameKind
        {
            //TODO: need double check
            internal const char Normal = 'W';
            internal const char Rapid = 'S';
            internal const char NearOrgasm = 'O';
        }

    }
}
