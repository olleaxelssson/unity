using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to any resource node GameObject (tree, rock, enemy, scavenge pile).
///
/// Setup:
///  1. Create a GameObject for the resource (Sprite + this script)
///  2. Add a CircleCollider2D (set as Trigger) for click detection
///  3. Create a ResourceData ScriptableObject and assign it here
///  4. Place multiple resource nodes around your level
/// </summary>
public class ResourceNode : MonoBehaviour
{
    [Header("Data")]
    public ResourceData data;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color idleColor     = Color.white;
    public Color targetedColor = new Color(1f, 0.85f, 0.3f); // gold tint when targeted
    public Color hitColor      = new Color(1f, 0.4f,  0.4f); // red flash on hit

    // ── Private state ──────────────────────────────
    private bool   isTargeted   = false;
    private bool   isGathering  = false;
    private float  cooldownTimer = 0f;
    private Camera mainCam;

    // Reference to the player
    private PlayerController player;
    private PlayerInventory  inventory;

    // Floating text pool (optional)
    public GameObject floatingTextPrefab; // assign a prefab with TMP if you want +Wood popups

    void Awake()
    {
        mainCam   = Camera.main;
        player    = FindFirstObjectByType<PlayerController>();
        inventory = PlayerInventory.Instance;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (data != null && spriteRenderer != null && data.resourceSprite != null)
            spriteRenderer.sprite = data.resourceSprite;
    }

    void Update()
    {
        HandleClick();

        if (isTargeted)
            UpdateGathering();
    }

    // ── Click Detection ────────────────────────────

    void HandleClick()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        // Raycast to see if this node was clicked
        Vector2 mouseWorld = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

        if (hit != null && hit.gameObject == gameObject)
        {
            TargetThis();
        }
        else if (isTargeted)
        {
            // Clicked somewhere else — stop gathering
            Untarget();
        }
    }

    void TargetThis()
    {
        // Untarget all other nodes first
        foreach (var node in FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))
            if (node != this) node.Untarget();

        isTargeted = true;
        spriteRenderer.color = targetedColor;
        Debug.Log($"[ResourceNode] Targeted: {data?.resourceName}");
    }

    public void Untarget()
    {
        isTargeted  = false;
        isGathering = false;
        spriteRenderer.color = idleColor;
    }

    // ── Gathering Loop ─────────────────────────────

    void UpdateGathering()
    {
        if (data == null || player == null) return;

        // Check if player is in range
        float dist = Mathf.Abs(player.transform.position.x - transform.position.x);
        if (dist > data.gatherRange)
        {
            // Player is too far — stop gathering, wait
            isGathering = false;
            cooldownTimer = 0f;
            return;
        }

        // Player is in range — start/continue gathering
        isGathering = true;
        cooldownTimer += Time.deltaTime;

        if (cooldownTimer >= data.gatherCooldown)
        {
            cooldownTimer = 0f;
            GatherHit();
        }
    }

    void GatherHit()
    {
        if (data == null) return;

        // Give loot
        foreach (var loot in data.lootTable)
        {
            int amount = Random.Range(loot.minAmount, loot.maxAmount + 1);
            inventory?.AddLoot(loot.lootType, amount);
            SpawnFloatingText($"+{amount} {loot.lootType}");
        }

        // Give XP
        if (data.xpPerHit > 0)
        {
            GiveXP(data.xpPerHit);
            SpawnFloatingText($"+{data.xpPerHit} XP");
        }

        // Flash red
        StartCoroutine(HitFlash());

        // Notify HUD
        GameplayManager gm = FindFirstObjectByType<GameplayManager>();
        gm?.RefreshCoinsHUD();
    }

    void GiveXP(int amount)
    {
        if (GameManager.Instance?.ActiveCharacter == null) return;
        CharacterData cd = GameManager.Instance.ActiveCharacter;
        cd.totalExp += amount;

        // Simple level up check (100 XP per level)
        long xpNeeded = cd.level * 100L;
        if (cd.totalExp >= xpNeeded)
        {
            cd.level++;
            cd.totalExp = 0;
            Debug.Log($"[ResourceNode] Level up! Now Lv.{cd.level}");
            SpawnFloatingText("LEVEL UP!");
        }

        GameManager.Instance.SaveCharacter(cd.characterSlot);
    }

    System.Collections.IEnumerator HitFlash()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = isTargeted ? targetedColor : idleColor;
    }

    void SpawnFloatingText(string text)
    {
        if (floatingTextPrefab == null) return;
        Vector3 pos = transform.position + Vector3.up * 1.2f;
        GameObject go = Instantiate(floatingTextPrefab, pos, Quaternion.identity);
        var tmp = go.GetComponentInChildren<TMPro.TextMeshPro>();
        if (tmp != null) tmp.text = text;
        Destroy(go, 1.5f);
    }

    // ── Gizmo (editor helper) ──────────────────────

    void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, data.gatherRange);
    }
}