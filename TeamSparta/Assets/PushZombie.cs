using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushZombie : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Zombie"))
        {
            ZombieController zombieController = collision.transform.GetComponent<ZombieController>();
            StartCoroutine(DelayedCoroutine (zombieController));
        }
        
    }
    IEnumerator DelayedCoroutine (ZombieController zombieController)
    {
        yield return new WaitForSeconds(0.3f);
        zombieController.UnderZombiePush();
    }
  
}
