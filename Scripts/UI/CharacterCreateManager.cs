using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manages the Character Creation screen.
/// Attach to a "CharacterCreateManager" GameObject in the CharacterCreate scene.
///
/// Scene Setup:
///  - Canvas
///    - NameInputField (TMP_InputField)
///    - ClassSelectionPanel
///      - BeginnerBtn, WarriorBtn, ArcherBtn, MageBtn  (Buttons)
///      - ClassDescriptionText (TMP)
///    - AppearancePanel
///      - HairColorPicker (see ColorPickerUI.cs)
///      - SkinColorPicker
///      - ShirtColorPicker
///      - PrevHairBtn / NextHairBtn (Buttons)
///      - PrevFaceBtn / NextFaceBtn (Buttons)
///    - CharacterPreview (Image — live preview sprite)
///    - StatsPanel
///      - STR_Text, AGI_Text, WIS_Text, LCK_Text (TMP)
///    - CreateButton (Button)
///    - BackButton (Button)
/// </summary>
public class CharacterCreateManager : MonoBehaviour
{
    // ── UI References ──────────────────────────────
    [Header("Name")]
    [SerializeField] private TMP_InputField nameInputField;

    [Header("Class Buttons")]
    [SerializeField] private Button beginnerBtn;
    [SerializeField] private Button warriorBtn;
    [SerializeField] private Button archerBtn;
    [SerializeField] private Button mageBtn;
    [SerializeField] private TextMeshProUGUI classDescriptionText;

    [Header("Appearance")]
    [SerializeField] private Slider hairHueSlider;
    [SerializeField] private Slider skinHueSlider;
    [SerializeField] private Slider shirtHueSlider;
    [SerializeField] private Image  hairColorPreview;
    [SerializeField] private Image  skinColorPreview;
    [SerializeField] private Image  shirtColorPreview;
    [SerializeField] private Button prevHairBtn;
    [SerializeField] private Button nextHairBtn;
    [SerializeField] private Button prevFaceBtn;
    [SerializeField] private Button nextFaceBtn;
    [SerializeField] private TextMeshProUGUI hairStyleLabel;
    [SerializeField] private TextMeshProUGUI faceLabel;

    [Header("Preview & Stats")]
    [SerializeField] private Image  characterPreview;
    [SerializeField] private TextMeshProUGUI strText;
    [SerializeField] private TextMeshProUGUI agiText;
    [SerializeField] private TextMeshProUGUI wisText;
    [SerializeField] private TextMeshProUGUI lckText;

    [Header("Class Sprites (for preview)")]
    [SerializeField] private Sprite beginnerSprite;
    [SerializeField] private Sprite warriorSprite;
    [SerializeField] private Sprite archerSprite;
    [SerializeField] private Sprite mageSprite;

    [Header("Action Buttons")]
    [SerializeField] private Button createButton;
    [SerializeField] private Button backButton;

    [Header("Limits")]
    [SerializeField] private int totalHairStyles = 5;
    [SerializeField] private int totalFaces      = 4;

    [Header("Scene Names")]
    [SerializeField] private string characterSelectScene = "CharacterSelect";

    // ── Working data ───────────────────────────────
    private CharacterClass selectedClass = CharacterClass.Beginner;
    private Color hairColor  = Color.yellow;
    private Color skinColor  = new Color(1f, 0.85f, 0.7f);
    private Color shirtColor = Color.blue;
    private int hairStyleIndex = 0;
    private int faceIndex      = 0;

    // ── Class descriptions ─────────────────────────
    private static readonly string[] ClassDescriptions = {
        "Beginner\nA balanced starting class. Good for learning the ropes.",
        "Warrior\nHigh strength and defense. Excels at melee combat.",
        "Archer\nAgile and precise. Deadly at range with bows.",
        "Mage\nCommands powerful spells. Fragile but devastating."
    };

    // ── Lifecycle ──────────────────────────────────

    void Start()
    {
        // Class buttons
        if (beginnerBtn) beginnerBtn.onClick.AddListener(() => SelectClass(CharacterClass.Beginner));
        if (warriorBtn)  warriorBtn .onClick.AddListener(() => SelectClass(CharacterClass.Warrior));
        if (archerBtn)   archerBtn  .onClick.AddListener(() => SelectClass(CharacterClass.Archer));
        if (mageBtn)     mageBtn    .onClick.AddListener(() => SelectClass(CharacterClass.Mage));

        // Color sliders
        if (hairHueSlider)  hairHueSlider .onValueChanged.AddListener(v => { hairColor  = HueToColor(v); RefreshColors(); });
        if (skinHueSlider)  skinHueSlider .onValueChanged.AddListener(v => { skinColor  = HueToColor(v, 0.75f, 0.9f); RefreshColors(); });
        if (shirtHueSlider) shirtHueSlider.onValueChanged.AddListener(v => { shirtColor = HueToColor(v); RefreshColors(); });

        // Hair / Face cycling
        if (prevHairBtn) prevHairBtn.onClick.AddListener(() => CycleHairStyle(-1));
        if (nextHairBtn) nextHairBtn.onClick.AddListener(() => CycleHairStyle(+1));
        if (prevFaceBtn) prevFaceBtn.onClick.AddListener(() => CycleFace(-1));
        if (nextFaceBtn) nextFaceBtn.onClick.AddListener(() => CycleFace(+1));

        // Actions
        if (createButton) createButton.onClick.AddListener(OnCreateClicked);
        if (backButton)   backButton  .onClick.AddListener(OnBackClicked);

        // Initial state
        SelectClass(CharacterClass.Beginner);
        RefreshColors();
        RefreshLabels();

        // Default slider positions
        hairHueSlider .value = 0.15f; // golden yellow
        skinHueSlider .value = 0.08f; // warm skin
        shirtHueSlider.value = 0.60f; // blue
    }

    // ── Class Selection ────────────────────────────

    void SelectClass(CharacterClass cls)
    {
        selectedClass = cls;
        if (classDescriptionText) classDescriptionText.text = ClassDescriptions[(int)cls];

        if (characterPreview) characterPreview.sprite = cls switch
        {
            CharacterClass.Warrior => warriorSprite,
            CharacterClass.Archer  => archerSprite,
            CharacterClass.Mage    => mageSprite,
            _                      => beginnerSprite
        };

        var temp = ScriptableObject.CreateInstance<CharacterData>();
        temp.characterClass = cls;
        temp.InitFromClass();
        if (strText) strText.text = $"STR  {temp.strength}";
        if (agiText) agiText.text = $"AGI  {temp.agility}";
        if (wisText) wisText.text = $"WIS  {temp.wisdom}";
        if (lckText) lckText.text = $"LCK  {temp.luck}";
        Destroy(temp);

        SetClassButtonHighlights(cls);
    }

    void SetClassButtonHighlights(CharacterClass selected)
    {
        var btns = new[] { beginnerBtn, warriorBtn, archerBtn, mageBtn };
        var classes = new[] {
            CharacterClass.Beginner, CharacterClass.Warrior,
            CharacterClass.Archer,   CharacterClass.Mage
        };
        for (int i = 0; i < btns.Length; i++)
        {
            if (btns[i] == null) continue;
            bool isSelected = classes[i] == selected;

            // Button background color
            var colors = btns[i].colors;
            colors.normalColor      = isSelected ? new Color(0.3f, 0.7f, 1f)   : new Color(0.25f, 0.25f, 0.4f);
            colors.highlightedColor = isSelected ? new Color(0.4f, 0.8f, 1f)   : new Color(0.35f, 0.35f, 0.5f);
            colors.pressedColor     = isSelected ? new Color(0.2f, 0.5f, 0.8f) : new Color(0.2f, 0.2f, 0.3f);
            colors.colorMultiplier  = 1f; // prevents Unity from darkening the button
            btns[i].colors = colors;

            // Always keep text white and fully visible
            var tmp = btns[i].GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.color    = Color.white;
                tmp.fontSize = isSelected ? 22 : 20;
                tmp.fontStyle = isSelected
                    ? TMPro.FontStyles.Bold
                    : TMPro.FontStyles.Normal;
            }
        }
    }

    // ── Appearance ─────────────────────────────────

    Color HueToColor(float hue, float saturation = 1f, float value = 1f)
    {
        return Color.HSVToRGB(hue, saturation, value);
    }

    void RefreshColors()
    {
        if (hairColorPreview)  hairColorPreview.color  = hairColor;
        if (skinColorPreview)  skinColorPreview.color  = skinColor;
        if (shirtColorPreview) shirtColorPreview.color = shirtColor;
    }

    void CycleHairStyle(int delta)
    {
        hairStyleIndex = (hairStyleIndex + delta + totalHairStyles) % totalHairStyles;
        RefreshLabels();
    }

    void CycleFace(int delta)
    {
        faceIndex = (faceIndex + delta + totalFaces) % totalFaces;
        RefreshLabels();
    }

    void RefreshLabels()
    {
        if (hairStyleLabel) hairStyleLabel.text = $"Hair {hairStyleIndex + 1}/{totalHairStyles}";
        if (faceLabel)      faceLabel.text      = $"Face {faceIndex + 1}/{totalFaces}";
    }

    // ── Create / Back ──────────────────────────────

    void OnCreateClicked()
    {
        // Safety: if GameManager is missing (e.g. playing from editor without Boot scene)
        if (GameManager.Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
            Debug.LogWarning("[CharacterCreate] GameManager was missing — created a fallback.");
        }

        string charName = nameInputField != null ? nameInputField.text.Trim() : "";
        if (string.IsNullOrEmpty(charName))
        {
            Debug.LogWarning("[CharacterCreate] Name cannot be empty.");
            // TODO: show an error label in the UI
            return;
        }

        int slot = GameManager.Instance.activeCharacterIndex;
        if (slot < 0 || slot >= GameManager.MAX_CHARACTERS)
        {
            // Fallback: find first free slot
            slot = 0;
            for (int i = 0; i < GameManager.MAX_CHARACTERS; i++)
            {
                if (GameManager.Instance.characterSlots[i] == null)
                {
                    slot = i;
                    break;
                }
            }
            GameManager.Instance.activeCharacterIndex = slot;
        }

        CharacterData cd = ScriptableObject.CreateInstance<CharacterData>();
        cd.characterName  = charName;
        cd.characterSlot  = slot;
        cd.characterClass = selectedClass;
        cd.hairColor      = hairColor;
        cd.skinColor      = skinColor;
        cd.shirtColor     = shirtColor;
        cd.hairStyleIndex = hairStyleIndex;
        cd.faceIndex      = faceIndex;
        cd.InitFromClass();

        GameManager.Instance.characterSlots[slot] = cd;
        GameManager.Instance.SaveCharacter(slot);

        Debug.Log($"[CharacterCreate] Created '{charName}' as {selectedClass} in slot {slot}");
        SceneManager.LoadScene(characterSelectScene);
    }

    void OnBackClicked()
    {
        SceneManager.LoadScene(characterSelectScene);
    }
}