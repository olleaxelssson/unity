using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Handles fade to black scene transitions.
/// Attach to a persistent GameObject (add to GameManager prefab).
///
/// Setup:
///  1. Add this script to your GameManager prefab
///  2. It auto-creates its own fade canvas at runtime — no manual setup needed
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;
    public Color fadeColor    = Color.black;

    // Auto-created fade overlay
    private Canvas    fadeCanvas;
    private Image     fadeImage;
    private bool      isTransitioning = false;

    // The spawn point ID to use in the next scene
    public static string PendingSpawnID { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateFadeCanvas();
    }

    void CreateFadeCanvas()
    {
        // Create a canvas that sits on top of everything
        GameObject canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform);
        DontDestroyOnLoad(canvasGO);

        fadeCanvas = canvasGO.AddComponent<Canvas>();
        fadeCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999; // always on top

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Full screen black image
        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);

        fadeImage             = imageGO.AddComponent<Image>();
        fadeImage.color       = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        fadeImage.raycastTarget = true;

        RectTransform rt = imageGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Start transparent
        SetAlpha(0f);
    }

    // ── Public API ─────────────────────────────────

    /// <summary>
    /// Transition to a new scene, spawning at a specific portal ID.
    /// </summary>
    public void TransitionToScene(string sceneName, string spawnPortalID)
    {
        if (isTransitioning) return;
        PendingSpawnID = spawnPortalID;
        StartCoroutine(FadeAndLoad(sceneName));
    }

    // ── Coroutines ─────────────────────────────────

    IEnumerator FadeAndLoad(string sceneName)
    {
        isTransitioning = true;

        // Fade out to black
        yield return StartCoroutine(Fade(0f, 1f));

        // Load scene
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        yield return new WaitUntil(() => load.isDone);

        // Small buffer for scene Start() methods to run
        yield return new WaitForSeconds(0.1f);

        // Fade back in
        yield return StartCoroutine(Fade(1f, 0f));

        isTransitioning = false;
    }

    IEnumerator Fade(float fromAlpha, float toAlpha)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(fromAlpha, toAlpha, elapsed / fadeDuration));
            yield return null;
        }
        SetAlpha(toAlpha);
    }

    void SetAlpha(float alpha)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
        // Block input during fade
        fadeImage.raycastTarget = alpha > 0.01f;
    }
}
