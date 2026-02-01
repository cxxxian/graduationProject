using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerEventSystem;

public class TaskDefinition : MonoBehaviour
{
    // 预指定的标准行为序列
    public List<PlayerEvent> StandardSequence = new List<PlayerEvent>();
    // Start is called before the first frame update
    private void Start()
    {
        StandardSequence.Add(
            new PlayerEventSystem.PlayerEvent(
                PlayerEventSystem.EventType.Grab,
                "Plate",
                0
            )
        );

        StandardSequence.Add(
            new PlayerEventSystem.PlayerEvent(
                PlayerEventSystem.EventType.Drop,
                "Plate",
                0
            )
        );

        StandardSequence.Add(
            new PlayerEventSystem.PlayerEvent(
                PlayerEventSystem.EventType.Grab,
                "Bread",
                0
            )
        );

        StandardSequence.Add(
            new PlayerEventSystem.PlayerEvent(
                PlayerEventSystem.EventType.EnterZone,
                "Bread",
                0,
                "Plate"
            )
        );

        StandardSequence.Add(
            new PlayerEventSystem.PlayerEvent(
                PlayerEventSystem.EventType.Drop,
                "Bread",
                0
            )
        );

        StandardSequence.Add(
            new PlayerEventSystem.PlayerEvent(
                PlayerEventSystem.EventType.Grab,
                "Banana",
                0
            )
        );

        StandardSequence.Add(
            new PlayerEventSystem.PlayerEvent(
                PlayerEventSystem.EventType.EnterZone,
                "Banana",
                0,
                "Table"
            )
        );

        StandardSequence.Add(
            new PlayerEventSystem.PlayerEvent(
                PlayerEventSystem.EventType.Drop,
                "Banana",
                0
            )
        );

        //PlayerEventSystem.Instance.PrintAllLogs(StandardSequence);
        Debug.Log(StandardSequence.Count);
    }
}
