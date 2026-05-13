namespace Huragok.Data.RuntimeFormats;

/// <summary>
/// An <see cref="Enum"/> representing the possible coordinate unit sizes we are working with.
/// </summary>
internal enum CoordinateUnit {
    /// <summary>
    /// The Blam native unit; exactly 3.048 meters (10 feet).
    /// </summary>
    Blam,
    /// <summary>
    /// The Jointed Model Skeleton unit; exactly 100 Blam Units, or approximately 0.031 meters.
    /// </summary>
    JMS,
    /// <summary>
    /// Metric units (base unit is meters); approximately .32 world units.
    /// </summary>
    Metric
}