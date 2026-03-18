using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manages the main Gameplay scene.
/// Attach to a "GameplayManager" empty GameObject in the Gameplay scene.
///
/// Scene Setup:
///  - GameplayManager (this script)
///  - Main Camera (CameraFollow.cs)
///  - Player (PlayerController.cs)
///  - Ground (Tilemap or simple sprite platform)
///  - Canvas (HUD)
///    - CharacterNameText (TMP)
///    - CharacterClassText (TMP)
///    - LevelText (TMP)
///    - CoinsText (TMP)
///    - MenuButton (Button) → goes back to CharacterSelect
/// </summary>
public class GameplayManager : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;
    public CameraFollow cameraFollow;

    [Header("HUD")]
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterClassText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI coinsText;

    [Header("Player Spawn")]
    public Transform spawnPoint;

    [Header("Scene Names")]
    public string characterSelectScene = "CharacterSelect";

    void Start()
    {
        // Safety check
        if (GameManager.Instance == null || GameManager.Instance.ActiveCharacter == null)
        {
            Debug.LogWarning("[GameplayManager] No active character — returning to CharacterSelect.");
            SceneManager.LoadScene(characterSelectScene);
            return;
        }

        SetupPlayer();
        SetupHUD();
        SetupCamera();
    }

    void SetupPlayer()
    {
        if (player == null) return;

        // Spawn at spawn point if assigned
        if (spawnPoint != null)
            player.TeleportTo(spawnPoint.position);
    }

    void SetupHUD()
    {
        CharacterData cd = GameManager.Instance.ActiveCharacter;
        if (cd == null) return;

        if (characterNameText)  characterNameText.text  = cd.characterName;
        if (characterClassText) characterClassText.text = cd.characterClass.ToString();
        if (levelText)          levelText.text          = $"Lv. {cd.level}";
        if (coinsText)          coinsText.text          = $"Coins: {cd.coins}";
    }

    void SetupCamera()
    {
        if (cameraFollow == null) return;
        if (player != null)
            cameraFollow.target = player.transform;
        cameraFollow.SnapToTarget();
    }

    public void OnMenuButtonClicked()
    {
        SceneManager.LoadScene(characterSelectScene);
    }

    /// <summary>Call this whenever coins change to refresh the HUD</summary>
    public void RefreshCoinsHUD()
    {
        CharacterData cd = GameManager.Instance?.ActiveCharacter;
        if (cd != null && coinsText != null)
            coinsText.text = $"Coins: {cd.coins}";
    }
}
