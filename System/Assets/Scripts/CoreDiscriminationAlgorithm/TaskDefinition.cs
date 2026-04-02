using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerEventSystem;

[System.Serializable]
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
    //private void Start()
    private void Awake()
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
            // 显式把 string -> enum
            // 因为JsonUtility无法直接读取对应enum值，只能读取string然后我们自己对应
            if (!System.Enum.TryParse(e.StringType, out e.Type))
            {
                Debug.LogError($"Unknown EventType string: {e.StringType}");
                continue;
            }
            Debug.Log($"RAW Type = {e.StringType}, Target = {e.Target}");
            // 统一默认时间
            e.Time = 0f;                 
            // 防 null
            e.Context ??= "";            
            StandardSequence.Add(e);
        }
    }
}
