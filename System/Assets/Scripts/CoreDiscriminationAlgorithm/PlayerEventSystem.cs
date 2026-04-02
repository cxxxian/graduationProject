using System;
using System.Collections.Generic;
using UnityEngine;

// 玩家行为记录系统
// 记录玩家行为事件，用于任务脚本判断
public class PlayerEventSystem : MonoBehaviour
{
    public static PlayerEventSystem Instance;

    //private void Awake()
    //{
    //    Instance = this;
    //}

    private void Awake()
    {
        Instance = this;
    }


    //void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //        DontDestroyOnLoad(gameObject);
    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //    }
    //}

    // 事件类型枚举
    public enum EventType
    {
        // 拿起/放下物品
        Grab,
        Drop,
        // 进入/离开某个区域
        EnterZone,
        ExitZone,
        //打开/关闭冰箱/任务提示面板
        Open,
        Close,
    }

    // 单条事件结构体
    [System.Serializable]
    public class PlayerEvent
    {
        // JSON 里读进来的原始字符串
        public string StringType;

        // 程序真正用的枚举（不参与 JSON）
        [System.NonSerialized]
        public EventType Type;
        public string Target;
        public float Time;
        public string Context;

        public PlayerEvent(EventType type, string target, float currentTime, string context = "")
        {
            Type = type;
            Target = target;
            // 从 Timer 传入
            Time = currentTime;
            Context = context;
        }
    }

    // 事件记录与访问
    private List<PlayerEvent> logs = new List<PlayerEvent>();

    // 记录一个事件
    public void RecordEvent(EventType type, string target, float currentTime, string context = "")
    {
        var e = new PlayerEvent(type, target, currentTime, context);
        logs.Add(e);
        if (type == EventType.Grab || type == EventType.Drop)
        {
            Debug.Log($"[Instance]{Instance.GetInstanceID()} [PlayerEvent] {type}  Target:{target}  Time:{currentTime}");
        }
        else if (type == EventType.EnterZone || type == EventType.ExitZone) {
            Debug.Log($"[Instance]{Instance.GetInstanceID()}[PlayerEvent] {type}  Target:{target}  To Zone:{context}  Time:{currentTime}");
        }
        else if (type == EventType.Open || type == EventType.Close)
        {
            Debug.Log($"[PlayerEvent] {type}  Target:{target}  Time:{currentTime}");
        }

    }

    // 获取全部事件日志
    public List<PlayerEvent> GetAllEvents()
    {
        return logs;
    }

    // 清空事件日志
    public void Clear()
    {
        logs.Clear();
    }

    public void PrintAllLogs()
    {
        Debug.Log("===== Player Event Logs Start =====");

        foreach (var e in logs)
        {
            if (e.Type == EventType.Grab || e.Type == EventType.Drop)
            {
                Debug.Log($"[PlayerEvent] {e.Type}  Target:{e.Target}  Time:{e.Time}");
            }
            else if (e.Type == EventType.EnterZone || e.Type == EventType.ExitZone)
            {
                Debug.Log($"[PlayerEvent] {e.Type}  Target:{e.Target}  To Zone:{e.Context}  Time:{e.Time}");
            }
            else if (e.Type == EventType.Open || e.Type == EventType.Close)
            {
                Debug.Log($"[PlayerEvent] {e.Type}  Target:{e.Target}  Time:{e.Time}");
            }
        }

        Debug.Log("===== Player Event Logs End =====");
    }

    public void PrintStandardAllLogs(List<PlayerEvent> StandardSequence)
    {
        Debug.Log("===== Standard Event Logs Start =====");

        foreach (var e in StandardSequence)
        {
            if (e.Type == EventType.Grab || e.Type == EventType.Drop)
            {
                Debug.Log($"[PlayerEvent] {e.Type}  Target:{e.Target}  Time:{e.Time}");
            }
            else if (e.Type == EventType.EnterZone || e.Type == EventType.ExitZone)
            {
                Debug.Log($"[PlayerEvent] {e.Type}  Target:{e.Target}  To Zone:{e.Context}  Time:{e.Time}");
            }
            else if (e.Type == EventType.Open || e.Type == EventType.Close)
            {
                Debug.Log($"[PlayerEvent] {e.Type}  Target:{e.Target}  Time:{e.Time}");
            }
        }

        Debug.Log("===== Standard Event Logs End =====");
    }


    // 行为记录绑定到物体
    public void RecordGrab(GameObject obj)
    {
        RecordEvent(EventType.Grab, obj.name, Timer.Instance.GetTimePassed());
    }

    public void RecordDrop(GameObject obj)
    {
        RecordEvent(EventType.Drop, obj.name, Timer.Instance.GetTimePassed());
    }

    public void RecordEnterZone(GameObject obj, string zone)
    {
        RecordEvent(EventType.EnterZone, obj.name, Timer.Instance.GetTimePassed(), zone);
    }
    public void RecordExitZone(GameObject obj, string zone)
    {
        RecordEvent(EventType.ExitZone, obj.name, Timer.Instance.GetTimePassed(), zone);
    }
    public void RecordOpen(string door)
    {
        RecordEvent(EventType.Open, door, Timer.Instance.GetTimePassed());
    }
    public void RecordClose(string door)
    {
        RecordEvent(EventType.Close, door, Timer.Instance.GetTimePassed());
    }
}
