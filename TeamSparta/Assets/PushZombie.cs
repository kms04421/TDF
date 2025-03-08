using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushZombie : MonoBehaviour
{
    private ZombieManager zombieManager;
    void Start()
    {
        zombieManager = ZombieManager.Instance;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Zombie"))
        {
            ZombieController zombieController = zombieManager.FindZombieByID(collision.gameObject);
            zombieController.isEvent = true;
            StartCoroutine(DelayedCoroutine (zombieController));
            zombieManager.PauseAllZombies(true);
        }
        
    }
    IEnumerator DelayedCoroutine (ZombieController zombieController)
    {
        
        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(zombieManager.PushBackZombies());

        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(zombieManager.PushDownZombies());
        zombieController.isEvent = false;
        //zombieController.UnderZombiePush();
    }

}
