using UnityEngine;

/// <summary>
/// Place this script on a GameObject in your very first scene (e.g., "Boot").
/// It ensures GameManager is created before any other scene loads.
/// The Boot scene immediately transitions to CharacterSelect.
/// 
/// Build Settings scene order:
///   0 - Boot
///   1 - CharacterSelect
///   2 - CharacterCreate
///   3 - Gameplay
/// </summary>
public class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private string firstScene = "CharacterSelect";

    void Awake()
    {
        // Force 1920x1080 resolution
        Screen.SetResolution(960, 540, Screen.fullScreen);

        // If GameManager doesn't exist yet, spawn it
        if (GameManager.Instance == null && gameManagerPrefab != null)
        {
            Instantiate(gameManagerPrefab);
        }
    }

    void Start()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(firstScene);
    }
}