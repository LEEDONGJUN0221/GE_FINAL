using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class VinePatternSpawner : MonoBehaviour
{
    [Header("Reference Tilemap (바닥/기준)")]
    public Tilemap referenceTilemap;

    [Header("Playable Area (rows x cols)")]
    public int rows = 11;
    public int cols = 11;
    public Vector2Int originCell = new Vector2Int(1, 1);

    [Header("Prefabs (Inspector에 반드시 연결)")]
    public GameObject telegraphPrefab;       // 경고 프리팹(시각용)
    public GameObject vineHorizontalPrefab;  // 가로 vine 프리팹(콜라이더/트리거 포함)
    public GameObject vineVerticalPrefab;    // 세로 vine 프리팹(콜라이더/트리거 포함)

    [Header("Timing (GameManager에서 갱신 가능)")]
    public float telegraphStart = 0.9f;
    public float telegraphMin = 0.55f;

    [HideInInspector] public float telegraphTime = 0.8f;
    [HideInInspector] public float activeTime = 0.8f;
    public float activeTimeBase = 0.8f;

    [Header("Options")]
    public bool avoidPlayerCell = true;
    public Transform playerTransform;        // 권장: Player 넣기
    public string playerTag = "Player";      // playerTransform 비었을 때 fallback
    public bool clearBeforeTrigger = true;   // 패턴 발동 전 잔상 제거

    // 내부 상태
    private Coroutine runningRoutine;
    private readonly List<GameObject> spawnedThisTurn = new List<GameObject>(); // "현재 턴"에서 생성된 것만 관리
    private int patternIdCounter = 0; // 턴(패턴) ID 발급용

    // ---------------- Public API ----------------

    /// <summary>
    /// Phase2/3용: 홀/짝 행/열 4패턴 중 랜덤 1개
    /// </summary>
    public void TriggerRandomPattern()
    {
        if (!ValidateRefs()) return;
        if (clearBeforeTrigger) ClearAllImmediate();

        int mode = Random.Range(0, 4); // 0:OddRows 1:EvenRows 2:OddCols 3:EvenCols
        switch (mode)
        {
            case 0: TriggerOddEvenRows(true); break;
            case 1: TriggerOddEvenRows(false); break;
            case 2: TriggerOddEvenCols(true); break;
            default: TriggerOddEvenCols(false); break;
        }
    }

    /// <summary>
    /// Phase1용: 한 턴은 "행만" 또는 "열만" (섞이지 않음)
    /// 방향을 1번 결정한 뒤 min~max개의 라인을 랜덤 선택하여 발동
    /// </summary>
    public void TriggerRandomLines(int minLines, int maxLines)
    {
        if (!ValidateRefs()) return;
        if (clearBeforeTrigger) ClearAllImmediate();

        int minV = Mathf.Max(1, Mathf.Min(minLines, maxLines));
        int maxV = Mathf.Max(minV, Mathf.Max(minLines, maxLines));
        int lineCount = Random.Range(minV, maxV + 1);

        runningRoutine = StartCoroutine(CoRandomLinesOneDirection(lineCount));
    }

    public void ClearAllImmediate()
    {
        StopTurnRoutine();
        ClearTurnImmediate();
    }

    // ---------------- Pattern Builders ----------------

    public void TriggerOddEvenRows(bool odd)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        int target = odd ? 1 : 0; // 0-based row parity

        for (int r = 0; r < rows; r++)
        {
            if ((r % 2) != target) continue;

            for (int c = 0; c < cols; c++)
            {
                Vector2Int cell = new Vector2Int(originCell.x + c, originCell.y + r);
                if (avoidPlayerCell && IsPlayerCell(cell)) continue;
                cells.Add(cell);
            }
        }

        FireCells(cells, isHorizontal: true);
    }

    public void TriggerOddEvenCols(bool odd)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        int target = odd ? 1 : 0; // 0-based col parity

        for (int c = 0; c < cols; c++)
        {
            if ((c % 2) != target) continue;

            for (int r = 0; r < rows; r++)
            {
                Vector2Int cell = new Vector2Int(originCell.x + c, originCell.y + r);
                if (avoidPlayerCell && IsPlayerCell(cell)) continue;
                cells.Add(cell);
            }
        }

        FireCells(cells, isHorizontal: false);
    }

    private IEnumerator CoRandomLinesOneDirection(int lineCount)
    {
        // ★ 핵심: 턴당 1회만 방향 결정 (가로만 OR 세로만)
        bool chooseRows = Random.Range(0, 2) == 0;

        int maxIndex = chooseRows ? rows : cols;
        lineCount = Mathf.Clamp(lineCount, 1, maxIndex);

        HashSet<int> picked = new HashSet<int>();
        while (picked.Count < lineCount)
            picked.Add(Random.Range(0, maxIndex));

        List<Vector2Int> cells = new List<Vector2Int>();

        if (chooseRows)
        {
            foreach (int r in picked)
            {
                for (int c = 0; c < cols; c++)
                {
                    Vector2Int cell = new Vector2Int(originCell.x + c, originCell.y + r);
                    if (avoidPlayerCell && IsPlayerCell(cell)) continue;
                    cells.Add(cell);
                }
            }
            FireCells(cells, isHorizontal: true);
        }
        else
        {
            foreach (int c in picked)
            {
                for (int r = 0; r < rows; r++)
                {
                    Vector2Int cell = new Vector2Int(originCell.x + c, originCell.y + r);
                    if (avoidPlayerCell && IsPlayerCell(cell)) continue;
                    cells.Add(cell);
                }
            }
            FireCells(cells, isHorizontal: false);
        }

        yield return null;
    }

    // ---------------- Core Turn ----------------

    private void FireCells(List<Vector2Int> cells, bool isHorizontal)
    {
        if (cells == null || cells.Count == 0) return;

        StopTurnRoutine();
        ClearTurnImmediate();

        int patternId = ++patternIdCounter; // ★ 이번 턴(패턴) ID
        runningRoutine = StartCoroutine(CoTelegraphThenVine(cells, isHorizontal, patternId));
    }

    private IEnumerator CoTelegraphThenVine(List<Vector2Int> cells, bool isHorizontal, int patternId)
    {
        // 1) telegraph 생성
        for (int i = 0; i < cells.Count; i++)
        {
            Vector3 world = referenceTilemap.GetCellCenterWorld(new Vector3Int(cells[i].x, cells[i].y, 0));
            GameObject t = Instantiate(telegraphPrefab, world, Quaternion.identity, transform);
            spawnedThisTurn.Add(t);
        }

        // telegraph 유지
        yield return new WaitForSeconds(telegraphTime);

        // 2) telegraph 제거
        for (int i = 0; i < spawnedThisTurn.Count; i++)
        {
            if (spawnedThisTurn[i] != null)
                Destroy(spawnedThisTurn[i]);
        }
        spawnedThisTurn.Clear();

        // 3) vine 생성 (방향별 프리팹)
        GameObject vinePrefab = isHorizontal ? vineHorizontalPrefab : vineVerticalPrefab;

        for (int i = 0; i < cells.Count; i++)
        {
            Vector3 world = referenceTilemap.GetCellCenterWorld(new Vector3Int(cells[i].x, cells[i].y, 0));
            GameObject v = Instantiate(vinePrefab, world, Quaternion.identity, transform);

            // ★ vineDamageZone에 패턴ID 주입 (프리팹 구조가 자식에 붙어있어도 대응)
            var dz = v.GetComponent<VineDamageZone>();
            if (dz == null) dz = v.GetComponentInChildren<VineDamageZone>();
            if (dz != null) dz.SetPatternId(patternId);

            spawnedThisTurn.Add(v);
        }

        // vine 유지
        yield return new WaitForSeconds(activeTime);

        // 4) vine 제거
        for (int i = 0; i < spawnedThisTurn.Count; i++)
        {
            if (spawnedThisTurn[i] != null)
                Destroy(spawnedThisTurn[i]);
        }
        spawnedThisTurn.Clear();

        runningRoutine = null;
    }

    private void StopTurnRoutine()
    {
        if (runningRoutine != null)
        {
            StopCoroutine(runningRoutine);
            runningRoutine = null;
        }
    }

    private void ClearTurnImmediate()
    {
        for (int i = 0; i < spawnedThisTurn.Count; i++)
        {
            if (spawnedThisTurn[i] != null) Destroy(spawnedThisTurn[i]);
        }
        spawnedThisTurn.Clear();
    }

    // ---------------- Helpers ----------------

    private bool ValidateRefs()
    {
        if (referenceTilemap == null)
        {
            Debug.LogError("[VinePatternSpawner] referenceTilemap is NULL");
            return false;
        }
        if (telegraphPrefab == null)
        {
            Debug.LogError("[VinePatternSpawner] telegraphPrefab is NULL");
            return false;
        }
        if (vineHorizontalPrefab == null)
        {
            Debug.LogError("[VinePatternSpawner] vineHorizontalPrefab is NULL");
            return false;
        }
        if (vineVerticalPrefab == null)
        {
            Debug.LogError("[VinePatternSpawner] vineVerticalPrefab is NULL");
            return false;
        }
        return true;
    }

    private bool IsPlayerCell(Vector2Int cell)
    {
        if (referenceTilemap == null) return false;

        Vector3Int pCell;

        if (playerTransform != null)
        {
            pCell = referenceTilemap.WorldToCell(playerTransform.position);
        }
        else
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p == null) return false;
            pCell = referenceTilemap.WorldToCell(p.transform.position);
        }

        return (pCell.x == cell.x && pCell.y == cell.y);
    }
}
