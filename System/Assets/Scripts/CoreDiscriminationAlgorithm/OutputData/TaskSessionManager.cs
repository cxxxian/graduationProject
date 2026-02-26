using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskSessionManager : MonoBehaviour
{
    public static TaskSessionManager Instance;

    public List<TaskResultData> AllTaskResults = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddTaskResult(TaskResultData result)
    {
        AllTaskResults.Add(result);
    }
}
