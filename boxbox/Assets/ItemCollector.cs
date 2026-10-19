using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    public string itemTag = "Item";
    public int count = 0;             // จำนวนไอเทมทั้งหมดที่เก็บได้
    public int combo = 0;             // จำนวนครั้งที่เก็บติดต่อกัน

    public float comboTimeout = 3f;   // ถ้าไม่เก็บภายในกี่วินาที คอมโบจะรีเซ็ต
    public float praiseDuration = 1.5f; // แสดงคำชมนานกี่วินาที

    float lastCollectTime;
    string praiseText = "";
    float praiseUntil = 0f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(itemTag)) return;

        // ถ้าเก็บช้าเกินเวลา คอมโบเริ่มนับใหม่
        if (combo > 0 && Time.time - lastCollectTime > comboTimeout)
            combo = 0;

        combo++;
        count++;
        lastCollectTime = Time.time;

        string praise = GetPraise(combo);
        if (praise != "")
        {
            praiseText = praise;
            praiseUntil = Time.time + praiseDuration;
        }

        Debug.Log($"เก็บไอเทม รวม {count} ชิ้น | คอมโบ x{combo} {praise}");
        Destroy(other.gameObject);
    }

    string GetPraise(int c)
    {
        if (c >= 5) return "Perfect!";   // 5 ครั้งขึ้นไป ดีที่สุด
        if (c == 4) return "Excellent!"; // 4 ครั้ง ดีมาก
        if (c == 3) return "Great!";     // 3 ครั้ง ดี
        if (c == 2) return "OK";         // 2 ครั้ง ok
        return "";                       // ครั้งเดียว ไม่มีคำชม
    }

    public void ResetCombo()
{
    if (combo > 0) Debug.Log("พลาด! คอมโบรีเซ็ต");
    combo = 0;
}

    void Update()
    {
        // คอมโบหมดเวลา ให้รีเซ็ต
        if (combo > 0 && Time.time - lastCollectTime > comboTimeout)
            combo = 0;
    }

    void OnGUI()
    {
        var small = new GUIStyle(GUI.skin.label) { fontSize = 28 };
        GUI.Label(new Rect(10, 10, 300, 40), $"Items: {count}", small);
        if (combo >= 2)
            GUI.Label(new Rect(10, 50, 300, 40), $"Combo x{combo}", small);

        // คำชมตรงกลางจอ
        if (Time.time < praiseUntil)
        {
            var big = new GUIStyle(GUI.skin.label)
            {
                fontSize = 64,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            GUI.Label(new Rect(0, Screen.height * 0.25f, Screen.width, 100), praiseText, big);
        }
    }
}