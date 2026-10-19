
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float xLimit = 8f;

    void Update()
    {
        float moveInput = 0f;

        // รับปุ่ม A / D และลูกศรซ้าย / ขวา
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
            {
                moveInput = -1f;
            }
            else if (Keyboard.current.dKey.isPressed ||
                     Keyboard.current.rightArrowKey.isPressed)
            {
                moveInput = 1f;
            }
        }

        // ใช้ความเร็วตาม Level
        float currentSpeed = moveSpeed;

        if (LevelManager.Instance != null)
        {
            currentSpeed = LevelManager.Instance.CurrentSpeed;
        }

        // เคลื่อนที่ฐาน
        transform.Translate(
            Vector3.right * moveInput * currentSpeed * Time.deltaTime
        );

        // จำกัดพื้นที่การเคลื่อนที่
        float clampedX = Mathf.Clamp(
            transform.position.x,
            -xLimit,
            xLimit
        );

        transform.position = new Vector3(
            clampedX,
            transform.position.y,
            transform.position.z
        );
    }
}
