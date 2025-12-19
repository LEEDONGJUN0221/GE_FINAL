using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerStage0 : MonoBehaviour
{
    // ★ Restart 상태 기억용
    private static bool restartFromGame = false;

    [Header("시작 화면")]
    public GameObject startPanel;
    public GameObject gamePanel;

    [Header("일시정지 UI")]
    public GameObject pausePanel;

    [Header("BGM 설정")]
    public AudioClip stageBGM;
    [Range(0f, 1f)]
    public float bgmVolume = 0.5f;

    private AudioSource bgmSource;
    private bool isPaused = false;
    private bool isGameStarted = false;

    void Start()
    {
        // AudioSource 세팅
        bgmSource = GetComponent<AudioSource>();
        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;

        if (pausePanel != null) pausePanel.SetActive(false);

        // ======================
        // Restart로 들어온 경우
        // ======================
        if (restartFromGame)
        {
            restartFromGame = false; // 한 번 쓰고 초기화
            StartGameImmediate();
        }
        else
        {
            // 일반 시작
            if (startPanel != null) startPanel.SetActive(true);
            if (gamePanel != null) gamePanel.SetActive(false);

            Time.timeScale = 0f;
            isGameStarted = false;
        }
    }

    // ======================
    // 일반 게임 시작 (버튼)
    // ======================
    public void StartGame()
    {
        StartGameImmediate();
    }

    // ======================
    // 내부 공통 시작 로직
    // ======================
    private void StartGameImmediate()
    {
        isGameStarted = true;
        isPaused = false;

        if (startPanel != null) startPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);

        Time.timeScale = 1f;

        if (stageBGM != null)
        {
            bgmSource.clip = stageBGM;
            bgmSource.Play();
        }
    }

    // ======================
    // 일시정지
    // ======================
    public void TogglePause()
    {
        if (!isGameStarted) return;

        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // ======================
    // Restart (★ 시작화면 스킵)
    // ======================
    public void RestartGame()
    {
        restartFromGame = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
}
