using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class SequenceMinigame : MonoBehaviour
{
	public GameObject circlePrefab;
	public int numberOfCircles = 5;
	public float delayBetweenRounds = 0.5f;
	public float timeLimit = 30f;

	private GameManager gameManager;
	private List<GameObject> circles = new List<GameObject>();
	private int circlesClicked = 0;
	private bool gameActive = false;
	private float startTime;

	// Fixed spawn area
	public float minX = -6f;
	public float maxX = 6f;
	public float minY = -3f;
	public float maxY = 3f;

	void Start()
	{
		gameManager = FindFirstObjectByType<GameManager>();
	}

	public void StartMinigame()
	{
		circlesClicked = 0;
		startTime = Time.time;

		ClearCircles();

		// Spawn all circles
		for (int i = 0; i < numberOfCircles; i++)
		{
			SpawnCircle(i);
		}

		gameActive = true;
		Invoke("ActivateRandomCircle", delayBetweenRounds);
		StartCoroutine(TimerRoutine());

		Debug.Log("Sequence Minigame Started");
	}

	void SpawnCircle(int index)
	{
		Vector2 randomPos = new Vector2(
			Random.Range(minX, maxX),
			Random.Range(minY, maxY)
		);

		GameObject circle = Instantiate(circlePrefab, randomPos, Quaternion.identity, transform);
		circle.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

		// CRITICAL: Add collider
		CircleCollider2D collider = circle.GetComponent<CircleCollider2D>();
		if (collider == null)
			circle.AddComponent<CircleCollider2D>();

		// Setup appearance
		SpriteRenderer renderer = circle.GetComponent<SpriteRenderer>();
		if (renderer != null)
			renderer.color = Color.gray;

		// Add handler
		SequenceCircleHandler handler = circle.AddComponent<SequenceCircleHandler>();
		handler.minigame = this;
		handler.circleIndex = index;
		handler.isActive = false;

		circles.Add(circle);
	}

	void ActivateRandomCircle()
	{
		if (!gameActive) return;

		// Find all inactive circles
		List<GameObject> inactiveCircles = new List<GameObject>();

		foreach (GameObject circle in circles)
		{
			if (circle != null)
			{
				SequenceCircleHandler handler = circle.GetComponent<SequenceCircleHandler>();
				if (handler != null && !handler.wasClicked)
				{
					inactiveCircles.Add(circle);
				}
			}
		}

		if (inactiveCircles.Count > 0)
		{
			int randomIndex = Random.Range(0, inactiveCircles.Count);
			SequenceCircleHandler handler = inactiveCircles[randomIndex].GetComponent<SequenceCircleHandler>();
			handler.SetActive(true);
			Debug.Log($"Activated circle {circlesClicked + 1}");
		}
	}

	IEnumerator TimerRoutine()
	{
		yield return new WaitForSeconds(timeLimit);

		if (gameActive && circlesClicked < numberOfCircles)
		{
			float score = (circlesClicked / (float)numberOfCircles) * 100f;
			CompleteMinigame(score);
		}
	}

	public void CircleClicked(int index)
	{
		if (!gameActive) return;

		SequenceCircleHandler handler = circles[index].GetComponent<SequenceCircleHandler>();

		if (handler != null && handler.isActive)
		{
			// Correct click
			circlesClicked++;
			handler.wasClicked = true;
			handler.isActive = false;

			// Change to green
			SpriteRenderer renderer = circles[index].GetComponent<SpriteRenderer>();
			if (renderer != null)
				renderer.color = Color.green;

			Debug.Log($"Correct! {circlesClicked}/{numberOfCircles}");

			if (circlesClicked >= numberOfCircles)
			{
				CompleteMinigame(100f);
			}
			else
			{
				Invoke("ActivateRandomCircle", delayBetweenRounds);
			}
		}
	}

	void CompleteMinigame(float score)
	{
		if (!gameActive) return;

		gameActive = false;
		CancelInvoke();
		StopAllCoroutines();

		float multiplier = 1f + (score / 100f) * 3f;

		ClearCircles();

		if (gameManager != null)
			gameManager.MinigameComplete(multiplier);

		gameObject.SetActive(false);
	}

	void ClearCircles()
	{
		foreach (GameObject circle in circles)
		{
			if (circle != null)
				Destroy(circle);
		}
		circles.Clear();
	}
}

public class SequenceCircleHandler : MonoBehaviour
{
	public SequenceMinigame minigame;
	public int circleIndex;
	public bool isActive = false;
	public bool wasClicked = false;

	private SpriteRenderer spriteRenderer;
	private Camera mainCamera;

	void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		mainCamera = Camera.main;
	}

	void Update()
	{
		if (isActive && Mouse.current.leftButton.wasPressedThisFrame)
		{
			Vector2 mousePos = Mouse.current.position.ReadValue();
			Vector2 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));

			Collider2D hit = Physics2D.OverlapPoint(worldPoint);

			if (hit != null && hit.gameObject == gameObject)
			{
				Debug.Log($"Sequence circle {circleIndex} clicked with Input System!");
				if (minigame != null)
					minigame.CircleClicked(circleIndex);
			}
		}
	}

	public void SetActive(bool active)
	{
		isActive = active;
		if (spriteRenderer != null)
			spriteRenderer.color = active ? Color.green : Color.gray;
	}
}