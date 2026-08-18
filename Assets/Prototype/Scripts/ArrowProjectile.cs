using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Health health = player.GetComponent<Health>();
            if (health != null) health.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (other.GetComponent<ArrowProjectile>() != null || other.GetComponent<ArcherEnemy>() != null) return;

        Destroy(gameObject);
    }
}
