using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WarningManagerStage2 : MonoBehaviour
{
    [Header("타일맵 참조")]
    public Tilemap baseTilemap;      // 체스판 바닥 타일맵 (보라색 체스 무늬)
    public Tilemap warningTilemap;   // 깜빡이는 타일/장애물 타일을 올릴 타일맵 (OverlayTilemap)

    [Header("타일 설정")]
    public TileBase warningTile;         // 깜빡일 때 보여줄 타일
    public TileBase chocolateObstacleTile; // 깜빡임 끝나고 남길 초콜릿 장애물 타일 (나중에 넣어도 됨)

    [Header("보드 정보 (내부만)")]
    public Vector3Int boardMin = new Vector3Int(-5, -3, 0);  // 보드 안쪽 왼쪽 아래 칸 셀 좌표
    public int boardWidth = 10;   // 안쪽 가로 칸 수
    public int boardHeight = 10;  // 안쪽 세로 칸 수

    [Header("시간 설정")]
    public float intervalMin = 1.5f;     // 다음 공격까지 최소 대기 시간
    public float intervalMax = 3.0f;     // 다음 공격까지 최대 대기 시간
    public float warningDuration = 1.5f; // 깜빡이는 총 시간
    public float blinkInterval = 0.25f;  // 깜빡이는 간격

    // 내부용
    private readonly List<Vector3Int[]> shapes = new List<Vector3Int[]>();
    private bool isRunning = false;

    void Awake()
    {
        InitShapes();   // 테트리스 느낌 모양들 미리 등록
    }

    void OnEnable()
    {
        if (!isRunning)
        {
            StartCoroutine(WarningLoop());
        }
    }

    // ───────────────────────────────────────
    // 1. 테트리스 같은 모양 정의
    // ───────────────────────────────────────
    void InitShapes()
    {
        shapes.Clear();

        // helper
        Vector3Int V(int x, int y) => new Vector3Int(x, y, 0);

        // I 모양 (4칸) - 세로
        shapes.Add(new[]
        {
            V(0, 0), V(0, 1), V(0, 2), V(0, 3)
        });

        // I 모양 (4칸) - 가로
        shapes.Add(new[]
        {
            V(0, 0), V(1, 0), V(2, 0), V(3, 0)
        });

        // 3칸짜리 막대 - 세로
        shapes.Add(new[]
        {
            V(0, 0), V(0, 1), V(0, 2)
        });

        // 3칸짜리 막대 - 가로
        shapes.Add(new[]
        {
            V(0, 0), V(1, 0), V(2, 0)
        });

        // L 모양 (ㄴ자)
        shapes.Add(new[]
        {
            V(0, 0), V(0, 1), V(0, 2), V(1, 0)
        });

        shapes.Add(new[]
        {
            V(0, 0), V(1, 0), V(2, 0), V(0, 1)
        });

        // 2x2 네모 (정사각형)
        shapes.Add(new[]
        {
            V(0, 0), V(1, 0), V(0, 1), V(1, 1)
        });
    }

    // ───────────────────────────────────────
    // 2. 무한 루프: 일정 시간마다 공격 패턴 실행
    // ───────────────────────────────────────
    IEnumerator WarningLoop()
    {
        isRunning = true;

        while (true)
        {
            float wait = Random.Range(intervalMin, intervalMax);
            yield return new WaitForSeconds(wait);

            SpawnWarningShape();
        }
    }

    // ───────────────────────────────────────
    // 3. 보드 안 랜덤 위치에 모양 하나 생성
    // ───────────────────────────────────────
    void SpawnWarningShape()
    {
        if (warningTilemap == null || warningTile == null)
        {
            Debug.LogWarning("[WarningManagerStage2] WarningTilemap 또는 WarningTile이 비어 있습니다.");
            return;
        }

        if (shapes.Count == 0)
        {
            Debug.LogWarning("[WarningManagerStage2] 등록된 패턴이 없습니다.");
            return;
        }

        // 1) 패턴 하나 랜덤 선택
        Vector3Int[] shape = shapes[Random.Range(0, shapes.Count)];

        // 2) 이 패턴의 오프셋 범위 계산 (최소/최대 x,y)
        int minOffX = 0, maxOffX = 0, minOffY = 0, maxOffY = 0;
        foreach (var o in shape)
        {
            if (o.x < minOffX) minOffX = o.x;
            if (o.x > maxOffX) maxOffX = o.x;
            if (o.y < minOffY) minOffY = o.y;
            if (o.y > maxOffY) maxOffY = o.y;
        }

        // 3) 보드 안에서 패턴 전체가 들어갈 수 있는 앵커(기준 칸) 범위 계산
        int minAnchorX = boardMin.x - minOffX;
        int maxAnchorX = boardMin.x + boardWidth - 1 - maxOffX;
        int minAnchorY = boardMin.y - minOffY;
        int maxAnchorY = boardMin.y + boardHeight - 1 - maxOffY;

        if (minAnchorX > maxAnchorX || minAnchorY > maxAnchorY)
        {
            Debug.LogWarning("[WarningManagerStage2] 보드가 너무 작아서 이 패턴을 배치할 수 없습니다.");
            return;
        }

        // 4) 앵커 좌표 랜덤 선택
        Vector3Int anchor = new Vector3Int(
            Random.Range(minAnchorX, maxAnchorX + 1),
            Random.Range(minAnchorY, maxAnchorY + 1),
            0
        );

        // 5) 실제 셀 좌표 목록 만들기
        List<Vector3Int> cells = new List<Vector3Int>();
        foreach (var o in shape)
        {
            cells.Add(anchor + o);
        }

        // 6) 깜빡이는 코루틴 시작
        StartCoroutine(WarningCoroutine(cells));
    }

    // ───────────────────────────────────────
    // 4. 해당 셀들 깜빡이다가 최종적으로 장애물/삭제
    // ───────────────────────────────────────
    IEnumerator WarningCoroutine(List<Vector3Int> cells)
    {
        float elapsed = 0f;
        bool visible = false;

        while (elapsed < warningDuration)
        {
            visible = !visible;

            foreach (var cell in cells)
            {
                warningTilemap.SetTile(cell, visible ? warningTile : null);
            }

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        // 마지막에 초콜릿 장애물로 바꾸거나, 없애기
        foreach (var cell in cells)
        {
            if (chocolateObstacleTile != null)
                warningTilemap.SetTile(cell, chocolateObstacleTile);
            else
                warningTilemap.SetTile(cell, null);
        }
    }
}
