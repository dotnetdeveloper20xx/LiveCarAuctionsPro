using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Vehicles;

/// <summary>
/// Vehicle condition report created during inspection.
/// </summary>
public sealed class ConditionReport : ValueObject
{
    public ConditionGrade OverallGrade { get; }
    public ConditionGrade ExteriorGrade { get; }
    public ConditionGrade InteriorGrade { get; }
    public ConditionGrade MechanicalGrade { get; }
    public string? Notes { get; }
    public UserId InspectorId { get; }
    public DateTime InspectedAt { get; }
    public bool HasStructuralDamage { get; }
    public bool HasFrameDamage { get; }
    public bool HasFloodDamage { get; }
    public bool AirbagDeployed { get; }
    public bool OdometerDiscrepancy { get; }

    private ConditionReport(
        ConditionGrade overallGrade,
        ConditionGrade exteriorGrade,
        ConditionGrade interiorGrade,
        ConditionGrade mechanicalGrade,
        UserId inspectorId,
        DateTime inspectedAt,
        string? notes,
        bool hasStructuralDamage,
        bool hasFrameDamage,
        bool hasFloodDamage,
        bool airbagDeployed,
        bool odometerDiscrepancy)
    {
        OverallGrade = overallGrade;
        ExteriorGrade = exteriorGrade;
        InteriorGrade = interiorGrade;
        MechanicalGrade = mechanicalGrade;
        InspectorId = inspectorId;
        InspectedAt = inspectedAt;
        Notes = notes;
        HasStructuralDamage = hasStructuralDamage;
        HasFrameDamage = hasFrameDamage;
        HasFloodDamage = hasFloodDamage;
        AirbagDeployed = airbagDeployed;
        OdometerDiscrepancy = odometerDiscrepancy;
    }

    public static ConditionReport Create(
        ConditionGrade overallGrade,
        ConditionGrade exteriorGrade,
        ConditionGrade interiorGrade,
        ConditionGrade mechanicalGrade,
        UserId inspectorId,
        DateTime inspectedAt,
        string? notes = null,
        bool hasStructuralDamage = false,
        bool hasFrameDamage = false,
        bool hasFloodDamage = false,
        bool airbagDeployed = false,
        bool odometerDiscrepancy = false)
    {
        return new ConditionReport(
            overallGrade,
            exteriorGrade,
            interiorGrade,
            mechanicalGrade,
            inspectorId,
            inspectedAt,
            notes,
            hasStructuralDamage,
            hasFrameDamage,
            hasFloodDamage,
            airbagDeployed,
            odometerDiscrepancy);
    }

    public bool HasMajorIssues() =>
        HasStructuralDamage || HasFrameDamage || HasFloodDamage || AirbagDeployed;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return OverallGrade;
        yield return ExteriorGrade;
        yield return InteriorGrade;
        yield return MechanicalGrade;
        yield return InspectorId;
        yield return InspectedAt;
        yield return HasStructuralDamage;
        yield return HasFrameDamage;
        yield return HasFloodDamage;
        yield return AirbagDeployed;
        yield return OdometerDiscrepancy;
    }
}
