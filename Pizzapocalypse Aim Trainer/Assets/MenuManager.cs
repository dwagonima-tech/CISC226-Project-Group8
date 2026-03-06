using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenu : MonoBehaviour
{
	[Header("UI Panels - Drag these from Hierarchy")]
	public GameObject mainMenuPanel;    // The panel containing your buttons
	public GameObject optionsPanel;     // An options panel (create this later)

	[Header("Buttons - Optional reference")]
	public Button playButton;
	public Button optionsButton;
	public Button quitButton;
	public Button backButton;           // For returning from options

	[Header("Transition Settings")]
	public float transitionDelay = 1f;   // How long to wait before loading
	public Animator transitionAnimator;  // Optional fade animator
	public Image fadeImage;              // Optional simple fade image

	void Start()
	{
		// Safety check - make sure panels are set up correctly
		if (mainMenuPanel == null)
			Debug.LogError("MainMenuPanel not assigned in MainMenu script!");
		else
			mainMenuPanel.SetActive(true);

		if (optionsPanel != null)
			optionsPanel.SetActive(false);

		// Make sure cursor is visible and unlocked in menu
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		// Auto-find buttons if not assigned
		if (playButton == null && mainMenuPanel != null)
			playButton = mainMenuPanel.transform.Find("PlayButton")?.GetComponent<Button>();

		if (optionsButton == null && mainMenuPanel != null)
			optionsButton = mainMenuPanel.transform.Find("OptionsButton")?.GetComponent<Button>();

		if (quitButton == null && mainMenuPanel != null)
			quitButton = mainMenuPanel.transform.Find("QuitButton")?.GetComponent<Button>();

		// Add click listeners programmatically (alternative to Inspector setup)
		if (playButton != null)
			playButton.onClick.AddListener(PlayGame);

		if (optionsButton != null)
			optionsButton.onClick.AddListener(OpenOptions);

		if (quitButton != null)
			quitButton.onClick.AddListener(QuitGame);

		if (backButton != null)
			backButton.onClick.AddListener(BackToMainMenu);
	}

	// Method for Play button
	public void PlayGame()
	{
		Debug.Log("Play Game button clicked!"); // For testing
		StartCoroutine(LoadGameScene());
	}

	IEnumerator LoadGameScene()
	{
		// Simple fade out if we have a fade image
		if (fadeImage != null)
		{
			float elapsedTime = 0f;
			Color color = fadeImage.color;

			while (elapsedTime < transitionDelay)
			{
				elapsedTime += Time.deltaTime;
				float alpha = Mathf.Lerp(0f, 1f, elapsedTime / transitionDelay);
				fadeImage.color = new Color(color.r, color.g, color.b, alpha);
				yield return null;
			}
		}

		// Wait a moment
		yield return new WaitForSeconds(transitionDelay);

		// Load the game scene 
		SceneManager.LoadScene("MainGame");
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
		Debug.Log("Back button clicked!");
		if (mainMenuPanel != null)
			mainMenuPanel.SetActive(true);

		if (optionsPanel != null)
			optionsPanel.SetActive(false);
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