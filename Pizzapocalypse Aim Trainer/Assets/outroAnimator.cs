using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class outroAnimator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float delayBeforeSceneChange = 5f;
    [SerializeField] private string sceneToLoad = "MainMenu";

    [Header("Transition Effect")]
    [SerializeField] private Image transitionImage; // Your invisible image
    [SerializeField] private float fadeDuration = 1f;

    void Start()
    {
        // Make sure image starts invisible
        if (transitionImage != null)
        {
            Color color = transitionImage.color;
            color.a = 0f;
            transitionImage.color = color;
        }

        StartCoroutine(OutroSequence());
    }

    private IEnumerator OutroSequence()
    {
        // Wait for 5 seconds
        yield return new WaitForSeconds(delayBeforeSceneChange);

        // Fade to black (or whatever color your image is)
        if (transitionImage != null)
        {
            float elapsedTime = 0f;
            Color color = transitionImage.color;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                transitionImage.color = color;
                yield return null;
            }

            color.a = 1f;
            transitionImage.color = color;
        }

        // Load the next scene
        SceneManager.LoadScene(sceneToLoad);
    }
}