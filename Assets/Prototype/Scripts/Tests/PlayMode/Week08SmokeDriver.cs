#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEngine;

// Test-only bridge so smoke steps and delays run on game frames, not editor UI ticks.
public sealed class Week08SmokeDriver : MonoBehaviour
{
    public void Run(IEnumerator steps, Action<int> completed)
    {
        StartCoroutine(Execute(steps, completed));
    }

    private IEnumerator Execute(IEnumerator steps, Action<int> completed)
    {
        while (true)
        {
            bool hasNext;
            float delay;
            try
            {
                hasNext = steps.MoveNext();
                delay = hasNext ? Convert.ToSingle(steps.Current) : 0f;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                completed(1);
                yield break;
            }
            if (!hasNext)
            {
                Debug.Log("WEEK08_SMOKE_PASS: movimiento, carrera, salto, colisión, pendiente, cámara, objetos, caída y reinicio.");
                completed(0);
                yield break;
            }
            yield return new WaitForSeconds(delay);
        }
    }
}
#endif
