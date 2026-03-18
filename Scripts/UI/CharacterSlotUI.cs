using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Represents one character slot card in the character select screen.
///
/// Prefab hierarchy — each child must be named EXACTLY as shown:
///  - CharacterSlotUI  (root: this script + Button component)
///    - SlotBackground    (child GameObject with Image component)
///    - SlotPortrait      (child GameObject with Image component)
///    - NameText          (child GameObject with TextMeshProUGUI)
///    - ClassText         (child GameObject with TextMeshProUGUI)
///    - LevelText         (child GameObject with TextMeshProUGUI)
///    - EmptyLabel        (child GameObject with TextMeshProUGUI)
///    - SelectionBorder   (child GameObject with Image component)
///
/// IMPORTANT: Do NOT assign Background and Portrait in the Inspector —
/// they are found automatically by child name at runtime.
/// Only assign the Button field manually.
/// </summary>
public class CharacterSlotUI : MonoBehaviour
{
    [Header("Button (assign manually)")]
    [SerializeField] private Button slotButton;

    [Header("Leave these blank — auto-found by child name")]
    [SerializeField] private Image background;
    [SerializeField] private Image portrait;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI classText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject emptyLabel;
    [SerializeField] private GameObject selectionBorder;

    [Header("Class Sprites")]
    [SerializeField] private Sprite beginnerSprite;
    [SerializeField] private Sprite warriorSprite;
    [SerializeField] private Sprite archerSprite;
    [SerializeField] private Sprite mageSprite;
    [SerializeField] private Sprite emptySlotSprite;

    [Header("Colors")]
    [SerializeField] private Color emptyBgColor    = new Color(0.15f, 0.15f, 0.2f);
    [SerializeField] private Color filledBgColor   = new Color(0.1f,  0.2f,  0.35f);
    [SerializeField] private Color selectedBgColor = new Color(0.2f,  0.45f, 0.7f);

    private Action onClickCallback;

    void Awake()
    {
        // Background lives on the ROOT Image (the Button's own Image component)
        background      = GetComponent<Image>();
        // Portrait is a named child
        portrait        = FindChildImage("SlotPortrait");
        nameText        = FindChildTMP("NameText");
        classText       = FindChildTMP("ClassText");
        levelText       = FindChildTMP("LevelText");
        emptyLabel      = FindChildGO("EmptyLabel");
        selectionBorder = FindChildGO("SelectionBorder");

        if (slotButton == null)
            slotButton = GetComponent<Button>();
    }

    private Image FindChildImage(string childName)
    {
        Transform t = transform.Find(childName);
        if (t == null) { Debug.LogWarning($"[CharacterSlotUI] Missing child: {childName}"); return null; }
        return t.GetComponent<Image>();
    }

    private TextMeshProUGUI FindChildTMP(string childName)
    {
        Transform t = transform.Find(childName);
        if (t == null) { Debug.LogWarning($"[CharacterSlotUI] Missing child: {childName}"); return null; }
        return t.GetComponent<TextMeshProUGUI>();
    }

    private GameObject FindChildGO(string childName)
    {
        Transform t = transform.Find(childName);
        if (t == null) { Debug.LogWarning($"[CharacterSlotUI] Missing child: {childName}"); return null; }
        return t.gameObject;
    }

    public void Setup(int slotIndex, CharacterData data, Action onClick)
    {
        onClickCallback = onClick;
        slotButton.onClick.AddListener(() => onClickCallback?.Invoke());
        selectionBorder.SetActive(false);

        if (data == null)
        {
            ShowEmpty();
        }
        else
        {
            ShowCharacter(data);
        }
    }

    public void SetSelected(bool selected)
    {
        selectionBorder.SetActive(selected);
        background.color = selected ? selectedBgColor : filledBgColor;
    }

    // ──────────────────────────────────────────
    //  Private helpers
    // ──────────────────────────────────────────

    private void ShowEmpty()
    {
        if (background)      background.color  = emptyBgColor;
        if (portrait)        { portrait.sprite = emptySlotSprite; portrait.color = new Color(1, 1, 1, 0.3f); }
        if (nameText)        nameText.text     = "";
        if (classText)       classText.text    = "";
        if (levelText)       levelText.text    = "";
        if (emptyLabel)      emptyLabel.SetActive(true);
        if (selectionBorder) selectionBorder.SetActive(false);
    }

    private void ShowCharacter(CharacterData data)
    {
        if (background)      background.color  = filledBgColor;
        if (portrait)        { portrait.color  = Color.white; portrait.sprite = GetClassSprite(data.characterClass); }
        if (nameText)        nameText.text     = data.characterName;
        if (classText)       classText.text    = data.characterClass.ToString();
        if (levelText)       levelText.text    = $"Lv. {data.level}";
        if (emptyLabel)      emptyLabel.SetActive(false);
        if (selectionBorder) selectionBorder.SetActive(false);
    }

    private Sprite GetClassSprite(CharacterClass cls) => cls switch
    {
        CharacterClass.Warrior  => warriorSprite,
        CharacterClass.Archer   => archerSprite,
        CharacterClass.Mage     => mageSprite,
        _                       => beginnerSprite
    };
}