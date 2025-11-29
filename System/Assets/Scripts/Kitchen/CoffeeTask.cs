using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoffeeTask : MonoBehaviour, ITask
{
    public string TaskName => "将咖啡倒入杯子中";
    public bool IsTaskComplete { get; private set; }

    [Header("水体制作")]
    // 水体粒子
    public ParticleSystem pourParticles;
    // 倾倒角度
    public float pourAngleThreshold = 60f;
    // 倾倒点
    public Transform pourPoint;
    // 杯子标签
    public string cupTag = "Cup";

    [Header("倾倒相关设置")]
    // 倾倒速率
    public float fillRate = 0.2f;
    // 最大倾倒比例
    public float maxFill = 1.0f;
    [Range(0f, 1f)]
    // 目前倾倒比例
    public float currentFill = 0f;
    private bool isPouring = false;
    private bool isPouringIntoCup = false;

    // 倾倒比例字体
    public TMP_Text fillPercentageText;
    private bool isDisplayingProgress = false;

    public void InitializeTask()
    {
        IsTaskComplete = false;
        if (fillPercentageText != null)
        {
            // 显示倾倒比例UI面板
            fillPercentageText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // 计算倾倒角
        float angle = Vector3.Angle(transform.up, Vector3.up);

        // 当angle >= 倾倒角阈值时开始倾倒
        if (angle >= pourAngleThreshold)
        {
            if (!pourParticles.isEmitting)
                pourParticles.Play();
            isPouring = true;
        }
        else
        {
            if (pourParticles.isEmitting)
                pourParticles.Stop();
            isPouring = false;
        }

        // 判断是否倾倒进了杯子中
        if (isPouring && isPouringIntoCup)
        {
            currentFill += fillRate * Time.deltaTime;
            currentFill = Mathf.Clamp01(currentFill);

            // 展示杯子上倾倒比例的UI
            if (!isDisplayingProgress && fillPercentageText != null)
            {
                fillPercentageText.gameObject.SetActive(true); 
                isDisplayingProgress = true;
            }
            fillPercentageText.text = $"倒水进度: {(currentFill * 100f):F1}%";

        }
        if (currentFill >= 1f)
        {
            IsTaskComplete = true;
            Debug.Log("倒满啦");
        }
    }

    // 与杯子的碰撞盒进行检测
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(cupTag) && isPouring)
        {
            if (!isPouringIntoCup)
            {
                Debug.Log("开始倒水");
            }
            isPouringIntoCup = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(cupTag))
        {
            if (isPouringIntoCup)
            {
                Debug.Log("停止倒水");
            } 
            isPouringIntoCup = false;

            // 隐藏倒水进度
            if (fillPercentageText != null)
            {
                StartCoroutine(HideProgressUI());
            }
        }
        
    }

    
    // 是否正在倒水进杯子
    public bool IsPouringIntoCup()
    {
        return isPouringIntoCup;
    }

    // 倒水进度
    public float GetProgress()
    {
        return currentFill;
    }

    // 隐藏倒水进度UI
    private IEnumerator HideProgressUI()
    {
        // 延迟2s隐藏倒水进度
        yield return new WaitForSeconds(2f);
        if (fillPercentageText != null)
        {
            // 隐藏UI
            fillPercentageText.gameObject.SetActive(false);
            isDisplayingProgress = false;
        }
    }
}
