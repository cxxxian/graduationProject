using System.Collections;
using TMPro;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    // 关卡介绍面板
    public GameObject introducePanel;
    // 物品清单记忆面板
    public GameObject memoryPanel;
    // 任务面板
    public GameObject taskPanel;

    // 倒计时的text meshpro
    public TMP_Text timerText;
    // Inspector 调节
    public float countdownTime = 20f;

    private float currentTime;
    private Coroutine countdownCoroutine;

    // 按钮调用的方法
    public void StartMemory()
    {
        introducePanel.SetActive(false);
        memoryPanel.SetActive(true);
        StartCountdown();
    }

    private void StartCountdown()
    {
        // 如果已经在倒计时，先停止
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        currentTime = countdownTime;
        countdownCoroutine = StartCoroutine(TimerRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateText();
            yield return null;
        }

        // 防止显示 -0.01 之类
        currentTime = 0;
        UpdateText();

        OnCountdownFinished();
    }

    private void UpdateText()
    {
        timerText.text = Mathf.CeilToInt(currentTime).ToString();
    }

    private void OnCountdownFinished()
    {
        Debug.Log("倒计时结束！");
        memoryPanel.SetActive(false);
        taskPanel.SetActive(true);
    }
}

/** 
using System.Collections;
using TMPro;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    [Header("倒计时时间（秒）")]
    public float countdownTime = 20f; // Inspector 可调节

    [Header("倒计时显示文本")]
    public TMP_Text timerText; // 拖入 TextMeshPro 对象

    private float currentTime;
    private Coroutine countdownCoroutine;

    void Start()
    {
        ResetTimer();
        StartCountdown();
    }

    // 重置计时器
    public void ResetTimer()
    {
        currentTime = countdownTime;
        UpdateText();
    }

    // 开始倒计时
    public void StartCountdown()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(TimerRoutine());
    }

    // 停止倒计时
    public void StopCountdown()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = null;
    }

    // 协程倒计时逻辑
    private IEnumerator TimerRoutine()
    {
        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateText();
            yield return null;
        }

        currentTime = 0;
        UpdateText();
        OnCountdownFinished();
    }

    // 更新 TMP 文本
    private void UpdateText()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.Ceil(currentTime).ToString();
        }
    }

    // 倒计时结束事件（你可以在这里做事件回调）
    private void OnCountdownFinished()
    {
        Debug.Log("倒计时结束！");
    }
}
**/
