using System.Collections.Generic;
using UnityEngine;

public class TaskEvaluator : MonoBehaviour
{
    public TaskDefinition taskDefinition;

    // 对齐结果
    public List<EventMatchResult> MatchResults { get; private set; } = new List<EventMatchResult>();

    // 错误列表
    // 标准步骤中被遗漏的事件
    public List<PlayerEventSystem.PlayerEvent> OmissionErrors { get; private set; } = new List<PlayerEventSystem.PlayerEvent>();

    // 实际操作有问题，例如顺序错误或多余操作
    public List<EventMatchResult> CommissionErrors { get; private set; } = new List<EventMatchResult>();

    // 操作上的小错误，例如重复抓取、反复尝试或操作不稳定
    public List<EventMatchResult> MotorErrors { get; private set; } = new List<EventMatchResult>();

    // 评估入口
    public void Evaluate()
    {
        var actualSequence = PlayerEventSystem.Instance.GetAllEvents();
        var standardSequence = taskDefinition.StandardSequence;

        MatchResults.Clear();
        OmissionErrors.Clear();
        CommissionErrors.Clear();
        MotorErrors.Clear();

        PlayerEventSystem.Instance.PrintStandardAllLogs(taskDefinition.StandardSequence);

        bool[] standardMatched = new bool[standardSequence.Count];

        // ===== 1️⃣ 事件对齐（不推进标准指针）=====
        foreach (var actual in actualSequence)
        {
            int matchedIndex = -1;

            // 在所有“尚未匹配的标准步骤”中寻找
            for (int i = 0; i < standardSequence.Count; i++)
            {
                if (standardMatched[i]) continue;

                if (EventMatch(actual, standardSequence[i]))
                {
                    Debug.Log("真实事件"+ $"[PlayerEvent] {actual.Type}  Target:{actual.Target}  Time:{actual.Time}");
                    Debug.Log("对应标准事件"+ $"[PlayerEvent] {standardSequence[i].Type}  Target:{standardSequence[i].Target}");
                    matchedIndex = i;
                    standardMatched[i] = true;
                    break;
                }
            }

            MatchResults.Add(new EventMatchResult(actual, matchedIndex));
        }

        // ===== 2️⃣ 顺序 & Commission 错误判断 =====
        int lastMatchedStandard = -1;

        foreach (var r in MatchResults)
        {
            if (r.MatchedStandardIndex == -1)
            {
                // 完全不在标准中的操作
                CommissionErrors.Add(r);
                continue;
            }

            if (r.MatchedStandardIndex < lastMatchedStandard)
            {
                // 回退 / 提前操作 → 顺序错误
                CommissionErrors.Add(r);
            }

            lastMatchedStandard = r.MatchedStandardIndex;
        }

        // ===== 3️⃣ Motor 错误（重复 / 不稳定）=====
        Dictionary<(PlayerEventSystem.EventType, string), int> repeatCounter = new();

        foreach (var r in MatchResults)
        {
            var key = (r.ActualEvent.Type, r.ActualEvent.Target);

            if (!repeatCounter.ContainsKey(key))
                repeatCounter[key] = 0;

            repeatCounter[key]++;

            if (repeatCounter[key] >= 3)
            {
                MotorErrors.Add(r);
            }
        }

        // ===== 4️⃣ Omission 错误 =====
        for (int i = 0; i < standardSequence.Count; i++)
        {
            if (!standardMatched[i])
            {
                OmissionErrors.Add(standardSequence[i]);
            }
        }

        // ===== Debug 输出 =====
        Debug.Log($"[Evaluation] Matched {standardSequence.Count - OmissionErrors.Count} / {standardSequence.Count}");

        foreach (var e in OmissionErrors)
            Debug.Log($"[遗漏错误Omission Error] Missing {e.Type} {e.Target}");

        foreach (var e in CommissionErrors)
            Debug.Log($"[执行错误Commission Error] Extra or wrong order: {e.ActualEvent.Type} {e.ActualEvent.Target}");

        foreach (var e in MotorErrors)
            Debug.Log($"[运动错误Motor Error] Repeated or unstable action: {e.ActualEvent.Type} {e.ActualEvent.Target}");
    }


    // 核心匹配逻辑
    private bool EventMatch(PlayerEventSystem.PlayerEvent actual, PlayerEventSystem.PlayerEvent standard)
    {
        if (actual.Type != standard.Type)
            return false;

        if (!TargetMatch(actual.Target, standard.Target))
            return false;

        switch (standard.Type)
        {
            case PlayerEventSystem.EventType.EnterZone:
            case PlayerEventSystem.EventType.ExitZone:
                return actual.Context == standard.Context;

            case PlayerEventSystem.EventType.Grab:
            case PlayerEventSystem.EventType.Drop:
            case PlayerEventSystem.EventType.Open:
            case PlayerEventSystem.EventType.Close:
                return true;
        }

        return false;
    }

    private bool TargetMatch(string actual, string standard)
    {
        if (string.IsNullOrEmpty(actual) || string.IsNullOrEmpty(standard))
            return false;

        return actual.ToLower().Contains(standard.ToLower());
    }
}
