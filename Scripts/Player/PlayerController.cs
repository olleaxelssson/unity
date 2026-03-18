using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player movement for a sidescroller with no gravity.
/// Supports keyboard (WASD / Arrow keys) and mouse click-to-move.
///
/// Setup:
///  1. Create a GameObject named "Player" in your Gameplay scene
///  2. Add a SpriteRenderer component
///  3. Add a BoxCollider2D component
///  4. Add a Rigidbody2D — set Gravity Scale to 0, Freeze Rotation Z
///  5. Attach this script
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float clickMoveThreshold = 0.1f; // how close to target before stopping

    [Header("World Boundaries")]
    public bool useBoundaries = true;
    public float boundaryPadding = 0.5f; // how far from screen edge to stop

    private float leftBoundary;
    private float rightBoundary;

    private void CalculateBoundaries()
    {
        if (mainCam == null) return;
        float halfWidth = mainCam.orthographicSize * mainCam.aspect;
        leftBoundary  = mainCam.transform.position.x - halfWidth + boundaryPadding;
        rightBoundary = mainCam.transform.position.x + halfWidth - boundaryPadding;
    }
    public bool showClickMarker = true;
    public GameObject clickMarkerPrefab; // optional — small dot showing where clicked

    [Header("Animation (optional)")]
    public Animator animator; // assign if you have animations
    private static readonly int AnimIsWalking = Animator.StringToHash("isWalking");
    private static readonly int AnimDirection = Animator.StringToHash("direction"); // 1=right, -1=left

    // ── Private state ──────────────────────────────
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Camera mainCam;

    private Vector2 moveInput;          // keyboard input
    private Vector2? clickTarget;       // mouse click target (null = no target)
    private GameObject activeMarker;

    private bool isMoving;
    private float facingDirection = 1f; // 1 = right, -1 = left

    // ── Lifecycle ──────────────────────────────────

    void Awake()
    {
        rb      = GetComponent<Rigidbody2D>();
        sr      = GetComponent<SpriteRenderer>();
        mainCam = Camera.main;
        CalculateBoundaries();
        ApplyCharacterVisuals();
    }

    void Update()
    {
        CalculateBoundaries(); // recalculate every frame in case window resizes
        HandleKeyboardInput();
        HandleMouseInput();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    // ── Input ──────────────────────────────────────

    void HandleKeyboardInput()
    {
        // New Input System
        float h = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)  h = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h =  1f;
        }
        moveInput = new Vector2(h, 0f);

        // Keyboard input cancels click-to-move
        if (Mathf.Abs(h) > 0.01f)
        {
            clickTarget = null;
            ClearMarker();
        }
    }

    void HandleMouseInput()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 worldPos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            worldPos.z = 0f;

            if (UnityEngine.EventSystems.EventSystem.current != null &&
                !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                float clampedClickX = useBoundaries
                    ? Mathf.Clamp(worldPos.x, leftBoundary, rightBoundary)
                    : worldPos.x;
                clickTarget = new Vector2(clampedClickX, transform.position.y);
                SpawnMarker(clickTarget.Value);
            }
        }
    }

    // ── Movement ───────────────────────────────────

    void ApplyMovement()
    {
        Vector2 velocity = Vector2.zero;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            // Keyboard movement takes priority
            velocity = moveInput * moveSpeed;
            facingDirection = moveInput.x > 0 ? 1f : -1f;
            isMoving = true;
        }
        else if (clickTarget.HasValue)
        {
            // Click-to-move
            float dist = Mathf.Abs(clickTarget.Value.x - transform.position.x);
            if (dist > clickMoveThreshold)
            {
                float dir = Mathf.Sign(clickTarget.Value.x - transform.position.x);
                velocity = new Vector2(dir * moveSpeed, 0f);
                facingDirection = dir;
                isMoving = true;
            }
            else
            {
                // Reached target
                clickTarget = null;
                ClearMarker();
                isMoving = false;
            }
        }
        else
        {
            isMoving = false;
        }

        rb.linearVelocity = velocity;

        // Clamp position to world boundaries
        if (useBoundaries)
        {
            float clampedX = Mathf.Clamp(transform.position.x, leftBoundary, rightBoundary);
            if (clampedX != transform.position.x)
            {
                transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
                rb.linearVelocity  = Vector2.zero;
                clickTarget        = null;
                ClearMarker();
            }
        }

        // Flip sprite based on direction
        sr.flipX = facingDirection < 0;
    }

    // ── Animator ───────────────────────────────────

    void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetBool(AnimIsWalking, isMoving);
        animator.SetFloat(AnimDirection, facingDirection);
    }

    // ── Click Marker ───────────────────────────────

    void SpawnMarker(Vector2 pos)
    {
        if (!showClickMarker) return;
        ClearMarker();
        if (clickMarkerPrefab != null)
        {
            activeMarker = Instantiate(clickMarkerPrefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
            Destroy(activeMarker, 1f); // auto-destroy after 1 second
        }
    }

    void ClearMarker()
    {
        if (activeMarker != null)
            Destroy(activeMarker);
    }

    // ── Character Visuals ──────────────────────────

    void ApplyCharacterVisuals()
    {
        if (GameManager.Instance == null) return;
        CharacterData cd = GameManager.Instance.ActiveCharacter;
        if (cd == null) return;

        // Tint the sprite with the character's shirt color as a base tint
        // Replace this with proper sprite swapping once you have character art
        sr.color = cd.shirtColor;

        Debug.Log($"[PlayerController] Loaded character: {cd.characterName} ({cd.characterClass})");
    }

    // ── Public Helpers ─────────────────────────────

    public bool IsMoving => isMoving;
    public float FacingDirection => facingDirection;

    /// <summary>Programmatically tell the player to walk to a position</summary>
    public void SetClickTarget(Vector2 target)
    {
        clickTarget = target;
        ClearMarker();
    }

    /// <summary>Call this to teleport the player to a position (e.g. on zone change)</summary>
    public void TeleportTo(Vector2 pos)
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = new Vector3(pos.x, pos.y, 0f);
        clickTarget = null;
        ClearMarker();
    }
}