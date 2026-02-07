using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerEventSystem;

class PlayerEventListWrapper
{
    public List<PlayerEvent> StandardSequence;
}

public class TaskDefinition : MonoBehaviour
{
    public TextAsset taskJson;

    // 预指定的标准行为序列
    public List<PlayerEvent> StandardSequence = new();
    // Start is called before the first frame update
    private void Start()
    {
        LoadFromJson();
 
        //PlayerEventSystem.Instance.PrintAllLogs(StandardSequence);
        Debug.Log(StandardSequence.Count);
    }

    void LoadFromJson()
    {
        Debug.Log(taskJson == null ? "taskJson is NULL" : "taskJson OK");
        var wrapper = JsonUtility.FromJson<PlayerEventListWrapper>(taskJson.text);

        StandardSequence.Clear();

        foreach (var e in wrapper.StandardSequence)
        {
            e.Time = 0f;                 // 统一补时间
            e.Context ??= "";            // 防 null
            StandardSequence.Add(e);
        }
    }
}
