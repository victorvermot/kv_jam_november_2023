using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[Serializable]
public class KeyValuePair {
    public int key;
    public Animator val;
}

public class PlayerActions : MonoBehaviour
{
    public int beeCounter = 0; 
    public int ressourceCounter = 0;
    private GameObject currentFlower;

    [SerializeField] private int maxBee = 50;
    [SerializeField] private GameObject beeObject;
    [SerializeField] private List <GameObject> listOfBeeObject;
    [SerializeField] private GameObject flowerDObject;
    [SerializeField] private GameObject flowerTObject;
    [SerializeField] private HoneySpawnMonitor spawnMonitor;
    public List<KeyValuePair> MyList = new List<KeyValuePair>();
    private Dictionary<int, Animator> Animations = new Dictionary<int, Animator>();

    private BoxCollider2D beeSpawnArena;
    private HoneyState currentFlowerState;
    private int countCollider;
    private bool _canPlantFlowers = true;
    private bool _canInteractWithFlowers = true;
    private bool _canInteractWithEnd = true;
    private CounterHandler _counterHandler;

    private void Awake()
    {
        foreach (var kvp in MyList) {
            Animations[kvp.key] = kvp.val;
        }
        listOfBeeObject = new List<GameObject>(maxBee);
        _counterHandler = FindObjectOfType<CounterHandler>();
    }

    public int getBeeCounter()
    {
        return beeCounter;
    }

    private void Start()
    {
        _counterHandler.updateHoneyCounter(ressourceCounter);
        _counterHandler.updateBeeCounter(beeCounter);
        beeSpawnArena = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log(other.tag);
        if (other.CompareTag("Flower"))
        {
            _canInteractWithFlowers = false;
            currentFlower = null;
            other.gameObject.GetComponent<FlowerGAction>().HideBeeUI();
        }
        else if (other.CompareTag("FlowerSpawn"))
        {
            _canPlantFlowers = true;
        }
        else if (other.CompareTag("End"))
        {
            _canInteractWithEnd = false;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Honey"))
        {
            _setRessources(ressourceCounter + 5);
            _counterHandler.updateHoneyCounter(ressourceCounter);
            Destroy(col.gameObject);
        }
        else if (col.CompareTag("Flower"))
        {
            _canInteractWithFlowers = true;
            currentFlower = col.gameObject;
            col.gameObject.GetComponent<FlowerGAction>().ShowBeeUI();
        }
        else if (col.CompareTag("FlowerSpawn"))
        {
            _canPlantFlowers = false;
        }
        else if (col.CompareTag("End"))
        {
            _canInteractWithEnd = true;
        }
    }



    public Vector2 GetRandomPointInCollider(BoxCollider2D boxCollider)
    {
        Vector2 center = boxCollider.bounds.center;
        Vector2 size = boxCollider.bounds.size;

        float randomX = Random.Range(center.x - size.x / 2, center.x + size.x / 2);
        float randomY = Random.Range(center.y - size.y / 2, center.y + size.y / 2);

        return new Vector2(randomX, randomY);
    }

    private void _setRessources(int newValue)
    {
        foreach (var x in Animations)
        {
            if (newValue >= x.Key && ressourceCounter < x.Key)
            {
                x.Value.SetBool("isTrigger", true);
            }
            else if (newValue < x.Key && ressourceCounter >= x.Key)
            {
                x.Value.SetBool("isTrigger", false);
            }
        }
        ressourceCounter = newValue;
    }

    private void SpawnFlower()
    {
        if (!(ressourceCounter >= 20) || !_canPlantFlowers) return;
        if (Input.GetKeyDown("1"))
        {
            Instantiate(flowerDObject, GetRandomPointInCollider(beeSpawnArena), Quaternion.identity);
            _setRessources(ressourceCounter - 20);
            _counterHandler.updateHoneyCounter(ressourceCounter);
        }
        else if (Input.GetKeyDown("2"))
        {
            Instantiate(flowerTObject, GetRandomPointInCollider(beeSpawnArena), Quaternion.identity);
            _setRessources(ressourceCounter - 20);
            _counterHandler.updateHoneyCounter(ressourceCounter);
        }
    }

    private void SpawnBee()
    {
        if (!Input.GetKeyDown(KeyCode.E) || !(ressourceCounter >= 5) || beeCounter >= maxBee) return;
        _setRessources(ressourceCounter - 5);
        beeCounter += 1;
        _counterHandler.updateBeeCounter(beeCounter);
        _counterHandler.updateHoneyCounter(ressourceCounter);
        GameObject res = Instantiate(beeObject, GetRandomPointInCollider(beeSpawnArena), Quaternion.identity, transform);
        listOfBeeObject.Add(res);
    }

    public void removeBees()
    {
        int max = 5 > listOfBeeObject.Count ? 5 : listOfBeeObject.Count;
        for (int i = 0; i < max; i++)
        {
            listOfBeeObject.RemoveAt(0);
        }
    }
    
    private void FeedFlowers()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _canInteractWithFlowers && beeCounter > 0)
        {
            HoneyState res = currentFlower.GetComponent<FlowerGAction>().UpdateBeeNumber();
            if (res == HoneyState.Added)
            {
                beeCounter -= 1;
                _counterHandler.updateBeeCounter(beeCounter);
                Destroy(listOfBeeObject[0]);
                listOfBeeObject.RemoveAt(0);
            }
        }
    }

    private void HarvestFlowers()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _canInteractWithFlowers && currentFlower.GetComponent<FlowerGAction>().isReadyToHarvest)
        {
            HoneyState res = currentFlower.GetComponent<FlowerGAction>().UpdateBeeNumber();
            if (res == HoneyState.Ready)
            {
                _setRessources(ressourceCounter + 20);
                _counterHandler.updateHoneyCounter(ressourceCounter);
            }
        }
    }

    private void CompleteGame()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _canInteractWithEnd && ressourceCounter >= 200)
        {
            Debug.Log("Gg you win");
            SceneManager.LoadScene("0");
        }
    }

    // Update is called once per frame
    void Update()
    {
        SpawnBee();
        SpawnFlower();
        FeedFlowers();
        HarvestFlowers();
        CompleteGame();
    }
}
