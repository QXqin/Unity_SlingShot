using UnityEngine;

/// <summary>
/// AngularPIDController 使用 PID 算法调节刚体的角速度，使其平滑减速到 0。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class AngularPIDController : MonoBehaviour
{
    [Header("PID 参数")]
    [Tooltip("比例系数，控制响应力度")]
    public float kp = 1f;
    [Tooltip("积分系数，用于消除稳态误差")]
    public float ki = 0f;
    [Tooltip("微分系数，用于抑制震荡")]
    public float kd = 0f;

    [Header("控制设置")]
    [Tooltip("目标角速度，一般设为 0，即期望石头最终静止旋转")]
    public float targetAngularVelocity = 0f;

    private Rigidbody2D rb;         // 当前对象的 Rigidbody2D 组件
    private float integral;         // 误差积分项
    private float previousError;    // 上一帧的误差

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        integral = 0f;
        previousError = 0f;
    }

    /// <summary>
    /// FixedUpdate 用于物理计算，每 FixedUpdate 周期调用一次
    /// </summary>
    void FixedUpdate()
    {
        // 当前角速度，单位为度/秒（注意：Rigidbody2D.angularVelocity 为 float，表示角速度，单位依场景设置而定）
        float currentAngularVelocity = rb.angularVelocity;

        // 计算目标与当前之间的误差
        float error = targetAngularVelocity - currentAngularVelocity;

        // 积分项累加（乘以周期，保证积分量合适）
        integral += error * Time.fixedDeltaTime;

        // 计算误差变化率（微分项）
        float derivative = (error - previousError) / Time.fixedDeltaTime;
        previousError = error;

        // PID 控制公式
        float pidOutput = kp * error + ki * integral + kd * derivative;

        // 应用控制力矩，调节角速度：输出的力矩直接作用于刚体，
        // 根据你的需求，可以适当调整 kp、ki、kd 参数以获得理想的减速效果
        rb.AddTorque(pidOutput, ForceMode2D.Force);
    }
}
