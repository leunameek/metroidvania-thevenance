using Nemequene.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// TEMPORARY test button (2026-10-06, user's request): «Capítulos · F9» on the left edge of every
// gameplay scene opens the list of CampaignJump. Only in the editor and development builds; the
// title menu does not show it. Remove this file (and CampaignJump) before the final build.
public sealed class ChapterSkipMenu : MonoBehaviour
{
    private static ChapterSkipMenu _instance;
    private GameObject _root, _tab, _panel;
    private TMP_Text _current;
    private bool _cursorVisible;
    private CursorLockMode _cursorLock;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (_instance != null || Application.isBatchMode || !(Application.isEditor || Debug.isDebugBuild)) return;
        var go = new GameObject("ChapterSkipMenu");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<ChapterSkipMenu>();
        _instance.Build();
    }

    private void Build()
    {
        var canvas = UIKit.ScreenCanvas(transform, "ChapterSkipCanvas", 4000);
        canvas.gameObject.AddComponent<GraphicRaycaster>();
        _root = canvas.gameObject;

        var tab = UIKit.Place(UIKit.Rect("Tab", canvas), new Vector2(0, .5f), new Vector2(0, 230), new Vector2(250, 48));
        _tab = tab.gameObject;
        UIKitButton.Create(tab, "Capítulos · F9", Toggle, false, 48);

        var panel = UIKit.Place(UIKit.HudPanel(canvas, "Panel"), new Vector2(0, .5f), new Vector2(24, 0), new Vector2(520, 820));
        _panel = panel.gameObject;
        var title = UIKit.Label(panel, "Saltar a capítulo (temporal)", 28, UIPalette.GoldLight, true);
        title.rectTransform.anchorMin = new Vector2(0, .92f); title.rectTransform.anchorMax = new Vector2(1, .985f);
        _current = UIKit.Label(panel, "", 19, UIPalette.Muted);
        _current.rectTransform.anchorMin = new Vector2(.06f, .855f); _current.rectTransform.anchorMax = new Vector2(.94f, .92f);
        var list = UIKit.Rect("List", panel);
        list.anchorMin = new Vector2(.04f, .03f); list.anchorMax = new Vector2(.96f, .85f);
        var column = list.gameObject.AddComponent<VerticalLayoutGroup>();
        column.spacing = 4; column.childControlWidth = column.childControlHeight = true;
        column.childForceExpandWidth = true; column.childForceExpandHeight = false;
        foreach (var entry in CampaignJump.Entries)
        {
            var chapter = entry.chapter;
            UIKitButton.Create(list, entry.title, () => { Close(); CampaignJump.Jump(chapter); }, false, 58);
        }
        UIKitButton.Create(list, "Cerrar · F9", Close, true, 58);
        _panel.SetActive(false);
    }

    private void Update()
    {
        bool gameplay = SceneManager.GetActiveScene().name != "MainMenu" && !SceneLoader.Loading;
        if (_root.activeSelf != gameplay) _root.SetActive(gameplay);
        if (!gameplay) { if (_panel.activeSelf) Close(); return; }
        var k = Keyboard.current;
        if (k != null && k.f9Key.wasPressedThisFrame) Toggle();
        if (_panel.activeSelf)
        {
            _current.text = "Ahora: " + CampaignModel.ChapterTitle(CampaignProgress.Chapter)
                + (WorldTravel.SaveSlot >= 0 ? " · ranura " + (WorldTravel.SaveSlot + 1) : " · sin ranura (prueba)");
            Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
        }
    }

    private void Toggle() { if (_panel.activeSelf) Close(); else Open(); }

    private void Open()
    {
        if (EventSystem.current == null)
        {
            var events = new GameObject("ChapterSkip_EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
        _cursorVisible = Cursor.visible; _cursorLock = Cursor.lockState;
        _panel.SetActive(true); _tab.SetActive(false);
    }

    private void Close()
    {
        if (!_panel.activeSelf) return;
        _panel.SetActive(false); _tab.SetActive(true);
        Cursor.visible = _cursorVisible; Cursor.lockState = _cursorLock;
    }
}
