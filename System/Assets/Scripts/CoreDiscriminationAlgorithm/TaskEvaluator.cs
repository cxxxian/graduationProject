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
        // 从 PlayerEventSystem 获取玩家执行的所有事件
        List<PlayerEventSystem.PlayerEvent> actualSequence = PlayerEventSystem.Instance.GetAllEvents();
        // 从 TaskDefinition 获取标准流程
        List<PlayerEventSystem.PlayerEvent> standardSequence = taskDefinition.StandardSequence;

        MatchResults.Clear();
        OmissionErrors.Clear();
        CommissionErrors.Clear();
        MotorErrors.Clear();

        // 布尔数组标记标准事件是否被匹配
        // 用于 遗漏错误（Omission）
        bool[] standardMatched = new bool[standardSequence.Count];

        // 快慢指针的慢指针，用于顺序匹配
        int standardIndex = 0;
        int trueStandardIndex = 0;

        // 循环每条实际事件
        // 内部 while 循环 → 慢指针匹配标准事件
        foreach (var actual in actualSequence)
        {
            // 如果最终没匹配 → matchedIndex = -1
            int matchedIndex = -1;
            // 每次从已匹配的下一个行为标准开始匹配
            standardIndex = trueStandardIndex;

            // 快慢指针匹配
            while (standardIndex < standardSequence.Count)
            {
                
                if (EventMatch(actual, standardSequence[standardIndex]))
                {
                    // 匹配成功
                    // 记录 matchedIndex
                    matchedIndex = standardIndex;
                    Debug.Log("匹配成功的行为：" + $"[PlayerEvent] {actual.Type}  Target:{actual.Target}  Time:{actual.Time}");
                    // 将对应标准事件标记为已匹配
                    standardMatched[standardIndex] = true;
                    standardIndex++;
                    trueStandardIndex = standardIndex;
                    break;
                }
                else
                {
                    // 匹配失败
                    Debug.Log("匹配失败的行为：" + $"[PlayerEvent] {actual.Type}  Target:{actual.Target}  Time:{actual.Time}");
                    Debug.Log("当前的标准行为为：" + $"[PlayerEvent] {standardSequence[standardIndex].Type}  Target:{standardSequence[standardIndex].Target}  Time:{standardSequence[standardIndex].Time}");
                    // standardIndex 前进，继续尝试匹配后面的标准事件
                    standardIndex++;
                }
            }
            // 每条实际事件都生成一条对应结果，用于错误检测
            MatchResults.Add(new EventMatchResult(actual, matchedIndex));

            // Commission / Motor 错误初步判断
            if (matchedIndex == -1)
            {
                // 没匹配到任何标准步骤 → 多余操作或错误顺序
                CommissionErrors.Add(new EventMatchResult(actual, -1));
            }
            else if (standardIndex > matchedIndex + 1)
            {
                // 匹配到的标准步骤之前被跳过 → 顺序问题，也算 Commission
                CommissionErrors.Add(new EventMatchResult(actual, matchedIndex));
            }

            // Motor 错误示例：重复抓取或多余动作
            // 这里简单示例：同一对象连续 Grab/Drop 视为 Motor
            if (MatchResults.Count >= 2)
            {
                var prev = MatchResults[MatchResults.Count - 2];
                if (actual.Type == PlayerEventSystem.EventType.Grab && prev.ActualEvent.Target == actual.Target && prev.ActualEvent.Type == PlayerEventSystem.EventType.Grab)
                {
                    MotorErrors.Add(new EventMatchResult(actual, matchedIndex));
                }
            }
        }

        // 检查遗漏标准事件
        for (int i = 0; i < standardSequence.Count; i++)
        {
            if (!standardMatched[i])
            {
                OmissionErrors.Add(standardSequence[i]);
            }
        }

        // 输出统计
        Debug.Log($"[Evaluation] Matched {standardSequence.Count - OmissionErrors.Count} / {standardSequence.Count}");
        foreach (var e in OmissionErrors)
        {
            Debug.Log($"[Omission Error] Missing {e.Type} {e.Target}");
        }

        foreach (var e in CommissionErrors)
        {
            Debug.Log($"[Commission Error] Extra or wrong order: {e.ActualEvent.Type} {e.ActualEvent.Target}");
        }

        foreach (var e in MotorErrors)
        {
            Debug.Log($"[Motor Error] Repeated or unstable action: {e.ActualEvent.Type} {e.ActualEvent.Target}");
        }
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
