using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TaskUIManager : MonoBehaviour
{
    [Header("厨房任务对应的状态文本")]
    public TMP_Text breadStatusText;
    public TMP_Text coffeeStatusText;
    public TMP_Text bananaStatusText;

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

}
