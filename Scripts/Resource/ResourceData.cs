using UnityEngine;

public enum ResourceType
{
    Tree,
    Rock,
    Enemy,
    Scavenge
}

public enum LootType
{
    Wood,
    Stone,
    Ore,
    Metal,
    Plastic,
    Coins,
    XP
}

[System.Serializable]
public class LootEntry
{
    public LootType lootType;
    public int minAmount = 1;
    public int maxAmount = 3;
}

/// <summary>
/// ScriptableObject defining a resource node type.
/// Create via: Assets → Right Click → Create → IdleonGame → Resource Data
/// </summary>
[CreateAssetMenu(fileName = "NewResource", menuName = "IdleonGame/Resource Data")]
public class ResourceData : ScriptableObject
{
    [Header("Identity")]
    public string resourceName;
    public ResourceType resourceType;
    public Sprite resourceSprite;

    [Header("Gathering")]
    public float gatherCooldown  = 2f;   // seconds between each hit
    public float gatherRange     = 2f;   // how close player must be

    [Header("Loot")]
    public LootEntry[] lootTable;        // what this resource drops per hit

    [Header("XP")]
    public int xpPerHit = 5;
}
