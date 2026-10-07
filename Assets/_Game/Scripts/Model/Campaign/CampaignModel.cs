using System;
using System.Collections.Generic;
using UnityEngine;

// Campaign order of the script (guion 08 "Propuesta de montaje de campaña").
public enum CampaignChapter
{
    Prologue, PlazaTutorial, LowerWorld, LunarReturn, Urn, UpperWorld, SolarReturn, Ending, Complete
}

public readonly struct CampaignObjective
{
    public readonly CampaignChapter Chapter;
    public readonly string Title, Text, Place;
    public CampaignObjective(CampaignChapter chapter, string title, string text, string place)
    {
        Chapter = chapter; Title = title; Text = text; Place = place;
    }
}

// The story state of one save slot: a set of flags that only grows. Chapter, objective and the
// portal gates are derived from it, so loading, skipping a cutscene or replaying a scene never
// needs a separate "current step" that could disagree with what the player owns.
public sealed class CampaignModel
{
    public const int Version = 1;

    [Serializable]
    private sealed class Data
    {
        public int version = Version;
        public List<string> flags = new List<string>();
    }

    private readonly HashSet<string> _flags = new HashSet<string>();
    public event Action<string> Changed;

    public bool Has(string id) => !string.IsNullOrEmpty(id) && _flags.Contains(id);
    public IEnumerable<string> All => _flags;
    public int Count => _flags.Count;

    // Adds the flag and every flag it implies. True when anything was new.
    public bool Set(string id)
    {
        if (string.IsNullOrEmpty(id) || !_flags.Add(id)) return false;
        Imply(id);
        Changed?.Invoke(id);
        return true;
    }
    private void Imply(string id)
    {
        var table = CampaignFlags.Implications;
        for (int i = 0; i < table.GetLength(0); i++)
            if (table[i, 0] == id && _flags.Add(table[i, 1])) Imply(table[i, 1]);
    }
    public void Clear() { _flags.Clear(); Changed?.Invoke(null); }

    public bool LowerWorldOpen => Has(CampaignFlags.MiUnlocked);
    public bool UpperWorldOpen => Has(CampaignFlags.MsUnlocked);
    public bool FinalDuelReady => Has(CampaignFlags.FinalDuelAvailable) && !Has(CampaignFlags.QuimueDefeated);

    public CampaignChapter Chapter
    {
        get
        {
            if (Has(CampaignFlags.CampaignComplete)) return CampaignChapter.Complete;
            if (Has(CampaignFlags.QuimueDefeated)) return CampaignChapter.Ending;
            if (Has(CampaignFlags.SueReleased)) return CampaignChapter.SolarReturn;
            if (Has(CampaignFlags.MsUnlocked)) return CampaignChapter.UpperWorld;
            if (Has(CampaignFlags.GuacamayaAffinity)) return CampaignChapter.Urn;
            if (Has(CampaignFlags.ChiaReleased)) return CampaignChapter.LunarReturn;
            if (Has(CampaignFlags.MiUnlocked)) return CampaignChapter.LowerWorld;
            if (Has(CampaignFlags.HubActive)) return CampaignChapter.PlazaTutorial;
            return CampaignChapter.Prologue;
        }
    }

    public static string ChapterTitle(CampaignChapter chapter)
    {
        switch (chapter)
        {
            case CampaignChapter.Prologue: return "Prólogo: Bacatá";
            case CampaignChapter.PlazaTutorial: return "Plaza Núñez";
            case CampaignChapter.LowerWorld: return "El inframundo";
            case CampaignChapter.LunarReturn: return "Mitad del viaje";
            case CampaignChapter.Urn: return "La urna vacía";
            case CampaignChapter.UpperWorld: return "El mundo superior";
            case CampaignChapter.SolarReturn: return "Quimue";
            case CampaignChapter.Ending: return "Victoria incompleta";
            default: return "Legado";
        }
    }

    // The next concrete step, in the words of the journal. Only steps of the current chapter are
    // named, so no future dialogue or place leaks early (guion 08 "Historia").
    public CampaignObjective Objective
    {
        get
        {
            var chapter = Chapter;
            string text, place;
            switch (chapter)
            {
                case CampaignChapter.Prologue:
                    place = "Bacatá";
                    text = !Has(CampaignFlags.StaffOwned) ? "Recibe el bastón de Saguanmachica."
                        : !Has(CampaignFlags.VisionSeen) ? "Ve al consejo: Tisquesusa te espera."
                        : !Has(CampaignFlags.PoporoOwned) ? "Medita en el punto de meditación."
                        : !Has(CampaignFlags.MapOwned) ? "Examina el mapa que te dio Bachué."
                        : "Sigue el mapa hasta Plaza Núñez.";
                    break;
                case CampaignChapter.PlazaTutorial:
                    place = "Plaza Núñez";
                    text = !Has(CampaignFlags.LessonsComplete) ? "Examina las tres piezas de las estaciones."
                        : !Has(CampaignFlags.TrainingComplete) ? "Entra al círculo y completa el duelo de práctica."
                        : "Cruza el portal inferior.";
                    break;
                case CampaignChapter.LowerWorld:
                    place = "Mundo inferior";
                    text = !Has(CampaignFlags.MiSeed) ? "Encuentra la semilla en el santuario de raíces."
                        : !Has(CampaignFlags.MiBracelets1) ? "Busca el primer par de brazaletes en la galería."
                        : !Has(CampaignFlags.MiBracelets2) || !Has(CampaignFlags.MiBracelets3) ? "Reúne los brazaletes del patio de centinelas."
                        : !Has(CampaignFlags.CocaAffinity) ? "Recoge la coca en el nicho del tercer par."
                        : !Has(CampaignFlags.MiShield04) ? "Rompe el escudo del centinela y abre la reja."
                        : !Has(CampaignFlags.MiHorn) ? "Encuentra el cuerno de respuesta."
                        : !Has(CampaignFlags.MiHornGate) ? "Usa el cuerno en el soporte de la antesala."
                        : !Has(CampaignFlags.ChiaSealed) ? "Examina la máscara sellada de Chía."
                        : "Corta el lazo del guardián caimán-murciélago.";
                    break;
                case CampaignChapter.LunarReturn:
                    place = "Plaza Núñez";
                    text = !Has(CampaignFlags.ChiaInCustody) ? "Regresa a la plaza y entrega Chía a Bachué."
                        : "Recibe el yopo de Bachué.";
                    break;
                case CampaignChapter.Urn:
                    place = "Plaza Núñez";
                    text = "Busca la urna sin restos junto al portal superior.";
                    break;
                case CampaignChapter.UpperWorld:
                    place = "Mundo superior";
                    text = !Has(CampaignFlags.MsRune1) ? "Examina la hebilla de armadura del primer altar."
                        : !Has(CampaignFlags.MsRune2) ? "Cruza el portal y examina el brazal de armadura."
                        : !Has(CampaignFlags.CondorResolved) ? "Responde a la prueba de la mujer-cóndor."
                        : !Has(CampaignFlags.MsWings) ? "Escala la pared marcada hasta las alas."
                        : !Has(CampaignFlags.EagleResolved) ? "Cruza los vuelos hasta la terraza de la mujer-águila."
                        : !Has(CampaignFlags.MsKey) ? "Examina la placa de armadura de la terraza."
                        : !Has(CampaignFlags.MsLockOpen) ? "Usa la llave en el cierre de la antesala."
                        : !Has(CampaignFlags.MsGuardian) ? "Libera a la serpiente de dos cabezas."
                        : "Examina la máscara de Sué.";
                    break;
                case CampaignChapter.SolarReturn:
                    place = "Plaza Núñez";
                    text = !Has(CampaignFlags.FinalDuelAvailable) ? "Regresa a la plaza con Sué."
                        : "Enfrenta a Quimue en el círculo.";
                    break;
                case CampaignChapter.Ending:
                    place = "Plaza Núñez";
                    text = "Habla con Bachué para regresar a Bacatá.";
                    break;
                default:
                    place = "Iguaque";
                    text = "La campaña ha terminado.";
                    break;
            }
            return new CampaignObjective(chapter, ChapterTitle(chapter), text, place);
        }
    }

    public string ToJson()
    {
        var data = new Data { flags = new List<string>(_flags) };
        data.flags.Sort(StringComparer.Ordinal);
        return JsonUtility.ToJson(data);
    }

    // Replaces the state with a stored one. A missing, foreign or broken record leaves it empty.
    public bool LoadJson(string json)
    {
        _flags.Clear();
        if (string.IsNullOrEmpty(json)) return false;
        try
        {
            var data = JsonUtility.FromJson<Data>(json);
            if (data == null || data.version != Version || data.flags == null) return false;
            foreach (var f in data.flags) if (!string.IsNullOrEmpty(f) && _flags.Add(f)) Imply(f);
            return true;
        }
        catch (Exception) { _flags.Clear(); return false; }
    }
}
