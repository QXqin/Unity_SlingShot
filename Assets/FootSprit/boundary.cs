using UnityEngine;

/// <summary>
/// PlayerMovement �ű����ڿ�����ң����������Ļ�ڵ��ƶ���Χ���ƣ�ȷ���ö��󲻻ᳬ����Ļ�߽硣
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    // �����������
    private Camera mainCam;
    // ��Ļ�߽磬�����������±�ʾ
    private Vector2 screenBounds;
    // �����ȵ�һ�루���ڼ���߽���ײ��
    private float objectWidth;
    // ����߶ȵ�һ�루���ڼ���߽���ײ��
    private float objectHeight;

    /// <summary>
    /// Start �ڽű���ʼ��ʱ���ã����ڻ�ȡ�������������Ļ�߽缰����ߴ硣
    /// </summary>
    void Start()
    {
        // ��ȡ�������������������
        mainCam = Camera.main;

        // ������Ļ���Ͻǵ��������꣬ת�����������꣬�����Ļ�߽�
        // ע�⣺�������Z��λ������ȷ��ת�������
        screenBounds = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCam.transform.position.z));

        // ��ȡ�����SpriteRenderer��������ڼ������İ��Ͱ��
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        objectWidth = sprite.bounds.extents.x;
        objectHeight = sprite.bounds.extents.y;
    }

    /// <summary>
    /// LateUpdate ÿ֡��Update֮����ã����ڵ�������λ�ã�ȷ����ʼ�մ�����Ļ�߽��ڡ�
    /// </summary>
    void LateUpdate()
    {
        // ��ȡ��ǰ����λ�õĿ���
        Vector3 clampedPosition = transform.position;

        // ����X��λ�ã�ȷ�����󲻳�����Ļ���ұ߽�
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x + objectWidth, screenBounds.x - objectWidth);
        // ����Y��λ�ã�ȷ�����󲻳�����Ļ���±߽�
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y + objectHeight, screenBounds.y - objectHeight);

        // ���¶���λ��Ϊ���ƺ������
        transform.position = clampedPosition;
    }
}
