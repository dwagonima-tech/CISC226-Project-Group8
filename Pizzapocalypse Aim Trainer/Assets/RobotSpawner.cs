using UnityEngine;

public class RobotSpawner : MonoBehaviour
{
	public GameObject jerryPrefab;
	public GameObject paulPrefab;
	public GameObject haroldPrefab;

	public Transform spawnPoint; // Where the robot should appear

	void Awake()
	{
		SpawnRobot();
	}

	void SpawnRobot()
	{
		// Get the selected robot from PlayerPrefs
		string selectedRobot = PlayerPrefs.GetString("selectedBot", "Jerry"); // Default to Jerry

		// Spawn the appropriate robot
		GameObject robotToSpawn = null;

		switch (selectedRobot)
		{
			case "Jerry":
				robotToSpawn = jerryPrefab;
				break;
			case "Paul":
				robotToSpawn = paulPrefab;
				break;
			case "Harold":
				robotToSpawn = haroldPrefab;
				break;
		}

		if (robotToSpawn != null)
		{
			GameObject spawnedRobot;
			if (spawnPoint != null)
			{
				spawnedRobot = Instantiate(robotToSpawn, spawnPoint.position, spawnPoint.rotation);
			}
			else
			{
				spawnedRobot = Instantiate(robotToSpawn);
			}

			spawnedRobot.tag = "Player";

			GameManager gameManager = FindObjectOfType<GameManager>();
			if (gameManager != null) 
			{
				gameManager.OnRobotSpawned(spawnedRobot);
			}
			
		}
		
	}
}