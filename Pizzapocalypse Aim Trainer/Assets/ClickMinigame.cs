using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ClickMinigame : MonoBehaviour
{
	public GameObject circlePrefab;
	public int totalCirclesToSpawn = 10;
	public float timeLimit = 30f;
	public float CircleSize = 1f;

	private GameManager gameManager;
	private int circlesClicked = 0;
	private float startTime;
	private bool isActive = false;
	private GameObject currentCircle;
	private bool isWaitingForAnimation = false;

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

		Vector2 randomPos = new Vector2(Random.Range(spawnMinX, spawnMaxX), Random.Range(spawnMinY, spawnMaxY));

		currentCircle = Instantiate(circlePrefab, randomPos, Quaternion.identity, transform);
		currentCircle.transform.localScale = new Vector3(CircleSize, CircleSize, CircleSize);

		CircleAnimationEvents animEvents = currentCircle.GetComponent<CircleAnimationEvents>();
		if (animEvents == null)
		{
			animEvents = currentCircle.AddComponent<CircleAnimationEvents>();
		}
		animEvents.Initialize(this);



		// ABSOLUTELY CRITICAL: Ensure circle has a collider
		CircleCollider2D collider = currentCircle.GetComponent<CircleCollider2D>();
		if (collider == null)
			currentCircle.AddComponent<CircleCollider2D>();

		// Make sure it's visible
		SpriteRenderer renderer = currentCircle.GetComponent<SpriteRenderer>();
		if (renderer != null)
			renderer.color = Color.green;

		// Add click handler
		ClickCircleHandler handler = currentCircle.AddComponent<ClickCircleHandler>();
		handler.minigame = this;

		isWaitingForAnimation = false;
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
		if (!isActive || isWaitingForAnimation || currentCircle == null) return;

		circlesClicked++;
		Debug.Log($"Circle clicked: {circlesClicked}/{totalCirclesToSpawn}");

		// Prevent multiple clicks
		Collider2D collider = currentCircle.GetComponent<Collider2D>();
		if( collider != null)
		{
			collider.enabled = false;
			Debug.Log("Collider disabled");
		}

		// Trigger animation
		Animator anim = currentCircle.GetComponent<Animator>();
		if (anim != null)
		{

			Debug.Log($"Animator found. Controller: {anim.runtimeAnimatorController?.name}");
			AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo(0);

			isWaitingForAnimation = true;
			anim.SetBool("isBroken", true);

		}

		else
		{
			Debug.Log("No Animator found on circle!");
			DestroyCurrentCircleAndContinue();
		}

	}


	public void OnCircleAnimationComplete(GameObject circle)
	{
		 Debug.Log($"OnCircleAnimationComplete called for circle: {circle.name}");
        
        // Make sure we're not processing the same circle twice
        if (circle != currentCircle)
        {
            Debug.Log($"Circle {circle.name} is not currentCircle, destroying it");
            Destroy(circle);
            return;
        }
        
        if (!isActive)
        {
            Debug.Log("Minigame not active, destroying circle");
            Destroy(circle);
            currentCircle = null;
            return;
        }
        
        // Now destroy the circle and continue
        Debug.Log("Animation complete, destroying circle and continuing");
        DestroyCurrentCircleAndContinue();
	}

	void DestroyCurrentCircleAndContinue()
	{
		if (currentCircle != null)
		{
			Destroy(currentCircle);
			currentCircle = null;
		}

		isWaitingForAnimation = false;

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
		isWaitingForAnimation = false;

		float multiplier = 1f + (score / 100f) * 3f;

		if (currentCircle != null)
		{
			Destroy(currentCircle);
			currentCircle = null;

		}

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
			}
		}
	}
}