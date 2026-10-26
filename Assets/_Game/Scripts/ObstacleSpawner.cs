using System.Collections;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private Transform trackParent;
    [SerializeField] private float spawnX = 180f;
    [SerializeField] private float lockedY = 23.5f;
    [SerializeField] private float minInterval = 1.4f;
    [SerializeField] private float maxInterval = 2.4f;

    private Coroutine spawnRoutine;

    public void StartSpawning()
    {
        StopSpawning();
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        
        foreach (Transform child in trackParent)
        {
            if (child != transform) Destroy(child.gameObject);
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (RunnerGameManager.Instance.IsRunning)
        {
            float dynamicInterval = Random.Range(minInterval, maxInterval) / RunnerGameManager.Instance.SpeedMultiplier;
            yield return new WaitForSeconds(dynamicInterval);

            if (obstaclePrefabs.Length > 0 && RunnerGameManager.Instance.IsRunning)
            {
                GameObject chosenPrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
                GameObject inst = Instantiate(chosenPrefab, trackParent);
                inst.transform.localPosition = new Vector3(spawnX, lockedY, 0f);
            }
        }
    }
}