using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class FlowerManager : MonoBehaviour
{
    public static FlowerManager Instance;
    Dictionary<int, AllyHP> Allies;
    private void Awake()
    {
        Instance = this;
        Allies = new Dictionary<int, AllyHP>();
    }
    
    public static void Register(int id, AllyHP ally) {
        if (Instance.Allies.ContainsKey(id)) {
            Debug.Log("Duplicate registration attempted... " + id.ToString());
            return;
        }
        Debug.Log("Registered + " + ally);
        Instance.Allies.Add(id, ally);
    }

    public static void Remove(int id)
    {
        Instance.Allies.Remove(id);
    }

    public GameObject getRandomAlly()
    {
        if (Allies.Count > 0)
        {
            System.Random rand = new System.Random();
            GameObject test = Allies.ElementAt(rand.Next(0, Allies.Count)).Value.gameObject;
            return test;
        }

        return null;
    }
}
