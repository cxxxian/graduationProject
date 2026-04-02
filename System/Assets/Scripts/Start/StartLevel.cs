using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartLevel : MonoBehaviour
{
    // 音频Source
    public AudioSource introAudio;
    // 延迟播放秒数
    public float introDelay = 3f;
    void Start()
    {
        // 万物开始的地方
        /*
                           _ooOoo_
                          o8888888o
                          88" . "88
                          (| -_- |)
                          O\  =  /O
                       ____/`---'\____
                     .'  \\|     |//  `.
                    /  \\|||  :  |||//  \
                   /  _||||| -:- |||||-  \
                   |   | \\\  -  /// |   |
                   | \_|  ''\---/''  |   |
                   \  .-\__  `-`  ___/-. /
                 ___`. .'  /--.--\  `. . __
              ."" '<  `.___\_<|>_/___.'  >'"".
             | | :  `- \`.;`\ _ /`;.`/ - ` : | |
             \  \ `-.   \_ __\ /__ _/   .-` /  /
        ======`-.____`-.___\_____/___.-`____.-'======
                           `=---='
        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                    佛祖保佑       永无BUG
        */

        // 延迟播放介绍音频
        StartCoroutine(PlayIntroAfterDelay());
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

}
