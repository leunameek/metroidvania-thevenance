using UnityEngine;

// How a piece turns in the hands, as in the first prototype (InspectablePickup): the open left
// hand moving sideways turns it (yaw), the open right hand moving up or down tilts it (pitch),
// and closing both fists freezes it where it is. Shared by the plaza stations' pieces, the urns
// and the finds of both worlds, so every inspection answers the same gestures.
public static class HandInspection
{
    private const float DeadZone = .0015f, Clamp = .04f, Degrees = 300f;

    // Degrees to add this frame. True when both fists are closed (the piece stays still).
    public static bool Read(HandGestureTracker tracker, float sensitivity, out float yaw, out float pitch)
    {
        yaw = pitch = 0;
        if (tracker == null) return false;
        float x = tracker.ConsumeLeftHandDeltaX();
        float y = tracker.ConsumeRightHandDeltaY();
        if (Frozen(tracker)) return true;
        x = Mathf.Abs(x) < DeadZone ? 0 : Mathf.Clamp(x, -Clamp, Clamp);
        y = Mathf.Abs(y) < DeadZone ? 0 : Mathf.Clamp(y, -Clamp, Clamp);
        yaw = -x * Degrees * sensitivity;
        pitch = y * Degrees * sensitivity;
        return false;
    }

    public static bool Frozen(HandGestureTracker tracker) =>
        tracker != null && tracker.LeftHandPresent && tracker.RightHandPresent && !tracker.LeftHandOpen && !tracker.RightHandOpen;

    // Drops the movement gathered before the inspection opened.
    public static void Reset(HandGestureTracker tracker)
    {
        if (tracker == null) return;
        tracker.ConsumeLeftHandDeltaX();
        tracker.ConsumeRightHandDeltaY();
    }

    public const string Guide = "Mano izquierda abierta: girar · mano derecha abierta: inclinar · dos puños: detener";
}
