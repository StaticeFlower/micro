using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject itemPrefab; // ตัวแปรเก็บแม่แบบ Item
    public float spawnInterval = 1.5f; // เวลาสุ่มเกิดทุกๆ กี่วินาที
    public float xMin = -7f; // พิกัดสุ่มด้านซ้ายสุด
    public float xMax = 7f;  // พิกัดสุ่มด้านขวาสุด
    public float spawnY = 6f; // ความสูงด้านบนที่จะให้ของเกิด
    public float destroyYBoundary = -6f; // พิกัด Y ด้านล่างที่ถ้าหลุดลงไปให้ลบทิ้ง

    void Start()
    {
        // สั่งให้ทำงานฟังก์ชัน SpawnItem ซ้ำๆ ตามเวลาที่กำหนด
        InvokeRepeating(nameof(SpawnItem), 0.5f, spawnInterval);
    }

    void SpawnItem()
    {
        // สุ่มตำแหน่ง X
        float randomX = Random.Range(xMin, xMax);
        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);

        // เสก Item ออกมาที่ตำแหน่งสุ่ม
        GameObject newItem = Instantiate(itemPrefab, spawnPosition, Quaternion.identity);

        // ใส่สคริปต์จิ๋วแบบ Inline เพื่อเช็กและลบวัตถุเมื่อตกลงไปต่ำกว่าขอบล่าง (destroyYBoundary)
        AutoDestroyWhenOutOfBounds autoDestroy = newItem.AddComponent<AutoDestroyWhenOutOfBounds>();
        autoDestroy.bottomY = destroyYBoundary;
    }
}

// คลาสเสริมสำหรับคอยตรวจเช็กตำแหน่ง และลบตัวเองเมื่อหลุดขอบล่าง
public class AutoDestroyWhenOutOfBounds : MonoBehaviour
{
    public float bottomY = -6f;

    void Update()
    {
        if (transform.position.y < bottomY)
        {
            // ไอเทมหลุดขอบล่างโดยไม่โดน Player = พลาด รีเซ็ตคอมโบ
            var collector = FindFirstObjectByType<ItemCollector>();
            if (collector != null) collector.ResetCombo();

            Destroy(gameObject);
        }
    }
}