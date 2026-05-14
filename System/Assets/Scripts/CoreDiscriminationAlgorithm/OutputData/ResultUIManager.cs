using TMPro;
using UnityEngine;

public class ResultUIManager : MonoBehaviour
{
    public ResultPanel kitchenPanel;
    public ResultPanel shoppingPanel;
    public ResultPanel handworkPanel;

    //public TMP_Text finalScoreText;
    public TMP_Text finalLevelText;
    public TMP_Text finalRiskText;

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
        var sceneScores = new System.Collections.Generic.Dictionary<string, float>();

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
            sceneScores[result.SceneName] = score;
            totalScore += score;
            count++;
        }
        // 计算总评分
        float finalScore = count > 0 ? totalScore / count : 0f;
        string finalLevel = GetLevel(finalScore);

        Debug.Log($"风险分：{finalScore:F2}，风险等级：{finalLevel}");

        // 显示在UI上
        finalLevelText.text = $"{finalLevel}";
        finalRiskText.text = $"综合风险分值：{finalScore:F2}";

        // 导出报告
        string userName = InformationCollection.Instance != null ? InformationCollection.Instance.userName : "";
        string gender   = InformationCollection.Instance != null ? InformationCollection.Instance.gender : "";
        ReportExporter.Export(userName, gender, TaskSessionManager.Instance.AllTaskResults, finalScore, finalLevel, Mathf.RoundToInt(finalScore * 100f), sceneScores);
    }

    float CalculateScore(TaskResultData data)
    {
        // ===== 启发式预设归一化上限（用于将各指标映射至[0,1]，后续可结合真实样本修正）=====
        const float MAX_Omission   = 4f;   // 遗漏错误预设上限
        const float MAX_Commission = 8f;   // 执行错误预设上限
        const float MAX_BE         = 30f;  // 重复无效操作（RIO/BE）预设上限

        // ===== 各指标归一化为风险贡献值（0=无异常，1=异常最显著）=====

        // 遗漏错误风险（权重30%）：遗漏越多风险越高
        float omissionRisk = Mathf.Clamp01(data.OmissionCount / MAX_Omission);

        // 执行错误风险（权重30%）：多余/顺序错误越多风险越高
        float commissionRisk = Mathf.Clamp01(data.CommissionCount / MAX_Commission);

        // 重复无效操作风险 RIO（权重15%）：BE值越高风险越高
        float rioRisk = Mathf.Clamp01(data.BE / MAX_BE);

        // 操作效率风险 OE-risk（权重15%）：PSMM已归一化至[0,1]，效率越低风险越高
        float oeRisk = 1f - Mathf.Clamp01(data.PSMM);

        // ===== 加权风险分（当前权重为启发式设置，需后续验证）=====
        float riskScore =
            0.35f * omissionRisk   +
            0.35f * commissionRisk +
            0.15f * rioRisk        +
            0.15f * oeRisk;

        Debug.Log($"[{data.SceneName}] 遗漏:{omissionRisk:F2} 执行:{commissionRisk:F2} RIO:{rioRisk:F2} OE-risk:{oeRisk:F2} → 风险分:{riskScore:F2}");
        return riskScore;
    }

    string GetLevel(float score)
    {
        // 阈值为启发式设置，后续可结合真实样本数据校准
        if (score < 0.3f)      return "低风险";
        else if (score < 0.6f) return "中风险";
        else                   return "高风险";
    }
}