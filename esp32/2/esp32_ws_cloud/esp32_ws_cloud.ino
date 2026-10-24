#include <WiFiManager.h>
#include <WebSocketsClient.h>

#define LW 16
#define RW 18

// ===== ตั้งค่า Relay server บนคลาวด์ =====
const char* WS_HOST = "your-relay.onrender.com";  // โดเมนของ relay (ไม่ต้องมี https://)
const uint16_t WS_PORT = 443;                     // wss = 443 ; ทดสอบในบ้านแบบ ws ใช้ 8080
const char* WS_PATH = "/device";
const bool USE_SSL = true;                        // true = wss://, false = ws:// (ทดสอบ local)

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
  Serial.print("🔑 MAC (เอาไปใส่ใน Unity): ");
  Serial.println(WiFi.macAddress());
}

void sendRegister() {
  String msg = "{\"type\":\"register\",\"mac\":\"" + WiFi.macAddress() + "\"}";
  webSocket.sendTXT(msg);
}

void sendSensor(int L, int R) {
  char buf[64];
  snprintf(buf, sizeof(buf), "{\"type\":\"sensor\",\"L\":%d,\"R\":%d}", L, R);
  webSocket.sendTXT(buf);
  Serial.println(buf);
}

void onWsEvent(WStype_t type, uint8_t* payload, size_t length) {
  switch (type) {
    case WStype_CONNECTED:
      Serial.println("✅ ต่อ relay แล้ว");
      sendRegister();                               // ลงทะเบียนทุกครั้งที่ต่อ (รวมตอนต่อใหม่)
      sendSensor(digitalRead(LW), digitalRead(RW)); // ส่งค่าล่าสุดทันที
      lastL = digitalRead(LW);
      lastR = digitalRead(RW);
      break;

    case WStype_DISCONNECTED:
      Serial.println("❌ หลุดจาก relay (จะลองต่อใหม่อัตโนมัติ)");
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

  if (USE_SSL) webSocket.beginSSL(WS_HOST, WS_PORT, WS_PATH);
  else         webSocket.begin(WS_HOST, WS_PORT, WS_PATH);

  webSocket.onEvent(onWsEvent);
  webSocket.setReconnectInterval(5000);
  webSocket.enableHeartbeat(15000, 3000, 2);   // กันการเชื่อมต่อที่ค้างเงียบ
}

void loop() {
  webSocket.loop();   // ต้องเรียกทุกรอบ ห้ามมี delay ยาวๆ

  int L = digitalRead(LW);
  int R = digitalRead(RW);

  if ((L != lastL || R != lastR) && webSocket.isConnected()) {
    lastL = L;
    lastR = R;
    sendSensor(L, R);
  }
}
