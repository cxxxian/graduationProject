using System.Collections;
using UnityEngine;

public class LanternStickAssembler : MonoBehaviour
{

    [Header("玩家当前手中的竹条")]
    public Transform currentStick;

    [Header("下 / 中 / 上 三层木棍插槽层")]
    public SlotLayer downStickLayer;
    public SlotLayer middleStickLayer;
    public SlotLayer upStickLayer;

    [Header("下 / 中 胶水层")]
    public GluePoint[] downGlueLayer;
    public GluePoint[] middleGlueLayer;

    [Header("下 / 中 / 上 三层贴纸插槽层")]
    public SlotLayer downPlasterLayer;
    public SlotLayer middlePlasterLayer;
    public SlotLayer upPlasterLayer;

    [Header("Star贴纸插槽层")]
    public SlotLayer middleStarLayer;

    [Header("Tassel插槽层")]
    public SlotLayer tasselLayer;

    [Header("吸附判定距离")]
    public float snapDistance = 0.1f;

    [Header("透明插槽显示距离")]
    public float showGhostDistance = 0.25f;

    [Header("吸附速度")]
    public float snapSpeed = 10f;

    [Header("插入后使用的实体材质")]
    public Material stickMaterial;
    public Material plasterMaterial;
    public Material starMaterial;

    public BuildStage currentStage = BuildStage.DownBuild;
    private bool isSnapping = false;

    private void Start()
    {
        // 木棍层
        InitLayer(downStickLayer);
        InitLayer(middleStickLayer);
        InitLayer(upStickLayer);

        // 贴纸层
        InitLayer(downPlasterLayer);
        InitLayer(middlePlasterLayer);
        InitLayer(upPlasterLayer);
        InitLayer(middleStarLayer);
        InitLayer(tasselLayer);

        currentStage = BuildStage.DownBuild;
    }

    private void Update()
    {
        if (IsGlueStage())
        {
            return;
        }

        if (currentStick == null || !currentStick.gameObject.activeSelf)
            return;

        if (isSnapping)
            return;

        SlotLayer layer = GetCurrentLayer();
        if (layer == null)
            return;

        int nearestSlot = -1;
        float nearestDist = float.MaxValue;

        // 找最近的未填充插槽 
        for (int i = 0; i < layer.ghostSlots.Length; i++)
        {
            if (layer.filled[i]) continue;

            float d = Vector3.Distance(
                currentStick.position,
                layer.ghostSlots[i].position
            );

            if (d < nearestDist)
            {
                nearestDist = d;
                nearestSlot = i;
            }
        }

        if (nearestSlot == -1)
            return;

        // 显示 / 隐藏 ghost
        if (nearestDist < showGhostDistance)
            ShowOnlyGhost(layer, nearestSlot);
        else
        {
            HideAllGhosts(layer);
            return;
        }

        // 吸附
        if (nearestDist < snapDistance)
        {
            if((layer == downStickLayer || layer == upStickLayer) && currentStick.CompareTag("ShortStick"))
            {
                StartCoroutine(SnapStickToSlot(layer, nearestSlot));
            }
            else if(layer == middleStickLayer && currentStick.CompareTag("LongStick"))
            {
                StartCoroutine(SnapStickToSlot(layer, nearestSlot));
            }
            else if ((layer == downPlasterLayer || layer == upPlasterLayer) && currentStick.CompareTag("TrianglePlaster"))
            {
                StartCoroutine(SnapStickToSlot(layer, nearestSlot));
                Debug.Log("吸附TrianglePlaster");
            }
            else if (layer == middlePlasterLayer && currentStick.CompareTag("QuadPlaster"))
            {
                StartCoroutine(SnapStickToSlot(layer, nearestSlot));
                Debug.Log("吸附QuadPlaster");
            }
            else if (layer == middleStarLayer && currentStick.CompareTag("Star"))
            {
                StartCoroutine(SnapStickToSlot(layer, nearestSlot));
                Debug.Log("吸附Star");
            }
            else if (layer == tasselLayer)
            {
                StartCoroutine(SnapStickToSlot(layer, nearestSlot));
                Debug.Log("吸附Tassel");
            }
            else
            {
                Debug.Log("木棍 / 贴纸匹配错误");
            }
        }
    }
    IEnumerator SnapStickToSlot(SlotLayer layer, int index)
    {
        isSnapping = true;
        layer.filled[index] = true;

        Transform slot = layer.ghostSlots[index];

        // 吸附动画
        Vector3 startPos = currentStick.position;
        Quaternion startRot = currentStick.rotation;

        Vector3 targetPos = slot.position;
        Quaternion targetRot = slot.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * snapSpeed;
            currentStick.position = Vector3.Lerp(startPos, targetPos, t);
            currentStick.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        // 手中木棍消失
        currentStick.gameObject.SetActive(false);

        // ghost → 实体
        Renderer[] renderers = slot.GetComponentsInChildren<Renderer>(true);
        if (IsStickStage())
        {
            foreach (Renderer r in renderers)
                r.material = stickMaterial;
        }
        else if (IsPlasterStage())
        {
            foreach (Renderer r in renderers)
                r.material = plasterMaterial;
        }
        else if (IsStarStage())
        {
            foreach (Renderer r in renderers)
                r.material = starMaterial;
        }
        else if (IsTasselStage())
        {
            foreach (Renderer r in renderers)
                r.material = plasterMaterial;
        }


        slot.gameObject.SetActive(true);

        HideAllGhosts(layer);

        // 检查是否完成该层
        if (IsLayerStructureFinished(layer) && 
            (currentStage != BuildStage.UpBuild && 
            currentStage != BuildStage.DownPlaster && 
            currentStage != BuildStage.MiddlePlaster && 
            currentStage != BuildStage.UpPlaster && 
            currentStage != BuildStage.MiddleStar &&
            currentStage != BuildStage.Tassel))
        {
            EnterGlueStage();
        }
        else if(IsLayerStructureFinished(layer) && 
            (currentStage == BuildStage.UpBuild || 
            currentStage == BuildStage.DownPlaster || 
            currentStage == BuildStage.MiddlePlaster || 
            currentStage == BuildStage.UpPlaster ||
            currentStage == BuildStage.MiddleStar ||
            currentStage == BuildStage.Tassel))
        {
            EnterPlasterStage();
        }
        isSnapping = false;
    }
    void EnterGlueStage()
    {
        if (currentStage == BuildStage.DownBuild)
        {
            currentStage = BuildStage.DownGlue;
            Debug.Log("进入 DownGlue 阶段");
        }
        else if (currentStage == BuildStage.MiddleBuild)
        {
            currentStage = BuildStage.MiddleGlue;
            Debug.Log("进入 MiddleGlue 阶段");
        }

    }
    void EnterPlasterStage()
    {
        if (currentStage == BuildStage.UpBuild)
        {
            currentStage = BuildStage.DownPlaster;
            Debug.Log("进入 DownPlaster 阶段");
        }
        else if (currentStage == BuildStage.DownPlaster)
        {
            currentStage = BuildStage.MiddlePlaster;
            Debug.Log("进入 MiddlePlaster 阶段");
        }
        else if (currentStage == BuildStage.MiddlePlaster)
        {
            currentStage = BuildStage.UpPlaster;
            Debug.Log("进入 UpPlaster 阶段");
        }
        else if (currentStage == BuildStage.UpPlaster)
        {
            currentStage = BuildStage.MiddleStar;
            Debug.Log("进入 MiddleStar 阶段");
        }
        else if (currentStage == BuildStage.MiddleStar)
        {
            currentStage = BuildStage.Tassel;
            Debug.Log("进入 Tassel 阶段");
        }
        else if (currentStage == BuildStage.Tassel)
        {
            currentStage = BuildStage.Finished;
            Debug.Log("进入 Finished 阶段");
        }
    }
    public void OnGlueLayerFinished()
    {
        if (currentStage == BuildStage.DownGlue)
        {
            currentStage = BuildStage.MiddleBuild;
            Debug.Log("进入 MiddleBuild");
        }
        else if (currentStage == BuildStage.MiddleGlue)
        {
            currentStage = BuildStage.UpBuild;
            Debug.Log("进入 UpBuild");
        }
    }

    SlotLayer GetCurrentLayer()
    {
        switch (currentStage)
        {
            case BuildStage.DownBuild: return downStickLayer;
            case BuildStage.MiddleBuild: return middleStickLayer;
            case BuildStage.UpBuild: return upStickLayer;
            case BuildStage.DownPlaster: return downPlasterLayer;
            case BuildStage.MiddlePlaster: return middlePlasterLayer;
            case BuildStage.UpPlaster: return upPlasterLayer;
            case BuildStage.MiddleStar: return middleStarLayer;
            case BuildStage.Tassel: return tasselLayer;
            default: return null;
        }
    }

    bool IsLayerStructureFinished(SlotLayer layer)
    {
        foreach (bool f in layer.filled)
            if (!f) return false;
        return true;
    }

    void ShowOnlyGhost(SlotLayer layer, int index)
    {
        for (int i = 0; i < layer.ghostSlots.Length; i++)
        {
            if (layer.filled[i]) continue;
            layer.ghostSlots[i].gameObject.SetActive(i == index);
        }
    }

    void HideAllGhosts(SlotLayer layer)
    {
        for (int i = 0; i < layer.ghostSlots.Length; i++)
        {
            if (!layer.filled[i])
                layer.ghostSlots[i].gameObject.SetActive(false);
        }
    }

    void InitLayer(SlotLayer layer)
    {
        layer.filled = new bool[layer.ghostSlots.Length];
        foreach (var g in layer.ghostSlots)
            g.gameObject.SetActive(false);
    }

    public void SetCurrentStick(Transform stick)
    {
        currentStick = stick;
    }

    public void ClearCurrentStick(Transform stick)
    {
        if (currentStick == stick)
            currentStick = null;
    }

    //是否为搭建木棍阶段
    public bool IsStickStage()
    {
        return currentStage == BuildStage.DownBuild 
            || currentStage == BuildStage.MiddleBuild 
            || currentStage == BuildStage.UpBuild;
    }

    //是否为胶水涂抹阶段
    public bool IsGlueStage()
    {
        return currentStage == BuildStage.DownGlue
            || currentStage == BuildStage.MiddleGlue;
    }

    //是否为Plaster阶段
    public bool IsPlasterStage()
    {
        return currentStage == BuildStage.DownPlaster
            || currentStage == BuildStage.MiddlePlaster
            || currentStage == BuildStage.UpPlaster;
    }

    //是否为Star阶段
    public bool IsStarStage()
    {
        return currentStage == BuildStage.MiddleStar;
    }

    //是否为Tassel阶段
    public bool IsTasselStage()
    {
        return currentStage == BuildStage.Tassel;
    }
}

// 数据结构
public enum BuildStage
{
    // 底层拼接
    DownBuild,
    // 底层上胶
    DownGlue,
    MiddleBuild,
    MiddleGlue,
    UpBuild,
    DownPlaster,
    MiddlePlaster,
    UpPlaster,
    MiddleStar,
    Tassel,
    Finished
}

[System.Serializable]
public class SlotLayer
{
    public Transform[] ghostSlots;
    [HideInInspector] public bool[] filled;
}