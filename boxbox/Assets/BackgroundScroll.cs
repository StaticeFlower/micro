
using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    public Transform otherBackground;
    public float speed = 2f;

    private float width;
    private Camera cam;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr == null || otherBackground == null)
        {
            Debug.LogError("กรุณาตรวจสอบ Sprite Renderer และ Other Background");
            enabled = false;
            return;
        }

        width = sr.bounds.size.x;
        cam = Camera.main;

        // วางภาพที่สองต่อจากภาพแรก
        otherBackground.position = new Vector3(
            transform.position.x + width,
            transform.position.y,
            transform.position.z
        );
    }

    void Update()
    {
        float movement = speed * Time.deltaTime;

        transform.position += Vector3.left * movement;
        otherBackground.position += Vector3.left * movement;

        if (cam == null) return;

        // ขอบซ้ายของพื้นที่กล้องในพิกัดโลก
        float cameraLeft = cam.ViewportToWorldPoint(
            new Vector3(0f, 0.5f, -cam.transform.position.z)
        ).x;

        // ย้ายภาพกลับเมื่อพ้นขอบซ้ายจริง ๆ เท่านั้น
        if (transform.position.x + width / 2f < cameraLeft)
        {
            transform.position = new Vector3(
                otherBackground.position.x + width,
                transform.position.y,
                transform.position.z
            );
        }
        else if (otherBackground.position.x + width / 2f < cameraLeft)
        {
            otherBackground.position = new Vector3(
                transform.position.x + width,
                otherBackground.position.y,
                otherBackground.position.z
            );
        }
    }
}
