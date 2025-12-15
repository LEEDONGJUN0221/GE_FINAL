using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class VinePatternSpawner : MonoBehaviour
{
    [Header("Reference Tilemap (바닥/기준)")]
    public Tilemap referenceTilemap;

    [Header("Playable Area (셀 기준)")]
    public int gridSize = 11;                    // 11x11
    public Vector2Int originCell = new Vector2Int(1, 1); // 플레이 영역 좌하단 셀 좌표 (예: (1,1)~(11,11))

    [Header("Prefabs")]
    public GameObject telegraphPrefab;
    public GameObject vinePrefab;

    [Header("Timing")]
    public float telegraphStart = 1.20f;
    public float telegraphMin = 0.95f;
    [HideInInspector] public float telegraphTime = 0.7f;

    public float activeTimeBase = 0.40f;
    [HideInInspector] public float activeTime = 0.45f;

    [Header("Safety")]
    public bool avoidPlayerCell = false;          // 플레이어가 서있는 셀은 스폰 제외
    public Transform playerTransform;            // 비우면 FindWithTag로 자동 탐색

    private readonly List<GameObject> spawned = new List<GameObject>();
    private int patternId = 0;
    private Coroutine running;

    public enum PatternType { EvenRows, OddRows, EvenCols, OddCols }

    private void Awake()
    {
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }
    }
    

    public void TriggerRandomLines(int minLines, int maxLines)
    {
        if (referenceTilemap == null || telegraphPrefab == null || vinePrefab == null)
        {
            Debug.LogError("[VinePatternSpawner] Missing references.");
            return;
        }

        if (running != null) StopCoroutine(running);
        running = StartCoroutine(CoRunRandomLines(minLines, maxLines));
    }

    private IEnumerator CoRunRandomLines(int minLines, int maxLines)
    {
        ClearAllImmediate();

        patternId++;

        // 행만/열만 랜덤 선택
        bool useRows = (Random.Range(0, 2) == 0);

        int count = Random.Range(minLines, maxLines + 1);
        count = Mathf.Clamp(count, 1, gridSize);

        // 중복 없이 라인 인덱스 선택
        HashSet<int> picked = new HashSet<int>();
        while (picked.Count < count)
            picked.Add(Random.Range(0, gridSize));

        // 1) Telegraph
        foreach (int line in picked)
            SpawnSingleLine(useRows, line, telegraphPrefab, -1);

        yield return new WaitForSeconds(telegraphTime);

        // 2) Clear Telegraph
        ClearAllImmediate();

        // 3) Vine
        foreach (int line in picked)
            SpawnSingleLine(useRows, line, vinePrefab, patternId);

        yield return new WaitForSeconds(activeTime);

        // 4) Clear Vine
        ClearAllImmediate();

        running = null;
    }

    private void SpawnSingleLine(bool useRows, int lineIndex, GameObject prefab, int pid)
    {
        // useRows=true: lineIndex번째 "행" 전체
        // useRows=false: lineIndex번째 "열" 전체
        for (int i = 0; i < gridSize; i++)
        {
            int r = useRows ? lineIndex : i;
            int c = useRows ? i : lineIndex;

            int cellX = originCell.x + c;
            int cellY = originCell.y + r;
            Vector3Int cellPos = new Vector3Int(cellX, cellY, 0);

            Vector3 worldCenter = referenceTilemap.GetCellCenterWorld(cellPos);
            GameObject obj = Instantiate(prefab, worldCenter, Quaternion.identity, transform);
            spawned.Add(obj);

            if (pid >= 0)
            {
                VineDamageZone dmg = obj.GetComponent<VineDamageZone>();
                if (dmg != null) dmg.SetPatternId(pid);
            }
        }
    }


    public void TriggerRandomPattern()
    {
        if (referenceTilemap == null)
        {
            Debug.LogError("[VinePatternSpawner] referenceTilemap is NULL");
            return;
        }
        if (telegraphPrefab == null || vinePrefab == null)
        {
            Debug.LogError("[VinePatternSpawner] telegraphPrefab 또는 vinePrefab이 NULL");
            return;
        }

        if (running != null) StopCoroutine(running);
        running = StartCoroutine(CoRunPattern());
    }

    private IEnumerator CoRunPattern()
    {
        ClearAllImmediate();

        patternId++;
        PatternType selected = (PatternType)Random.Range(0, 4);
        Debug.Log($"[VinePatternSpawner] Pattern = {selected} (ID={patternId})");

        // 1) Telegraph
        SpawnPattern(selected, telegraphPrefab, -1);
        yield return new WaitForSeconds(telegraphTime);

        // 2) Clear Telegraph
        ClearAllImmediate();

        // 3) Vine
        SpawnPattern(selected, vinePrefab, patternId);
        yield return new WaitForSeconds(activeTime);

        // 4) Clear Vine
        ClearAllImmediate();

        running = null;
    }

    private void SpawnPattern(PatternType type, GameObject prefab, int pid)
    {
        switch (type)
        {
            case PatternType.EvenRows: SpawnHorizontal(0, prefab, pid); break;
            case PatternType.OddRows:  SpawnHorizontal(1, prefab, pid); break;
            case PatternType.EvenCols: SpawnVertical(0, prefab, pid); break;
            case PatternType.OddCols:  SpawnVertical(1, prefab, pid); break;
        }
    }

    // r: row(0~gridSize-1), c: col(0~gridSize-1)
    private void SpawnHorizontal(int startRowParity, GameObject prefab, int pid)
    {
        for (int r = startRowParity; r < gridSize; r += 2)
        {
            for (int c = 0; c < gridSize; c++)
            {
                SpawnAtCell(r, c, prefab, pid);
            }
        }
    }

    private void SpawnVertical(int startColParity, GameObject prefab, int pid)
    {
        for (int c = startColParity; c < gridSize; c += 2)
        {
            for (int r = 0; r < gridSize; r++)
            {
                SpawnAtCell(r, c, prefab, pid);
            }
        }
    }

    private void SpawnAtCell(int r, int c, GameObject prefab, int pid)
    {
        // 플레이 영역 셀 좌표 계산
        int cellX = originCell.x + c;
        int cellY = originCell.y + r;
        Vector3Int cellPos = new Vector3Int(cellX, cellY, 0);

        // 플레이어가 서있는 셀은 스폰 제외(즉사 방지)
        if (avoidPlayerCell && playerTransform != null)
        {
            Vector3Int playerCell = referenceTilemap.WorldToCell(playerTransform.position);
            if (playerCell.x == cellPos.x && playerCell.y == cellPos.y)
                return;
        }

        Vector3 worldCenter = referenceTilemap.GetCellCenterWorld(cellPos);
        GameObject obj = Instantiate(prefab, worldCenter, Quaternion.identity, transform);
        spawned.Add(obj);

        if (pid >= 0)
        {
            VineDamageZone dmg = obj.GetComponent<VineDamageZone>();
            if (dmg != null) dmg.SetPatternId(pid);
        }
    }

    public void ClearAllImmediate()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] != null) Destroy(spawned[i]);
        }
        spawned.Clear();
    }
}
