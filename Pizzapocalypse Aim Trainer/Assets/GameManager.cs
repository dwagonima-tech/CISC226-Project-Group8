using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
	[Header("Player Stats")]
	public int playerMaxHP = 50;
	public int playerCurrentHP;
	public int playerBaseDamage = 10;
	private float score = 0f;

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
	private int defenseTurnsRemaining = 0;
	private float damageReduction = 0.5f;

	private GameObject robotUsed;
	private Animator RobotAnimator;

	public GameObject plantGuy;
	private Animator plantAnimator;

	[Header("Level Attributes")]
	public int currentLevel;
	private int saveSlot;
	private bool isPlayerTurn = true;
	private bool isMinigameActive = false;
	private string currentAction = "";

	// References to minigame scripts
	private ClickMinigame clickMinigame;
	private SequenceMinigame sequenceMinigame;
	private TrackingMinigame trackingMinigame;

	void Start()
	{
		saveSlot = PlayerPrefs.GetInt("SelectedSaveSlot");
		string currentBot = PlayerPrefs.GetString("selectedBot");

		FindSpawnedRobot();

		setupAttributes(currentBot);

		if (robotUsed != null)
		{
			RobotAnimator = robotUsed.GetComponent<Animator>();
			if (RobotAnimator == null)
			{
				Debug.LogError("Robot prefab missing Animator!");
			}
			else
			{
				Debug.Log("Animator Found!");
			}

		}
		else
		{
			Debug.LogError("Failed to find spawned robot in scene!");
		}


		plantAnimator = plantGuy.GetComponent<Animator>();
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

	void FindSpawnedRobot()
	{
		GameObject robotByTag = GameObject.FindGameObjectWithTag("Player");

		if (robotByTag != null)
		{
			robotUsed = robotByTag;
			Debug.Log("Found robot by tag: " +  robotUsed.name);
			return;
		}
	}

	void setupAttributes(string bot)
	{
		if (bot == "Jerry")
		{
			playerMaxHP = 150;
			playerBaseDamage = 25;
			damageReduction = 0.5f;
		}
		else if (bot == "Paul")
		{
			playerMaxHP = 100;
			playerBaseDamage = 45;
			damageReduction = 0.75f;
		}
		else if (bot == "Harold")
		{
			playerMaxHP = 200;
			playerBaseDamage = 15;
			damageReduction = 0.25f;
		}
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

		score += 100f * multiplier;


        messageText.text = $"You dealt {damageDealt} damage! (x{multiplier:F1})";

		UpdateUI();

		PlayCombatAnimations();


		if (enemyCurrentHP <= 0)
		{
			enemyCurrentHP = 0;
			Victory();
			return;
		}


        Invoke("EnemyTurn", 2.5f);
	}

	void ProcessDefend(float multiplier)
	{
		
		if (defenseTurnsRemaining == 0)
		{

            messageText.text = $"Defense up! Reducing damage to {damageReduction * 100}% for {Mathf.RoundToInt(multiplier)} turns.";
        }
		else

			messageText.text = $"Defense up! Reducing damage to {damageReduction * 100}% for an additional {Mathf.RoundToInt(multiplier)} turns.";

        defenseTurnsRemaining += Mathf.RoundToInt(multiplier);
		

        Invoke("EnemyTurn", 2.5f);
	}

	void EnemyTurn()
	{
		plantAnimator.SetTrigger("goBackToIdle");

		isPlayerTurn = false;

		bool defenseWasActive = defenseTurnsRemaining > 0;

		int damageToPlayer = enemyBaseDamage;

		if (defenseWasActive)
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

		PlayCombatAnimations(defenseWasActive);

		if (playerCurrentHP <= 0)
		{
			playerCurrentHP = 0;
            GameOver();
			return;
		}


        Invoke("PlayerTurn", 2.5f);
	}

	void PlayerTurn()
	{
		isPlayerTurn = true;
		currentAction = "";
        RobotAnimator.SetTrigger("BackToIdle");
		plantAnimator.SetTrigger("goBackToIdle");
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
		int hasBeatGame = PlayerPrefs.GetInt($"{saveSlot}_hasBeatGame");
        float oldScore = PlayerPrefs.GetFloat($"{saveSlot}_score");

        float newScore = score + oldScore;

        if (  hasBeatGame == 0 && currentLevel != 7 || hasBeatGame == 1)
		{

            int levelsCompleted = PlayerPrefs.GetInt($"{saveSlot}_LevelsCompleted");

            if (currentLevel > levelsCompleted)
            {
                PlayerPrefs.SetInt($"{saveSlot}_LevelsCompleted", currentLevel);
            }

            PlayerPrefs.SetFloat($"{saveSlot}_score", newScore);

            PlayerPrefs.Save();
            Debug.Log("We have completed " + PlayerPrefs.GetInt($"{saveSlot}_LevelsCompleted"));
            SceneManager.LoadScene("LevelSelect");
        }
		else if (hasBeatGame == 0 && currentLevel == 7)
		{
            int levelsCompleted = PlayerPrefs.GetInt($"{saveSlot}_LevelsCompleted");

            if (currentLevel > levelsCompleted)
            {
                PlayerPrefs.SetInt($"{saveSlot}_LevelsCompleted", currentLevel);
            }

            PlayerPrefs.SetFloat($"{saveSlot}_score", newScore);
			PlayerPrefs.SetInt($"{saveSlot}_hasBeatGame", 1);

            PlayerPrefs.Save();
            Debug.Log("We have completed " + PlayerPrefs.GetInt($"{saveSlot}_LevelsCompleted"));
            SceneManager.LoadScene("winCutscene");
        }
		

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

	public void OnRobotSpawned(GameObject spawnedRobot)
	{
		robotUsed = spawnedRobot;
		RobotAnimator = robotUsed.GetComponent<Animator>();

		if (RobotAnimator != null)
		{
			Debug.Log("Robot received from spawner: " + robotUsed.name);
		}
		else
		{
			Debug.LogError("Robot prefab missing Animator!");
		}
	}

	private void PlayCombatAnimations(bool enemyAttackDefenseActive = false)
	{
		// Death first
		if (playerCurrentHP <= 0)
		{
			RobotAnimator.SetTrigger("DeadTrigger");
			return;
		}
		if (enemyCurrentHP <= 0)
		{
			plantAnimator.SetTrigger("dieTrigger");
			return;
		}

		// Not dead
		if (isPlayerTurn)
		{	
			if (currentAction == "attack")
			{
				// Player attack
				RobotAnimator.SetTrigger("AttackTrigger");
				plantAnimator.SetTrigger("hitTrigger");
			}
			else if (currentAction == "defend")
			{
				RobotAnimator.SetTrigger("DefendTrigger");
			}
				
		}
		else if (!isPlayerTurn)
		{
			// Enemy attack
			plantAnimator.SetTrigger("attackTrigger");
			if (enemyAttackDefenseActive)
			{
				RobotAnimator.SetTrigger("DefendTrigger");
			}
			else
			{
				RobotAnimator.SetTrigger("BackToIdle");
			}
		}
	}
}