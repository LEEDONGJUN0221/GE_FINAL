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
    [Tooltip("계란 획득 시 재생할 사운드")]
    public AudioClip eggGetSound;
    [Tooltip("보스에게 달걀 전달 성공 시 재생할 사운드")]
    public AudioClip bossGiveEggSound;
    [Tooltip("몬스터에게 피격 시 재생할 사운드")]
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
        if (targetRenderer != null)
            spriteRenderer = targetRenderer;
        else
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Animator 설정
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // AudioSource 자동 세팅
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        respawnPosition = transform.position;

        gameManager = FindAnyObjectByType<GameManagerStage1>();

        UpdateEggSprite();
    }

    void Update()
    {
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
                spriteRenderer.color = new Color(1, 1, 1, 1);
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
                UpdateEggSprite();

                // ▶ 보스에게 달걀 전달 성공 사운드
                PlaySound(bossGiveEggSound);
            }
        }
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
                UpdateEggSprite();

                // ▶ 계란 획득 사운드
                PlaySound(eggGetSound);
            }
        }

        // 2. 몬스터 충돌
        if (collision.CompareTag("Stage1_Monster"))
        {
            if (isInvincible) return;

            // ▶ 몬스터 피격 사운드
            PlaySound(hitByMonsterSound);

            if (gameManager != null)
                gameManager.TakeDamage();

            if (currentEggs > minEggs)
            {
                currentEggs--;
                UpdateEggSprite();
            }

            StartInvincibility();
        }

        // 3. 보스 구역 진입 (❌ 효과음 제거됨)
        if (collision.CompareTag("Stage1_Boss"))
        {
            nearbyBoss = collision.GetComponent<BossController>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Stage1_Boss"))
        {
            nearbyBoss = null;
        }
    }

    // ---------------- 피격 무적 ----------------
    void StartInvincibility()
    {
        isInvincible = true;
        invincibleTimer = hitInvincibleTime;
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

        if (animator != null && !string.IsNullOrEmpty(hasEggBoolName))
            animator.SetBool(hasEggBoolName, hasEgg);
    }

    // ---------------- 공통 사운드 재생 함수 ----------------
    void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null)
            return;

        audioSource.PlayOneShot(clip, soundVolume);
    }
}
