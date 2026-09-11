using UnityEngine;

public class OnSpawn : MonoBehaviour
{
    private float xSpawn = 0.2f;

    private float ySpawn = 0.2f;

    private void Start()
    {
        SpawnLogic();   
    }

    private void SpawnLogic()
    {
        float xPos = Random.Range(-xSpawn, xSpawn);
        float yPos = Random.Range(-ySpawn, ySpawn);
        Vector3 spawnOffset = new Vector3(xPos, yPos, transform.position.z);
        transform.position = transform.position + spawnOffset;
    }
}
