using UnityEngine;

// Ҫ��ýű����ڶ�����븽��Rigidbody2D��Collider2D���
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class DraggableStone : MonoBehaviour
{
    // ------------------ �ֶζ������� ------------------

    // �����������ĸ������
    private Rigidbody2D rb;

    // �����Ƿ�������ק
    private bool isDragging = false;

    // ����������ã�������Ļ����������ת��
    private Camera cam;

    // �ؿ���Ψһ�ĵ����ű����ã�����ʯ�������������߼�
    private Line lineScript;

    // ʯ�������λ�ü�¼���������ã�
    private Vector3 originalPosition;

    // ���ʯ���Ƿ��Ѿ�������������
    private bool isAttachedToSlingshot = false;

    [Header("��ק����")]
    // ָ����קʱʯ����Z���ϵ���ȣ�ȷ���������㼶��ʾ
    [SerializeField] float dragZDepth = 0f;


    // ------------------ Unity�������ں��� ------------------

    /// <summary>
    /// �ű���ʼ��������Ϸ��ʼʱ���ã���ʼ��������������͸��壬��¼��ʼλ��
    /// </summary>
    void Start()
    {
        // ��ȡ�����������������
        cam = Camera.main;
        // Ѱ�ҳ����е�Line�ű����������ƽű���
        lineScript = FindObjectOfType<Line>();
        // ��¼ʯ�ӵĳ�ʼλ�ã����ں�������
        originalPosition = transform.position;
        // ��ȡ�����Rigidbody2D���
        rb = GetComponent<Rigidbody2D>();
    }


    // ------------------ ��꽻������ ------------------

    /// <summary>
    /// ����������ö���ʱ��������ʼ��קǰ׼������
    /// </summary>
    void OnMouseDown()
    {
        // ���ʯ���Ѿ������������ϣ�����������ק
        if (isAttachedToSlingshot) return;

        // �������PlayerMovement�ű�������ñ߽����ƻ����������߼���ʹ��ק�ڼ䲻����Ӱ�죩
        var playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // �����ٶȼ����ٶȣ���������Զ�Ӱ����ק
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        // ����Ϊ�˶�ѧ���ر�����ģ�⣬ȷ��λ�ÿ����ɴ���ֱ�ӿ���
        rb.isKinematic = true;
        isDragging = true;
    }

    /// <summary>
    /// �������ק�ö���ʱ������ʵʱ���¶���λ�����������λ��
    /// </summary>
    void OnMouseDrag()
    {
        // ���������ק״̬�����������������ϣ�����Ӧ��ק��Ϊ
        if (!isDragging || isAttachedToSlingshot) return;

        // �����λ��ת��Ϊ�������꣬������ָ����Z���
        Vector3 mousePos = GetMouseWorldPos();
        mousePos.z = dragZDepth;
        // ����ʯ��λ��Ϊ���λ��
        transform.position = mousePos;
    }

    /// <summary>
    /// ������ͷź󴥷���������ק״̬���ָ����������Լ������ű�����
    /// </summary>
    void OnMouseUp()
    {
        // ����������������ϣ��������ͷŲ���
        if (isAttachedToSlingshot) return;

        // ��������PlayerMovement�����������ڣ����ָ�Ĭ����Ϊ
        var playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // �ָ�����ģ��
        rb.isKinematic = false;
        isDragging = false;
    }


    // ------------------ �������� ------------------

    /// <summary>
    /// �������Ļ����ת��Ϊ�������꣬��������ק�����Z�����
    /// </summary>
    /// <returns>��������������е�λ��</returns>
    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        // ����������������Ŀ����ק���֮��ľ���
        mousePos.z = -cam.transform.position.z + dragZDepth;
        return cam.ScreenToWorldPoint(mousePos);
    }


    // ------------------ �����봥���߼� ------------------

    /// <summary>
    /// ��ʯ�ӵ���ײ�����������ײ�壨��������ʱ������
    /// </summary>
    /// <param name="collision">��������ײ���Collider2D����</param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        // ���ʯ���Ѿ������������ϣ�����Ӧ��ײ�¼�
        if (isAttachedToSlingshot) return;

        // �����ײ���ǩ�Ƿ�Ϊ"PullArea"���������������ʶ
        if (collision.CompareTag("PullArea"))
        {
            // ������������
            AttachToSlingshot();
        }
    }

    /// <summary>
    /// ִ�������߼�����ʯ�Ӹ��ӵ������ϣ������ò���������Ϊ
    /// </summary>
    void AttachToSlingshot()
    {
        // ���õ����ű�����������������ǰʯ�Ӽ��뵯���߼���
        lineScript.AttachStoneToLine(gameObject);
        isAttachedToSlingshot = true;
        // ��֤���徲ֹ����������ģ��Ӱ�죩
        rb.isKinematic = true;

        // �����Ҫ���Խ�����ײ������ֹ�ظ��������˴����뱻ע�ͣ��ɸ�����������
        // GetComponent<Collider2D>().enabled = false;
    }


    // ------------------ ״̬���úͷ����Ĵ��� ------------------

    /// <summary>
    /// ����ʯ��״̬��������״̬���ݻָ�����ʼ״̬�������ٴ�ʹ��
    /// </summary>
    public void ResetStone()
    {
        // ֹͣ����Э�̣�������ܵĵ����ص����̣�
        StopAllCoroutines();
        // ������Ҫ���ɽ�ʯ��λ�����õ����λ�ã�����Ŀǰ��ע�ͣ��ɸ����������ã�
        // transform.position = originalPosition;
        isDragging = false;
        isAttachedToSlingshot = false;

        // �ָ���������Ϊ���˶�ѧ״̬������������ײ���
        rb.isKinematic = false;
        GetComponent<Collider2D>().enabled = true;
    }

    /// <summary>
    /// ��ʯ�ӱ������������ã���������״̬�ָ���������λ��
    /// </summary>
    public void OnFired()
    {
        // ���ʯ�Ӳ��ٴ��ڵ�������״̬
        isAttachedToSlingshot = false;
        rb.isKinematic = false;
        // �ָ���ײ��״̬��ȷ�������������������֮ǰ�����ã�
        GetComponent<Collider2D>().enabled = true;
        // ע�⣺�˴�����λ�ý������ã����ⲿ�߼�����ʯ�ӵĹ켣
    }
}
