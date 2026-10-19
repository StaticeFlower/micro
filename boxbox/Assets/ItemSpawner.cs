
using System.Collections;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Ball Prefabs")]
    public GameObject whiteBallPrefab;
    public GameObject pinkBallPrefab;
    public GameObject greenBallPrefab;
    public GameObject redBallPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 1.5f;
    public float xMin = -7f;
    public float xMax = 7f;
    public float spawnY = 6f;
    public float destroyYBoundary = -6f;

    [Header("Level Spawn Settings")]
    public float level2SpawnInterval = 1.0f;
    public float level3SpawnInterval = 0.7f;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnItem();
            yield return new WaitForSeconds(GetSpawnInterval());
        }
    }

    float GetSpawnInterval()
    {
        int level = LevelManager.Instance != null
            ? LevelManager.Instance.currentLevel
            : 1;

        switch (level)
        {
            case 2:
                return level2SpawnInterval;
            case 3:
                return level3SpawnInterval;
            default:
                return spawnInterval;
        }
    }

    void SpawnItem()
    {
        GameObject[] balls =
        {
            whiteBallPrefab,
            pinkBallPrefab,
            greenBallPrefab,
            redBallPrefab
        };

        // สุ่มเฉพาะ Prefab ที่กำหนดไว้แล้ว
        System.Collections.Generic.List<GameObject> availableBalls =
            new System.Collections.Generic.List<GameObject>();

        foreach (GameObject ball in balls)
        {
            if (ball != null)
                availableBalls.Add(ball);
        }

        if (availableBalls.Count == 0)
        {
            Debug.LogWarning("กรุณากำหนด Ball Prefabs ใน Inspector");
            return;
        }

        GameObject selectedBall =
            availableBalls[Random.Range(0, availableBalls.Count)];

        float randomX = Random.Range(xMin, xMax);

        Vector3 spawnPosition =
            new Vector3(randomX, spawnY, 0f);

        GameObject newItem = Instantiate(
            selectedBall,
            spawnPosition,
            Quaternion.identity
        );

        AutoDestroyWhenOutOfBounds autoDestroy =
            newItem.GetComponent<AutoDestroyWhenOutOfBounds>();

        if (autoDestroy == null)
        {
            autoDestroy =
                newItem.AddComponent<AutoDestroyWhenOutOfBounds>();
        }

        autoDestroy.bottomY = destroyYBoundary;
    }
}

public class AutoDestroyWhenOutOfBounds : MonoBehaviour
{
    public float bottomY = -6f;

    void Update()
    {
        if (transform.position.y < bottomY)
        {
            ItemCollector collector =
            FindAnyObjectByType<ItemCollector>();

            if (collector != null)
            {
                collector.ResetCombo();
            }

            Destroy(gameObject);
        }
    }
}
