using UnityEngine;

// 要求该脚本所在对象必须附加Rigidbody2D和Collider2D组件
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class DraggableStone : MonoBehaviour
{
    // ------------------ 字段定义区域 ------------------

    // 用于物理计算的刚体组件
    private Rigidbody2D rb;

    // 控制是否正在拖拽
    private bool isDragging = false;

    // 主摄像机引用，用于屏幕与世界坐标转换
    private Camera cam;

    // 关卡中唯一的弹弓脚本引用，用于石子吸附到弹弓逻辑
    private Line lineScript;

    // 石子最初的位置记录（用于重置）
    private Vector3 originalPosition;

    // 标记石子是否已经吸附到弹弓上
    private bool isAttachedToSlingshot = false;

    [Header("拖拽设置")]
    // 指定拖拽时石子在Z轴上的深度，确保在期望层级显示
    [SerializeField] float dragZDepth = 0f;


    // ------------------ Unity生命周期函数 ------------------

    /// <summary>
    /// 脚本初始化，在游戏开始时调用，初始化摄像机、弹弓和刚体，记录初始位置
    /// </summary>
    void Start()
    {
        // 获取场景中主摄像机引用
        cam = Camera.main;
        // 寻找场景中的Line脚本（弹弓控制脚本）
        lineScript = FindObjectOfType<Line>();
        // 记录石子的初始位置，便于后续重置
        originalPosition = transform.position;
        // 获取自身的Rigidbody2D组件
        rb = GetComponent<Rigidbody2D>();
    }


    // ------------------ 鼠标交互处理 ------------------

    /// <summary>
    /// 当鼠标点击到该对象时触发，开始拖拽前准备工作
    /// </summary>
    void OnMouseDown()
    {
        // 如果石子已经吸附到弹弓上，则不再允许拖拽
        if (isAttachedToSlingshot) return;

        // 如果存在PlayerMovement脚本，则禁用边界限制或其他控制逻辑（使拖拽期间不受其影响）
        var playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // 重置速度及角速度，避免因惯性而影响拖拽
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        // 设置为运动学，关闭物理模拟，确保位置可以由代码直接控制
        rb.isKinematic = true;
        isDragging = true;
    }

    /// <summary>
    /// 当鼠标拖拽该对象时触发，实时更新对象位置至鼠标所在位置
    /// </summary>
    void OnMouseDrag()
    {
        // 如果不在拖拽状态或者已吸附到弹弓上，则不响应拖拽行为
        if (!isDragging || isAttachedToSlingshot) return;

        // 将鼠标位置转换为世界坐标，并设置指定的Z深度
        Vector3 mousePos = GetMouseWorldPos();
        mousePos.z = dragZDepth;
        // 更新石子位置为鼠标位置
        transform.position = mousePos;
    }

    /// <summary>
    /// 当鼠标释放后触发，结束拖拽状态，恢复物理属性以及其他脚本控制
    /// </summary>
    void OnMouseUp()
    {
        // 如果已吸附到弹弓上，不处理释放操作
        if (isAttachedToSlingshot) return;

        // 重新启用PlayerMovement组件（如果存在），恢复默认行为
        var playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // 恢复物理模拟
        rb.isKinematic = false;
        isDragging = false;
    }


    // ------------------ 辅助函数 ------------------

    /// <summary>
    /// 将鼠标屏幕坐标转换为世界坐标，并考虑拖拽所需的Z轴深度
    /// </summary>
    /// <returns>鼠标在世界坐标中的位置</returns>
    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        // 计算鼠标在摄像机与目标拖拽深度之间的距离
        mousePos.z = -cam.transform.position.z + dragZDepth;
        return cam.ScreenToWorldPoint(mousePos);
    }


    // ------------------ 吸附与触发逻辑 ------------------

    /// <summary>
    /// 当石子的碰撞体进入其他碰撞体（触发器）时被调用
    /// </summary>
    /// <param name="collision">触发器碰撞体的Collider2D对象</param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 如果石子已经吸附到弹弓上，则不响应碰撞事件
        if (isAttachedToSlingshot) return;

        // 检查碰撞体标签是否为"PullArea"，这是吸附区域标识
        if (collision.CompareTag("PullArea"))
        {
            // 吸附到弹弓上
            AttachToSlingshot();
        }
    }

    /// <summary>
    /// 执行吸附逻辑，将石子附加到弹弓上，并禁用部分物理行为
    /// </summary>
    void AttachToSlingshot()
    {
        // 调用弹弓脚本的吸附函数，将当前石子加入弹弓逻辑中
        lineScript.AttachStoneToLine(gameObject);
        isAttachedToSlingshot = true;
        // 保证物体静止（不受物理模拟影响）
        rb.isKinematic = true;

        // 如果需要可以禁用碰撞器，防止重复吸附（此处代码被注释，可根据需求开启）
        // GetComponent<Collider2D>().enabled = false;
    }


    // ------------------ 状态重置和发射后的处理 ------------------

    /// <summary>
    /// 重置石子状态，将所有状态数据恢复到初始状态，方便再次使用
    /// </summary>
    public void ResetStone()
    {
        // 停止所有协程（例如可能的弹弓回弹过程）
        StopAllCoroutines();
        // 若有需要，可将石子位置重置到最初位置（本行目前被注释，可根据需求启用）
        // transform.position = originalPosition;
        isDragging = false;
        isAttachedToSlingshot = false;

        // 恢复物理属性为非运动学状态，重新启用碰撞检测
        rb.isKinematic = false;
        GetComponent<Collider2D>().enabled = true;
    }

    /// <summary>
    /// 当石子被弹弓发射后调用，处理发射后的状态恢复，不重置位置
    /// </summary>
    public void OnFired()
    {
        // 标记石子不再处于弹弓吸附状态
        isAttachedToSlingshot = false;
        rb.isKinematic = false;
        // 恢复碰撞器状态，确保后续交互正常（如果之前被禁用）
        GetComponent<Collider2D>().enabled = true;
        // 注意：此处不对位置进行重置，由外部逻辑控制石子的轨迹
    }
}
