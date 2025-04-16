using UnityEngine;
using System.Collections.Generic;

public class StoneBasket : MonoBehaviour
{
    public List<Transform> storedPositions; // ���ʯͷ��λ�õ㣨�ֶ����룩
    private int currentIndex = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Stone"))
        {
            // ����ʯͷ������Ч��
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.isKinematic = true;
            }

            // ����������
            if (currentIndex < storedPositions.Count)
            {
                collision.transform.position = storedPositions[currentIndex].position;
                collision.transform.rotation = Quaternion.identity;
                collision.transform.SetParent(transform);
                currentIndex++;
            }
            else
            {
                Debug.Log("����������");
            }

            // ��ѡ��������Ч�򶯻�
        }
    }
}
