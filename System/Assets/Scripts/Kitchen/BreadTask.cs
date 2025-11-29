using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreadTask : MonoBehaviour, ITask
{
    public string TaskName => "拿两片吐司到盘子中";
    public bool IsTaskComplete { get; private set; }
    public int goalCount = 2;

    private HashSet<GameObject> breadsInPlate = new HashSet<GameObject>();
    int index = 0;

    public void InitializeTask()
    {
        IsTaskComplete = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bread"))
        {
            index++;
            breadsInPlate.Add(other.gameObject);
            Debug.Log($"Bread 数量{breadsInPlate.Count}" + $", {index}");
            if (breadsInPlate.Count >= goalCount)
            {
                IsTaskComplete = true;
                Debug.Log("面包任务完成");

            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bread"))
        {
            index++;
            breadsInPlate.Remove(other.gameObject);
            Debug.Log($"Bread 数量{breadsInPlate.Count}" + $", {index}");
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
