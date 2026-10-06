using System.Collections;
using UnityEngine;

namespace Nemequene.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UIPanelTransition : MonoBehaviour
    {
        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            var group = GetComponent<CanvasGroup>();
            if (UIManager.Instance == null || UIManager.Instance.Settings.Values.reducedMotion) { group.alpha = 1; return; }
            StartCoroutine(Fade(group));
        }
        private IEnumerator Fade(CanvasGroup group)
        {
            for (float t = 0; t < .22f; t += Time.unscaledDeltaTime)
            { group.alpha = .85f + .15f * (1-Mathf.Pow(1-t/.22f,3)); yield return null; }
            group.alpha = 1;
        }
        private void OnDisable() { StopAllCoroutines(); GetComponent<CanvasGroup>().alpha=1; }
    }
}
