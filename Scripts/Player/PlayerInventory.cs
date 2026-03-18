using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Holds the player's gathered resources in memory.
/// Attach to the GameManager or Player — persists via GameManager save system.
/// Access via PlayerInventory.Instance
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    // Resource counts
    private Dictionary<LootType, int> inventory = new Dictionary<LootType, int>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize all loot types to 0
        foreach (LootType lt in System.Enum.GetValues(typeof(LootType)))
            inventory[lt] = 0;

        LoadInventory();
    }

    // ── Public API ─────────────────────────────────

    public void AddLoot(LootType type, int amount)
    {
        inventory[type] += amount;
        Debug.Log($"[Inventory] +{amount} {type} (total: {inventory[type]})");
        OnInventoryChanged?.Invoke();
        SaveInventory();
    }

    public int GetAmount(LootType type)
    {
        return inventory.TryGetValue(type, out int val) ? val : 0;
    }

    public bool HasAmount(LootType type, int amount)
    {
        return GetAmount(type) >= amount;
    }

    public bool Spend(LootType type, int amount)
    {
        if (!HasAmount(type, amount)) return false;
        inventory[type] -= amount;
        OnInventoryChanged?.Invoke();
        SaveInventory();
        return true;
    }

    // ── Event ──────────────────────────────────────
    public event System.Action OnInventoryChanged;

    // ── Save / Load ────────────────────────────────

    private void SaveInventory()
    {
        foreach (var kvp in inventory)
            PlayerPrefs.SetInt($"Inv_{kvp.Key}", kvp.Value);
        PlayerPrefs.Save();
    }

    private void LoadInventory()
    {
        foreach (LootType lt in System.Enum.GetValues(typeof(LootType)))
        {
            string key = $"Inv_{lt}";
            if (PlayerPrefs.HasKey(key))
                inventory[lt] = PlayerPrefs.GetInt(key);
        }
    }

    public string GetInventorySummary()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var kvp in inventory)
            if (kvp.Value > 0)
                sb.AppendLine($"{kvp.Key}: {kvp.Value}");
        return sb.ToString();
    }
}
