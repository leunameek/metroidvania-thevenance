using System;

// The main encounters of the campaign (guion 07, E07-E11). The plaza training (E01) keeps
// PlazaCombatModel; the common lower-world creatures (E03-E06) are fought with the dash.
public static class DuelEncounters
{
    public static DuelRules Create(string id)
    {
        switch (id)
        {
            case HybridGuardianRules.EncounterId: return new HybridGuardianRules();
            case SerpentRules.EncounterId: return new SerpentRules();
            case CondorRules.EncounterId: return new CondorRules();
            case EagleRules.EncounterId: return new EagleRules();
            case QuimueRules.EncounterId: return new QuimueRules();
            default: throw new ArgumentException("Unknown encounter " + id);
        }
    }
}

// E07 — caimán-murciélago, MI09. Two bonds broken only by Jaguar while the core is exposed;
// the second one once health is 80 or less. Cuerno opens the core for the next turn.
public sealed class HybridGuardianRules : DuelRules
{
    public const string EncounterId = "E07";
    public override string Id => EncounterId;
    public override string Name => "Guardián caimán-murciélago";
    public override int MaxHealth => 160;
    public override int PhaseTwoAt => 80;
    public override string Opening => "El núcleo está cerrado: defiende bien o usa Cuerno para abrirlo.";
    public override string Victory => "Las dos ataduras ceden. Chía vuelve a responder.";

    public int BondsLeft { get; private set; } = 2;
    public bool CoreOpen { get; private set; }
    private bool _hornNext;
    private int _step;

    private static readonly DuelMove Sweep = M("barrido", "Barrido de ala", true, 20, DuelDefense.Dodge, DuelDefense.Parry);
    private static readonly DuelMove Wave = M("onda", "Onda en el suelo", false, 20, DuelDefense.Block);
    private static readonly DuelMove Stones = M("piedras", "Caen piedras", false, 20, DuelDefense.Cover);

    public override string Status(TurnDuelModel duel) =>
        $"Ataduras {BondsLeft} / 2   ·   Núcleo {(CoreOpen ? "expuesto" : "cerrado")}";
    public override string Progress(TurnDuelModel duel) => $"Ataduras {BondsLeft} / 2";
    public bool JaguarBreaksBond(TurnDuelModel duel) =>
        CoreOpen && (BondsLeft == 2 || (BondsLeft == 1 && duel.EnemyHealth <= 80));

    public override bool IsLegal(TurnDuelModel duel, DuelAction action, ref string reason)
    {
        if (action == DuelAction.Horn) return true;
        if (action == DuelAction.Jaguar && !CoreOpen) { reason = "El jaguar necesita el núcleo expuesto."; return false; }
        return base.IsLegal(duel, action, ref reason);
    }
    public override void Apply(TurnDuelModel duel, ref DuelHit hit)
    {
        if (hit.Action == DuelAction.Horn)
        {
            _hornNext = true; hit.Text = "El cuerno responde: el núcleo se abrirá en tu próximo turno."; return;
        }
        if (hit.Action == DuelAction.Jaguar && JaguarBreaksBond(duel))
        {
            BondsLeft--;
            hit.Text = $"¡Jaguar! Una atadura se rompe ({hit.Damage} de daño). Quedan {BondsLeft}.";
        }
        else if (!CoreOpen && hit.Action != DuelAction.Jaguar)
        {
            hit.Damage /= 2; hit.Text = $"Núcleo cerrado: {hit.Damage} de daño.";
        }
        else hit.Text = $"{TurnDuelModel.ActionName(hit.Action)} al núcleo: {hit.Damage} de daño.";
        if (BondsLeft > 0 && duel.EnemyHealth - hit.Damage <= 1)
            hit.Text += " Una atadura sigue en pie: espera el núcleo expuesto y usa Jaguar.";
    }
    public override int MinimumHealth(TurnDuelModel duel) => BondsLeft > 0 ? 1 : 0;
    public override DuelMove NextMove(TurnDuelModel duel)
    {
        CoreOpen = false;
        // Phase 1 repeats the full cycle; phase 2 alternates two patterns in successive rounds.
        DuelMove[] cycle = duel.PhaseTwo ? new[] { Stones, Wave, Sweep, Wave } : new[] { Sweep, Wave, Stones };
        return cycle[_step++ % cycle.Length];
    }
    public override void OnResolved(TurnDuelModel duel, DuelMove move, bool correct)
    {
        CoreOpen = correct || _hornNext;
        _hornNext = false;
        if (CoreOpen) duel.Say(duel.Message + " El núcleo queda expuesto.");
    }
}

// E08 — serpiente bicéfala, MS08. A correct defense makes the announcing head vulnerable.
// Hitting the vulnerable head cuts its bond (40 each, part of the 180 of health).
public sealed class SerpentRules : DuelRules
{
    public const string EncounterId = "E08";
    public override string Id => EncounterId;
    public override string Name => "Serpiente bicéfala";
    public override int MaxHealth => 180;
    public override int PhaseTwoAt => 90;
    public override string Opening => "La cabeza A está vulnerable. Elige cabeza y luego Atacar.";
    public override string Victory => "Ninguna cabeza obedece ya. Sué devuelve la dirección.";
    public override DuelTarget[] Targets => new[] { DuelTarget.HeadA, DuelTarget.HeadB };

    public DuelTarget Vulnerable { get; private set; } = DuelTarget.HeadA;
    public int BondA { get; private set; } = 40;
    public int BondB { get; private set; } = 40;
    private int _step;

    private static DuelMove Press => Head(M("presion", "Presión frontal", true, 20, DuelDefense.Block, DuelDefense.Parry), DuelTarget.HeadA);
    private static DuelMove Sweep => Head(M("barrido", "Barrido largo", true, 20, DuelDefense.Dodge, DuelDefense.Parry), DuelTarget.HeadB);
    private static DuelMove Pulse => Head(M("pulso", "Pulso de garganta", false, 20, DuelDefense.Block), DuelTarget.HeadB);
    private static DuelMove Shards => Head(M("fragmentos", "Lluvia de fragmentos", false, 20, DuelDefense.Cover), DuelTarget.HeadA);
    private static DuelMove Head(DuelMove m, DuelTarget t) { m.Origin = t; return m; }

    public override bool NeedsTarget(DuelAction action) => action != DuelAction.Horn;
    public override string Status(TurnDuelModel duel) =>
        $"Vulnerable: {TurnDuelModel.TargetName(Vulnerable)}   ·   Vínculo A {BondA}   ·   Vínculo B {BondB}";
    public override string Progress(TurnDuelModel duel) => $"Vínculo A {BondA}   ·   Vínculo B {BondB}";
    public override void Apply(TurnDuelModel duel, ref DuelHit hit)
    {
        string head = TurnDuelModel.TargetName(hit.Target);
        if (hit.Target != Vulnerable)
        {
            hit.Damage /= 2; hit.Text = $"{head} no estaba expuesta: {hit.Damage} al cuerpo."; return;
        }
        int bond = hit.Target == DuelTarget.HeadA ? BondA : BondB;
        int cut = Math.Min(bond, hit.Damage);
        if (hit.Target == DuelTarget.HeadA) BondA -= cut; else BondB -= cut;
        bond -= cut;
        hit.Text = cut > 0
            ? $"{head}: {hit.Damage} de daño, su vínculo {(bond == 0 ? "se corta" : "queda en " + bond)}."
            : $"{head}: {hit.Damage} de daño.";
        if (BondA + BondB > 0 && duel.EnemyHealth - hit.Damage <= 1)
            hit.Text += " Falta cortar un vínculo.";
    }
    public override int MinimumHealth(TurnDuelModel duel) => BondA + BondB > 0 ? 1 : 0;
    public override DuelMove NextMove(TurnDuelModel duel)
    {
        DuelMove[] cycle = duel.PhaseTwo ? new[] { Press, Pulse, Shards, Sweep } : new[] { Press, Sweep };
        return cycle[_step++ % cycle.Length];
    }
    public override void OnResolved(TurnDuelModel duel, DuelMove move, bool correct)
    {
        if (correct) Vulnerable = move.Origin;
        duel.Say(duel.Message + " Vulnerable: " + TurnDuelModel.TargetName(Vulnerable) + ".");
    }
    // A cut bond whose head is targeted again only hurts the body; if both heads are cut the
    // vulnerable one stays wherever the last defense left it.
}

// E09 — mujer-cóndor, MS03 annex. Anclar nullifies the next wind round and gives 1 concentration.
public sealed class CondorRules : DuelRules
{
    public const string EncounterId = "E09";
    public override string Id => EncounterId;
    public override string Name => "Mujer-cóndor";
    public override int MaxHealth => 60;
    public override string Opening => "Alcanzar altura no basta. Anclar te sostiene contra el viento.";
    public override string Victory => "La mujer-cóndor pliega las alas: te reconoce.";

    private bool _anchored;
    public override string Status(TurnDuelModel duel) => _anchored ? "Anclado contra el viento" : "";
    private int _step;
    private static DuelMove Wing => M("ala", "Golpe de ala frontal", true, 15, DuelDefense.Block, DuelDefense.Parry);
    private static DuelMove Wind
    {
        get { var m = M("viento", "Viento lateral", false, 15, DuelDefense.Dodge); m.Wind = true; return m; }
    }

    public override bool IsLegal(TurnDuelModel duel, DuelAction action, ref string reason)
    {
        if (action == DuelAction.Anchor) return true;
        return base.IsLegal(duel, action, ref reason);
    }
    public override void Apply(TurnDuelModel duel, ref DuelHit hit)
    {
        if (hit.Action == DuelAction.Anchor) { _anchored = true; hit.Text = "Anclas los pies: el próximo viento no te moverá."; return; }
        base.Apply(duel, ref hit);
    }
    public override DuelMove NextMove(TurnDuelModel duel) => _step++ % 2 == 0 ? Wing : Wind;
    public override bool Skip(TurnDuelModel duel, DuelMove move, out string why)
    {
        why = "";
        if (!move.Wind || !_anchored) return false;
        _anchored = false;
        duel.GrantConcentration(1);
        why = "Viento lateral: sigues anclado. +1 concentración.";
        return true;
    }
}

// E10 — mujer-águila, MS06. Elevated she takes half damage; Interrumpir (D/2) grounds her and
// cancels her next charge. She rises again when she completes a charge.
public sealed class EagleRules : DuelRules
{
    public const string EncounterId = "E10";
    public override string Id => EncounterId;
    public override string Name => "Mujer-águila";
    public override int MaxHealth => 60;
    public override string Opening => "Está en lo alto: Interrumpir la baja al suelo.";
    public override string Victory => "La mujer-águila baja las alas. Arriba, mira cuál cabeza anuncia.";

    public bool Elevated { get; private set; } = true;
    public override string Status(TurnDuelModel duel) => Elevated ? "En lo alto: recibe la mitad del daño" : "En el suelo";
    private bool _chargeCancelled;
    private int _step;
    private static DuelMove ChargeMove
    {
        get { var m = M("carga", "Carga aérea", true, 15, DuelDefense.Dodge, DuelDefense.Parry); m.Charge = true; return m; }
    }
    private static DuelMove Feathers => M("plumas", "Descarga de plumas", false, 15, DuelDefense.Cover, DuelDefense.Block);

    public override bool IsLegal(TurnDuelModel duel, DuelAction action, ref string reason)
    {
        if (action == DuelAction.Interrupt)
        {
            if (Elevated) return true;
            reason = "Ya está en el suelo."; return false;
        }
        return base.IsLegal(duel, action, ref reason);
    }
    public override void Apply(TurnDuelModel duel, ref DuelHit hit)
    {
        if (hit.Action == DuelAction.Interrupt)
        {
            hit.Damage = Math.Max(1, hit.Base / 2); Elevated = false; _chargeCancelled = true;
            hit.Text = $"Interrumpes: {hit.Damage} de daño y baja al suelo."; return;
        }
        if (Elevated) { hit.Damage /= 2; hit.Text = $"Está en lo alto: {hit.Damage} de daño."; return; }
        base.Apply(duel, ref hit);
    }
    public override DuelMove NextMove(TurnDuelModel duel) => _step++ % 2 == 0 ? ChargeMove : Feathers;
    public override bool Skip(TurnDuelModel duel, DuelMove move, out string why)
    {
        why = "";
        if (!move.Charge || !_chargeCancelled) return false;
        _chargeCancelled = false;
        why = "La carga quedó interrumpida. Tu turno.";
        return true;
    }
    public override void OnResolved(TurnDuelModel duel, DuelMove move, bool correct)
    {
        if (move.Charge) Elevated = true;
    }
}

// E11 — Quimue, plaza circle. Two anchors (moon/sun, 40 each, inside the 200 of health). The
// active origin is the last announced one; Vincular on it cuts that anchor. Phase 2 adds the
// union: two consecutive defensive rounds (moon then sun), each with its own window.
public sealed class QuimueRules : DuelRules
{
    public const string EncounterId = "E11";
    public override string Id => EncounterId;
    public override string Name => "Quimue";
    public override int MaxHealth => 200;
    public override int PhaseTwoAt => 100;
    public override string Opening => "Origen activo: Luna. Elige Luna o Sol y usa Vincular para cortar su anclaje.";
    public override string Victory => "Los lazos de luna y sol se apagan. Quimue cae de rodillas.";
    public override DuelTarget[] Targets => new[] { DuelTarget.Moon, DuelTarget.Sun };

    public DuelTarget ActiveOrigin { get; private set; } = DuelTarget.Moon;
    public int MoonAnchor { get; private set; } = 40;
    public int SunAnchor { get; private set; } = 40;
    public bool AnchorsCut => MoonAnchor == 0 && SunAnchor == 0;
    public override string Status(TurnDuelModel duel) =>
        $"Origen activo: {TurnDuelModel.TargetName(ActiveOrigin)}   ·   Luna {MoonAnchor}   ·   Sol {SunAnchor}";
    public override string Progress(TurnDuelModel duel) => $"Luna {MoonAnchor}   ·   Sol {SunAnchor}";
    private int _step;
    private bool _unionPending;

    private static DuelMove Lunar(string label = "Barrido lunar")
    {
        var m = M("lunar", label, true, 20, DuelDefense.Dodge, DuelDefense.Parry); m.Origin = DuelTarget.Moon; return m;
    }
    private static DuelMove Solar(string label = "Golpe solar frontal")
    {
        var m = M("solar", label, true, 20, DuelDefense.Block, DuelDefense.Parry); m.Origin = DuelTarget.Sun; return m;
    }

    public override bool IsLegal(TurnDuelModel duel, DuelAction action, ref string reason)
    {
        if (action == DuelAction.Bind)
        {
            if (!AnchorsCut) return true;
            reason = "Ya no quedan anclajes."; return false;
        }
        if (action == DuelAction.Jaguar && !AnchorsCut) { reason = "Primero corta los dos anclajes."; return false; }
        return base.IsLegal(duel, action, ref reason);
    }
    public override bool NeedsTarget(DuelAction action) => action == DuelAction.Bind;
    public override void Apply(TurnDuelModel duel, ref DuelHit hit)
    {
        if (hit.Action == DuelAction.Bind)
        {
            if (hit.Target != ActiveOrigin)
            {
                hit.Damage /= 2;
                hit.Text = $"{TurnDuelModel.TargetName(hit.Target)} no es el origen activo ({TurnDuelModel.TargetName(ActiveOrigin)}): {hit.Damage} de daño, sin corte.";
                return;
            }
            int left = hit.Target == DuelTarget.Moon ? MoonAnchor : SunAnchor;
            int cut = Math.Min(left, hit.Damage);
            if (hit.Target == DuelTarget.Moon) MoonAnchor -= cut; else SunAnchor -= cut;
            left -= cut;
            hit.Text = $"Vincular {TurnDuelModel.TargetName(hit.Target)}: {hit.Damage} de daño, anclaje {(left == 0 ? "cortado" : "en " + left)}.";
            if (left == 0 && cut == 0) hit.Text = $"Ese anclaje ya está cortado: {hit.Damage} de daño.";
            return;
        }
        if (!AnchorsCut && hit.Action == DuelAction.Attack) { hit.Damage /= 2; hit.Text = $"Los anclajes lo protegen: {hit.Damage} de daño."; return; }
        base.Apply(duel, ref hit);
    }
    public override int MinimumHealth(TurnDuelModel duel) => AnchorsCut ? 0 : 1;
    public override DuelMove NextMove(TurnDuelModel duel)
    {
        if (duel.PhaseTwo && _step % 3 == 2) { _step++; _unionPending = true; return Lunar("Unión 1/2: barrido lunar"); }
        return _step++ % 2 == 0 ? Lunar() : Solar();
    }
    public override DuelMove FollowUp(TurnDuelModel duel)
    {
        if (!_unionPending) return null;
        _unionPending = false;
        return Solar("Unión 2/2: golpe solar");
    }
    public override void OnResolved(TurnDuelModel duel, DuelMove move, bool correct)
    {
        ActiveOrigin = move.Origin;
        duel.Say(duel.Message + " Origen activo: " + TurnDuelModel.TargetName(ActiveOrigin) + ".");
    }
}
