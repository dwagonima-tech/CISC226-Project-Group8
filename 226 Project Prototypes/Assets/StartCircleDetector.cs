using UnityEngine;
using UnityEngine.InputSystem;

public class StartCircleDetector : MonoBehaviour
{
	public TrackManager trackManager;
	private Camera mainCamera;

	void Start()
	{
		mainCamera = Camera.main;

		if (trackManager == null)
			trackManager = FindFirstObjectByType<TrackManager>();
	}

	void Update()
	{
		if (Mouse.current.leftButton.wasPressedThisFrame)
		{
			CheckClick();
		}
	}

	void CheckClick()
	{
		Vector2 mousePosition = Mouse.current.position.ReadValue();
		Vector2 worldPoint = mainCamera.ScreenToWorldPoint(mousePosition);

		Collider2D hit = Physics2D.OverlapPoint(worldPoint);

		if (hit != null && hit.gameObject == gameObject)
		{
			Debug.Log("Start circle clicked!");
			trackManager.StartTracking();
		}
	}
}
