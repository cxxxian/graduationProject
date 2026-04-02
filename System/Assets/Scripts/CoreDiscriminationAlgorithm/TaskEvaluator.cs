using System.Collections.Generic;
using UnityEngine;
using static PlayerEventSystem;

public class TaskEvaluator : MonoBehaviour
{
    public TaskDefinition KitentaskDefinition;
    public TaskDefinition HandWorkDefinition;

    // 对齐结果
    public List<EventMatchResult> MatchResults { get; private set; } = new List<EventMatchResult>();

    // 错误列表
    // 标准步骤中被遗漏的事件
    public List<PlayerEventSystem.PlayerEvent> OmissionErrors { get; private set; } = new List<PlayerEventSystem.PlayerEvent>();

    // 实际操作有问题，例如顺序错误或多余操作
    public List<EventMatchResult> CommissionErrors { get; private set; } = new List<EventMatchResult>();

    // 操作上的小错误，例如重复抓取、反复尝试或操作不稳定
    public List<EventMatchResult> MotorErrors { get; private set; } = new List<EventMatchResult>();

    // 手工任务的完成stage
    //public LanternStickAssembler lanternStage;

    public float ITMN;
    public bool FirstTrueAction;
    public float PSMM;
    public float BE;

    // 厨房任务评估入口
    public void EvaluateKitchen()
    {
        var actualSequence = PlayerEventSystem.Instance.GetAllEvents();
        var standardSequence = KitentaskDefinition.StandardSequence;


        MatchResults.Clear();
        OmissionErrors.Clear();
        CommissionErrors.Clear();
        MotorErrors.Clear();
        ITMN = 0;
        FirstTrueAction = true;
        PSMM = 0;
        BE = 0;

        PlayerEventSystem.Instance.PrintStandardAllLogs(KitentaskDefinition.StandardSequence);

        bool[] standardMatched = new bool[standardSequence.Count];

        // ===== 1️⃣ 事件对齐 =====
        foreach (var actual in actualSequence)
        {
            int matchedIndex = -1;

            // 在所有“尚未匹配的标准步骤”中寻找
            for (int i = 0; i < standardSequence.Count; i++)
            {
                if (standardMatched[i]) continue;

                if (EventMatch(actual, standardSequence[i]))
                {
                    if (FirstTrueAction)
                    {
                        ITMN = actual.Time;
                        FirstTrueAction = false;
                    }
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

        /**
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
        **/
        // 记录做过的无效操作
        HashSet<(PlayerEventSystem.EventType, string)> invalidVisited = new();

        foreach (var r in MatchResults)
        {
            string normalizedTarget = NormalizeTarget(r.ActualEvent.Target);
            var key = (r.ActualEvent.Type, normalizedTarget);

            if (r.MatchedStandardIndex == -1)
            {
                // 第一次遇到无效操作 → 记录
                if (!invalidVisited.Contains(key))
                {
                    invalidVisited.Add(key);
                }
                else
                {
                    // 已经访问过的无效对象 → BE++
                    BE++;
                }

                CommissionErrors.Add(r);
                continue;
            }

            if (r.MatchedStandardIndex < lastMatchedStandard)
            {
                CommissionErrors.Add(r);
            }

            lastMatchedStandard = r.MatchedStandardIndex;
        }

        // ===== 3️⃣ Motor 错误（重复 / 不稳定）=====
        var lastKey = default((PlayerEventSystem.EventType, string));
        int consecutiveCount = 0;

        foreach (var r in MatchResults)
        {
            var key = (r.ActualEvent.Type, r.ActualEvent.Target);

            if (key.Equals(lastKey))
            {
                consecutiveCount++;
            }
            else
            {
                consecutiveCount = 1;
            }

            if (consecutiveCount >= 3)
            {
                MotorErrors.Add(r);
            }

            lastKey = key;
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

        PSMM = (float)standardSequence.Count / (float)actualSequence.Count;
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

    public class ShoppingState
    {
        public int milkNormalCount = 0;
        public int milkChocolateCount = 0;

        public int biscuitNormalCount = 0;
        public int biscuitWithNutCount = 0;

        public int pencilHBCount = 0;
        public int pencilOtherCount = 0;

        public int yogurtStrawberryCount = 0;

        public int totalPrice = 0;
    }

    // 购物任务评估入口
    public void EvaluateShopping()
    {
        var logs = PlayerEventSystem.Instance.GetAllEvents();

        OmissionErrors.Clear();
        CommissionErrors.Clear();
        MotorErrors.Clear();
        ITMN = 0;
        FirstTrueAction = true;
        PSMM = 0;
        BE = 0;

        ShoppingState state = new ShoppingState();

        // 用于 BE
        HashSet<string> invalidVisited = new HashSet<string>();

        // 解析日志，统计购物状态
        foreach (var e in logs)
        {
            if (e.Type != PlayerEventSystem.EventType.EnterZone)
                continue;

            if (FirstTrueAction)
            {
                ITMN = e.Time;
                FirstTrueAction = false;
            }

            string target = NormalizeTarget(e.Target);
            Debug.Log("当前判断对象" + target);
            switch (target)
            {
                case "NormalMilk":
                    state.milkNormalCount++;
                    state.totalPrice += 8;
                    break;

                case "ChocolateMilk":
                    state.milkChocolateCount++;
                    state.totalPrice += 8;
                    HandleInvalid(target, invalidVisited);
                    break;

                case "NormalBiscuit":
                    state.biscuitNormalCount++;
                    state.totalPrice += 15;
                    break;

                case "NutsBiscuit":
                    state.biscuitWithNutCount++;
                    state.totalPrice += 15;
                    HandleInvalid(target, invalidVisited);
                    break;

                case "Pencil":
                    state.pencilHBCount++;
                    state.totalPrice += 2;
                    break;

                case "RedPen":
                    state.pencilOtherCount++;
                    state.totalPrice += 3;
                    HandleInvalid(target, invalidVisited);
                    break;
                case "BlackPen":
                    state.pencilOtherCount++;
                    state.totalPrice += 3;
                    HandleInvalid(target, invalidVisited);
                    break;

                case "Yogurt":
                    state.yogurtStrawberryCount++;
                    state.totalPrice += 10;
                    break;
            }
        }

        // 规则验证
        // 基础任务
        if (state.milkNormalCount < 2)
        {
            //OmissionErrors.Add("牛奶数量不足");
            OmissionErrors.Add(new PlayerEvent(type: PlayerEventSystem.EventType.Grab, target: "", currentTime: 0f, context: "牛奶数量不足"));
            Debug.Log("牛奶数量不足");
        }
            
        if (state.biscuitNormalCount < 1 && state.yogurtStrawberryCount <= 0)
        {
            OmissionErrors.Add(new PlayerEvent(type: PlayerEventSystem.EventType.Grab, target: "", currentTime: 0f, context: "无坚果饼干缺失"));
            Debug.Log("无坚果饼干缺失");
        }

        if (state.pencilHBCount < 3)
        {
            OmissionErrors.Add(new PlayerEvent(type: PlayerEventSystem.EventType.Grab, target: "", currentTime: 0f, context: "HB铅笔不足"));
            Debug.Log("HB铅笔不足");
        }
            
        // --- 类型错误 ---
        if (state.milkChocolateCount > 0)
        {
            CommissionErrors.Add(new EventMatchResult(new PlayerEvent(type: PlayerEventSystem.EventType.Grab, target: "", currentTime: 0f, context: "买错牛奶类型"), 1));
            Debug.Log("买错牛奶类型");
        }

        if (state.biscuitWithNutCount > 0)
        {
            CommissionErrors.Add(new EventMatchResult(new PlayerEvent(type: PlayerEventSystem.EventType.Grab, target: "", currentTime: 0f, context: "买了有坚果饼干"), 1));
            Debug.Log("买了有坚果饼干");

        }

        if (state.pencilOtherCount > 0)
        {
            CommissionErrors.Add(new EventMatchResult(new PlayerEvent(type: PlayerEventSystem.EventType.Grab, target: "", currentTime: 0f, context: "买错铅笔型号"), 1));
            Debug.Log("买错铅笔型号");
        }

        // --- 预算错误 ---
        if (state.totalPrice > 40)
        {
            CommissionErrors.Add(new EventMatchResult(new PlayerEvent(type: PlayerEventSystem.EventType.Grab, target: "", currentTime: 0f, context: "预算超支"), 1));
            Debug.Log("预算超支");

        }

        // --- 临时规则 ---
        if (state.yogurtStrawberryCount > 0 &&
            state.biscuitNormalCount != 0)
        {
            CommissionErrors.Add(new EventMatchResult(new PlayerEvent(type: PlayerEventSystem.EventType.Grab, target: "", currentTime: 0f, context: "未扣减饼干预算"), 1));
            Debug.Log("未扣减饼干预算");
        }

        Debug.Log($"Omission: {OmissionErrors.Count}");
        Debug.Log($"Commission: {CommissionErrors.Count}");
        Debug.Log($"BE: {BE}");
    }

    // 手工任务评估入口
    public void EvaluateHandwork()
    {
        var actualSequence = PlayerEventSystem.Instance.GetAllEvents();
        var standardSequence = HandWorkDefinition.StandardSequence;

        MatchResults.Clear();
        //OmissionErrors.Clear();
        CommissionErrors.Clear();
        MotorErrors.Clear();

        ITMN = 0;
        FirstTrueAction = true;
        PSMM = 0;
        BE = 0;

        PlayerEventSystem.Instance.PrintStandardAllLogs(HandWorkDefinition.StandardSequence);

        int currentStandardIndex = 0;

        HashSet<(PlayerEventSystem.EventType, string)> invalidVisited = new();
        Dictionary<(PlayerEventSystem.EventType, string), int> repeatCounter = new();

        foreach (var actual in actualSequence)
        {
            string normalizedTarget = NormalizeTarget(actual.Target);
            var key = (actual.Type, normalizedTarget);

            int matchedIndex = -1;

            // 只匹配当前期待步骤
            if (currentStandardIndex < standardSequence.Count &&
                EventMatch(actual, standardSequence[currentStandardIndex]))
            {
                matchedIndex = currentStandardIndex;

                if (FirstTrueAction)
                {
                    ITMN = actual.Time;
                    FirstTrueAction = false;
                }

                currentStandardIndex++; // 严格推进
            }
            else
            {
                // 判断是不是未来步骤提前做
                bool isFutureStep = false;

                for (int i = currentStandardIndex + 1; i < standardSequence.Count; i++)
                {
                    if (EventMatch(actual, standardSequence[i]))
                    {
                        isFutureStep = true;
                        break;
                    }
                }

                if (isFutureStep)
                {
                    CommissionErrors.Add(new EventMatchResult(actual, -1));
                }
                else
                {
                    // 完全不在标准中的操作
                    if (!invalidVisited.Contains(key))
                        invalidVisited.Add(key);
                    else
                        BE++;

                    CommissionErrors.Add(new EventMatchResult(actual, -1));
                }
            }

            MatchResults.Add(new EventMatchResult(actual, matchedIndex));

            
        }
        // Motor 错误（重复 / 不稳定）
        var lastKey = default((PlayerEventSystem.EventType, string));
        int consecutiveCount = 0;

        foreach (var r in MatchResults)
        {
            var key = (r.ActualEvent.Type, r.ActualEvent.Target);

            if (key.Equals(lastKey))
            {
                consecutiveCount++;
            }
            else
            {
                consecutiveCount = 1;
            }

            if (consecutiveCount >= 3)
            {
                MotorErrors.Add(r);
            }

            lastKey = key;
        }

        // Omission（剩下没完成的都是遗漏）
        for (int i = currentStandardIndex; i < standardSequence.Count; i++)
        {
            OmissionErrors.Add(standardSequence[i]);
        }

        // ===== Debug 输出 =====
        Debug.Log($"[Evaluation] Matched {standardSequence.Count - OmissionErrors.Count} / {standardSequence.Count}");

        foreach (var e in OmissionErrors)
            Debug.Log($"[遗漏错误Omission Error] Missing {e.Type} {e.Target}");

        foreach (var e in CommissionErrors)
            Debug.Log($"[执行错误Commission Error] Extra or wrong order: {e.ActualEvent.Type} {e.ActualEvent.Target}");

        foreach (var e in MotorErrors)
            Debug.Log($"[运动错误Motor Error] Repeated or unstable action: {e.ActualEvent.Type} {e.ActualEvent.Target}");

        PSMM = (float)standardSequence.Count / (float)actualSequence.Count;
    
    }
    private void HandleInvalid(string target, HashSet<string> invalidVisited)
    {
        if (!invalidVisited.Contains(target))
        {
            invalidVisited.Add(target);
        }
        else
        {
            BE++;
        }
    }

    // 收集结果总结
    public TaskResultData GetResultSummary()
    {
        TaskResultData result = new TaskResultData();

        result.SceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        result.MatchedCount = MatchResults.Count - CommissionErrors.Count;
        result.OmissionCount = OmissionErrors.Count;
        result.CommissionCount = CommissionErrors.Count;
        result.MotorCount = MotorErrors.Count;

        int totalStandard = 1;
        if (HandWorkDefinition != null)
        {
            totalStandard = HandWorkDefinition.StandardSequence.Count;
        }
        else if (KitentaskDefinition != null)
        {
            totalStandard = KitentaskDefinition.StandardSequence.Count;
        }
        
        
        result.CompletionRate = (float)(totalStandard - OmissionErrors.Count) / totalStandard;

        // 保存真实 logs
        result.RawLogs = new List<PlayerEventSystem.PlayerEvent>(
            PlayerEventSystem.Instance.GetAllEvents()
        );

        result.ITMN = ITMN;
        result.PSMM = PSMM;
        result.BE = BE;

        return result;
    }

    //工具函数，用来屏蔽物体后面的数字编号
    string NormalizeTarget(string target)
    {
        int i = target.Length - 1;

        while (i >= 0 && char.IsDigit(target[i]))
        {
            i--;
        }

        return target.Substring(0, i + 1);
    }
}
