using UnityEngine;
using UnityEngine.InputSystem;

public class ClickableCircle : MonoBehaviour
{
	private CircleSpawner spawner;
	private Camera mainCamera;

	void Start()
	{
		spawner = FindFirstObjectByType<CircleSpawner>();
		mainCamera = Camera.main;
	}

	void Update()
	{
		if (Mouse.current.leftButton.wasPressedThisFrame)
		{
			Vector2 mousePosition = Mouse.current.position.ReadValue();

			Vector2 worldPoint = mainCamera.ScreenToWorldPoint(mousePosition);

			Collider2D hit = Physics2D.OverlapPoint(worldPoint);

			if (hit != null && hit.gameObject == gameObject)
			{
				Debug.Log("Circle clicked with Input System!");
				Destroy(gameObject);

				if (spawner != null)
				{
					spawner.SpawnNewCircle();
				}
			}
		}
	}
}
