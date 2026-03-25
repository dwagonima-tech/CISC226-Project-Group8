using UnityEngine;
using UnityEngine.InputSystem; 
using System.Collections;
using System.Collections.Generic;

public class TrackingMinigame : MonoBehaviour
{
	public GameObject startCirclePrefab;
	public GameObject endCirclePrefab;
	public GameObject trackingCirclePrefab;


    [Header("Path Settings")]
	public float pathLength = 12f;
	public float curveHeight = 4f;
	public int curveSegments = 3;

	[Header("Tracking Settings")]
	public float trackingSpeed = 2f;
	public float circleScale = 1f;
	public float successRadius = 1.5f;
	public float startLeewayTime = 5f;

	[Header("Spawn Area")]
	public float minX = -5f;
	public float maxX = 5f;
	public float minY = -3f;
	public float maxY = 3f;

	private GameManager gameManager;
	private GameObject startCircle;
	private GameObject endCircle;
	private GameObject trackingCircle;
	private List<Vector2> pathPoints = new List<Vector2>();

	private bool isActive = false;
	private bool isTracking = false;
	private float pathProgress = 0f;
	private Camera mainCamera;
	private float startTime;
	private bool mouseOnStart = false;
	private Mouse mouse; // Reference to mouse input

	void Start()
	{
		gameManager = FindFirstObjectByType<GameManager>();
		mainCamera = Camera.main;
		mouse = Mouse.current; // Get mouse reference
	}

	public void StartMinigame()
	{
		CleanupObjects();

		isActive = true;
		isTracking = false;
		mouseOnStart = false;
		startTime = Time.time;

		GeneratePath();

		StartCoroutine(LeewayTimer());

		Debug.Log($"Tracking Minigame Started - Place mouse on GREEN start circle within {startLeewayTime} seconds");
	}

	IEnumerator LeewayTimer()
	{
		float timer = 0f;

		while (timer < startLeewayTime && !mouseOnStart)
		{
			CheckMouseOnStart();
			timer += Time.deltaTime;
			yield return null;
		}

		if (!mouseOnStart)
		{
			Debug.Log("Failed to place mouse on start circle in time");
			FailMinigame();
		}
	}

	void CheckMouseOnStart()
	{
		if (!isActive || isTracking || startCircle == null || mouse == null) return;

		// FIXED: Using Input System
		Vector2 mousePos = mouse.position.ReadValue();
		Vector2 worldMousePos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));

		Collider2D hit = Physics2D.OverlapPoint(worldMousePos);

		if (hit != null && hit.gameObject == startCircle)
		{
			Debug.Log("Mouse on start circle! Beginning tracking...");
			mouseOnStart = true;
			StartTracking();
		}
	}

	void StartTracking()
	{
		isTracking = true;

		if (trackingCirclePrefab != null && startCircle != null)
		{
			trackingCircle = Instantiate(trackingCirclePrefab, startCircle.transform.position, Quaternion.identity, transform);
			trackingCircle.transform.localScale = new Vector3(circleScale * 0.8f, circleScale * 0.8f, circleScale * 0.8f);
		}

		SpriteRenderer startRenderer = startCircle.GetComponent<SpriteRenderer>();
		if (startRenderer != null)
			startRenderer.color = Color.cyan;
	}

	void Update()
	{
		if (!isActive) return;

		if (!isTracking && !mouseOnStart)
		{
			CheckMouseOnStart();
		}

		if (isTracking)
		{
			pathProgress += Time.deltaTime * trackingSpeed / pathLength;

			if (pathProgress >= 1f)
			{
				CheckCompletion();
			}
			else
			{
				Vector2 targetPos = GetPathPoint(pathProgress);
				if (trackingCircle != null)
					trackingCircle.transform.position = targetPos;

				CheckMousePosition();
			}
		}
	}

	void GeneratePath()
	{
		CleanupObjects();

		if (startCircle != null) Destroy(startCircle);
		if (endCircle != null) Destroy(endCircle);
		if (trackingCircle != null) Destroy(trackingCircle);

		Vector2 startPoint = new Vector2(
			Random.Range(minX + 2f, maxX - 2f),
			Random.Range(minY + 2f, maxY - 2f)
		);

		Vector2 endPoint = new Vector2(
			Random.Range(minX + 2f, maxX - 2f),
			Random.Range(minY + 2f, maxY - 2f)
		);

		pathPoints = GeneratePathPoints(startPoint, endPoint);

		startCircle = Instantiate(startCirclePrefab, startPoint, Quaternion.identity, transform);
		startCircle.transform.localScale = new Vector3(circleScale, circleScale, circleScale);

		endCircle = Instantiate(endCirclePrefab, endPoint, Quaternion.identity, transform);
		endCircle.transform.localScale = new Vector3(circleScale, circleScale, circleScale);

		if (startCircle.GetComponent<Collider2D>() == null)
			startCircle.AddComponent<CircleCollider2D>();
	}

	List<Vector2> GeneratePathPoints(Vector2 start, Vector2 end)
	{
		List<Vector2> points = new List<Vector2>();
		List<Vector2> controlPoints = new List<Vector2> { start };

		Vector2 direction = (end - start).normalized;
		Vector2 perpendicular = new Vector2(-direction.y, direction.x);

		for (int i = 1; i <= curveSegments; i++)
		{
			float t = (float)i / (curveSegments + 1);
			Vector2 basePoint = Vector2.Lerp(start, end, t);
			float offset = curveHeight * Mathf.Sin(t * Mathf.PI);
			float sign = (i % 2 == 0) ? 1f : -1f;

			Vector2 controlPoint = basePoint + perpendicular * offset * sign;
			controlPoints.Add(controlPoint);
		}

		controlPoints.Add(end);

		int resolution = 50;
		for (int i = 0; i <= resolution; i++)
		{
			float t = (float)i / resolution;
			points.Add(GetBezierPoint(controlPoints, t));
		}

		return points;
	}

	Vector2 GetBezierPoint(List<Vector2> points, float t)
	{
		if (points.Count == 1) return points[0];
		if (points.Count == 2) return Vector2.Lerp(points[0], points[1], t);

		List<Vector2> newPoints = new List<Vector2>();
		for (int i = 0; i < points.Count - 1; i++)
		{
			newPoints.Add(Vector2.Lerp(points[i], points[i + 1], t));
		}
		return GetBezierPoint(newPoints, t);
	}

	Vector2 GetPathPoint(float progress)
	{
		if (pathPoints.Count == 0) return Vector2.zero;
		float exactIndex = progress * (pathPoints.Count - 1);
		int index1 = Mathf.FloorToInt(exactIndex);
		int index2 = Mathf.Min(index1 + 1, pathPoints.Count - 1);
		float lerp = exactIndex - index1;
		return Vector2.Lerp(pathPoints[index1], pathPoints[index2], lerp);
	}

	void CheckMousePosition()
	{
		if (!isTracking || trackingCircle == null || mouse == null) return;

		// FIXED: Using Input System
		Vector2 mousePos = mouse.position.ReadValue();
		Vector2 worldMousePos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
		float distance = Vector2.Distance(worldMousePos, trackingCircle.transform.position);

		if (distance > successRadius)
		{
			Debug.Log("Mouse left the circle!");
			FailMinigame();
		}
	}

	void CheckCompletion()
	{
		if (!isTracking || mouse == null) return;

		// FIXED: Using Input System
		Vector2 mousePos = mouse.position.ReadValue();
		Vector2 worldMousePos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
		float distance = Vector2.Distance(worldMousePos, trackingCircle.transform.position);

		if (distance <= successRadius)
		{
			float timeTaken = Time.time - startTime - startLeewayTime;
			float timeBonus = Mathf.Max(0, 30f - timeTaken);
			float accuracyBonus = 100f - (distance / successRadius) * 30f;
			float score = Mathf.Clamp(accuracyBonus + timeBonus, 0, 100);

			CompleteMinigame(score);
		}
		else
		{
			FailMinigame();
		}
	}

	void CompleteMinigame(float score)
	{
		if (!isActive) return;
		isActive = false;
		isTracking = false;
		mouseOnStart = false;

		float multiplier = 1f + (score / 100f) * 3f;

		Debug.Log($"Tracking Complete! Score: {score}, Multiplier: {multiplier}");

		if (gameManager != null)
			gameManager.MinigameComplete(multiplier);

		// Clean up all objects
		CleanupObjects();

		gameObject.SetActive(false);
	}

	void FailMinigame()
	{
		if (!isActive) return;
		isActive = false;
		isTracking = false;
		mouseOnStart = false;

		Debug.Log("Tracking Failed - Minimum multiplier");

		if (gameManager != null)
			gameManager.MinigameComplete(1f);

		// Clean up all objects
		CleanupObjects();

		gameObject.SetActive(false);
	}

	void CleanupObjects()
	{
		// Stop all coroutines
		StopAllCoroutines();

		// Destroy all circles
		if (startCircle != null)
		{
			Destroy(startCircle);
			startCircle = null;
		}

		if (endCircle != null)
		{
			Destroy(endCircle);
			endCircle = null;
		}

		if (trackingCircle != null)
		{
			Destroy(trackingCircle);
			trackingCircle = null;
		}

		// Clear path points
		pathPoints.Clear();

		// Reset variables
		pathProgress = 0f;
	}
}