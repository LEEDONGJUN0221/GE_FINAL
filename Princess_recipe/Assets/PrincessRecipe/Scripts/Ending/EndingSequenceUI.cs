using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class EndingSequenceUI : MonoBehaviour
{
    [Header("패널 3단계")]
    public GameObject panelIntro;    // 1) "당신이 만든 케이크는 과연!!"
    public GameObject panelDrum;     // 2) "두구두구두구..."
    public GameObject panelResult;   // 3) "짜잔!" + 결과 + 버튼

    [Header("결과 이미지 (UI Image)")]
    public Image resultImage;
    public Sprite[] endingSprites = new Sprite[8];

    [Header("페이드 (검정 풀스크린 Image)")]
    public Image fadeImage;
    public float fadeToDarkSeconds = 1.2f;
    public float fadeToBrightSeconds = 1.0f;

    [Header("효과음")]
    public AudioSource sfxSource;
    public AudioClip sfxIntro;   // 과연!!
    public AudioClip sfxDrum;    // 두구두구
    public AudioClip sfxTaDa;    // 짜잔

    [Header("연출 타이밍")]
    public float introHoldSeconds = 1.2f;
    public float drumHoldSeconds = 1.4f;

    [Header("버튼")]
    public Button quitButton;
    public Button restartButton;

    [Header("Stage0 씬 이름")]
    public string stage0SceneName = "Stage_00";

    private bool played = false;

    void Start()
    {
        // 초기 상태
        if (panelIntro != null) panelIntro.SetActive(false);
        if (panelDrum != null) panelDrum.SetActive(false);
        if (panelResult != null) panelResult.SetActive(false);

        SetFadeAlpha(0f);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartFromStage0);
    }

    // =================================================
    // ★ 외부(상호작용 오브젝트)가 호출하는 시작 함수
    // =================================================
    public void StartSequence()
    {
        if (played) return;
        played = true;

        Time.timeScale = 1f; // 혹시 멈춰있었을 경우 대비
        StartCoroutine(SequenceCoroutine());
    }

    // =================================================
    // 메인 연출 코루틴
    // =================================================
    private IEnumerator SequenceCoroutine()
    {
        if (RunData.I == null)
        {
            Debug.LogError("RunData 없음!");
            yield break;
        }

        int endingId = Mathf.Clamp(RunData.I.EndingId(), 0, 7);
        Debug.Log($"[ENDING] id = {endingId}");

        // 1️⃣ Intro
        ActivateOnly(panelIntro);
        PlaySfx(sfxIntro);
        yield return new WaitForSecondsRealtime(introHoldSeconds);

        // 2️⃣ Drum + 암전
        ActivateOnly(panelDrum);
        PlaySfx(sfxDrum);
        yield return Fade(0f, 1f, fadeToDarkSeconds);
        yield return new WaitForSecondsRealtime(drumHoldSeconds);

        // 3️⃣ Result + 밝아짐
        ActivateOnly(panelResult);

        if (resultImage != null &&
            endingId < endingSprites.Length &&
            endingSprites[endingId] != null)
        {
            resultImage.sprite = endingSprites[endingId];
        }

        PlaySfx(sfxTaDa);
        yield return Fade(1f, 0f, fadeToBrightSeconds);

        // 이후는 버튼 입력 대기
    }

    // =================================================
    // 버튼 기능
    // =================================================
    public void RestartFromStage0()
    {
        if (RunData.I != null)
            RunData.I.ResetForNewRun();

        Time.timeScale = 1f;
        SceneManager.LoadScene(stage0SceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // =================================================
    // 유틸
    // =================================================
    private void ActivateOnly(GameObject go)
    {
        if (panelIntro != null) panelIntro.SetActive(go == panelIntro);
        if (panelDrum != null) panelDrum.SetActive(go == panelDrum);
        if (panelResult != null) panelResult.SetActive(go == panelResult);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    private IEnumerator Fade(float from, float to, float seconds)
    {
        if (fadeImage == null)
            yield break;

        float t = 0f;
        seconds = Mathf.Max(0.0001f, seconds);

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, t / seconds);
            SetFadeAlpha(a);
            yield return null;
        }

        SetFadeAlpha(to);
    }

    private void SetFadeAlpha(float a)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = Mathf.Clamp01(a);
        fadeImage.color = c;
    }
}
