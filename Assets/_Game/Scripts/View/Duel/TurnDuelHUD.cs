using System;
using System.Collections.Generic;
using Nemequene.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Turn duel screen of the Bacatá kit (guion 07): enemy name, health and bond status at the top;
// the turn banner (TU TURNO / PREPARA / RESPONDE) with the window bar in the middle; on the left
// the player's concentration and counter chance; at the bottom only the legal actions as buttons
// with their key and spoken word. Form, name and verb always accompany the colour.
public sealed class TurnDuelHUD
{
    private readonly GameObject _root;
    private readonly TMP_Text _enemy, _status, _banner, _verbs, _message, _concentration, _hint;
    private readonly Image _enemyHealth, _window, _bannerPlate;
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
        _enemy.rectTransform.anchorMin = new Vector2(0, .62f); _enemy.rectTransform.anchorMax = new Vector2(1, .96f);
        var barHolder = UIKit.Rect("Bar", top); barHolder.anchorMin = new Vector2(.08f, .38f); barHolder.anchorMax = new Vector2(.92f, .58f);
        _enemyHealth = UIKit.Bar(barHolder, new Vector2(620, 18), UIPalette.Crimson);
        _status = UIKit.Label(top, "", 19, UIPalette.Muted);
        _status.rectTransform.anchorMin = new Vector2(0, .04f); _status.rectTransform.anchorMax = new Vector2(1, .34f);

        var banner = UIKit.Place(UIKit.Ribbon(canvas, "Turn"), new Vector2(.5f, 1), new Vector2(0, -176), new Vector2(620, 70));
        _bannerPlate = banner.GetComponent<Image>();
        _banner = UIKit.Label(banner, "", 30, UIPalette.Ivory, true);
        var windowHolder = UIKit.Rect("Window", canvas);
        UIKit.Place(windowHolder, new Vector2(.5f, 1), new Vector2(0, -254), new Vector2(520, 14));
        _window = UIKit.Bar(windowHolder, new Vector2(520, 14), UIPalette.GoldLight);
        _verbs = UIKit.Shadow(UIKit.Label(canvas, "", 26, UIPalette.GoldLight));
        UIKit.Place(_verbs.rectTransform, new Vector2(.5f, 1), new Vector2(0, -280), new Vector2(1100, 44));
        _message = UIKit.Shadow(UIKit.Label(canvas, "", 22, UIPalette.Ivory));
        UIKit.Place(_message.rectTransform, new Vector2(.5f, 1), new Vector2(0, -326), new Vector2(1200, 70));

        var left = UIKit.Place(UIKit.HudPanel(canvas, "Concentration"), new Vector2(0, .5f), new Vector2(40, 0), new Vector2(330, 120));
        _concentration = UIKit.Label(left, "", 22, UIPalette.Ivory);
        _concentration.margin = new Vector4(18, 8, 18, 8);

        _actions = UIKit.Place(UIKit.Rect("Actions", canvas), new Vector2(.5f, 0), new Vector2(0, 168), new Vector2(1100, 60));
        var row = _actions.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 12; row.childAlignment = TextAnchor.MiddleCenter;
        row.childControlWidth = row.childControlHeight = true; row.childForceExpandWidth = false;
        _hint = UIKit.Shadow(UIKit.Label(canvas, "", 19, UIPalette.Muted));
        UIKit.Place(_hint.rectTransform, new Vector2(.5f, 0), new Vector2(0, 124), new Vector2(1100, 36));
        _root.SetActive(false);
    }

    public void Show(bool visible) { if (_root.activeSelf != visible) _root.SetActive(visible); }

    public void Refresh(TurnDuelModel m, bool force)
    {
        _enemyHealth.fillAmount = m.EnemyMaxHealth > 0 ? (float)m.EnemyHealth / m.EnemyMaxHealth : 0;
        _status.text = m.Rules.Status(m);
        _concentration.text = $"Vida {m.PlayerHealth}\nConcentración {m.Concentration} / {TurnDuelModel.MaxConcentration}"
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

    private static string Announce(TurnDuelModel m)
    {
        if (m.Move == null) return "";
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
        foreach (var (label, action, primary) in entries)
        {
            var button = UIKitButton.Create(_actions, label, action, primary, 54);
            var size = button.GetComponent<LayoutElement>(); size.preferredWidth = entries.Count > 4 ? 210 : 250;
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
        d == DuelDefense.Block ? "F" : d == DuelDefense.Dodge ? "Espacio" : d == DuelDefense.Cover ? "G" : "R";
}
