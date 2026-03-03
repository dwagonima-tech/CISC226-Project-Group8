using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastTester : MonoBehaviour
{
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
			Debug.Log("Mouse clicked at: " + mousePos);

			Vector2 worldPoint = mainCamera.ScreenToWorldPoint(mousePos);
			Debug.Log("World position: " + worldPoint);

			RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
			if (hit.collider != null)
			{
				Debug.Log("HIT: " + hit.collider.gameObject.name);
			}
			else
			{
				Debug.Log("No hit - clicked empty space");
			}
		}
	}
}