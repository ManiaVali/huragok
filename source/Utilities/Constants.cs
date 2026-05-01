
namespace Huragok.Utilities {
    public static class GlobalConstants {
        /// <summary>
        /// Float scalar representing the amount of meters in a world unit. 
        /// </summary>
        public const float WU_TO_METERS = 3.048f;
        /// <summary>
        /// Float scalar representing the amount of world units in a meter. 
        /// </summary>
        public const float METERS_TO_WU = 1 / WU_TO_METERS;
        /// <summary>
        /// Float scalar representing the amount of JMS units in a world unit.
        /// </summary>
        public const float WU_TO_JMS = 100;
        /// <summary>
        /// Float scalar representing the amount of world units in a JMS unit.
        /// </summary>
        public const float JMS_TO_WU = 1 / WU_TO_JMS;


        internal const string PROGRAM_NAME = "Huragok";

        /// <summary>
        /// Name of the game whose engine we are expecting.
        /// </summary>
#if USING_BLAM_H2AMP
        public const string ENGINE_PRETTY_NAME = "Halo 2: Anniversary Multiplayer";
#elif USING_BLAM_H3
        public const string ENGINE_PRETTY_NAME = "Halo 3";
#elif USING_BLAM_H3ODST
        public const string ENGINE_PRETTY_NAME = "Halo 3: ODST";
#elif USING_BLAM_HR
        public const string ENGINE_PRETTY_NAME = "Halo: Reach";
#elif USING_BLAM_H4
        public const string ENGINE_PRETTY_NAME = "Halo 4";
#endif
    }
}