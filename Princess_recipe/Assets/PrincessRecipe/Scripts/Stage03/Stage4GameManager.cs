using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro; // TMP 쓰면 유지, 안 쓰면 지워도 됨
using UnityEngine.SceneManagement;
public class Stage4GameManager : MonoBehaviour
{
    public static Stage4GameManager Instance;

    [Header("Player HP")]
    public int maxHP = 3;
    public int currentHP;

    [Header("Vine Pattern")]
    public VinePatternSpawner vineSpawner;
    public float beatInterval = 1.2f;

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
    private bool isGameOver = false;
    private bool isGameClear = false;
    private float playTime = 0f;

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

    private void Start()
    {
        currentHP = maxHP;

        if (vineSpawner == null)
        {
            Debug.LogError("[Stage4] VinePatternSpawner is NULL (Inspector에 연결 필요)");
            return;
        }

        UpdateUI();
        StartCoroutine(CoPatternLoop());
    }

    private void Update()
    {
        if (isGameOver || isGameClear) return;

        playTime += Time.deltaTime;
        UpdateUI();
    }

    private IEnumerator CoPatternLoop()
    {
        yield return new WaitForSeconds(1.0f);

        while (!isGameOver && !isGameClear)
        {
            // 난이도: 과일 진행도 기준으로 경고시간 줄이기(원하면 HP 기준으로 바꿔도 됨)
            float progress = (targetFruitCount <= 0) ? 0f : Mathf.Clamp01((float)currentFruitCount / targetFruitCount);

            // 스포너 쪽 타이밍을 “여기서” 갱신
            vineSpawner.telegraphTime = Mathf.Lerp(vineSpawner.telegraphStart, vineSpawner.telegraphMin, progress);
            vineSpawner.activeTime = vineSpawner.activeTimeBase; // 고정(원하면 progress로 조절 가능)

            vineSpawner.TriggerRandomPattern();

            yield return new WaitForSeconds(beatInterval);
        }
    }

    // ---------------- Damage ----------------
    public void TakeDamage(int damage)
    {
        if (isGameOver || isGameClear) return;

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        Debug.Log($"[Stage4] HP {currentHP}/{maxHP}");
        UpdateUI();

        if (currentHP <= 0)
            GameOver();
    }
    private void ShowResult(string message){
            if (resultPanel != null) resultPanel.SetActive(true);
            if (resultText != null) resultText.text = message;

            // (선택) 게임 정지
            Time.timeScale = 0f; 
        }

    private void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        
        StopAllCoroutines();
        if (vineSpawner != null) vineSpawner.ClearAllImmediate();
        
        
        ShowResult("GAME OVER");

        Debug.Log("[Stage4] GAME OVER");
        // TODO: 게임오버 UI 표시/씬 이동
    }

    private void GameClear()
    {
        if (isGameClear) return;
        isGameClear = true;
        
        StopAllCoroutines();
        if (vineSpawner != null) vineSpawner.ClearAllImmediate();
        
        
        Debug.Log("[Stage4] GAME CLEAR");
        ShowResult("CLEAR");
    }

    public void Restart()
    {
        Time.timeScale = 1f;

        // restartSceneName 비어있으면 현재 씬 재시작
        if (string.IsNullOrEmpty(restartSceneName))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            SceneManager.LoadScene(restartSceneName);
        }
    }


    // ---------------- Fruit API (FruitTree.cs 호환 유지) ----------------
    public bool CanCollectFrom(int treeId)
    {
        return treeId != lastCollectedTreeId;
    }

    public void OnFruitCollected()
    {
        if (isGameOver || isGameClear) return;

        currentFruitCount++;
        Debug.Log($"[Stage4] Fruit {currentFruitCount}/{targetFruitCount}");
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
