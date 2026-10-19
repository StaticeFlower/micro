#include <WiFiManager.h>
#include <Preferences.h>
#include <WebSocketsClient.h>

#define LW 16
#define RW 18

// ===== ตั้งค่าเซิร์ฟเวอร์ WebSocket =====
const char* WS_HOST = "10.220.65.156";   // เปลี่ยนเป็น IP/โดเมนของคุณ
const uint16_t WS_PORT = 8080;
const char* WS_PATH = "/";

Preferences preferences;
WebSocketsClient webSocket;

int lastL = -1, lastR = -1;

void connectWiFi(WiFiManager &wm) {
  wm.setConfigPortalTimeout(180);
  wm.setConnectTimeout(20);

  bool connected = wm.autoConnect("ESP32-Config-AP", "12345678");

  while (!connected) {
    Serial.println("❌ เชื่อมต่อไม่ได้ เปิด AP ให้เปลี่ยน WiFi...");
    connected = wm.startConfigPortal("ESP32-Config-AP", "12345678");
  }

  Serial.println("\n✅ เชื่อมต่อ Wi-Fi สำเร็จ!");
}

void sendRegister() {
  String msg = "{\"type\":\"register\",\"mac\":\"" + WiFi.macAddress() + "\"}";
  webSocket.sendTXT(msg);
}

void onWsEvent(WStype_t type, uint8_t* payload, size_t length) {
  switch (type) {
    case WStype_CONNECTED:
      Serial.println("✅ WS เชื่อมต่อแล้ว");
      sendRegister();          // ลงทะเบียนทุกครั้งที่ต่อ (รวมตอนต่อใหม่)
      lastL = lastR = -1;      // บังคับส่งค่าล่าสุดอีกรอบ
      break;

    case WStype_DISCONNECTED:
      Serial.println("❌ WS หลุด (จะลองต่อใหม่อัตโนมัติ)");
      break;

    case WStype_TEXT:
      Serial.printf("📩 รับ: %s\n", (char*)payload);
      break;

    default:
      break;
  }
}

void setup() {
  Serial.begin(9600);
  pinMode(LW, INPUT_PULLUP);
  pinMode(RW, INPUT_PULLUP);

  WiFi.mode(WIFI_STA);
  WiFiManager wm;
  connectWiFi(wm);

  webSocket.begin(WS_HOST, WS_PORT, WS_PATH);   // ถ้าเป็น wss ใช้ webSocket.beginSSL(WS_HOST, 443, WS_PATH);
  webSocket.onEvent(onWsEvent);
  webSocket.setReconnectInterval(5000);
  webSocket.enableHeartbeat(15000, 3000, 2);    // ping ทุก 15 วิ ตรวจจับสายหลุด
}

void loop() {
  webSocket.loop();   // ต้องเรียกทุกรอบ ห้ามมี delay ยาวๆ

  int L = digitalRead(LW);
  int R = digitalRead(RW);

  if ((L != lastL || R != lastR) && webSocket.isConnected()) {
    lastL = L;
    lastR = R;

    char buf[64];
    snprintf(buf, sizeof(buf), "{\"type\":\"sensor\",\"L\":%d,\"R\":%d}", L, R);
    webSocket.sendTXT(buf);
    Serial.println(buf);
  }
}