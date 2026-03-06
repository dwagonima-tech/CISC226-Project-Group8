using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ClickMinigame : MonoBehaviour
{
	public GameObject circlePrefab;
	public int totalCirclesToSpawn = 10;
	public float timeLimit = 30f;

	private GameManager gameManager;
	private int circlesClicked = 0;
	private float startTime;
	private bool isActive = false;
	private GameObject currentCircle;

	// Fixed spawn area that works
	public float spawnMinX = -6f;
	public float spawnMaxX = 6f;
	public float spawnMinY = -3f;
	public float spawnMaxY = 3f;

	void Start()
	{
		gameManager = FindFirstObjectByType<GameManager>();
	}

	public void StartMinigame()
	{
		isActive = true;
		circlesClicked = 0;
		startTime = Time.time;

		ClearCurrentCircle();
		SpawnNextCircle();
		StartCoroutine(TimerRoutine());

		Debug.Log("Click Minigame Started");
	}

	void SpawnNextCircle()
	{
		if (!isActive) return;

		Vector2 randomPos = new Vector2(
			Random.Range(spawnMinX, spawnMaxX),
			Random.Range(spawnMinY, spawnMaxY)
		);

		currentCircle = Instantiate(circlePrefab, randomPos, Quaternion.identity, transform);

		// ABSOLUTELY CRITICAL: Ensure circle has a collider
		CircleCollider2D collider = currentCircle.GetComponent<CircleCollider2D>();
		if (collider == null)
			currentCircle.AddComponent<CircleCollider2D>();

		// Make sure it's visible
		SpriteRenderer renderer = currentCircle.GetComponent<SpriteRenderer>();
		if (renderer != null)
			renderer.color = Color.white;

		// Add click handler
		ClickCircleHandler handler = currentCircle.AddComponent<ClickCircleHandler>();
		handler.minigame = this;
	}

	IEnumerator TimerRoutine()
	{
		yield return new WaitForSeconds(timeLimit);

		if (isActive && circlesClicked < totalCirclesToSpawn)
		{
			float score = (circlesClicked / (float)totalCirclesToSpawn) * 100f;
			CompleteMinigame(score);
		}
	}

	public void CircleClicked()
	{
		if (!isActive) return;

		circlesClicked++;
		Debug.Log($"Circle clicked: {circlesClicked}/{totalCirclesToSpawn}");

		ClearCurrentCircle();

		if (circlesClicked >= totalCirclesToSpawn)
		{
			float timeTaken = Time.time - startTime;
			float timeBonus = Mathf.Max(0, (timeLimit - timeTaken) / timeLimit * 50f);
			float score = Mathf.Clamp(50f + timeBonus, 0, 100);
			CompleteMinigame(score);
		}
		else
		{
			SpawnNextCircle();
		}
	}

	void ClearCurrentCircle()
	{
		if (currentCircle != null)
			Destroy(currentCircle);
	}

	void CompleteMinigame(float score)
	{
		if (!isActive) return;

		isActive = false;
		StopAllCoroutines();

		float multiplier = 1f + (score / 100f) * 3f;

		ClearCurrentCircle();

		if (gameManager != null)
			gameManager.MinigameComplete(multiplier);

		gameObject.SetActive(false);
	}
}

public class ClickCircleHandler : MonoBehaviour
{
	public ClickMinigame minigame;
	private Camera mainCamera;

	void Start()
	{
		mainCamera = Camera.main;
	}

	void Update()
	{
		if (Mouse.current.leftButton.wasPressedThisFrame)
		{
			Vector2 mousePos = Mouse.current.position.ReadValue();
			Vector2 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));

			Collider2D hit = Physics2D.OverlapPoint(worldPoint);

			if (hit != null && hit.gameObject == gameObject)
			{
				Debug.Log("Circle clicked with Input System!");
				if (minigame != null)
					minigame.CircleClicked();
				Destroy(gameObject);
			}
		}
	}
}