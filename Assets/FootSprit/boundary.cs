using UnityEngine;

/// <summary>
/// PlayerMovement 脚本用于控制玩家（或对象）在屏幕内的移动范围限制，确保该对象不会超出屏幕边界。
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    // 主摄像机引用
    private Camera mainCam;
    // 屏幕边界，在世界坐标下表示
    private Vector2 screenBounds;
    // 对象宽度的一半（用于计算边界碰撞）
    private float objectWidth;
    // 对象高度的一半（用于计算边界碰撞）
    private float objectHeight;

    /// <summary>
    /// Start 在脚本初始化时调用，用于获取摄像机、计算屏幕边界及对象尺寸。
    /// </summary>
    void Start()
    {
        // 获取场景中主摄像机的引用
        mainCam = Camera.main;

        // 根据屏幕右上角的像素坐标，转换成世界坐标，获得屏幕边界
        // 注意：摄像机的Z轴位置用于确定转换的深度
        screenBounds = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCam.transform.position.z));

        // 获取对象的SpriteRenderer组件，用于计算对象的半宽和半高
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        objectWidth = sprite.bounds.extents.x;
        objectHeight = sprite.bounds.extents.y;
    }

    /// <summary>
    /// LateUpdate 每帧在Update之后调用，用于调整对象位置，确保它始终处于屏幕边界内。
    /// </summary>
    void LateUpdate()
    {
        // 获取当前对象位置的拷贝
        Vector3 clampedPosition = transform.position;

        // 限制X轴位置，确保对象不超出屏幕左右边界
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x + objectWidth, screenBounds.x - objectWidth);
        // 限制Y轴位置，确保对象不超出屏幕上下边界
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y + objectHeight, screenBounds.y - objectHeight);

        // 更新对象位置为限制后的坐标
        transform.position = clampedPosition;
    }
}
