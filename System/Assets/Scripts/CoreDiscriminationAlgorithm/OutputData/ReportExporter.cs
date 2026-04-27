using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class ReportExporter
{
    static readonly Dictionary<string, string> SceneNameMap = new Dictionary<string, string>
    {
        { "KitchenTaskScene", "厨房任务" },
        { "ShoppingScene",    "购物任务" },
        { "HandworkScene",    "手工任务" },
    };

    public static void Export(
        string userName,
        string gender,
        List<TaskResultData> results,
        float finalScore,
        string finalLevel,
        int riskPercent,
        Dictionary<string, float> sceneScores)
    {
        string safeUserName = string.IsNullOrEmpty(userName) ? "未知用户" : userName;
        string timestamp    = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string displayTime  = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        string folder   = Path.Combine(Application.persistentDataPath, "Reports");
        Directory.CreateDirectory(folder);
        string baseName = $"{safeUserName}_{timestamp}";

        // HTML
        string htmlContent = BuildHtml(userName, gender, displayTime, results, finalScore, finalLevel, riskPercent, sceneScores);
        File.WriteAllText(Path.Combine(folder, baseName + ".html"), htmlContent, Encoding.UTF8);

        Debug.Log($"[ReportExporter] 报告已导出至：{Path.Combine(folder, baseName + ".html")}");
    }

    // ===== HTML =====
    static string BuildHtml(
        string userName, string gender, string displayTime,
        List<TaskResultData> results,
        float finalScore, string finalLevel, int riskPercent,
        Dictionary<string, float> sceneScores)
    {
        string levelCss = finalLevel == "低风险" ? "low" : (finalLevel == "中风险" ? "mid" : "high");

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"zh-CN\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\">");
        sb.AppendLine("  <title>VR执行功能评估报告</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("    body{font-family:'Microsoft YaHei',Arial,sans-serif;max-width:860px;margin:40px auto;color:#333;padding:0 20px;}");
        sb.AppendLine("    h1{text-align:center;color:#2c3e50;border-bottom:3px solid #3498db;padding-bottom:12px;}");
        sb.AppendLine("    .card{background:#f8f9fa;padding:16px 22px;border-radius:8px;margin:18px 0;border:1px solid #e0e0e0;}");
        sb.AppendLine("    .risk-wrap{text-align:center;padding:10px 0;}");
        sb.AppendLine("    .badge{display:inline-block;padding:8px 28px;border-radius:20px;font-size:1.3em;font-weight:bold;color:#fff;margin-bottom:8px;}");
        sb.AppendLine("    .low{background:#27ae60;} .mid{background:#f39c12;} .high{background:#e74c3c;}");
        sb.AppendLine("    .scene-title{font-size:1.05em;font-weight:bold;margin:28px 0 8px;color:#2c3e50;border-left:4px solid #3498db;padding-left:10px;}");
        sb.AppendLine("    table{width:100%;border-collapse:collapse;margin:8px 0;}");
        sb.AppendLine("    th{background:#3498db;color:#fff;padding:9px 12px;text-align:left;font-weight:normal;}");
        sb.AppendLine("    td{padding:8px 12px;border-bottom:1px solid #ddd;}");
        sb.AppendLine("    tr:nth-child(even){background:#f4f4f4;}");
        sb.AppendLine("    .footer{text-align:center;color:#aaa;font-size:0.82em;margin-top:40px;border-top:1px solid #eee;padding-top:12px;}");
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        sb.AppendLine("  <h1>VR 执行功能评估报告</h1>");

        // 用户信息
        sb.AppendLine("  <div class=\"card\">");
        sb.AppendLine($"    <p><strong>姓名：</strong>{Esc(userName)} &nbsp;&nbsp; <strong>性别：</strong>{Esc(gender)}</p>");
        sb.AppendLine($"    <p><strong>报告生成时间：</strong>{displayTime}</p>");
        sb.AppendLine("  </div>");

        // 综合风险
        sb.AppendLine("  <div class=\"card risk-wrap\">");
        sb.AppendLine("    <p style=\"font-size:1.05em;margin-bottom:10px;\"><strong>综合风险评估</strong></p>");
        sb.AppendLine($"    <span class=\"badge {levelCss}\">{finalLevel}</span>");
        sb.AppendLine($"    <p>与 DD 组认知特征匹配度：<strong>{riskPercent}%</strong></p>");
        sb.AppendLine($"    <p style=\"color:#888;font-size:0.88em;\">综合风险分：{finalScore:F3}（0=完全接近正常儿童，1=完全接近发育障碍儿童）</p>");
        sb.AppendLine("  </div>");

        // 各场景
        foreach (var data in results)
        {
            string cnName = SceneNameMap.TryGetValue(data.SceneName, out var n) ? n : data.SceneName;
            float  score  = sceneScores.TryGetValue(data.SceneName, out var s) ? s : 0f;

            sb.AppendLine($"  <div class=\"scene-title\">{cnName}（{data.SceneName}）</div>");
            sb.AppendLine("  <table>");
            sb.AppendLine("    <tr><th>指标</th><th>数值</th><th>说明</th></tr>");
            sb.AppendLine($"    <tr><td>完成率</td><td>{(data.CompletionRate * 100f):F1}%</td><td>标准步骤中正确完成的比例</td></tr>");
            sb.AppendLine($"    <tr><td>遗漏错误（Omission）</td><td>{data.OmissionCount}</td><td>标准步骤中未执行的操作数</td></tr>");
            sb.AppendLine($"    <tr><td>执行错误（Commission）</td><td>{data.CommissionCount}</td><td>多余或顺序错误的操作数</td></tr>");
            sb.AppendLine($"    <tr><td>运动错误（Motor）</td><td>{data.MotorCount}</td><td>重复或不稳定操作次数</td></tr>");
            sb.AppendLine($"    <tr><td>ITMN（任务启动时间）</td><td>{data.ITMN:F2} 秒</td><td>首次正确操作的时间戳</td></tr>");
            sb.AppendLine($"    <tr><td>PSMM（计划效率）</td><td>{data.PSMM:F3}</td><td>标准步骤数 / 实际操作数，越低越冗余</td></tr>");
            sb.AppendLine($"    <tr><td>BE（空间工作记忆错误）</td><td>{data.BE:F1}</td><td>对无效目标的重复访问次数</td></tr>");
            sb.AppendLine($"    <tr><td>场景风险分</td><td>{score:F3}</td><td>加权相似度评分（0~1）</td></tr>");
            sb.AppendLine("  </table>");

            // 原始日志（折叠）
            if (data.RawLogs != null && data.RawLogs.Count > 0)
            {
                sb.AppendLine($"  <details style=\"margin:6px 0 18px;\"><summary style=\"cursor:pointer;color:#3498db;\">查看原始操作日志（{data.RawLogs.Count} 条）</summary>");
                sb.AppendLine("  <table style=\"margin-top:6px;\">");
                sb.AppendLine("    <tr><th>序号</th><th>事件类型</th><th>目标</th><th>时间(秒)</th><th>上下文</th></tr>");
                for (int i = 0; i < data.RawLogs.Count; i++)
                {
                    var e = data.RawLogs[i];
                    sb.AppendLine($"    <tr><td>{i + 1}</td><td>{e.Type}</td><td>{Esc(e.Target)}</td><td>{e.Time:F2}</td><td>{Esc(e.Context)}</td></tr>");
                }
                sb.AppendLine("  </table></details>");
            }
        }

        sb.AppendLine("  <div class=\"footer\">本报告由 VR 执行功能评估系统自动生成 &nbsp;|&nbsp; 风险模型基准数据来源：参考文献 TD/DD 组实验数据</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    // ===== JSON =====
    static string BuildJson(
        string userName, string gender, string displayTime,
        List<TaskResultData> results,
        float finalScore, string finalLevel, int riskPercent,
        Dictionary<string, float> sceneScores)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"reportTime\": \"{displayTime}\",");
        sb.AppendLine($"  \"userInfo\": {{ \"name\": \"{Esc(userName)}\", \"gender\": \"{Esc(gender)}\" }},");
        sb.AppendLine($"  \"finalRiskScore\": {finalScore:F4},");
        sb.AppendLine($"  \"finalRiskLevel\": \"{finalLevel}\",");
        sb.AppendLine($"  \"riskPercent\": {riskPercent},");
        sb.AppendLine("  \"scenes\": [");

        for (int i = 0; i < results.Count; i++)
        {
            var data = results[i];
            float score = sceneScores.TryGetValue(data.SceneName, out var s) ? s : 0f;
            bool last = (i == results.Count - 1);

            sb.AppendLine("    {");
            sb.AppendLine($"      \"sceneName\": \"{data.SceneName}\",");
            sb.AppendLine($"      \"completionRate\": {data.CompletionRate:F4},");
            sb.AppendLine($"      \"omissionCount\": {data.OmissionCount},");
            sb.AppendLine($"      \"commissionCount\": {data.CommissionCount},");
            sb.AppendLine($"      \"motorCount\": {data.MotorCount},");
            sb.AppendLine($"      \"itmn\": {data.ITMN:F4},");
            sb.AppendLine($"      \"psmm\": {data.PSMM:F4},");
            sb.AppendLine($"      \"be\": {data.BE:F4},");
            sb.AppendLine($"      \"sceneRiskScore\": {score:F4},");

            // 原始日志
            sb.AppendLine("      \"rawLogs\": [");
            if (data.RawLogs != null)
            {
                for (int j = 0; j < data.RawLogs.Count; j++)
                {
                    var e = data.RawLogs[j];
                    bool lastLog = (j == data.RawLogs.Count - 1);
                    sb.Append($"        {{ \"type\": \"{e.Type}\", \"target\": \"{Esc(e.Target)}\", \"time\": {e.Time:F3}, \"context\": \"{Esc(e.Context)}\" }}");
                    sb.AppendLine(lastLog ? "" : ",");
                }
            }
            sb.AppendLine("      ]");
            sb.Append("    }");
            sb.AppendLine(last ? "" : ",");
        }

        sb.AppendLine("  ]");
        sb.AppendLine("}");
        return sb.ToString();
    }

    static string Esc(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("<", "&lt;").Replace(">", "&gt;");
    }
}
