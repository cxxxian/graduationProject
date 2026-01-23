using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ProgressController : MonoBehaviour
{
    [Header("UI 配置")]
    // 所有小任务面板
    public GameObject[] taskPages;
    // 空白页面（放置next/again按钮）
    public GameObject emptyPage;
    // 任务未完成提示面板
    public GameObject uncompletedPage;
    // 任务未完成提示面板
    public GameObject completedPage;
    // 每个任务面板显示的时长
    public float showPageDuration = 5f;
    // 未完成面板显示时长
    public float uncompletedShowDuration = 5f;

    public LanternStickAssembler snapper;

    [Header("按钮")]
    public Button againButton;
    public Button nextButton;

    // 当前任务面板索引
    private int currentTaskIndex = 0;
    // 计时协程，用于停止计时
    private Coroutine showPageCoroutine;
    // 是否处于空白页面
    private bool isOnEmptyPage = false;
    // 标记是否正在处理未完成流程
    private bool isProcessingUncompleted = false; 

    private void Start()
    {
        // 初始化检查
        if (taskPages == null || taskPages.Length == 0)
        {
            Debug.LogWarning("任务面板数组为空，请检查赋值！");
            return;
        }
        if (emptyPage == null)
        {
            Debug.LogWarning("空白页面未赋值，请检查！");
            return;
        }
        if (uncompletedPage == null)
        {
            Debug.LogWarning("任务未完成面板未赋值，请检查！");
            return;
        }
        if (completedPage == null)
        {
            Debug.LogWarning("任务完成面板未赋值，请检查！");
            return;
        }

        // 初始化页面状态：隐藏所有页面
        InitAllPages();
        ShowCurrentTaskPage();

        // 绑定按钮事件
        if (againButton != null)
        {
            againButton.onClick.AddListener(OnAgainButtonClick);
        }
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClick);
        }

        // 启动第一个任务面板的计时
        StartShowPageTimer();
    }

    // 初始化所有页面状态：隐藏所有任务面板、空白页、未完成面板
    private void InitAllPages()
    {
        // 隐藏所有任务面板
        foreach (var page in taskPages)
        {
            if (page != null) page.SetActive(false);
        }
        // 隐藏空白页
        if (emptyPage != null) emptyPage.SetActive(false);
        // 隐藏未完成面板
        if (uncompletedPage != null) uncompletedPage.SetActive(false);
        // 隐藏完成面板
        if (completedPage != null) completedPage.SetActive(false);

        isOnEmptyPage = false;
    }

    // 显示当前索引的任务面板
    private void ShowCurrentTaskPage()
    {
        // 先隐藏所有页面
        InitAllPages();
        // 显示当前任务面板
        if (taskPages[currentTaskIndex] != null)
        {
            taskPages[currentTaskIndex].SetActive(true);
        }
        isOnEmptyPage = false;
        isProcessingUncompleted = false; // 重置未完成流程标记
    }

    // 切换到空白页面
    private void SwitchToEmptyPage()
    {
        // 停止计时
        if (showPageCoroutine != null)
        {
            StopCoroutine(showPageCoroutine);
            showPageCoroutine = null;
        }

        // 隐藏所有任务面板，显示空白页
        InitAllPages();
        emptyPage.SetActive(true);
        isOnEmptyPage = true;
        isProcessingUncompleted = false; // 重置未完成流程标记
    }

    // 切换到下一个任务面板
    private void SwitchToNextTaskPage()
    {
        // 停止当前计时
        if (showPageCoroutine != null)
        {
            StopCoroutine(showPageCoroutine);
            showPageCoroutine = null;
        }

        // 切换索引
        currentTaskIndex++;
        // 边界判断：所有任务完成
        if (currentTaskIndex >= taskPages.Length)
        {
            Debug.Log("所有任务已完成！");
            emptyPage.SetActive(false);
            completedPage.SetActive(true);
            return;
        }

        // 显示下一个任务面板并启动计时
        ShowCurrentTaskPage();
        StartShowPageTimer();
    }

    // 启动任务面板显示计时
    private void StartShowPageTimer()
    {
        // 如果已有计时协程，先停止
        if (showPageCoroutine != null)
        {
            StopCoroutine(showPageCoroutine);
        }
        // 启动新的计时协程
        showPageCoroutine = StartCoroutine(ShowPageTimerCoroutine());
    }

    // 任务面板显示时长的协程
    private IEnumerator ShowPageTimerCoroutine()
    {
        // 等待指定时长
        yield return new WaitForSeconds(showPageDuration);

        // 前两个面板（索引0、1）直接切下一页，第三个及以后切空白页
        if (currentTaskIndex < 2)
        {
            SwitchToNextTaskPage();
        }
        else
        {
            SwitchToEmptyPage();
        }
    }

    // 处理任务未完成的流程协程：未完成面板→任务面板→空白页
    private IEnumerator ProcessUncompletedFlow()
    {
        isProcessingUncompleted = true; // 标记正在处理未完成流程
        // 显示未完成面板并等待指定时长
        InitAllPages();
        uncompletedPage.SetActive(true);
        yield return new WaitForSeconds(uncompletedShowDuration);

        // 重新显示当前任务面板并等待指定时长（复用showPageDuration）
        ShowCurrentTaskPage();
        yield return new WaitForSeconds(showPageDuration);

        // 切回空白页面
        SwitchToEmptyPage();
    }

    // Again按钮点击：重新显示当前任务面板（仅空白页生效）
    private void OnAgainButtonClick()
    {
        // 正在处理未完成流程时，不响应按钮
        if (!isOnEmptyPage || isProcessingUncompleted) return;

        // 重新显示当前任务面板
        ShowCurrentTaskPage();
        // 重新启动计时
        StartShowPageTimer();
    }

    // Next按钮点击：检查任务完成状态，处理后续逻辑
    private void OnNextButtonClick()
    {
        // 非空白页 或 正在处理未完成流程时，不响应按钮
        if (!isOnEmptyPage || isProcessingUncompleted) return;

        // 检查当前任务是否完成
        bool isTaskCompleted = CheckCurrentTaskCompletion();

        // 如果任务完成，切换到下一个任务面板
        if (isTaskCompleted)
        {
            SwitchToNextTaskPage();
        }
        else
        {
            Debug.Log("当前任务未完成！显示未完成面板，5秒后重新显示任务面板，再切回空白页");
            // 启动未完成流程协程
            StartCoroutine(ProcessUncompletedFlow());
        }
    }

    // 检查当前任务是否完成（预留函数，后期实现）
    // return是否完成
    private bool CheckCurrentTaskCompletion()
    {
        Debug.Log($"检查第 {currentTaskIndex + 1} 个任务是否完成");
        switch (currentTaskIndex)
        {
            case 2:
                return snapper.currentStage == BuildStage.DownGlue;
            case 3:
                return snapper.currentStage == BuildStage.MiddleBuild;
            case 4:
                return snapper.currentStage == BuildStage.DownPlaster;
            case 5:
                return snapper.currentStage == BuildStage.MiddleStar;
            case 6:
                return snapper.currentStage == BuildStage.Tassel;
            case 7:
                return snapper.currentStage == BuildStage.Finished;
            default:
                return false;
        }
    }

    // 防止场景切换/对象销毁时协程残留
    private void OnDestroy()
    {
        if (showPageCoroutine != null)
        {
            StopCoroutine(showPageCoroutine);
        }
        // 停止未完成流程协程（如果正在执行）
        StopAllCoroutines();
    }
}