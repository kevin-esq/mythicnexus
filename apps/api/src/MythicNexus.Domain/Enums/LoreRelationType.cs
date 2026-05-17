namespace MythicNexus.Domain.Enums;

/// <summary>
/// Typed directed edge between lore entries. Stored in PostgreSQL as the enum member name.
/// </summary>
public enum LoreRelationType
{
    References = 0,
    RelatedTo = 1,
    Contradicts = 2,
    PartOf = 3,
    LocatedIn = 4,
    EnemyOf = 5,
    AllyOf = 6,
}
