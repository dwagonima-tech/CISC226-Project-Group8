using UnityEngine;

public class TrackCircle : MonoBehaviour
{
	private SpriteRenderer spriteRenderer;
	public TrackSpawner spawner;

	public Color defaultColor = Color.white;
	public Color startColor = Color.green;
	public Color endColor = Color.red;

	public int orderIndex = 0;
	public bool isStart = false;
	public bool isEnd = false;

	void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();

		if (spawner == null)
		{
			spawner = FindFirstObjectByType<TrackSpawner>();
		}

		UpdateColor();
	}

	public void SetAsStart()
	{
		isStart = true;
		isEnd = false;
		UpdateColor();
	}

	public void SetAsEnd()
	{
		isEnd = true;
		isStart = false;
		UpdateColor();
	}

	void UpdateColor()
	{
		if (spriteRenderer == null) return;

		if (isStart)
			spriteRenderer.color = startColor;
		else if (isEnd)
			spriteRenderer.color = endColor;
		else
			spriteRenderer.color = defaultColor;
	}

	void OnMouseEnter()
	{
		if (spawner != null && spawner.isTracking)
		{
			Debug.Log($"Mouse entered circle {orderIndex} - isStart: {isStart}, isEnd: {isEnd}");

			// If this is the start circle and we're tracking, do nothing special
			if (isStart)
				return;

			// Tell spawner we reached this circle
			spawner.CircleReached(orderIndex);
		}
	}
}