using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class AppleController : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] float gravityScale = 1f;
    [SerializeField] float collisionActivateDelay = 0.1f;
    [Header("��ײЧ��")]
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
        // ��ʼ״̬����
        rb.isKinematic = true;
        rb.bodyType = RigidbodyType2D.Kinematic; // ���־�ֹ
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
        isActivated = false;
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"��ײ��⵽����{collision.gameObject.name}����ǩ��{collision.gameObject.tag}");

        if (isActivated) return;

        // �����ײ�����Ƿ���ʯ��
        if (collision.gameObject.CompareTag("Stone") ||
            collision.gameObject.GetComponent<DraggableStone>() != null)
        {
            ActivatePhysics();
        }
    }

    void ActivatePhysics()
    {
        // ����Ч��
        if (hitEffect != null) hitEffect.Play();
        if (hitSound != null) AudioSource.PlayClipAtPoint(hitSound, transform.position);

        isActivated = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = gravityScale;

        // �ӳ�������ײȷ������Ч���ȶ�
        StartCoroutine(DelayedCollisionEnable());
    }

    IEnumerator DelayedCollisionEnable()
    {
        yield return new WaitForSeconds(collisionActivateDelay);
        rb.velocity = Vector2.zero;
    }

    // ����ƻ��״̬����ѡ��
    public void ResetApple()
    {
        InitializeApple();
        transform.rotation = Quaternion.identity;
    }
}