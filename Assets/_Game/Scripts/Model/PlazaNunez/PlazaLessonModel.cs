using UnityEngine;

// Progress only comes from measured movement; opening an object never completes a lesson.
public sealed class PlazaLessonModel
{
    public int Lesson { get; }
    public float YawDegrees { get; private set; }
    public float PitchDegrees { get; private set; }
    public float FreezeSeconds { get; private set; }
    public bool UsedHands { get; private set; }
    public bool Complete => (Lesson == 1 || YawDegrees >= 28f)
        && (Lesson == 0 || PitchDegrees >= 28f) && (Lesson != 2 || FreezeSeconds >= 0.65f);
    public float Progress => Lesson == 0 ? Mathf.Clamp01(YawDegrees / 28f)
        : Lesson == 1 ? Mathf.Clamp01(PitchDegrees / 28f)
        : (Mathf.Clamp01(YawDegrees / 28f) + Mathf.Clamp01(PitchDegrees / 28f) + Mathf.Clamp01(FreezeSeconds / 0.65f)) / 3f;

    public PlazaLessonModel(int lesson) { Lesson = Mathf.Clamp(lesson, 0, 2); }

    public void RestoreComplete()
    {
        YawDegrees = 28f;
        PitchDegrees = 28f;
        FreezeSeconds = .65f;
    }

    public void Move(float yaw, float pitch, bool hands)
    {
        if (Complete) return;
        YawDegrees += Mathf.Min(Mathf.Abs(yaw), 12f);
        PitchDegrees += Mathf.Min(Mathf.Abs(pitch), 12f);
        if (hands && (Mathf.Abs(yaw) > 0.1f || Mathf.Abs(pitch) > 0.1f)) UsedHands = true;
    }

    public void Freeze(bool held, float deltaTime)
    {
        if (Complete || Lesson != 2) return;
        if (YawDegrees < 28f || PitchDegrees < 28f || !held) FreezeSeconds = 0;
        else FreezeSeconds += Mathf.Max(0, deltaTime);
    }
}
