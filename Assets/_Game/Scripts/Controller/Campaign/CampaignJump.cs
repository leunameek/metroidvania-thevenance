using UnityEngine;

// TEMPORARY test tool (2026-10-06, user's request): puts the active slot at the start of any
// chapter and loads its scene. The story, the lower world and the upper world of the slot are
// erased and rebuilt with exactly what the earlier chapters leave: their sequences count as
// lived (no dialogue repeats) and the world finds of finished worlds are owned.
public static class CampaignJump
{
    // Chapter to jump to, with the button title; Complete stands for the epilogue in Bacatá.
    public static readonly (CampaignChapter chapter, string title)[] Entries =
    {
        (CampaignChapter.Prologue, "1 · Prólogo en Bacatá"),
        (CampaignChapter.PlazaTutorial, "2 · Plaza: tutorial"),
        (CampaignChapter.LowerWorld, "3 · Mundo inferior"),
        (CampaignChapter.LunarReturn, "4 · Regreso con Chía"),
        (CampaignChapter.Urn, "5 · La urna vacía"),
        (CampaignChapter.UpperWorld, "6 · Mundo superior"),
        (CampaignChapter.SolarReturn, "7 · Quimue en la plaza"),
        (CampaignChapter.Ending, "8 · Hablar con Bachué"),
        (CampaignChapter.Complete, "9 · Epílogo"),
    };

    // Story sequences each chapter leaves behind (index = CampaignChapter).
    private static readonly string[][] Lived =
    {
        new[] { "H01", "H02", "H03" },
        new[] { "H04", "H05" },
        new[] { "H06", "H07", "H08", "H09", "H10" },
        new[] { "H11" },
        new[] { "H12" },
        new[] { "H13", "H14", "H15", "H16" },
        new[] { "H17" },
        new[] { "H18" },
    };

    private static readonly string[] LowerWorld =
    {
        MIProgress.Seed, MIProgress.Bracelets1, MIProgress.Bracelets2, MIProgress.Bracelets3, MIProgress.Coca,
        MIProgress.Shield04, MIProgress.Shortcut03, MIProgress.Corridor06, MIProgress.Horn, MIProgress.HornGate,
        MIProgress.ChiaSealed, MIProgress.Guardian, MIProgress.ChiaReleased,
    };

    public static void Jump(CampaignChapter chapter)
    {
        if (SceneLoader.Loading) return;
        Time.timeScale = 1;
        if (TurnDuelController.Current != null) TurnDuelController.Current.Abort();
        StoryPlayer.SkipAll();
        Prepare(chapter);
        WorldTravel.ClearReturn();
        switch (chapter)
        {
            case CampaignChapter.Prologue:
                CampaignScenes.NextBacataMode = CampaignScenes.BacataMode.Prologue;
                SceneLoader.Load(CampaignScenes.Bacata); break;
            case CampaignChapter.LowerWorld: SceneLoader.Load(WorldTravel.LowerWorldScene); break;
            case CampaignChapter.UpperWorld: SceneLoader.Load(WorldTravel.UpperWorldScene); break;
            case CampaignChapter.Complete: CampaignEpilogue.Begin(); break;
            default: SceneLoader.Load(CampaignScenes.Plaza); break;
        }
        Debug.Log("[Campaign] Salto de prueba a " + CampaignModel.ChapterTitle(chapter) + " · ranura " + WorldTravel.SaveSlot);
    }

    // The flags only: erase the slot's story and worlds, then rebuild the start of the chapter.
    public static void Prepare(CampaignChapter chapter)
    {
        int slot = WorldTravel.SaveSlot;
        CampaignProgress.Erase(slot); MIProgress.Erase(slot); MSProgress.Erase(slot);
        CampaignProgress.Load();
        int index = (int)chapter;
        for (int c = 0; c < index && c < Lived.Length; c++)
            foreach (var id in Lived[c]) Live(id);
        if (index >= (int)CampaignChapter.PlazaTutorial) CampaignProgress.Set(CampaignFlags.HubActive);
        if (index >= (int)CampaignChapter.LowerWorld) CampaignProgress.Set(CampaignFlags.MiUnlocked);
        if (index >= (int)CampaignChapter.LunarReturn) foreach (var f in LowerWorld) MIProgress.Set(f);
        if (index >= (int)CampaignChapter.SolarReturn)
            foreach (var f in CampaignWorldFlags.WorldIds(CampaignWorldFlags.Upper)) MSProgress.Set(f);
        if (index >= (int)CampaignChapter.Ending) CampaignProgress.Set(CampaignFlags.QuimueDefeated);
    }

    // A sequence and every trigger that plays part of it count as already heard.
    private static void Live(string sequence)
    {
        CampaignProgress.CompleteSequence(sequence);
        foreach (var trigger in StoryTriggers.All)
            if (trigger.Sequence == sequence) CampaignProgress.Set(trigger.SeenFlag);
    }
}
