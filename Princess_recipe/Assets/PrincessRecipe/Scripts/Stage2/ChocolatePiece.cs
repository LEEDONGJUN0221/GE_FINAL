using UnityEngine;

public class ChocolatePiece : MonoBehaviour
{
    public int value = 1;
    private bool collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        
        Destroy(gameObject);
    }
}
