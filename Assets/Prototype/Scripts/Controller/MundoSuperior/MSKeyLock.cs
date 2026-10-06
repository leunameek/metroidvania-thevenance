using System.Collections;
using Nemequene.UI;
using UnityEngine;

// O10 receptacle of the 07 lock (guide 5.10 and 6.5). Without the key E only explains what is
// missing; with it one E writes ms_cierre_abierto before animating, shows the inserted medallion
// and swings the A04 leaf 95 degrees in 0.8 s towards its niche. The key is never consumed, and a
// saved open lock loads already open.
public sealed class MSKeyLock : MIInteractable
{
    [SerializeField] private Transform hinge;
    [SerializeField] private GameObject placedMedallion;
    [SerializeField] private float openAngle = 95f, duration = .8f;

    private bool _opening;

    public bool Open => MSProgress.Has(MSProgress.LockOpen);
    public override bool Available => base.Available && !Open && !_opening;
    public override string Prompt => MSProgress.Has(MSProgress.Key) ? "Colocar el medallón" : "Falta el medallón de la llave";

    private void Start()
    {
        if (placedMedallion != null) placedMedallion.SetActive(Open);
        if (Open && hinge != null) hinge.localRotation = Quaternion.Euler(0, openAngle, 0);
    }

    public override void Interact(PlayerController player)
    {
        var hud = MundoSuperiorDirector.Instance?.Hud;
        if (!MSProgress.Has(MSProgress.Key))
        {
            hud?.Notify("Cierre de la cima", "Falta el medallón de la llave. Está en el altar del camino de la llave.", UIIcon.Info, UIPalette.Muted);
            MSAudio.Unavailable();
            return;
        }
        MSProgress.Set(MSProgress.LockOpen);
        if (placedMedallion != null) placedMedallion.SetActive(true);
        MSAudio.Play("cierre_insertar", .9f);
        hud?.Notify("Cierre abierto", "El paso a la cima queda abierto.", UIIcon.Objective, UIPalette.GoldLight);
        MundoSuperiorDirector.Instance?.RefreshObjective();
        StartCoroutine(Swing());
    }

    private IEnumerator Swing()
    {
        _opening = true;
        if (hinge != null) MSAudio.PlayAt("cierre_puerta", hinge.position, 1f);
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            if (hinge != null) hinge.localRotation = Quaternion.Euler(0, Mathf.SmoothStep(0, openAngle, t / duration), 0);
            yield return null;
        }
        if (hinge != null) hinge.localRotation = Quaternion.Euler(0, openAngle, 0);
        _opening = false;
    }
}
