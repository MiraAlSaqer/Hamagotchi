using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float baseSpeed = 80f;
    [SerializeField] private float despawnX = -180f;

    void Update()
    {
        if (!RunnerGameManager.Instance.IsRunning) return;

        float speed = baseSpeed * RunnerGameManager.Instance.SpeedMultiplier;
        transform.localPosition += Vector3.left * speed * Time.deltaTime;

        if (transform.localPosition.x < despawnX)
        {
            Destroy(gameObject);
        }
    }
}