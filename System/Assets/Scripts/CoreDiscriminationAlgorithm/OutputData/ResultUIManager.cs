using TMPro;
using UnityEngine;

public class ResultUIManager : MonoBehaviour
{
    public ResultPanel kitchenPanel;
    public ResultPanel shoppingPanel;
    public ResultPanel handworkPanel;

    //public TMP_Text finalScoreText;
    public TMP_Text finalLevelText;

    public TMP_Text nameText;
    public TMP_Text genderText;

    private string userName;
    private string gender;

    void Start()
    {
        ShowAllResults();
        if (InformationCollection.Instance != null)
        {
            userName = InformationCollection.Instance.userName;
            gender = InformationCollection.Instance.gender;

            nameText.text = userName;
            genderText.text = gender;
        }

    }

    void ShowAllResults()
    {
        float totalScore = 0f;
        int count = 0;
        foreach (var result in TaskSessionManager.Instance.AllTaskResults)
        {
            switch (result.SceneName)
            {
                case "KitchenTaskScene":
                    kitchenPanel.ShowResult(result);
                    break;

                case "ShoppingScene":
                    shoppingPanel.ShowResult(result);
                    break;

                case "HandworkScene":
                    handworkPanel.ShowResult(result);
                    break;
            }
            // 计算每个场景得分
            float score = CalculateScore(result);
            totalScore += score;
            count++;
        }
        // 计算总评分
        float finalScore = count > 0 ? totalScore / count : 0f;
        string finalLevel = GetLevel(finalScore);
        int riskPercent = Mathf.RoundToInt(finalScore * 100f);

        Debug.Log($"风险分：{finalScore:F2}（{riskPercent}%），风险等级：{finalLevel}");

        // 显示在UI上
        finalLevelText.text = $"{finalLevel}\n与DD组认知特征匹配度：{riskPercent}%";
    }

    float CalculateScore(TaskResultData data)
    {
        // ===== 论文基准数据（表4、表6）=====
        // TD组（正常发育，n=23）
        const float TD_Omission   = 0f;
        const float TD_Commission = 2f;
        const float TD_BE         = 13.70f;
        const float TD_PSMM       = 6.48f;
        // DD组（发育障碍，n=15，ID与ADHD均值平均）
        const float DD_Omission   = 1f;
        const float DD_Commission = 5f;
        const float DD_BE         = 26.35f; // (25.20 + 27.50) / 2
        const float DD_PSMM       = 3.80f;  // (3.11 + 4.50) / 2

        // ===== 各指标与DD组相似度（0=接近TD，1=接近DD）=====

        // 遗漏错误（p<0.001，差异极显著，权重35%）
        // DD中位数1作为50%相似度基准，2倍DD中位数为上限
        float omissionSim = Mathf.Clamp01(data.OmissionCount / (DD_Omission * 2f));

        // 执行错误（p<0.001，差异极显著，权重35%）
        // 低于TD基准时为0，达到DD中位数时为100%
        float commissionSim = Mathf.Max(0f, Mathf.Clamp01(
            (data.CommissionCount - TD_Commission) / (DD_Commission - TD_Commission)));

        // SWM BE值（正相关于错误数，权重15%）
        float beSim = Mathf.Max(0f, Mathf.Clamp01(
            (data.BE - TD_BE) / (DD_BE - TD_BE)));

        // SOC PSMM值（负相关于错误数，权重15%）
        float psmmSim = Mathf.Max(0f, Mathf.Clamp01(
            (TD_PSMM - data.PSMM) / (TD_PSMM - DD_PSMM)));

        // 运动错误：论文 p>0.05，TD与DD组无显著差异，不纳入评分

        // ===== 加权风险分（越高越接近DD组认知特征）=====
        float riskScore =
            0.35f * omissionSim   +
            0.35f * commissionSim +
            0.15f * beSim         +
            0.15f * psmmSim;

        Debug.Log($"[{data.SceneName}] 遗漏相似度:{omissionSim:F2} 执行相似度:{commissionSim:F2} BE相似度:{beSim:F2} PSMM相似度:{psmmSim:F2} → 风险分:{riskScore:F2}");
        return riskScore;
    }

    string GetLevel(float score)
    {
        // 参考论文DD组占总样本39.5%（15/38）的分布
        if (score < 0.3f)      return "低风险";  // 接近TD组
        else if (score < 0.6f) return "中风险";  // 部分接近DD组
        else                   return "高风险";  // 显著接近DD组
    }
}