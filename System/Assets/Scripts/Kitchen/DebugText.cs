using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugText : MonoBehaviour
{
    // 关联倒水控制脚本
    public CoffeeTask kettle;
    public Text infoText;

    private string lastState = "";

    void Update()
    {
        if (kettle == null || infoText == null)
            return;

        string text = "";

        if (kettle.IsPouringIntoCup())
        {
            text = $" 正在倒水中...进度: {(kettle.GetProgress() * 100f):F1}%";
        }
        else if (kettle.GetProgress() >= 1f)
        {
            text = " 倒满啦！";
        }
        else
        {
            text = $" 未倒水.进度: {(kettle.GetProgress() * 100f):F1}%";
        }

        // 避免频繁更新文本导致性能浪费
        if (text != lastState)
        {
            infoText.text = text;
            lastState = text;
        }
    }
}
