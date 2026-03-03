using UnityEngine;

public class PathCircle : MonoBehaviour
{
	private SpriteRenderer spriteRenderer;
	private PathSpawner spawner;


	public Color defaultColor = Color.white;
	public Color startColor = Color.green;
	public Color endColor = Color.red;
	public Color tracedColor = Color.green;
	public Color wrongColor = Color.red; //Flashes when mistake is made

	public bool isStart = false;
	public bool isEnd = false;
	public bool isTraced = false;
	public int orderIndex = 0; // Position in the path

	void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		spawner = FindFirstObjectByType<PathSpawner>();

		UpdateColor();
	}

	public void UpdateColor()
	{
		if (spriteRenderer == null) return;

		if (isStart)
			spriteRenderer.color = startColor;
		else if (isEnd)
			spriteRenderer.color = endColor;
		else if (isTraced)
			spriteRenderer.color = tracedColor;
		else
			spriteRenderer.color = defaultColor;
	}

	public void FlashWrong()
	{
		StartCoroutine(FlashRoutine());
	}

	System.Collections.IEnumerator FlashRoutine()
	{
		spriteRenderer.color = wrongColor;
		yield return new WaitForSeconds(0.2f);
		UpdateColor();
	}

	void OnMouseEnter()
	{
		// Only process if we're in tracking mode and this circle is next
		if (spawner != null && spawner.isTracking)
		{
			spawner.CircleEntered(this);
		}
	}
}