using KarmicVessel.Tier1;
using ThunderRoad;

namespace KarmicVessel.Other
{
    public class ModOptions
    {
        public delegate void AbilityChanged();
        public static AbilityChanged AbilitySwitched;
        
        public static SpellHands LeftHandAbilities = SpellHands.ChakraRods;
        public static SpellHands RightHandAbilities = SpellHands.ChakraRods;
        
        
        
        [ModOptionCategory("Chakra Rods", 1)]
        [ModOption("Projectile launch location")]
        public static KarmaBase.RodLaunchLocation ProjectileLaunchesFromEyes = KarmaBase.RodLaunchLocation.Hand;
        
        [ModOptionCategory("Chakra Rods", 1)]
        [ModOptionFloatValues(0.1f, 2, 0.1f)]
        [ModOption("Large Rod Length")]
        public static float LargeRodLength = 1f;
        
        [ModOptionCategory("Chakra Rods", 1)]
        [ModOptionFloatValues(0.1f, 3, 0.1f)]
        [ModOption("Large Rod Width")]
        public static float LargeRodWidth = 1f;
        
        [ModOptionCategory("Chakra Rods", 1)]
        [ModOptionFloatValues(0.1f, 5, 0.1f)]
        [ModOption("Growth Speed")]
        public static float GrowthSpeed = 1f;
        
        [ModOptionCategory("Chakra Rods", 1)]
        [ModOptionFloatValues(0.1f, 15, 0.1f)]
        [ModOption("Laquer Bloom Cooldown Time")]
        public static float BloomCooldownTime = 10f;

        

        
        
        
        
        [ModOptionCategory("Sukunahikuna", 2)]
        [ModOptionFloatValues(0.1f, 5, 0.1f)]
        [ModOption("Scale Speed")]
        public static float GrowShrinkSpeed = 1f;
        
        [ModOptionCategory("Sukunahikuna", 2)]
        [ModOptionFloatValues(0.1f, 15f, 0.1f)]
        [ModOption("Kokugan Pulse Cooldown Time")]
        public static float PulseCooldwonSpeed = 10f;

        [ModOptionCategory("Sukunahikuna", 2)] [ModOption("Allow Shrinking with Telekinesis")]
        public static bool ShrinkUseTelekinesis = false;

        [ModOptionCategory("Daikokuten", 3)]
        [ModOption("Projectile launch location")]
        public static void DaikokutenDimensionCrossSave(bool value)
        {
            // not implememnted for now...
        }
        
        
        [ModOptionCategory("Other", 4)]
        [ModOption("Show Karma Marks")]
        public static bool ShowKarmaMarks = true;
        
        
        

        [ModOptionCategory("DEV", 4)]
        [ModOption("Enable Debug Logs")]
        public static bool AllowLogs = false;
        [ModOptionCategory("DEV", 4)]
        [ModOption("Enable Debug Rays")]
        public static bool ShowDebugRays = false;
        
        
        public enum SpellHands {
            ChakraRods,
            Shrink,
            Daikokuten
        }
    }
}