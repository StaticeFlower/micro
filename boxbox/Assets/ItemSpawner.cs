using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // ใช้สำหรับ TextMeshPro (หากใช้ UI Text ปกติ ให้เปลี่ยนเป็น UnityEngine.UI)

public class ItemSpawner : MonoBehaviour
{
    [Header("Item Prefabs")]
    public GameObject whiteBallPrefab;
    public GameObject greenBallPrefab;
    public GameObject redBallPrefab;
    public GameObject pinkBallPrefab;
    public GameObject item3Prefab;

    [Header("New Item Prefabs")]
    public GameObject item4Prefab; // +5 Combo
    public GameObject item5Prefab; // +10 Combo
    public GameObject item6Prefab; // -10 Combo
    public GameObject item7Prefab; // -1 Missed (ลดค่าการพลาดลง 1)

    [Header("Spawn Settings")]
    public float spawnInterval = 1.5f;
    public float spawnY = 6f;
    public float xMin = -8f;
    public float xMax = 8f;
    public float destroyYBoundary = -6f;

    [Header("Defeat Settings (ระบบแพ้)")]
    public int maxMissedWhiteBalls = 5; // จำนวนครั้งที่พลาดได้
    private int missedWhiteBallCount = 0; // ตัวนับที่พลาดไปแล้ว
    private bool isGameOver = false; // สถานะจบเกม
    public GameObject defeatUI; // ลาก UI / Text คำว่า Defeat มาใส่ตรงนี้ใน Inspector

    [Header("Spawn Probabilities (%)")]
    [Range(0, 100)] public int whiteChance = 50;
    [Range(0, 100)] public int greenChance = 25;
    [Range(0, 100)] public int redChance = 15;
    [Range(0, 100)] public int pinkChance = 10;
    [Range(0, 100)] public int item3Chance = 10;
    [Range(0, 100)] public int item4Chance = 5;
    [Range(0, 100)] public int item5Chance = 5;
    [Range(0, 100)] public int item6Chance = 5;
    [Range(0, 100)] public int item7Chance = 5; // โอกาสเกิดของ item 7

    private Dictionary<GameObject, float> horizontalSpeeds =
        new Dictionary<GameObject, float>();

    private List<GameObject> activeItems = new List<GameObject>();
    private float timer = 0f;

    private void Update()
    {
        // หากเกมจบแล้ว (แพ้) ให้หยุดการทำงานทั้งหมด
        if (isGameOver) return;

        // 1. Spawn ตามช่วงเวลาเดิม
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnItem();
            timer = 0f;
        }

        // 2. ใช้ความเร็วจาก LevelManager เหมือนเดิม
        float currentSpeed = LevelManager.Instance != null
            ? LevelManager.Instance.CurrentSpeed
            : 5f;

        // 3. เคลื่อนที่ไอเทม
        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            GameObject item = activeItems[i];

            if (item == null)
            {
                horizontalSpeeds.Remove(item);
                activeItems.RemoveAt(i);
                continue;
            }

            float horizontalSpeed = 0f;

            if (horizontalSpeeds.TryGetValue(item, out float savedSpeed))
            {
                horizontalSpeed = savedSpeed;
            }

            // ทุกไอเทมตกลงด้านล่าง
            // ไอเทมปกติตกตรง ส่วน Item 3-7 ตกเฉียง
            Vector3 movement = new Vector3(
                horizontalSpeed,
                -currentSpeed,
                0f
            );

            item.transform.Translate(movement * Time.deltaTime);

            if (item.transform.position.y <= destroyYBoundary)
            {
                string ballName = item.name.Replace("(Clone)", "").Trim();

                if (ballName == "Item")
                {
                    if (ItemCollector.Instance != null)
                    {
                        ItemCollector.Instance.ResetCombo();
                    }

                    // --- เพิ่มเงื่อนไขนับลูกบอลสีขาวที่ตกพื้น/พลาด ---
                    RegisterMissedWhiteBall();
                }

                horizontalSpeeds.Remove(item);
                Destroy(item);
                activeItems.RemoveAt(i);
            }
        }
    }

    private void RegisterMissedWhiteBall()
    {
        missedWhiteBallCount++;
        Debug.Log($"Missed White Ball: {missedWhiteBallCount}/{maxMissedWhiteBalls}");

        if (missedWhiteBallCount >= maxMissedWhiteBalls)
        {
            TriggerDefeat();
        }
    }

    // ฟังก์ชันสั่งลดค่านับ Missed เมื่อเก็บ item 7 ได้
    public void ReduceMissedCount()
    {
        if (missedWhiteBallCount > 0)
        {
            missedWhiteBallCount--;
            Debug.Log($"Reduced Missed Count! Current Missed: {missedWhiteBallCount}/{maxMissedWhiteBalls}");
        }
    }

    private void TriggerDefeat()
    {
        isGameOver = true;

        // แสดงข้อความ Defeat
        if (defeatUI != null)
        {
            defeatUI.SetActive(true);
        }

        Debug.Log("Game Over: Defeat!");
    }

    private void SpawnItem()
    {
        // ใช้ระบบสุ่มไอเทมเดิม
        GameObject prefabToSpawn = GetRandomItemPrefab();

        if (prefabToSpawn == null) return;

        float randomX = Random.Range(xMin, xMax);
        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);

        GameObject newItem = Instantiate(
            prefabToSpawn,
            spawnPosition,
            Quaternion.identity
        );

        activeItems.Add(newItem);

        // กำหนดความเร็วแนวนอน
        float horizontalSpeed = 0f;

        if (IsSpecialItem(newItem))
        {
            // สุ่มซ้ายหรือขวาเท่านั้น ไม่มีแนวดิ่ง
            int direction = Random.Range(0, 2);

            if (direction == 0)
            {
                horizontalSpeed = -Random.Range(1.5f, 3.5f);
            }
            else
            {
                horizontalSpeed = Random.Range(1.5f, 3.5f);
            }
        }

        horizontalSpeeds[newItem] = horizontalSpeed;

        // นับ Items ตามเงื่อนไขเดิม
        string ballName = newItem.name.Replace("(Clone)", "").Trim();

        if (ballName == "Item")
        {
            if (ItemCollector.Instance != null)
            {
                ItemCollector.Instance.AddWhiteBallItemCount();
            }
        }
    }

    private bool IsSpecialItem(GameObject item)
    {
        string ballName = item.name.Replace("(Clone)", "").Trim().ToLower();

        return ballName == "item 3"
            || ballName == "item 4"
            || ballName == "item 5"
            || ballName == "item 6"
            || ballName == "item 7"; // กำหนดให้ item 7 เป็น Special Item (ตกลงมาแบบเฉียง)
    }

    private GameObject GetRandomItemPrefab()
    {
        // ระบบสุ่มและน้ำหนักเดิมทั้งหมด
        int totalWeight =
            whiteChance + greenChance + redChance + pinkChance +
            item3Chance + item4Chance + item5Chance + item6Chance + item7Chance;

        if (totalWeight <= 0) return whiteBallPrefab;

        int randomValue = Random.Range(0, totalWeight);

        if (randomValue < whiteChance) return whiteBallPrefab;
        randomValue -= whiteChance;

        if (randomValue < greenChance)
            return greenBallPrefab != null ? greenBallPrefab : whiteBallPrefab;
        randomValue -= greenChance;

        if (randomValue < redChance)
            return redBallPrefab != null ? redBallPrefab : whiteBallPrefab;
        randomValue -= redChance;

        if (randomValue < pinkChance)
            return pinkBallPrefab != null ? pinkBallPrefab : whiteBallPrefab;
        randomValue -= pinkChance;

        if (randomValue < item3Chance)
            return item3Prefab != null ? item3Prefab : whiteBallPrefab;
        randomValue -= item3Chance;

        if (randomValue < item4Chance)
            return item4Prefab != null ? item4Prefab : whiteBallPrefab;
        randomValue -= item4Chance;

        if (randomValue < item5Chance)
            return item5Prefab != null ? item5Prefab : whiteBallPrefab;
        randomValue -= item5Chance;

        if (randomValue < item6Chance)
            return item6Prefab != null ? item6Prefab : whiteBallPrefab;
        randomValue -= item6Chance;

        return item7Prefab != null ? item7Prefab : whiteBallPrefab; // คืนค่า item 7
    }
}