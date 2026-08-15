using UnityEngine;
using UnityEngine.SceneManagement;

public class HandTrackingBootstrapper : MonoBehaviour
{
    [SerializeField] private string handTrackingSceneName = "Hand Landmark Detection";

    private void Awake()
    {
        if (!SceneManager.GetSceneByName(handTrackingSceneName).isLoaded)
            SceneManager.LoadScene(handTrackingSceneName, LoadSceneMode.Additive);
    }
}
