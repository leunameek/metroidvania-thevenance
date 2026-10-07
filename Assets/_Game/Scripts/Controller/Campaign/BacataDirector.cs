using System;
using System.Collections;
using System.Collections.Generic;
using Nemequene.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Prologue (H01-H03 / C01-C03) and epilogue (H19-H21 / C19-C21) of the campaign, staged as
// cinematics in four places built at runtime: Bacatá, the meditation hill, the refuge of Tunja and
// the lagoon of Iguaque. The open places stand on painted ground with instanced grass, swaying
// plants, still water and the Andean sky (Scripts/View/Nature). The places use the models of
// Resources/Bacata (BacataModelSetup) and keep a block where one is missing; the staff, the poporo, the bow and the arrow are Resources/Props. Fixed camera shots, the story camera frames whoever speaks, fades between
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
        _forest.Build(_world);
        BuildCast();
        bool epilogue = CampaignScenes.NextBacataMode == CampaignScenes.BacataMode.Epilogue
            || CampaignProgress.Has(CampaignFlags.MasksInCustody) && !CampaignProgress.Has(CampaignFlags.CampaignComplete);
        CampaignScenes.NextBacataMode = CampaignScenes.BacataMode.Prologue;
        StartCoroutine(epilogue ? Epilogue() : Prologue());
    }

    // ------------------------------------------------------------------ sequences

    private IEnumerator Prologue()
    {
        SetSky(NatureAtmosphere.Morning);
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
        SetSky(NatureAtmosphere.Smoke);
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
        Place(_nemequene, Refuge + new Vector3(0, .06f, 1), Refuge + new Vector3(0, .06f, 4));
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
        SetSky(NatureAtmosphere.Paramo);
        // C21: water and wind at the lagoon; the resolved motif.
        Sound("amb_laguna", "musica_legado", .85f);
        Destroy(hearth);
        AmbientScatter.On(gameObject).Add("Ambiente/ave_canto", 6f, 14f, 10f, 24f, .35f, 4f);
        Place(_tisquesusa, _lagoonGround.On(Lagoon + new Vector3(0, 0, -15.5f)), Lagoon + new Vector3(0, 0, -8));
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
        sun.transform.rotation = rest; sun.color = color; sun.intensity = _sunIntensity;
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

    // Sky, haze, light and grade of the place (morning savanna, burning village, clear páramo).
    private void SetSky(NatureAtmosphere.Look look)
    {
        // Morning sun from behind the cameras' right: faces lit, long soft shadows to the left.
        if (sun != null) sun.transform.rotation = Quaternion.Euler(38, -55, 0);
        NatureAtmosphere.Apply(look, sun, _camera);
        _sunIntensity = look.SunIntensity;
    }

    private float _sunIntensity = 1.3f;

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

    // ------------------------------------------------------------------ the land

    private NatureGround _savanna, _lagoonGround;
    private readonly NatureTrees.Forest _forest = new NatureTrees.Forest();

    // The savanna of Bacatá and the meditation hill share one ground: flat where the scenes are
    // acted, a low mound under the offering stone, rolling pasture beyond the palisade and green
    // cerros closing the horizon (the procedural sky adds the far ranges behind them).
    private static float SavannaHeight(float x, float z)
    {
        float village = Vector2.Distance(new Vector2(x, z), new Vector2(Village.x, Village.z + 8));
        float hill = Vector2.Distance(new Vector2(x, z), new Vector2(Hill.x, Hill.z));
        float centre = Vector2.Distance(new Vector2(x, z), new Vector2(15, 20));
        float flat = Mathf.Max(1 - NatureGround.Smooth(26, 58, village), 1 - NatureGround.Smooth(13, 30, hill));
        float rolling = NatureGround.Rolling(x, z, .016f, 3.1f) * 12f - 2f;
        float mound = .8f * (1 - NatureGround.Smooth(4.5f, 13f, hill));
        float cerros = NatureGround.Smooth(95, 175, centre) * (14 + NatureGround.Rolling(x, z, .009f, 7.7f) * 34);
        return mound + (1 - flat) * rolling + cerros;
    }

    // Iguaque: a still basin in the páramo, a low rim of mud and stones, hills rising behind the
    // water (north, where the shot looks) and high cerros all around.
    private const float LagoonHalfX = 16.5f, LagoonHalfZ = 13f, LagoonLevel = -.05f;
    private static float LagoonHeight(float x, float z)
    {
        float lx = x - Lagoon.x, lz = z - Lagoon.z;
        float e = Mathf.Sqrt(lx * lx / (LagoonHalfX * LagoonHalfX) + lz * lz / (LagoonHalfZ * LagoonHalfZ));
        float basin = e < 1 ? -2.4f * Mathf.Pow(1 - e * e, .6f) : 0;
        float rim = .1f * NatureGround.Smooth(1, 1.35f, e);
        float d = Mathf.Sqrt(lx * lx + lz * lz);
        float north = Mathf.Clamp01(lz / Mathf.Max(d, 1f) * .5f + .5f);
        float hills = NatureGround.Smooth(26, 80, d) * (NatureGround.Rolling(x, z, .02f, 11f) * 12 + 4) * (.35f + .65f * north);
        float cerros = NatureGround.Smooth(90, 170, d) * (20 + NatureGround.Rolling(x, z, .008f, 4.2f) * 40);
        return basin + rim + hills + cerros;
    }

    // Trees and bushes of the savanna, grown from code (NatureTrees); a species without a
    // generator falls back to its model (Art/Environments/Bacatá, built by BacataModelSetup).
    private static readonly string[] GroveTrees = { "Aliso", "Aliso", "Roble", "Encenillo" };
    private static readonly string[] Bushes = { "Chilco", "Mortino" };

    private void BuildVillage()
    {
        _savanna = new NatureGround(SavannaHeight);
        var g = _savanna;
        // The path through the village to the zipa's door, the trail to the hill and its climb.
        g.Path(Village + new Vector3(0, 0, -14), Village + new Vector3(0, 0, 17), 2.6f);
        g.Path(Village + new Vector3(1, 0, 1), Village + new Vector3(14, 0, 4), 1.4f, .75f);
        g.Path(Village + new Vector3(14, 0, 4), Hill + new Vector3(0, 0, -11), 1.4f, .75f);
        g.Path(Hill + new Vector3(0, 0, -11), Hill + new Vector3(0, 0, -1), 1.5f, .9f);
        g.Path(Village + new Vector3(-1, 0, 4), Village + new Vector3(-12, 0, 4), 1.3f, .7f);
        g.Clearing(Village + new Vector3(0, 0, 13), 7.5f);
        g.Clearing(Village + new Vector3(-14, 0, 4), 6.2f, .9f);
        g.Clearing(Hill, 2.4f, .55f);
        g.Bare(Village + new Vector3(0, 0, 21), 6.5f);
        var houses = new[] { new Vector3(9, 0, 6), new Vector3(12, 0, 15), new Vector3(-9, 0, 15), new Vector3(-20, 0, 12), new Vector3(16, 0, -2), new Vector3(-6, 0, -6), new Vector3(7, 0, -9) };
        foreach (var h in houses) { g.Clearing(Village + h, 3.8f, .75f); g.Bare(Village + h, 2.8f); }
        g.Build(_world, "Sabana", new Vector3(15, 0, 20), 190f, NatureKit.Savanna);

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
                Model("Empalizada", g.On(at), -Mathf.Rad2Deg * a);
            }

        // Pasture all around, thinning out with distance (the painted ground carries on beyond),
        // and wild flowers among it.
        var centre = Village + new Vector3(0, 0, 8);
        float Meadow(float x, float z) => g.Grassy(x, z) && Vector2.Distance(new Vector2(x, z), new Vector2(Hill.x, Hill.z)) > 19
            ? 1 - NatureGround.Smooth(30, 48, Vector2.Distance(new Vector2(x, z), new Vector2(centre.x, centre.z))) * .8f : 0;
        Grass("Pasto", NatureKit.Clump(8, .2f, .46f, .2f, .065f, 11), NatureKit.Savanna,
            NatureGrass.Scatter(centre, 48, .52f, Meadow, g.Height, new Vector2(.8f, 1.35f), 101), .38f);
        Grass("Flores", NatureKit.Flowers(4, .18f, .34f, .25f, .045f, 12), NatureKit.Savanna,
            NatureGrass.Scatter(centre, 40, 1.5f, (x, z) => Meadow(x, z) * .4f, g.Height, new Vector2(.8f, 1.2f), 102), .3f);

        // Groves of alisos and robles on the pasture beyond the palisade, bushes along the houses.
        var random = new System.Random(7);
        float R() => (float)random.NextDouble();
        for (int i = 0; i < 46; i++)
        {
            float a = R() * Mathf.PI * 2, r = 31 + R() * 42;
            var at = centre + new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r);
            if (Vector3.Distance(at, Hill) < 17 || g.Dirt(at.x, at.z) > .1f) continue;
            Plant(GroveTrees[i % GroveTrees.Length], g.On(at), R() * 360, .8f + R() * .45f, .045f);
        }
        for (int i = 0; i < 26; i++)
        {
            float a = R() * Mathf.PI * 2, r = 9 + R() * 17;
            var at = centre + new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r);
            if (!g.Grassy(at.x, at.z)) continue;
            Plant(Bushes[i % Bushes.Length], g.On(at), R() * 360, .8f + R() * .5f, .1f);
        }
        Plant("ManoDeOso", Village + new Vector3(13.5f, 0, 9.5f), 40, 1f, .06f);
        Plant("ManoDeOso", Village + new Vector3(-12.5f, 0, 18.5f), 200, .9f, .06f);
    }

    private void BuildHill()
    {
        var g = _savanna;
        // The offering stone where he keeps his vigil (C03), under his knees, on top of the mound.
        if (Model("PiedraOfrenda", Hill + new Vector3(0, .74f, 0), 0) == null)
            Block(PrimitiveType.Cylinder, Hill + new Vector3(0, .82f, 0), new Vector3(1.3f, .08f, 1.3f), new Color(.55f, .54f, .5f));
        // Frailejones and straw of the páramo on its slopes.
        for (int i = 0; i < 9; i++)
        {
            float a = i * 40f * Mathf.Deg2Rad + .3f;
            float r = 3.2f + (i % 3) * .9f;
            var plant = Model("Frailejon", g.On(Hill + new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r)) - Vector3.up * .05f, i * 33f, "Bacata", .8f + .1f * (i % 4));
            NatureFoliage.Sway(plant, .03f, .006f);
        }
        float Straw(float x, float z) => g.Grassy(x, z) ? 1 - NatureGround.Smooth(12, 19, Vector2.Distance(new Vector2(x, z), new Vector2(Hill.x, Hill.z))) : 0;
        Grass("Pajonal colina", NatureKit.Clump(13, .4f, .8f, .12f, .035f, 21), NatureKit.Paramo,
            NatureGrass.Scatter(Hill, 19, .75f, Straw, g.Height, new Vector2(.75f, 1.25f), 103), .5f);
        Plant("Encenillo", g.On(Hill + new Vector3(-7.5f, 0, 5)), 30, 1f, .045f);
        Plant("Chusque", g.On(Hill + new Vector3(6.5f, 0, 6)), 120, 1f, .12f);
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
        _lagoonGround = new NatureGround(LagoonHeight) { ShoreLevel = LagoonLevel };
        var g = _lagoonGround;
        // The trail Tisquesusa comes down and the trodden bank where he waits (C21).
        g.Path(Lagoon + new Vector3(0, 0, -45), Lagoon + new Vector3(0, 0, -15), 1.6f, .7f);
        g.Clearing(Lagoon + new Vector3(0, 0, -16), 3f, .5f);
        g.Build(_world, "Paramo", Lagoon, 190f, NatureKit.Paramo);

        // The water: a sheet a little wider than the basin; the rim of the ground draws the shore.
        var water = new GameObject("Laguna");
        water.transform.SetParent(_world, false);
        water.transform.position = Lagoon + Vector3.up * LagoonLevel;
        water.AddComponent<MeshFilter>().sharedMesh = NatureKit.Ellipse(LagoonHalfX * 1.25f, LagoonHalfZ * 1.25f, 32, 128);
        var material = NatureKit.Water();
        if (material != null) water.AddComponent<MeshRenderer>().sharedMaterial = material;
        else Block(PrimitiveType.Cylinder, Lagoon + new Vector3(0, .02f, 0), new Vector3(30, .02f, 22), new Color(.22f, .4f, .48f));

        // Reeds standing in the shallows, rocks on the shore and frailejones beyond; the near shore
        // stays open where Tisquesusa stands (the shot looks across it).
        bool reeds = Resources.Load<GameObject>("Bacata/Juncos") != null;
        for (int i = 0; i < 24; i++)
        {
            float a = i * Mathf.PI * 2 / 24;
            Vector3 at = Lagoon + new Vector3(Mathf.Cos(a) * 15.5f, 0, Mathf.Sin(a) * 11.5f);
            bool front = Mathf.Abs(Mathf.Cos(a)) < .35f && Mathf.Sin(a) < 0;
            if (front) continue;
            if (reeds) NatureFoliage.Sway(Model("Juncos", g.On(at), i * 51f, "Bacata", .8f + .15f * (i % 3)), .09f, .01f);
            else Block(PrimitiveType.Cylinder, g.On(at) + Vector3.up * .5f, new Vector3(.08f, .5f, .08f), new Color(.45f, .55f, .3f));
        }
        for (int i = 0; i < 10; i++)
        {
            float a = (i * 36f + 12f) * Mathf.Deg2Rad;
            Vector3 shore = Lagoon + new Vector3(Mathf.Cos(a) * 17.5f, 0, Mathf.Sin(a) * 13.5f);
            if (Mathf.Abs(Mathf.Cos(a)) < .3f && Mathf.Sin(a) < 0) continue;
            Model("RocaOrilla" + (1 + i % 3), g.On(shore) - Vector3.up * .15f, i * 71f);
            g.Bare(shore, 1.2f);
            var plant = Model("Frailejon", g.On(Lagoon + new Vector3(Mathf.Cos(a + .15f) * 21f, 0, Mathf.Sin(a + .15f) * 16.5f)) - Vector3.up * .05f, i * 29f, "Bacata", .9f + .1f * (i % 3));
            NatureFoliage.Sway(plant, .03f, .006f);
        }
        // More frailejones scattered up the slopes behind the water.
        var random = new System.Random(19);
        float R() => (float)random.NextDouble();
        for (int i = 0; i < 40; i++)
        {
            float a = R() * Mathf.PI * 2, r = 24 + R() * 40;
            var at = Lagoon + new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r);
            if (g.Dirt(at.x, at.z) > .1f) continue;
            NatureFoliage.Sway(Model("Frailejon", g.On(at) - Vector3.up * .05f, R() * 360, "Bacata", .8f + R() * .5f), .03f, .006f);
        }
        Plant("Sauce", g.On(Lagoon + new Vector3(-19, 0, 8)), 20, 1f, .05f);
        Plant("Chusque", g.On(Lagoon + new Vector3(18, 0, 9)), 200, 1f, .12f);
        Plant("Chusque", g.On(Lagoon + new Vector3(-21, 0, -6)), 80, .9f, .12f);

        // Golden straw of the páramo (pajonal) from the bank up the slopes, and a few flowers.
        float Straw(float x, float z) => g.Grassy(x, z) ? 1 - NatureGround.Smooth(36, 52, Vector2.Distance(new Vector2(x, z), new Vector2(Lagoon.x, Lagoon.z))) : 0;
        Grass("Pajonal", NatureKit.Clump(13, .4f, .85f, .12f, .035f, 31), NatureKit.Paramo,
            NatureGrass.Scatter(Lagoon, 52, .62f, Straw, g.Height, new Vector2(.75f, 1.3f), 104), .5f);
        Grass("Pasto orilla", NatureKit.Clump(8, .15f, .32f, .2f, .06f, 32), NatureKit.Paramo,
            NatureGrass.Scatter(Lagoon, 30, .55f, (x, z) => Straw(x, z) * .8f, g.Height, new Vector2(.8f, 1.2f), 105), .3f);
        Grass("Flores paramo", NatureKit.Flowers(3, .15f, .28f, .2f, .04f, 33), NatureKit.Paramo,
            NatureGrass.Scatter(Lagoon, 40, 2.2f, (x, z) => Straw(x, z) * .35f, g.Height, new Vector2(.8f, 1.2f), 106), .3f);
    }

    // A grass or flower field with its own material on the given palette.
    private void Grass(string name, Mesh mesh, NatureKit.Palette palette, List<Matrix4x4> instances, float wind)
    {
        var material = NatureKit.Grass();
        if (material == null || instances.Count == 0) return;
        NatureKit.Apply(material, palette);
        material.SetFloat("_WindStrength", wind);
        NatureGrass.Create(_world, name, mesh, material, instances);
    }

    // A tree or bush grown from code and drawn with the others of its kind; otherwise its model
    // from Resources/Bacata, planted and swaying (nothing when the model is missing).
    private void Plant(string key, Vector3 at, float yaw, float scale, float wind)
    {
        if (_forest.Add(key, at - Vector3.up * .05f, yaw, scale)) return;
        var plant = Model(key, at - Vector3.up * .08f, yaw, "Bacata", scale);
        if (plant != null) NatureFoliage.Sway(plant, wind, .02f);
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
