using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public sealed class LevelTeleportTrigger : MonoBehaviour
{
    [Header("Destination")]
    [Tooltip("Name or asset path of a scene included in Build Settings.")]
    [SerializeField] private string destinationSceneName;

    private bool _isLoading;

    private void OnTriggerEnter(Collider other)
    {
        if (_isLoading || other.GetComponentInParent<PlayerController>() == null) return;

        string destination = destinationSceneName?.Trim();
        if (string.IsNullOrEmpty(destination))
        {
            Debug.LogError($"{name}: no destination scene was configured.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(destination))
        {
            Debug.LogError(
                $"{name}: scene '{destination}' is not available. Add it to File > Build Profiles > Scene List.",
                this);
            return;
        }

        _isLoading = true;
        SceneManager.LoadScene(destination, LoadSceneMode.Single);
    }

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnValidate()
    {
        BoxCollider trigger = GetComponent<BoxCollider>();
        if (trigger != null) trigger.isTrigger = true;
    }
}
