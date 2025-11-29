using UnityEngine;

public class BasketTrigger : MonoBehaviour
{
    [Tooltip("指定用来检测物品的 Trigger Collider")]
    public Collider triggerCollider;

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

        // 调用任务统计
        var taskController = FindObjectOfType<ShoppingTaskController>();
        taskController?.OnItemMinus(itemComp);
    }
}