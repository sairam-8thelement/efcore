namespace EfCore36741Repro;

/// <summary>
/// Fixed seed data shared by every test and every provider. Keeping it a
/// single, deterministic set of rows (rather than random/generated data) is
/// what lets AC#6's parameterized variant assert "only row <see cref="TargetId"/>
/// changed" with a plain equality check against the other rows' original
/// <see cref="User.LastActive"/> values.
/// </summary>
public static class Seed
{
    /// <summary>2020-01-01T00:00:00Z.</summary>
    public static readonly DateTime Base = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>Number of seeded rows. Ids are 1..RowCount.</summary>
    public const int RowCount = 3;

    /// <summary>The single seeded row every parameterized/Where-filtered test variant targets.</summary>
    public const int TargetId = 2;

    /// <summary>
    /// A fresh, unattached set of seed rows: Id 1..RowCount, LastActive =
    /// Base.AddDays(Id - 1). Returns new instances on every call -- EF entity
    /// instances must not be shared live across more than one DbContext/change
    /// tracker at a time, and a fixture's NewSeededContext[Async] may be called
    /// more than once.
    /// </summary>
    public static IReadOnlyList<User> FreshRows() => BuildRows();

    /// <summary>Seeds <paramref name="context"/> with exactly <see cref="RowCount"/> rows and saves (async).</summary>
    public static async Task SeedAsync(ReproContext context)
    {
        context.Users.AddRange(FreshRows());
        await context.SaveChangesAsync();
    }

    /// <summary>Seeds <paramref name="context"/> with exactly <see cref="RowCount"/> rows and saves (sync).</summary>
    public static void SeedSync(ReproContext context)
    {
        context.Users.AddRange(FreshRows());
        context.SaveChanges();
    }

    private static IReadOnlyList<User> BuildRows()
    {
        var rows = new User[RowCount];
        for (var id = 1; id <= RowCount; id++)
        {
            rows[id - 1] = new User { Id = id, LastActive = Base.AddDays(id - 1) };
        }

        return rows;
    }
}
