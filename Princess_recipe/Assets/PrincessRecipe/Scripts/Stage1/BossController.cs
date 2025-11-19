using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("보스 달걀 요구량")]
    public int requiredEggs = 6; // 👈 6개로 수정
    private int receivedEggs = 0; 
    private bool isDefeated = false; 

    // GameManagerStage1 참조를 저장할 변수
    private GameManagerStage1 gameManager; // 👈 타입 지정

    void Start()
    {
        // GameManagerStage1 인스턴스 찾기
        gameManager = FindObjectOfType<GameManagerStage1>(); // 👈 GameManagerStage1 찾기
        if (gameManager == null)
        {
            Debug.LogError("씬에서 GameManagerStage1를 찾을 수 없습니다! 보스 기능이 정상 작동하지 않습니다.");
        }
    }

    /// <summary>
    /// 플레이어로부터 달걀을 받는 메서드.
    /// </summary>
    public bool ReceiveEgg()
    {
        if (isDefeated)
        {
            Debug.Log("보스는 이미 격퇴되었습니다.");
            return false;
        }

        receivedEggs++;
        
        // 1. 점수와 연결: GameManagerStage1에 점수 추가를 요청합니다.
        if (gameManager != null)
        {
            gameManager.AddScore(1); // 👈 점수 증가 로직 추가
        }
        else
        {
            Debug.LogError("GameManagerStage1이 연결되지 않아 점수를 추가할 수 없습니다.");
        }

        Debug.Log($"보스가 달걀을 받았습니다. 현재 {receivedEggs} / {requiredEggs}");

        if (receivedEggs >= requiredEggs)
        {
            DefeatBoss();
        }

        return true;
    }

    /// <summary>
    /// 보스가 격퇴되었을 때의 처리.
    /// </summary>
    void DefeatBoss()
    {
        isDefeated = true;
        Debug.Log("🎉 보스 격퇴! 스테이지 클리어!");
        
        // 2. GameManagerStage1에 게임 클리어 처리를 요청합니다.
        if (gameManager != null)
        {
            gameManager.GameClear(); // 👈 게임 클리어 호출
        }
        
        // 보스 오브젝트 비활성화 또는 파괴 로직 추가
        gameObject.SetActive(false);
    }
}