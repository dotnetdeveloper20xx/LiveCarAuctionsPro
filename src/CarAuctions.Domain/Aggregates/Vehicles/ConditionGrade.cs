namespace CarAuctions.Domain.Aggregates.Vehicles;

/// <summary>
/// Standard auction condition grades (1-5 scale).
/// </summary>
public enum ConditionGrade
{
    /// <summary>
    /// Exceptional condition, like new.
    /// </summary>
    Grade5 = 5,

    /// <summary>
    /// Very good condition, minimal wear.
    /// </summary>
    Grade4 = 4,

    /// <summary>
    /// Good condition, average wear for age/mileage.
    /// </summary>
    Grade3 = 3,

    /// <summary>
    /// Fair condition, noticeable wear or minor damage.
    /// </summary>
    Grade2 = 2,

    /// <summary>
    /// Poor condition, significant wear or damage.
    /// </summary>
    Grade1 = 1,

    /// <summary>
    /// Salvage/parts only.
    /// </summary>
    Grade0 = 0
}
