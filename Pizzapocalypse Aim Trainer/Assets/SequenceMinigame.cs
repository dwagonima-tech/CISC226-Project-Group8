using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class SequenceMinigame : MonoBehaviour
{
	public GameObject circlePrefab;
	public int numberOfCircles = 5;
    public float CircleSize = 1f;
    public float delayBetweenRounds = 0.5f;
	public float timeLimit = 30f;

	private GameManager gameManager;
	private List<GameObject> circles = new List<GameObject>();
	private int circlesClicked = 0;
	private bool gameActive = false;
	private float startTime;
	private bool isWaitingForAnimation = false;

	private List<Vector2> circlePositions = new List<Vector2>();

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
		isWaitingForAnimation = false;

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
		Vector2 randomPos = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));

		while (circlePositions.Contains(randomPos))
		{
			randomPos = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));

		}

		circlePositions.Add(randomPos);


		GameObject circle = Instantiate(circlePrefab, randomPos, Quaternion.identity, transform);
        circle.transform.localScale = new Vector3(CircleSize, CircleSize, CircleSize);

		CircleAnimationEvents animEvents = circle.GetComponent<CircleAnimationEvents>();
		if (animEvents == null)
		{
			animEvents = circle.AddComponent<CircleAnimationEvents>();
		}
		animEvents.InitializeForSequence(this);

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

		else if (circlesClicked >= numberOfCircles)
		{
			CompleteMinigame(100f);
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

	public void CircleClicked(GameObject clickedCircle)
	{
		if (!gameActive || isWaitingForAnimation) return;

		if (clickedCircle == null)
		{
			Debug.LogWarning("Clicked circle is null!");
			return;
		}
		SequenceCircleHandler handler = clickedCircle.GetComponent<SequenceCircleHandler>();

		if (handler != null && handler.isActive && !handler.wasClicked)
		{
			// Correct click
			circlesClicked++;
			handler.wasClicked = true;
			handler.isActive = false;

			Collider2D collider = clickedCircle.GetComponent<Collider2D>();
			if (collider != null)
			{
				collider.enabled = false;
			}

			Animator anim = clickedCircle.GetComponent<Animator>();
			if (anim != null)
			{
				isWaitingForAnimation = true;
				handler.isAnimating = true;
				anim.SetBool("isBroken", true);
			}

			else
			{
				DestroyCircleAndContinue(clickedCircle);
			}
		}
		else
		{
			Debug.Log($"Invalid click on {clickedCircle.name} - isActive: {handler?.isActive}, wasClicked: {handler?.wasClicked}");
		}
	}

	public void OnCircleAnimationComplete(GameObject circle)
	{
		Debug.Log($"Animation complete for circle {circle?.name}");

		if (circle == null)
		{
			Debug.LogWarning("Circle is null in animation complete");
			isWaitingForAnimation = false;

			if (circlesClicked >= numberOfCircles)
			{
				CompleteMinigame(100f);
			}
			else
			{
				Invoke("ActivateRandomCircle", delayBetweenRounds);
			}
			return;
		}

		// Destroy the circle
		if (circles.Contains(circle))
		{
			circles.Remove(circle);
			Destroy(circle);
		}

		isWaitingForAnimation = false;

		if (circlesClicked >= numberOfCircles)
		{
			CompleteMinigame(100f);
		}

		else
		{
			Invoke("ActivateRandomCircle", delayBetweenRounds);
		}
	}

	void DestroyCircleAndContinue(GameObject circle)
	{
		if (circle != null && circles.Contains(circle))
		{
			circles.Remove(circle);
			Destroy(circle);
		}

		isWaitingForAnimation = false;

		if (circlesClicked >= numberOfCircles)
		{
			CompleteMinigame(100f);
		}
		else
		{
			Invoke("ActivateRandomCircle", delayBetweenRounds);
		}
		
	}

	void CompleteMinigame(float score)
	{
		if (!gameActive) return;

		gameActive = false;
		isWaitingForAnimation = false;
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
	public bool isActive = false;
	public bool wasClicked = false;
	public bool isAnimating = false;

	private SpriteRenderer spriteRenderer;
	private Camera mainCamera;

	void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		mainCamera = Camera.main;
	}

	void Update()
	{
		
		if (isActive && !wasClicked && !isAnimating && Mouse.current.leftButton.wasPressedThisFrame)
		{
			Vector2 mousePos = Mouse.current.position.ReadValue();
			Vector2 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));

			Collider2D hit = Physics2D.OverlapPoint(worldPoint);

			if (hit != null && hit.gameObject == gameObject)
			{
				Debug.Log($"Sequence circle {gameObject.name} clicked with Input System!");
				if (minigame != null)
					minigame.CircleClicked(gameObject);
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