using UnityEngine;

public class ShoppingTaskController : MonoBehaviour
{

    [Header("基础任务目标数量")]
    public int targetMilkOriginal = 2;
    public int targetBiscuitNormal = 1;
    public int targetPencilHB = 3;

    [Header("预算限制")]
    public int budgetLimit = 40;

    [Header("当前进度")]
    public int curMilkOriginal = 0;
    public int curBiscuitNormal = 0;
    public int curPencilHB = 0;
    public int curYogurtStrawberry = 0;

    // 当前的总价格
    public int totalPrice = 0;

    // 是否完成任务
    public bool isTaskCompleted = false;
    // 任务完成结算文字，返回给ShoppingCart调用
    public string taskCompletedText;

    // 当物体加入篮子调用
    public void OnItemAdded(ItemComponent itemComp)
    {
        if (isTaskCompleted) 
        {
            return;
        }
            

        var data = itemComp.data;

        // 根据类型判断数量
        switch (data.itemType)
        {
            case ItemType.Milk:
                if (data.milkType == MilkType.Original) 
                {
                    Debug.Log("milk Enter");
                    curMilkOriginal++;
                }
                else if (data.milkType == MilkType.StrawberryYogurt)
                {
                    Debug.Log("yogurt Enter");
                    curYogurtStrawberry++;
                }
                break;
            case ItemType.Biscuit:
                if (data.biscuitType == BiscuitType.Normal)
                {
                    curBiscuitNormal++;
                }
                break;
            case ItemType.Pencil:
                if (data.pencilType == PencilType.HB) 
                {
                    curPencilHB++;
                }
                break;
        }

        // 累加总价
        totalPrice += data.price;

        Debug.Log($"加入物品: {data.itemName}，总价: {totalPrice}");

    }

    public void OnItemMinus(ItemComponent itemComp)
    {
        if (isTaskCompleted)
        {
            return;
        }


        var data = itemComp.data;

        // 根据类型判断数量
        switch (data.itemType)
        {
            case ItemType.Milk:
                if (data.milkType == MilkType.Original)
                {
                    Debug.Log("milk Exist");
                    curMilkOriginal--;
                }
                else if (data.milkType == MilkType.StrawberryYogurt)
                {
                    Debug.Log("yogurt Exist");
                    curYogurtStrawberry--;
                }
                break;
            case ItemType.Biscuit:
                if (data.biscuitType == BiscuitType.Normal)
                {
                    curBiscuitNormal--;
                }
                break;
            case ItemType.Pencil:
                if (data.pencilType == PencilType.HB)
                {
                    curPencilHB--;
                }
                break;
        }

        // 累加总价
        totalPrice -= data.price;

        Debug.Log($"移除物品: {data.itemName}，总价: {totalPrice}");

    }
    public void Settle()
    {
        Debug.Log("点击了结算按钮，开始检查任务…");

        bool basic = CheckBasicTask();
        bool budget = CheckBudgetTask();
        bool rule = CheckRuleTask();

        if (basic && !budget)
        {
            isTaskCompleted = true;
            taskCompletedText = "基础任务完成！";
            Debug.Log("基础任务完成！");
        }
        else if (basic && budget)
        {
            isTaskCompleted = true;
            taskCompletedText = "预算任务完成！";
            Debug.Log("预算任务完成！");
        }
        else if (rule)
        {
            isTaskCompleted = true;
            taskCompletedText = "挑战任务（酸奶规则）完成！";
            Debug.Log("挑战任务（酸奶规则）完成！");
        }
        else
        {
            taskCompletedText = "任务未完成，请检查条件。";
            Debug.Log("任务未完成，请检查条件。");
        }
    }

    private bool CheckBasicTask()
    {
        bool ok =
            curMilkOriginal >= targetMilkOriginal &&
            curBiscuitNormal >= targetBiscuitNormal &&
            curPencilHB >= targetPencilHB;
        return ok;
    }
    private bool CheckBudgetTask()
    {
        bool ok = totalPrice <= budgetLimit;
        return ok;
    }
    private bool CheckRuleTask()
    {
        // 买了酸奶 -> 必须减少一包饼干
        bool ok = (curBiscuitNormal >= (targetBiscuitNormal - 1)) && 
            (totalPrice <= budgetLimit) && 
            (curMilkOriginal >= targetMilkOriginal) && 
            (curPencilHB >= targetPencilHB);
        return ok;
    }



    // 重置任务
    public void ResetTask()
    {
        curMilkOriginal = 0;
        curBiscuitNormal = 0;
        curPencilHB = 0;
        totalPrice = 0;
        isTaskCompleted = false;
    }
}
