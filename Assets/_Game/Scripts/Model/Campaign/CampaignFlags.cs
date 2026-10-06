// Persistent story flags of the campaign (guion "Estados persistentes", H01-H21). Each id is set
// once and never cleared; the chapter, objective and portal gates are derived from them.
public static class CampaignFlags
{
    // Prólogo (C01-C04)
    public const string StaffOwned = "staff_owned", VisionSeen = "vision_seen";
    public const string PoporoOwned = "poporo_owned", MapOwned = "map_owned", HubActive = "hub_active";
    // Plaza tutorial (H05)
    public const string LessonsComplete = "lessons_complete", TrainingComplete = "training_complete", MiUnlocked = "mi_unlocked";
    // Mundo inferior (H06-H10). The world items mirror MIProgress (see CampaignWorldFlags).
    public const string MiSeed = "mi_seed", MiBracelets1 = "mi_bracelets_1", MiBracelets2 = "mi_bracelets_2", MiBracelets3 = "mi_bracelets_3";
    public const string CocaAffinity = "coca_affinity", MiShield04 = "mi_shield_04", MiShortcut03 = "mi_shortcut_03", MiShortcut06 = "mi_shortcut_06";
    public const string MiHorn = "mi_horn", MiHornGate = "mi_horn_gate", ChiaSealed = "chia_sealed";
    public const string MiGuardian = "mi_guardian", ChiaReleased = "chia_released";
    // Retorno lunar y urna (H11-H12)
    public const string ChiaInCustody = "chia_in_custody", GuacamayaAffinity = "guacamaya_affinity";
    public const string UrnSolved = "urn_solved", MsUnlocked = "ms_unlocked", FirstGuacamayaSeen = "first_guacamaya_seen";
    // Mundo superior (H13-H16). The world items mirror MSProgress.
    public const string MsRune1 = "ms_rune_1", MsRune2 = "ms_rune_2", MsWings = "ms_wings";
    public const string MsYopo1 = "ms_yopo_1", MsYopo2 = "ms_yopo_2", MsKey = "ms_key", MsLockOpen = "ms_lock_open";
    public const string ArmorMemory1 = "armor_memory_1", ArmorMemory2 = "armor_memory_2", ArmorMemory3 = "armor_memory_3";
    public const string CondorResolved = "condor_resolved", EagleResolved = "eagle_resolved", QuimueEchoSeen = "quimue_echo_seen";
    public const string MsGuardian = "ms_guardian", SueReleased = "sue_released";
    // Quimue y desenlace (H17-H21)
    public const string FinalDuelAvailable = "final_duel_available", QuimueDefeated = "quimue_defeated";
    public const string MasksInCustody = "masks_in_custody", ReturnHomeSeen = "return_home_seen";
    public const string CanonicalWound = "canonical_wound", NemequeneDead = "nemequene_dead";
    public const string TisquesusaStaff = "tisquesusa_staff", CampaignComplete = "campaign_complete";

    // A later flag implies the earlier ones it can only follow (a save written mid-sequence, or a
    // world flag mirrored before the story caught up). Pairs: { flag, implied }.
    public static readonly string[,] Implications =
    {
        { VisionSeen, StaffOwned }, { PoporoOwned, VisionSeen }, { MapOwned, PoporoOwned }, { HubActive, MapOwned },
        { MiUnlocked, LessonsComplete }, { MiUnlocked, TrainingComplete }, { MiUnlocked, HubActive },
        { ChiaReleased, MiGuardian }, { ChiaReleased, ChiaSealed }, { ChiaReleased, MiUnlocked }, { ChiaSealed, MiHornGate }, { MiHornGate, MiHorn },
        { ChiaInCustody, ChiaReleased }, { GuacamayaAffinity, ChiaInCustody }, { UrnSolved, GuacamayaAffinity },
        { MsUnlocked, UrnSolved }, { MsRune2, MsRune1 }, { MsWings, MsRune2 }, { MsLockOpen, MsKey },
        { MsRune1, ArmorMemory1 }, { MsRune2, ArmorMemory2 }, { MsKey, ArmorMemory3 },
        { SueReleased, MsGuardian }, { SueReleased, MsUnlocked }, { MsGuardian, MsLockOpen }, { FinalDuelAvailable, SueReleased }, { QuimueDefeated, FinalDuelAvailable },
        { MasksInCustody, QuimueDefeated }, { ReturnHomeSeen, MasksInCustody }, { CanonicalWound, ReturnHomeSeen },
        { NemequeneDead, CanonicalWound }, { TisquesusaStaff, NemequeneDead }, { CampaignComplete, TisquesusaStaff },
    };
}

// World progress keeps its own short ids (MIProgress/MSProgress, already in saves). This table
// maps them to the campaign ids so the story reads one vocabulary. Pairs: { world id, campaign id }.
public static class CampaignWorldFlags
{
    public static readonly string[,] Lower =
    {
        { "semilla", CampaignFlags.MiSeed }, { "brazaletes1", CampaignFlags.MiBracelets1 },
        { "brazaletes2", CampaignFlags.MiBracelets2 }, { "brazaletes3", CampaignFlags.MiBracelets3 },
        { "coca", CampaignFlags.CocaAffinity }, { "salida_04", CampaignFlags.MiShield04 },
        { "atajo_03_01", CampaignFlags.MiShortcut03 }, { "corredor_06", CampaignFlags.MiShortcut06 },
        { "cuerno", CampaignFlags.MiHorn }, { "reja_cuerno", CampaignFlags.MiHornGate },
        { "chia_sellada", CampaignFlags.ChiaSealed }, { "guardian", CampaignFlags.MiGuardian },
        { "chia_libre", CampaignFlags.ChiaReleased },
    };
    public static readonly string[,] Upper =
    {
        { "ms_runa_portales", CampaignFlags.MsRune1 }, { "ms_runa_escalada", CampaignFlags.MsRune2 },
        { "ms_alas", CampaignFlags.MsWings }, { "ms_yopo_1", CampaignFlags.MsYopo1 }, { "ms_yopo_2", CampaignFlags.MsYopo2 },
        { "ms_llave", CampaignFlags.MsKey }, { "ms_cierre_abierto", CampaignFlags.MsLockOpen },
        { "ms_condor", CampaignFlags.CondorResolved }, { "ms_aguila", CampaignFlags.EagleResolved },
        { "ms_eco_quimue", CampaignFlags.QuimueEchoSeen },
        { "ms_jefe_vencido", CampaignFlags.MsGuardian }, { "ms_sue", CampaignFlags.SueReleased },
    };

    public static System.Collections.Generic.IEnumerable<string> WorldIds(string[,] table)
    {
        for (int i = 0; i < table.GetLength(0); i++) yield return table[i, 0];
    }
    public static string ToCampaign(string[,] table, string worldId)
    {
        for (int i = 0; i < table.GetLength(0); i++) if (table[i, 0] == worldId) return table[i, 1];
        return null;
    }
    public static string ToWorld(string[,] table, string campaignId)
    {
        for (int i = 0; i < table.GetLength(0); i++) if (table[i, 1] == campaignId) return table[i, 0];
        return null;
    }
}
