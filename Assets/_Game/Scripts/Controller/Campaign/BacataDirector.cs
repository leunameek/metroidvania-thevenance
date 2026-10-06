using System;
using System.Collections;
using System.Collections.Generic;
using Nemequene.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Prologue (H01-H03 / C01-C03) and epilogue (H19-H21 / C19-C21) of the campaign, staged as
// cinematics in four places built at runtime: Bacatá, the meditation hill, the refuge of Tunja and
// the lagoon of Iguaque. Fixed camera shots, the story camera frames whoever speaks, fades between
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
        yield return FadeIn("Bacatá");
        // C01: the staff carved by the uncle; the adult heir receives it.
        Place(_child, Village + new Vector3(2.2f, 0, 13), Village + new Vector3(0, 0, 13));
        Place(_saguanmachica, Village + new Vector3(-1.2f, 0, 13), Village + new Vector3(2, 0, 13));
        Hide(_nemequene); Hide(_tisquesusa); Hide(_bachue); Hide(_furachogua); Hide(_invader);
        Shot(Village + new Vector3(0.5f, 1.6f, 8.5f), Village + new Vector3(0.5f, 1.1f, 13));
        yield return Lines("H01", 0, 3, false, "Dos extremos");
        yield return Cut();
        Hide(_child);
        Place(_nemequene, Village + new Vector3(1.6f, 0, 12.5f), Village + new Vector3(-1.2f, 0, 13));
        Shot(Village + new Vector3(4f, 2f, 7f), Village + new Vector3(0, 1.2f, 13));
        yield return Lines("H01", 3, 2, true);
        // C02: the vision of gold and silver, before the council.
        yield return Cut();
        Hide(_saguanmachica);
        Place(_tisquesusa, Village + new Vector3(-14, 0, 6.5f), Village + new Vector3(-14, 0, 2));
        Place(_nemequene, Village + new Vector3(-14, 0, 2), Village + new Vector3(-14, 0, 6.5f));
        Shot(Village + new Vector3(-8.5f, 2.2f, 1.5f), Village + new Vector3(-14, 1.3f, 4.2f));
        yield return Lines("H02", 0, 99, true, "Oro y plata");
        // C03: seven days and seven nights on the hill; Bachué comes along the same path.
        yield return Cut();
        Hide(_tisquesusa);
        Place(_nemequene, Hill + new Vector3(0, .9f, 0), Hill + new Vector3(0, .9f, -6));
        Shot(Hill + new Vector3(7, 3.5f, -7), Hill + new Vector3(0, 1.5f, 0));
        yield return Days(3);
        Place(_bachue, Hill + new Vector3(0, .9f, -4.5f), Hill + new Vector3(0, .9f, 0));
        yield return Lines("H03", 0, 99, true, "Siete días y siete noches");
        // The poporo and the map are received; the journey to the plaza is elided (C04).
        yield return FadeOut();
        _card.text = "";
        LoadPlaza();
    }

    private IEnumerator Epilogue()
    {
        SetSky(true);
        foreach (var s in _smoke) s.Play();
        Hide(_child); Hide(_bachue); Hide(_furachogua);
        // C19: the same path, now smoke. The uncle defends the passage and dies; the invader aims.
        Place(_saguanmachica, Village + new Vector3(-.8f, 0, 12), Village + new Vector3(0, 0, 8));
        Place(_nemequene, Village + new Vector3(.6f, 0, 9), Village + new Vector3(-.8f, 0, 12));
        Place(_tisquesusa, Village + new Vector3(-6, 0, 4), Village + new Vector3(0, 0, 9));
        Place(_invader, Village + new Vector3(9, 0, 17), Village + new Vector3(.6f, 0, 9));
        Hide(_invader);
        yield return FadeIn("La verdadera imagen");
        Shot(Village + new Vector3(4, 2.2f, 3.5f), Village + new Vector3(0, 1.1f, 10.5f));
        yield return Lines("H19", 0, 2, false);
        yield return Fall(_saguanmachica, 1.2f);
        Show(_invader);
        Shot(Village + new Vector3(-2.5f, 2f, 6.5f), Village + new Vector3(7, 1.6f, 15));
        yield return Lines("H19", 2, 1, false);
        yield return Arrow(_invader.position + Vector3.up * 1.5f, _nemequene.position + Vector3.up * 1.3f);
        yield return Fall(_nemequene, .8f);
        Shot(Village + new Vector3(-3.5f, 2f, 3f), Village + new Vector3(0, .8f, 9));
        yield return Lines("H19", 3, 1, true);
        // C20: the refuge of Tunja, the journey elided. Last words; the staff to the nephew.
        yield return Cut("Refugio de Tunja");
        foreach (var s in _smoke) s.Stop();
        Hide(_saguanmachica); Hide(_invader);
        Place(_nemequene, Refuge + new Vector3(0, .35f, 1), Refuge + new Vector3(0, .35f, 4));
        _nemequene.rotation = Quaternion.Euler(-90, 90, 0);
        Place(_tisquesusa, Refuge + new Vector3(-1.3f, 0, 1.2f), Refuge + new Vector3(0, 0, 1));
        Shot(Refuge + new Vector3(2.8f, 2.1f, -2.2f), Refuge + new Vector3(-.4f, .6f, 1.1f));
        yield return Lines("H20", 0, 99, true, "Transmitir antes de morir");
        // C21: Iguaque. Furachogua brings the masks kept by Bachué; the staff is laid down and taken up.
        yield return Cut("Iguaque");
        Hide(_nemequene);
        SetSky(false);
        Place(_tisquesusa, Lagoon + new Vector3(0, 0, -15.5f), Lagoon + new Vector3(0, 0, -8));
        Place(_furachogua, Lagoon + new Vector3(0, -1.6f, -11), Lagoon + new Vector3(0, 0, -15.5f));
        Shot(Lagoon + new Vector3(5.5f, 2.6f, -20f), Lagoon + new Vector3(0, 1f, -12.5f));
        yield return Rise(_furachogua, 1.6f, 2.5f);
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

    private IEnumerator Fall(Transform actor, float seconds)
    {
        if (actor == null) yield break;
        Quaternion start = actor.rotation, end = actor.rotation * Quaternion.Euler(-85, 0, 0);
        for (float t = 0; t < seconds; t += Time.deltaTime) { actor.rotation = Quaternion.Slerp(start, end, t / seconds); yield return null; }
        actor.rotation = end;
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
        var arrow = StoryProps.Part(PrimitiveType.Cylinder, _world, from, new Vector3(.04f, .45f, .04f), new Color(.75f, .72f, .6f));
        arrow.rotation = Quaternion.FromToRotation(Vector3.up, to - from);
        for (float t = 0; t < .45f; t += Time.deltaTime) { arrow.position = Vector3.Lerp(from, to, t / .45f); yield return null; }
        arrow.position = to;
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
            yield return null;
        }
        sun.transform.rotation = rest; sun.color = color; sun.intensity = 1.1f;
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
        // Temple of the training with its door (C01).
        Block(PrimitiveType.Cube, Village + new Vector3(0, 2.5f, 20), new Vector3(10, 5, 6), new Color(.62f, .52f, .38f));
        Block(PrimitiveType.Cube, Village + new Vector3(0, 1.4f, 16.95f), new Vector3(1.8f, 2.8f, .1f), new Color(.25f, .17f, .1f));
        Block(PrimitiveType.Cube, Village + new Vector3(0, 5.6f, 20), new Vector3(11, .5f, 7), new Color(.8f, .66f, .35f));
        // Council circle (C02).
        for (int i = 0; i < 8; i++)
        {
            float a = i * Mathf.PI / 4;
            Block(PrimitiveType.Cube, Village + new Vector3(-14 + Mathf.Cos(a) * 4.5f, .45f, 4 + Mathf.Sin(a) * 4.5f), new Vector3(.9f, .9f, .9f), new Color(.5f, .5f, .48f));
        }
        // Round houses with thatched roofs; smoke rises from them in the epilogue.
        var houses = new[] { new Vector3(9, 0, 6), new Vector3(12, 0, 15), new Vector3(-9, 0, 15), new Vector3(-20, 0, 12), new Vector3(16, 0, -2), new Vector3(-6, 0, -6), new Vector3(7, 0, -9) };
        foreach (var h in houses)
        {
            Vector3 at = Village + h;
            Block(PrimitiveType.Cylinder, at + Vector3.up * 1.2f, new Vector3(4.5f, 1.2f, 4.5f), new Color(.72f, .62f, .48f));
            Block(PrimitiveType.Sphere, at + Vector3.up * 2.9f, new Vector3(5.6f, 2.6f, 5.6f), new Color(.6f, .5f, .28f));
            _smoke.Add(Smoke(at + Vector3.up * 3.6f));
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
        Block(PrimitiveType.Cylinder, Hill + new Vector3(0, .82f, 0), new Vector3(1.3f, .08f, 1.3f), new Color(.55f, .54f, .5f));
        Block(PrimitiveType.Cube, Hill + new Vector3(0, .3f, -6), new Vector3(1.5f, .05f, 8), new Color(.55f, .45f, .32f));
    }

    private void BuildRefuge()
    {
        Block(PrimitiveType.Plane, Refuge, new Vector3(3, 1, 3), new Color(.45f, .36f, .26f));
        Block(PrimitiveType.Cube, Refuge + new Vector3(0, 1.5f, 4.5f), new Vector3(9, 3, .3f), new Color(.66f, .56f, .42f));
        Block(PrimitiveType.Cube, Refuge + new Vector3(-4.5f, 1.5f, 0), new Vector3(.3f, 3, 9), new Color(.66f, .56f, .42f));
        Block(PrimitiveType.Cube, Refuge + new Vector3(4.5f, 1.5f, 0), new Vector3(.3f, 3, 9), new Color(.66f, .56f, .42f));
        Block(PrimitiveType.Cube, Refuge + new Vector3(0, .1f, 1), new Vector3(1.2f, .2f, 2.4f), new Color(.7f, .58f, .35f)); // mat
        var fire = new GameObject("Fuego").AddComponent<Light>();
        fire.transform.SetParent(_world, false); fire.transform.position = Refuge + new Vector3(2, .6f, -1);
        fire.type = LightType.Point; fire.color = new Color(1f, .6f, .3f); fire.range = 9; fire.intensity = 3;
    }

    private void BuildLagoon()
    {
        Block(PrimitiveType.Plane, Lagoon, new Vector3(10, 1, 10), new Color(.46f, .5f, .32f));
        Block(PrimitiveType.Cylinder, Lagoon + new Vector3(0, .02f, 0), new Vector3(30, .02f, 22), new Color(.22f, .4f, .48f));
        for (int i = 0; i < 24; i++)
        {
            float a = i * Mathf.PI * 2 / 24;
            Block(PrimitiveType.Cylinder, Lagoon + new Vector3(Mathf.Cos(a) * 15.5f, .5f, Mathf.Sin(a) * 11.5f), new Vector3(.08f, .5f, .08f), new Color(.45f, .55f, .3f));
        }
        for (int i = 0; i < 6; i++)
            Block(PrimitiveType.Sphere, Lagoon + new Vector3(-50 + i * 20, 8, 55), new Vector3(34, 26, 24), new Color(.36f, .42f, .4f));
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
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader != null) renderer.material = new Material(shader) { color = new Color(.25f, .23f, .22f, .55f) };
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
