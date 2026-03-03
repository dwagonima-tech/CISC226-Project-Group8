using UnityEngine;
using System.Collections.Generic;

public class SequenceSpawner : MonoBehaviour
{
	public GameObject circlePrefab;
	public float circleScale = 0.8f;

	[Header("Spawn Area")]
	public float minX = -8f;
	public float maxX = 8f;
	public float minY = -4f;
	public float maxY = 4f;

	[Header("Game Settings")]
	public int numberOfCircles = 5; // How many circles to spawn
	public float delayBetweenRounds = 0.5f; // Delay before next circle activates

	private List<GameObject> circles = new List<GameObject>();
	private int circlesClicked = 0;
	private bool gameActive = false;

	void Start()
	{
		if (circlePrefab == null)
		{
			Debug.LogError("Circle Prefab is not assigned!");
			return;
		}

		StartNewGame();
	}

	public void StartNewGame()
	{
		// Clear any existing circles
		ClearCircles();

		// Spawn all circles
		for (int i = 0; i < numberOfCircles; i++)
		{
			SpawnCircle();
		}

		// Start the sequence
		circlesClicked = 0;
		gameActive = true;
		Invoke("ActivateRandomCircle", delayBetweenRounds);
	}

	void SpawnCircle()
	{
		// Generate random position (with a few attempts to avoid overlap)
		Vector2 randomPosition;
		int attempts = 0;
		bool validPosition;

		do
		{
			validPosition = true;
			float randomX = Random.Range(minX, maxX);
			float randomY = Random.Range(minY, maxY);
			randomPosition = new Vector2(randomX, randomY);

			// Check if too close to other circles
			foreach (GameObject circle in circles)
			{
				if (circle != null && Vector2.Distance(circle.transform.position, randomPosition) < circleScale * 2)
				{
					validPosition = false;
					break;
				}
			}

			attempts++;
			if (attempts > 100) break; // Prevent infinite loop
		} while (!validPosition);

		GameObject newCircle = Instantiate(circlePrefab, randomPosition, Quaternion.identity);
		newCircle.transform.localScale = new Vector3(circleScale, circleScale, circleScale);

		// Ensure it has our SequenceCircle script
		SequenceCircle seqCircle = newCircle.GetComponent<SequenceCircle>();
		if (seqCircle == null)
		{
			seqCircle = newCircle.AddComponent<SequenceCircle>();
		}

		circles.Add(newCircle);
	}

	void ActivateRandomCircle()
	{
		if (!gameActive) return;

		// Get all circles that are still active in the scene (not destroyed)
		List<GameObject> availableCircles = new List<GameObject>();

		foreach (GameObject circle in circles)
		{
			if (circle != null)
			{
				SequenceCircle seq = circle.GetComponent<SequenceCircle>();
				if (seq != null && !seq.wasClicked)
				{
					availableCircles.Add(circle);
				}
			}
		}

		// If there are inactive circles left, activate one randomly
		if (availableCircles.Count > 0)
		{
			int randomIndex = Random.Range(0, availableCircles.Count);
			SequenceCircle randomCircle = availableCircles[randomIndex].GetComponent<SequenceCircle>();
			randomCircle.SetActive(true);
			Debug.Log($"Activated circle {circlesClicked + 1} of {numberOfCircles}");
		}
		else
		{
			// All circles have been clicked!
			GameComplete();
		}
	}

	public void CircleClicked(GameObject clickedCircle)
	{
		circlesClicked++;
		Debug.Log($"Clicked {circlesClicked} of {numberOfCircles}");

		// Remove the clicked circle from our list (it will be destroyed)
		circles.Remove(clickedCircle);

		if (circlesClicked >= numberOfCircles)
		{
			GameComplete();
		}
		else
		{
			// Activate next circle after a short delay
			Invoke("ActivateRandomCircle", delayBetweenRounds);
		}
	}

	void GameComplete()
	{
		Debug.Log("GAME COMPLETE!");
		gameActive = false;

		// You could add effects, sounds, or restart option here

		// Auto-restart after 2 seconds
		Invoke("RestartGame", 2f);
	}

	void RestartGame()
	{
		Debug.Log("Restarting...");
		StartNewGame();
	}

	void ClearCircles()
	{
		foreach (GameObject circle in circles)
		{
			if (circle != null)
			{
				Destroy(circle);
			}
		}
		circles.Clear();
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Vector2 center = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
		Vector2 size = new Vector2(maxX - minX, maxY - minY);
		Gizmos.DrawWireCube(center, size);
	}
}
