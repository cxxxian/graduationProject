using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageSwitcher : MonoBehaviour
{
    [Header("UI 面板")]
    public GameObject[] pages;
    public Button nextButton;
    public Button prevButton;

    [Header("动画设置")]
    // Animation 组件
    public Animation handleAnimation;
    // 平滑过渡时间
    private float crossFadeTime = 0.5f;

    [Header("高亮设置")]
    // 高亮颜色
    public Color highlightColor = Color.yellow;
    // 每秒闪烁次数
    public float highlightFrequency = 2f;
    // 高亮淡入淡出时间
    public float highlightFadeTime = 0.3f;

    // 每页高亮对象
    [System.Serializable]
    public class HighlightPage
    {   
        // 维护每页需要高亮的多个对象
        public GameObject[] objectsToHighlight;
    }

    public HighlightPage[] highlightPerPage;


    // 保存原始颜色
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
    // 字典保存闪烁协程，便于停止
    private Dictionary<Renderer, Coroutine> flashCoroutines = new Dictionary<Renderer, Coroutine>();

    private int currentIndex = 0;

    void Start()
    {
        ShowPage(0, true);

        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PrevPage);

    }

    void ShowPage(int index, bool forward)
    {
        // 隐藏所有 UI 页面
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == index);
        }
            

        currentIndex = index;

        prevButton.interactable = (currentIndex > 0);
        nextButton.interactable = (currentIndex < pages.Length - 1);

        // 播放对应动画
        PlayAnimationState(index, forward);

        // 更新高亮对应对象
        if (highlightPerPage != null && index < highlightPerPage.Length)
        {
            UpdateHighlight(highlightPerPage[index].objectsToHighlight);
        }
    }
    void UpdateHighlight(GameObject[] newHighlights)
    {
        // 停止之前所有闪烁
        foreach (var kvp in flashCoroutines)
        {
            if (kvp.Value != null)
            {
                StopCoroutine(kvp.Value);
            }
                
        }
        // 清空字典，准备存储新页面的协程。
        flashCoroutines.Clear();

        // 恢复原色
        foreach (var kvp in originalColors)
        {
            kvp.Key.material.color = kvp.Value;
        }
        originalColors.Clear();

        if (newHighlights == null) 
        {
            return;
        }

        // 遍历所有需要高亮的对象
        foreach (var obj in newHighlights)
        {
            if (obj == null)
            {
                continue;
            } 
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null)
            {
                originalColors[rend] = rend.material.color;
                // 启动闪烁协程，让该物体颜色循环在原色和高亮色之间。
                Coroutine c = StartCoroutine(FlashColor(rend, originalColors[rend], highlightColor, highlightFrequency));
                flashCoroutines[rend] = c;
            }
        }
    }

    // 闪烁协程，通过不断改变材质颜色实现闪烁效果。
    IEnumerator FlashColor(Renderer rend, Color original, Color target, float frequency)
    {
        // 实例化材质，克隆材质实例，修改颜色不会影响其他共享材质的对象。
        Material mat = rend.material;
        // 计时器，计算颜色插值。
        float t = 0f;

        // 无限循环，使闪烁持续生效，直到协程被停止。
        while (true)
        {
            t += Time.deltaTime * frequency;
            // Mathf.PingPong 返回 在 0~1 之间往返循环的值。
            float lerp = Mathf.PingPong(t, 1f);
            mat.color = Color.Lerp(original, target, lerp);
            yield return null;
        }
    }

    void PlayAnimationState(int index, bool forward)
    {
        if (handleAnimation == null) return;

        string stateName = "Pose" + index;

        // 获取动画剪辑的长度
        AnimationState animState = handleAnimation[stateName];
        if (animState == null) return;

        // 播放
        handleAnimation[stateName].speed = 1f;
        animState.time = 0f; // 确保从头开始

        // 使用 CrossFade 平滑过渡
        handleAnimation.CrossFade(stateName, crossFadeTime);
    }

    public void NextPage()
    {
        if (currentIndex < pages.Length - 1)
        {
            ShowPage(currentIndex + 1, true);
        }
    }

    public void PrevPage()
    {
        if (currentIndex > 0)
        {
            ShowPage(currentIndex - 1, false);
        }
    }
}
