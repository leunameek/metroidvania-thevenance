using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public sealed class TechnicalDemoHUD : MonoBehaviour
{
    [SerializeField] private TechnicalDemoController demo;
    private GameObject _canvas, _detailPanel, _helpPanel;
    private Text _title, _objectives, _prompt, _detail, _camera, _message, _controls;
    private Image _progress, _fade;
    private GameObject _previewPanel;
    private RawImage _preview;
    private Font _font;
    private readonly Color _ink = new Color(0.045f, 0.10f, 0.11f, 0.96f);
    private readonly Color _cream = new Color(0.94f, 0.92f, 0.81f);
    private readonly Color _gold = new Color(0.86f, 0.69f, 0.33f);

    private void Start()
    {
        if (demo == null) { enabled = false; return; }
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _canvas = new GameObject("UI_PlazaNunez", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        _canvas.transform.SetParent(transform, false);
        _canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.GetComponent<Canvas>().sortingOrder = 100;
        var scaler = _canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1440, 900);
        scaler.matchWidthOrHeight = 0.5f;
        var header = Panel("Identity", _canvas.transform, new Vector2(28, -28), new Vector2(345, 84));
        _title = Label(header.transform, "Title", new Vector2(20, -13), new Vector2(310, 58), 25);
        var tasks = Panel("Journey", _canvas.transform, new Vector2(28, -126), new Vector2(345, 157));
        _objectives = Label(tasks.transform, "Objectives", new Vector2(20, -14), new Vector2(308, 135), 18);
        var cameraPanel = Panel("Camera status", _canvas.transform, new Vector2(-28, -28), new Vector2(425, 70), true);
        _camera = Label(cameraPanel.transform, "Camera", new Vector2(-12, -9), new Vector2(401, 56), 17, true);
        _camera.alignment = TextAnchor.UpperRight;
        _camera.color = _cream;
        _previewPanel = Panel("Local camera preview", _canvas.transform, new Vector2(28, -301), new Vector2(345, 244));
        var previewLabel = Label(_previewPanel.transform, "Preview title", new Vector2(14, -10), new Vector2(317, 26), 16);
        previewLabel.text = "TU CÁMARA  /  VISTA LOCAL";
        var previewObject = new GameObject("Webcam", typeof(RectTransform), typeof(RawImage));
        previewObject.transform.SetParent(_previewPanel.transform, false);
        _preview = previewObject.GetComponent<RawImage>(); _preview.raycastTarget = false;
        _preview.rectTransform.anchorMin = _preview.rectTransform.anchorMax = _preview.rectTransform.pivot = new Vector2(0.5f, 1);
        _preview.rectTransform.anchoredPosition = new Vector2(0, -42); _preview.rectTransform.sizeDelta = new Vector2(317, 164);
        var previewNote = Label(_previewPanel.transform, "Preview note", new Vector2(14, -214), new Vector2(317, 23), 14);
        previewNote.text = "Mantén ambas manos dentro del encuadre.";
        _previewPanel.SetActive(false);
        _detailPanel = Panel("Lesson", _canvas.transform, new Vector2(-28, -113), new Vector2(397, 434), true);
        _detail = Label(_detailPanel.transform, "Instruction", new Vector2(24, -23), new Vector2(348, 348), 20);
        var rail = Panel("ProgressRail", _detailPanel.transform, new Vector2(24, -392), new Vector2(348, 6));
        rail.GetComponent<Image>().color = new Color(0.2f, 0.3f, 0.3f);
        var fill = Panel("Progress", rail.transform, Vector2.zero, new Vector2(348, 6));
        _progress = fill.GetComponent<Image>();
        _progress.color = _gold;
        var bottom = Panel("Context", _canvas.transform, new Vector2(0, 75), new Vector2(790, 66));
        var rect = bottom.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0);
        _prompt = Label(bottom.transform, "Prompt", new Vector2(20, -12), new Vector2(750, 44), 22);
        _prompt.alignment = TextAnchor.MiddleCenter;
        _message = Label(_canvas.transform, "Message", new Vector2(0, 154), new Vector2(880, 54), 18);
        rect = _message.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0);
        _message.alignment = TextAnchor.MiddleCenter;
        _controls = Label(_canvas.transform, "Controls", new Vector2(0, 19), new Vector2(1250, 34), 16);
        rect = _controls.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0);
        _controls.alignment = TextAnchor.MiddleCenter;
        MakeHelp();
        var fade = Panel("PortalFade", _canvas.transform, Vector2.zero, Vector2.zero);
        _fade = fade.GetComponent<Image>();
        _fade.color = Color.clear;
        rect = fade.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var events = new GameObject("Plaza_EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(transform, false);
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
    }

    private GameObject Panel(string name, Transform parent, Vector2 position, Vector2 size, bool right = false)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(right ? 1 : 0, 1);
        rect.anchoredPosition = position; rect.sizeDelta = size;
        obj.GetComponent<Image>().color = _ink;
        obj.GetComponent<Image>().raycastTarget = false;
        return obj;
    }
    private Text Label(Transform parent, string name, Vector2 position, Vector2 size, int fontSize, bool right = false)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        var text = obj.GetComponent<Text>();
        var rect = text.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(right ? 1 : 0, 1);
        rect.anchoredPosition = position; rect.sizeDelta = size;
        text.font = _font; text.fontSize = fontSize; text.color = _cream;
        text.supportRichText = true; text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        var shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.75f);
        shadow.effectDistance = new Vector2(1, -1);
        return text;
    }
    private void Button(Transform parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        var obj = Panel(label, parent, position, size);
        obj.GetComponent<Image>().color = new Color(0.20f, 0.33f, 0.31f);
        obj.GetComponent<Image>().raycastTarget = true;
        obj.AddComponent<Button>().onClick.AddListener(action);
        var text = Label(obj.transform, "Label", new Vector2(8, -6), size - new Vector2(16, 12), 18);
        text.alignment = TextAnchor.MiddleCenter; text.text = label;
    }
    private void MakeHelp()
    {
        _helpPanel = Panel("HelpAndAudio", _canvas.transform, new Vector2(0, 0), new Vector2(760, 670));
        var rect = _helpPanel.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        var text = Label(_helpPanel.transform, "Help", new Vector2(32, -28), new Vector2(696, 390), 20);
        text.text = "<color=#DDB45C><size=28>Tu primera visita a la plaza</size></color>\n\n"
            + "01  OBJETOS · Acércate a una estación y pulsa E.\n"
            + "C activa tu cámara. Deja espacio para ambas manos.\n"
            + "Mano izquierda abierta: gira de lado a lado.\n"
            + "Mano derecha abierta: inclina arriba y abajo.\n"
            + "Cierra los puños para detener el objeto. E termina; Esc sale.\n"
            + "M cambia al mouse: arrastra con clic izquierdo y suelta para detener.\n\n"
            + "02  COMBATE · E en la entrada del círculo inicia el duelo.\n"
            + "E ataca en tu turno. Espera ¡AHORA! y usa Espacio para esquivar\n"
            + "o F para bloquear. Los errores repiten la defensa. Esc sale.\n\n"
            + "03  PORTALES · Tres estaciones + duelo abren ambos accesos.\n"
            + "Acércate y pulsa E. Cada destino tiene un portal de regreso.";
        Slider(_helpPanel.transform, "Efectos", new Vector2(32, -434), demo.Audio.EffectsVolume,
            value => demo.Audio.SetVolumes(value, demo.Audio.AmbienceVolume));
        Slider(_helpPanel.transform, "Ambiente", new Vector2(32, -493), demo.Audio.AmbienceVolume,
            value => demo.Audio.SetVolumes(demo.Audio.EffectsVolume, value));
        Button(_helpPanel.transform, "Activar / pausar cámara [C]", new Vector2(32, -568), new Vector2(325, 43), () => demo.Hands.Toggle());
        Button(_helpPanel.transform, "Cerrar ayuda [H]", new Vector2(377, -568), new Vector2(350, 43), () => SetHelp(false));
        var note = Label(_helpPanel.transform, "ArtNote", new Vector2(32, -628), new Vector2(696, 24), 14);
        note.text = "Plaza y piezas: interpretación artística para el juego.";
        _helpPanel.SetActive(false);
    }
    private void Slider(Transform parent, string label, Vector2 position, float value, UnityEngine.Events.UnityAction<float> changed)
    {
        var text = Label(parent, label, position, new Vector2(150, 30), 18); text.text = label;
        var track = Panel(label + "Volume", parent, position + new Vector2(170, -4), new Vector2(510, 24));
        track.GetComponent<Image>().color = new Color(0.2f, 0.31f, 0.3f);
        track.GetComponent<Image>().raycastTarget = true;
        var handle = Panel("Handle", track.transform, Vector2.zero, new Vector2(18, 30));
        handle.GetComponent<Image>().color = _gold;
        var slider = track.AddComponent<Slider>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
        slider.value = value;
        slider.onValueChanged.AddListener(changed);
    }

    private string Check(bool value) => value ? "<color=#8DD3AB>✓</color>" : "○";
    private void SetHelp(bool open)
    {
        _helpPanel.SetActive(open);
        demo.SetHelp(open);
    }
    private void Update()
    {
        if (demo.Objectives == null || _title == null) return;
        if (GameBindings.Pressed(GameAction.Help))
            SetHelp(!_helpPanel.activeSelf);
        _title.text = "<size=13><color=#DDB45C>NEMEQUENE  /  EL UMBRAL</color></size>\n"
            + (demo.World < 0 ? "Mundo inferior" : demo.World > 0 ? "Mundo superior" : "Plaza Núñez");
        _objectives.text = "<color=#DDB45C>TU RECORRIDO</color>\n\n"
            + Check(demo.Objectives.IsComplete) + "  Objetos activados  " + demo.Objectives.AnalyzedCount + " / 3\n"
            + Check(demo.Combat.Completed) + "  Duelo de entrenamiento\n"
            + Check(demo.PortalsUnlocked) + "  Portales de los dos mundos";
        _camera.text = (demo.MouseMode ? "MODO MOUSE  ·  M cambia a manos" : "MODO MANOS  ·  C activa la cámara")
            + "\n<size=14>" + demo.Hands.Status + "</size>";
        var source = Mediapipe.Unity.Sample.ImageSourceProvider.ImageSource;
        bool preview = demo.Hands.Requested && demo.Selected != null && source != null && source.isPrepared;
        _previewPanel.SetActive(preview);
        if (preview)
        {
            _preview.texture = source.GetCurrentTexture();
            _preview.uvRect = new Rect(source.isFrontFacing ? 1 : 0, source.isVerticallyFlipped ? 1 : 0,
                source.isFrontFacing ? -1 : 1, source.isVerticallyFlipped ? -1 : 1);
            float ratio = source.textureWidth / (float)Mathf.Max(1, source.textureHeight);
            _preview.rectTransform.sizeDelta = ratio > 317f / 164 ? new Vector2(317, 317 / ratio) : new Vector2(164 * ratio, 164);
        }
        _message.text = demo.Status;
        _controls.text = "WASD  caminar     SHIFT  correr     ESPACIO  saltar     Q  dash     RMB + mouse  cámara     H  ayuda / audio     V  " + (demo.Audio.Muted ? "activar sonido" : "silenciar") + "     R  reiniciar";
        _fade.color = new Color(0.035f, 0.07f, 0.09f, demo.Fade);
        _detailPanel.SetActive(demo.Selected != null || demo.State == TechnicalDemoState.Combat);
        if (demo.Selected != null) Analysis();
        else if (demo.State == TechnicalDemoState.Combat) Combat();
        else
        {
            _prompt.text = demo.Nearby != null ? "[ E ]  Examinar " + demo.Nearby.Data.displayName
                : demo.NearbyPortal != null ? demo.NearbyPortal.Available ? "[ E ]  Viajar a " + demo.NearbyPortal.destinationName : "Portal sellado · completa objetos y entrenamiento"
                : demo.NearCombat ? "[ E ]  " + (demo.Combat.Completed ? "Repetir entrenamiento" : "Iniciar duelo de entrenamiento")
                : demo.World != 0 ? "Explora el umbral · el arco luminoso te devuelve a la plaza"
                : !demo.Objectives.IsComplete ? "Busca las estaciones 01, 02 y 03 · [ C ] cámara · [ M ] mouse"
                : !demo.Combat.Completed ? "Ve al círculo de entrenamiento, a la derecha de la fuente"
                : "Elige un portal: inferior a la izquierda, superior a la derecha";
        }
    }
    private void Analysis()
    {
        var lesson = demo.Lesson;
        string instruction = lesson.Lesson == 0 ? "Gira el objeto de lado a lado." : lesson.Lesson == 1 ? "Inclina el objeto arriba y abajo." : "Combina ambos giros y detén el objeto.";
        _detail.text = "<color=#DDB45C>ESTACIÓN 0" + (lesson.Lesson + 1) + "  /  MANOS</color>\n\n"
            + "<size=27>" + demo.Selected.Data.displayName + "</size>\n"
            + "<size=16>" + demo.Selected.Data.description + "</size>\n\n"
            + instruction + "\n\n"
            + (demo.MouseMode ? "Arrastra con clic izquierdo.\nSuelta el botón para congelar.\n\n"
                : "Izquierda abierta → giro horizontal.\nDerecha abierta → giro vertical.\nPuños cerrados → congelar.\n\n")
            + (lesson.Lesson != 1 ? Check(lesson.YawDegrees >= 28) + "  Giro horizontal\n" : "")
            + (lesson.Lesson != 0 ? Check(lesson.PitchDegrees >= 28) + "  Giro vertical\n" : "")
            + (lesson.Lesson == 2 ? Check(lesson.FreezeSeconds >= 0.65f) + "  Detener durante un instante\n" : "")
            + (lesson.Complete ? "\n<color=#8DD3AB>Estación activada</color>" : "");
        _progress.rectTransform.sizeDelta = new Vector2(348 * lesson.Progress, 6);
        _prompt.text = lesson.Complete ? "[ E ]  Volver a la plaza · puedes seguir practicando" : "[ ESC ] salir   ·   [ C ] cámara   ·   [ M ] cambiar manos / mouse";
    }
    private void Combat()
    {
        var model = demo.Combat.Model;
        string title, body;
        if (model.Phase == PlazaCombatPhase.Attack)
        {
            title = "TU TURNO";
            body = "Pulsa E para golpear.\nNo hay límite de tiempo.\n\nEl guardián responderá después.\nObserva el círculo y espera la señal.";
        }
        else if (model.Phase == PlazaCombatPhase.Telegraph)
        {
            title = "PREPÁRATE";
            body = model.Expected == PlazaDefense.Dodge
                ? "El guardián prepara un golpe directo.\n\nEspera ¡AHORA! y pulsa ESPACIO\npara esquivar hacia un lado."
                : "El guardián prepara una onda amplia.\n\nEspera ¡AHORA! y pulsa F\npara bloquear el impacto.";
        }
        else if (model.Phase == PlazaCombatPhase.React)
        {
            title = "¡AHORA!";
            body = model.Expected == PlazaDefense.Dodge ? "<size=28>ESPACIO · ESQUIVAR</size>" : "<size=28>F · BLOQUEAR</size>";
            body += "\n\nTiempo para responder: " + model.Remaining.ToString("0.0") + " s";
        }
        else if (model.Phase == PlazaCombatPhase.Won)
        {
            title = "PRUEBA SUPERADA";
            body = "Aprendiste a atacar, esquivar y bloquear.\n\n"
                + (demo.PortalsUnlocked ? "Ambos portales están abiertos.\nElige tu próximo destino." : "Completa las tres estaciones de objetos\npara abrir los dos portales.")
                + "\n\nE para regresar a la plaza.";
        }
        else
        {
            title = model.LastDefenseSucceeded ? "¡BIEN HECHO!" : "INTÉNTALO OTRA VEZ";
            body = model.LastDefenseSucceeded ? "Defensa correcta.\nAhora puedes contraatacar."
                : "Espera la señal y usa la tecla indicada.\nRepetimos esta defensa.\n\nLa práctica no termina por un error.";
        }
        _detail.text = "<color=#DDB45C>DUELO DE ENTRENAMIENTO</color>\n\n<size=28>" + title + "</size>\n\n"
            + body + "\n\n<color=#DDB45C>Guardián</color>  " + (3 - model.Hits) + " / 3\n"
            + "<size=16>Tu energía  " + demo.PlayerHealth.CurrentHealth.ToString("0") + " / 100</size>";
        float progress = model.Phase == PlazaCombatPhase.React ? model.Remaining / model.ReactionSeconds
            : model.Phase == PlazaCombatPhase.Telegraph ? model.Remaining / model.TelegraphSeconds : model.Hits / 3f;
        _progress.rectTransform.sizeDelta = new Vector2(348 * Mathf.Clamp01(progress), 6);
        _prompt.text = model.Phase == PlazaCombatPhase.Attack ? "[ E ] atacar   ·   [ ESC ] salir del entrenamiento"
            : model.Phase == PlazaCombatPhase.Won ? "[ E ] volver a la plaza"
            : model.Phase == PlazaCombatPhase.React ? model.Expected == PlazaDefense.Dodge ? "[ ESPACIO ] esquivar ahora" : "[ F ] bloquear ahora"
            : "[ ESC ] salir del entrenamiento";
        _controls.text = "COMBATE POR TURNOS     E  atacar     ESPACIO  esquivar     F  bloquear     ESC  salir     H  ayuda";
    }
}
