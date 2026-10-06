using System.Collections;
using Nemequene.UI;
using UnityEngine;
using UnityEngine.InputSystem;

// Guardián de la cima (guide 7): a reactive turn duel started with an explicit E on its mark.
// Player turn: E attacks once (20, 25 or 30 by yopos). Guardian turn: 1.8 s warning with the
// verb, then a 2.4 s window for one answer (F blocks, Space dodges); right = 0 damage, wrong or
// late = 20. Phase 2 at 120 adds the fragment rain and the core pulse in a fixed cycle that
// restarts on every attempt. The fragments and rings are presentation: damage is applied once,
// here. Victory writes ms_jefe_vencido before presenting it.
public sealed class MSGuardian : MIInteractable
{
    private enum Attack { Frontal, Sweep, Fragments, Pulse }

    [SerializeField] private float maxHealth = 240f, phaseTwoAt = 120f, failDamage = 20f;
    [SerializeField] private float warning = 1.8f, window = 2.4f, feedback = 1.2f;
    [SerializeField] private Transform body, leftArm, rightArm, playerMark;
    [SerializeField] private Renderer core;
    [SerializeField] private Transform[] fragments = new Transform[0];
    // Provisional model (hub training guardian): Hit, Attack, Die and Reset triggers.
    [SerializeField] private Animator animator;

    private static readonly Attack[] PhaseOne = { Attack.Frontal, Attack.Sweep };
    private static readonly Attack[] PhaseTwo = { Attack.Frontal, Attack.Fragments, Attack.Sweep, Attack.Pulse };

    private float _health;
    private int _step;
    private bool _fighting;
    private Coroutine _routine;
    private Color _coreBase = new Color(1f, .7f, .3f);
    private PlayerController _player;

    public bool Fighting => _fighting;
    public bool Defeated => MSProgress.Has(MSProgress.Guardian);
    public float Health01 => maxHealth > 0 ? _health / maxHealth : 0;
    public override bool Available => base.Available && !_fighting && !Defeated;
    public override string Prompt => "Enfrentar al guardián";

    private void Start()
    {
        _health = maxHealth;
        if (Defeated) ShowDefeated();
    }

    public override void Interact(PlayerController player)
    {
        if (!Available) return;
        _player = player;
        _routine = StartCoroutine(Duel());
    }

    // Defeat or abandon: full health again, first phase, the cycle from its start.
    public void ResetEncounter()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;
        _fighting = false;
        _health = maxHealth; _step = 0;
        Pose(0, 0);
        SetCore(_coreBase, 1);
        foreach (var f in fragments) if (f != null) f.gameObject.SetActive(false);
        if (!Defeated) Trigger("Reset");
        UIWorldPrompt.Hide(this);
    }

    private void Trigger(string name)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        foreach (var t in new[] { "Hit", "Attack", "Die", "Reset" }) animator.ResetTrigger(t);
        animator.SetTrigger(name);
    }

    private IEnumerator Duel()
    {
        var director = MundoSuperiorDirector.Instance;
        _fighting = true; _health = maxHealth; _step = 0;
        director?.BeginCombat(this, playerMark);
        MSAudio.Play("jefe_despierta", .9f);
        yield return new WaitForSeconds(1f);
        var health = _player.GetComponent<Health>();
        while (_health > 0 && health != null && !health.IsDead)
        {
            // 2. Player turn: E without a time limit; the E that started the duel does not count.
            UIWorldPrompt.Show(this, "E", "Atacar al núcleo");
            yield return null;
            while (!Pressed(Keyboard.current?.eKey)) yield return null;
            UIWorldPrompt.Hide(this);
            float damage = MSProgress.AttackDamage;
            _health = Mathf.Max(0, _health - damage);
            MSAudio.Play("golpe_nucleo", .9f);
            MIBurst.Spawn(core != null ? core.transform.position : transform.position + Vector3.up * 4, new Color(1f, .8f, .4f));
            SetCore(Color.white, 3f);
            if (_health > 0) Trigger("Hit");
            director?.Hud?.Notify("Golpe al núcleo", "−" + damage.ToString("0") + " · vida del guardián " + _health.ToString("0") + " / " + maxHealth.ToString("0"), UIIcon.Objective, UIPalette.GoldLight);
            yield return new WaitForSeconds(.6f);
            SetCore(_coreBase, 1);
            // 3. Immediate victory: no final retaliation.
            if (_health <= 0) break;

            // 4. Warning: silhouette and verb; presses now are ignored.
            var cycle = _health <= phaseTwoAt ? PhaseTwo : PhaseOne;
            var attack = cycle[_step % cycle.Length]; _step++;
            bool block = attack == Attack.Frontal || attack == Attack.Pulse;
            string verb = block ? "Bloquea" : "Esquiva";
            UIWorldPrompt.Show(this, block ? "F" : "Espacio", verb + " · " + Name(attack));
            Trigger("Attack");
            MSAudio.Play("jefe_aviso", .85f, attack == Attack.Sweep ? 1.1f : attack == Attack.Pulse ? 1.25f : 1f);
            for (float t = 0; t < warning; t += Time.deltaTime) { Telegraph(attack, t / warning); SetCore(Color.Lerp(_coreBase, new Color(1f, .3f, .2f), t / warning), 1 + t); yield return null; }

            // 5. Reactive window: one accepted answer.
            bool? answer = null;
            float limit = window * (director != null ? director.ReactionMultiplier : 1f);
            for (float t = 0; t < limit && answer == null; t += Time.deltaTime)
            {
                yield return null;
                var k = Keyboard.current;
                if (Pressed(k?.fKey)) answer = block;
                else if (Pressed(k?.spaceKey)) answer = !block;
            }
            UIWorldPrompt.Hide(this);

            // 6. Resolution, then 7. feedback.
            bool right = answer == true;
            Strike(attack);
            if (!block && answer != null) _player.PerformDodge(_step % 2 == 0 ? 1 : -1);
            if (right) { MSAudio.Play(block ? "defensa_bloqueo" : "defensa_esquiva", .9f); director?.Hud?.Notify(block ? "Bloqueo" : "Esquiva", "Sin daño.", UIIcon.Dodge, UIPalette.Jade); }
            else { MSAudio.Play("defensa_fallida", .8f); director?.Damage(failDamage); director?.Hud?.Notify(answer == null ? "Demasiado tarde" : "Respuesta equivocada", "−" + failDamage.ToString("0") + " de vida. Era «" + verb + "».", UIIcon.Info, UIPalette.Danger); }
            for (float t = 0; t < feedback; t += Time.deltaTime) { Recover(t / feedback); yield return null; }
            SetCore(_coreBase, 1);
        }
        UIWorldPrompt.Hide(this);
        if (_health <= 0)
        {
            // 9. Victory: flag first, then the presentation.
            MSProgress.Set(MSProgress.Guardian);
            MSAudio.Play("victoria", 1f);
            ShowDefeated();
            _fighting = false;
            director?.EndCombat(true);
        }
        _routine = null;
    }

    private static bool Pressed(UnityEngine.InputSystem.Controls.KeyControl key)
    {
        var director = MundoSuperiorDirector.Instance;
        return key != null && key.wasPressedThisFrame && (director == null || !director.Paused);
    }

    private static string Name(Attack attack) => attack switch
    {
        Attack.Frontal => "golpe frontal", Attack.Sweep => "barrido lateral",
        Attack.Fragments => "lluvia de fragmentos", _ => "pulso del núcleo",
    };

    // Readable silhouettes: the arm rises for the frontal blow, opens for the sweep; the rain shows
    // two shadows at the player's mark; the pulse grows a ring on the core.
    private void Telegraph(Attack attack, float k)
    {
        switch (attack)
        {
            case Attack.Frontal: Pose(-110f * k, 0); break;
            case Attack.Sweep: Pose(0, 80f * k); break;
            case Attack.Pulse: SetCore(new Color(1f, .45f, .2f), 1 + 3 * k); break;
            case Attack.Fragments:
                for (int i = 0; i < fragments.Length && i < 2; i++)
                {
                    var f = fragments[i]; if (f == null || playerMark == null) continue;
                    f.gameObject.SetActive(true);
                    f.position = playerMark.position + new Vector3(i == 0 ? -.8f : .8f, 9f - 2f * k, i == 0 ? .3f : -.4f);
                }
                break;
        }
    }

    private void Strike(Attack attack)
    {
        if (attack == Attack.Fragments)
            for (int i = 0; i < fragments.Length && i < 2; i++)
                if (fragments[i] != null && playerMark != null) fragments[i].position = playerMark.position + new Vector3(i == 0 ? -1.4f : 1.4f, .4f, i == 0 ? .6f : -.6f);
        MSAudio.Play("jefe_golpe", .85f);
    }

    private void Recover(float k)
    {
        Pose(Mathf.Lerp(-60f, 0, k), 0);
        if (k > .9f) foreach (var f in fragments) if (f != null) f.gameObject.SetActive(false);
    }

    private void Pose(float raise, float open)
    {
        if (rightArm != null) rightArm.localRotation = Quaternion.Euler(raise, 0, open);
        if (leftArm != null) leftArm.localRotation = Quaternion.Euler(raise * .3f, 0, -open * .3f);
    }

    private void SetCore(Color color, float intensity)
    {
        if (core == null) return;
        var m = core.material;
        m.SetColor("_BaseColor", color);
        if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", color * intensity);
    }

    // The defeated guardian kneels, its core dark: no hostile logic remains.
    private void ShowDefeated()
    {
        Trigger("Die");
        if (body != null) body.localRotation = Quaternion.Euler(18f, 0, 0);
        Pose(30f, 0);
        SetCore(new Color(.25f, .22f, .2f), 0);
    }
}
