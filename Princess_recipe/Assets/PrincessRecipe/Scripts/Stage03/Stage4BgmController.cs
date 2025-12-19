using System.Collections;
using UnityEngine;

public class Stage4BgmController : MonoBehaviour
{
    [Header("AudioSource")]
    public AudioSource source;

    [Header("BGM Clips")]
    public AudioClip phase1Bgm;
    public AudioClip phase2Bgm;
    public AudioClip phase3Bgm;

    [Header("Result BGM")]
    public AudioClip clearBgm;
    public AudioClip gameOverBgm;

    [Header("Fade Settings")]
    public float fadeOutTime = 0.35f;
    public float fadeInTime = 0.35f;
    [Range(0f, 1f)] public float targetVolume = 0.8f;

    private Coroutine fadeRoutine;
    private int currentPhase = -1;

    private void Awake()
    {
        if (source == null) source = GetComponent<AudioSource>();
        if (source != null)
        {
            source.loop = true;
            source.playOnAwake = false;
        }
    }

    public void PlayPhase(int phase, bool force = false)
    {
        if (!force && phase == currentPhase) return;

        AudioClip next = GetClip(phase);
        if (next == null || source == null) return;

        currentPhase = phase;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(CoSwapWithFade(next));
    }

    public void PlayClear(bool force = true)
    {
        if (clearBgm == null || source == null) return;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(CoSwapWithFade(clearBgm));
    }

    public void PlayGameOver(bool force = true)
    {
        if (gameOverBgm == null || source == null) return;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(CoSwapWithFade(gameOverBgm));
    }


    private AudioClip GetClip(int phase)
    {
        switch (phase)
        {
            case 1: return phase1Bgm;
            case 2: return phase2Bgm;
            default: return phase3Bgm;
        }
    }

    private IEnumerator CoSwapWithFade(AudioClip next)
    {
        // fade out
        float t = 0f;
        float startVol = source.volume;
        while (t < fadeOutTime)
        {
            t += Time.unscaledDeltaTime; // Time.timeScale=0 이어도 페이드됨
            source.volume = Mathf.Lerp(startVol, 0f, fadeOutTime <= 0 ? 1f : t / fadeOutTime);
            yield return null;
        }

        source.volume = 0f;
        source.clip = next;
        source.Play();

        // fade in
        t = 0f;
        while (t < fadeInTime)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, fadeInTime <= 0 ? 1f : t / fadeInTime);
            yield return null;
        }

        source.volume = targetVolume;
        fadeRoutine = null;
    }
}
