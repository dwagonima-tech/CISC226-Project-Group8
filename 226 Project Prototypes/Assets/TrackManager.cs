using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour
{
	public GameObject startCirclePrefab;
	public GameObject endCirclePrefab;
	public GameObject trackingCirclePrefab;
	public GameObject pathSegmentPrefab; // Optional: visual path

	[Header("Path Settings")]
	public float pathLength = 8f;
	public float curveHeight = 3f;
	public int curveSegments = 4;
	public bool showPath = true; // Toggle to show/hide the path line

	[Header("Tracking Settings")]
	public float trackingSpeed = 2f; // Speed of the tracking circle
	public float circleScale = 0.8f;
	public float successRadius = 1.2f; // How far mouse can be from circle

	[Header("Spawn Area")]
	public float minX = -6f;
	public float maxX = 6f;
	public float minY = -3f;
	public float maxY = 3f;

	private GameObject startCircle;
	private GameObject endCircle;
	private GameObject trackingCircle;
	private List<Vector2> pathPoints = new List<Vector2>();
	private LineRenderer pathLine;

	private bool isTracking = false;
	private bool gameComplete = false;
	private float pathProgress = 0f;
	private Camera mainCamera;

	void Start()
	{
		mainCamera = Camera.main;
		GenerateNewPath();
	}

	void Update()
	{
		if (isTracking && !gameComplete)
		{
			// Move the tracking circle along the path
			pathProgress += Time.deltaTime * trackingSpeed / pathLength;

			if (pathProgress >= 1f)
			{
				// Reached the end
				CheckTrackingSuccess();
			}
			else
			{
				// Update tracking circle position
				Vector2 targetPos = GetPathPoint(pathProgress);
				trackingCircle.transform.position = targetPos;

				// Check if mouse is inside the tracking circle
				CheckMousePosition();
			}
		}

		// Press R to restart
		if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
		{
			ResetGame();
		}
	}

	void GenerateNewPath()
	{
		// Clear existing objects
		ClearPath();

		// Generate random start and end points
		Vector2 startPoint = new Vector2(
			Random.Range(minX + 2f, maxX - 2f),
			Random.Range(minY + 2f, maxY - 2f)
		);

		Vector2 endPoint = new Vector2(
			Random.Range(minX + 2f, maxX - 2f),
			Random.Range(minY + 2f, maxY - 2f)
		);

		// Generate path points
		pathPoints = GeneratePathPoints(startPoint, endPoint);

		// Create start circle (green)
		if (startCirclePrefab != null)
		{
			startCircle = Instantiate(startCirclePrefab, startPoint, Quaternion.identity);
			startCircle.transform.localScale = new Vector3(circleScale, circleScale, circleScale);

			// Add detector script
			StartCircleDetector detector = startCircle.AddComponent<StartCircleDetector>();
			detector.trackManager = this;
		}

		// Create end circle (red)
		if (endCirclePrefab != null)
		{
			endCircle = Instantiate(endCirclePrefab, endPoint, Quaternion.identity);
			endCircle.transform.localScale = new Vector3(circleScale, circleScale, circleScale);
		}

		// Create visual path if enabled
		if (showPath)
		{
			CreateVisualPath();
		}

		// Reset state
		isTracking = false;
		gameComplete = false;
		pathProgress = 0f;

		Debug.Log("New path generated. Click the green circle to start!");
	}

	List<Vector2> GeneratePathPoints(Vector2 start, Vector2 end)
	{
		List<Vector2> points = new List<Vector2>();

		// Generate control points for bezier curve
		List<Vector2> controlPoints = new List<Vector2>();
		controlPoints.Add(start);

		Vector2 direction = (end - start).normalized;
		Vector2 perpendicular = new Vector2(-direction.y, direction.x);

		// Create control points for curves
		int numControlPoints = curveSegments;
		for (int i = 1; i <= numControlPoints; i++)
		{
			float t = (float)i / (numControlPoints + 1);
			Vector2 basePoint = Vector2.Lerp(start, end, t);

			// Add perpendicular offset for curves
			float offsetAmount = curveHeight * Mathf.Sin(t * Mathf.PI);
			float sign = (i % 2 == 0) ? 1f : -1f;

			Vector2 controlPoint = basePoint + perpendicular * offsetAmount * sign;

			// Clamp to bounds
			controlPoint.x = Mathf.Clamp(controlPoint.x, minX + 0.5f, maxX - 0.5f);
			controlPoint.y = Mathf.Clamp(controlPoint.y, minY + 0.5f, maxY - 0.5f);

			controlPoints.Add(controlPoint);
		}

		controlPoints.Add(end);

		// Generate points along the bezier curve
		int resolution = 50; // Number of points to sample
		for (int i = 0; i <= resolution; i++)
		{
			float t = (float)i / resolution;
			points.Add(GetBezierPoint(controlPoints, t));
		}

		return points;
	}

	Vector2 GetBezierPoint(List<Vector2> points, float t)
	{
		if (points.Count == 1)
			return points[0];

		if (points.Count == 2)
			return Vector2.Lerp(points[0], points[1], t);

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

		// Convert progress to index
		float exactIndex = progress * (pathPoints.Count - 1);
		int index1 = Mathf.FloorToInt(exactIndex);
		int index2 = Mathf.Min(index1 + 1, pathPoints.Count - 1);
		float lerpFactor = exactIndex - index1;

		return Vector2.Lerp(pathPoints[index1], pathPoints[index2], lerpFactor);
	}

	void CreateVisualPath()
	{
		// Add LineRenderer component to this object
		pathLine = GetComponent<LineRenderer>();
		if (pathLine == null)
			pathLine = gameObject.AddComponent<LineRenderer>();

		// Configure LineRenderer
		pathLine.positionCount = pathPoints.Count;
		pathLine.startWidth = 0.1f;
		pathLine.endWidth = 0.1f;
		pathLine.material = new Material(Shader.Find("Sprites/Default"));
		pathLine.startColor = Color.gray;
		pathLine.endColor = Color.gray;

		// Set positions
		for (int i = 0; i < pathPoints.Count; i++)
		{
			pathLine.SetPosition(i, pathPoints[i]);
		}
	}

	public void StartTracking()
	{
		if (!isTracking && !gameComplete)
		{
			isTracking = true;

			// Create tracking circle at start position
			if (trackingCirclePrefab != null && startCircle != null)
			{
				trackingCircle = Instantiate(trackingCirclePrefab, startCircle.transform.position, Quaternion.identity);
				trackingCircle.transform.localScale = new Vector3(circleScale * 0.8f, circleScale * 0.8f, circleScale * 0.8f);
			}

			Debug.Log("Tracking started! Keep your mouse inside the moving circle.");
		}
	}

	void CheckMousePosition()
	{
		if (trackingCircle == null || !isTracking) return;

		Vector2 mousePos = Mouse.current.position.ReadValue();
		Vector2 worldMousePos = mainCamera.ScreenToWorldPoint(mousePos);

		float distance = Vector2.Distance(worldMousePos, trackingCircle.transform.position);

		if (distance > successRadius)
		{
			Debug.Log($"Mouse left the circle! Distance: {distance}");
			FailTracking();
		}
	}

	void CheckTrackingSuccess()
	{
		if (!isTracking || gameComplete) return;

		// Final check at the end
		Vector2 mousePos = Mouse.current.position.ReadValue();
		Vector2 worldMousePos = mainCamera.ScreenToWorldPoint(mousePos);
		float distance = Vector2.Distance(worldMousePos, trackingCircle.transform.position);

		if (distance <= successRadius)
		{
			CompleteGame();
		}
		else
		{
			FailTracking();
		}
	}

	void FailTracking()
	{
		Debug.Log("Tracking failed! Resetting...");
		isTracking = false;

		// Destroy tracking circle
		if (trackingCircle != null)
		{
			Destroy(trackingCircle);
			trackingCircle = null;
		}

		// Brief pause then reset
		Invoke("ResetGame", 1f);
	}

	void CompleteGame()
	{
		Debug.Log("🎉 SUCCESS! Great tracking! 🎉");
		gameComplete = true;
		isTracking = false;

		// Destroy tracking circle
		if (trackingCircle != null)
		{
			Destroy(trackingCircle);
			trackingCircle = null;
		}

		// Generate new path after delay
		Invoke("GenerateNewPath", 2f);
	}

	void ResetGame()
	{
		// Cancel any pending invokes
		CancelInvoke();

		// Destroy tracking circle if it exists
		if (trackingCircle != null)
		{
			Destroy(trackingCircle);
			trackingCircle = null;
		}

		// Generate new path
		GenerateNewPath();
	}

	void ClearPath()
	{
		if (startCircle != null)
			Destroy(startCircle);

		if (endCircle != null)
			Destroy(endCircle);

		if (trackingCircle != null)
			Destroy(trackingCircle);

		// Clear line renderer
		if (pathLine != null)
		{
			pathLine.positionCount = 0;
		}
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Vector2 center = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
		Vector2 size = new Vector2(maxX - minX, maxY - minY);
		Gizmos.DrawWireCube(center, size);
	}
}