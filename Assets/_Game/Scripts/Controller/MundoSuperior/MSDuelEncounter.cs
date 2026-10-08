using Nemequene.UI;
using UnityEngine;

// The two trials of the upper world that the script adds (guion E09 mujer-cóndor after Runa 2 at
// MS03, E10 mujer-águila on the key terrace of MS06). Built at runtime next to their terraces so
// the hand-edited scene stays untouched: an E mark on the terrace and the creature hovering over
// the back edge, in front of the exploration camera (2026-10-07 playtest: behind the camera she
// could not be found). She is not there before her trial is ready: when it is (Runa 2 taken, for
// the condor), she comes down from the sky over the terrace with a call and a notice. The figure is a provisional silhouette until the rigged model is added under
// Resources/Characters/<model> (it is used instead when present). A victory is recognition: the
// creature says its last line (D09 / D10) and goes out in motes, leaving nothing on the terrace.
public sealed class MSDuelEncounter : MIInteractable, IDuelStage
{
    private string _encounter, _flag, _speaker, _modelPath;
    private Color _plumage;
    private Transform _figure, _wingLeft, _wingRight, _playerMark;
    private TurnDuelController _duel;
    private DuelMove _move;
    private float _time, _moveTime;
    private bool _fighting;
    private PlayerController _player;
    private Vector3 _home;
    // Arrival: -1 not yet (hidden), 0..1 coming down, 1 there.
    private float _arrival = -1;
    private const float ArrivalHeight = 9f, ArrivalSeconds = 2.6f;

    public bool Fighting => _fighting;
    public bool Resolved => MSProgress.Has(_flag);
    public override bool Available => base.Available && !_fighting && !Resolved && Ready && _arrival >= 1 && !StoryPlayer.Pending;
    public override string Prompt => "Responder a la prueba de la " + _speaker.ToLowerInvariant();
    public Transform Focus => _figure;
    // E09 waits for Runa 2; E10 for the arrival on the key terrace (always true there).
    private bool Ready => _encounter != CondorRules.EncounterId || MSProgress.Has(MSProgress.RuneClimb);

    // zone: the terrace root; mark: local position of the E mark; perch: local position of the figure.
    public static MSDuelEncounter Spawn(string zoneName, string encounter, string flag, string speaker, string modelPath,
        Color plumage, Vector3 mark, Vector3 perch)
    {
        var zone = GameObject.Find(zoneName);
        if (zone == null) { Debug.LogWarning("[MS] Zona no encontrada para " + encounter + ": " + zoneName); return null; }
        var root = new GameObject("Encuentro_" + encounter);
        root.transform.SetParent(zone.transform, false);
        root.transform.localPosition = mark;
        root.transform.localRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(perch - mark, Vector3.up));
        var e = root.AddComponent<MSDuelEncounter>();
        e._encounter = encounter; e._flag = flag; e._speaker = speaker; e._modelPath = modelPath; e._plumage = plumage;
        e.range = 3f; e.displayName = speaker;
        e._playerMark = new GameObject("MarcaJugador").transform;
        e._playerMark.SetParent(root.transform, false);
        e._playerMark.localPosition = Vector3.back * 1.2f;
        e._playerMark.localRotation = Quaternion.identity;
        e.BuildFigure(zone.transform.TransformPoint(perch));
        StoryActor.Ensure(e._figure.gameObject, speaker, 1.7f);
        if (e.Resolved) CreatureDissolve.HideNow(e._figure);
        else if (e.Ready) e._arrival = 1;
        else e._figure.gameObject.SetActive(false);
        return e;
    }

    private void BuildFigure(Vector3 position)
    {
        _figure = new GameObject(_speaker).transform;
        _figure.SetParent(transform, true);
        _figure.position = position; _home = position;
        _figure.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.position - position, Vector3.up));
        var prefab = Resources.Load<GameObject>(_modelPath);
        if (prefab != null) { Instantiate(prefab, _figure, false); return; }
        // Provisional silhouette: body, head and two wings that beat (form, not colour, tells the move).
        var body = Part(PrimitiveType.Capsule, _figure, new Vector3(0, 1f, 0), new Vector3(.7f, 1f, .5f), _plumage);
        Part(PrimitiveType.Sphere, _figure, new Vector3(0, 2.2f, .05f), Vector3.one * .45f, new Color(.85f, .78f, .65f));
        Part(PrimitiveType.Cube, _figure, new Vector3(0, 2.15f, .3f), new Vector3(.12f, .1f, .3f), new Color(.9f, .7f, .25f));
        _wingLeft = Wing(-1); _wingRight = Wing(1);
        Destroy(body.GetComponent<Collider>());
    }

    private Transform Wing(int side)
    {
        var pivot = new GameObject(side < 0 ? "AlaIzquierda" : "AlaDerecha").transform;
        pivot.SetParent(_figure, false); pivot.localPosition = new Vector3(.3f * side, 1.6f, -.1f);
        Part(PrimitiveType.Cube, pivot, new Vector3(1.1f * side, -.2f, 0), new Vector3(2.2f, .9f, .08f), _plumage * .8f);
        Part(PrimitiveType.Cube, pivot, new Vector3(2.0f * side, -.55f, 0), new Vector3(.8f, .5f, .06f), Color.Lerp(_plumage, Color.white, .6f));
        return pivot;
    }

    private static Transform Part(PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(type);
        var collider = go.GetComponent<Collider>(); if (collider != null) Destroy(collider);
        go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale;
        var renderer = go.GetComponent<Renderer>();
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader != null) renderer.material = new Material(shader);
        renderer.material.color = color;
        return go.transform;
    }

    public override void Interact(PlayerController player)
    {
        if (!Available) return;
        _player = player; _fighting = true;
        MundoSuperiorDirector.Instance?.BeginCombat(this, _playerMark);
        if (!StoryPlayer.Trigger(StoryTriggers.Duel(_encounter, "intro"), StartDuel)) StartDuel();
    }

    private void StartDuel()
    {
        if (!_fighting) return;
        var director = MundoSuperiorDirector.Instance;
        _duel = TurnDuelController.Run(DuelEncounters.Create(_encounter), this, Mathf.RoundToInt(MSProgress.AttackDamage),
            _player.GetComponent<Health>(), director != null ? director.Natural : null,
            director != null ? director.ReactionMultiplier : 1f, OnDuelEnded);
    }

    public void ResetEncounter()
    {
        if (_duel != null) { var d = _duel; _duel = null; d.Abort(); }
        _fighting = false; _move = null;
        if (_figure != null) _figure.position = _home;
    }

    private void OnDuelEnded(bool victory)
    {
        _duel = null;
        if (!victory) return;
        MSProgress.Set(_flag);
        _fighting = false;
        ShowResolved();
        MSAudio.Play("victoria", .8f);
        var director = MundoSuperiorDirector.Instance;
        director?.EndCombat(false);
        director?.Hud?.Notify(_speaker, "Te reconoce y pliega las alas. El camino sigue abierto.", UIIcon.Objective, UIPalette.Jade);
        var hint = CampaignProgress.Script != null ? CampaignProgress.Script.Hint(_encounter == CondorRules.EncounterId ? "D09" : "D10") : null;
        if (hint != null) StoryPlayer.PlayHint(hint, _speaker, Vanish); else Vanish();
    }

    private void Vanish()
    {
        if (_figure == null || !_figure.gameObject.activeSelf) return;
        CreatureDissolve.Run(_figure, new Color(.95f, .85f, .6f), .2f);
    }

    // ---------- IDuelStage ----------
    // The calls, wings and blows of the two birds come from DuelAudio (E09 condor, E10 eagle).
    public void OnTelegraph(DuelMove move) { _move = move; _moveTime = 0; }
    public void OnResolved(DuelMove move, bool correct)
    {
        if (correct && move.Accepts(DuelDefense.Dodge) && _player != null) _player.PerformDodge(TurnDuelController.Current != null ? TurnDuelController.Current.DodgeSide : 1);
        _move = null;
    }
    public void OnPlayerAction(DuelAction action, DuelTarget target, string result)
    {
        if (_figure != null) MIBurst.Spawn(_figure.position + Vector3.up * 1.4f, new Color(1f, .8f, .4f));
    }
    public void OnDecide() { }

    // She comes down over the terrace, wings beating, with her call and a notice.
    private void Arrive()
    {
        _arrival = 0;
        _figure.gameObject.SetActive(true);
        _figure.position = _home + Vector3.up * ArrivalHeight;
        GameAudio.PlayAt("Criaturas/" + (_encounter == CondorRules.EncounterId ? "condor_aviso" : "aguila_grito"), _home + Vector3.up * 4f, 1f, AudioChannel.Voice, 60f);
        MundoSuperiorDirector.Instance?.Hud?.Notify(_speaker, "Desciende sobre la terraza. Acércate a la marca para responder a su prueba.", UIIcon.Guardian, UIPalette.GoldLight);
    }

    private void Update()
    {
        if (_figure == null) return;
        if (_arrival < 0)
        {
            if (Ready && !Resolved) Arrive();
            return;
        }
        if (_arrival < 1)
        {
            _arrival = Mathf.Min(1, _arrival + Time.deltaTime / ArrivalSeconds);
            float k = 1 - Mathf.Pow(1 - _arrival, 3);
            _figure.position = Vector3.Lerp(_home + Vector3.up * ArrivalHeight, _home + Vector3.up * 1.2f, k);
            if (_arrival >= 1) MIBurst.Spawn(_figure.position + Vector3.up * 1.4f, new Color(1f, .9f, .6f));
        }
        _time += Time.deltaTime;
        bool folded = Resolved && !_fighting;
        float beat = folded ? 0 : Mathf.Sin(_time * (_move != null ? 9f : 3f)) * 25f;
        float raise = 0;
        if (_move != null)
        {
            _moveTime += Time.deltaTime;
            float k = Mathf.Clamp01(_moveTime / TurnDuelModel.TelegraphSeconds);
            raise = _move.Id == "ala" || _move.Id == "carga" ? -60f * k : _move.Id == "viento" ? 40f * k : 20f * k;
        }
        if (_wingLeft != null) _wingLeft.localRotation = Quaternion.Euler(0, 0, folded ? -70 : beat + raise);
        if (_wingRight != null) _wingRight.localRotation = Quaternion.Euler(0, 0, folded ? 70 : -beat - raise);
        // The eagle stands high until Interrumpir grounds her (EagleRules.Elevated).
        var eagle = _duel != null ? _duel.Model.Rules as EagleRules : null;
        float lift = folded ? 0 : (eagle == null || eagle.Elevated ? 1.2f : -.4f) + Mathf.Sin(_time * 1.7f) * .15f;
        if (_arrival >= 1) _figure.position = Vector3.Lerp(_figure.position, _home + Vector3.up * lift, Time.deltaTime * 4f);
    }

    private void ShowResolved()
    {
        if (_figure != null) _figure.position = _home;
    }
}
