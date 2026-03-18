using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manages the Character Selection screen.
/// Attach to a "CharacterSelectManager" GameObject in the CharacterSelect scene.
///
/// Scene Setup:
///  - Canvas
///    - TitleText (TMP)
///    - CharacterSlotContainer (Horizontal/Grid Layout Group)
///      - CharacterSlotPrefab x6  (see CharacterSlotUI.cs)
///    - PlayButton (Button) → disabled until a character is chosen
///    - DeleteButton (Button) → deletes selected character
///    - CreateNewButton (Button) → loads CharacterCreate scene
/// </summary>
public class CharacterSelectManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject slotPrefab;         // CharacterSlotUI prefab
    [SerializeField] private Button playButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button createNewButton;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Scene Names")]
    [SerializeField] private string characterCreateScene = "CharacterCreate";
    [SerializeField] private string gameplayScene = "Gameplay";

    private CharacterSlotUI[] slotUIs;
    private int selectedSlot = -1;

    void Start()
    {
        titleText.text = "Choose Your Character";
        BuildSlots();
        SetButtonStates();

        playButton.onClick.AddListener(OnPlayClicked);
        deleteButton.onClick.AddListener(OnDeleteClicked);
        createNewButton.onClick.AddListener(OnCreateNewClicked);
    }

    void BuildSlots()
    {
        // Clear existing children (in case of reload)
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        slotUIs = new CharacterSlotUI[GameManager.MAX_CHARACTERS];

        for (int i = 0; i < GameManager.MAX_CHARACTERS; i++)
        {
            GameObject go = Instantiate(slotPrefab, slotContainer);
            CharacterSlotUI ui = go.GetComponent<CharacterSlotUI>();
            int capturedIndex = i; // closure capture
            ui.Setup(i, GameManager.Instance.characterSlots[i], () => OnSlotClicked(capturedIndex));
            slotUIs[i] = ui;
        }
    }

    void OnSlotClicked(int index)
    {
        // If empty slot → go create a character in that slot
        if (GameManager.Instance.characterSlots[index] == null)
        {
            GameManager.Instance.activeCharacterIndex = index;
            SceneManager.LoadScene(characterCreateScene);
            return;
        }

        // Otherwise select it
        selectedSlot = index;
        GameManager.Instance.activeCharacterIndex = index;

        for (int i = 0; i < slotUIs.Length; i++)
            slotUIs[i].SetSelected(i == selectedSlot);

        SetButtonStates();
    }

    void SetButtonStates()
    {
        bool hasSelection = selectedSlot >= 0 &&
                            GameManager.Instance.characterSlots[selectedSlot] != null;
        playButton.interactable   = hasSelection;
        deleteButton.interactable = hasSelection;

        // Disable "Create New" if all slots are filled
        bool hasFreeSlot = false;
        foreach (var cd in GameManager.Instance.characterSlots)
            if (cd == null) { hasFreeSlot = true; break; }
        createNewButton.interactable = hasFreeSlot;
    }

    void OnPlayClicked()
    {
        if (selectedSlot < 0) return;
        Debug.Log($"[CharacterSelect] Playing as: {GameManager.Instance.ActiveCharacter.characterName}");
        SceneManager.LoadScene(gameplayScene);
    }

    void OnDeleteClicked()
    {
        if (selectedSlot < 0) return;
        // In production: show a confirmation dialog first
        GameManager.Instance.DeleteCharacter(selectedSlot);
        selectedSlot = -1;
        GameManager.Instance.activeCharacterIndex = -1;
        BuildSlots();
        SetButtonStates();
    }

    void OnCreateNewClicked()
    {
        // Find first free slot
        for (int i = 0; i < GameManager.MAX_CHARACTERS; i++)
        {
            if (GameManager.Instance.characterSlots[i] == null)
            {
                GameManager.Instance.activeCharacterIndex = i;
                SceneManager.LoadScene(characterCreateScene);
                return;
            }
        }
    }
}
