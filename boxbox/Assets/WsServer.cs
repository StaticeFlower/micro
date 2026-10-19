using System.Collections.Concurrent;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

[System.Serializable]
public class SensorMsg
{
    public string type;
    public string mac;
    public int L;
    public int R;
}

// ตัวรับ-ส่งข้อความของแต่ละ client (ทำงานบน thread อื่น ไม่ใช่ main thread)
public class SensorBehavior : WebSocketBehavior
{
    public static ConcurrentQueue<string> Inbox = new ConcurrentQueue<string>();

    protected override void OnOpen()
    {
        Debug.Log("ESP32 เชื่อมต่อแล้ว");
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        Inbox.Enqueue(e.Data);
    }

    protected override void OnClose(CloseEventArgs e)
    {
        Debug.Log("ESP32 หลุด");
    }
}

public class WsServer : MonoBehaviour
{
    public int port = 8080;
    public float speed = 5f;          // ใหม่: ความเร็วเดิน ปรับได้ใน Inspector

    WebSocketServer wss;
    int curL = 1, curR = 1;           // ใหม่: เก็บค่าล่าสุด (1 = ไม่กด, 0 = กด)

    void Start()
    {
        wss = new WebSocketServer(port);
        wss.AddWebSocketService<SensorBehavior>("/");
        wss.Start();
        Debug.Log($"WS server เปิดที่พอร์ต {port}");
    }

    void Update()
    {
        while (SensorBehavior.Inbox.TryDequeue(out string msg))
        {
            var data = JsonUtility.FromJson<SensorMsg>(msg);
            if (data.type == "sensor")
            {
                curL = data.L;        // ใหม่: เก็บค่าไว้
                curR = data.R;
                Debug.Log($"L={curL}, R={curR}");
            }
            else if (data.type == "register")
                Debug.Log($"ลงทะเบียน MAC {data.mac}");
        }

        // ใหม่: ขยับทุกเฟรมตามค่าที่เก็บไว้
        float dir = 0;
        if (curL == 0) dir -= 1;
        if (curR == 0) dir += 1;
        transform.Translate(dir * speed * Time.deltaTime, 0, 0);
    }

    public void Broadcast(string msg)
    {
        wss?.WebSocketServices["/"].Sessions.Broadcast(msg);
    }

    void OnDestroy()
    {
        wss?.Stop();
    }
}