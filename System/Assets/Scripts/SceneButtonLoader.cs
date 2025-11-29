using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonLoader : MonoBehaviour
{
    [Header("要切换的场景名称")]
    public string targetSceneName;

    // 绑定到切换场景按钮
    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("SceneButtonLoader: targetSceneName 为空，请在 Inspector 设置要切换的场景名称！");
            return;
        }

        // 加载指定场景
        SceneManager.LoadScene(targetSceneName);
    }
}
