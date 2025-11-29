using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    // Timer实例
    public static Timer Instance;

    public TMP_Text timerText;

    private float timePassed = 0f;
    private bool isRunning = false; // 是否在计时

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!isRunning) return; // 未开始不计时

        timePassed += Time.deltaTime;

        int minutes = (int)(timePassed / 60);
        int seconds = (int)(timePassed % 60);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    // 在开始按钮中调用这个
    public void StartTimer()
    {
        isRunning = true;
        // 如果你希望每次开始游戏都重置计时器
        timePassed = 0f;
    }

    // 如果有结束任务逻辑，用来停止计时
    public void StopTimer()
    {
        isRunning = false;
    }

    // 提供获取当前时间的方法
    public float GetTimePassed()
    {
        return timePassed;
    }
}
