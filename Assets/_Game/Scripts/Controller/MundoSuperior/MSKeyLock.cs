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
            PlayerInteraction.Perform(player, transform, () =>
            {
                hud?.Notify("Cierre de la cima", "Falta el medallón de la llave. Está en el altar del camino de la llave.", UIIcon.Info, UIPalette.Muted);
                MSAudio.Unavailable();
            }, "Reach");
            return;
        }
        // The medallion goes in, and the leaf swings, when the hands reach the lock.
        _opening = true;
        PlayerInteraction.Perform(player, transform, WallFacing(player), () => { _opening = false; Insert(hud); }, "Open", "Reach");
    }

    // He opens facing the wall square on, not turned toward the lock beside the leaf (2026-10-07
    // playtest: the opening was played 45 degrees askew): the wall runs from the lock to the hinge.
    private Vector3? WallFacing(PlayerController player)
    {
        if (hinge == null) return null;
        Vector3 along = hinge.position - transform.position; along.y = 0;
        if (along.sqrMagnitude < .01f) return null;
        Vector3 normal = Vector3.Cross(Vector3.up, along.normalized);
        Vector3 toWall = transform.position - player.transform.position; toWall.y = 0;
        return Vector3.Dot(normal, toWall) < 0 ? -normal : normal;
    }

    private void Insert(MIHud hud)
    {
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
