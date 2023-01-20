using BepInEx.Configuration;
using BepInEx.IL2CPP;

namespace HSceneCrowdReaction
{
    internal class Config
    {
        private const string GENERAL = "General";
        private const string H_REACTION = "H Reaction";
        private const string STANDARD_REACTION = "Standard Reaction";


        internal static bool Enabled { get { return _enabled.Value; } }
        private static ConfigEntry<bool> _enabled;

        internal static HAnimMatchingType HAnimMatchType { get { return _matchingType.Value; } }
        private static ConfigEntry<HAnimMatchingType> _matchingType;

        internal static bool SexExpPrerequisite { get { return _sexExpPrerequisite.Value; } }
        private static ConfigEntry<bool> _sexExpPrerequisite;

        internal static bool EnableSeriousAwkward { get { return _enableSeriousAwkward.Value; } }
        private static ConfigEntry<bool> _enableSeriousAwkward;

        internal static bool EnableSeriousAngry { get { return _enableSeriousAngry.Value; } }
        private static ConfigEntry<bool> _enableSeriousAngry;

        internal static bool EnablePlayfulHurray { get { return _enablePlayfulHurray.Value; } }
        private static ConfigEntry<bool> _enablePlayfulHurray;

        internal static bool EnablePlayfulExcited { get { return _enablePlayfulExcited.Value; } }
        private static ConfigEntry<bool> _enablePlayfulExcited;

        internal static bool EnableUniqueWorry { get { return _enableUniqueWorry.Value; } }
        private static ConfigEntry<bool> _enableUniqueWorry;

        internal static bool EnableUniqueHappy { get { return _enableUniqueHappy.Value; } }
        private static ConfigEntry<bool> _enableUniqueHappy;

        internal static bool EnableStandardMasturbation { get { return _enableStandardMasturbation.Value; } }
        private static ConfigEntry<bool> _enableStandardMasturbation;

        internal static int MasturbationLibidoThreshold { get { return _masturbationLibidoThreshold.Value; } }
        private static ConfigEntry<int> _masturbationLibidoThreshold;

        internal static bool EnableStandardCry { get { return _enableStandardCry.Value; } }
        private static ConfigEntry<bool> _enableStandardCry;

        internal static bool StandardCryPrerequisite { get { return _standardCryPrerequisite.Value; } }
        private static ConfigEntry<bool> _standardCryPrerequisite;

        internal static void Init(BasePlugin plugin)
        {
            _enabled = plugin.Config.Bind(GENERAL, "Enable this plugin", true, "If false, this plugin will do nothing (requires game restart)");

            _enableSeriousAwkward = plugin.Config.Bind(STANDARD_REACTION, "Enable Awkward reaction", true, 
                "If false, Awkward reaction will not be included in the possible reaction list for Serious type characters");
            _enableSeriousAngry = plugin.Config.Bind(STANDARD_REACTION, "Enable Angry reaction", true,
                "If false, Angry reaction will not be included in the possible reaction list for Serious type characters");
            _enablePlayfulHurray = plugin.Config.Bind(STANDARD_REACTION, "Enable Hurray reaction", true,
                "If false, Hurray reaction will not be included in the possible reaction list for Playful type characters");
            _enablePlayfulExcited = plugin.Config.Bind(STANDARD_REACTION, "Enable Excited reaction", true,
                "If false, Excited reaction will not be included in the possible reaction list for Playful type characters");
            _enableUniqueWorry = plugin.Config.Bind(STANDARD_REACTION, "Enable Worry reaction", true,
                "If false, Worry reaction will not be included in the possible reaction list for Unique type characters");
            _enableUniqueHappy = plugin.Config.Bind(STANDARD_REACTION, "Enable Happy reaction", true,
                "If false, Happy reaction will not be included in the possible reaction list for Unique type characters");
            _enableStandardMasturbation = plugin.Config.Bind(STANDARD_REACTION, "Enable Masturbation reaction", true,
                "If false, Masturbation reaction will not be included in the possible reaction list");
            _masturbationLibidoThreshold = plugin.Config.Bind(STANDARD_REACTION, "Masturbation Libido Threshold", Settings.DefaultLibidoThreshold,
                "Range: 0 to 100. The lower the value it is, the easier for masturbation reaction to be included");
            _enableStandardCry = plugin.Config.Bind(STANDARD_REACTION, "Enable Cry reaction", true,
                "If false, Cry reaction will not be included in the possible reaction list");
            _standardCryPrerequisite = plugin.Config.Bind(STANDARD_REACTION, "Cry reaction requirement", true,
                "If true, Cry reaction will only be included for characters who witness his/her partner involved in H-scene");

            _matchingType = plugin.Config.Bind(H_REACTION, "H reaction grouping method", HAnimMatchingType.Default, 
                "<b>Default</b> - The characters need to be in pair before H scene started\n"
                + "<b>AutoMatch</b> - The system will automatically do the matching. Pairing before H scene is NOT required\n"
                + "<b>Off</b> - No H reaction"
                );
            _sexExpPrerequisite = plugin.Config.Bind(H_REACTION, "Sex experience requirement", true, 
                "If true, the characters in the pair/group need to have sex with each other to trigger H reaction\n"
                + "For MMF, the male character need to have sex with both female beforehand\n"
                + "For FFM, the female character need to have sex with both male beforehand"
                );
        }

        internal enum HAnimMatchingType
        {
            Default,
            AutoMatch,
            Off
        }
    }
}
