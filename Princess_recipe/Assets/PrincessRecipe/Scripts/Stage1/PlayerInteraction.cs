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
    public float hitInvincibleTime = 1.5f;
    private bool isInvincible = false;
    private float invincibleTimer = 0f;

    [Header("피격 깜빡임 설정")]
    public float blinkInterval = 0.1f;

    [Header("플레이어 스프라이트 설정")]
    public SpriteRenderer targetRenderer;

    // 🔥 애니메이션 설정
    [Header("애니메이션 설정")]
    public Animator animator;
    public string hasEggBoolName = "HasEgg";

    [Header("사운드 설정")]
    public AudioClip eggGetSound;
    public AudioClip bossGiveEggSound;
    public AudioClip hitByMonsterSound;
    [Range(0f, 1f)]
    public float soundVolume = 1f;
    private AudioSource audioSource;

    private BossController nearbyBoss = null;
    private GameManagerStage1 gameManager;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // SpriteRenderer 설정
        spriteRenderer = targetRenderer != null ?
            targetRenderer : GetComponentInChildren<SpriteRenderer>();

        // Animator 설정
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // AudioSource 설정
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        respawnPosition = transform.position;
        gameManager = FindAnyObjectByType<GameManagerStage1>();

        Debug.Log($"[Start] 플레이어 초기화 완료. 시작 위치: {respawnPosition}");

        UpdateEggSprite();
    }

    void Update()
    {
        // 무적 시간 체크
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
                spriteRenderer.color = new Color(1, 1, 1, 1);
                Debug.Log("[Invincible] 무적 시간 종료");
            }
        }

        // 스페이스로 보스에게 달걀 전달
        if (Input.GetKeyDown(KeyCode.Space) && nearbyBoss != null)
        {
            Debug.Log("[Boss] 스페이스 입력: 달걀 전달 시도");
            GiveEggToBoss();
        }
    }

    // ---------------- 달걀 전달 ----------------
    void GiveEggToBoss()
    {
        if (currentEggs > 0 && nearbyBoss != null)
        {
            Debug.Log($"[Boss] 보스에게 달걀 전달 시도 (현재 달걀: {currentEggs})");

            if (nearbyBoss.ReceiveEgg())
            {
                currentEggs--;
                UpdateEggSprite();

                Debug.Log($"[Boss] 달걀 전달 성공 (남은 달걀: {currentEggs})");

                PlaySound(bossGiveEggSound);
            }
            else
            {
                Debug.Log("[Boss] 보스가 달걀을 받지 않음");
            }
        }
        else
        {
            Debug.Log("[Boss] 달걀이 없어서 전달 실패");
        }
    }

    // ---------------- 충돌 처리 ----------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 달걀 획득
        if (collision.CompareTag("Stage1_Egg"))
        {
            Debug.Log("[Egg] 달걀과 충돌 감지");

            if (currentEggs < maxEggs)
            {
                currentEggs++;
                UpdateEggSprite();

                Debug.Log($"[Egg] 달걀 획득! 현재 달걀: {currentEggs}");

                PlaySound(eggGetSound);
            }
            else
            {
                Debug.Log("[Egg] 달걀 보유량 꽉 찼음. 획득 불가.");
            }
        }

        // 몬스터 충돌
        if (collision.CompareTag("Stage1_Monster"))
        {
            Debug.Log("[Hit] 몬스터와 충돌!");

            if (isInvincible)
            {
                Debug.Log("[Hit] 이미 무적 상태이므로 피해 없음");
                return;
            }

            PlaySound(hitByMonsterSound);

            if (gameManager != null)
                gameManager.TakeDamage();

            if (currentEggs > minEggs)
            {
                currentEggs--;
                UpdateEggSprite();
                Debug.Log($"[Hit] 피격! 달걀 하나 떨어뜨림. 현재 달걀: {currentEggs}");
            }
            else
            {
                Debug.Log("[Hit] 달걀이 없어 감소 없음");
            }

            StartInvincibility();
        }

        // 보스 범위 진입
        if (collision.CompareTag("Stage1_Boss"))
        {
            nearbyBoss = collision.GetComponent<BossController>();
            Debug.Log("[Boss] 보스 범위 진입");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Stage1_Boss"))
        {
            nearbyBoss = null;
            Debug.Log("[Boss] 보스 범위 이탈");
        }
    }

    // ---------------- 무적 ----------------
    void StartInvincibility()
    {
        isInvincible = true;
        invincibleTimer = hitInvincibleTime;

        Debug.Log($"[Invincible] 무적 시작 — {hitInvincibleTime}초 동안 피해 없음");

        StartCoroutine(HitBlink());
    }

    private IEnumerator HitBlink()
    {
        Debug.Log("[Invincible] 피격 깜빡임 코루틴 시작");

        while (isInvincible)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.3f);
            yield return new WaitForSeconds(blinkInterval);
            spriteRenderer.color = new Color(1, 1, 1, 1f);
            yield return new WaitForSeconds(blinkInterval);
        }

        spriteRenderer.color = new Color(1, 1, 1, 1);
        Debug.Log("[Invincible] 깜빡임 종료");
    }

    // ---------------- 달걀 애니메이션 반영 ----------------
    void UpdateEggSprite()
    {
        bool hasEgg = currentEggs > 0;

        if (animator != null && !string.IsNullOrEmpty(hasEggBoolName))
        {
            animator.SetBool(hasEggBoolName, hasEgg);
            Debug.Log($"[Animator] HasEgg = {hasEgg}");
        }
    }

    // ---------------- 사운드 재생 ----------------
    void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null)
        {
            Debug.LogWarning("[Audio] 재생 실패: 클립 또는 오디오 소스 없음");
            return;
        }

        audioSource.PlayOneShot(clip, soundVolume);
        Debug.Log($"[Audio] 사운드 재생: {clip.name}");
    }
}
