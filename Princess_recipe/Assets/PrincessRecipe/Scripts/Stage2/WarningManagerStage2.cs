using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WarningManagerStage2 : MonoBehaviour
{
    [Header("타일맵")]
    public Tilemap baseTilemap;        // 바닥
    public Tilemap warningTilemap;     // 깜빡임용
    public Tilemap obstacleTilemap;    // 실제 장애물

    [Header("타일")]
    public TileBase warningTile;
    // ⭐ 화이트 / 다크 장애물 타일 (인스펙터에서 넣을 것)
    public TileBase whiteChocolateObstacleTile;
    public TileBase darkChocolateObstacleTile;

    // ⭐ 실제로 사용할 타일 (런타임에 자동 결정)
    private TileBase chocolateObstacleTile;

    [Header("보드 범위 (Cell 좌표)")]
    public Vector3Int boardMin = new Vector3Int(-5, -3, 0);
    public int boardWidth = 10;
    public int boardHeight = 10;

    [Header("블럭 크기 제한")]
    public int maxBlockWidth = 8;
    public int maxBlockHeight = 8;
    [Header("시간 설정")]
    public float intervalMin = 1.2f;
    public float intervalMax = 2.5f;
    public float obstacleDuration = 3f;

    [Header("깜빡임 설정")]
    public int blinkCount = 3;
    public float fadeDuration = 0.25f;

    [Header("난이도")]
    public int difficultyLevel = 1; // 1~5

    [Header("초콜릿 드랍")]
    public GameObject chocolatePrefab;
    [Range(0f, 1f)]
    public float chocolateDropChance = 0.6f;

    private List<Vector3Int[]> shapes = new List<Vector3Int[]>();
    private bool isRunning = false;

    [Header("Boss")]
    public BossAttackController boss;

    void Start()
    {
        ApplyObstacleThemeFromRunData();
    }

    private void ApplyObstacleThemeFromRunData()
    {
        // 기본값은 다크
        int choice = (RunData.I != null) ? RunData.I.choice1 : 1;

        bool isWhite = (choice == 0);

        chocolateObstacleTile = isWhite
            ? whiteChocolateObstacleTile
            : darkChocolateObstacleTile;

        Debug.Log($"[ObstacleTile] {(isWhite ? "WHITE" : "DARK")} selected");
    }


    // =================================================
    // 플레이어가 위험 블럭 위에 있는지 체크용
    // =================================================
    public bool IsDangerCell(Vector3 worldPos)
    {
        if (obstacleTilemap == null) return false;
        Vector3Int cell = obstacleTilemap.WorldToCell(worldPos);
        return obstacleTilemap.HasTile(cell);
    }

    void Awake()
    {
        InitShapes();
    }

    void OnEnable()
    {
        if (!isRunning)
            StartCoroutine(WarningLoop());

        if (warningTilemap != null)
            warningTilemap.color = Color.white;
    }

    // =================================================
    // 1. 패턴 초기화
    // =================================================
    void InitShapes()
    {
        shapes.Clear();
        Vector3Int V(int x, int y) => new Vector3Int(x, y, 0);

        int maxW = Mathf.Min(boardWidth, maxBlockWidth);
        int maxH = Mathf.Min(boardHeight, maxBlockHeight);


        int minW = Mathf.Clamp(4 + difficultyLevel, 4, maxW);
        int minH = Mathf.Clamp(3 + difficultyLevel / 2, 3, maxH);

        for (int h = minH; h <= maxH; h++)
        {
            for (int w = minW; w <= maxW; w++)
            {
                List<Vector3Int> rect = new();
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        rect.Add(V(x, y));
                shapes.Add(rect.ToArray());
            }
        }
    }

    // =================================================
    // 2. 반복 루프
    // =================================================
    IEnumerator WarningLoop()
    {
        isRunning = true;

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(intervalMin, intervalMax));
            SpawnWarningShape();
        }
    }

    // =================================================
    // 3. 패턴 생성
    // =================================================
    void SpawnWarningShape()
    {
        if (warningTilemap == null || warningTile == null) return;

        Vector3Int[] shape = shapes[Random.Range(0, shapes.Count)];

        int minOffX = 0, maxOffX = 0, minOffY = 0, maxOffY = 0;
        foreach (var o in shape)
        {
            minOffX = Mathf.Min(minOffX, o.x);
            maxOffX = Mathf.Max(maxOffX, o.x);
            minOffY = Mathf.Min(minOffY, o.y);
            maxOffY = Mathf.Max(maxOffY, o.y);
        }

        int minX = boardMin.x - minOffX;
        int maxX = boardMin.x + boardWidth - 1 - maxOffX;
        int minY = boardMin.y - minOffY;
        int maxY = boardMin.y + boardHeight - 1 - maxOffY;

        Vector3Int anchor = new Vector3Int(
            Random.Range(minX, maxX + 1),
            Random.Range(minY, maxY + 1),
            0
        );

        List<Vector3Int> cells = new();
        foreach (var o in shape)
            cells.Add(anchor + o);

        StartCoroutine(WarningCoroutine(cells));
    }


    // =================================================
    // 4. 경고 → 깜빡임 → 장애물 → 제거 + 초콜릿
    // =================================================
    IEnumerator WarningCoroutine(List<Vector3Int> cells)
    {   
        // 보스 공격 연출
        if (boss != null)
            boss.PlayAttack();

        // 1️⃣ 경고 타일 찍기
        foreach (var c in cells)
            warningTilemap.SetTile(c, warningTile);

        warningTilemap.color = Color.white;

        // 2️⃣ 깜빡임 (Fade)
        for (int i = 0; i < blinkCount; i++)
        {
            yield return FadeTilemap(1f, 0f, fadeDuration);
            yield return FadeTilemap(0f, 1f, fadeDuration);
        }

        // 3️⃣ 경고 제거
        foreach (var c in cells)
            warningTilemap.SetTile(c, null);

        warningTilemap.color = Color.white;

        // 4️⃣ 장애물 생성
        foreach (var c in cells)
            obstacleTilemap.SetTile(c, chocolateObstacleTile);

        // 5️⃣ 유지
        yield return new WaitForSeconds(obstacleDuration);

        // 6️⃣ 장애물 제거 + 초콜릿 드랍
        // ===== 6️⃣ 장애물 제거 =====
        foreach (var c in cells)
        {
            obstacleTilemap.SetTile(c, null);
        }

        // ===== 7️⃣ 초콜릿 1개만 랜덤 생성 =====
        if (chocolatePrefab != null && Random.value < chocolateDropChance)
        {
            // 이번 블럭 묶음 중 하나를 랜덤 선택
            Vector3Int dropCell = cells[Random.Range(0, cells.Count)];

            Vector3 pos = obstacleTilemap.GetCellCenterWorld(dropCell);

            // Z값 고정 (2D에서 매우 중요)
            pos.z = 0f;

            Instantiate(chocolatePrefab, pos, Quaternion.identity);


            Debug.Log("Chocolate Spawn at " + dropCell);
            Debug.Log($"Drop Cell: {dropCell} / World Pos: {pos}");

        }


    }

    // =================================================
    // 5. Tilemap Fade
    // =================================================
    IEnumerator FadeTilemap(float fromA, float toA, float duration)
    {
        float t = 0f;
        Color baseColor = warningTilemap.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(fromA, toA, t / duration);
            warningTilemap.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }

        warningTilemap.color = new Color(baseColor.r, baseColor.g, baseColor.b, toA);
    }
}
