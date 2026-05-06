
namespace Huragok.Utilities {
    internal static class GlobalConstants {
        /// <summary>
        /// Float scalar representing the amount of meters in a world unit. 
        /// </summary>
        internal const float WU_TO_METERS = 3.048f;
        /// <summary>
        /// Float scalar representing the amount of world units in a meter. 
        /// </summary>
        internal const float METERS_TO_WU = 1 / WU_TO_METERS;
        /// <summary>
        /// Float scalar representing the amount of JMS units in a world unit.
        /// </summary>
        internal const float WU_TO_JMS = 100;
        /// <summary>
        /// Float scalar representing the amount of world units in a JMS unit.
        /// </summary>
        internal const float JMS_TO_WU = 1 / WU_TO_JMS;


        internal const string PROGRAM_NAME = "Huragok";

        /// <summary>
        /// Name of the game whose engine we are expecting.
        /// </summary>
#if USING_BLAM_H2AMP
        internal const string ENGINE_PRETTY_NAME = "Halo 2: Anniversary Multiplayer";
#elif USING_BLAM_H3
        internal const string ENGINE_PRETTY_NAME = "Halo 3";
#elif USING_BLAM_H3ODST
        internal const string ENGINE_PRETTY_NAME = "Halo 3: ODST";
#elif USING_BLAM_HR
        internal const string ENGINE_PRETTY_NAME = "Halo: Reach";
#elif USING_BLAM_H4
        internal const string ENGINE_PRETTY_NAME = "Halo 4";
#endif
    }
}