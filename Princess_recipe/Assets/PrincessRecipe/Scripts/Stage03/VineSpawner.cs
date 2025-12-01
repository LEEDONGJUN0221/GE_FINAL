using System.Collections;
using UnityEngine;

public class VineSpawner : MonoBehaviour
{
    [Header("덩굴 프리팹")]
    public GameObject vinePrefab;

    [Header("스폰 위치 5개 (Inspector에서 할당)")]
    public Transform[] spawnPoints;   // Size = 5

    [Header("공격 간격 (초)")]
    public float spawnInterval = 5f;  // 5초마다

    [Header("덩굴 방향 (모든 구역 동일)")]
    public Vector2 defaultDirection = Vector2.left; // 왼쪽으로 찌르기

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnRandomVine();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnRandomVine()
    {
        if (vinePrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("VineSpawner: 프리팹 또는 스폰 포인트가 비어있습니다.", this);
            return;
        }

        int index = Random.Range(0, spawnPoints.Length);
        Transform point = spawnPoints[index];

        GameObject vineObj = Instantiate(vinePrefab, point.position, Quaternion.identity);

        VineAttackTopDown vine = vineObj.GetComponent<VineAttackTopDown>();
        if (vine != null)
        {
            // 1) 모든 구역 동일하게 왼쪽으로 공격
            vine.SetDirection(defaultDirection);

            // 2) 구역마다 방향 다르게 하고 싶으면 (각 SpawnPoint의 up 방향 기준):
            // vine.SetDirection(point.up);
        }
    }
}
