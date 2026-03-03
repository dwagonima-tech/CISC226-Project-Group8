using UnityEngine;
using UnityEngine.InputSystem;


public class StartCircleTrigger : MonoBehaviour
{
	private PathSpawner spawner;
	private PathCircle pathCircle;
	private Camera mainCamera;
	private bool isMouseDown = false;

	void Start()
	{
		spawner = FindFirstObjectByType<PathSpawner>();
		pathCircle = GetComponent<PathCircle>();
		mainCamera = Camera.main;
	}

	void Update()
	{
		//Track mouse button state
		if (Mouse.current.leftButton.wasPressedThisFrame)
		{
			isMouseDown = true;
			CheckStartClick();
		}

		if (Mouse.current.leftButton.wasReleasedThisFrame)
		{
			isMouseDown = false;
		}

		//Check for drag onto start circle
		if (isMouseDown && !spawner.isTracking)
		{
			CheckMousePosition();
		}
	}

	void CheckStartClick()
	{
		if (pathCircle != null && pathCircle.isStart && !spawner.isTracking)
		{
			Vector2 mousePosition = Mouse.current.position.ReadValue();
			Vector2 worldPoint = mainCamera.ScreenToWorldPoint(mousePosition);

			Collider2D hit = Physics2D.OverlapPoint(worldPoint);

			if (hit != null && hit.gameObject == gameObject)
			{
				Debug.Log("Start circle clicked with Input System!");
				spawner.StartTracking(pathCircle);
			}
		}
	}

	void CheckMousePosition()
	{
		Vector2 mousePosition = Mouse.current.position.ReadValue();
		Vector2 worldPoint = mainCamera.ScreenToWorldPoint(mousePosition);

		Collider2D hit = Physics2D.OverlapPoint(worldPoint);

		if (hit != null && hit.gameObject == gameObject && pathCircle.isStart)
		{
			Debug.Log("Dragged onto start circle!");
			spawner.StartTracking(pathCircle);
		}
	}
}
