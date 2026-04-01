using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;

public class LevelManager : MonoBehaviour
{
    private int slot;
    public GameObject mapPanel;
    public GameObject robotPanel;
    public Button[] robotButtons = new Button[3];

    public Button[] levelButtons;

    private TaskCompletionSource<bool> robotSelectionTcs;
    private int pendingLevel;
    

    private void Awake()
    {
        slot = PlayerPrefs.GetInt("SelectedSaveSlot");
    }
    void Start()
    {
        
        robotPanel.SetActive(false);    
        int levelsCompleted = PlayerPrefs.GetInt($"{slot}_LevelsCompleted");
        activateMap(levelsCompleted);

        AddButtonListeners();
        
    }

    void AddButtonListeners()
    {
        // Level buttons
        if (levelButtons[0] != null)
            levelButtons[0].onClick.AddListener(() => onLevelClicked(1));

        if (levelButtons[1] != null)
            levelButtons[1].onClick.AddListener(() => onLevelClicked(2));

        if (levelButtons[2] != null)
            levelButtons[2].onClick.AddListener(() => onLevelClicked(3));

        if (levelButtons[3] != null)
            levelButtons[3].onClick.AddListener(() => onLevelClicked(4));

        if (levelButtons[4] != null)
            levelButtons[4].onClick.AddListener(() => onLevelClicked(5));

        if (levelButtons[5] != null)
            levelButtons[5].onClick.AddListener(() => onLevelClicked(6));

		if (levelButtons[6] != null)
			levelButtons[6].onClick.AddListener(() => onLevelClicked(6));

		// Robot buttons
		if (robotButtons[0] != null)
            robotButtons[0].onClick.AddListener(() => OnRobotSelected("Jerry"));

		if (robotButtons[1] != null)
			robotButtons[1].onClick.AddListener(() => OnRobotSelected("Paul"));

		if (robotButtons[2] != null)
			robotButtons[2].onClick.AddListener(() => OnRobotSelected("Harold"));
	}


    public async void onLevelClicked(int level)
    {
        pendingLevel = level;
        await selectRobot();
        SceneManager.LoadScene($"Level0{level}");
    }

    async Task selectRobot()
    {
        robotPanel.SetActive(true);
        robotSelectionTcs = new TaskCompletionSource<bool> ();

        // wait for the robot selection to complete
        await robotSelectionTcs.Task;

        robotPanel.SetActive(false);
    }

    void OnRobotSelected(string robotName)
    {
        PlayerPrefs.SetString("selectedBot", robotName);

        //Signal that selection is complete
        if (robotSelectionTcs != null && !robotSelectionTcs.Task.IsCompleted) 
        {
            robotSelectionTcs.SetResult(true);
        }
    }

    void activateMap(int levelsCompleted)
    {   
        if (levelsCompleted > 0)
        {
            for (int i = 0; i < levelsCompleted+1; i++)
            {
                levelButtons[i].interactable = true;
                Debug.Log("Level " + i + " Should be active");

            }
            for (int i = levelsCompleted+1; i < 7; i++)
            {
                levelButtons[i].interactable = false;
            }
        }
        else
        {
            levelButtons[0].interactable = true;
            for (int i = 1; i < 7; i++)
            {
                levelButtons[i].interactable = false;
            }
        }

    }
    

}
