using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PathSpawner : MonoBehaviour
{
	public GameObject circlePrefab;
	public float circleScale = 0.5f;

	[Header("Path Generation")]
	public int numberOfCircles = 10;
	public float pathLength = 5f; // How squiggly the path is
	public float stepDistance = 1f; // Distance between circles
	public float curveIntensity = 0.8f; // How much the path curves

	[Header("Spawn Area")]
	public float minX = -7f;
	public float maxX = 7f;
	public float minY = -4f;
	public float maxY = 4f;

	[Header("Game Settings")]
	public float resetDelay = 1f; // Delay before resetting on mistake

	private List<PathCircle> pathCircles = new List<PathCircle>();
	private int currentIndex = 0;
	public bool isTracking = false;
	private bool gameComplete = false;
	private Camera mainCamera;


	void Start()
	{
		mainCamera = Camera.main;

		if (circlePrefab == null)
		{
			Debug.LogError("Circle Prefab is not assigned!");
			return;
		}

		GenerateNewPath();
	}

	void Update()
	{
		// Check for mouse button release
		if (isTracking && Mouse.current.leftButton.wasReleasedThisFrame)
		{
			// Player let go early - reset
			if (!gameComplete)
			{
				Debug.Log("Released too early! Resetting...");
				ResetPath();
			}
		}

	}

	void GenerateNewPath()
	{
		// Clear existing path
		ClearPath();

		// Generate random start position within bounds
		float startX = Random.Range(minX + 2f, maxX - 2f);
		float startY = Random.Range(minY + 2f, maxY - 2f);
		Vector2 currentPos = new Vector2(startX, startY);

		// Create the path circles
		for (int i = 0; i < numberOfCircles; i++)
		{
			// Calculate next position with some randomness for squiggle
			float angle = i * 0.5f; // Base angle
																	// Add Perlin noise for organic squiggle
			float noiseX = Mathf.PerlinNoise(i * 0.3f, 0) * 2 - 1;
			float noiseY = Mathf.PerlinNoise(0, i * 0.3f) * 2 - 1;

			Vector2 direction = new Vector2(
				Mathf.Sin(angle) * curveIntensity + noiseX * 0.5f,
				Mathf.Cos(angle * 0.7f) * curveIntensity + noiseY * 0.5f
				).normalized;

			Vector2 nextPos = currentPos + direction * stepDistance;

			// Keep within bounds by reflecting if needed
			if (nextPos.x < minX + 1f || nextPos.x > maxX - 1f ||
				nextPos.y < minY + 1f || nextPos.y > maxY - 1f)
			{
				// Bounce off boundaries
				direction = -direction;
				nextPos = currentPos + direction * stepDistance;
			}

			// Create circle at current position
			GameObject circle = Instantiate(circlePrefab, currentPos, Quaternion.identity);
			circle.transform.localScale = new Vector3(circleScale, circleScale, circleScale);

			// Add and configure PathCircle component
			PathCircle pathCircle = circle.GetComponent<PathCircle>();
			if (pathCircle == null)
				pathCircle = circle.AddComponent<PathCircle>();

			pathCircle.orderIndex = i;

			// Mark start and end
			if (i == 0)
				pathCircle.isStart = true;
			if (i == numberOfCircles - 1)
				pathCircle.isEnd = true;

			pathCircle.UpdateColor();
			pathCircles.Add(pathCircle);

			currentPos = nextPos;
		}

		// Reset game state
		currentIndex = 0;
		isTracking = false;
		gameComplete = false;

		Debug.Log("Path generated! Click and hold the green start circle.");
	}


	public void StartTracking(PathCircle startCircle)
	{
		if (startCircle != null && startCircle.isStart && !isTracking && !gameComplete)
		{
			Debug.Log("Starting tracking! Hold and drag through the path.");
			isTracking = true;
			startCircle.isTraced = true;
			startCircle.UpdateColor();
			currentIndex = 1; // Next circle to trace
		}
	}

	public void CircleEntered(PathCircle circle)
	{
		if (!isTracking || gameComplete) return;

		// Check if this is the next circle in sequence
		if (circle.orderIndex == currentIndex)
		{
			// Correct circle!
			if (!circle.isTraced)
			{
				circle.isTraced = true;
				circle.UpdateColor();

				currentIndex++;

				// Check if we've completed the path
				if (currentIndex >= numberOfCircles)
				{
					CompletePath();
				}
			}
		}
		else if (circle.orderIndex < currentIndex)
		{
			// Already traced - ignore (allow backtracking over traced circles)
			return;
		}
		else
		{
			// Wrong circle - out of order!
			Debug.Log($"Wrong circle! Expected index {currentIndex}, got {circle.orderIndex}");
			StartCoroutine(ResetAfterMistake(circle));
		}
	}

	void CompletePath()
	{
		Debug.Log("PATH COMPLETE! Great tracking!");
		gameComplete = true;
		isTracking = false;

		// Highlight end circle or play effect
		if (pathCircles.Count > 0)
		{
			PathCircle endCircle = pathCircles[pathCircles.Count - 1];
			StartCoroutine(CompleteFlash(endCircle));
		}

		// Generate new path after delay
		Invoke("GenerateNewPath", 2f);
	}

	System.Collections.IEnumerator CompleteFlash(PathCircle endCircle)
	{
		// Flash effect on completion
		for (int i = 0; i < 3; i++)
		{
			endCircle.UpdateColor();
			yield return new WaitForSeconds(0.2f);
		}
	}

	System.Collections.IEnumerator ResetAfterMistake(PathCircle wrongCircle)
	{
		isTracking = false;

		// Flash the wrong circle
		wrongCircle.FlashWrong();

		// Wait a moment
		yield return new WaitForSeconds(resetDelay);

		// Reset the path
		ResetPath();
	}

	void ResetPath()
	{
		Debug.Log("Resetting path...");

		// Reset all circles
		foreach (PathCircle circle in pathCircles)
		{
			if (circle != null)
			{
				circle.isTraced = false;
				circle.UpdateColor();
			}
		}

		// Reset game state
		currentIndex = 0;
		isTracking = false;
		gameComplete = false;

		Debug.Log("Path reset! Click and hold the green start circle.");
	}

	void ClearPath()
	{
		foreach (PathCircle circle in pathCircles)
		{
			if (circle != null)
				Destroy(circle.gameObject);
		}
		pathCircles.Clear();
	}

	// Visualize spawn area in editor
	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Vector2 center = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
		Vector2 size = new Vector2(maxX - minX, maxY - minY);
		Gizmos.DrawWireCube(center, size);
	}
}