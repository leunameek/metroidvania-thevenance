using System.Collections;
using Nemequene.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Every scene change of the game goes through here (screen 07 «Carga»): the destination's
// illustration and name, a short tip and the real progress of the operation, kept on screen
// until the new scene has run its first frames (the worlds build pieces at runtime), so a
// change of place never looks like a frozen game.
public sealed class SceneLoader : MonoBehaviour
{
    private const float FadeIn = .2f, FadeOut = .45f;
    private static SceneLoader _instance;
    public static bool Loading => _instance != null;

    private static readonly string[] Tips =
    {
        "Di «examinar» cerca de un objeto para usarlo; «siguiente» avanza los diálogos.",
        "En los duelos por turnos puedes decir la acción en español o en inglés: «atacar» o «attack».",
        "Con la cámara activa (C), la mano izquierda abierta gira la pieza y la derecha la inclina.",
        "Cierra los dos puños para detener el giro de una pieza; sostén el puño para tomarla.",
        "Mantén Esc durante un segundo para saltar un diálogo: cuenta como si lo hubieras escuchado.",
    };

    private CanvasGroup _group;
    private Image _progress;
    private TMP_Text _status;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => _instance = null;

    // Replaces SceneManager.LoadScene(path) for gameplay scene changes.
    public static void Load(string path)
    {
        if (_instance != null || string.IsNullOrEmpty(path)) return;
        if (Application.isBatchMode || !Application.isPlaying) { SceneManager.LoadScene(path); return; }
        var go = new GameObject("SceneLoader");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<SceneLoader>();
        _instance.Build(path);
        _instance.StartCoroutine(_instance.Run(path));
    }

    private void Build(string path)
    {
        Describe(path, out string place, out string art);
        var canvas = UIKit.ScreenCanvas(transform, "LoadingCanvas", 5000);
        canvas.gameObject.AddComponent<GraphicRaycaster>();
        _group = canvas.gameObject.AddComponent<CanvasGroup>();
        _group.alpha = 0; _group.blocksRaycasts = true;
        var black = UIKit.Rect("Background", canvas).gameObject.AddComponent<Image>();
        black.color = new Color(.031f, .039f, .043f, 1);
        var texture = UIBacata.Art(art);
        if (texture != null)
        {
            var holder = UIKit.Rect("Illustration", canvas);
            var raw = holder.gameObject.AddComponent<RawImage>(); raw.texture = texture; raw.raycastTarget = false;
            raw.color = new Color(.78f, .78f, .78f, 1);
            var fit = holder.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent; fit.aspectRatio = (float)texture.width / texture.height;
        }
        var panel = UIKit.Place(UIKit.HudPanel(canvas, "Panel"), new Vector2(.5f, 0), new Vector2(0, 72), new Vector2(1180, 230));
        var title = UIKit.Label(panel, place, 56, UIPalette.GoldLight, true);
        title.rectTransform.anchorMin = new Vector2(.04f, .56f); title.rectTransform.anchorMax = new Vector2(.96f, .94f);
        var tip = UIKit.Label(panel, Tips[Random.Range(0, Tips.Length)], 22, UIPalette.Ivory);
        tip.rectTransform.anchorMin = new Vector2(.06f, .30f); tip.rectTransform.anchorMax = new Vector2(.94f, .56f);
        var rail = UIKit.Rect("Rail", panel); rail.anchorMin = new Vector2(.06f, .12f); rail.anchorMax = new Vector2(.74f, .20f);
        _progress = UIKit.Bar(rail, new Vector2(800, 12), UIPalette.GoldLight);
        _progress.fillAmount = 0;
        _status = UIKit.Label(panel, "Cargando…", 20, UIPalette.Muted);
        _status.alignment = TextAlignmentOptions.MidlineRight;
        _status.rectTransform.anchorMin = new Vector2(.74f, .06f); _status.rectTransform.anchorMax = new Vector2(.94f, .26f);
    }

    private IEnumerator Run(string path)
    {
        // The old scene stops under the curtain: nothing moves or reacts while it closes.
        float scale = Time.timeScale;
        Time.timeScale = 0;
        for (float t = 0; t < FadeIn; t += Time.unscaledDeltaTime) { _group.alpha = t / FadeIn; yield return null; }
        _group.alpha = 1;
        yield return null;
        AsyncOperation operation = null;
        try { operation = SceneManager.LoadSceneAsync(path, LoadSceneMode.Single); }
        catch (System.Exception e) { Debug.LogWarning("[SceneLoader] " + e.Message); }
        if (operation == null)
        {
            Time.timeScale = scale; Finish(); yield break;
        }
        operation.allowSceneActivation = false;
        while (operation.progress < .9f) { _progress.fillAmount = operation.progress / .9f * .85f; yield return null; }
        _progress.fillAmount = .85f;
        Time.timeScale = scale > 0 ? scale : 1;
        operation.allowSceneActivation = true;
        while (!operation.isDone) yield return null;
        // The new scene builds its runtime pieces in Start and its first frames.
        _status.text = "Preparando…";
        for (int i = 0; i < 4; i++) { _progress.fillAmount = .85f + .15f * (i + 1) / 4f; yield return null; }
        _status.text = "";
        for (float t = 0; t < FadeOut; t += Time.unscaledDeltaTime) { _group.alpha = 1 - t / FadeOut; yield return null; }
        Finish();
    }

    private void Finish()
    {
        if (_instance == this) _instance = null;
        Destroy(gameObject);
    }

    private static void Describe(string path, out string place, out string art)
    {
        string name = System.IO.Path.GetFileNameWithoutExtension(path ?? "");
        switch (name)
        {
            case "MundoInferior": place = "Mundo inferior"; art = "Mundo_Inferior"; break;
            case "MundoSuperior": place = "Mundo superior"; art = "Mundo_Superior"; break;
            case "Bacata": place = "Bacatá"; art = "Mapa_Bacata"; break;
            case "MainMenu": place = "El asedio de Bacatá"; art = "Portada_Bacata"; break;
            default: place = "Plaza Núñez"; art = "Plaza_Nunez"; break;
        }
    }
}
