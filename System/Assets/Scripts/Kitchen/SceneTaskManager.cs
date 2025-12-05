using Pico.Platform;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.PICO.TOBSupport;
using Unity.XR.PXR;
using UnityEngine;
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
        }

        CollectTasks();

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
                        break;
                    case "将咖啡倒入杯子中":
                        uiManager.SetCoffeeTaskComplete(done);
                        break;
                    case "拿两片吐司到盘子中":
                        uiManager.SetBreadTaskComplete(done);
                        break;
                    case "开启/关闭虚拟计算器":
                        uiManager.SetPanelTaskComplete(done);
                        break;
                    case "推动购物车到指定位置":
                        uiManager.SetPushTaskComplete(done);
                        break;

                }
            }

            if (!done)
                allDone = false;
        }

        if (allDone)
        {
            allComplete = true;
            OnAllTasksComplete();
        }
    }

    private void OnAllTasksComplete()
    {
        // 任务完成恢复 UI 面板
        if (taskUIPanel != null)
        {
            taskUIPanel.SetActive(true);
        }
        Debug.Log("任务完成");
    }
}
