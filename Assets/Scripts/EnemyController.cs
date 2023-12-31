using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour
{
    private GameObject _target;
    [SerializeField] private float speed = 1f;
    [SerializeField] private GameObject destroyParticles;
    private bool isQuitting;
    
    void Start()
    {
        speed += Random.Range(0, 2f);
        enemyManager.Register(gameObject.GetInstanceID(), this);
        findTarget();
    }
    
    public void findTarget()
    {
        if (!_target)
        {
            _target = FlowerManager.Instance.getRandomAlly();
        }
    }

    private void Update()
    {
        if (_target)
        {
            Transform target = _target.transform;
            transform.position =
                Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
        else
        {
            findTarget();
        }
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
    }


    private void OnDestroy()
    {
        if (!isQuitting && Time.timeScale != 0f)
        {
            Instantiate(destroyParticles, transform.position, Quaternion.identity);
        }
    }
}
