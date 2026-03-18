using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "IdleonGame/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Identity")]
    public string characterName;
    public int characterSlot; // 0-based index

    [Header("Class")]
    public CharacterClass characterClass;

    [Header("Appearance")]
    public Color hairColor = Color.yellow;
    public Color skinColor = new Color(1f, 0.85f, 0.7f);
    public Color shirtColor = Color.blue;
    public int hairStyleIndex = 0;
    public int faceIndex = 0;

    [Header("Stats (auto-set from class)")]
    public int strength;
    public int agility;
    public int wisdom;
    public int luck;

    [Header("Progress")]
    public int level = 1;
    public long totalExp = 0;
    public long coins = 0;

    public void InitFromClass()
    {
        switch (characterClass)
        {
            case CharacterClass.Warrior:
                strength = 10; agility = 5; wisdom = 3; luck = 2; break;
            case CharacterClass.Archer:
                strength = 5; agility = 10; wisdom = 4; luck = 6; break;
            case CharacterClass.Mage:
                strength = 3; agility = 4; wisdom = 12; luck = 6; break;
            case CharacterClass.Beginner:
                strength = 5; agility = 5; wisdom = 5; luck = 5; break;
        }
    }
}

public enum CharacterClass
{
    Beginner,
    Warrior,
    Archer,
    Mage
}
