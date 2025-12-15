using UnityEngine;
using System.Collections;

public class GameManagerStage1 : MonoBehaviour
{
    private HUDManagerStage1 hudManager;

    [Header("UI 패널 연결")]
    public GameObject gameOverPanel;   // ★ gameClearPanel 제거

    [Header("다음 스테이지 선택 (MapUI가 패널/이동 담당)")]
    public MapUI mapUI;
    public string nextSceneName = "Stage2";
    public int choiceIndex = 1;

    private int currentScore = 0;

    [Header("체력")]
    public int maxHealth = 4;
    private int currentHealth;

    private bool isGameClear = false;

    [Header("BGM 설정")]
    public AudioClip stageBGM;
    public AudioClip gameClearBGM;
    public AudioClip gameOverBGM;
    [Range(0f, 1f)]
    public float bgmVolume = 0.5f;

    private AudioSource bgmSource;

    void Awake()
    {
        hudManager = FindAnyObjectByType<HUDManagerStage1>();

        if (mapUI == null)
            mapUI = FindAnyObjectByType<MapUI>();
    }

    void Start()
    {
        // BGM AudioSource 세팅
        bgmSource = GetComponent<AudioSource>();
        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;

        if (stageBGM != null) PlayBGM(stageBGM, true);

        // 초기 UI 상태
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        Time.timeScale = 1f;

        currentScore = 0;
        currentHealth = maxHealth;

        if (hudManager != null)
        {
            hudManager.UpdateScore(currentScore);
            hudManager.UpdateHealth(currentHealth);
            hudManager.SetGameActive(true);
        }
        else
        {
            Debug.LogWarning("HUDManagerStage1이 씬에 없음 (HUD 업데이트 생략)");
        }
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        if (hudManager != null) hudManager.UpdateScore(currentScore);
    }

    public void TakeDamage()
    {
        currentHealth = Mathf.Max(0, currentHealth - 1);
        if (hudManager != null) hudManager.UpdateHealth(currentHealth);

        if (currentHealth <= 0) GameOver();
    }

    public void GameClear()
    {
        if (isGameClear) return;
        isGameClear = true;
        StartCoroutine(GameClearCoroutine(10f));
    }

    private IEnumerator GameClearCoroutine(float delay)
    {
        if (hudManager != null) hudManager.SetGameActive(false);

        yield return new WaitForSeconds(delay);

        if (gameClearBGM != null) PlayBGM(gameClearBGM, false);

        // ★ 클리어 패널은 MapUI가 띄움 + 선택 진행
        if (mapUI != null)
        {
            mapUI.OpenChoice(choiceIndex, nextSceneName);
        }
        else
        {
            Debug.LogError("mapUI가 null임! Stage1 씬에 MapUI가 있어야 함.");
            Time.timeScale = 0f;
        }
    }

    public void GameOver()
    {
        if (gameOverBGM != null) PlayBGM(gameOverBGM, false);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;

        if (hudManager != null) hudManager.SetGameActive(false);
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

    public void RestartGame()
    {
        Time.timeScale = 1f;
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
    }

    private void PlayBGM(AudioClip clip, bool loop)
    {
        if (bgmSource == null || clip == null) return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }
}
