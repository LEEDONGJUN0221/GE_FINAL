using UnityEngine;

public class ChocolatePickup : MonoBehaviour
{
    public int value = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("🍫 Chocolate Picked!");

        GameManagerStage2.Instance.AddChocolate(value);
        Destroy(gameObject);
    }
}
