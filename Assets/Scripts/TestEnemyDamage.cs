using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemyDamage : MonoBehaviour
{

    public int MaxHealth = 10;
    public int health;

    // Start is called before the first frame update
    void Start()
    {
        health = MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damage){
        health -= damage;
    }
}
