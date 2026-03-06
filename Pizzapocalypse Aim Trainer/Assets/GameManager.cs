using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;

public class GameManager : MonoBehaviour
{
	[Header("Player Stats")]
	public int playerMaxHP = 50;
	public int playerCurrentHP;
	public int playerBaseDamage = 10;

	[Header("Enemy Stats")]
	public int enemyMaxHP = 200;
	public int enemyCurrentHP;
	public int enemyBaseDamage = 8;

	[Header("UI References")]
	public Slider playerHealthBar;
	public Slider enemyHealthBar;
	public TMP_Text playerHealthText;
	public TMP_Text enemyHealthText;
	public TMP_Text messageText;
	public GameObject battleButton;
	public GameObject defendButton;

	[Header("Minigame Panels")]
	public GameObject clickMinigamePanel;      // Your first minigame (click circles)
	public GameObject sequenceMinigamePanel;   // Your second minigame (green circle sequence)
	public GameObject trackingMinigamePanel;   // Your third minigame (path tracking)

	[Header("Battle Modifiers")]
	public float defenseReductionDuration = 2;
	private int defenseTurnsRemaining = 0;
	private float damageReduction = 1f;

	private bool isPlayerTurn = true;
	private bool isMinigameActive = false;
	private string currentAction = "";

	// References to minigame scripts
	private ClickMinigame clickMinigame;
	private SequenceMinigame sequenceMinigame;
	private TrackingMinigame trackingMinigame;

	void Start()
	{
		playerCurrentHP = playerMaxHP;
		enemyCurrentHP = enemyMaxHP;


		// Get references to minigame scripts
		if (clickMinigamePanel != null)
			clickMinigame = clickMinigamePanel.GetComponent<ClickMinigame>();

		if (sequenceMinigamePanel != null)
			sequenceMinigame = sequenceMinigamePanel.GetComponent<SequenceMinigame>();

		if (trackingMinigamePanel != null)
			trackingMinigame = trackingMinigamePanel.GetComponent<TrackingMinigame>();

		// Ensure all minigame panels start hidden
		HideAllMinigamePanels();

		UpdateUI();
		messageText.text = "Your turn! Choose an action.";
		Debug.Log("GameManager Started - Buttons should be active");
	}



	void HideAllMinigamePanels()
	{
		if (clickMinigamePanel != null)
			clickMinigamePanel.SetActive(false);

		if (sequenceMinigamePanel != null)
			sequenceMinigamePanel.SetActive(false);

		if (trackingMinigamePanel != null)
			trackingMinigamePanel.SetActive(false);
	}

	public void OnBattleButton()
	{

		if (!isPlayerTurn || isMinigameActive) return;

		currentAction = "attack";
		StartRandomMinigame();
	}

	public void OnDefendButton()
	{
		if (!isPlayerTurn || isMinigameActive) return;

		currentAction = "defend";
		StartRandomMinigame();
	}

	void StartRandomMinigame()
	{
		isMinigameActive = true;
		battleButton.SetActive(false);
		defendButton.SetActive(false);
		messageText.text = "Complete the minigame!";

		// Choose random minigame
		int randomIndex = Random.Range(0, 3); // 0, 1, or 2

		switch (randomIndex)
		{
			case 0:
				if (clickMinigamePanel != null)
				{
					clickMinigamePanel.SetActive(true);
					if (clickMinigame != null)
						clickMinigame.StartMinigame();
				}
				break;

			case 1:
				if (sequenceMinigamePanel != null)
				{
					sequenceMinigamePanel.SetActive(true);
					if (sequenceMinigame != null)
						sequenceMinigame.StartMinigame();
				}
				break;

			case 2:
				if (trackingMinigamePanel != null)
				{
					trackingMinigamePanel.SetActive(true);
					if (trackingMinigame != null)
						trackingMinigame.StartMinigame();
				}
				break;
		}
	}

	// Called by minigames when completed
	public void MinigameComplete(float performanceMultiplier)
	{
		HideAllMinigamePanels();

		// Process the action
		if (currentAction == "attack")
		{
			ProcessAttack(performanceMultiplier);
		}
		else if (currentAction == "defend")
		{
			ProcessDefend(performanceMultiplier);
		}

		isMinigameActive = false;
	}

	void ProcessAttack(float multiplier)
	{
		int damageDealt = Mathf.RoundToInt(playerBaseDamage * multiplier);
		enemyCurrentHP -= damageDealt;

		messageText.text = $"You dealt {damageDealt} damage! (x{multiplier:F1})";

		UpdateUI();

		if (enemyCurrentHP <= 0)
		{
			enemyCurrentHP = 0;
			Victory();
			return;
		}

		Invoke("EnemyTurn", 1.5f);
	}

	void ProcessDefend(float multiplier)
	{
		damageReduction = 1f / multiplier;
		defenseTurnsRemaining = 2;

		messageText.text = $"Defense up! Reducing damage to {damageReduction * 100}% for 2 turns.";

		Invoke("EnemyTurn", 1.5f);
	}

	void EnemyTurn()
	{
		isPlayerTurn = false;

		int damageToPlayer = enemyBaseDamage;

		if (defenseTurnsRemaining > 0)
		{
			damageToPlayer = Mathf.RoundToInt(damageToPlayer * damageReduction);
			defenseTurnsRemaining--;

			if (defenseTurnsRemaining <= 0)
			{
				damageReduction = 1f;
				messageText.text = $"Enemy deals {damageToPlayer} damage! Defense wore off.";
			}
			else
			{
				messageText.text = $"Enemy deals {damageToPlayer} damage! ({defenseTurnsRemaining} turns of defense remaining)";
			}
		}
		else
		{
			messageText.text = $"Enemy deals {damageToPlayer} damage!";
		}

		playerCurrentHP -= damageToPlayer;
		UpdateUI();

		if (playerCurrentHP <= 0)
		{
			playerCurrentHP = 0;
			GameOver();
			return;
		}

		Invoke("PlayerTurn", 1.5f);
	}

	void PlayerTurn()
	{
		isPlayerTurn = true;
		battleButton.SetActive(true);
		defendButton.SetActive(true);
		messageText.text = "Your turn! Choose an action.";
	}

	void Victory()
	{
		messageText.text = "VICTORY! You defeated the enemy!";
		battleButton.SetActive(false);
		defendButton.SetActive(false);
		Invoke("ResetGame", 3f);
	}

	void GameOver()
	{
		messageText.text = "GAME OVER! You were defeated...";
		battleButton.SetActive(false);
		defendButton.SetActive(false);
		Invoke("ResetGame", 3f);
	}

	void ResetGame()
	{
		playerCurrentHP = playerMaxHP;
		enemyCurrentHP = enemyMaxHP;
		defenseTurnsRemaining = 0;
		damageReduction = 1f;

		UpdateUI();
		PlayerTurn();
	}

	void UpdateUI()
	{
		if (playerHealthBar != null)
		{
			playerHealthBar.maxValue = playerMaxHP;
			playerHealthBar.value = playerCurrentHP;
		}

		if (enemyHealthBar != null)
		{
			enemyHealthBar.maxValue = enemyMaxHP;
			enemyHealthBar.value = enemyCurrentHP;
		}

		if (playerHealthText != null)
			playerHealthText.text = $"{playerCurrentHP}/{playerMaxHP}";

		if (enemyHealthText != null)
			enemyHealthText.text = $"{enemyCurrentHP}/{enemyMaxHP}";
	}
}