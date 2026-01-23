using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreadTask : MonoBehaviour, ITask
{
    public string TaskName => "拿两片吐司到盘子中";
    public bool IsTaskComplete { get; private set; }
    public int goalCount = 2;

    private HashSet<GameObject> breadsInPlate = new HashSet<GameObject>();

    // 延迟确认中的面包
    private Dictionary<GameObject, Coroutine> pendingChecks = new Dictionary<GameObject, Coroutine>();

    // 延迟时间（VR 抗抖）
    [SerializeField]
    private float confirmDelay = 0.3f;
    int index = 0;

    public void InitializeTask()
    {
        IsTaskComplete = false;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Bread"))
    //    {
    //        index++;
    //        breadsInPlate.Add(other.gameObject);
    //        Debug.Log($"Bread 数量{breadsInPlate.Count}" + $", {index}");
    //        PlayerEventSystem.Instance.RecordEnterZone(other.transform.parent.gameObject, "Plate");
    //        if (breadsInPlate.Count >= goalCount)
    //        {
    //            IsTaskComplete = true;
    //            Debug.Log("面包任务完成");

    //        }
    //    }

    //}

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Bread")) return;

        GameObject bread = other.gameObject;

        // 已经确认在盘子里了，不重复处理
        if (breadsInPlate.Contains(bread)) return;

        // 已经在等待确认中，也不重复
        if (pendingChecks.ContainsKey(bread)) return;

        // 开始延迟确认
        pendingChecks[bread] = StartCoroutine(ConfirmEnter(bread));
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Bread"))
    //    {
    //        index++;
    //        breadsInPlate.Remove(other.gameObject);
    //        Debug.Log($"Bread 数量{breadsInPlate.Count}" + $", {index}");
    //        PlayerEventSystem.Instance.RecordExitZone(other.transform.parent.gameObject, "Plate");
    //    }
    //}

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Bread")) return;

        GameObject bread = other.gameObject;

        // 如果还在确认中,即代表抖动，直接取消这次进入尝试
        if (pendingChecks.TryGetValue(bread, out var co))
        {
            StopCoroutine(co);
            pendingChecks.Remove(bread);
            return;
        }

        // 真的离开盘子
        if (breadsInPlate.Remove(bread))
        {
            Debug.Log($"Bread 数量 {breadsInPlate.Count}");

            PlayerEventSystem.Instance.RecordExitZone(bread.transform.root.gameObject, "Plate");
        }
    }

    IEnumerator ConfirmEnter(GameObject bread)
    {
        yield return new WaitForSeconds(confirmDelay);

        // 如果中途被 Exit 取消了
        if (!pendingChecks.ContainsKey(bread))
            yield break;

        pendingChecks.Remove(bread);
        breadsInPlate.Add(bread);

        Debug.Log($"Bread 数量 {breadsInPlate.Count}");

        PlayerEventSystem.Instance.RecordEnterZone(bread.transform.root.gameObject, "Plate");

        if (breadsInPlate.Count >= goalCount)
        {
            IsTaskComplete = true;
            Debug.Log("面包任务完成");
        }
    }

    public int GetBreadCount()
    {
        return breadsInPlate.Count;
    }
    public float GetProgress()
    {
        return Mathf.Clamp01(GetBreadCount() / goalCount);
    }

}
