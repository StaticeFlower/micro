using UnityEngine;
using UnityEngine.InputSystem; // 1. ดึงระบบ Input System ใหม่มาใช้งาน

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float xLimit = 8f;

    void Update()
    {
        float moveInput = 0f;

        // 2. เช็กการกดปุ่ม A / D หรือ ลูกศรซ้าย / ขวา
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                moveInput = -1f;
            }
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                moveInput = 1f;
            }
        }

        // เคลื่อนที่ตัวละคร
        transform.Translate(Vector3.right * moveInput * moveSpeed * Time.deltaTime);

        // จำกัดพื้นที่ให้อยู่ในขอบเขตหน้าจอ
        float clampedX = Mathf.Clamp(transform.position.x, -xLimit, xLimit);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Item"))
        {
            Debug.Log("เก็บของได้แล้ว!");
            Destroy(collision.gameObject);
        }
    }
}