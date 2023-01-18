using BepInEx.Configuration;
using BepInEx.IL2CPP;

namespace HSceneCrowdReaction
{
    internal class Config
    {
        internal static bool Enabled { get { return _enabled.Value; } }
        private static ConfigEntry<bool> _enabled;

        internal static HAnimMatchingType HAnimMatchType { get { return _matchingType.Value; } }
        private static ConfigEntry<HAnimMatchingType> _matchingType;

        internal static bool SexExpPrerequisite { get { return _sexExpPrerequisite.Value; } }
        private static ConfigEntry<bool> _sexExpPrerequisite;

        internal static void Init(BasePlugin plugin)
        {
            _enabled = plugin.Config.Bind("General", "Enable this plugin", true, "If false, this plugin will do nothing (requires game restart)");

            _matchingType = plugin.Config.Bind("H Reaction", "H reaction grouping method", HAnimMatchingType.Default, 
                "<b>Default</b> - The characters need to be in pair before H scene started\n"
                + "<b>AutoMatch</b> - The system will automatically do the matching. Pairing before H scene is NOT required\n"
                + "<b>Off</b> - No H reaction"
                );
            _sexExpPrerequisite = plugin.Config.Bind("H Reaction", "Sex experience requirement", true, 
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
