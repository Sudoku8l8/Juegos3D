using UnityEngine;

public class KillObstacle : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damageAmount);
            return;
        }

        playerController player = other.GetComponent<playerController>();
        if (player != null)
        {
            player.Die();
        }
    }
}