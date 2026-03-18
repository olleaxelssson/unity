using UnityEngine;
using TMPro;

/// <summary>
/// Displays the player's current inventory on the HUD.
/// Attach to a UI panel in your Canvas.
///
/// Setup:
///  - Create a Panel in Canvas named "InventoryHUD"
///  - Add TextMeshProUGUI children for each resource
///  - Attach this script and wire up the text fields
/// </summary>
public class InventoryHUD : MonoBehaviour
{
    [Header("Resource Text Fields")]
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI oreText;
    public TextMeshProUGUI metalText;
    public TextMeshProUGUI plasticText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI xpText;

    [Header("Level & XP")]
    public TextMeshProUGUI levelText;

    void Start()
    {
        // Subscribe to inventory changes
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnInventoryChanged += RefreshHUD;

        RefreshHUD();
    }

    void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnInventoryChanged -= RefreshHUD;
    }

    void RefreshHUD()
    {
        var inv = PlayerInventory.Instance;
        if (inv == null) return;

        if (woodText)    woodText.text    = $"Wood:    {inv.GetAmount(LootType.Wood)}";
        if (stoneText)   stoneText.text   = $"Stone:   {inv.GetAmount(LootType.Stone)}";
        if (oreText)     oreText.text     = $"Ore:     {inv.GetAmount(LootType.Ore)}";
        if (metalText)   metalText.text   = $"Metal:   {inv.GetAmount(LootType.Metal)}";
        if (plasticText) plasticText.text = $"Plastic: {inv.GetAmount(LootType.Plastic)}";
        if (coinsText)   coinsText.text   = $"Coins:   {inv.GetAmount(LootType.Coins)}";

        // Level and XP from character data
        var cd = GameManager.Instance?.ActiveCharacter;
        if (cd != null)
        {
            if (levelText) levelText.text = $"Lv. {cd.level}";
            if (xpText)    xpText.text    = $"XP: {cd.totalExp} / {cd.level * 100}";
        }
    }

    // Call this from anywhere to force a refresh
    public void ForceRefresh() => RefreshHUD();
}
