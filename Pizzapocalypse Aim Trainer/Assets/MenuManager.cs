using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenu : MonoBehaviour
{
	[Header("UI Panels")]
	public GameObject mainMenuPanel;    // The panel containing your buttons
	public GameObject optionsPanel;     // An options panel (create this later)
	public GameObject gameSelectionPanel; // Panel for New Game/Load Game buttons
	public GameObject saveSlotPanel; // Panel for save file slots

	[Header("Main Buttons")]
	public Button playButton;
	public Button optionsButton;
	public Button quitButton;

	[Header("Game Select Buttons")]
	public Button newGameButton;
	public Button loadGameButton;
	public Button backToMainButton;

	[Header("Save Slot Buttons")]
	public Button[] saveSlotButtons;
	public TextMeshProUGUI[] saveSlotTexts;

    [Header("Transition Settings")]
	public float transitionDelay = 0.5f;   // How long to wait before loading
	public Animator transitionAnimator;  // Optional fade animator
	public Image fadeImage;              // Optional simple fade image

	private bool isNewGameMode = false;

	void Start()
	{
		// Safety check - make sure panels are set up correctly
		if (mainMenuPanel == null)
			Debug.LogError("MainMenuPanel not assigned in MainMenu script!");
		else
			mainMenuPanel.SetActive(true);

		if (optionsPanel != null)
			optionsPanel.SetActive(false);

        if (gameSelectionPanel != null)
            gameSelectionPanel.SetActive(false);

        if (saveSlotPanel != null)
            saveSlotPanel.SetActive(false);

        // Make sure cursor is visible and unlocked in menu
        Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		// Auto-find buttons if not assigned
		AutoFindButtons();

		// Add click listeners
		AddButtonListeners();

		// Load save file info to display on buttons
		UpdateSaveSlotInfo();
	}

    void AutoFindButtons()
    {
        // Find main menu buttons
        if (playButton == null && mainMenuPanel != null)
            playButton = mainMenuPanel.transform.Find("PlayButton")?.GetComponent<Button>();

        if (optionsButton == null && mainMenuPanel != null)
            optionsButton = mainMenuPanel.transform.Find("OptionsButton")?.GetComponent<Button>();

        if (quitButton == null && mainMenuPanel != null)
            quitButton = mainMenuPanel.transform.Find("QuitButton")?.GetComponent<Button>();

        // Find game selection buttons
        if (newGameButton == null && gameSelectionPanel != null)
            newGameButton = gameSelectionPanel.transform.Find("NewGameButton")?.GetComponent<Button>();

        if (loadGameButton == null && gameSelectionPanel != null)
            loadGameButton = gameSelectionPanel.transform.Find("LoadGameButton")?.GetComponent<Button>();

        if (backToMainButton == null && gameSelectionPanel != null)
            backToMainButton = gameSelectionPanel.transform.Find("BackButton")?.GetComponent<Button>();

        // If save slot buttons not assigned, try to find them
        if (saveSlotButtons == null || saveSlotButtons.Length == 0)
        {
            if (saveSlotPanel != null)
            {
                saveSlotButtons = new Button[3];
                for (int i = 0; i < 3; i++)
                {
                    Transform slotTransform = saveSlotPanel.transform.Find($"SaveSlot{i + 1}");
                    if (slotTransform != null)
                        saveSlotButtons[i] = slotTransform.GetComponent<Button>();
                }
            }
        }
    }

    void AddButtonListeners()
    {
        // Main menu buttons
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenOptions);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        // Game selection buttons
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGameClicked);

        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(OnLoadGameClicked);

        if (backToMainButton != null)
            backToMainButton.onClick.AddListener(BackToMainMenu);

        // Save slot buttons
        if (saveSlotButtons != null)
        {
            for (int i = 0; i < saveSlotButtons.Length; i++)
            {
                if (saveSlotButtons[i] != null)
                {
                    int slotIndex = i; // Capture the index for the lambda
                    saveSlotButtons[i].onClick.AddListener(() => OnSaveSlotClicked(slotIndex));
                }
            }
        }
    }



    // Called when Play button is clicked
    void OnPlayClicked()
    {
        Debug.Log("Play Game button clicked!");

        // Hide main menu buttons, show game selection panel
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (gameSelectionPanel != null)
            gameSelectionPanel.SetActive(true);
    }

    // Called when New Game button is clicked
    void OnNewGameClicked()
    {
        Debug.Log("New Game button clicked!");
        isNewGameMode = true;

        // Hide game selection panel, show save slots
        if (gameSelectionPanel != null)
            gameSelectionPanel.SetActive(false);

        ShowSaveSlots();
    }

    // Called when Load Game button is clicked
    void OnLoadGameClicked()
    {
        Debug.Log("Load Game button clicked!");
        isNewGameMode = false;

        // Hide game selection panel, show save slots
        if (gameSelectionPanel != null)
            gameSelectionPanel.SetActive(false);

        ShowSaveSlots();
    }

    void ShowSaveSlots()
    {
        if (saveSlotPanel != null)
            saveSlotPanel.SetActive(true);

        // Update save slot information (show if slots have saved data)
        UpdateSaveSlotInfo();
    }

    void UpdateSaveSlotInfo()
    {
        // This is where you would load save file data from PlayerPrefs or a save system
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            if (saveSlotTexts != null && i < saveSlotTexts.Length && saveSlotTexts[i] != null)
            {
                // Check if this save slot has data
                bool hasSaveData = CheckIfSaveSlotExists(i);

                if (hasSaveData)
                {
                    // You can display additional info like playtime, level, etc.
                    string levels = PlayerPrefs.GetString($"{i}_LevelsCompleted", "0");
                    float score = PlayerPrefs.GetFloat($"{i}_score", 0f);
                    saveSlotTexts[i].text = $"Slot {i + 1}\nLevels Completed: {levels}";
                }
                else
                {
                    saveSlotTexts[i].text = $"Slot {i + 1}\n[Empty]";
                }
            }
        }
    }

    bool CheckIfSaveSlotExists(int slotIndex)
    {
        // Example using PlayerPrefs - replace with your actual save system
        return PlayerPrefs.HasKey($"SaveSlot_{slotIndex}_Exists") &&
               PlayerPrefs.GetInt($"SaveSlot_{slotIndex}_Exists") == 1;
    }

    void OnSaveSlotClicked(int slotIndex)
    {
        Debug.Log($"Save Slot {slotIndex + 1} clicked! Mode: {(isNewGameMode ? "New Game" : "Load Game")}");

        bool slotHasData = CheckIfSaveSlotExists(slotIndex);

        // If in Load Game mode but slot is empty, treat it as New Game
        if (!isNewGameMode && !slotHasData)
        {
            Debug.Log("Empty slot selected in Load Game mode. Starting New Game instead.");
            isNewGameMode = true;
        }

        // Save the selected slot index for the next scene
        PlayerPrefs.SetInt($"SaveSlot_{slotIndex}_Exists", 1);
        PlayerPrefs.SetInt("SelectedSaveSlot", slotIndex);
        if (isNewGameMode)
        {
            PlayerPrefs.SetInt($"{slotIndex}_LevelsCompleted", 0);
            PlayerPrefs.SetFloat($"{slotIndex}_score", 0);
        }
        PlayerPrefs.Save();

        // Start the game
        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
	{
        // Simple fade out if we have a fade image
        if (fadeImage != null)
        {
            float elapsedTime = 0f;
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
            fadeImage.gameObject.SetActive(true);

            while (elapsedTime < transitionDelay)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsedTime / transitionDelay);
                fadeImage.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(transitionDelay);
        }

        // Load the game scene
        SceneManager.LoadScene("LevelSelect"); 
    }

	// Method for Options button
	public void OpenOptions()
	{
		Debug.Log("Options button clicked!");
		if (mainMenuPanel != null)
			mainMenuPanel.SetActive(false);

		if (optionsPanel != null)
			optionsPanel.SetActive(true);
	}

	// Method for Back button in options
	public void BackToMainMenu()
	{
        // Hide all secondary panels
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (gameSelectionPanel != null)
            gameSelectionPanel.SetActive(false);

        if (saveSlotPanel != null)
            saveSlotPanel.SetActive(false);

        // Show main menu
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

	// Method for Quit button
	public void QuitGame()
	{
		Debug.Log("Quit Game button clicked!");

#if UNITY_EDITOR
            // If we're in the Unity Editor, stop playing
            UnityEditor.EditorApplication.isPlaying = false;
#else
		// If we're in a built game, quit the application
		Application.Quit();
#endif
	}
}