using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerEventSystem;

public class EventMatchResult : MonoBehaviour
{
    public PlayerEvent ActualEvent;
    public int MatchedStandardIndex; // -1 = ûƥ�䵽

    public EventMatchResult(PlayerEvent actual, int matchedIndex)
    {
        ActualEvent = actual;
        MatchedStandardIndex = matchedIndex;
    }
}
