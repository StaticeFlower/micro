using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    public static ItemCollector Instance { get; private set; }

    public string itemTag = "Item";
    public int count = 0;
    public int combo = 0;

    [Header("Defeat Settings (ระบบแพ้)")]
    public int maxMisses = 5;            // จำนวนครั้งที่พลาดได้
    public int currentMisses = 0;        // จำนวนครั้งที่พลาดลูกบอลสีขาวไปแล้ว
    public bool isGameOver = false;       // สถานะจบเกม
    public GameObject defeatUI;          // (Optional) ลาก Defeat UI/Panel ใน Canvas มาใส่ตรงนี้

    [Header("Ball Tags")]
    public string whiteBallTag = "WhiteBall";
    public string pinkBallTag = "PinkBall";
    public string greenBallTag = "GreenBall";
    public string redBallTag = "RedBall";

    [Header("New Ball Tags")]
    public string Tram = "Tram"; // Item 3 (+3 Combo)
    public string ohm = "ohm";   // Item 4 (+5 Combo)
    public string dew = "dew";   // Item 5 (+10 Combo)
    public string F = "F";       // Item 6 (-10 Combo)
    public string Heart = "Heart"; // Item 7 (ลดค่า Missed 1)

    public float praiseDuration = 1.5f;

    private string praiseText = "";
    private float praiseUntil = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    // นับ Items ตอนบอลสีขาว Spawn
    public void AddWhiteBallItemCount()
    {
        count++;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isGameOver) return; // หากเกมจบแล้ว ไม่ต้องประมวลผลการเก็บไอเทมเพิ่ม

        int comboChange;

        // ดึงชื่อวัตถุ ตัด (Clone) และช่องว่างออก
        string ballName = other.gameObject.name.Replace("(Clone)", "").Trim();

        // ตรวจเช็กชื่อ (ปรับให้เปลี่ยนเป็นตัวพิมพ์เล็กทั้งหมดเพื่อป้องกันปัญหาพิมพ์เล็ก/ใหญ่)
        string lowerBallName = ballName.ToLower();

        if (lowerBallName == "item 1")
        {
            comboChange = -2;
        }
        else if (lowerBallName == "item 2")
        {
            comboChange = -5;
        }
        else if (lowerBallName == "item 3" || other.CompareTag(Tram))
        {
            comboChange = 3;
        }
        else if (lowerBallName == "item 4" || other.CompareTag(ohm))
        {
            comboChange = 5;
        }
        else if (lowerBallName == "item 5" || other.CompareTag(dew))
        {
            comboChange = 10;
        }
        else if (lowerBallName == "item 6" || other.CompareTag(F))
        {
            comboChange = -10;
        }
        // 🔹 ปรับตรงนี้: ตรวจสอบทั้งชื่อ "item 7", "item7" และ Tag "Heart"
        else if (lowerBallName == "item 7" ||(!string.IsNullOrEmpty(Heart) && other.CompareTag(Heart)))
        {
            // เมื่อเก็บ item 7 ให้ลดค่านับ Missed ลง 1
            ReduceMissedCount();
            comboChange = 0; // ไม่กระทบค่า Combo
        }
        else if (lowerBallName == "item")
        {
            comboChange = 1;
        }
        else if (other.CompareTag(pinkBallTag))
        {
            comboChange = -1;
        }
        else if (other.CompareTag(greenBallTag))
        {
            comboChange = -2;
        }
        else if (other.CompareTag(redBallTag))
        {
            comboChange = -5;
        }
        else if (other.CompareTag(whiteBallTag))
        {
            comboChange = 1;
        }
        else
        {
            return;
        }

        combo = Mathf.Max(0, combo + comboChange);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UpdateLevel(combo);
        }

        // ==========================================
        // 🔹 ส่งค่า combo ไปเช็คและเซฟลง AccountSaver
        // ==========================================
        SaveMaxCombo();

        string praise = GetPraise(combo);

        if (!string.IsNullOrEmpty(praise))
        {
            praiseText = praise;
            praiseUntil = Time.time + praiseDuration;
        }

        Destroy(other.gameObject);
    }

    private string GetPraise(int c)
    {
        if (c >= 5) return "Perfect!";
        if (c == 4) return "Excellent!";
        if (c == 3) return "Great!";
        if (c == 2) return "OK!";
        return "";
    }

    // บอลขาวตกหลุดจอ: รีเซ็ต Combo และนับจำนวนครั้งที่พลาด
    public void ResetCombo()
    {
        combo = 0;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ResetLevel();
        }

        // --- เพิ่มระบบนับเมื่อจับบอลขาวไม่ได้ ---
        if (!isGameOver)
        {
            currentMisses++;
            Debug.Log($"Missed White Ball: {currentMisses}/{maxMisses}");

            if (currentMisses >= maxMisses)
            {
                TriggerDefeat();
            }
        }
    }

    // 🔹 ฟังก์ชันสำหรับลดค่านับ Missed ลง 1
    public void ReduceMissedCount()
    {
        if (currentMisses > 0)
        {
            currentMisses--;
            Debug.Log($"Heart Collected! Reduced Missed Count: {currentMisses}/{maxMisses}");
        }
    }

    private void TriggerDefeat()
    {
        isGameOver = true;

        if (defeatUI != null)
        {
            defeatUI.SetActive(true);
        }

        Debug.Log("Defeat! Game Over.");
    }

    // ==========================================
    // 🔹 ฟังก์ชันเรียกสั่งบันทึก Max Combo
    // ==========================================
    public void SaveMaxCombo()
    {
        AccountSaver saver = FindFirstObjectByType<AccountSaver>(FindObjectsInactive.Exclude);
        if (saver != null)
        {
            saver.SaveScore(combo); // ส่งค่า combo ล่าสุดไปเช็คกับ AccountSaver
        }
    }

    private void OnGUI()
    {
        if (Instance != this) return;

        GUIStyle smallStyle = new GUIStyle();
        smallStyle.fontSize = 28;
        smallStyle.fontStyle = FontStyle.Bold;
        smallStyle.normal.textColor = Color.white;

        GUI.Label(new Rect(20, 20, 400, 40),
            $"Items: {count}", smallStyle);

        GUI.Label(new Rect(20, 65, 400, 40),
            $"Combo x{combo}", smallStyle);

        int level = LevelManager.Instance != null
            ? LevelManager.Instance.currentLevel
            : 1;

        GUI.Label(new Rect(20, 110, 400, 40),
            $"Level: {level}", smallStyle);

        // แสดงจำนวนครั้งที่พลาด
        GUI.Label(new Rect(20, 155, 400, 40),
            $"Missed: {currentMisses}/{maxMisses}", smallStyle);

        // หากแพ้ ให้แสดงคำว่า Defeat กลางจอ
        if (isGameOver)
        {
            GUIStyle defeatStyle = new GUIStyle();
            defeatStyle.fontSize = 72;
            defeatStyle.alignment = TextAnchor.MiddleCenter;
            defeatStyle.fontStyle = FontStyle.Bold;
            defeatStyle.normal.textColor = Color.red;

            GUI.Label(
                new Rect(0, Screen.height * 0.4f, Screen.width, 100),
                "Defeat",
                defeatStyle
            );
            return;
        }

        if (Time.time < praiseUntil)
        {
            GUIStyle bigStyle = new GUIStyle();
            bigStyle.fontSize = 64;
            bigStyle.alignment = TextAnchor.MiddleCenter;
            bigStyle.fontStyle = FontStyle.Bold;
            bigStyle.normal.textColor = Color.white;

            GUI.Label(
                new Rect(0, Screen.height * 0.25f,
                    Screen.width, 100),
                praiseText,
                bigStyle
            );
        }
    }
}