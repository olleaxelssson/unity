using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Identity")]
    public string spawnID;
    public bool   isDefault = false;

    void Start()
    {
        TrySpawnPlayer();
    }

    void TrySpawnPlayer()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        string pendingID = SceneTransitionManager.PendingSpawnID;

        bool shouldSpawnHere = (!string.IsNullOrEmpty(pendingID) && pendingID == spawnID)
                            || (string.IsNullOrEmpty(pendingID) && isDefault);

        if (shouldSpawnHere)
        {
            player.TeleportTo(transform.position);
            Debug.Log($"[SpawnPoint] Spawned player at: {spawnID}");

            // Tell all portals in this scene to ignore the player briefly
            foreach (var portal in FindObjectsByType<Portal>(FindObjectsSortMode.None))
                portal.StartIgnoreCooldown();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isDefault ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.8f);
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.9f,
            isDefault ? $"DEFAULT\n{spawnID}" : spawnID);
        #endif
    }
}