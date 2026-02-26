using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TaskResultData
{
    public string SceneName;

    public int MatchedCount;
    public int OmissionCount;
    public int CommissionCount;
    public int MotorCount;

    public float CompletionRate;

    // 原始行为日志
    public List<PlayerEventSystem.PlayerEvent> RawLogs;

    public float ITMN;
    public float PSMM;
    public float BE;
}