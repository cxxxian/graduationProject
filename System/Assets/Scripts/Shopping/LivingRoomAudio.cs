using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingRoomAudio : MonoBehaviour
{
    // “Ù∆µSource
    public AudioSource introAudio;
    // —”≥Ÿ≤•∑≈√Î ˝
    public float introDelay = 3f;
    void Start()
    {
        // —”≥Ÿ≤•∑≈ΩÈ…‹“Ù∆µ
        StartCoroutine(PlayIntroAfterDelay());
    }
    IEnumerator PlayIntroAfterDelay()
    {
        yield return new WaitForSeconds(introDelay);
        PlayIntroAudio();
    }
    // ≤•∑≈ΩÈ…‹“Ù∆µ£®∞Û∂®÷ÿ–¬≤•∑≈∞¥≈•£©
    public void PlayIntroAudio()
    {
        if (introAudio != null)
        {
            introAudio.Stop();
            introAudio.Play();
        }
    }
}
