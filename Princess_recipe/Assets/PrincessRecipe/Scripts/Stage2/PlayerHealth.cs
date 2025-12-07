using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHP = 3;
    public int currentHP;

    void Start()
    {
        currentHP = maxHP;
        Debug.Log($"[PlayerHealth] 시작 HP = {currentHP}");
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        Debug.Log($"[PlayerHealth] 데미지 {dmg} → 현재 HP = {currentHP}");

        if (currentHP <= 0)
        {
            Debug.Log("[PlayerHealth] 플레이어 사망");
            // TODO: 리스폰 / 게임오버 처리
        }
    }
}
