using UnityEngine;

public class EndingTrigger : MonoBehaviour
{
    public EndingSequenceUI endingUI;
    public KeyCode interactKey = KeyCode.Space;

    private bool playerInRange = false;
    private bool used = false;

    void Update()
    {
        if (used || !playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            endingUI.StartSequence();
            used = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
