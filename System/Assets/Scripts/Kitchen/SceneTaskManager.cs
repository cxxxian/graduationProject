using Pico.Platform;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.PICO.TOBSupport;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTaskManager : MonoBehaviour
{
    [Header("是否自动开始游戏（训练关卡开启）")]
    public bool autoStartGame = false;

    [Header("任务管理List")]
    private List<ITask> tasks = new List<ITask>();

    private bool allComplete = false;

    [Header("UIManager")]
    public TaskUIManager uiManager;

    [Header("介绍音频")]
    // 音频Source
    public AudioSource introAudio;
    // 延迟播放秒数
    public float introDelay = 3f;
    // 游戏是否开始
    private bool gameStarted = false;

    [Header("开始游戏后要隐藏的 UI 面板")]
    public GameObject taskUIPanel;
    public GameObject nextLevelToggle;
    public GameObject backHomeToggle;
    public GameObject startButton;
    public GameObject againButton;

    [Header("所有任务完成后显示的结算按钮")]
    public GameObject settleButton;

    [Header("任务完成检测")]
    public TaskEvaluator evaluator;

    [Header("阶段完成音效")]
    public AudioSource stageAudioSource;

    [Header("关卡完成音效")]
    public AudioSource levelAudioSource;

    void Start()
    {
        // 延迟播放介绍音频
        StartCoroutine(PlayIntroAfterDelay());

        // 训练关卡直接开始游戏无音频
        if (autoStartGame)
        {
            StartGame();
        }
    }

    IEnumerator PlayIntroAfterDelay()
    {
        yield return new WaitForSeconds(introDelay);
        PlayIntroAudio();
    }

    // 播放介绍音频（绑定重新播放按钮）
    public void PlayIntroAudio()
    {
        if (introAudio != null)
        {
            introAudio.Stop();
            introAudio.Play();
        }
    }

    // 开始游戏（绑定开始按钮）
    public void StartGame()
    {
        if (gameStarted) return;

        gameStarted = true;
        //HighlightService.StartRecord();
        if (introAudio != null)
        {
            introAudio.Stop();
        }
        // 隐藏 UI 面板
        if (taskUIPanel != null)
        {
            taskUIPanel.SetActive(false);
            startButton.SetActive(false);
            againButton.SetActive(false);
}

        CollectTasks();
        // 开始计时
        //Timer.Instance.StartTimer();
    }

    //public void ReStart()
    //{
    //    SceneManager.LoadScene("KitchenTaskScene");
    //}
    public void NextLevel()
    {
        SceneManager.LoadScene("ShoppingPracticeScene");
    }
    public void BackHome()
    {
        TaskSessionManager.Instance = null;
        SceneManager.LoadScene("StartScene");
    }

    private void CollectTasks()
    {
        tasks.Clear();

        // 自动收集场景中继承自 ITask 的任务
        var allTaskComponents = FindObjectsOfType<MonoBehaviour>();
        foreach (var comp in allTaskComponents)
        {
            if (comp is ITask task)
            {
                task.InitializeTask();
                tasks.Add(task);
                Debug.Log($"任务: {task.TaskName}");
            }
        }

        Debug.Log($"已收集到场景中 {tasks.Count} 个任务");
    }

    void Update()
    {   
        // 游戏未开始或者结束都不需要判断
        if (!gameStarted) return;
        if (allComplete) return;

        bool allDone = true;

        foreach (var task in tasks)
        {
            bool done = task.IsTaskComplete;


            // 更新是否完成UI
            if (uiManager != null)
            {
                switch (task.TaskName)
                {
                    case "把冰箱中的香蕉全部拿到桌子上":
                        uiManager.SetBananaTaskComplete(done);
                        if (done)
                        {
                            PlayStageCompleteSound();
                        }
                        break;
                    case "将咖啡倒入杯子中":
                        uiManager.SetCoffeeTaskComplete(done);
                        if (done)
                        {
                            PlayStageCompleteSound();
                        }
                        break;
                    case "拿两片吐司到盘子中":
                        uiManager.SetBreadTaskComplete(done);
                        if (done)
                        {
                            PlayStageCompleteSound();
                        }
                        break;
                    case "开启/关闭虚拟计算器":
                        uiManager.SetPanelTaskComplete(done);
                        if (done)
                        {
                            PlayStageCompleteSound();
                        }
                        break;
                    case "推动购物车到指定位置":
                        uiManager.SetPushTaskComplete(done);
                        if (done)
                        {
                            PlayStageCompleteSound();
                        }
                        break;

                }
            }

            if (!done)
                allDone = false;
        }

        if (allDone)
        {
            allComplete = true;

            // 停止计时
            Timer.Instance.StopTimer();

            // 显示任务面板和结算按钮，等待玩家关好冰箱后手动点击结算
            if (taskUIPanel != null) taskUIPanel.SetActive(true);
            if (settleButton != null) settleButton.SetActive(true);

            Debug.Log("所有任务已完成，等待玩家点击结算");
        }
    }
    void PlayStageCompleteSound()
    {
        if (stageAudioSource != null && stageAudioSource.clip != null)
            stageAudioSource.Play();
    }

    void PlayLevelCompleteSound()
    {
        if (levelAudioSource != null && levelAudioSource.clip != null)
            levelAudioSource.Play();
    }

    // 结算按钮点击（绑定到结算按钮）
    public void OnSettleButtonClick()
    {
        if (settleButton != null) settleButton.SetActive(false);
        PlayLevelCompleteSound();
        OnAllTasksComplete();
    }

    private void OnAllTasksComplete()
    {
        Debug.Log("开始结算");

        // 结算完成后显示下一关/回主页按钮
        if (nextLevelToggle != null) nextLevelToggle.SetActive(true);
        if (backHomeToggle != null) backHomeToggle.SetActive(true);
        PlayerEventSystem.Instance.PrintAllLogs();

        // 进行行为判定
        evaluator.EvaluateKitchen();
        evaluator.gameCompletionRate = 1.0f;

        // 收集该场景的总结
        var result = evaluator.GetResultSummary();
        TaskSessionManager.Instance.AddTaskResult(result);
        // 清空准备进入下一个场景
        PlayerEventSystem.Instance.Clear();

        // 输出存储的数据结果
        foreach (var r in TaskSessionManager.Instance.AllTaskResults)
        {
            Debug.Log($"Scene: {r.SceneName}");
            Debug.Log($"Completion: {r.CompletionRate}");
            Debug.Log($"Omission: {r.OmissionCount}");
            Debug.Log($"Commission: {r.CommissionCount}");
            Debug.Log($"Motor: {r.MotorCount}");
            Debug.Log($"ITMN: {r.ITMN}");
            Debug.Log($"PSMM: {r.PSMM}");
            Debug.Log($"BE: {r.BE}");
        }

        // 临时测试场景连串
        //SceneManager.LoadScene("LivingRoomScene");

    }

}
