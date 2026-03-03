using UnityEngine;
using UnityEngine.InputSystem;


public class SequenceCircle : MonoBehaviour
{
	private SequenceSpawner spawner;
	private Camera mainCamera;
	private SpriteRenderer spriteRenderer;

	public Color defaultColor = Color.gray;
	public Color activeColor = Color.green;
	public bool isActive = false; // Can this circle currently clickable?
	public bool wasClicked = false; // Has this circle been clicked?

	void Start()
    {
		spawner = FindFirstObjectByType<SequenceSpawner>();
		mainCamera = Camera.main;
		spriteRenderer = GetComponent<SpriteRenderer>();

		SetColor(defaultColor);

	}

    // Update is called once per frame
    void Update()
    {
		if (Mouse.current.leftButton.wasPressedThisFrame && isActive && !wasClicked)
		{
			Vector2 mousePosition = Mouse.current.position.ReadValue();
			Vector2 worldPoint = mainCamera.ScreenToWorldPoint(mousePosition);

			Collider2D hit = Physics2D.OverlapPoint(worldPoint);

			if (hit != null && hit.gameObject == gameObject)
			{
				Debug.Log("Correct circle clicked!");

				// Mark as clicked and destroy
				wasClicked = true;

				// Tell spawner to activate next random circle
				if (spawner != null)
				{
					spawner.CircleClicked(gameObject);
				}

				// Destroy this circle
				Destroy(gameObject);
			}
		}
	}
	public void SetActive(bool active)
	{
		isActive = active;
		wasClicked = false; // Reset clicked state when activated
		SetColor(active ? activeColor : defaultColor);
	}

	void SetColor(Color color)
	{
		if (spriteRenderer != null)
		{
			spriteRenderer.color = color;
		}
	}
}
