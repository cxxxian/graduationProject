using System.Collections.Generic;
using UnityEngine;

public class BasketTrigger : MonoBehaviour
{
    [Tooltip("指定用来检测物品的 Trigger Collider")]
    public Collider triggerCollider;

    public List<BasketItemInfo> insideBasketItems = new List<BasketItemInfo>();

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

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("TriggerEnter: " + other.name);
        var itemComp = other.GetComponent<ItemComponent>();
        Debug.Log("ItemComponent: " + (itemComp != null ? itemComp.data.itemName : "null"));
        if (itemComp == null) return;

        if (!insideBasketItems.Exists(x => x.obj == other.gameObject))
        {
            insideBasketItems.Add(new BasketItemInfo(other.gameObject, transform));
        }

        // 调用任务统计
        var taskController = FindObjectOfType<ShoppingTaskController>();
        taskController?.OnItemAdded(itemComp);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("TriggerExist: " + other.name);
        var itemComp = other.GetComponent<ItemComponent>();
        Debug.Log("ItemComponent: " + (itemComp != null ? itemComp.data.itemName : "null"));
        if (itemComp == null) return;

        insideBasketItems.RemoveAll(x => x.obj == other.gameObject);

        // 调用任务统计
        var taskController = FindObjectOfType<ShoppingTaskController>();
        taskController?.OnItemMinus(itemComp);
    }
}

