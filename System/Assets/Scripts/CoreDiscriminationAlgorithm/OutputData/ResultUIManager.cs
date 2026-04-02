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
        float finalScore = totalScore / count;
        string finalLevel = GetLevel(finalScore);

        Debug.Log($"最终得分：{finalScore}，等级：{finalLevel}");

        // 显示在UI上
        //finalScoreText.text = "综合得分：" + finalScore.ToString("F2");
        //finalLevelText.text = "综合等级：" + finalLevel;
        finalLevelText.text = finalLevel;
    }

    float CalculateScore(TaskResultData data)
    {
        float normOmission = data.OmissionCount / 5f;
        float normCommission = data.CommissionCount / 10f;
        float normMotor = data.MotorCount / 5f;
        float normBE = data.BE / 30f;
        float normPSMM = 1 - (data.PSMM / 10f);

        float score =
            0.30f * normOmission +
            0.30f * normCommission +
            0.10f * normMotor +
            0.20f * normBE +
            0.10f * normPSMM;

        return score;
    }
    string GetLevel(float score)
    {
        if (score < 0.3f)
            return "A";
        else if (score < 0.6f)
            return "B";
        else
            return "C";
    }
}