using UnityEngine;

public class EndingInteractable : MonoBehaviour
{
    [Header("연출 대상")]
    public GameObject endingRoot;        // 엔딩 연출 전체 묶음
    public SpriteRenderer imageRenderer; // 이미지 엔딩용
    public Animator animator;            // 애니메이션 엔딩용

    [Header("엔딩 이미지 (0~7)")]
    public Sprite[] endingSprites = new Sprite[8];

    [Header("엔딩 애니메이션 트리거 (0~7)")]
    public string[] endingAnimTriggers = new string[8];

    private bool used = false;

    void Update()
    {
        if (used) return;

        // Space 눌렀을 때
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayEnding();
            used = true;
        }
    }

    void PlayEnding()
    {
        if (RunData.I == null)
        {
            Debug.LogError("RunData 없음!");
            return;
        }

        int endingId = RunData.I.EndingId(); // ⭐ 0~7 자동 계산됨
        Debug.Log($"[ENDING] id = {endingId}");

        if (endingRoot != null)
            endingRoot.SetActive(true);

        // 1️⃣ 이미지 연출
        if (imageRenderer != null &&
            endingSprites != null &&
            endingId < endingSprites.Length &&
            endingSprites[endingId] != null)
        {
            imageRenderer.sprite = endingSprites[endingId];
        }

        // 2️⃣ 애니메이션 연출
        if (animator != null &&
            endingAnimTriggers != null &&
            endingId < endingAnimTriggers.Length &&
            !string.IsNullOrEmpty(endingAnimTriggers[endingId]))
        {
            animator.SetTrigger(endingAnimTriggers[endingId]);
        }
    }
}
