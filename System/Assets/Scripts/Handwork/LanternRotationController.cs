using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class LanternRotationController : MonoBehaviour
{
    // 右手设备
    private InputDevice rightHand;
    // 需要旋转的目标物体（可在Inspector面板中指定）
    public GameObject rotateTarget;
    // 旋转速度系数（可根据需求调整）
    public float rotationSpeed = 50f;
    // 绕Z轴的最大旋转角度限制（±90度）
    [Header("旋转角度限制")]
    public float maxZRotation = 90f;

    // 记录绕Z轴的累计旋转角度
    private float currentZRotation = 0f;

    void Start()
    {
        // 初始化时创建一个默认的旋转目标物体（如果未指定）
        if (rotateTarget == null)
        {
            rotateTarget = new GameObject("RotationTarget");
            // 给默认物体添加一个立方体Mesh，方便可视化
            rotateTarget.AddComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            rotateTarget.AddComponent<MeshRenderer>();
            // 设置默认位置在视野前方便于观察
            rotateTarget.transform.position = new Vector3(0, 1.5f, 2f);
        }

        // 获取右手设备
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);

        if (devices.Count > 0)
        {
            rightHand = devices[0];
        }

        // 初始化当前Z轴旋转角度为0
        currentZRotation = 0f;
    }

    void Update()
    {
        // 检查手柄是否有效，无效则重新获取
        if (!rightHand.isValid)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);

            if (devices.Count > 0)
            {
                rightHand = devices[0];
                Debug.Log("RightHand 重新连接");
            }
            return;
        }

        // 读取右手柄摇杆的2D轴数据（primary2DAxis）
        Vector2 joystickValue;
        if (rightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystickValue))
        {
            // 1. 绕Y轴的左右旋转（无角度限制）
            rotateTarget.transform.Rotate(
                Vector3.up * joystickValue.x * rotationSpeed * Time.deltaTime,
                Space.World
            );

            // 2. 绕Z轴的前后旋转（带90度角度限制）
            // 计算本次要旋转的角度增量
            float zRotationDelta = joystickValue.y * rotationSpeed * Time.deltaTime;
            // 计算旋转后的累计角度
            float newZRotation = currentZRotation + zRotationDelta;

            // 检查是否超出±90度范围，超出则修正
            if (newZRotation > maxZRotation)
            {
                zRotationDelta = maxZRotation - currentZRotation;
                currentZRotation = maxZRotation;
            }
            else if (newZRotation < -maxZRotation)
            {
                zRotationDelta = -maxZRotation - currentZRotation;
                currentZRotation = -maxZRotation;
            }
            else
            {
                currentZRotation = newZRotation;
            }

            // 执行带限制的旋转
            rotateTarget.transform.Rotate(
                Vector3.forward * zRotationDelta,
                Space.World
            );

            // 调试输出（可选）
            // Debug.Log($"当前Z轴旋转角度: {currentZRotation}, 摇杆Y值: {joystickValue.y}");
        }
    }
}