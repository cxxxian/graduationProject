using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class ShoppingCart : MonoBehaviour
{
    [Header("购物车 UI 部分")]
    public ShoppingTaskController shoppingTaskController;
    
    // 计算器UI
    public GameObject calculatorPricePanel;
    public TMP_Text calculatorPriceText;

    // 结算面板UI
    public GameObject ResultPanel;
    public TMP_Text ResultText;

    public GameObject UnFinishedPanel;

    public GameObject TaskPanel;

    // calculatorPricePanel是否显示
    private bool isShow = false;

    // 右手设备
    private InputDevice rightHand;
    // 左手设备
    private InputDevice leftHand;

    // 用来做按下瞬间检测
    private bool lastAPressed = false;

    [Header("购物车 Push 部分")]
    public bool leftHandOn = false;
    public bool rightHandOn = false;

    // 玩家（XR Origin）的 Transform
    public Transform playerRig;
    // 购物车的 Transform
    public Transform cart;
    // 当前手是否在是推车状态
    bool isPushing = false;

    // 记录购物车相对玩家的局部坐标偏移
    private Vector3 localOffset;
    // 玩家与购物车之间的水平旋转差（角度）
    private float yawOffset;

    public float contactGraceTime = 0.2f;
    public float pushSmoothTime = 0.04f;
    public float pushMaxSpeed = 8f;
    public float pushRotateSpeed = 18f;
    public float itemFollowPositionStrength = 18f;
    public float itemFollowRotationStrength = 12f;
    public float itemMaxFollowSpeed = 4.5f;

    private Vector3 cartVelocity;
    private bool isFinalPushing = false;
    private float leftHandLastContactTime = -999f;
    private float rightHandLastContactTime = -999f;

    public BasketTrigger basketTrigger;

    [Header("关卡完成音效")]
    public AudioSource levelAudioSource;

    void Start()
    {
        if (basketTrigger == null)
            basketTrigger = GetComponent<BasketTrigger>();

        calculatorPricePanel.SetActive(false);
        ResultPanel.SetActive(false);

        // 获取右手设备
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);

        if (devices.Count > 0)
        {
            rightHand = devices[0];
        }

        // 获取左手设备
        devices.Clear();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);

        if (devices.Count > 0)
        {
            leftHand = devices[0];
        }
    }

    void Update()
    {
        // 如果设备无效，则尝试重新获取
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
        if (!leftHand.isValid)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);

            if (devices.Count > 0)
            {
                leftHand = devices[0];
                Debug.Log("LeftHand 重新连接");
            }
            return;
        }

        bool rightTrigger = false;
        bool leftTrigger = false;

        rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out rightTrigger);
        leftHand.TryGetFeatureValue(CommonUsages.triggerButton, out leftTrigger);

        bool leftHandContactValid = leftHandOn || (Time.time - leftHandLastContactTime <= contactGraceTime);
        bool rightHandContactValid = rightHandOn || (Time.time - rightHandLastContactTime <= contactGraceTime);

        if (isPushing && !(leftHandContactValid && rightHandContactValid))
        {
            EndPushMode();
        }

        // 双手必须都按下
        isFinalPushing = (rightTrigger && leftTrigger && isPushing && leftHandContactValid && rightHandContactValid);

        // 推车逻辑
        if (isFinalPushing)
        {
            // 当前帧计算购物车应在的世界坐标
            // 玩家 TransformPoint 可以把 localOffset 转换成正确的世界坐标
            Vector3 targetPos = playerRig.TransformPoint(localOffset);

            // 锁定购物车高度不随玩家头部上下动作改变
            targetPos.y = cart.position.y;

            // 把购物车位置设置到目标位置
            cart.position = Vector3.SmoothDamp(cart.position, targetPos, ref cartVelocity, pushSmoothTime, pushMaxSpeed);

            // 计算目标旋转
            float playerYaw = playerRig.eulerAngles.y;
            float targetYaw = playerYaw + yawOffset;
            Quaternion targetRotation = Quaternion.Euler(0f, targetYaw, 0f);
            cart.rotation = Quaternion.Slerp(cart.rotation, targetRotation, pushRotateSpeed * Time.deltaTime);
        }
        else
        {
            cartVelocity = Vector3.zero;
        }

        // 让购物车内物体跟随
        if (basketTrigger != null)
        {
            foreach (var info in basketTrigger.insideBasketItems)
            {
                if (info.obj == null)
                {
                    continue;
                }

                Rigidbody rb = info.obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.useGravity = !isFinalPushing;
                }

                // 仅在推动时控制位置和旋转
                if (isFinalPushing)
                {
                    Vector3 targetItemPos = cart.TransformPoint(info.localPos);
                    Quaternion targetItemRot = cart.rotation * info.localRot;

                    if (rb != null)
                    {
                        Vector3 toTarget = targetItemPos - rb.position;
                        Vector3 desiredVelocity = toTarget * itemFollowPositionStrength;
                        rb.velocity = Vector3.ClampMagnitude(desiredVelocity, itemMaxFollowSpeed);

                        Quaternion deltaRotation = targetItemRot * Quaternion.Inverse(rb.rotation);
                        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

                        if (angle > 180f)
                        {
                            angle -= 360f;
                        }

                        if (!float.IsNaN(axis.x) && axis.sqrMagnitude > 0.0001f)
                        {
                            Vector3 desiredAngularVelocity = axis.normalized * angle * Mathf.Deg2Rad * itemFollowRotationStrength;
                            rb.angularVelocity = desiredAngularVelocity;
                        }
                        else
                        {
                            rb.angularVelocity = Vector3.zero;
                        }
                    }
                    else
                    {
                        info.obj.transform.position = Vector3.Lerp(info.obj.transform.position, targetItemPos, itemFollowPositionStrength * Time.deltaTime);
                        info.obj.transform.rotation = Quaternion.Slerp(info.obj.transform.rotation, targetItemRot, itemFollowRotationStrength * Time.deltaTime);
                    }
                }
                else if (rb != null)
                {
                    rb.velocity *= 0.9f;
                    rb.angularVelocity *= 0.85f;
                }
            }
        }

        // 购物车 UI 交互逻辑
        bool aPressed = false;

        // primaryButton = A / B 之一（看系统映射）
        bool primary = rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out aPressed);

        // secondaryButton = 另一个按钮（防止 primary 是 B）
        bool secondary = false;
        rightHand.TryGetFeatureValue(CommonUsages.secondaryButton, out secondary);

        bool isPressed = (primary && aPressed) || secondary;

        // 检测按下瞬间
        if (isPressed && !lastAPressed)
        {
            Debug.Log("A 按钮按下（捕捉到 DOWN）");
            ToggleCalculator();
        }

        lastAPressed = isPressed;
    }

    private void ToggleCalculator()
    {
        isShow = !isShow;
        calculatorPricePanel.SetActive(isShow);
    }

    public void GetTotalPrice()
    {
        int price = shoppingTaskController.totalPrice;
        calculatorPriceText.text = price.ToString();
    }

    void PlayLevelCompleteSound()
    {
        if (levelAudioSource != null && levelAudioSource.clip != null)
            levelAudioSource.Play();
    }

    public void Settle()
    {
        calculatorPricePanel.SetActive(false);
        shoppingTaskController.Settle();
        if (shoppingTaskController.isTaskCompleted)
        {
            ResultPanel.SetActive(true);
            PlayLevelCompleteSound();
            ResultText.text = shoppingTaskController.taskCompletedText;
        }
        else
        {
            UnFinishedPanel.SetActive(true);
            // 启动延迟逻辑
            StartCoroutine(HandleUnfinishedUI());
        }
    }

    IEnumerator HandleUnfinishedUI()
    {
        yield return new WaitForSeconds(5f); // 等5秒

        UnFinishedPanel.SetActive(false);
        TaskPanel.SetActive(true);
        yield return new WaitForSeconds(5f); // 等5秒
        TaskPanel.SetActive(false);

        calculatorPricePanel.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LeftHand")) 
        {
            leftHandOn = true;
            leftHandLastContactTime = Time.time;
        }
        if (other.CompareTag("RightHand")) 
        {
            rightHandOn = true;
            rightHandLastContactTime = Time.time;
        }

        if (leftHandOn && rightHandOn && !isPushing)
        {
            Debug.Log("准备推车");
            StartPushMode();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("LeftHand"))
        {
            leftHandOn = true;
            leftHandLastContactTime = Time.time;
        }
        if (other.CompareTag("RightHand"))
        {
            rightHandOn = true;
            rightHandLastContactTime = Time.time;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LeftHand")) 
        {
            leftHandOn = false;
        }
        if (other.CompareTag("RightHand")) 
        {
            rightHandOn = false;
        }
    }

    void StartPushMode()
    {
        isPushing = true;

        // 把购物车当前的世界坐标转换成玩家局部坐标
        // 这样当玩家旋转时，局部坐标也会跟着旋转，不会乱飘
        localOffset = playerRig.InverseTransformPoint(cart.position);

        // 记录购物车与玩家之间在 yaw 的角度差
        yawOffset = Mathf.DeltaAngle(playerRig.eulerAngles.y, cart.eulerAngles.y);
    }

    void EndPushMode()
    {
        isPushing = false;
        isFinalPushing = false;
        cartVelocity = Vector3.zero;
    }
}