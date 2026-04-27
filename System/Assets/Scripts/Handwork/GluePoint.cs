using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GluePoint : MonoBehaviour
{
    public bool glued = false;
    public LanternStickAssembler snapper;

    [Header("上胶后的材质（不透明）")]
    public Material gluedMaterial;

    private void OnTriggerEnter(Collider other)
    {
        if (snapper.IsStickStage() || snapper.IsPlasterStage())
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

        // 切换材质为不透明
        if (gluedMaterial != null)
        {
            Renderer renderer = GetComponentInChildren<Renderer>(true);
            if (renderer != null)
            {
                Debug.Log($"{name} 切换材质 → {gluedMaterial.name}");
                renderer.material = gluedMaterial;
            }
            else
            {
                Debug.LogWarning($"{name} 未找到 Renderer！请在 GluePoint 上添加 MeshRenderer 组件");
            }
        }
        else
        {
            Debug.LogWarning($"{name} gluedMaterial 未赋值！");
        }
    }
}
