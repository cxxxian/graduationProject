using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CabinetController : MonoBehaviour
{   
    // 冰箱Animator
    public Animator animator;
    // 是否开门
    private bool isOpen = false;

    public void ToggleDoor()
    {
        isOpen = !isOpen;

        if (animator != null)
        {
            if (isOpen)
            {
                PlayerEventSystem.Instance.RecordOpen("柜子");
                animator.SetTrigger("Open");
            }
            else
            {
                PlayerEventSystem.Instance.RecordClose("柜子");
                animator.SetTrigger("Close");
            }
                
        }
    }
}
