using UnityEngine;

/// <summary>
/// Smooth camera follow for a 2D sidescroller.
/// Follows the player horizontally, stays locked on Y.
/// Includes world boundary clamping so camera doesn't go past level edges.
///
/// Setup:
///  1. Select your Main Camera
///  2. Attach this script
///  3. Drag your Player GameObject into the Target field
///  4. Set your world left/right boundaries
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // drag Player here

    [Header("Follow Settings")]
    public float smoothSpeed   = 6f;
    public float horizontalOffset = 0f; // nudge camera ahead of player

    [Header("World Boundaries (set to your level width)")]
    public bool  useBoundaries = true;
    public float leftBoundary  = -50f;
    public float rightBoundary =  50f;

    [Header("Fixed Y Position")]
    public bool  lockY    = true;
    public float fixedY   = 0f; // camera stays at this Y always

    private Camera cam;
    private float  halfWidth;

    void Awake()
    {
        cam = GetComponent<Camera>();
        halfWidth = cam.orthographicSize * cam.aspect;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Target X position (with optional offset)
        float targetX = target.position.x + horizontalOffset;

        // Clamp to world boundaries
        if (useBoundaries)
        {
            targetX = Mathf.Clamp(targetX,
                leftBoundary  + halfWidth,
                rightBoundary - halfWidth);
        }

        float targetY = lockY ? fixedY : transform.position.y;

        // Smooth follow
        Vector3 desiredPos  = new Vector3(targetX, targetY, transform.position.z);
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        transform.position  = smoothedPos;
    }

    /// <summary>Snap camera instantly to player (call on scene load / teleport)</summary>
    public void SnapToTarget()
    {
        if (target == null) return;
        float x = Mathf.Clamp(target.position.x,
            leftBoundary  + halfWidth,
            rightBoundary - halfWidth);
        transform.position = new Vector3(x, fixedY, transform.position.z);
    }
}
