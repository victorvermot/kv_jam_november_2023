using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

[Serializable] public class KeyValuePair {
    public int key;
    public Animator val;
}

public class PlayerActions : MonoBehaviour
{
    public int beeCounter = 0; 
    public int ressourceCounter = 0;
    private GameObject currentFlower;

    [SerializeField] private int maxBee = 50;
    [SerializeField] private int ressourceToEnd = 50;
    [SerializeField] private GameObject beeObject;
    [SerializeField] private List <GameObject> listOfBeeObject;
    [SerializeField] private GameObject flowerDObject;
    [SerializeField] private GameObject flowerTObject;
    [SerializeField] private GameObject playerCollider;
    [SerializeField] private HoneySpawnMonitor spawnMonitor;
    [SerializeField] private EndingManager _endingManager;
    [SerializeField] private enemySpawner enemySpawner;
    [SerializeField] private Animator _endFade;
    [SerializeField] private Animator _playerHitAnim;
    public List<KeyValuePair> MyList = new List<KeyValuePair>();
    private Dictionary<int, Animator> Animations = new Dictionary<int, Animator>();
    private HoneyState _currentFlowerState;

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
        if (other.CompareTag("Flower"))
        {
            _canInteractWithFlowers = false;
            currentFlower.GetComponent<FlowerGAction>().HideBeeUI();
            currentFlower = null;
        }
        else if (other.CompareTag("FlowerSpawn"))
        {
            _canPlantFlowers = true;
        }
        else if (other.CompareTag("End"))
        {
            _canPlantFlowers = true;
            _canInteractWithEnd = false;
            _endingManager.baseTree();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Honey"))
        {
            AudioManager.Instance.playSound("Collectible");
            Debug.Log("HONEYYYY");
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
            _canPlantFlowers = false;
            _canInteractWithEnd = true;
            _endingManager.highlightTree();
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
            if (newValue >= x.Key)
            {
                x.Value.SetBool("isTrigger", true);
            }
            else if (newValue < x.Key)
            {
                x.Value.SetBool("isTrigger", false);
            }
        }
        ressourceCounter = newValue;
    }

    private void SpawnFlower()
    {
        if (!_canPlantFlowers) return;
        if (Input.GetKeyDown("1") && ressourceCounter >= 10)
        {
            AudioManager.Instance.playSound("ButtonClick");
            Instantiate(flowerDObject, GetRandomPointInCollider(beeSpawnArena), Quaternion.identity);
            _setRessources(ressourceCounter - 10);
            _counterHandler.updateHoneyCounter(ressourceCounter);
        }
        else if (Input.GetKeyDown("2") && ressourceCounter >= 20)
        {
            AudioManager.Instance.playSound("ButtonClick");
            Instantiate(flowerTObject, GetRandomPointInCollider(beeSpawnArena), Quaternion.identity);
            _setRessources(ressourceCounter - 20);
            _counterHandler.updateHoneyCounter(ressourceCounter);
        }
    }

    private void SpawnBee()
    {
        if (!Input.GetKeyDown(KeyCode.E) || !(ressourceCounter >= 5) || beeCounter >= maxBee) return;
        AudioManager.Instance.playSound("BeePop");
        _setRessources(ressourceCounter - 5);
        beeCounter += 1;
        _counterHandler.updateBeeCounter(beeCounter);
        _counterHandler.updateHoneyCounter(ressourceCounter);
        GameObject res = Instantiate(beeObject, GetRandomPointInCollider(beeSpawnArena), Quaternion.identity, transform);
        listOfBeeObject.Add(res);
    }

    public void removeBees()
    {
        int max = 5 > listOfBeeObject.Count ? listOfBeeObject.Count : 5;
        for (int i = 0; i < max; i++)
        {
            Destroy(listOfBeeObject[0]);
            listOfBeeObject.RemoveAt(0);
            beeCounter -= 1;
            _counterHandler.updateBeeCounter(beeCounter);
        }
    }
    
    private void FeedFlowers()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _canInteractWithFlowers)
        {
            AudioManager.Instance.playSound("ButtonClick");
            FlowerGAction flowerScript = currentFlower.GetComponent<FlowerGAction>();
            if (beeCounter > 0 && !flowerScript.isReadyToHarvest)
            {
                flowerScript.UpdateBeeNumber();
                beeCounter -= 1;
                _counterHandler.updateBeeCounter(beeCounter);
                Destroy(listOfBeeObject[0]);
                listOfBeeObject.RemoveAt(0);
            }
            else if (flowerScript.isReadyToHarvest)
            {
                flowerScript.UpdateBeeNumber();
                _setRessources(ressourceCounter + 20);
                _counterHandler.updateHoneyCounter(ressourceCounter);
            }
        }
    }
    
    private void CompleteGame()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _canInteractWithEnd && ressourceCounter >= ressourceToEnd)
        {
            StartCoroutine(launchEndGame());
        }
    }
    
    public IEnumerator launchEndGame()
    {
        _endFade.SetTrigger("start");
        // enemySpawner.StopEnemiesSpawn();
        // enemyManager.Instance.KillAllEnemies();
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(2);
    }
    
    // Update is called once per frame
    void Update()
    {
        godMove();
        SpawnBee();
        SpawnFlower();
        FeedFlowers();
        CompleteGame();
    }

    private void godMove()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            ressourceCounter += 50;
            _counterHandler.updateHoneyCounter(ressourceCounter);
        }
    }

    private IEnumerator deactivatePlayerCollider()
    {
        playerCollider.SetActive(false);
        yield return new WaitForSeconds(1f);
        playerCollider.SetActive(true);
    }

    public void playHitAnim()
    {
        _playerHitAnim.SetTrigger("isHit");
        StartCoroutine(deactivatePlayerCollider());
    }
}
