using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITask
{
    // 任务是否完成
    bool IsTaskComplete { get; }

    // 任务名称
    string TaskName { get; }

    // 任务进度 (0 ~ 1)
    float GetProgress();

    // 任务初始化
    void InitializeTask();
}