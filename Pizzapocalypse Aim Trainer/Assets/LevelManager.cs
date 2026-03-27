using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    private int slot;
    public GameObject mapPanel;

    public Button[] levelButtons;
    

    private void Awake()
    {
        slot = PlayerPrefs.GetInt("SelectedSaveSlot");
    }
    void Start()
    {
        

        int levelsCompleted = PlayerPrefs.GetInt($"{slot}_LevelsCompleted");
        activateMap(levelsCompleted);

        AddButtonListeners();
        
    }
    
    void AddButtonListeners()
    {
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
    }


    public void onLevelClicked(int level)
    {
        
        if (level == 1)
        {
            SceneManager.LoadScene("Level01");
        }
    }

    void activateMap(int levelsCompleted)
    {   //if (levelsCompleted > 0)
        //{
            for (int i = 0; i <= levelsCompleted; i++)
            {
                levelButtons[i].interactable = true;
                Debug.Log("Level " + i + " Should be active");

            }
            for (int i = levelsCompleted+1; i < 6; i++)
            {
                levelButtons[i].interactable = false;
            }
        //}
        //else
        //{
        //    levelButtons[0].interactable = true;
        //    for (int i = 1; i < 6; i++)
        //    {
        //        levelButtons[i].interactable = false;
        //    }
        //}

    }
    

}
