using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushTask : MonoBehaviour, ITask
{
    public string TaskName => "推动购物车到指定位置";
    public bool IsTaskComplete { get; private set; }

    public void InitializeTask()
    {
        IsTaskComplete = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ShoppingCart"))
        {
            Debug.Log($"购物车到达指定地点");
            IsTaskComplete = true;
        }
    }

}
