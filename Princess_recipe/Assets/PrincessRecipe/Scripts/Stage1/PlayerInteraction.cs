using System.Collections;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("리스폰 설정")]
    public Vector3 respawnPosition = new Vector3(0, 0, 0);
    private Rigidbody2D rb;

    [Header("달걀 설정")]
    public int currentEggs = 0;
    public int maxEggs = 1;
    public int minEggs = 0;

    [Header("피격 무적 설정")]
    [Tooltip("몬스터에 맞은 후 무적 유지 시간(초)")]
    public float hitInvincibleTime = 1.5f;
    private bool isInvincible = false;
    private float invincibleTimer = 0f;

    [Header("피격 깜빡임 설정")]
    [Tooltip("플레이어가 깜빡이는 속도(초)")]
    public float blinkInterval = 0.1f;

    [Header("플레이어 스프라이트 설정")]
    [Tooltip("플레이어 스프라이트가 자식에 있다면 여기 직접 넣어주세요.")]
    public SpriteRenderer targetRenderer;

    // 🔥 애니메이션 설정 (계란 보유 여부에 따라 상태 전환)
    [Header("애니메이션 설정")]
    [Tooltip("플레이어 Animator (계란 보유 애니메이션 전환용)")]
    public Animator animator;
    [Tooltip("계란 보유 여부 Bool 파라미터 이름")]
    public string hasEggBoolName = "HasEgg";

    private BossController nearbyBoss = null;
    private GameManagerStage1 gameManager;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // SpriteRenderer 찾기 (직접 지정 > 자식에서 찾기 > 자기 자신)
        if (targetRenderer != null)
            spriteRenderer = targetRenderer;
        else
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("PlayerInteraction: SpriteRenderer를 찾을 수 없습니다.", this);
        }

        // Animator 자동 찾기 (인스펙터에서 안 넣어도 되게)
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("PlayerInteraction: Animator를 찾지 못했습니다. HasEgg 애니메이션은 동작하지 않습니다.", this);
            }
        }

        respawnPosition = transform.position;

        gameManager = FindAnyObjectByType<GameManagerStage1>();
        if (gameManager == null)
        {
            Debug.LogError("GameManagerStage1을 씬에서 찾을 수 없습니다.");
        }

        if (hitInvincibleTime < 0.2f)
            hitInvincibleTime = 0.5f;

        // 시작할 때 계란 상태에 맞게 애니메이션 세팅
        UpdateEggSprite();   // 👉 이름은 그대로 두고, 내부에서 Animator만 제어
    }

    void Update()
    {
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = new Color(1, 1, 1, 1);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && nearbyBoss != null)
        {
            GiveEggToBoss();
        }
    }

    // ---------------- 보스에게 달걀 전달 ----------------
    void GiveEggToBoss()
    {
        if (currentEggs > 0 && nearbyBoss != null)
        {
            if (nearbyBoss.ReceiveEgg())
            {
                currentEggs--;
                Debug.Log("보스에게 달걀 전달 성공! 현재: " + currentEggs);
                UpdateEggSprite();   // 계란 수 변경 → 애니메이션 상태 갱신
            }
        }
        else if (currentEggs <= 0)
        {
            Debug.Log("전달할 달걀이 없습니다.");
        }
    }

    // ---------------- (선택적) 리스폰 함수 ----------------
    void Respawn()
    {
        transform.position = respawnPosition;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Debug.Log("리스폰 위치(" + respawnPosition + ")로 이동했습니다.");
    }

    // ---------------- 충돌 처리 ----------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 달걀 획득
        if (collision.CompareTag("Stage1_Egg"))
        {
            if (currentEggs < maxEggs)
            {
                currentEggs++;
                Debug.Log("달걀 획득! 현재: " + currentEggs);
                UpdateEggSprite();   // 계란 상태 변경 → 애니메이션 갱신
            }
            else
            {
                Debug.Log("달걀 최대 보유량 도달!");
            }

            // 필요하면 실제 달걀 오브젝트 제거
            // Destroy(collision.gameObject);
        }

        // 2. 몬스터 충돌
        if (collision.CompareTag("Stage1_Monster"))
        {
            if (isInvincible)
            {
                Debug.Log("무적 상태라 몬스터 충돌 무시");
                return;
            }

            Debug.Log("몬스터에게 피격!");

            if (gameManager != null)
                gameManager.TakeDamage();

            if (currentEggs > minEggs)
            {
                currentEggs--;
                Debug.Log("몬스터와 충돌! 달걀 1개 잃음. 현재: " + currentEggs);
                UpdateEggSprite();   // 계란 수 감소 → 애니메이션 갱신
            }
            else
            {
                Debug.Log("몬스터와 충돌했지만 가지고 있는 달걀이 없습니다.");
            }

            StartInvincibility();
        }

        // 3. 보스 구역 진입
        if (collision.CompareTag("Stage1_Boss"))
        {
            nearbyBoss = collision.GetComponent<BossController>();
            if (nearbyBoss != null)
                Debug.Log("보스 근처에 진입했습니다. Space 키로 달걀 전달 가능.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Stage1_Boss") && nearbyBoss != null)
        {
            nearbyBoss = null;
            Debug.Log("보스 구역에서 벗어났습니다.");
        }
    }

    // ---------------- 피격 무적 + 깜빡임 ----------------
    void StartInvincibility()
    {
        isInvincible = true;
        invincibleTimer = hitInvincibleTime;

        if (spriteRenderer != null)
            StartCoroutine(HitBlink());
    }

    private IEnumerator HitBlink()
    {
        while (isInvincible)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.3f);
            yield return new WaitForSeconds(blinkInterval);

            spriteRenderer.color = new Color(1, 1, 1, 1f);
            yield return new WaitForSeconds(blinkInterval);
        }

        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    // ---------------- 계란 상태에 따른 애니메이션 변경 ----------------
    void UpdateEggSprite()
    {
        bool hasEgg = currentEggs > 0;

        // 🔥 Animator에 계란 보유 상태 전달 (애니메이션/스프라이트 전환은 Animator에서 처리)
        if (animator != null && !string.IsNullOrEmpty(hasEggBoolName))
        {
            animator.SetBool(hasEggBoolName, hasEgg);
        }

        // 👉 만약 나중에 코드로도 스프라이트 바꾸고 싶으면 아래 주석 해제해서 사용하면 됨
        /*
        if (spriteRenderer == null)
            return;

        if (hasEgg)
        {
            if (eggHoldingSprite != null)
                spriteRenderer.sprite = eggHoldingSprite;
        }
        else
        {
            if (normalSprite != null)
                spriteRenderer.sprite = normalSprite;
        }
        */
    }
}
