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

    public void InitializeTask()
    {
        IsTaskComplete = false;
        bananaIndex = 0;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Banana") && !bananaInTable.Contains(other.gameObject))
        {
            // 记录香蕉进入桌子区域
            //PlayerEventSystem.Instance.RecordEnterZone(other.transform.parent.gameObject, "Table");

            bananaIndex++;
            bananaInTable.Add(other.gameObject);
            Debug.Log($"Banana 数量{bananaInTable.Count}" + $", {bananaIndex}");
            if (bananaInTable.Count >= goalCount)
            {
                IsTaskComplete = true;
                Debug.Log("香蕉任务完成");
            }
        }
        else if (other.CompareTag("Apple") && !appleInTable.Contains(other.gameObject))
        {
            appleIndex++;
            appleInTable.Add(other.gameObject);
            Debug.Log($"Apple 数量{appleInTable.Count}" + $", {appleIndex}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Banana") && bananaInTable.Contains(other.gameObject))
        {
            // 记录香蕉离开桌子区域
            //PlayerEventSystem.Instance.RecordExitZone(other.transform.parent.gameObject, "Table");

            bananaIndex++;
            bananaInTable.Remove(other.gameObject);
            Debug.Log($"Banana 数量{bananaInTable.Count}" + $", {bananaIndex}");
        }else if (other.CompareTag("Apple") && appleInTable.Contains(other.gameObject))
        {
            appleIndex++;
            appleInTable.Remove(other.gameObject);
            Debug.Log($"Apple 数量{appleInTable.Count}" + $", {appleIndex}");
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
