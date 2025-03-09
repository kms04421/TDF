using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieData
{
    
    public float health;// 좀비 체력
    public float maxHealth;// 좀비의 최대 체력
    public float speed; //좀비 속도
    public ZombieData(float health, float maxHealth, float speed)
    {
        this.health = health;
        this.maxHealth = maxHealth;
        this.speed = speed;
    }
}
