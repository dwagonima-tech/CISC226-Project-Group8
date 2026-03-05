using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Put the name OR build index of your gameplay scene here
    [SerializeField] private string gameSceneName = "Game"; // change to your scene name

    public void PlayGame()
    {
        // Loads the gameplay scene
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        // Quits the built game (does nothing in the editor)
        Debug.Log("QuitGame() called");
        Application.Quit();
    }

    // Optional: a button to return to menu from other scenes
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // change if your menu scene is named differently
    }
}
