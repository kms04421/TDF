using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieData
{
    
    public float health;// ���� ü��
    public float maxHealth;// ������ �ִ� ü��
    public float speed; //���� �ӵ�
    public ZombieData(float health, float maxHealth, float speed)
    {
        this.health = health;
        this.maxHealth = maxHealth;
        this.speed = speed;
    }
}
