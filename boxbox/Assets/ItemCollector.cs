
using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    public string itemTag = "Item";
    public int count = 0;
    public int combo = 0;

    [Header("Ball Tags")]
    public string whiteBallTag = "WhiteBall";
    public string pinkBallTag = "PinkBall";
    public string greenBallTag = "GreenBall";
    public string redBallTag = "RedBall";

    public float praiseDuration = 1.5f;

    string praiseText = "";
    float praiseUntil = 0f;

    void OnTriggerEnter2D(Collider2D other)
    {
        int comboChange;

        if (other.CompareTag(whiteBallTag))
        {
            comboChange = 1;
        }
        else if (other.CompareTag(pinkBallTag))
        {
            comboChange = -1;
        }
        else if (other.CompareTag(greenBallTag))
        {
            comboChange = -3;
        }
        else if (other.CompareTag(redBallTag))
        {
            comboChange = -5;
        }
        else if (other.CompareTag(itemTag))
        {
            // รองรับบอลเดิมที่ใช้ Tag = Item
            comboChange = 1;
        }
        else
        {
            return;
        }

        // Combo ต่ำสุดคือ 0
        combo = Mathf.Max(0, combo + comboChange);
        count++;

        if (LevelManager.Instance != null)
            LevelManager.Instance.UpdateLevel(combo);

        string praise = GetPraise(combo);

        if (praise != "")
        {
            praiseText = praise;
            praiseUntil = Time.time + praiseDuration;
        }

        Debug.Log(
            $"เก็บบอล {count} ลูก | " +
            $"เปลี่ยน Combo {comboChange:+#;-#;0} | " +
            $"Combo ปัจจุบัน = {combo}"
        );

        Destroy(other.gameObject);
    }

    string GetPraise(int c)
    {
        if (c >= 5) return "Perfect!";
        if (c == 4) return "Excellent!";
        if (c == 3) return "Great!";
        if (c == 2) return "OK!";
        return "";
    }

    public void ResetCombo()
    {
        combo = 0;

        if (LevelManager.Instance != null)
            LevelManager.Instance.ResetLevel();

        Debug.Log("พลาด! Combo กลับเป็น 0 และ Level กลับเป็น 1");
    }

    void OnGUI()
    {
        var small = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28
        };

        GUI.Label(
            new Rect(10, 10, 350, 40),
            $"Items: {count}",
            small
        );

        GUI.Label(
            new Rect(10, 50, 350, 40),
            $"Combo x{combo}",
            small
        );

        int level = LevelManager.Instance != null
            ? LevelManager.Instance.currentLevel
            : 1;

        GUI.Label(
            new Rect(10, 90, 350, 40),
            $"Level: {level}",
            small
        );

        if (Time.time < praiseUntil)
        {
            var big = new GUIStyle(GUI.skin.label)
            {
                fontSize = 64,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            GUI.Label(
                new Rect(0, Screen.height * 0.25f,
                    Screen.width, 100),
                praiseText,
                big
            );
        }
    }
}
