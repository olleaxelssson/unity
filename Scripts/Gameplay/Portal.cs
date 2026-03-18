using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Place on a portal GameObject in your scene.
/// Player clicks it → walks to portal → fades to black → loads scene.
///
/// Setup:
///  1. Create a GameObject for the portal (Sprite + this script)
///  2. Add a BoxCollider2D or CircleCollider2D for click detection
///  3. Fill in the destination scene name and portal IDs
///  4. Place a matching SpawnPoint in the destination scene
/// </summary>
public class Portal : MonoBehaviour
{
    [Header("Portal Identity")]
    public string portalID;
    public string destinationScene;
    public string destinationPortalID;

    [Header("Label (optional)")]
    public TextMeshPro portalLabel;
    public string labelText;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Color idleColor    = new Color(0.4f, 0.8f, 1f,  1f);
    public Color hoveredColor = new Color(0.7f, 1f,   1f,  1f);
    public Color activeColor  = new Color(1f,   1f,   0.5f,1f);

    [Header("Pulse Animation")]
    public float pulseSpeed    = 2f;
    public float pulseMinScale = 0.9f;
    public float pulseMaxScale = 1.1f;

    [Header("Walk To Portal")]
    public float arrivalThreshold = 0.6f; // how close player must be before teleporting

    private Camera           mainCam;
    private bool             isHovered     = false;
    private bool             isActivated   = false; // player is walking toward this portal
    private bool             isOnCooldown  = false;
    private Vector3          baseScale;
    private PlayerController player;

    [Header("Spawn Cooldown")]
    public float spawnCooldown = 2f;

    void Awake()
    {
        mainCam = Camera.main;
        player  = FindFirstObjectByType<PlayerController>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        baseScale            = transform.localScale;
        spriteRenderer.color = idleColor;

        if (portalLabel != null)
            portalLabel.text = labelText;
    }

    void Update()
    {
        if (!isActivated)
        {
            HandleHover();
            HandleClick();
        }
        else
        {
            CheckPlayerArrival();
        }

        PulseAnimation();
    }

    // ── Hover ──────────────────────────────────────

    void HandleHover()
    {
        if (Mouse.current == null) return;
        Vector2 mouseWorld = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D col = GetComponent<Collider2D>();
        bool hovering = col != null && col.OverlapPoint(mouseWorld);

        if (hovering != isHovered)
        {
            isHovered            = hovering;
            spriteRenderer.color = isHovered ? hoveredColor : idleColor;
        }
    }

    // ── Click ──────────────────────────────────────

    void HandleClick()
    {
        if (isOnCooldown) return;
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        Vector2 mouseWorld = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D col = GetComponent<Collider2D>();
        if (col == null || !col.OverlapPoint(mouseWorld)) return;

        ActivatePortal();
    }

    public void StartIgnoreCooldown()
    {
        isOnCooldown = true;
        isActivated  = false;
        spriteRenderer.color = idleColor;
        StartCoroutine(CooldownRoutine());
    }

    System.Collections.IEnumerator CooldownRoutine()
    {
        yield return new UnityEngine.WaitForSeconds(spawnCooldown);
        isOnCooldown = false;
        Debug.Log($"[Portal] {portalID} cooldown ended — ready to use");
    }

    void ActivatePortal()
    {
        if (string.IsNullOrEmpty(destinationScene))
        {
            Debug.LogWarning($"[Portal] {portalID} has no destination scene set.");
            return;
        }

        isActivated          = true;
        spriteRenderer.color = activeColor;

        // Tell player to walk to portal
        if (player != null)
            player.SetClickTarget(new Vector2(transform.position.x, player.transform.position.y));

        Debug.Log($"[Portal] Activated — walking to {destinationScene}");
    }

    void CheckPlayerArrival()
    {
        if (player == null) return;

        float dist = Mathf.Abs(player.transform.position.x - transform.position.x);
        Debug.Log($"[Portal] Distance to portal: {dist}"); // remove after testing

        if (dist <= arrivalThreshold)
            OnPlayerArrived();
    }

    void OnPlayerArrived()
    {
        isActivated = false;
        Debug.Log($"[Portal] Player arrived at {portalID} → loading {destinationScene}");

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(destinationScene, destinationPortalID);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(destinationScene);
    }

    // ── Pulse ──────────────────────────────────────

    void PulseAnimation()
    {
        float pulse = Mathf.Lerp(pulseMinScale, pulseMaxScale,
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        transform.localScale = baseScale * pulse;
    }

    // ── Gizmo ──────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, arrivalThreshold);
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up,
            $"ID: {portalID}\n→ {destinationScene}\nSpawn: {destinationPortalID}");
        #endif
    }
}