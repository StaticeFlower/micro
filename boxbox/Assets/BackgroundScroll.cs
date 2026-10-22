
using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private Transform nextBackground;

    private float spriteWidth;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null || nextBackground == null)
        {
            Debug.LogError("Background ต้องมี SpriteRenderer และกำหนด Next Background");
            enabled = false;
            return;
        }

        // ความกว้างจริงของภาพหลังใช้ Scale แล้ว
        spriteWidth = spriteRenderer.bounds.size.x;
    }

    void Update()
    {
        // เลื่อนพื้นหลังไปทางซ้ายอย่างต่อเนื่อง
        float movement = speed * Time.deltaTime;
        transform.position += Vector3.left * movement;

        // เมื่อรูปนี้เลื่อนไปจนถึงตำแหน่งที่ต้องวนกลับ
        if (transform.position.x <= -spriteWidth)
        {
            // นำรูปมาต่อท้ายรูปถัดไปทันที
            transform.position = new Vector3(
                nextBackground.position.x + spriteWidth,
                transform.position.y,
                transform.position.z
            );
        }
    }
}
