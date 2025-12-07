using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageOnTouch : MonoBehaviour
{
    public int damage = 1;
    public bool useTrigger = false;   // 타일맵 콜라이더 IsTrigger 쓰면 체크

    void OnCollisionEnter2D(Collision2D other)
    {
        if (useTrigger) return;  // 트리거 모드면 충돌은 무시

        Debug.Log($"{name} OnCollisionEnter2D -> {other.gameObject.name}");
        TryDamage(other.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTrigger) return; // 충돌 모드면 트리거는 무시

        Debug.Log($"{name} OnTriggerEnter2D -> {other.gameObject.name}");
        TryDamage(other.gameObject);
    }

    void TryDamage(GameObject target)
    {
        // 자식/부모 어디에 PlayerHealth가 있어도 찾게
        var hp = target.GetComponent<PlayerHealth>() 
                 ?? target.GetComponentInParent<PlayerHealth>() 
                 ?? target.GetComponentInChildren<PlayerHealth>();

        if (hp != null)
        {
            Debug.Log($"→ {target.name} 에게 {damage} 데미지");
            hp.TakeDamage(damage);
        }
        else
        {
            Debug.Log($"⚠ {target.name} 에 PlayerHealth 없음");
        }
    }
}
