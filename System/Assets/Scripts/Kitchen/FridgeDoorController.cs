using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FridgeDoorController : MonoBehaviour
{   
    // 冰箱Animator
    public Animator animator;
    // 是否开�?
    private bool isOpen = false;

    public void ToggleDoor()
    {
        isOpen = !isOpen;

        if (animator != null)
        {
            if (isOpen)
            {
                PlayerEventSystem.Instance.RecordOpen("Fridge");
                animator.SetTrigger("Open");
            }
            else
            {
                PlayerEventSystem.Instance.RecordClose("Fridge");
                animator.SetTrigger("Close");
            }
                
        }
    }
}
