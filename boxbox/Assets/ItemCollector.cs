using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    public static ItemCollector Instance { get; private set; }

    public string itemTag = "Item";
    public int count = 0;
    public int combo = 0;

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

    // บอลขาวตกหลุดจอ: รีเซ็ต Combo แต่ไม่ลด Items
    public void ResetCombo()
    {
        combo = 0;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ResetLevel();
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