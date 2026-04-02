using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.Rendering.DebugUI;

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

    // 通过手柄控制开关的任务面板
    public GameObject missionPanel;
    public GameObject backHomeToggle;
    // 上一帧 A 键状态
    private bool lastAPressed = false;

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
    void Update()
    {
        if (!rightHand.isValid) return;

        bool aPressed;
        rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out aPressed);

        // 只在“按下瞬间”触发
        if (aPressed && !lastAPressed)
        {
            TogglePanel();
        }

        lastAPressed = aPressed;
    }

    void TogglePanel()
    {
        if (missionPanel == null) return;

        bool isActive = missionPanel.activeSelf;

        missionPanel.SetActive(!isActive);
        backHomeToggle.SetActive(!isActive);

        if (isActive)
        {
            PlayerEventSystem.Instance.RecordClose("任务面板");
        }
        else
        {
            PlayerEventSystem.Instance.RecordOpen("任务面板");
        }
        
        Debug.Log(isActive ? "关闭面板" : "打开面板");
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
