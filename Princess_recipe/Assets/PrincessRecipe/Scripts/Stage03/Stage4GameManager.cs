using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Stage4GameManager : MonoBehaviour
{
    public static Stage4GameManager Instance;

    [Header("Player HP")]
    public int maxHP = 3;
    public int currentHP;

    [Header("Vine Pattern")]
    public VinePatternSpawner vineSpawner;

    [Header("Fruit Progress")]
    public int targetFruitCount = 16;
    public int currentFruitCount = 0;

    [Header("UI (둘 중 하나만 연결해도 됨)")]
    public Text hpText;
    public Text fruitText;
    public Text timeText;

    public TMP_Text hpTMP;
    public TMP_Text fruitTMP;
    public TMP_Text timeTMP;

    [Header("Result UI")]
    public GameObject resultPanel;
    public TMP_Text resultText;
    public string restartSceneName;

    [Header("Phase Speed (Fruit Count Based)")]
    public float phase1BeatInterval = 2.40f;
    public float phase2BeatInterval = 2.20f;
    public float phase3BeatInterval = 1.90f;

    [Header("Phase Transition: Camera Zoom")]
    public CameraZoomController cameraZoom; // ✅ 인스펙터 연결

    private bool isGameOver = false;
    private bool isGameClear = false;
    private float playTime = 0f;

    private int lastPhase = -1;

    // 같은 나무 연속 수확 금지
    private int lastCollectedTreeId = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private IEnumerator Start()
    {
        currentHP = maxHP;

        if (vineSpawner == null)
        {
            Debug.LogError("[Stage4] VinePatternSpawner is NULL (Inspector에 연결 필요)");
            yield break;
        }

        UpdateUI();

        // ✅ 게임 시작 연출: 일시정지 → 보스 줌인 → 복귀 → 게임 시작
        if (cameraZoom != null)
        {
            cameraZoom.PlayBossCinematic(1);
            while (cameraZoom.IsPlaying) yield return null;
        }

        StartCoroutine(CoPatternLoop());
    }

    private void Update()
    {
        if (isGameOver || isGameClear) return;

        playTime += Time.deltaTime;
        UpdateUI();
    }

    private int GetPhaseByFruit()
    {
        // 0~4 = 1, 5~11 = 2, 12~ = 3
        if (currentFruitCount >= 12) return 3;
        if (currentFruitCount >= 5) return 2;
        return 1;
    }

    private float GetBeatIntervalByPhase(int phase)
    {
        switch (phase)
        {
            case 1: return phase1BeatInterval;
            case 2: return phase2BeatInterval;
            default: return phase3BeatInterval;
        }
    }

    private IEnumerator CoPatternLoop()
    {
        yield return new WaitForSeconds(1.0f);

        while (!isGameOver && !isGameClear)
        {
            // 1) 과일 진행도 기준 경고시간 계산
            float progress = (targetFruitCount <= 0)
                ? 0f
                : Mathf.Clamp01((float)currentFruitCount / targetFruitCount);

            vineSpawner.telegraphTime =
                Mathf.Lerp(vineSpawner.telegraphStart, vineSpawner.telegraphMin, progress);

            vineSpawner.activeTime = vineSpawner.activeTimeBase;

            // 2) 페이즈 계산 + 페이즈 전환 연출(Phase2/3만)
            int phase = GetPhaseByFruit();
            if (phase != lastPhase)
            {
                lastPhase = phase;

                // ✅ Phase2, Phase3 진입 시: 일시정지 → 보스 줌인 → 복귀 → 게임 재개
                if (cameraZoom != null && (phase == 2 || phase == 3))
                {
                    cameraZoom.PlayBossCinematic(phase);
                    while (cameraZoom.IsPlaying) yield return null;
                }
            }

            // 3) 패턴 발동
            if (phase == 1)
            {
                // Phase1: 랜덤 행/열 일부 라인 (원하면 1,3 / 2,4 같은 느낌으로 튜닝 가능)
                vineSpawner.TriggerRandomLines(2, 3);
            }
            else
            {
                // Phase2/3: 기존 홀/짝 행/열 패턴 랜덤
                vineSpawner.TriggerRandomPattern();
            }

            // 4) 페이즈별 속도
            float interval = GetBeatIntervalByPhase(phase);
            yield return new WaitForSeconds(interval);
        }
    }

    // ---------------- Damage ----------------
    public void TakeDamage(int damage)
    {
        if (isGameOver || isGameClear) return;

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        UpdateUI();

        if (currentHP <= 0)
            GameOver();
    }

    private void ShowResult(string message)
    {
        if (resultPanel != null) resultPanel.SetActive(true);
        if (resultText != null) resultText.text = message;

        // 결과 UI에서 정지
        Time.timeScale = 0f;
    }

    private void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        StopAllCoroutines();
        if (vineSpawner != null) vineSpawner.ClearAllImmediate();

        ShowResult("GAME OVER");
    }

    private void GameClear()
    {
        if (isGameClear) return;
        isGameClear = true;

        StopAllCoroutines();
        if (vineSpawner != null) vineSpawner.ClearAllImmediate();

        // ✅ 클리어 연출: 보스 줌인(죽은 아트/회색 등) → 클리어 UI
        StartCoroutine(CoClearSequence());
    }

    private IEnumerator CoClearSequence()
    {
        if (cameraZoom != null)
        {
            cameraZoom.PlayBossCinematic(99); // clearColor
            while (cameraZoom.IsPlaying) yield return null;
        }

        ShowResult("CLEAR");
    }

    public void Restart()
    {
        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(restartSceneName))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else
            SceneManager.LoadScene(restartSceneName);
    }

    // ---------------- Fruit API ----------------
    public bool CanCollectFrom(int treeId)
    {
        return treeId != lastCollectedTreeId;
    }

    public void OnFruitCollected()
    {
        if (isGameOver || isGameClear) return;

        currentFruitCount++;
        UpdateUI();

        if (currentFruitCount >= targetFruitCount)
            GameClear();
    }

    public void OnCollectedFromTree(int treeId)
    {
        lastCollectedTreeId = treeId;
    }

    // ---------------- UI ----------------
    private void UpdateUI()
    {
        string hpStr = $"HP: {currentHP}/{maxHP}";
        string fruitStr = $"Fruit: {currentFruitCount}/{targetFruitCount}";
        string timeStr = $"Time: {playTime:0.0}s";

        if (hpText) hpText.text = hpStr;
        if (fruitText) fruitText.text = fruitStr;
        if (timeText) timeText.text = timeStr;

        if (hpTMP) hpTMP.text = hpStr;
        if (fruitTMP) fruitTMP.text = fruitStr;
        if (timeTMP) timeTMP.text = timeStr;
    }
}
