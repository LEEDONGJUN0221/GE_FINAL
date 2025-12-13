using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interact Key")]
    public KeyCode interactKey = KeyCode.Space;

    [Header("Debug")]
    public bool showDebugLog = false;

    private FruitTree currentTree;

    private void Update()
    {
        if (currentTree == null) return;

        if (Input.GetKeyDown(interactKey))
        {
            bool success = currentTree.TryCollectOne();

            if (showDebugLog)
                Debug.Log($"[Interact] Tree={currentTree.name}, success={success}, left={currentTree.currentFruit}");
        }
    }

    public void SetCurrentTree(FruitTree tree)
    {
        currentTree = tree;
    }

    public void ClearCurrentTree(FruitTree tree)
    {
        if (currentTree == tree)
            currentTree = null;
    }
}
