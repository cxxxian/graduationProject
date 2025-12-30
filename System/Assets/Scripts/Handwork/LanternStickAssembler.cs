using System.Collections;
using UnityEngine;



public class LanternSlotSnapper : MonoBehaviour
{
    [Header("玩家当前手中的竹条")]
    public Transform currentStick;

    [Header("下 / 中 / 上 三层插槽")]
    public SlotLayer downLayer;
    public SlotLayer middleLayer;
    public SlotLayer upLayer;

    [Header("吸附判定距离")]
    public float snapDistance = 0.1f;

    [Header("透明插槽显示距离")]
    public float showGhostDistance = 0.25f;

    [Header("吸附速度")]
    public float snapSpeed = 10f;

    [Header("插入后使用的实体材质")]
    public Material solidMaterial;

    private LanternStage currentStage = LanternStage.Down;
    private bool isSnapping = false;

    private void Start()
    {
        InitLayer(downLayer);
        InitLayer(middleLayer);
        InitLayer(upLayer);

        currentStage = LanternStage.Down;
    }

    private void Update()
    {
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
            StartCoroutine(SnapStickToSlot(layer, nearestSlot));
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
        foreach (Renderer r in renderers)
            r.material = solidMaterial;

        slot.gameObject.SetActive(true);

        HideAllGhosts(layer);

        // 检查是否完成该层
        if (IsLayerFinished(layer))
            AdvanceStage();

        isSnapping = false;
    }

    void AdvanceStage()
    {
        if (currentStage == LanternStage.Down)
            currentStage = LanternStage.Middle;
        else if (currentStage == LanternStage.Middle)
            currentStage = LanternStage.Up;
        else if (currentStage == LanternStage.Up)
            currentStage = LanternStage.Finished;

        Debug.Log("进入阶段：" + currentStage);
    }

    SlotLayer GetCurrentLayer()
    {
        switch (currentStage)
        {
            case LanternStage.Down: return downLayer;
            case LanternStage.Middle: return middleLayer;
            case LanternStage.Up: return upLayer;
            default: return null;
        }
    }

    bool IsLayerFinished(SlotLayer layer)
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
}

// 数据结构
public enum LanternStage
{
    Down,
    Middle,
    Up,
    Finished
}

[System.Serializable]
public class SlotLayer
{
    public Transform[] ghostSlots;
    [HideInInspector] public bool[] filled;
}

