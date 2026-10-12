using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private Color activatedColor = Color.green;
    private Renderer checkpointRenderer;
    private bool isActivated = false;

    private void Start()
    {
        checkpointRenderer = GetComponent<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        playerController player = other.GetComponent<playerController>();
        if (player != null && !isActivated)
        {
            isActivated = true;
            player.SetSpawnPoint(transform);
            Debug.Log("[CheckPoint] ¡Punto de control activado!");

            if (checkpointRenderer != null)
            {
                checkpointRenderer.material.color = activatedColor;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.SoundType.Checkpoint);
            }
        }
    }
}