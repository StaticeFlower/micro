
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

        // ตรวจชื่อก่อน เพราะทุกลูกอาจใช้ Tag "Item"
        string ballName = other.gameObject.name.Replace("(Clone)", "").Trim();

        if (ballName == "Item 1")
        {
            comboChange = -2;
        }
        else if (ballName == "Item 2")
        {
            comboChange = -5;
        }
        else if (ballName == "Item")
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
