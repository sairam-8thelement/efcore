namespace EfCore36741Repro;

/// <summary>
/// The entity used throughout the repro. Deliberately minimal: an Id plus a
/// single timestamp column, mirroring the "last active" write-on-every-request
/// pattern described in the issue.
/// </summary>
public class User
{
    public int Id { get; set; }

    public DateTime LastActive { get; set; }
}
