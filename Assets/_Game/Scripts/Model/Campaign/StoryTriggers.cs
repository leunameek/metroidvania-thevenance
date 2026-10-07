// Where each piece of the story is said during play. A trigger key names a moment ("flag:<id>"
// when a campaign flag is first set, "room:MI02" when a room is entered, "plaza:<moment>"), and
// plays the given cues of one sequence once per save. Complete = the sequence ends there and its
// flags are applied (StoryScript.Complete).
public sealed class StoryTrigger
{
    public readonly string Key, Sequence;
    public readonly string[] Cues;
    public readonly bool Complete;
    public StoryTrigger(string key, string sequence, bool complete, params string[] cues)
    {
        Key = key; Sequence = sequence; Complete = complete; Cues = cues;
    }
    public string SeenFlag => "seen_trigger_" + Key;
}

public static class StoryTriggers
{
    public const string PlazaArrival = "plaza:arrival", PlazaTraining = "plaza:training";
    public static string PlazaStation(int index) => "plaza:station" + index;
    public static string Flag(string id) => "flag:" + id;
    public static string Room(int index) => "room:MI0" + (index + 1);
    public static string Zone(int index) => "zone:MS0" + (index + 1);
    public static string Duel(string encounter, string moment) => "duel:" + encounter + ":" + moment;

    public static readonly StoryTrigger[] All =
    {
        // Plaza Núñez (H04-H05)
        new StoryTrigger(PlazaArrival, "H04", true),
        new StoryTrigger(PlazaStation(0), "H05", false, "vasija"),
        new StoryTrigger(PlazaStation(1), "H05", false, "disco"),
        new StoryTrigger(PlazaStation(2), "H05", false, "figura"),
        new StoryTrigger(PlazaTraining, "H05", false, "duelo_inicio", "duelo_defensa"),
        new StoryTrigger(Flag(CampaignFlags.MiUnlocked), "H05", true, "portal"),
        // Mundo inferior (H06-H09)
        new StoryTrigger(Room(1), "H06", false, "custodio"),
        new StoryTrigger(Flag(CampaignFlags.MiSeed), "H06", true, "semilla"),
        new StoryTrigger(Room(2), "H07", false, "caiman"),
        new StoryTrigger(Room(3), "H07", false, "murcielago"),
        new StoryTrigger(Flag(CampaignFlags.MiBracelets3), "H07", true, "tercer_par"),
        new StoryTrigger(Room(4), "H08", false, "pendulos"),
        new StoryTrigger(Room(5), "H08", false, "derrumbe"),
        new StoryTrigger(Flag(CampaignFlags.MiShortcut06), "H08", true, "atajo"),
        new StoryTrigger(Flag(CampaignFlags.MiHorn), "H09", false, "cuerno"),
        new StoryTrigger(Flag(CampaignFlags.MiHornGate), "H09", true, "soporte"),
        new StoryTrigger(Duel("E07", "intro"), "H10", false, "intro"),
        new StoryTrigger(Duel("E07", "jaguar"), "H10", false, "jaguar"),
        new StoryTrigger(Duel("E07", "won"), "H10", true, "liberado"),
        // Mundo superior (H13-H15)
        new StoryTrigger(Flag(CampaignFlags.MsRune1), "H13", false, "hebilla"),
        new StoryTrigger(Flag(CampaignFlags.MsRune2), "H13", false, "pared"),
        new StoryTrigger(Duel("E09", "intro"), "H13", true, "condor"),
        new StoryTrigger(Flag(CampaignFlags.MsWings), "H14", false, "alas"),
        new StoryTrigger(Duel("E10", "intro"), "H14", true, "aguila"),
        new StoryTrigger(Flag(CampaignFlags.MsKey), "H15", false, "placa", "eco"),
        new StoryTrigger(Flag(CampaignFlags.MsLockOpen), "H15", true, "llave"),
        new StoryTrigger(Duel("E08", "intro"), "H16", false, "intro"),
        new StoryTrigger(Duel("E08", "won"), "H16", true, "liberado", "eco"),
    };

    public static StoryTrigger Find(string key)
    {
        foreach (var t in All) if (t.Key == key) return t;
        return null;
    }
}
