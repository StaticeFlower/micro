using System;
using System.Collections.Concurrent;
using UnityEngine;
using WebSocketSharp;

[Serializable]
public class SensorMsg
{
    public string type;
    public string mac;
    public int L;
    public int R;
}

// Unity เป็น Client เชื่อมไปที่ WebSocket Server ของ ESP32
public class WsClient : MonoBehaviour
{
    [Header("ESP32 WebSocket Server")]
    public string esp32Ip = "172.20.10.2";   // ใส่ IP ของ ESP32 (ดูจาก Serial Monitor)
    public int port = 8080;                    // พอร์ตที่ ESP32 เปิดไว้
    public string path = "/";                  // path ของ WS server บน ESP32 เช่น "/" หรือ "/ws"

    [Header("Reconnect")]
    public bool autoReconnect = true;
    public float reconnectDelay = 3f;          // วินาที

    [Header("Movement")]
    public float speed = 5f;

    WebSocket ws;
    readonly ConcurrentQueue<string> inbox = new ConcurrentQueue<string>();
    int curL = 1, curR = 1;                    // 1 = ไม่กด, 0 = กด
    float lastAttempt = -999f;
    volatile bool connecting = false;

    string Url => $"ws://{esp32Ip}:{port}{path}";

    void Start()
    {
        Connect();
    }

    void Connect()
    {
        if (connecting) return;
        connecting = true;
        lastAttempt = Time.time;

        // สร้างใหม่ทุกครั้งที่เชื่อมต่อ เพื่อเลี่ยงปัญหา state ค้างจากการเชื่อมต่อครั้งก่อน
        DisposeSocket();

        ws = new WebSocket(Url);
        ws.WaitTime = TimeSpan.FromSeconds(5);

        ws.OnOpen += (s, e) =>
        {
            connecting = false;
            Debug.Log($"เชื่อมต่อ ESP32 สำเร็จ: {Url}");
        };

        // ทำงานบน thread อื่น → ใส่คิวไว้ให้ Update() (main thread) ประมวลผล
        ws.OnMessage += (s, e) =>
        {
            if (e.IsText) inbox.Enqueue(e.Data);
        };

        ws.OnError += (s, e) =>
        {
            connecting = false;
            Debug.LogWarning($"WS error: {e.Message}");
        };

        ws.OnClose += (s, e) =>
        {
            connecting = false;
            Debug.Log($"หลุดจาก ESP32 (code {e.Code})");
        };

        Debug.Log($"กำลังเชื่อมต่อ {Url} ...");
        ws.ConnectAsync();   // ไม่บล็อก main thread
    }

    void Update()
    {
        // เชื่อมต่อใหม่อัตโนมัติถ้าหลุด
        if (autoReconnect && !connecting &&
            (ws == null || !ws.IsAlive) &&
            Time.time - lastAttempt >= reconnectDelay)
        {
            Connect();
        }

        while (inbox.TryDequeue(out string msg))
        {
            SensorMsg data;
            try
            {
                data = JsonUtility.FromJson<SensorMsg>(msg);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"JSON ผิดรูปแบบ: {msg} ({ex.Message})");
                continue;
            }
            if (data == null) continue;

            if (data.type == "sensor")
            {
                curL = data.L;
                curR = data.R;
                Debug.Log($"L={curL}, R={curR}");
            }
            else if (data.type == "register")
            {
                Debug.Log($"ลงทะเบียน MAC {data.mac}");
            }
        }

        // ขยับทุกเฟรมตามค่าล่าสุด
        float dir = 0;
        if (curL == 0) dir -= 1;
        if (curR == 0) dir += 1;
        transform.Translate(dir * speed * Time.deltaTime, 0, 0);
    }

    // ส่งข้อความไปหา ESP32
    public void Send(string msg)
    {
        if (ws != null && ws.IsAlive) ws.Send(msg);
        else Debug.LogWarning("ยังไม่ได้เชื่อมต่อ ESP32");
    }

    void DisposeSocket()
    {
        if (ws == null) return;
        try
        {
            if (ws.ReadyState == WebSocketState.Open) ws.Close();
        }
        catch { }
        ws = null;
    }

    void OnDestroy()
    {
        autoReconnect = false;
        DisposeSocket();
    }
}

