using UnityEngine;

[RequireComponent(typeof(Health))]
public class DestroyOnDeath : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<Health>().Died += HandleDied;
    }

    private void OnDisable()
    {
        GetComponent<Health>().Died -= HandleDied;
    }

    private void HandleDied()
    {
        Destroy(gameObject);
    }
}
