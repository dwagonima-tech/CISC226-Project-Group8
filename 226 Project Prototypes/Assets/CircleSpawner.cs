using UnityEngine;

public class CircleSpawner : MonoBehaviour
{
	public GameObject circlePrefab;
	public float circleScale = 0.3f;

	public float minX = -8f;
	public float maxX = 8f;
	public float minY = -5f;
	public float maxY = 5f;

	void Start()
	{
		if (circlePrefab == null)
		{
			Debug.LogError("Circle Prefab is not assigned in the inspector!");
			return;
		}

		SpawnNewCircle();
	}

	public void SpawnNewCircle()
	{
		float randomX = Random.Range(minX, maxX);
		float randomY = Random.Range(minY, maxY);
		Vector2 randomPosition = new Vector2(randomX, randomY);


		GameObject newCircle = Instantiate(circlePrefab, randomPosition, Quaternion.identity);
		newCircle.transform.localScale = new Vector3(circleScale, circleScale, circleScale);
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Vector2 center = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
		Vector2 size = new Vector2(maxX - minX, maxY - minY);
		Gizmos.DrawWireCube(center, size);
	}

}
