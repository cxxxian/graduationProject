using TMPro;
using UnityEngine;

public class ResultPanel : MonoBehaviour
{
    public TMP_Text matchedText;
    public TMP_Text omissionText;
    public TMP_Text commissionText;
    public TMP_Text motorText;

    //public TMP_Text completionText;

    public TMP_Text itmnText;
    public TMP_Text psmmText;
    public TMP_Text beText;

    public void ShowResult(TaskResultData data)
    {
        matchedText.text = "任务完成进度: " + data.MatchedCount;
        omissionText.text = "遗漏错误数量: " + data.OmissionCount;
        commissionText.text = "执行错误数量: " + data.CommissionCount;
        motorText.text = "运动错误数量: " + data.MotorCount;

        //completionText.text = "Completion: " + (data.CompletionRate * 100).ToString("F1") + "%";

        itmnText.text = "ITMN: " + data.ITMN.ToString("F2");
        psmmText.text = "PSMM: " + data.PSMM.ToString("F2");
        beText.text = "BE: " + data.BE.ToString("F2");
    }
}