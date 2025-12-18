using UnityEngine;
using System.Collections;

public class ChocolatePiece : MonoBehaviour
{
    [Header("Theme Sprites")]
    public Sprite whiteSprite;
    public Sprite darkSprite;

    [Header("Lifetime")]
    public float lifetime = 8f;

    [Header("Blink Before Destroy")]
    public int blinkCount = 3;
    public float blinkInterval = 0.1f;

    private SpriteRenderer sr;
    private bool collected = false;

    [Header("Sound")]
    public AudioClip pickupSound;


    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        ApplyThemeFromRunData();
        StartCoroutine(LifeRoutine());
    }

    // ===============================
    // Stage1 선택값 기반 테마 적용
    // ===============================
    void ApplyThemeFromRunData()
    {
        int choice = (RunData.I != null) ? RunData.I.choice1 : 1; // 기본 다크
        bool isWhite = (choice == 0);

        if (sr != null)
            sr.sprite = isWhite ? whiteSprite : darkSprite;
    }

    // ===============================
    // 수명 + 깜빡임
    // ===============================
    IEnumerator LifeRoutine()
    {
        float blinkTotalTime = blinkCount * blinkInterval * 2f;
        yield return new WaitForSeconds(lifetime - blinkTotalTime);

        for (int i = 0; i < blinkCount; i++)
        {
            sr.enabled = false;
            yield return new WaitForSeconds(blinkInterval);
            sr.enabled = true;
            yield return new WaitForSeconds(blinkInterval);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        Destroy(gameObject);
    }
}
