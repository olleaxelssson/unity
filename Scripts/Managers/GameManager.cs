using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton that persists across scenes and holds all character slot data.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public const int MAX_CHARACTERS = 6;

    // Runtime list of saved characters (null = empty slot)
    public CharacterData[] characterSlots = new CharacterData[MAX_CHARACTERS];

    // Which character is currently selected/active
    public int activeCharacterIndex = -1;

    public CharacterData ActiveCharacter =>
        (activeCharacterIndex >= 0 && activeCharacterIndex < MAX_CHARACTERS)
            ? characterSlots[activeCharacterIndex]
            : null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAllCharacters();
    }

    // ──────────────────────────────────────────
    //  Save / Load  (PlayerPrefs JSON approach)
    // ──────────────────────────────────────────

    [System.Serializable]
    private class CharacterSaveData
    {
        public string characterName;
        public int characterClass;
        public float hairR, hairG, hairB;
        public float skinR, skinG, skinB;
        public float shirtR, shirtG, shirtB;
        public int hairStyleIndex;
        public int faceIndex;
        public int strength, agility, wisdom, luck;
        public int level;
        public long totalExp;
        public long coins;
    }

    public void SaveCharacter(int slot)
    {
        if (slot < 0 || slot >= MAX_CHARACTERS || characterSlots[slot] == null) return;
        CharacterData cd = characterSlots[slot];
        var save = new CharacterSaveData
        {
            characterName   = cd.characterName,
            characterClass  = (int)cd.characterClass,
            hairR = cd.hairColor.r,  hairG = cd.hairColor.g,  hairB = cd.hairColor.b,
            skinR = cd.skinColor.r,  skinG = cd.skinColor.g,  skinB = cd.skinColor.b,
            shirtR= cd.shirtColor.r, shirtG= cd.shirtColor.g, shirtB= cd.shirtColor.b,
            hairStyleIndex  = cd.hairStyleIndex,
            faceIndex       = cd.faceIndex,
            strength        = cd.strength,
            agility         = cd.agility,
            wisdom          = cd.wisdom,
            luck            = cd.luck,
            level           = cd.level,
            totalExp        = cd.totalExp,
            coins           = cd.coins
        };
        PlayerPrefs.SetString($"Character_{slot}", JsonUtility.ToJson(save));
        PlayerPrefs.Save();
        Debug.Log($"[GameManager] Saved character in slot {slot}: {cd.characterName}");
    }

    public void DeleteCharacter(int slot)
    {
        if (slot < 0 || slot >= MAX_CHARACTERS) return;
        PlayerPrefs.DeleteKey($"Character_{slot}");
        PlayerPrefs.Save();
        if (characterSlots[slot] != null)
        {
            Destroy(characterSlots[slot]);
            characterSlots[slot] = null;
        }
        Debug.Log($"[GameManager] Deleted character in slot {slot}");
    }

    private void LoadAllCharacters()
    {
        for (int i = 0; i < MAX_CHARACTERS; i++)
        {
            string key = $"Character_{i}";
            if (!PlayerPrefs.HasKey(key)) continue;

            var save = JsonUtility.FromJson<CharacterSaveData>(PlayerPrefs.GetString(key));
            CharacterData cd = ScriptableObject.CreateInstance<CharacterData>();
            cd.characterName   = save.characterName;
            cd.characterSlot   = i;
            cd.characterClass  = (CharacterClass)save.characterClass;
            cd.hairColor       = new Color(save.hairR,  save.hairG,  save.hairB);
            cd.skinColor       = new Color(save.skinR,  save.skinG,  save.skinB);
            cd.shirtColor      = new Color(save.shirtR, save.shirtG, save.shirtB);
            cd.hairStyleIndex  = save.hairStyleIndex;
            cd.faceIndex       = save.faceIndex;
            cd.strength        = save.strength;
            cd.agility         = save.agility;
            cd.wisdom          = save.wisdom;
            cd.luck            = save.luck;
            cd.level           = save.level;
            cd.totalExp        = save.totalExp;
            cd.coins           = save.coins;
            characterSlots[i]  = cd;
        }
        Debug.Log("[GameManager] Characters loaded.");
    }
}
