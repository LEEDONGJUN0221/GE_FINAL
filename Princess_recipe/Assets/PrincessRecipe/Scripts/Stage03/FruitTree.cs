using System.Collections;
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

    [Header("Disappear Effect")]
    public float disappearDuration = 0.5f; // 페이드 시간(추천 0.3~0.6)
    public bool disableColliderImmediately = true;

    private bool isDisappearing = false;

    // 플레이어가 범위 안에 있는 상태에서 사라질 수 있으므로 참조 보관
    private PlayerInteractor currentInteractor;
    private Collider2D triggerCol;

    // 페이드를 위해 트리 아래의 SpriteRenderer들을 잡아둠(자식 포함)
    private SpriteRenderer[] renderers;

    private void Awake()
    {
        triggerCol = GetComponent<Collider2D>();
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void Start()
    {
        currentFruit = Mathf.Clamp(currentFruit, 0, maxFruit);
        RefreshVisual();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDisappearing) return;
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
        if (isDisappearing) return false;
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

        // ✅ 0개가 되면 페이드 후 제거
        if (currentFruit <= 0)
        {
            StartCoroutine(CoDisappear());
        }

        return true;
    }

    private IEnumerator CoDisappear()
    {
        isDisappearing = true;

        // 1) 상호작용 대상 해제
        if (currentInteractor != null)
        {
            currentInteractor.ClearCurrentTree(this);
            currentInteractor = null;
        }

        // 2) 콜라이더 비활성화(사라지는 중 추가 상호작용 방지)
        if (disableColliderImmediately && triggerCol != null)
            triggerCol.enabled = false;

        // 3) 페이드아웃
        float t = 0f;

        // 각 렌더러의 원래 색 저장(알파만 건드릴거라 RGB는 유지)
        Color[] original = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            original[i] = renderers[i].color;
        }

        while (t < disappearDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / disappearDuration);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                Color c = original[i];
                c.a = a;
                renderers[i].color = c;
            }

            yield return null;
        }

        // 4) 최종 제거
        Destroy(gameObject);
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
