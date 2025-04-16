using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class AppleController : MonoBehaviour
{
    [Header("物理设置")]
    [SerializeField] float gravityScale = 1f;
    [SerializeField] float collisionActivateDelay = 0.1f;
    [Header("碰撞效果")]
    [SerializeField] ParticleSystem hitEffect;
    [SerializeField] AudioClip hitSound;

    private Rigidbody2D rb;
    private bool isActivated;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        InitializeApple();
    }

    void InitializeApple()
    {
        // 初始状态设置
        rb.isKinematic = true;
        rb.bodyType = RigidbodyType2D.Kinematic; // 保持静止
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
        isActivated = false;
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"碰撞检测到对象：{collision.gameObject.name}，标签：{collision.gameObject.tag}");

        if (isActivated) return;

        // 检查碰撞对象是否是石子
        if (collision.gameObject.CompareTag("Stone") ||
            collision.gameObject.GetComponent<DraggableStone>() != null)
        {
            ActivatePhysics();
        }
    }

    void ActivatePhysics()
    {
        // 播放效果
        if (hitEffect != null) hitEffect.Play();
        if (hitSound != null) AudioSource.PlayClipAtPoint(hitSound, transform.position);

        isActivated = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = gravityScale;

        // 延迟启用碰撞确保物理效果稳定
        StartCoroutine(DelayedCollisionEnable());
    }

    IEnumerator DelayedCollisionEnable()
    {
        yield return new WaitForSeconds(collisionActivateDelay);
        rb.velocity = Vector2.zero;
    }

    // 重置苹果状态（可选）
    public void ResetApple()
    {
        InitializeApple();
        transform.rotation = Quaternion.identity;
    }
}