using UnityEngine;

public class ClickDiagnostic : MonoBehaviour
{
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Debug.Log("🔴 Mouse button DETECTED at: " + Input.mousePosition);

			// Method 1: Screen point to world point
			Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Debug.Log("   World point: " + worldPoint);

			// Method 2: Physics2D raycast
			RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
			if (hit.collider != null)
			{
				Debug.Log("   ✅ HIT: " + hit.collider.gameObject.name + " on layer: " + hit.collider.gameObject.layer);

				// Check if it has a click handler
				if (hit.collider.GetComponent<MonoBehaviour>() != null)
				{
					Debug.Log("   ✅ Has MonoBehaviour components");
				}
			}
			else
			{
				Debug.Log("   ❌ NO HIT - Clicked empty space");

				// Cast a wider ray to see if anything is nearby
				RaycastHit2D[] hits = Physics2D.CircleCastAll(worldPoint, 0.5f, Vector2.zero);
				Debug.Log("   Nearby objects: " + hits.Length);
				foreach (var nearHit in hits)
				{
					Debug.Log("     - " + nearHit.collider.gameObject.name + " at distance: " + Vector2.Distance(worldPoint, nearHit.point));
				}
			}
		}
	}
}