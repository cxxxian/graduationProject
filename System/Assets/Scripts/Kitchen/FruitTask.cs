using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class FruitTask : MonoBehaviour, ITask
{
    public string TaskName => "把冰箱中的香蕉全部拿到桌子上";
    public bool IsTaskComplete { get; private set; }
    public int goalCount = 2;

    private HashSet<GameObject> bananaInTable = new HashSet<GameObject>();
    private HashSet<GameObject> appleInTable = new HashSet<GameObject>();
    int bananaIndex = 0;
    int appleIndex = 0;

    // 延迟确认中的面包
    private Dictionary<GameObject, Coroutine> pendingChecks = new Dictionary<GameObject, Coroutine>();

    // 延迟时间（VR 抗抖）
    [SerializeField]
    private float confirmDelay = 0.3f;

    public void InitializeTask()
    {
        IsTaskComplete = false;
        bananaIndex = 0;
    }


    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Banana") && !bananaInTable.Contains(other.gameObject))
    //    {
    //        // 记录香蕉进入桌子区域
    //        PlayerEventSystem.Instance.RecordEnterZone(other.transform.parent.gameObject, "Table");

    //        bananaIndex++;
    //        bananaInTable.Add(other.gameObject);
    //        Debug.Log($"Banana 数量{bananaInTable.Count}" + $", {bananaIndex}");
    //        if (bananaInTable.Count >= goalCount)
    //        {
    //            IsTaskComplete = true;
    //            Debug.Log("香蕉任务完成");
    //        }
    //    }
    //    else if (other.CompareTag("Apple") && !appleInTable.Contains(other.gameObject))
    //    {
    //        appleIndex++;
    //        appleInTable.Add(other.gameObject);
    //        Debug.Log($"Apple 数量{appleInTable.Count}" + $", {appleIndex}");
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;

        if (!other.CompareTag("Banana") && !other.CompareTag("Apple"))
            return;

        // 已经确认在桌子上了
        if (bananaInTable.Contains(obj) || appleInTable.Contains(obj))
            return;

        // 已经在确认中
        if (pendingChecks.ContainsKey(obj))
            return;

        pendingChecks[obj] = StartCoroutine(ConfirmEnter(obj));
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Banana") && bananaInTable.Contains(other.gameObject))
    //    {
    //        // 记录香蕉离开桌子区域
    //        PlayerEventSystem.Instance.RecordExitZone(other.transform.parent.gameObject, "Table");

    //        bananaIndex++;
    //        bananaInTable.Remove(other.gameObject);
    //        Debug.Log($"Banana 数量{bananaInTable.Count}" + $", {bananaIndex}");
    //    }else if (other.CompareTag("Apple") && appleInTable.Contains(other.gameObject))
    //    {
    //        appleIndex++;
    //        appleInTable.Remove(other.gameObject);
    //        Debug.Log($"Apple 数量{appleInTable.Count}" + $", {appleIndex}");
    //    }
    //}

    private void OnTriggerExit(Collider other)
    {
        GameObject obj = other.gameObject;

        if (!other.CompareTag("Banana") && !other.CompareTag("Apple"))
            return;

        // 如果还在确认中,即代表抖动，直接取消这次进入尝试
        if (pendingChecks.TryGetValue(obj, out var co))
        {
            StopCoroutine(co);
            pendingChecks.Remove(obj);
            return;
        }

        // 真的离开桌子
        if (other.CompareTag("Banana") && bananaInTable.Remove(obj))
        {
            bananaIndex++;

            PlayerEventSystem.Instance.RecordExitZone(
                obj.transform.root.gameObject,
                "Table"
            );

            Debug.Log($"Banana 数量 {bananaInTable.Count}");
        }
        else if (other.CompareTag("Apple") && appleInTable.Remove(obj))
        {
            appleIndex++;
            Debug.Log($"Apple 数量 {appleInTable.Count}");
        }
    }


    IEnumerator ConfirmEnter(GameObject obj)
    {
        yield return new WaitForSeconds(confirmDelay);

        // 如果中途 Exit 过，说明是抖动
        if (!pendingChecks.ContainsKey(obj))
            yield break;

        pendingChecks.Remove(obj);

        if (obj.CompareTag("Banana"))
        {
            bananaInTable.Add(obj);
            bananaIndex++;

            PlayerEventSystem.Instance.RecordEnterZone(obj.transform.root.gameObject, "Table");

            Debug.Log($"Banana 数量 {bananaInTable.Count}");

            if (bananaInTable.Count >= goalCount)
            {
                IsTaskComplete = true;
                Debug.Log("香蕉任务完成");
            }
        }
        else if (obj.CompareTag("Apple"))
        {
            appleInTable.Add(obj);
            appleIndex++;
            Debug.Log($"Apple 数量 {appleInTable.Count}");
        }
    }


    public int GetBananaCount()
    {
        return bananaInTable.Count;
    }
    public int GetAppleCount()
    {
        return appleInTable.Count;
    }
    public float GetProgress()
    {
        return Mathf.Clamp01(GetBananaCount() / goalCount);
    }
}
