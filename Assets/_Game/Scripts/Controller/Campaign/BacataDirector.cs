using System;
using System.Collections;
using System.Collections.Generic;
using Nemequene.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Prologue (H01-H03 / C01-C03) and epilogue (H19-H21 / C19-C21) of the campaign, staged as
// cinematics in four places built at runtime: Bacatá, the meditation hill, the refuge of Tunja and
// the lagoon of Iguaque. The places use the models of Resources/Bacata (BacataModelSetup) and keep
// a block where one is missing; the staff, the poporo, the bow and the arrow are Resources/Props. Fixed camera shots, the story camera frames whoever speaks, fades between
// places. Characters are provisional figures until their rigged models exist (prefab fields here or
// Resources/Characters/<Name>). The death is cinematic: no HUD, no command heard, no respawn.
public sealed class BacataDirector : MonoBehaviour
{
    [SerializeField] private GameObject nemequenePrefab;
    [SerializeField] private GameObject tisquesusaPrefab;
    [SerializeField] private GameObject saguanmachicaPrefab;
    [SerializeField] private GameObject bachuePrefab;
    [SerializeField] private GameObject furachoguaPrefab;
    [SerializeField] private Light sun;

    private static readonly Vector3 Village = Vector3.zero, Hill = new Vector3(40, 0, 34), Refuge = new Vector3(220, 0, 0), Lagoon = new Vector3(420, 0, 0);
    private Camera _camera;
    private Image _fade;
    private TMPro.TMP_Text _card;
    private Transform _world;
    private readonly List<ParticleSystem> _smoke = new List<ParticleSystem>();
    private Transform _nemequene, _saguanmachica, _tisquesusa, _bachue, _furachogua, _invader, _child;
    private bool _waiting;

    private void Start()
    {
        _camera = Camera.main;
        if (sun == null) foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None)) if (l.type == LightType.Directional) sun = l;
        BuildOverlay();
        _world = new GameObject("Bacata_Mundo").transform;
        BuildVillage(); BuildHill(); BuildRefuge(); BuildLagoon();
        BuildCast();
        bool epilogue = CampaignScenes.NextBacataMode == CampaignScenes.BacataMode.Epilogue
            || CampaignProgress.Has(CampaignFlags.MasksInCustody) && !CampaignProgress.Has(CampaignFlags.CampaignComplete);
        CampaignScenes.NextBacataMode = CampaignScenes.BacataMode.Prologue;
        StartCoroutine(epilogue ? Epilogue() : Prologue());
    }

    // ------------------------------------------------------------------ sequences

    private IEnumerator Prologue()
    {
        SetSky(false);
        // Morning in Bacatá: wind, leaves, birds; the staff's wooden motif.
        Sound("amb_bacata_manana", "musica_bacata_prologo", .8f);
        AmbientScatter.On(gameObject).Add("Ambiente/ave_canto", 5f, 12f, 8f, 20f, .45f, 5f);
        yield return FadeIn("Bacatá");
        // C01: the staff carved by the uncle; the adult heir receives it.
        Place(_child, Village + new Vector3(2.2f, 0, 13), Village + new Vector3(0, 0, 13));
        Place(_saguanmachica, Village + new Vector3(-1.2f, 0, 13), Village + new Vector3(2, 0, 13));
        Hide(_nemequene); Hide(_tisquesusa); Hide(_bachue); Hide(_furachogua); Hide(_invader);
        Shot(Village + new Vector3(0.5f, 1.6f, 8.5f), Village + new Vector3(0.5f, 1.1f, 13));
        // The uncle holds the staff he carved; it passes to the heir when he reaches out (C01).
        var staff = Hold(_saguanmachica, "Baston", true);
        yield return Lines("H01", 0, 3, false, "Dos extremos");
        yield return Cut();
        Hide(_child);
        Place(_nemequene, Village + new Vector3(1.6f, 0, 12.5f), Village + new Vector3(-1.2f, 0, 13));
        Shot(Village + new Vector3(4f, 2f, 7f), Village + new Vector3(0, 1.2f, 13));
        Act(_saguanmachica, "Reach");
        StartCoroutine(Hand(staff, _nemequene, .9f));
        yield return Lines("H01", 3, 2, true);
        // C02: the vision of gold and silver, before the council.
        yield return Cut();
        Hide(_saguanmachica);
        Place(_tisquesusa, Village + new Vector3(-14, 0, 6.5f), Village + new Vector3(-14, 0, 2));
        Place(_nemequene, Village + new Vector3(-14, 0, 2), Village + new Vector3(-14, 0, 6.5f));
        Shot(Village + new Vector3(-8.5f, 2.2f, 1.5f), Village + new Vector3(-14, 1.3f, 4.2f));
        // C02: the vision brings the two-tone hum that returns with Quimue (C13, C16).
        StartCoroutine(Swell("Criaturas/quimue_zumbido", 6f, .55f));
        GameAudio.Caption("Zumbido de dos tonos");
        yield return Lines("H02", 0, 99, true, "Oro y plata");
        // C03: seven days and seven nights on the hill; Bachué comes along the same path.
        yield return Cut();
        Hide(_tisquesusa);
        Place(_nemequene, Hill + new Vector3(0, .9f, 0), Hill + new Vector3(0, .9f, -6));
        Shot(Hill + new Vector3(7, 3.5f, -7), Hill + new Vector3(0, 1.5f, 0));
        // C03: water and wind on the hill; the music gives way to silence during the vigil.
        Sound("amb_colina", null, 0);
        AmbientScatter.On(gameObject).Clear();
        Act(_nemequene, "Pray");
        yield return Days(3);
        Place(_bachue, Hill + new Vector3(0, .9f, -4.5f), Hill + new Vector3(0, .9f, 0));
        Hold(_bachue, "Poporo", false);
        Act(_bachue, "Reach");
        yield return Lines("H03", 0, 99, true, "Siete días y siete noches");
        // The poporo and the map are received; the journey to the plaza is elided (C04).
        yield return FadeOut();
        _card.text = "";
        LoadPlaza();
    }

    private IEnumerator Epilogue()
    {
        SetSky(true);
        // C19: the same path in smoke: fire, wind, distant voices; a tense drum.
        Sound("amb_bacata_incendio", "musica_bacata_epilogo", .8f);
        AmbientScatter.On(gameObject).Add("Ambiente/fuego_chasquido", 1.5f, 4f, 4f, 12f, .5f, 1f);
        foreach (var s in _smoke) s.Play();
        Hide(_child); Hide(_bachue); Hide(_furachogua);
        // C19: the same path, now smoke. The uncle defends the passage and dies; the invader aims.
        Place(_saguanmachica, Village + new Vector3(-.8f, 0, 12), Village + new Vector3(0, 0, 8));
        Place(_nemequene, Village + new Vector3(.6f, 0, 9), Village + new Vector3(-.8f, 0, 12));
        Place(_tisquesusa, Village + new Vector3(-6, 0, 4), Village + new Vector3(0, 0, 9));
        Place(_invader, Village + new Vector3(9, 0, 17), Village + new Vector3(.6f, 0, 9));
        Hold(_invader, "Arco", false);
        var heirStaff = Hold(_nemequene, "Baston", true);
        Hide(_invader);
        yield return FadeIn("La verdadera imagen");
        Shot(Village + new Vector3(4, 2.2f, 3.5f), Village + new Vector3(0, 1.1f, 10.5f));
        yield return Lines("H19", 0, 2, false);
        yield return Fall(_saguanmachica, 1.2f, "Death");
        Show(_invader);
        Shot(Village + new Vector3(-2.5f, 2f, 6.5f), Village + new Vector3(7, 1.6f, 15));
        yield return Lines("H19", 2, 1, false);
        yield return Arrow(_invader.position + Vector3.up * 1.5f, _nemequene.position + Vector3.up * 1.3f);
        yield return Fall(_nemequene, .8f, "DeathBack");
        Shot(Village + new Vector3(-3.5f, 2f, 3f), Village + new Vector3(0, .8f, 9));
        yield return Lines("H19", 3, 1, true);
        // C20: the refuge of Tunja, the journey elided. Last words; the staff to the nephew.
        yield return Cut("Refugio de Tunja");
        foreach (var s in _smoke) s.Stop();
        // C20: inside the refuge, the hearth crackles beside them; no music under the last words.
        Sound("amb_refugio", null, 0);
        AmbientScatter.On(gameObject).Clear();
        var hearth = new GameObject("Sonido_Hogar");
        hearth.transform.SetParent(_world, false); hearth.transform.position = Refuge + new Vector3(2, .6f, -1);
        GameAudio.Loop(hearth, "Ambiente/hoguera_bucle", .7f, true, AudioChannel.Ambience, 12f);
        Hide(_saguanmachica); Hide(_invader);
        Place(_nemequene, Refuge + new Vector3(0, .35f, 1), Refuge + new Vector3(0, .35f, 4));
        // Lying wounded: the rigged body holds the last frame of its fall; the figure is laid down.
        var wounded = CharacterActions.Of(_nemequene);
        if (wounded == null || !wounded.Hold("DeathBack")) _nemequene.rotation = Quaternion.Euler(-90, 90, 0);
        // The staff lies on the mat beside him, ready to be handed on.
        if (heirStaff != null) { heirStaff.SetParent(_world, true); heirStaff.SetPositionAndRotation(Refuge + new Vector3(.55f, .12f, 1.4f), Quaternion.Euler(0, 15, 90)); }
        Place(_tisquesusa, Refuge + new Vector3(-1.3f, 0, 1.2f), Refuge + new Vector3(0, 0, 1));
        var nephew = CharacterActions.Of(_tisquesusa); if (nephew != null) nephew.Hold("Kneel");
        Shot(Refuge + new Vector3(2.8f, 2.1f, -2.2f), Refuge + new Vector3(-.4f, .6f, 1.1f));
        yield return Lines("H20", 0, 99, true, "Transmitir antes de morir");
        // C21: Iguaque. Furachogua brings the masks kept by Bachué; the staff is laid down and taken up.
        // C20 ends on the staff's motif.
        GameAudio.Stinger("Musica/estinger_objetivo", .7f);
        yield return Cut("Iguaque");
        Hide(_nemequene);
        SetSky(false);
        // C21: water and wind at the lagoon; the resolved motif.
        Sound("amb_laguna", "musica_legado", .85f);
        Destroy(hearth);
        AmbientScatter.On(gameObject).Add("Ambiente/ave_canto", 6f, 14f, 10f, 24f, .35f, 4f);
        Place(_tisquesusa, Lagoon + new Vector3(0, 0, -15.5f), Lagoon + new Vector3(0, 0, -8));
        if (heirStaff != null) Attach(heirStaff, _tisquesusa, true);
        Place(_furachogua, Lagoon + new Vector3(0, -1.6f, -11), Lagoon + new Vector3(0, 0, -15.5f));
        Shot(Lagoon + new Vector3(5.5f, 2.6f, -20f), Lagoon + new Vector3(0, 1f, -12.5f));
        GameAudio.PlayAt("Foley/aterrizaje_agua", _furachogua.position + Vector3.up * 1.6f, 1f, AudioChannel.Effects, 40f);
        GameAudio.PlayDelayed("Foley/paso_agua", .9f, .8f, AudioChannel.Effects, _furachogua.position + Vector3.up * 1.6f);
        GameAudio.Caption("El agua se abre");
        yield return Rise(_furachogua, 1.6f, 2.5f);
        Act(_furachogua, "Reach");
        yield return Lines("H21", 0, 99, true, "Furachogua");
        yield return Credits();
    }

    // ------------------------------------------------------------------ staging helpers

    private IEnumerator Lines(string sequence, int from, int count, bool complete, string title = null)
    {
        _waiting = true;
        StoryPlayer.PlayLines(sequence, from, count, complete, () => _waiting = false, title);
        while (_waiting) yield return null;
    }

    private void Shot(Vector3 eye, Vector3 target)
    {
        if (_camera == null) return;
        _camera.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(target - eye));
    }

    private static void Place(Transform actor, Vector3 at, Vector3 lookAt)
    {
        if (actor == null) return;
        actor.gameObject.SetActive(true);
        actor.position = at;
        Vector3 d = lookAt - at; d.y = 0;
        actor.rotation = d.sqrMagnitude > .001f ? Quaternion.LookRotation(d) : Quaternion.identity;
    }
    private static void Hide(Transform actor) { if (actor != null) actor.gameObject.SetActive(false); }
    private static void Show(Transform actor) { if (actor != null) actor.gameObject.SetActive(true); }

    private static void Act(Transform actor, string state)
    {
        var acting = actor != null ? CharacterActions.Of(actor) : null;
        if (acting != null) acting.Play(state);
    }

    // A rigged actor plays its death clip; the provisional figure tips over.
    private IEnumerator Fall(Transform actor, float seconds, string state)
    {
        if (actor == null) yield break;
        var acting = CharacterActions.Of(actor);
        Vector3 ground = actor.position;
        if (acting != null && acting.Play(state, .1f))
        {
            GameAudio.PlayDelayed("Foley/aterrizaje_fuerte", .9f, .8f, AudioChannel.Effects, ground);
            yield return new WaitForSeconds(2.4f); yield break;
        }
        Quaternion start = actor.rotation, end = actor.rotation * Quaternion.Euler(-85, 0, 0);
        for (float t = 0; t < seconds; t += Time.deltaTime) { actor.rotation = Quaternion.Slerp(start, end, t / seconds); yield return null; }
        actor.rotation = end;
        GameAudio.PlayAt("Foley/aterrizaje_fuerte", ground, .8f, AudioChannel.Effects, 40f);
        yield return new WaitForSeconds(.6f);
    }

    private IEnumerator Rise(Transform actor, float height, float seconds)
    {
        Vector3 start = actor.position, end = start + Vector3.up * height;
        for (float t = 0; t < seconds; t += Time.deltaTime) { actor.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0, 1, t / seconds)); yield return null; }
        actor.position = end;
    }

    private IEnumerator Arrow(Vector3 from, Vector3 to)
    {
        // The arrow model lies along its X (point at +X); the block stands in without it.
        var arrow = Model("Flecha", from, 0, "Props");
        if (arrow != null) arrow.rotation = Quaternion.LookRotation(to - from) * Quaternion.Euler(0, -90, 0);
        else
        {
            arrow = StoryProps.Part(PrimitiveType.Cylinder, _world, from, new Vector3(.04f, .45f, .04f), new Color(.75f, .72f, .6f));
            arrow.rotation = Quaternion.FromToRotation(Vector3.up, to - from);
        }
        GameAudio.PlayAt("Combate/baston_aire", from, .9f, AudioChannel.Effects, 40f, 1.6f, 0f);
        for (float t = 0; t < .45f; t += Time.deltaTime) { arrow.position = Vector3.Lerp(from, to, t / .45f); yield return null; }
        arrow.position = to;
        GameAudio.PlayAt("Combate/impacto_criatura", to, .9f, AudioChannel.Effects, 40f);
    }

    // Three light cycles condense the seven days (C03), without seven minutes of waiting.
    private IEnumerator Days(int cycles)
    {
        if (sun == null) yield break;
        Quaternion rest = sun.transform.rotation; Color color = sun.color;
        for (float t = 0; t < cycles; t += Time.deltaTime / 1.1f)
        {
            float a = t % 1f;
            sun.transform.rotation = Quaternion.Euler(Mathf.Lerp(-10, 190, a), 30, 0);
            sun.color = Color.Lerp(new Color(1f, .62f, .4f), Color.white, Mathf.Sin(a * Mathf.PI));
            sun.intensity = Mathf.Lerp(.15f, 1.3f, Mathf.Sin(a * Mathf.PI));
            // Birds greet each dawn, insects each night (the seven days condensed).
            int phase = Mathf.FloorToInt(t * 4f);
            if (phase != _dayPhase)
            {
                _dayPhase = phase;
                Vector3 near = _camera != null ? _camera.transform.position + _camera.transform.forward * 6f : Hill;
                if (phase % 4 == 0) GameAudio.PlayAt("Ambiente/ave_canto", near + Vector3.up * 3f, .6f, AudioChannel.Ambience, 30f);
                else if (phase % 4 == 3) GameAudio.PlayAt("Ambiente/insecto_noche", near, .5f, AudioChannel.Ambience, 30f);
            }
            yield return null;
        }
        sun.transform.rotation = rest; sun.color = color; sun.intensity = 1.1f;
    }

    private int _dayPhase = -1;

    // The bed and the music of a place (null music = silence under the scene).
    private static void Sound(string bed, string music, float musicVolume)
    {
        GameAudio.Ambience("Ambiente/" + bed, 1f, 2.5f);
        if (music == null) GameAudio.StopMusic(3f);
        else GameAudio.Music("Musica/" + music, musicVolume, 3f);
    }

    // A loop that rises and fades away once (the two-tone hum of the vision).
    private IEnumerator Swell(string id, float seconds, float volume)
    {
        var host = new GameObject("Sonido_" + id);
        host.transform.SetParent(transform, false);
        var source = GameAudio.Loop(host, id, 0f, false, AudioChannel.Voice);
        for (float t = 0; t < seconds; t += Time.deltaTime)
        {
            GameAudio.SetLoopVolume(source, volume * Mathf.Sin(Mathf.Clamp01(t / seconds) * Mathf.PI));
            yield return null;
        }
        Destroy(host);
    }

    private void SetSky(bool smoke)
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = smoke ? new Color(.32f, .25f, .22f) : new Color(.7f, .78f, .82f);
        RenderSettings.fogDensity = smoke ? .03f : .006f;
        RenderSettings.ambientLight = smoke ? new Color(.35f, .25f, .22f) : new Color(.55f, .58f, .6f);
        if (_camera != null) { _camera.clearFlags = CameraClearFlags.SolidColor; _camera.backgroundColor = RenderSettings.fogColor; }
        if (sun != null) { sun.color = smoke ? new Color(1f, .55f, .35f) : new Color(1f, .96f, .88f); sun.intensity = smoke ? .7f : 1.1f; }
    }

    // ------------------------------------------------------------------ overlay

    private void BuildOverlay()
    {
        var canvas = UIKit.ScreenCanvas(transform, "BacataOverlay", 1250);
        _fade = UIKit.Rect("Fade", canvas).gameObject.AddComponent<Image>();
        _fade.color = Color.black; _fade.raycastTarget = false;
        _card = UIKit.Label(canvas, "", 54, UIPalette.GoldLight, true);
    }

    private IEnumerator FadeIn(string card)
    {
        _card.text = card ?? "";
        _fade.color = Color.black;
        yield return new WaitForSeconds(card != null ? 1.6f : .3f);
        _card.text = "";
        for (float t = 0; t < .9f; t += Time.deltaTime) { _fade.color = new Color(0, 0, 0, 1 - t / .9f); yield return null; }
        _fade.color = Color.clear;
    }
    private IEnumerator FadeOut()
    {
        for (float t = 0; t < .7f; t += Time.deltaTime) { _fade.color = new Color(0, 0, 0, t / .7f); yield return null; }
        _fade.color = Color.black;
    }
    private IEnumerator Cut(string card = null)
    {
        yield return FadeOut();
        yield return FadeIn(card);
    }

    private IEnumerator Credits()
    {
        yield return FadeOut();
        _card.fontSize = 40;
        _card.text = "El asedio de Bacatá\n\n<size=26>Una fantasía inspirada en la cosmovisión muisca.\nParentescos, magia y cronología son adaptación dramática, no lección histórica.\n\nFerretware</size>";
        _card.richText = true;
        CampaignProgress.Set(CampaignFlags.CampaignComplete);
        float until = Time.time + 9f;
        while (Time.time < until)
        {
            var k = UnityEngine.InputSystem.Keyboard.current;
            if (Time.time > until - 6f && k != null && (k.eKey.wasPressedThisFrame || k.escapeKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame)) break;
            yield return null;
        }
        SceneLoader.Load(CampaignScenes.Title);
    }

    private static void LoadPlaza()
    {
        WorldTravel.ClearReturn();
        SceneLoader.Load(CampaignScenes.Plaza);
    }

    // ------------------------------------------------------------------ places

    private Transform Block(PrimitiveType type, Vector3 at, Vector3 scale, Color color, Transform parent = null)
    {
        var part = StoryProps.Part(type, parent != null ? parent : _world, Vector3.zero, scale, color);
        part.position = at;
        return part;
    }

    private void BuildVillage()
    {
        Block(PrimitiveType.Plane, Village + new Vector3(0, 0, 10), new Vector3(14, 1, 14), new Color(.42f, .45f, .28f));
        Block(PrimitiveType.Cube, Village + new Vector3(0, .02f, 4), new Vector3(3, .04f, 26), new Color(.55f, .45f, .32f)); // path
        // The zipa's enclosure with its door toward the path (C01).
        if (Model("CasaZipa", Village + new Vector3(0, 0, 21), 180) == null)
        {
            Block(PrimitiveType.Cube, Village + new Vector3(0, 2.5f, 20), new Vector3(10, 5, 6), new Color(.62f, .52f, .38f));
            Block(PrimitiveType.Cube, Village + new Vector3(0, 1.4f, 16.95f), new Vector3(1.8f, 2.8f, .1f), new Color(.25f, .17f, .1f));
            Block(PrimitiveType.Cube, Village + new Vector3(0, 5.6f, 20), new Vector3(11, .5f, 7), new Color(.8f, .66f, .35f));
        }
        // Council circle of standing stones (C02).
        for (int i = 0; i < 8; i++)
        {
            float a = i * Mathf.PI / 4;
            Vector3 at = Village + new Vector3(-14 + Mathf.Cos(a) * 4.5f, 0, 4 + Mathf.Sin(a) * 4.5f);
            if (Model("PiedraVertical" + (1 + i % 3), at, i * 47f) == null)
                Block(PrimitiveType.Cube, at + Vector3.up * .45f, new Vector3(.9f, .9f, .9f), new Color(.5f, .5f, .48f));
        }
        // Round houses with thatched roofs, their doors toward the path; smoke rises from them in the epilogue.
        var houses = new[] { new Vector3(9, 0, 6), new Vector3(12, 0, 15), new Vector3(-9, 0, 15), new Vector3(-20, 0, 12), new Vector3(16, 0, -2), new Vector3(-6, 0, -6), new Vector3(7, 0, -9) };
        for (int i = 0; i < houses.Length; i++)
        {
            Vector3 at = Village + houses[i];
            Vector3 toPath = new Vector3(-houses[i].x, 0, 0);
            float yaw = Quaternion.LookRotation(toPath.sqrMagnitude > .01f ? toPath : Vector3.forward).eulerAngles.y;
            if (Model(i % 2 == 0 ? "Bohio1" : "Bohio2", at, yaw) == null)
            {
                Block(PrimitiveType.Cylinder, at + Vector3.up * 1.2f, new Vector3(4.5f, 1.2f, 4.5f), new Color(.72f, .62f, .48f));
                Block(PrimitiveType.Sphere, at + Vector3.up * 2.9f, new Vector3(5.6f, 2.6f, 5.6f), new Color(.6f, .5f, .28f));
            }
            _smoke.Add(Smoke(at + Vector3.up * 3.6f));
        }
        // The palisade closes the village behind the houses, open toward the camera's side.
        if (Resources.Load<GameObject>("Bacata/Empalizada") != null)
            for (int i = 0; i < 40; i++)
            {
                float a = Mathf.Lerp(-20f, 200f, i / 39f) * Mathf.Deg2Rad;
                Vector3 at = Village + new Vector3(Mathf.Cos(a) * 27, 0, 8 + Mathf.Sin(a) * 27);
                Model("Empalizada", at, -Mathf.Rad2Deg * a);
            }
        for (int i = 0; i < 14; i++)
        {
            float a = i * Mathf.PI * 2 / 14;
            Block(PrimitiveType.Sphere, Village + new Vector3(Mathf.Cos(a) * 48, 6, 10 + Mathf.Sin(a) * 48), new Vector3(22, 14, 22), new Color(.3f, .38f, .26f));
        }
    }

    private void BuildHill()
    {
        Block(PrimitiveType.Sphere, Hill + new Vector3(0, -3.2f, 0), new Vector3(16, 8, 16), new Color(.4f, .44f, .27f));
        // The offering stone where he keeps his vigil (C03), under his knees.
        if (Model("PiedraOfrenda", Hill + new Vector3(0, .74f, 0), 0) == null)
            Block(PrimitiveType.Cylinder, Hill + new Vector3(0, .82f, 0), new Vector3(1.3f, .08f, 1.3f), new Color(.55f, .54f, .5f));
        Block(PrimitiveType.Cube, Hill + new Vector3(0, .3f, -6), new Vector3(1.5f, .05f, 8), new Color(.55f, .45f, .32f));
        // Frailejones on the slopes of the páramo.
        for (int i = 0; i < 9; i++)
        {
            float a = i * 40f * Mathf.Deg2Rad + .3f;
            float r = 3.2f + (i % 3) * .9f;
            float y = Mathf.Sqrt(Mathf.Max(0, 1 - (r * r) / 64f)) * 4f - 3.2f;
            Model("Frailejon", Hill + new Vector3(Mathf.Cos(a) * r, y - .1f, Mathf.Sin(a) * r), i * 33f, "Bacata", .8f + .1f * (i % 4));
        }
    }

    private void BuildRefuge()
    {
        Block(PrimitiveType.Plane, Refuge, new Vector3(3, 1, 3), new Color(.45f, .36f, .26f));
        if (Model("InteriorRefugio", Refuge + new Vector3(0, 0, .5f), 180) == null)
        {
            Block(PrimitiveType.Cube, Refuge + new Vector3(0, 1.5f, 4.5f), new Vector3(9, 3, .3f), new Color(.66f, .56f, .42f));
            Block(PrimitiveType.Cube, Refuge + new Vector3(-4.5f, 1.5f, 0), new Vector3(.3f, 3, 9), new Color(.66f, .56f, .42f));
            Block(PrimitiveType.Cube, Refuge + new Vector3(4.5f, 1.5f, 0), new Vector3(.3f, 3, 9), new Color(.66f, .56f, .42f));
        }
        if (Model("Estera", Refuge + new Vector3(0, .02f, 1), 90) == null)
            Block(PrimitiveType.Cube, Refuge + new Vector3(0, .1f, 1), new Vector3(1.2f, .2f, 2.4f), new Color(.7f, .58f, .35f)); // mat
        Model("Fogon", Refuge + new Vector3(2, 0, -1), 0);
        var fire = new GameObject("Fuego").AddComponent<Light>();
        fire.transform.SetParent(_world, false); fire.transform.position = Refuge + new Vector3(2, .6f, -1);
        fire.type = LightType.Point; fire.color = new Color(1f, .6f, .3f); fire.range = 9; fire.intensity = 3;
    }

    private void BuildLagoon()
    {
        Block(PrimitiveType.Plane, Lagoon, new Vector3(10, 1, 10), new Color(.46f, .5f, .32f));
        Block(PrimitiveType.Cylinder, Lagoon + new Vector3(0, .02f, 0), new Vector3(30, .02f, 22), new Color(.22f, .4f, .48f));
        // Reeds along the water, rocks on the shore and frailejones beyond; the near shore stays
        // open where Tisquesusa stands (the shot looks across it).
        bool reeds = Resources.Load<GameObject>("Bacata/Juncos") != null;
        for (int i = 0; i < 24; i++)
        {
            float a = i * Mathf.PI * 2 / 24;
            Vector3 at = Lagoon + new Vector3(Mathf.Cos(a) * 15.5f, 0, Mathf.Sin(a) * 11.5f);
            bool front = Mathf.Abs(Mathf.Cos(a)) < .35f && Mathf.Sin(a) < 0;
            if (reeds) { if (!front) Model("Juncos", at, i * 51f, "Bacata", .8f + .15f * (i % 3)); }
            else Block(PrimitiveType.Cylinder, at + Vector3.up * .5f, new Vector3(.08f, .5f, .08f), new Color(.45f, .55f, .3f));
        }
        for (int i = 0; i < 10; i++)
        {
            float a = (i * 36f + 12f) * Mathf.Deg2Rad;
            Vector3 shore = Lagoon + new Vector3(Mathf.Cos(a) * 17.5f, 0, Mathf.Sin(a) * 13.5f);
            if (Mathf.Abs(Mathf.Cos(a)) < .3f && Mathf.Sin(a) < 0) continue;
            Model("RocaOrilla" + (1 + i % 3), shore, i * 71f);
            Model("Frailejon", Lagoon + new Vector3(Mathf.Cos(a + .15f) * 21f, 0, Mathf.Sin(a + .15f) * 16.5f), i * 29f, "Bacata", .9f + .1f * (i % 3));
        }
        for (int i = 0; i < 6; i++)
            Block(PrimitiveType.Sphere, Lagoon + new Vector3(-50 + i * 20, 8, 55), new Vector3(34, 26, 24), new Color(.36f, .42f, .4f));
    }

    // Soft, alpha-blended puffs (the shared particle material is additive: dark smoke vanished in it).
    private static Material _smokeMaterial;
    private static Material SmokeMaterial
    {
        get
        {
            if (_smokeMaterial != null) return _smokeMaterial;
            var m = new Material(MIParticles.Material) { name = "Humo" };
            m.SetFloat("_Blend", 0);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            return _smokeMaterial = m;
        }
    }

    // A model of Resources/<folder>/<key> placed on the ground (null when it is missing).
    private Transform Model(string key, Vector3 at, float yaw, string folder = "Bacata", float scale = 1f)
    {
        var prefab = Resources.Load<GameObject>(folder + "/" + key);
        if (prefab == null) return null;
        var go = Instantiate(prefab, _world, false);
        go.name = key;
        go.transform.SetPositionAndRotation(at, Quaternion.Euler(0, yaw, 0));
        go.transform.localScale = Vector3.one * scale;
        return go.transform;
    }

    // A prop in an actor's right (or left) hand; on a provisional figure it rests at its side.
    private Transform Hold(Transform actor, string key, bool right)
    {
        if (actor == null) return null;
        var prop = Model(key, actor.position, 0, "Props");
        if (prop == null) return null;
        Attach(prop, actor, right);
        return prop;
    }

    private static void Attach(Transform prop, Transform actor, bool right)
    {
        var animator = actor.GetComponentInChildren<Animator>();
        var hand = animator != null && animator.isHuman ? animator.GetBoneTransform(right ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand) : null;
        if (hand != null)
        {
            prop.SetParent(hand, false);
            // Held upright through the fist (props stand on their base along +Y).
            prop.localPosition = Vector3.zero;
            prop.localRotation = Quaternion.identity;
            prop.position = hand.position - actor.up * (prop.name == "Baston" ? .75f : .05f);
            prop.rotation = Quaternion.LookRotation(actor.forward, actor.up);
            var s = prop.lossyScale; prop.localScale = new Vector3(prop.localScale.x / s.x, prop.localScale.y / s.y, prop.localScale.z / s.z);
            return;
        }
        prop.SetParent(actor, false);
        prop.localPosition = new Vector3(right ? .35f : -.35f, prop.name == "Baston" ? 0 : .9f, .1f);
        prop.localRotation = Quaternion.identity;
    }

    // The staff passes from one hand to another (C01: the uncle to the heir).
    private IEnumerator Hand(Transform prop, Transform to, float delay)
    {
        if (prop == null || to == null) yield break;
        yield return new WaitForSeconds(delay);
        Attach(prop, to, true);
    }

    private ParticleSystem Smoke(Vector3 at)
    {
        var go = new GameObject("Humo");
        go.transform.SetParent(_world, false); go.transform.position = at; go.transform.rotation = Quaternion.Euler(-90, 0, 0);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main; main.startLifetime = 6; main.startSpeed = 1.2f; main.startSize = 2.5f; main.startColor = new Color(.25f, .23f, .22f, .55f);
        main.playOnAwake = false;
        var emission = ps.emission; emission.rateOverTime = 4;
        var shape = ps.shape; shape.radius = .8f;
        var size = ps.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, .6f, 1, 2.2f));
        var fade = ps.colorOverLifetime; fade.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
            new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, .2f), new GradientAlphaKey(0, 1) });
        fade.color = new ParticleSystem.MinMaxGradient(g);
        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = SmokeMaterial;
        return ps;
    }

    // ------------------------------------------------------------------ cast

    private void BuildCast()
    {
        _nemequene = Actor("Nemequene", nemequenePrefab, new Color(.7f, .55f, .3f), new Color(.85f, .7f, .3f), 1.78f);
        _child = Actor("Nemequene niño", null, new Color(.7f, .58f, .38f), new Color(.85f, .7f, .3f), 1.2f, "Nemequene");
        _saguanmachica = Actor("Saguanmachica", saguanmachicaPrefab, new Color(.5f, .36f, .22f), new Color(.9f, .75f, .35f), 1.7f);
        _tisquesusa = Actor("Tisquesusa", tisquesusaPrefab, new Color(.86f, .8f, .66f), new Color(.75f, .35f, .2f), 1.72f);
        _bachue = Actor("Bachué", bachuePrefab, new Color(.88f, .84f, .74f), new Color(.2f, .55f, .45f), 1.72f);
        _furachogua = Actor("Furachogua", furachoguaPrefab, new Color(.3f, .52f, .55f), new Color(.85f, .82f, .7f), 1.72f);
        _invader = Actor("Invasor", null, new Color(.78f, .7f, .4f), new Color(.82f, .84f, .88f), 1.82f, "Invasor de oro y plata");
        foreach (var a in new[] { _nemequene, _child, _saguanmachica, _tisquesusa, _bachue, _furachogua, _invader }) Hide(a);
    }

    private Transform Actor(string name, GameObject prefab, Color robe, Color accent, float height, string speaker = null)
    {
        Transform actor;
        var model = prefab != null ? prefab : Resources.Load<GameObject>("Characters/" + name.Replace(" ", "").Replace("é", "e"));
        if (model != null)
        {
            actor = new GameObject(name).transform; actor.SetParent(_world, false);
            Instantiate(model, actor, false);
        }
        else actor = StoryProps.Figure(name, _world, Vector3.zero, robe, accent, height);
        StoryActor.Ensure(actor.gameObject, speaker ?? name, height * .9f);
        return actor;
    }
}
