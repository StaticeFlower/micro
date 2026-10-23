using UnityEngine;
using System.Collections.Generic;

public class ItemCollector : MonoBehaviour
{
    public static ItemCollector Instance { get; private set; }

    public string itemTag = "Item";
    public int count = 0;
    public int combo = 0;

    [Header("Defeat Settings (ระบบแพ้)")]
    public int maxMisses = 5;
    public int currentMisses = 0;
    public bool isGameOver = false;
    public GameObject defeatUI;

    [Header("Ball Tags")]
    public string whiteBallTag = "WhiteBall";
    public string pinkBallTag = "PinkBall";
    public string greenBallTag = "GreenBall";
    public string redBallTag = "RedBall";

    [Header("New Ball Tags")]
    public string Tram = "Tram";
    public string ohm = "ohm";
    public string dew = "dew";
    public string F = "F";
    public string Heart = "Heart";

    public float praiseDuration = 1.5f;

    private string praiseText = "";
    private float praiseUntil = 0f;

    private HashSet<GameObject> collectedItems = new HashSet<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void AddWhiteBallItemCount()
    {
        count++;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isGameOver) return;

        if (collectedItems.Contains(other.gameObject)) return;
        collectedItems.Add(other.gameObject);

        int comboChange;

        string ballName = other.gameObject.name.Replace("(Clone)", "").Trim();
        string lowerBallName = ballName.ToLower();

        if (lowerBallName == "item 1")
        {
            comboChange = -2;
        }
        else if (lowerBallName == "item 2")
        {
            comboChange = -5;
        }
        else if (lowerBallName == "item 3" || (!string.IsNullOrEmpty(Tram) && other.CompareTag(Tram)))
        {
            comboChange = 3;
        }
        else if (lowerBallName == "item 4" || (!string.IsNullOrEmpty(ohm) && other.CompareTag(ohm)))
        {
            comboChange = 5;
        }
        else if (lowerBallName == "item 5" || (!string.IsNullOrEmpty(dew) && other.CompareTag(dew)))
        {
            comboChange = 10;
        }
        else if (lowerBallName == "item 6" || (!string.IsNullOrEmpty(F) && other.CompareTag(F)))
        {
            comboChange = -10;
        }
        else if (lowerBallName == "item 7" || (!string.IsNullOrEmpty(Heart) && other.CompareTag(Heart)))
        {
            ReduceMissedCount();
            comboChange = 0;
        }
        else if (lowerBallName == "item" || other.CompareTag(whiteBallTag) || other.CompareTag(itemTag) || lowerBallName.Contains("ball-removebg"))
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
        else
        {
            collectedItems.Remove(other.gameObject);
            return;
        }

        combo = Mathf.Max(0, combo + comboChange);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UpdateLevel(combo);
        }

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

    public void ResetCombo()
    {
        combo = 0;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ResetLevel();
        }

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

    public void SaveMaxCombo()
    {
        AccountSaver saver = FindAnyObjectByType<AccountSaver>(FindObjectsInactive.Exclude);
        if (saver != null)
        {
            saver.SaveScore(combo);
        }
    }

    private void OnGUI()
    {
        if (Instance != this) return;

        GUIStyle smallStyle = new GUIStyle();
        smallStyle.fontSize = 28;
        smallStyle.fontStyle = FontStyle.Bold;
        smallStyle.normal.textColor = Color.white;

        GUI.Label(new Rect(20, 20, 400, 40), $"Items: {count}", smallStyle);
        GUI.Label(new Rect(20, 65, 400, 40), $"Combo x{combo}", smallStyle);

        int level = LevelManager.Instance != null ? LevelManager.Instance.currentLevel : 1;
        GUI.Label(new Rect(20, 110, 400, 40), $"Level: {level}", smallStyle);
        GUI.Label(new Rect(20, 155, 400, 40), $"Missed: {currentMisses}/{maxMisses}", smallStyle);

        if (isGameOver)
        {
            GUIStyle defeatStyle = new GUIStyle();
            defeatStyle.fontSize = 72;
            defeatStyle.alignment = TextAnchor.MiddleCenter;
            defeatStyle.fontStyle = FontStyle.Bold;
            defeatStyle.normal.textColor = Color.red;

            GUI.Label(new Rect(0, Screen.height * 0.4f, Screen.width, 100), "Defeat", defeatStyle);
            return;
        }

        if (Time.time < praiseUntil)
        {
            GUIStyle bigStyle = new GUIStyle();
            bigStyle.fontSize = 64;
            bigStyle.alignment = TextAnchor.MiddleCenter;
            bigStyle.fontStyle = FontStyle.Bold;
            bigStyle.normal.textColor = Color.white;

            GUI.Label(new Rect(0, Screen.height * 0.25f, Screen.width, 100), praiseText, bigStyle);
        }
    }
}