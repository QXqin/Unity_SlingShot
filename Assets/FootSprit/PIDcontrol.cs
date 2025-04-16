using UnityEngine;

/// <summary>
/// AngularPIDController ʹ�� PID �㷨���ڸ���Ľ��ٶȣ�ʹ��ƽ�����ٵ� 0��
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class AngularPIDController : MonoBehaviour
{
    [Header("PID ����")]
    [Tooltip("����ϵ����������Ӧ����")]
    public float kp = 1f;
    [Tooltip("����ϵ��������������̬���")]
    public float ki = 0f;
    [Tooltip("΢��ϵ��������������")]
    public float kd = 0f;

    [Header("��������")]
    [Tooltip("Ŀ����ٶȣ�һ����Ϊ 0��������ʯͷ���վ�ֹ��ת")]
    public float targetAngularVelocity = 0f;

    private Rigidbody2D rb;         // ��ǰ����� Rigidbody2D ���
    private float integral;         // ��������
    private float previousError;    // ��һ֡�����

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        integral = 0f;
        previousError = 0f;
    }

    /// <summary>
    /// FixedUpdate ����������㣬ÿ FixedUpdate ���ڵ���һ��
    /// </summary>
    void FixedUpdate()
    {
        // ��ǰ���ٶȣ���λΪ��/�루ע�⣺Rigidbody2D.angularVelocity Ϊ float����ʾ���ٶȣ���λ���������ö�����
        float currentAngularVelocity = rb.angularVelocity;

        // ����Ŀ���뵱ǰ֮������
        float error = targetAngularVelocity - currentAngularVelocity;

        // �������ۼӣ��������ڣ���֤���������ʣ�
        integral += error * Time.fixedDeltaTime;

        // �������仯�ʣ�΢���
        float derivative = (error - previousError) / Time.fixedDeltaTime;
        previousError = error;

        // PID ���ƹ�ʽ
        float pidOutput = kp * error + ki * integral + kd * derivative;

        // Ӧ�ÿ������أ����ڽ��ٶȣ����������ֱ�������ڸ��壬
        // ����������󣬿����ʵ����� kp��ki��kd �����Ի������ļ���Ч��
        rb.AddTorque(pidOutput, ForceMode2D.Force);
    }
}
