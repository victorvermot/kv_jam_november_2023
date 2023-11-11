using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LevelGeneration : MonoBehaviour
{
    public GameObject[] objects;

    private void Start()
    {
        StartCoroutine(SpawnHoney());
        // int rand = Random.Range(0, objects.Length); // rand in asset array
        // Instantiate(objects[rand], transform.position, Quaternion.identity); // pops in fixed position

    }

    private IEnumerator SpawnHoney()
    {
        int rand = Random.Range(0, objects.Length); // rand in asset array
        while(true && GameObject.FindGameObjectsWithTag("Honey").Length < GameObject.FindGameObjectsWithTag("Spawn").Length) {
            // must check the number of honey in map before spawning
            // if (isHoneyInMap())
            // {
                // Debug.Log("Spawn :" + GameObject.FindGameObjectsWithTag("Spawn").Length); // logs n spawn objects
                // Debug.Log("Honey :" + GameObject.FindGameObjectsWithTag("Honey").Length); // logs n honey objects
                yield return new WaitForSeconds( Random.Range( 1,4 ) ); // waits 1 to 4 sec
                Instantiate(objects[rand], transform.position, Quaternion.identity); // pops in fixed position
            // }

        }
    }
    
    private bool isHoneyInMap()
    {
        if (GameObject.FindGameObjectWithTag("Honey"))
        {
            Debug.Log("There is a honey");
            return (true);
        }
        return (false);
    }
}