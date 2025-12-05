using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelTask : MonoBehaviour, ITask
{
    public string TaskName => "开启/关闭虚拟计算器";
    public bool IsTaskComplete { get; private set; }

    [Header("要检测的面板")]
    public GameObject calculatorPricePanel;

    private bool hasOpened = false;

    public void InitializeTask()
    {
        IsTaskComplete = false;
        hasOpened = false;
    }

    private void Update()
    {
        if (IsTaskComplete)
        {
            return;
        }
        if (calculatorPricePanel == null)
        {
            return;
        }

        // 第一步：检测是否被打开过
        if (!hasOpened && calculatorPricePanel.activeInHierarchy)
        {
            hasOpened = true;
            // Debug.Log("Panel 已开启");
        }

        // 第二步：检测已经打开过，又被关闭，任务完成
        if (hasOpened && !calculatorPricePanel.activeInHierarchy)
        {
            IsTaskComplete = true;
            // Debug.Log("Panel 已关闭，任务完成！");
        }
    }
}
