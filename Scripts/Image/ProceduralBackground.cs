using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to a UI RawImage that sits behind everything on your Canvas.
/// Creates a rich animated background: scrolling star layers + floating orbs + 
/// pulsing vignette — all generated in code, no textures needed.
///
/// Setup:
///  1. Right-click Canvas → UI → Raw Image
///  2. Name it "Background", move it to the bottom of the Canvas hierarchy
///  3. Stretch it to fill the full canvas (Anchor: stretch/stretch, all offsets 0)
///  4. Attach this script to it
/// </summary>
[RequireComponent(typeof(RawImage))]
public class ProceduralBackground : MonoBehaviour
{
    [Header("Colors")]
    public Color skyTop    = new Color(0.04f, 0.06f, 0.18f); // deep navy
    public Color skyBottom = new Color(0.08f, 0.03f, 0.12f); // dark purple
    public Color starColor = new Color(0.9f,  0.95f, 1.0f);
    public Color orbColor1 = new Color(0.15f, 0.4f,  0.9f,  0.18f); // blue orb
    public Color orbColor2 = new Color(0.6f,  0.2f,  0.9f,  0.14f); // purple orb
    public Color orbColor3 = new Color(0.1f,  0.7f,  0.6f,  0.10f); // teal orb

    [Header("Texture Resolution")]
    public int texWidth  = 512;
    public int texHeight = 512;

    [Header("Stars")]
    [Range(80, 400)]
    public int starCount = 200;
    public float starTwinkleSpeed = 1.2f;

    [Header("Scroll")]
    public float scrollSpeedX = 0.012f;
    public float scrollSpeedY = 0.005f;

    // ── Private state ──────────────────────────────
    private Texture2D tex;
    private Color32[] pixels;
    private RawImage rawImage;

    // Star data
    private Vector2[] starPositions;
    private float[]   starSizes;
    private float[]   starPhases;
    private float[]   starBrightness;

    // Orb data  (position, radius, color, drift speed)
    private struct Orb { public Vector2 pos, vel; public float radius; public Color col; }
    private Orb[] orbs;

    private float scrollX, scrollY;

    // ── Unity Lifecycle ────────────────────────────

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        tex      = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode   = TextureWrapMode.Repeat;
        pixels   = new Color32[texWidth * texHeight];
        rawImage.texture = tex;

        InitStars();
        InitOrbs();
    }

    void InitStars()
    {
        starPositions  = new Vector2[starCount];
        starSizes      = new float[starCount];
        starPhases     = new float[starCount];
        starBrightness = new float[starCount];
        for (int i = 0; i < starCount; i++)
        {
            starPositions[i]  = new Vector2(Random.value, Random.value);
            starSizes[i]      = Random.Range(0.5f, 2.5f);
            starPhases[i]     = Random.Range(0f, Mathf.PI * 2f);
            starBrightness[i] = Random.Range(0.5f, 1.0f);
        }
    }

    void InitOrbs()
    {
        orbs = new Orb[]
        {
            new Orb { pos=new Vector2(0.25f,0.70f), vel=new Vector2( 0.015f, 0.008f), radius=0.28f, col=orbColor1 },
            new Orb { pos=new Vector2(0.75f,0.30f), vel=new Vector2(-0.010f, 0.012f), radius=0.22f, col=orbColor2 },
            new Orb { pos=new Vector2(0.50f,0.85f), vel=new Vector2( 0.008f,-0.014f), radius=0.18f, col=orbColor3 },
            new Orb { pos=new Vector2(0.10f,0.20f), vel=new Vector2( 0.012f, 0.006f), radius=0.15f, col=orbColor2 },
        };
    }

    void Update()
    {
        float t = Time.time;
        scrollX += scrollSpeedX * Time.deltaTime;
        scrollY += scrollSpeedY * Time.deltaTime;

        UpdateOrbs();
        RenderFrame(t);
    }

    void UpdateOrbs()
    {
        for (int i = 0; i < orbs.Length; i++)
        {
            orbs[i].pos += orbs[i].vel * Time.deltaTime;
            // Bounce off edges
            if (orbs[i].pos.x < 0 || orbs[i].pos.x > 1) orbs[i].vel.x *= -1;
            if (orbs[i].pos.y < 0 || orbs[i].pos.y > 1) orbs[i].vel.y *= -1;
            orbs[i].pos = new Vector2(
                Mathf.Clamp01(orbs[i].pos.x),
                Mathf.Clamp01(orbs[i].pos.y));
        }
    }

    // ── Rendering ──────────────────────────────────

    void RenderFrame(float t)
    {
        float aspect = (float)texWidth / texHeight;

        for (int y = 0; y < texHeight; y++)
        {
            float fy = y / (float)texHeight;
            // Gradient base
            Color baseCol = Color.Lerp(skyBottom, skyTop, fy);

            for (int x = 0; x < texWidth; x++)
            {
                float fx = x / (float)texWidth;
                Color col = baseCol;

                // ── Orb glows ──────────────────────────
                foreach (var orb in orbs)
                {
                    float dx = (fx - orb.pos.x) * aspect;
                    float dy = fy - orb.pos.y;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float influence = Mathf.Clamp01(1f - dist / orb.radius);
                    influence = influence * influence; // quadratic falloff
                    col = Color.Lerp(col, col + orb.col, influence * orb.col.a * 6f);
                }

                // ── Vignette ───────────────────────────
                float vx = fx - 0.5f, vy = fy - 0.5f;
                float vignette = 1f - Mathf.Clamp01((vx*vx + vy*vy) * 2.8f);
                col *= vignette;

                // ── Subtle noise grain ─────────────────
                float grain = (Mathf.Sin(fx * 317.1f + t * 0.3f) *
                               Mathf.Cos(fy * 211.7f + t * 0.2f)) * 0.015f;
                col.r += grain; col.g += grain; col.b += grain;

                pixels[y * texWidth + x] = col;
            }
        }

        // ── Stars (painted on top) ─────────────────
        PaintStars(t);

        tex.SetPixels32(pixels);
        tex.Apply();
    }

    void PaintStars(float t)
    {
        for (int i = 0; i < starCount; i++)
        {
            float twinkle = 0.5f + 0.5f * Mathf.Sin(t * starTwinkleSpeed + starPhases[i]);
            float bright  = starBrightness[i] * twinkle;
            if (bright < 0.1f) continue;

            // Scrolling UV
            float sx = (starPositions[i].x + scrollX) % 1f;
            float sy = (starPositions[i].y + scrollY) % 1f;

            int px = Mathf.RoundToInt(sx * texWidth);
            int py = Mathf.RoundToInt(sy * texHeight);
            int radius = Mathf.RoundToInt(starSizes[i]);

            Color32 sc = starColor * bright;

            for (int dy = -radius; dy <= radius; dy++)
            for (int dx = -radius; dx <= radius; dx++)
            {
                float dist = Mathf.Sqrt(dx*dx + dy*dy);
                if (dist > starSizes[i]) continue;
                float falloff = 1f - dist / starSizes[i];

                int nx = (px + dx + texWidth)  % texWidth;
                int ny = (py + dy + texHeight) % texHeight;
                int idx = ny * texWidth + nx;

                Color32 existing = pixels[idx];
                Color32 blended  = Color32.Lerp(existing, sc, falloff * bright);
                pixels[idx] = blended;
            }
        }
    }
}
