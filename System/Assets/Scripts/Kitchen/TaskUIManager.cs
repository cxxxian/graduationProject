using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class TaskUIManager : MonoBehaviour
{
    [Header("厨房任务对应的状态文本")]
    public TMP_Text breadStatusText;
    public TMP_Text coffeeStatusText;
    public TMP_Text bananaStatusText;

    [Header("购物练习任务对应的状态文本")]
    public TMP_Text panelStatusText;
    public TMP_Text pushStatusText;

    // 右手设备
    private InputDevice rightHand;

    void Start()
    {
        // 获取右手设备
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);

        if (devices.Count > 0)
        {
            rightHand = devices[0];
        }

    }
    private void Update()
    {
        bool aPressed = false;
        // primaryButton = A / B 之一（看系统映射）
        bool primary = rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out aPressed);

        // 检测按下瞬间
        if (aPressed)
        {
            Debug.Log("A 按钮按下（捕捉到 DOWN）");
        }
    }

    // 更新面包任务状态
    public void SetBreadTaskComplete(bool completed)
    {
        if (breadStatusText != null)
        {
            //Debug.Log("面包标记为已完成！");
            breadStatusText.text = completed ? "已完成" : "未完成";
        }
    }

    // 更新咖啡任务状态
    public void SetCoffeeTaskComplete(bool completed)
    {
        if (coffeeStatusText != null)
        {
            coffeeStatusText.text = completed ? "已完成" : "未完成";
        }
    }

    // 更新香蕉任务状态
    public void SetBananaTaskComplete(bool completed)
    {
        if (bananaStatusText != null)
        {
            bananaStatusText.text = completed ? "已完成" : "未完成";
        }
    }

    // 更新开启/关闭虚拟计算器面板任务状态
    public void SetPanelTaskComplete(bool completed)
    {
        if (panelStatusText != null)
        {
            panelStatusText.text = completed ? "已完成" : "未完成";
        }
    }

    // 更新推车任务状态
    public void SetPushTaskComplete(bool completed)
    {
        if (pushStatusText != null)
        {
            pushStatusText.text = completed ? "已完成" : "未完成";
        }
    }

}
