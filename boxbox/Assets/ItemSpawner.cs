
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Item Prefabs")]
    public GameObject whiteBallPrefab;
    public GameObject greenBallPrefab;
    public GameObject redBallPrefab;
    public GameObject pinkBallPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 1.5f;
    public float spawnY = 6f;
    public float xMin = -8f;
    public float xMax = 8f;
    public float destroyYBoundary = -6f;

    [Header("Spawn Probabilities (%)")]
    [Range(0, 100)] public int whiteChance = 50;
    [Range(0, 100)] public int greenChance = 25;
    [Range(0, 100)] public int redChance = 15;
    [Range(0, 100)] public int pinkChance = 10;

    private List<GameObject> activeItems = new List<GameObject>();
    private float timer = 0f;

    private void Update()
    {
        // 1. จับเวลาสปอนบอล
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnItem();
            timer = 0f;
        }

        // 2. ใช้ความเร็วจาก LevelManager
        float currentSpeed = LevelManager.Instance != null
            ? LevelManager.Instance.CurrentSpeed
            : 5f;

        // 3. เคลื่อนที่บอลและตรวจสอบการตกหลุดจอ
        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            GameObject item = activeItems[i];

            if (item == null)
            {
                activeItems.RemoveAt(i);
                continue;
            }

            item.transform.Translate(
                Vector3.down * currentSpeed * Time.deltaTime
            );

            if (item.transform.position.y <= destroyYBoundary)
            {
                // ตรวจว่าเป็นบอลสีขาวจริงหรือไม่
                // Item(Clone) = บอลขาว
                // Item 1(Clone) = บอลเขียว
                // Item 2(Clone) = บอลแดง
                string ballName = item.name
                    .Replace("(Clone)", "")
                    .Trim();

                if (ballName == "Item")
                {
                    // บอลขาวตกหลุดจอ: รีเซ็ต Combo เท่านั้น
                    if (ItemCollector.Instance != null)
                    {
                        ItemCollector.Instance.ResetCombo();
                    }
                }

                // บอลทุกสีที่ตกหลุดจอจะถูกลบ
                Destroy(item);
                activeItems.RemoveAt(i);
            }
        }
    }

    private void SpawnItem()
    {
        GameObject prefabToSpawn = GetRandomItemPrefab();

        if (prefabToSpawn == null)
        {
            return;
        }

        // สุ่มตำแหน่ง X
        float randomX = Random.Range(xMin, xMax);

        Vector3 spawnPosition = new Vector3(
            randomX,
            spawnY,
            0f
        );

        // สร้างบอล
        GameObject newItem = Instantiate(
            prefabToSpawn,
            spawnPosition,
            Quaternion.identity
        );

        activeItems.Add(newItem);

        // นับ Items เฉพาะบอลสีขาวที่ Spawn
        string ballName = newItem.name
            .Replace("(Clone)", "")
            .Trim();

        if (ballName == "Item")
        {
            if (ItemCollector.Instance != null)
            {
                ItemCollector.Instance.AddWhiteBallItemCount();
            }
        }
    }

    private GameObject GetRandomItemPrefab()
    {
        int totalWeight =
            whiteChance +
            greenChance +
            redChance +
            pinkChance;

        if (totalWeight <= 0)
        {
            return whiteBallPrefab;
        }

        int randomValue = Random.Range(0, totalWeight);

        // บอลสีขาว
        if (randomValue < whiteChance)
        {
            return whiteBallPrefab;
        }

        randomValue -= whiteChance;

        // บอลสีเขียว
        if (randomValue < greenChance)
        {
            return greenBallPrefab != null
                ? greenBallPrefab
                : whiteBallPrefab;
        }

        randomValue -= greenChance;

        // บอลสีแดง
        if (randomValue < redChance)
        {
            return redBallPrefab != null
                ? redBallPrefab
                : whiteBallPrefab;
        }

        // บอลสีชมพู
        return pinkBallPrefab != null
            ? pinkBallPrefab
            : whiteBallPrefab;
    }
}
