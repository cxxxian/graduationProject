using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GluePoint : MonoBehaviour
{
    public bool glued = false;
    public LanternSlotSnapper snapper;

    private void OnTriggerEnter(Collider other)
    {
        if (!snapper.IsGlueStage())
        {
            return;
        }
        if (glued) return;

        if (other.CompareTag("GlueStick"))
        {
            glued = true;
            OnGlued();
        }
        // 通过这里去通知snapper的currentStage切换状态
        if (snapper.currentStage == BuildStage.DownGlue)
        {
            if (IsDownLayerGluedFinished())
            {
                snapper.OnGlueLayerFinished();
            }
        }else if(snapper.currentStage == BuildStage.MiddleGlue)
        {
            if (IsMiddleLayerGluedFinished())
            {
                snapper.OnGlueLayerFinished();
            }
        }
        
    }

    public bool IsDownLayerGluedFinished()
    {
        foreach (var p in snapper.downGlueLayer)
        {
            if (!p.glued)
                return false;
        }
        return true;
    }
    public bool IsMiddleLayerGluedFinished()
    {
        foreach (var p in snapper.middleGlueLayer)
        {
            if (!p.glued)
                return false;
        }
        return true;
    }

    void OnGlued()
    {
        Debug.Log($"{name} 上胶完成");

        // TODO:
        // 播放音效 / 特效
        // 改变颜色
        // 记录时间
    }
}
