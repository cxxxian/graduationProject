using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class ShoppingCart : MonoBehaviour
{
    public ShoppingTaskController shoppingTaskController;
    
    // 计算器UI
    public GameObject calculatorPricePanel;
    public TMP_Text calculatorPriceText;

    // 结算面板UI
    public GameObject ResultPanel;
    public TMP_Text ResultText;

    // calculatorPricePanel是否显示
    private bool isShow = false;

    // 右手设备
    private InputDevice rightHand;

    // 用来做按下瞬间检测
    private bool lastAPressed = false;

    void Start()
    {
        calculatorPricePanel.SetActive(false);
        ResultPanel.SetActive(false);

        // 获取右手设备
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);

        if (devices.Count > 0)
            rightHand = devices[0];
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

    public void Settle()
    {
        calculatorPricePanel.SetActive(false);
        shoppingTaskController.Settle();
        ResultPanel.SetActive(true);
        ResultText.text = shoppingTaskController.taskCompletedText;
    }
}