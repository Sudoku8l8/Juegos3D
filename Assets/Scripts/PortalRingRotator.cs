using UnityEngine;

/// <summary>
/// Rota continuamente el GameObject sobre su eje Y para crear el efecto de
/// anillos orbitantes del portal de energia.
/// </summary>
public class PortalRingRotator : MonoBehaviour
{
    [Tooltip("Grados por segundo. Negativo invierte la direccion.")]
    public float speed = 60f;

    private void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime, Space.World);
    }
}
