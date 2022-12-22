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
    }
}