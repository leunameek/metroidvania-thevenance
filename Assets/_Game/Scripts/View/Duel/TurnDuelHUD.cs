using System;
using System.Collections.Generic;
using Nemequene.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Turn duel screen of the Bacatá kit (guion 07): enemy name, health and bond status at the top;
// the turn banner (TU TURNO / PREPARA / RESPONDE) with the window bar in the middle; on the left
// the player's concentration and counter chance; the usual vitality frame at the bottom left; at
// the bottom only the legal actions as buttons with their key and spoken word. Form, name and verb always accompany the colour.
public sealed class TurnDuelHUD
{
    private readonly GameObject _root;
    // The announcement/result plate: only the accessibility option shows it; otherwise the player
    // reads the duel from the world (2026-10-07/08 playtest).
    private readonly GameObject _info;
    private readonly TMP_Text _enemy, _status, _banner, _verbs, _message, _concentration, _hint;
    private readonly Image _enemyHealth, _window, _bannerPlate, _playerHealth;
    private readonly TMP_Text _playerValue;
    private float _playerShown = -1;
    private readonly RectTransform _actions;
    private readonly Action<DuelAction> _act;
    private readonly Action<int> _target;
    private readonly Action<DuelDefense> _defend;
    private string _signature = "";

    public TurnDuelHUD(Transform parent, string enemyName, Action<DuelAction> act, Action<int> target, Action<DuelDefense> defend)
    {
        _act = act; _target = target; _defend = defend;
        var canvas = UIKit.ScreenCanvas(parent, "DuelCanvas", 1100);
        canvas.gameObject.AddComponent<GraphicRaycaster>();
        _root = canvas.gameObject;

        var top = UIKit.Place(UIKit.HudPanel(canvas, "Enemy"), new Vector2(.5f, 1), new Vector2(0, -28), new Vector2(760, 124));
        _enemy = UIKit.Label(top, enemyName, 28, UIPalette.GoldLight, true);
        // Inside the plate's carved top rim (2026-10-07 audit: the name sat on it).
        _enemy.rectTransform.anchorMin = new Vector2(.04f, .56f); _enemy.rectTransform.anchorMax = new Vector2(.96f, .86f);
        _enemy.enableAutoSizing = true; _enemy.fontSizeMax = 28; _enemy.fontSizeMin = 18; _enemy.textWrappingMode = TextWrappingModes.NoWrap;
        var barHolder = UIKit.Rect("Bar", top); barHolder.anchorMin = new Vector2(.08f, .36f); barHolder.anchorMax = new Vector2(.92f, .52f);
        _enemyHealth = UIKit.Bar(barHolder, new Vector2(620, 18), UIPalette.Crimson);
        _status = UIKit.Label(top, "", 19, UIPalette.Muted);
        _status.rectTransform.anchorMin = new Vector2(.04f, .1f); _status.rectTransform.anchorMax = new Vector2(.96f, .34f);

        var banner = UIKit.Place(UIKit.Ribbon(canvas, "Turn"), new Vector2(.5f, 1), new Vector2(0, -176), new Vector2(620, 70));
        _bannerPlate = banner.GetComponent<Image>();
        _banner = UIKit.Label(banner, "", 30, UIPalette.Ivory, true);
        var windowHolder = UIKit.Rect("Window", canvas);
        UIKit.Place(windowHolder, new Vector2(.5f, 1), new Vector2(0, -254), new Vector2(520, 14));
        _window = UIKit.Bar(windowHolder, new Vector2(520, 14), UIPalette.GoldLight);
        // The announcement and the result on their own plate (they used to float over the sky).
        var info = UIKit.Place(UIKit.HudPanel(canvas, "Info"), new Vector2(.5f, 1), new Vector2(0, -276), new Vector2(1040, 118));
        _info = info.gameObject;
        _verbs = UIKit.Label(info, "", 26, UIPalette.GoldLight);
        _verbs.rectTransform.anchorMin = new Vector2(0, .52f); _verbs.rectTransform.offsetMin = new Vector2(40, 0); _verbs.rectTransform.offsetMax = new Vector2(-40, -10);
        _verbs.enableAutoSizing = true; _verbs.fontSizeMax = 26; _verbs.fontSizeMin = 18; _verbs.textWrappingMode = TextWrappingModes.NoWrap;
        _message = UIKit.Label(info, "", 21, UIPalette.Ivory);
        _message.rectTransform.anchorMax = new Vector2(1, .52f); _message.rectTransform.offsetMin = new Vector2(40, 10); _message.rectTransform.offsetMax = new Vector2(-40, 0);
        _message.enableAutoSizing = true; _message.fontSizeMax = 21; _message.fontSizeMin = 16;

        var left = UIKit.Place(UIKit.HudPanel(canvas, "Concentration"), new Vector2(0, .5f), new Vector2(40, 0), new Vector2(330, 120));
        _concentration = UIKit.Label(left, "", 22, UIPalette.Ivory);
        _concentration.margin = new Vector4(18, 8, 18, 8);

        // The player's vitality, the same frame as in exploration, moved to the bottom left.
        _playerHealth = UIKit.HealthFrame(canvas, "Vida", out _playerValue);
        _playerValue.color = UIPalette.Ivory;
        UIKit.Place((RectTransform)_playerHealth.transform.parent.parent, new Vector2(0, 0), new Vector2(40, 40), new Vector2(452, 138));

        // The choices on one plate at the bottom (right of the vitality): the buttons and, under
        // them, how to answer.
        var bar = UIKit.Place(UIKit.HudPanel(canvas, "ActionBar"), new Vector2(.5f, 0), new Vector2(240, 44), new Vector2(1260, 150));
        _actions = UIKit.Rect("Actions", bar); _actions.anchorMin = new Vector2(0, .42f); _actions.offsetMin = new Vector2(30, 0); _actions.offsetMax = new Vector2(-30, -14);
        var row = _actions.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 10; row.childAlignment = TextAnchor.MiddleCenter;
        row.childControlWidth = row.childControlHeight = true; row.childForceExpandWidth = false; row.childForceExpandHeight = false;
        _hint = UIKit.Label(bar, "", 19, UIPalette.Muted);
        _hint.rectTransform.anchorMax = new Vector2(1, .42f); _hint.rectTransform.offsetMin = new Vector2(40, 14); _hint.rectTransform.offsetMax = new Vector2(-40, 0);
        _hint.enableAutoSizing = true; _hint.fontSizeMax = 19; _hint.fontSizeMin = 15;
        _root.SetActive(false);
    }

    public void Show(bool visible) { if (_root.activeSelf != visible) _root.SetActive(visible); }

    public void Refresh(TurnDuelModel m, bool force)
    {
        _enemyHealth.fillAmount = m.EnemyMaxHealth > 0 ? (float)m.EnemyHealth / m.EnemyMaxHealth : 0;
        // Who is exposed or active (head, moon/sun, core) is read from the enemy's glow; the line
        // keeps only the bonds left, unless the player asked to see everything.
        _status.text = m.ShowAnswers ? m.Rules.Status(m) : m.Rules.Progress(m);
        bool plate = m.ShowAnswers;
        if (_info.activeSelf != plate) _info.SetActive(plate);
        float health = (float)m.PlayerHealth / TurnDuelModel.PlayerMaxHealth;
        _playerShown = _playerShown < 0 ? health : Mathf.MoveTowards(_playerShown, health, Time.unscaledDeltaTime * 1.4f);
        _playerHealth.fillAmount = _playerShown;
        _playerValue.text = m.PlayerHealth + " / " + TurnDuelModel.PlayerMaxHealth;
        _concentration.text = $"Concentración {m.Concentration} / {TurnDuelModel.MaxConcentration}"
            + (m.Counter == CounterWindow.Reinforced ? "\nContraataque reforzado listo"
                : m.Counter == CounterWindow.Normal ? "\nContraataque listo" : "");
        _message.text = m.Message;
        switch (m.Phase)
        {
            case DuelPhase.Decide:
                _banner.text = "TU TURNO"; _verbs.text = "Decide sin prisa"; _window.fillAmount = 0; break;
            case DuelPhase.Telegraph:
                _banner.text = "PREPARA"; _verbs.text = Announce(m); _window.fillAmount = 1; break;
            case DuelPhase.Respond:
                _banner.text = "¡RESPONDE!"; _verbs.text = Announce(m);
                _window.fillAmount = m.Untimed ? 1 : Mathf.Clamp01(m.Remaining / Mathf.Max(.01f, m.ResponseSeconds)); break;
            case DuelPhase.Won: _banner.text = "PRUEBA SUPERADA"; _verbs.text = ""; break;
            case DuelPhase.Lost: _banner.text = "HAS CAÍDO"; _verbs.text = ""; break;
            default: _banner.text = ""; _verbs.text = ""; _window.fillAmount = 0; break;
        }
        if (_bannerPlate != null) _bannerPlate.color = m.Phase == DuelPhase.Respond ? new Color(1f, .8f, .8f) : Color.white;
        RebuildButtons(m, force);
    }

    // Only shown with the accessibility option: in every duel the answer is read from the enemy
    // itself; the explanations live in the plaza training (2026-10-08 playtest).
    private static string Announce(TurnDuelModel m)
    {
        if (m.Move == null || !m.ShowAnswers) return "";
        string origin = m.Move.Origin != DuelTarget.None ? TurnDuelModel.TargetName(m.Move.Origin) + " · " : "";
        return origin + m.Move.Label + "  →  " + m.Move.Verbs;
    }

    // Only legal choices are offered; the list is rebuilt when it changes.
    private void RebuildButtons(TurnDuelModel m, bool force)
    {
        var entries = new List<(string label, Action action, bool primary)>();
        string hint = "";
        if (m.Phase == DuelPhase.Decide)
        {
            var targets = m.Rules.Targets;
            for (int i = 0; i < targets.Length; i++)
            {
                int index = i; bool chosen = m.Target == targets[i];
                entries.Add((Choice(TargetWord(targets[i]), i == 0 ? "Z" : "X", TurnDuelModel.TargetName(targets[i])) + (chosen ? " (elegida)" : ""), () => _target(index), chosen));
            }
            foreach (var action in m.LegalActions())
            {
                var a = action;
                entries.Add((Choice(TurnDuelModel.ActionName(a).ToLowerInvariant(), Key(a), TurnDuelModel.ActionName(a)), () => _act(a), a == DuelAction.Attack));
            }
            hint = VoicePrompt.Enabled
                ? "Di en voz alta la palabra de un botón (también en inglés: «attack», «block»…). Entre paréntesis, su tecla."
                : "Elige con las teclas o los botones. Activa la voz en la pausa para decir las acciones.";
        }
        else if (m.Phase == DuelPhase.Telegraph || m.Phase == DuelPhase.Respond)
        {
            foreach (DuelDefense d in Enum.GetValues(typeof(DuelDefense)))
            {
                if (d == DuelDefense.Parry && !m.ParryOffered) continue;
                var defense = d;
                entries.Add((Choice(TurnDuelModel.DefenseName(d).ToLowerInvariant(), Key(d), TurnDuelModel.DefenseName(d)), () => _defend(defense), false));
            }
            hint = m.Phase == DuelPhase.Telegraph ? "Espera la señal: responder antes no cuenta."
                : VoicePrompt.Enabled ? "¡Dilo ahora! O pulsa la tecla entre paréntesis." : "Responde ahora con la tecla o el botón.";
        }
        _hint.text = hint;
        var signature = m.Phase + "|" + string.Join(",", entries.ConvertAll(e => e.label));
        if (signature == _signature) return;
        _signature = signature;
        for (int i = _actions.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(_actions.GetChild(i).gameObject);
        // Each button as wide as its words (with room for the ribbon's ends), all within the bar.
        float available = 1200 - 10 * (entries.Count - 1);
        var widths = new List<float>(); float total = 0;
        foreach (var (label, _, _) in entries)
        {
            float w = Mathf.Max(170, _hint.GetPreferredValues(label).x * 24f / 19f + 76);
            widths.Add(w); total += w;
        }
        float squeeze = total > available ? available / total : 1;
        for (int i = 0; i < entries.Count; i++)
        {
            var (label, action, primary) = entries[i];
            var button = UIKitButton.Create(_actions, label, action, primary, 54);
            var size = button.GetComponent<LayoutElement>(); size.preferredWidth = widths[i] * squeeze;
            var text = button.GetComponentInChildren<TMP_Text>();
            text.rectTransform.offsetMin = new Vector2(34, 4); text.rectTransform.offsetMax = new Vector2(-34, -4);
        }
    }

    // Voice first: «atacar» (1); with the voice off, the key and the name: 1 · Atacar.
    private static string Choice(string word, string key, string name) =>
        VoicePrompt.Enabled ? VoicePrompt.Word(word) + " (" + key + ")" + (word == name.ToLowerInvariant() ? "" : " · " + name) : key + " · " + name;
    private static string TargetWord(DuelTarget t) =>
        t == DuelTarget.HeadA ? "izquierda" : t == DuelTarget.HeadB ? "derecha" : t == DuelTarget.Moon ? "luna" : "sol";

    private static string Key(DuelAction a)
    {
        switch (a)
        {
            case DuelAction.Attack: return "1";
            case DuelAction.Counter: return "2";
            case DuelAction.Jaguar: return "3";
            case DuelAction.Horn: return "4";
            case DuelAction.Anchor: return "5";
            case DuelAction.Interrupt: return "6";
            default: return "7";
        }
    }
    private static string Key(DuelDefense d) =>
        GameBindings.Cap(d == DuelDefense.Block ? GameAction.Guard : d == DuelDefense.Dodge ? GameAction.Dodge : d == DuelDefense.Cover ? GameAction.Cover : GameAction.Parry);
}
