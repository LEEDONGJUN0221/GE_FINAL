using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WarningManagerStage2 : MonoBehaviour
{
    [Header("타일맵")]
    public Tilemap baseTilemap;       // 바닥
    public Tilemap warningTilemap;    // 깜빡임 전용
    public Tilemap obstacleTilemap;   // 장애물 전용(★ 새로 만든 Tilemap)

    [Header("타일")]
    public TileBase warningTile;            // 경고 타일
    public TileBase chocolateObstacleTile;  // 유지될 장애물 타일

    [Header("보드 범위 (Cell 좌표)")]
    public Vector3Int boardMin = new Vector3Int(-5, -3, 0);
    public int boardWidth = 10;
    public int boardHeight = 10;

    [Header("시간 설정")]
    public float intervalMin = 1.2f;
    public float intervalMax = 2.5f;
    public float obstacleDuration = 3f;    // 장애물 유지 시간

    [Header("깜빡임 설정")]
    public int blinkCount = 3;        // 깜빡임 횟수
    public float fadeDuration = 0.15f; // Fade In/out 속도

    private List<Vector3Int[]> shapes = new List<Vector3Int[]>();
    private bool isRunning = false;

    void Awake()
    {
        InitShapes();
    }

    void OnEnable()
    {
        if (!isRunning)
            StartCoroutine(WarningLoop());
    }

    // =============================================================
    // 1. 패턴 초기화 (작은+큰 테트리스 모양 포함)
    // =============================================================
    void InitShapes()
    {
        shapes.Clear();

        Vector3Int V(int x, int y) => new Vector3Int(x, y, 0);

        // 기본 패턴들
        shapes.Add(new[] { V(0,0), V(1,0), V(2,0) });
        shapes.Add(new[] { V(0,0), V(0,1), V(0,2) });
        shapes.Add(new[] { V(0,0), V(1,0), V(0,1), V(1,1) });
        shapes.Add(new[] { V(0,0), V(0,1), V(0,2), V(1,0) });
        shapes.Add(new[] { V(0,0), V(1,0), V(2,0), V(0,1) });

        // 큰 직사각형 자동 생성
        int maxW = 10;
        int maxH = 3;

        for (int h = 1; h <= maxH; h++)
        {
            for (int w = 3; w <= maxW; w++)
            {
                List<Vector3Int> rect = new();
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        rect.Add(V(x, y));
                shapes.Add(rect.ToArray());
            }
        }

        for (int w = 1; w <= maxH; w++)
        {
            for (int h = 3; h <= maxW; h++)
            {
                List<Vector3Int> rect = new();
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        rect.Add(V(x, y));
                shapes.Add(rect.ToArray());
            }
        }
    }

    // =============================================================
    // 2. 경고 공격 반복 루프
    // =============================================================
    IEnumerator WarningLoop()
    {
        isRunning = true;

        while (true)
        {
            float wait = Random.Range(intervalMin, intervalMax);
            yield return new WaitForSeconds(wait);

            // 병렬로 계속 새 패턴 생성
            SpawnWarningShape();
        }
    }



    // =============================================================
    // 3. 패턴 위치 결정 + 경고 시작
    // =============================================================
    void SpawnWarningShape()
    {
        if (warningTilemap == null || warningTile == null)
        {
            Debug.LogWarning("[Stage2] WarningTilemap 또는 WarningTile 누락");
            return;
        }

        Vector3Int[] shape = shapes[Random.Range(0, shapes.Count)];

        int minOffX = 0, maxOffX = 0, minOffY = 0, maxOffY = 0;
        foreach (var o in shape)
        {
            if (o.x < minOffX) minOffX = o.x;
            if (o.x > maxOffX) maxOffX = o.x;
            if (o.y < minOffY) minOffY = o.y;
            if (o.y > maxOffY) maxOffY = o.y;
        }

        int minAnchorX = boardMin.x - minOffX;
        int maxAnchorX = boardMin.x + boardWidth - 1 - maxOffX;
        int minAnchorY = boardMin.y - minOffY;
        int maxAnchorY = boardMin.y + boardHeight - 1 - maxOffY;

        Vector3Int anchor = new Vector3Int(
            Random.Range(minAnchorX, maxAnchorX + 1),
            Random.Range(minAnchorY, maxAnchorY + 1),
            0
        );

        List<Vector3Int> cells = new();
        foreach (var o in shape)
            cells.Add(anchor + o);

        StartCoroutine(WarningCoroutine(cells));
    }

    // =============================================================
    // 4. 깜빡임(Fade) → 장애물 유지 → 제거
    // =============================================================
    IEnumerator WarningCoroutine(List<Vector3Int> cells)
    {
        // 경고 타일만 WarningTilemap에 찍음
        foreach (var c in cells)
            warningTilemap.SetTile(c, warningTile);

        // Fade 깜빡임 N회
        for (int i = 0; i < blinkCount; i++)
        {
            yield return FadeTilemap(0f, 1f, fadeDuration); // In
            yield return FadeTilemap(1f, 0f, fadeDuration); // Out
        }

        // Fade가 warningTilemap 알파를 0으로 남기므로 반드시 복구!
        warningTilemap.color = new Color(1f, 1f, 1f, 1f);

        // 경고 타일 제거
        foreach (var c in cells)
            warningTilemap.SetTile(c, null);

        // 장애물 생성 (⚠️ ObstacleTilemap에만!)
        foreach (var c in cells)
            obstacleTilemap.SetTile(c, chocolateObstacleTile);

        // 유지 시간
        yield return new WaitForSeconds(obstacleDuration);

        // 장애물 제거
        foreach (var c in cells)
            obstacleTilemap.SetTile(c, null);
    }

    // =============================================================
    // 5. Fade In/Out (WarningTilemap 전용)
    // =============================================================
    IEnumerator FadeTilemap(float fromA, float toA, float duration)
    {
        float t = 0f;
        Color baseColor = warningTilemap.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(fromA, toA, t / duration);

            warningTilemap.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

            yield return null;
        }

        warningTilemap.color = new Color(baseColor.r, baseColor.g, baseColor.b, toA);
    }
}
