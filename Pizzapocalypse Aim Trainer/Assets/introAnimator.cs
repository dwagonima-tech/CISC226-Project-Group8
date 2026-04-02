using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class introAnimator : MonoBehaviour
{
    [Header("Text Objects")]
    [SerializeField] private List<TextMeshProUGUI> textObjects; // Assign all 7 TextMeshPro objects here

    [Header("Timing Settings")]
    [SerializeField] private float initialDelay = 1f; // Time before first text appears
    [SerializeField] private float textDisplayDuration = 4f; // How long each text stays visible
    [SerializeField] private float fadeDuration = 1f; // Duration of fade in/out transitions

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "LevelSelect";

    private CanvasGroup[] canvasGroups;

    void Start()
    {
        // Ensure all text starts invisible
        InitializeTextObjects();

        // Start the intro sequence
        StartCoroutine(PlayIntroSequence());
    }

    private void InitializeTextObjects()
    {
        canvasGroups = new CanvasGroup[textObjects.Count];

        for (int i = 0; i < textObjects.Count; i++)
        {
            // Add CanvasGroup component to each text object if not already present
            CanvasGroup cg = textObjects[i].GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = textObjects[i].gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroups[i] = cg;

            // Initially hide the text
            cg.alpha = 0f;
        }
    }

    private IEnumerator PlayIntroSequence()
    {
        // Initial delay before showing any text
        yield return new WaitForSeconds(initialDelay);

        // Show each text in sequence
        for (int i = 0; i < textObjects.Count; i++)
        {
            yield return StartCoroutine(ShowText(i));

            // If this is not the last text, wait before next one
            if (i < textObjects.Count - 1)
            {
                yield return new WaitForSeconds(textDisplayDuration);
            }
        }

        // Wait for the last text to finish displaying
        yield return new WaitForSeconds(textDisplayDuration);

        // Transition to the next scene
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator ShowText(int index)
    {
        float elapsedTime = 0f;
        CanvasGroup currentCG = canvasGroups[index];

        // Fade in
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            currentCG.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }
        currentCG.alpha = 1f;

        // Wait for the display duration
        yield return new WaitForSeconds(textDisplayDuration);

        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            currentCG.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }
        currentCG.alpha = 0f;
    }
}
