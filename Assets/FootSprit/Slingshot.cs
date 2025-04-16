using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Line : MonoBehaviour
{
    // ------------- 变量定义区域 -----------------

    [Header("Base Settings")]
    [SerializeField] Transform pullPointTransform; // 拉伸控制点的Transform，用于控制拉伸的中间点
    [SerializeField] float maxDragDistance = 2f;       // 最大拖拽距离，超过此距离将被限制
    [SerializeField] Camera cam;                       // 用于将鼠标屏幕坐标转换为世界坐标的相机

    [Header("Anchor References")]
    [SerializeField] Transform leftAnchor;  // 左侧锚点的Transform，用于构成绳线起始点
    [SerializeField] Transform rightAnchor; // 右侧锚点的Transform，用于构成绳线结束点

    [Header("Shoot Settings")]
    [SerializeField] float maxShootForce = 10f;       // 发射力量的最大值
    [SerializeField] float shootForceMultiplier = 5f;   // 发射力量的乘数

    [Header("Spring Settings")]
    [SerializeField] float springDamping = 3f;          // 弹簧阻尼系数，控制弹簧回弹的衰减
    [SerializeField] float springFrequency = 10f;       // 弹簧频率，控制回弹时的振荡频率
    [SerializeField] float springEndThreshold = 0.01f;  // 当弹簧振荡幅度低于此阈值时停止回弹效果
    [SerializeField] float maxSpringDuration = 3f;      // 最大弹簧回弹持续时间

    [Header("吸附设置")]
    [SerializeField] float 吸附半径 = 0.5f;    // 点击拉伸点的有效半径
    [SerializeField] bool 显示吸附范围 = true;  // 是否在Scene中调试时显示吸附范围

    GameObject currentStone;   // 当前吸附在拉伸点的石头对象
    bool isDragging;           // 表示是否正在拖拽
    LineRenderer lineRenderer; // 绘制绳线的LineRenderer组件
    Vector2 startPoint, endPoint; // 绳线的起点和终点位置

    // 外部接口属性
    public Transform PullPointTransform => pullPointTransform;
    public Transform LeftAnchor => leftAnchor;
    public Transform RightAnchor => rightAnchor;

    private Coroutine springCoroutine; // 用于保存回弹协程的引用

    // ------------- Unity生命周期函数 -----------------

    /// <summary>
    /// 初始化：设置LineRenderer和默认拉伸点位置
    /// </summary>
    void Start()
    {
        InitializeLineRenderer();
        SetDefaultLinePosition();
    }

    /// <summary>
    /// 初始化LineRenderer组件，设置初始起始点和终点
    /// </summary>
    void InitializeLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        startPoint = leftAnchor.position;
        endPoint = rightAnchor.position;
        // 初始时只设定两个点
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }

    /// <summary>
    /// 将拉伸控制点位置设置为左右锚点的中点
    /// </summary>
    void SetDefaultLinePosition()
    {
        Vector2 mid = (leftAnchor.position + rightAnchor.position) / 2f;
        pullPointTransform.position = mid;
    }

    /// <summary>
    /// 每帧更新：处理鼠标输入（点击、拖拽、释放）
    /// </summary>
    void Update()
    {
        // 当鼠标左键按下时检测是否位于吸附范围内
        if (Input.GetMouseButtonDown(0))
        {
            // 获取鼠标在限制拖拽范围内的世界坐标
            Vector3 鼠标世界坐标 = GetClampedMousePosition();
            float 距离 = Vector3.Distance(鼠标世界坐标, pullPointTransform.position);

            // 只有点击在有效吸附范围内才允许开始拖拽
            if (距离 <= 吸附半径)
            {
                StartDragging();
            }
        }

        // 这里再次检测鼠标按下（此处和上面重复，可考虑合并处理以避免重复判断）
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 点击位置 = GetClampedMousePosition();
            if (Vector3.Distance(点击位置, pullPointTransform.position) <= 吸附半径)
            {
                StartDragging();
            }
        }

        // 如果正在拖拽则更新位置
        if (isDragging)
        {
            UpdateDragPosition();

            // 当鼠标左键释放时结束拖拽
            if (Input.GetMouseButtonUp(0))
            {
                EndDragging();
            }
        }
    }

    /// <summary>
    /// 在Scene视图中绘制吸附范围的Gizmo图形（调试用）
    /// </summary>
    void OnDrawGizmos()
    {
        if (显示吸附范围 && pullPointTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pullPointTransform.position, 吸附半径);
        }
    }

    // ------------- 拖拽及射击逻辑 -----------------

    /// <summary>
    /// 开始拖拽：停止回弹协程（如果正在运行），并更新线段顶点数
    /// </summary>
    void StartDragging()
    {
        // 如果回弹协程正在运行，则停止协程
        if (springCoroutine != null)
        {
            StopCoroutine(springCoroutine);
            springCoroutine = null;
        }

        isDragging = true;
        // 拖拽状态下使用三个顶点（左锚点、拉伸点、右锚点）
        lineRenderer.positionCount = 3;
    }

    /// <summary>
    /// 更新拖拽时的控制点位置，并更新石头和线段位置
    /// </summary>
    void UpdateDragPosition()
    {
        // 更新左右锚点的坐标（可能会动态变化）
        startPoint = leftAnchor.position;
        endPoint = rightAnchor.position;

        Vector3 mousePos = GetClampedMousePosition();
        // 设定拉伸控制点的新位置
        pullPointTransform.position = mousePos;

        // 如果有当前石头，则同步更新其位置
        if (currentStone != null)
            currentStone.transform.position = mousePos;

        // 更新LineRenderer显示的线段
        UpdateLineRenderer();
    }

    /// <summary>
    /// 将屏幕鼠标坐标转换为世界坐标，并限制拖拽距离不超过最大拖拽距离
    /// </summary>
    /// <returns>限制后的鼠标位置</returns>
    Vector3 GetClampedMousePosition()
    {
        // 将鼠标屏幕位置转换为世界坐标
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        // 计算左右锚点中点
        Vector2 mid = (leftAnchor.position + rightAnchor.position) / 2f;
        // 计算鼠标方向
        Vector2 dir = ((Vector2)mousePos - mid).normalized;
        // 计算鼠标距离中点的距离
        float dist = Vector2.Distance(mousePos, mid);

        // 如果超过最大拖拽距离，则限制在最大范围内，否则返回实际坐标
        return dist > maxDragDistance ? mid + dir * maxDragDistance : mousePos;
    }

    /// <summary>
    /// 更新LineRenderer中每个顶点的位置
    /// </summary>
    void UpdateLineRenderer()
    {
        // 如果顶点数大于等于3，则更新三个关键点的位置
        if (lineRenderer.positionCount >= 3)
        {
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, pullPointTransform.position);
            lineRenderer.SetPosition(2, endPoint);
        }
        else
        {
            // 安全措施：确保线段最少有两个顶点，重置为左右锚点
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
        }
    }

    /// <summary>
    /// 当拖拽结束时处理发射逻辑和启动回弹效果协程
    /// </summary>
    void EndDragging()
    {
        isDragging = false;

        // 发射石子（若有当前石子挂载）
        if (currentStone != null)
            ShootStone(pullPointTransform.position);

        // 启动回弹协程，使拉伸线弹回默认中点位置
        springCoroutine = StartCoroutine(SpringReturnLine(pullPointTransform.position));
    }

    // ------------- 石子吸附及发射逻辑 -----------------

    /// <summary>
    /// 吸附石子到拉伸线上，如果已有石子，则不重复吸附
    /// </summary>
    /// <param name="stone">待吸附的石子对象</param>
    public void AttachStoneToLine(GameObject stone)
    {
        if (currentStone != null) return;
        currentStone = stone;
        ConfigureStoneComponents();
    }

    /// <summary>
    /// 配置当前石子的物理属性：设置父物体、位置、刚体属性（速度置零、设为运动学）
    /// </summary>
    void ConfigureStoneComponents()
    {
        // 将石子挂载到拉伸控制点下，并定位到控制点位置
        currentStone.transform.SetParent(pullPointTransform);
        currentStone.transform.position = pullPointTransform.position;

        // 获取或添加Rigidbody2D组件
        var rb = currentStone.GetComponent<Rigidbody2D>() ?? currentStone.AddComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
    }

    /// <summary>
    /// 发射石子：计算发射方向与力量，并执行发射操作
    /// </summary>
    /// <param name="releasePos">石子释放时的拉伸点位置</param>
    void ShootStone(Vector2 releasePos)
    {
        if (currentStone == null) return;

        // 计算左右锚点中点
        Vector2 mid = (leftAnchor.position + rightAnchor.position) / 2f;
        // 发射方向从拉伸点指向中点
        Vector2 direction = (mid - releasePos).normalized;
        // 计算发射力量
        float force = CalculateShootForce(mid, releasePos);

        ExecuteShoot(force, direction);
        // 分离当前石子，使其不再跟随拉伸点
        currentStone.transform.SetParent(null);
        // 调用石子自身的发射后处理方法（如状态复位），若存在DraggableStone组件
        currentStone.GetComponent<DraggableStone>()?.OnFired();
        currentStone = null;
    }

    /// <summary>
    /// 根据拉伸距离计算发射力量，考虑力量乘数和最大力量的限制
    /// </summary>
    /// <param name="mid">左右锚点中点</param>
    /// <param name="releasePos">释放位置</param>
    /// <returns>计算得到的发射力量</returns>
    float CalculateShootForce(Vector2 mid, Vector2 releasePos)
    {
        float pullDistance = Vector2.Distance(releasePos, mid);
        return Mathf.Min(pullDistance * shootForceMultiplier, maxShootForce);
    }

    /// <summary>
    /// 执行发射操作，取消运动学属性，并添加相应的力
    /// </summary>
    /// <param name="force">发射力量</param>
    /// <param name="direction">发射方向</param>
    void ExecuteShoot(float force, Vector2 direction)
    {
        var rb = currentStone.GetComponent<Rigidbody2D>();
        rb.isKinematic = false;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }

    /// <summary>
    /// 重置当前石子状态（调用石子组件的复位方法，并分离石子）
    /// </summary>
    void ResetStoneState()
    {
        currentStone.GetComponent<DraggableStone>()?.ResetStone();
        currentStone.transform.SetParent(null);
        currentStone = null;
    }

    // ------------- 弹簧回弹逻辑 -----------------

    /// <summary>
    /// 回弹协程：模拟弹簧回弹效果，将拉伸点逐步恢复至默认中点位置
    /// </summary>
    /// <param name="initialMid">初始拉伸点位置（释放时的位置）</param>
    /// <returns>协程迭代器</returns>
    IEnumerator SpringReturnLine(Vector2 initialMid)
    {
        // 目标中点位置为左右锚点的中点
        Vector2 targetMid = (startPoint + endPoint) / 2f;
        // 计算初始偏移量（释放位置与目标中点的差值）
        Vector2 delta = initialMid - targetMid;
        float elapsedTime = 0f;

        // 设置LineRenderer的顶点数为3，以便显示中间回弹位置
        lineRenderer.positionCount = 3;

        // 在限定的最大持续时间内模拟弹簧回弹
        while (elapsedTime < maxSpringDuration)
        {
            elapsedTime += Time.deltaTime;
            // 指数衰减因子，控制振荡幅度
            float decay = Mathf.Exp(-springDamping * elapsedTime);
            UpdateSpringPosition(targetMid, delta, decay, elapsedTime);

            // 当衰减后的偏移小于阈值时结束回弹
            if (ShouldStopSpring(decay, delta)) break;
            yield return null;
        }

        // 重置LineRenderer显示（仅左右两点显示）
        ResetLineRenderer();
        // 将拉伸控制点位置恢复为默认中点
        pullPointTransform.position = (leftAnchor.position + rightAnchor.position) / 2f;
        springCoroutine = null;
    }

    /// <summary>
    /// 根据弹簧回弹公式更新中间顶点的位置
    /// </summary>
    /// <param name="target">目标中点位置</param>
    /// <param name="delta">初始偏移向量</param>
    /// <param name="decay">当前衰减因子</param>
    /// <param name="time">已经经过的时间</param>
    void UpdateSpringPosition(Vector2 target, Vector2 delta, float decay, float time)
    {
        // 衰减与cos函数结合，模拟弹簧振荡效果
        float factor = decay * Mathf.Cos(springFrequency * time);
        Vector2 currentMid = target + delta * factor;
        lineRenderer.SetPosition(1, currentMid);
    }

    /// <summary>
    /// 判断是否满足停止弹簧回弹的条件
    /// </summary>
    /// <param name="decay">当前衰减因子</param>
    /// <param name="delta">初始偏移向量</param>
    /// <returns>如果回弹幅度足够小则返回true</returns>
    bool ShouldStopSpring(float decay, Vector2 delta)
    {
        return decay * delta.magnitude < springEndThreshold;
    }

    /// <summary>
    /// 重置LineRenderer，将其仅设为左右两个点（恢复静止状态）
    /// </summary>
    void ResetLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, leftAnchor.position);
        lineRenderer.SetPosition(1, rightAnchor.position);
    }
}
