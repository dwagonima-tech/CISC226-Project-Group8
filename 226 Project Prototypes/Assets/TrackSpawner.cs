using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TrackSpawner : MonoBehaviour
{
	public GameObject circlePrefab;
	public float circleScale = 2f;
	public float spacing = 1.0f; // Distance between circles

	[Header("Path Generation")]
	public float pathLength = 12f; // Total length of the path in world units
	public float curveHeight = 3f; // How much the path curves
	public int curveSegments = 3; // Number of curve segments (more = more squiggly)

	[Header("Spawn Area")]
	public float minX = -8f;
	public float maxX = 8f;
	public float minY = -4.5f;
	public float maxY = 4.5f;

	[Header("Game Settings")]
	public float resetDelay = 1f;

	private List<GameObject> pathCircles = new List<GameObject>();
	private int currentIndex = 0;
	public bool isTracking = false;
	private bool gameComplete = false;
	private Camera mainCamera;
	private Vector2 mousePosition;

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
		if (Mouse.current != null)
		{
			mousePosition = Mouse.current.position.ReadValue();
		}

		// Check for mouse button release
		if (Mouse.current.leftButton.wasReleasedThisFrame)
		{
			if (isTracking && !gameComplete)
			{
				Debug.Log("Mouse released - stopping tracking but keeping path");
				isTracking = false;
			}
		}
	}

	void GenerateNewPath()
	{
		ClearPath();

		// Calculate number of circles based on path length and spacing
		int numberOfCircles = Mathf.Max(8, Mathf.RoundToInt(pathLength / spacing));

		// Generate random start and end points within bounds, ensuring they're spread out
		Vector2 startPoint = new Vector2(
			Random.Range(minX + 3f, minX - 5f), //Start more to the left
			Random.Range(minY + 2f, maxY - 2f)
		);

		Vector2 endPoint = new Vector2(
			Random.Range(maxX - 5f, maxX - 3f), //End more to the right
			Random.Range(minY + 2f, maxY - 2f)
		);

		// Generate control points for bezier curve
		List<Vector2> controlPoints = GenerateControlPoints(startPoint, endPoint);

		// Create circles along the bezier curve
		for (int i = 0; i < numberOfCircles; i++)
		{
			float t = (float)i / (numberOfCircles - 1); // 0 to 1 along the path

			// Get position on bezier curve
			Vector2 position = GetBezierPoint(controlPoints, t);

			// Create circle
			GameObject circle = Instantiate(circlePrefab, position, Quaternion.identity);
			circle.transform.localScale = new Vector3(circleScale, circleScale, circleScale);

			// Add and configure TrackCircle component
			TrackCircle trackCircle = circle.GetComponent<TrackCircle>();
			if (trackCircle == null)
				trackCircle = circle.AddComponent<TrackCircle>();

			trackCircle.orderIndex = i;

			// First circle is active (green) last circle is target (red)
			if (i == 0)
			{
				trackCircle.SetAsStart();
			}
			else if (i == numberOfCircles - 1)
			{
				trackCircle.SetAsEnd();
			}

			pathCircles.Add(circle);
		}

		// Reset game state
		currentIndex = 0;
		isTracking = false;
		gameComplete = false;

		Debug.Log($"Path generated with {numberOfCircles} circles! Click and hold the first green circle.");
	}

	List<Vector2> GenerateControlPoints(Vector2 start, Vector2 end)
	{
		List<Vector2> points = new List<Vector2>();
		points.Add(start);

		// Calculate direction and perpendicular
		Vector2 direction = (end - start).normalized;
		Vector2 perpendicular = new Vector2(-direction.y, direction.x);

		// Generate intermediate control points
		float segmentLength = Vector2.Distance(start, end) / (curveSegments + 1);

		for (int i = 1; i <= curveSegments; i++)
		{
			float t = (float)i / (curveSegments + 1);
			Vector2 basePoint = Vector2.Lerp(start, end, t);

			// Add perpendicular offset with sine wave for smooth curves
			float offsetAmount = curveHeight * Mathf.Sin(t * Mathf.PI);

			// Randomly choose left or right for this segment
			float sign = (i % 2 == 0) ? 1f : -1f;
			Vector2 controlPoint = basePoint + perpendicular * offsetAmount * sign;

			// Ensure point is within bounds
			controlPoint.x = Mathf.Clamp(controlPoint.x, minX + 1f, maxX - 1f);
			controlPoint.y = Mathf.Clamp(controlPoint.y, minY + 1f, maxY - 1f);

			points.Add(controlPoint);
		}

		points.Add(end);
		return points;
	}

	Vector2 GetBezierPoint(List<Vector2> points, float t)
	{
		if (points.Count == 1)
			return points[0];

		if(points.Count == 2)
			return Vector2.Lerp(points[0], points[1], t);

		// Recursive bezier calculation
		List<Vector2> newPoints = new List<Vector2>();
		for (int i = 0; i < points.Count - 1; i++)
		{
			newPoints.Add(Vector2.Lerp(points[i], points[i + 1], t));
		}

		return GetBezierPoint(newPoints, t);
	}

	public void TryStartTracking()
	{
		if (!isTracking && !gameComplete && pathCircles.Count > 0)
		{
			Debug.Log("Starting tracking! Follow the green circle...");
			isTracking = true;

			// Make sure first circle is active
			TrackCircle firstCircle = pathCircles[0].GetComponent<TrackCircle>();
			if (firstCircle != null)
			{
				firstCircle.SetAsStart(); // Ensure it's green
			}
		}
	}

	public void CircleReached(int index)
	{
		if (!isTracking || gameComplete) return;

		Debug.Log($"Circle reached - Index: {index}, Current tracking: {isTracking}");

		// Check if this is the next circle in sequence (should be index 1 since index 0 is start)
		if (index == 1)
		{
			Debug.Log("Moving to next circle!");

			// Destroy the first circle (the one we just left)
			if (pathCircles.Count > 0 && pathCircles[0] != null)
			{
				Destroy(pathCircles[0]);
				pathCircles.RemoveAt(0);
			}

			// The circle we just reached becomes the new start (index 0)
			if (pathCircles.Count > 0 && pathCircles[0] != null)
			{
				TrackCircle newStart = pathCircles[0].GetComponent<TrackCircle>();
				if (newStart != null)
				{
					newStart.SetAsStart();
					Debug.Log($"New start circle set at index 0. Remaining circles: {pathCircles.Count}");
				}
			}

			// Update indices of remaining circles
			for (int i = 0; i < pathCircles.Count; i++)
			{
				TrackCircle circle = pathCircles[i].GetComponent<TrackCircle>();
				if (circle != null)
				{
					circle.orderIndex = i;
				}
			}

			// Check if we've reached the end (only the end circle remains)
			if (pathCircles.Count == 1)
			{
				TrackCircle endCircle = pathCircles[0].GetComponent<TrackCircle>();
				if (endCircle != null && endCircle.isEnd)
				{
					CompletePath();
				}
			}
		}
	}


	void CompletePath()
	{
		Debug.Log("PATH COMPLETE! Great tracking!");
		gameComplete = true;
		isTracking = false;

		// Generate new path after delay
		Invoke("GenerateNewPath", 2f);
	}


	void ResetPath()
	{
		Debug.Log("Resetting path...");

		// Destroy all circles and generate new path
		GenerateNewPath();
	}

	void ClearPath()
	{
		foreach (GameObject circle in pathCircles)
		{
			if (circle != null)
				Destroy(circle);
		}
		pathCircles.Clear();
	}

	// Public method for start circle detection
	public void CheckStartCircle(GameObject circle)
	{
		if (!isTracking && !gameComplete && pathCircles.Count > 0 && circle == pathCircles[0])
		{
			TryStartTracking();
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
