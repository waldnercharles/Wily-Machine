namespace Spaghetti;

public enum Faction
{
    Neutral,
    Player,
    Enemy,
    Misc
}

public static class FactionExt
{
    public static bool IsVulnerableTo(this Faction faction, Faction other)
    {
        return (faction, other) switch
        {
            (Faction.Neutral, Faction.Neutral) => false,
            (Faction.Neutral, Faction.Player) => false,
            (Faction.Neutral, Faction.Enemy) => false,
            (Faction.Neutral, Faction.Misc) => false,

            (Faction.Player, Faction.Neutral) => false,
            (Faction.Player, Faction.Player) => false,
            (Faction.Player, Faction.Enemy) => true,
            (Faction.Player, Faction.Misc) => true,

            (Faction.Enemy, Faction.Neutral) => false,
            (Faction.Enemy, Faction.Player) => true,
            (Faction.Enemy, Faction.Enemy) => false,
            (Faction.Enemy, Faction.Misc) => true,

            (Faction.Misc, Faction.Neutral) => false,
            (Faction.Misc, Faction.Player) => false,
            (Faction.Misc, Faction.Enemy) => false,
            (Faction.Misc, Faction.Misc) => false,
            _ => false
        };
    }
}
