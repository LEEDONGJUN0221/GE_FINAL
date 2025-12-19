using UnityEngine;

public class FruitTree : MonoBehaviour
{
    [Header("Tree ID (0~3)")]
    public int treeId = 0;

    [Header("Fruit Stock")]
    public int maxFruit = 4;
    public int currentFruit = 4;

    [Header("Optional Visuals (4개면 권장)")]
    public GameObject[] fruitVisuals;

    // =======================
    // NEW: Empty Tree Artwork
    // =======================
    [Header("Empty Tree Artwork")]
    [Tooltip("열매가 0개가 되면 이 스프라이트로 교체됩니다.")]
    public Sprite emptyTreeSprite;

    [Tooltip("트리 본체 스프라이트가 붙어있는 SpriteRenderer (비우면 자동으로 GetComponent로 찾습니다)")]
    public SpriteRenderer treeRenderer;

    [Tooltip("열매 0개가 되면 상호작용(수확) 불가능하게 만들지 여부")]
    public bool disableInteractionWhenEmpty = true;

    [Tooltip("열매 0개가 되면 트리 Trigger Collider를 끌지 여부(추천: true)")]
    public bool disableTriggerColliderWhenEmpty = true;

    private bool isEmptyApplied = false;

    // 플레이어가 범위 안에 있는 상태에서 상태가 바뀔 수 있으므로 참조 보관
    private PlayerInteractor currentInteractor;
    private Collider2D triggerCol;

    private void Awake()
    {
        triggerCol = GetComponent<Collider2D>();

        if (treeRenderer == null)
            treeRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        currentFruit = Mathf.Clamp(currentFruit, 0, maxFruit);
        RefreshVisual();

        if (currentFruit <= 0)
            ApplyEmptyState(); // 시작부터 0개일 수도 있으니 안전 처리
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (disableInteractionWhenEmpty && currentFruit <= 0) return;
        if (!other.CompareTag("Player")) return;

        currentInteractor = other.GetComponent<PlayerInteractor>();
        if (currentInteractor != null)
            currentInteractor.SetCurrentTree(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerInteractor interactor = other.GetComponent<PlayerInteractor>();
        if (interactor != null)
            interactor.ClearCurrentTree(this);

        if (currentInteractor == interactor)
            currentInteractor = null;
    }

    // 플레이어가 Space 눌렀을 때 호출
    public bool TryCollectOne()
    {
        if (disableInteractionWhenEmpty && currentFruit <= 0) return false;
        if (currentFruit <= 0) return false;

        // 직전 나무 금지 규칙
        if (Stage4GameManager.Instance != null)
        {
            if (!Stage4GameManager.Instance.CanCollectFrom(treeId))
                return false;
        }

        currentFruit--;
        RefreshVisual();

        if (Stage4GameManager.Instance != null)
        {
            Stage4GameManager.Instance.OnFruitCollected();
            Stage4GameManager.Instance.OnCollectedFromTree(treeId);
        }

        // ✅ 0개가 되면 페이드/삭제 대신 “빈 나무 아트웍으로 교체”
        if (currentFruit <= 0)
        {
            ApplyEmptyState();
        }

        return true;
    }

    private void ApplyEmptyState()
    {
        if (isEmptyApplied) return;
        isEmptyApplied = true;

        // 1) 상호작용 대상 해제
        if (currentInteractor != null)
        {
            currentInteractor.ClearCurrentTree(this);
            currentInteractor = null;
        }

        // 2) 트리 스프라이트 교체
        if (treeRenderer != null && emptyTreeSprite != null)
        {
            treeRenderer.sprite = emptyTreeSprite;
        }

        // 3) Trigger Collider 끄기(더 이상 플레이어가 들어와도 currentTree가 잡히지 않게)
        if (disableTriggerColliderWhenEmpty && triggerCol != null)
        {
            triggerCol.enabled = false;
        }
    }

    private void RefreshVisual()
    {
        if (fruitVisuals == null || fruitVisuals.Length == 0) return;

        for (int i = 0; i < fruitVisuals.Length; i++)
        {
            if (fruitVisuals[i] == null) continue;
            fruitVisuals[i].SetActive(i < currentFruit);
        }
    }
}
