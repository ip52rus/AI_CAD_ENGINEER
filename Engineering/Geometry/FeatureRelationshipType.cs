namespace AI_CAD_ENGINEER.Engineering.Geometry;

public enum FeatureRelationshipType
{
    Unknown = 0,

    ParentChild,

    BelongsToFace,

    AdjacentTo,

    Intersects,

    ConcentricWith,

    ParallelTo,

    PerpendicularTo,

    SymmetricWith,

    PartOfPattern,

    SameSourceFeature,

    SameDiameter,

    SameAxis,

    References,

    DimensionedBy
}