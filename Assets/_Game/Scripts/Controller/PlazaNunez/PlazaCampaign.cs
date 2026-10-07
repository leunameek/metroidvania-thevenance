using UnityEngine;

// The story of Plaza Núñez beyond the tutorial (guion H11-H12): Bachué by the fountain receives
// Chía and gives the yopo, answers the contextual questions (D02, D03, D08, D11), and three urns
// beside the upper portal hide the one without remains where the horn's memory opens the clouds.
// Built at runtime so the hand-edited scene stays untouched; provisional figures until the models
// exist (Resources/Characters/Bachue, Resources/Props/UrnaVacia...).
public sealed class PlazaCampaign : MonoBehaviour, IDuelStage
{
    public const int EmptyUrn = 1;
    private TechnicalDemoController _demo;
    private PlazaPortal _upper;

    public static PlazaCampaign Create(TechnicalDemoController demo)
    {
        var campaign = demo.gameObject.AddComponent<PlazaCampaign>();
        campaign._demo = demo;
        campaign.Build();
        return campaign;
    }

    private void Build()
    {
        foreach (var portal in _demo.Portals) if (portal != null && portal.world > 0) _upper = portal;
        Vector3 spawn = _demo.Player.transform.position;
        BuildBachue(spawn);
        if (_upper != null) BuildUrns(spawn);
        BuildQuimue();
    }

    // ---------- Quimue (C16, E11, C17) ----------
    private Transform _quimue;
    private Light _quimueLight;
    private bool _c16Queued, _finalRunning;
    private System.Collections.Generic.List<Renderer> _guardianRenderers;
    private bool _guardianHidden;

    private void BuildQuimue()
    {
        var combat = _demo.Combat;
        if (combat == null || combat.Guardian == null) return;
        Vector3 guardian = combat.Guardian.position;
        Vector3 mark = combat.PlayerMark != null ? combat.PlayerMark.position : guardian + Vector3.back * 4f;
        Vector3 toMark = mark - guardian; toMark.y = 0;
        Vector3 side = Vector3.Cross(Vector3.up, toMark.sqrMagnitude > .01f ? toMark.normalized : Vector3.back);
        Vector3 at = Ground(guardian + side * 2.4f);
        _quimue = StoryProps.Figure("Quimue", transform, at, new Color(.12f, .1f, .14f), new Color(.85f, .72f, .35f), 1.85f);
        _quimue.rotation = Quaternion.LookRotation(mark - at);
        StoryActor.Ensure(_quimue.gameObject, "Quimue", 1.7f);
        var glow = new GameObject("Lazos").AddComponent<Light>();
        glow.transform.SetParent(_quimue, false); glow.transform.localPosition = Vector3.up * 1.4f;
        glow.type = LightType.Point; glow.range = 5f; glow.intensity = 2.5f; glow.color = new Color(.6f, .7f, 1f);
        _quimueLight = glow;
        PlazaStoryPoint.Create("Enfrentar a Quimue", transform, mark, 3.2f,
            () => CampaignProgress.Model.FinalDuelReady && !_finalRunning, () => "Enfrentar a Quimue", StartFinal);
        RefreshQuimue();
    }

    private void RefreshQuimue()
    {
        if (_quimue == null) return;
        var c = CampaignProgress.Model;
        bool present = c.Has(CampaignFlags.SueReleased) && !c.Has(CampaignFlags.MasksInCustody);
        if (_quimue.gameObject.activeSelf != present) _quimue.gameObject.SetActive(present);
        if (_guardianRenderers == null || _guardianHidden != present)
        {
            _guardianHidden = present;
            // Quimue takes the circle: the training guardian steps out of sight meanwhile.
            // Only the renderers that were visible (the Tripo model): the hidden graybox stays hidden.
            if (_guardianRenderers == null)
            {
                _guardianRenderers = new System.Collections.Generic.List<Renderer>();
                var guardian = _demo.Combat != null ? _demo.Combat.Guardian : null;
                if (guardian != null)
                    foreach (var r in guardian.GetComponentsInChildren<Renderer>(true)) if (r.enabled) _guardianRenderers.Add(r);
            }
            foreach (var r in _guardianRenderers) if (r != null) r.enabled = !present;
        }
        // Defeated: on his knees, the two bonds dark.
        bool defeated = c.Has(CampaignFlags.QuimueDefeated);
        // C17: the rigged Quimue kneels; the provisional figure only shrinks.
        var acting = CharacterActions.Of(_quimue);
        if (acting != null) { if (defeated && acting.Current != "Kneel" && !_finalRunning) acting.PlayAny("Kneel"); }
        else _quimue.localScale = new Vector3(1, defeated ? .6f : 1, 1);
        if (_quimueLight != null) _quimueLight.enabled = !defeated;
    }

    private void Update()
    {
        RefreshQuimue();
        var c = CampaignProgress.Model;
        // Back with Sué: Quimue follows the solar resonance into the plaza (H17 / C16).
        if (!_c16Queued && c.Has(CampaignFlags.SueReleased) && !c.Has(CampaignFlags.FinalDuelAvailable)
            && _demo.State == TechnicalDemoState.Exploration)
        {
            _c16Queued = true;
            StoryPlayer.PlaySequence("H17", () => _demo.SetStatus("Quimue espera en el círculo. Prepárate y acércate cuando quieras."), "Quimue en la plaza");
        }
    }

    private void StartFinal()
    {
        var combat = _demo.Combat;
        _finalRunning = true;
        _demo.Player.SetInputLocked(true);
        if (combat != null && combat.PlayerMark != null)
        {
            _demo.Player.Teleport(combat.PlayerMark.position);
            _demo.Player.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(_quimue.position - combat.PlayerMark.position, Vector3.up));
        }
        var prefs = NaturalInputPrefs.Load();
        TurnDuelController.Run(new QuimueRules(), this, Mathf.RoundToInt(MSProgress.AttackDamage), null, null, prefs.reactionScale, OnFinalEnded);
    }

    private void OnFinalEnded(bool victory)
    {
        _finalRunning = false;
        _demo.Player.SetInputLocked(false);
        if (!victory)
        {
            _demo.SetStatus("Quimue te supera esta vez. Tus máscaras siguen a salvo: vuelve al círculo cuando estés listo.");
            return;
        }
        CampaignProgress.Set(CampaignFlags.QuimueDefeated); // DuelAudio plays the broken resonance and the release
        _demo.SetStatus("Los lazos de luna y sol se apagan. Habla con Bachué.");
    }

    // IDuelStage: the bonds glow with the announced origin; nothing here deals damage.
    public Transform Focus => _quimue;
    public void OnTelegraph(DuelMove move)
    {
        if (_quimueLight != null) { _quimueLight.color = move.Origin == DuelTarget.Sun ? new Color(1f, .78f, .3f) : new Color(.6f, .7f, 1f); _quimueLight.intensity = 6f; }
    }
    public void OnResolved(DuelMove move, bool correct)
    {
        if (_quimueLight != null) _quimueLight.intensity = 2.5f;
    }
    public void OnPlayerAction(DuelAction action, DuelTarget target, string result)
    {
        if (_quimue != null) MIBurst.Spawn(_quimue.position + Vector3.up * 1.4f, target == DuelTarget.Sun ? new Color(1f, .8f, .35f) : new Color(.65f, .75f, 1f));
    }
    public void OnDecide() { }

    // ---------- Bachué ----------
    private void BuildBachue(Vector3 spawn)
    {
        var fountain = GameObject.Find("MODEL_Fuente");
        Vector3 center = fountain != null ? Bounds(fountain).center : spawn + Vector3.forward * 8f;
        float radius = fountain != null ? Mathf.Max(Bounds(fountain).extents.x, Bounds(fountain).extents.z) : 1f;
        Vector3 toSpawn = spawn - center; toSpawn.y = 0;
        toSpawn = toSpawn.sqrMagnitude > .01f ? toSpawn.normalized : Vector3.back;
        // Beside the fountain, a quarter turn from the arrival path so she never blocks it.
        Vector3 side = Quaternion.Euler(0, 35, 0) * toSpawn;
        Vector3 at = Ground(center + side * (radius + 1.1f));
        var bachue = StoryProps.Figure("Bachue", transform, at, new Color(.88f, .84f, .74f), new Color(.2f, .55f, .45f), 1.72f);
        bachue.rotation = Quaternion.LookRotation(-side);
        StoryActor.Ensure(bachue.gameObject, "Bachué", 1.6f);
        PlazaStoryPoint.Create("Hablar con Bachue", transform, at, 2.8f, () => true, BachuePrompt, TalkToBachue);
    }

    private static string BachuePrompt()
    {
        var c = CampaignProgress.Model;
        if (c.Has(CampaignFlags.ChiaReleased) && !c.Has(CampaignFlags.ChiaInCustody)) return "Entregar Chía a Bachué";
        return "Hablar con Bachué";
    }

    private void TalkToBachue()
    {
        var c = CampaignProgress.Model;
        // H11 / C10: Chía in custody, the yopo for the guacamaya, the clue of the empty urn.
        if (c.Has(CampaignFlags.ChiaReleased) && !c.Has(CampaignFlags.ChiaInCustody))
        {
            StoryPlayer.PlaySequence("H11", () => _demo.SetStatus("Recibes el yopo: la guacamaya despierta. Busca la urna sin restos junto al portal superior."),
                "Mitad del viaje");
            return;
        }
        string hint;
        switch (c.Chapter)
        {
            case CampaignChapter.PlazaTutorial: hint = c.Has(CampaignFlags.LessonsComplete) ? "D03" : "D02"; break;
            case CampaignChapter.LowerWorld: hint = "D03"; break;
            case CampaignChapter.Urn: hint = "D08"; break;
            case CampaignChapter.Ending:
                // H18 / C18: the masks stay with Bachué; Nemequene goes home (the epilogue).
                StoryPlayer.PlaySequence("H18", () => CampaignEpilogue.Begin(), "Victoria incompleta");
                return;
            default: hint = "D11"; break;
        }
        StoryPlayer.PlayHint(CampaignProgress.Script.Hint(hint), "Bachué");
    }

    // ---------- Urns (O-N06, H12, C11) ----------
    private void BuildUrns(Vector3 spawn)
    {
        Vector3 portal = _upper.transform.position;
        Vector3 toSpawn = spawn - portal; toSpawn.y = 0;
        toSpawn = toSpawn.sqrMagnitude > .01f ? toSpawn.normalized : -_upper.transform.forward;
        Vector3 side = Vector3.Cross(Vector3.up, toSpawn);
        for (int i = 0; i < 3; i++)
        {
            int index = i;
            Vector3 at = Ground(portal + toSpawn * 3.5f + side * (4.5f + 1.8f * i));
            var urn = StoryProps.Build(index == EmptyUrn ? "UrnaVacia" : "UrnaMemoria", transform, at);
            urn.rotation = Quaternion.LookRotation(toSpawn);
            PlazaStoryPoint.Create("Urna " + (i + 1), transform, at, 1.4f,
                () => CampaignProgress.Chapter == CampaignChapter.Urn, () => "Examinar la urna " + (index + 1), () => InspectUrn(index, urn));
        }
    }

    // O-N06: each urn is taken in the hands and turned to look inside; only then it can be used.
    private void InspectUrn(int index, Transform urn)
    {
        bool empty = index == EmptyUrn;
        PlazaPieceInspection.Open(_demo, urn, "Urna " + (index + 1),
            "Una urna de barro de las que guardan a los antepasados, junto al portal superior.",
            () => empty ? "Está vacía y liviana: no guarda restos. El soporte de su interior tiene la forma del cuerno."
                : "Dentro reposan restos y ofrendas de alguien querido. Esta memoria debe quedarse aquí.",
            empty ? "Usar el poporo" : "Devolverla", () => UseUrn(index));
    }

    private void UseUrn(int index)
    {
        if (index != EmptyUrn)
        {
            _demo.SetStatus("Aquí permanece una memoria. La devuelves intacta; busca la urna sin restos.");
            GameAudio.Play("Foley/objeto_soltar", .8f);
            return;
        }
        // The poporo projects the memory of the horn into the empty support; the clouds open and
        // the guacamaya (C11) rises from the urn toward the upper portal.
        _demo.Audio?.Play(PlazaSound.Complete);
        CreatureFlight.Launch("Guacamaya", Ground(_upper != null ? _upper.transform.position + (_demo.Player.transform.position - _upper.transform.position).normalized * 3f : _demo.Player.transform.position),
            _upper != null ? _upper.transform.position + Vector3.up * 6f : _demo.Player.transform.position + Vector3.up * 8f);
        StoryPlayer.PlaySequence("H12", () =>
        {
            _demo.SetStatus("La guacamaya cruza las nubes hacia el umbral superior.");
            if (_upper != null && _upper.Available) _demo.Travel(_upper);
        }, "La urna vacía");
    }

    // ---------- helpers ----------
    private static Bounds Bounds(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(root.transform.position, Vector3.one);
        var b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return b;
    }

    private static Vector3 Ground(Vector3 at)
    {
        if (Physics.Raycast(at + Vector3.up * 4f, Vector3.down, out var hit, 12f, ~0, QueryTriggerInteraction.Ignore)) return hit.point;
        return at;
    }
}
