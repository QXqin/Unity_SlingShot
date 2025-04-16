using UnityEngine;
using System.Collections.Generic;

public class StoneBasket : MonoBehaviour
{
    public List<Transform> storedPositions; // 存放石头的位置点（手动拖入）
    private int currentIndex = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Stone"))
        {
            // 禁用石头的物理效果
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.isKinematic = true;
            }

            // 放入篮子中
            if (currentIndex < storedPositions.Count)
            {
                collision.transform.position = storedPositions[currentIndex].position;
                collision.transform.rotation = Quaternion.identity;
                collision.transform.SetParent(transform);
                currentIndex++;
            }
            else
            {
                Debug.Log("篮子已满！");
            }

            // 可选：播放音效或动画
        }
    }
}
