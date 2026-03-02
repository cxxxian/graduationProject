using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketTrigger : MonoBehaviour
{
    [Tooltip("指定用来检测物品的 Trigger Collider")]
    public Collider triggerCollider;

    public List<BasketItemInfo> insideBasketItems = new List<BasketItemInfo>();

    // 延迟抗抖核心
    private Dictionary<GameObject, Coroutine> pendingEnterChecks = new();
    private Dictionary<GameObject, Coroutine> pendingExitChecks = new();

    [SerializeField]
    private float confirmDelay = 0.3f;

    void Awake()
    {
        if (triggerCollider == null)
        {
            Debug.LogWarning("请在 Inspector 指定 Trigger Collider");
        }
        else
        {
            // 确保是 Trigger
            triggerCollider.isTrigger = true;
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log("TriggerEnter: " + other.name);
    //    var itemComp = other.GetComponent<ItemComponent>();
    //    Debug.Log("ItemComponent: " + (itemComp != null ? itemComp.data.itemName : "null"));
    //    if (itemComp == null) return;

    //    if (!insideBasketItems.Exists(x => x.obj == other.gameObject))
    //    {
    //        insideBasketItems.Add(new BasketItemInfo(other.gameObject, transform));
    //    }

    //    // 调用任务统计
    //    var taskController = FindObjectOfType<ShoppingTaskController>();
    //    taskController?.OnItemAdded(itemComp);
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    Debug.Log("TriggerExist: " + other.name);
    //    var itemComp = other.GetComponent<ItemComponent>();
    //    Debug.Log("ItemComponent: " + (itemComp != null ? itemComp.data.itemName : "null"));
    //    if (itemComp == null) return;

    //    insideBasketItems.RemoveAll(x => x.obj == other.gameObject);

    //    // 调用任务统计
    //    var taskController = FindObjectOfType<ShoppingTaskController>();
    //    taskController?.OnItemMinus(itemComp);
    //}
    // ===============================
    // Trigger Enter
    // ===============================

    // Trigger Exit
    private void OnTriggerEnter(Collider other)
    {
        var itemComp = other.GetComponent<ItemComponent>();
        if (itemComp == null) return;

        GameObject obj = other.gameObject;

        Debug.Log("TriggerEnter: " + obj.name);

        // 已经确认在篮子里
        if (insideBasketItems.Exists(x => x.obj == obj))
            return;

        // 如果之前正在 Exit 确认（说明是抖动回来）
        if (pendingExitChecks.TryGetValue(obj, out var exitCo))
        {
            StopCoroutine(exitCo);
            pendingExitChecks.Remove(obj);
            return;
        }

        // 已经在 Enter 确认中
        if (pendingEnterChecks.ContainsKey(obj))
            return;

        pendingEnterChecks[obj] = StartCoroutine(ConfirmEnter(obj, itemComp));
    }


    // Trigger Enter
    private void OnTriggerExit(Collider other)
    {
        var itemComp = other.GetComponent<ItemComponent>();
        if (itemComp == null) return;

        GameObject obj = other.gameObject;

        Debug.Log("TriggerExit: " + obj.name);

        // 如果还在 Enter 确认阶段 → 说明是抖动
        if (pendingEnterChecks.TryGetValue(obj, out var enterCo))
        {
            StopCoroutine(enterCo);
            pendingEnterChecks.Remove(obj);
            return;
        }

        // 如果根本不在篮子里 → 忽略
        if (!insideBasketItems.Exists(x => x.obj == obj))
            return;

        // 已经在 Exit 确认中
        if (pendingExitChecks.ContainsKey(obj))
            return;

        pendingExitChecks[obj] = StartCoroutine(ConfirmExit(obj, itemComp));
    }

    // 延迟确认 Enter
    IEnumerator ConfirmEnter(GameObject obj, ItemComponent itemComp)
    {
        yield return new WaitForSeconds(confirmDelay);

        if (!pendingEnterChecks.ContainsKey(obj))
            yield break;

        pendingEnterChecks.Remove(obj);

        if (!insideBasketItems.Exists(x => x.obj == obj))
        {
            insideBasketItems.Add(new BasketItemInfo(obj, transform));

            Debug.Log("确认加入篮子: " + obj.name);
            PlayerEventSystem.Instance.RecordEnterZone(obj.transform.gameObject, "ShoppingCart");

            var taskController = FindObjectOfType<ShoppingTaskController>();
            taskController?.OnItemAdded(itemComp);
        }
    }


    // 延迟确认 Exit
    IEnumerator ConfirmExit(GameObject obj, ItemComponent itemComp)
    {
        yield return new WaitForSeconds(confirmDelay);

        if (!pendingExitChecks.ContainsKey(obj))
            yield break;

        pendingExitChecks.Remove(obj);

        if (insideBasketItems.Exists(x => x.obj == obj))
        {
            insideBasketItems.RemoveAll(x => x.obj == obj);

            Debug.Log("确认移出篮子: " + obj.name);
            PlayerEventSystem.Instance.RecordExitZone(obj.transform.gameObject, "ShoppingCart");

            var taskController = FindObjectOfType<ShoppingTaskController>();
            taskController?.OnItemMinus(itemComp);
        }
    }
}

