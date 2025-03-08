using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum State
{
    Move,
    Idle
}
public class ZombieManager : Singleton<ZombieManager>
{

    public GameObject zombiePrefab; // ���� ������
    public Transform spawnPoint;    // ���� ���� ��ġ
    public List<ZombieController> zombies = new List<ZombieController>(); // ���� ����Ʈ

    public ZombieController SpawnZombie(Vector2 position)    // ���� ����
    {
        GameObject newZombie = Instantiate(zombiePrefab, position, Quaternion.identity);
        ZombieController zombieController = newZombie.GetComponent<ZombieController>();

        if (zombieController != null)
        {
            zombies.Add(zombieController);
        }

        return zombieController;
    }

    public void PauseAllZombies(bool isPaused)    // ��ü ���� �ൿ �Ͻ� ����
    {
        foreach (ZombieController zombie in zombies)
        {
            if (!zombie.isGround || !zombie.isFirstZombieHitWall) continue;

            if (!isPaused)
            {
                zombie.ChangeState(State.Move);
            }
            else
            {
                zombie.ChangeState(State.Idle);
            }

        }
    }

    public ZombieController FindZombieByID(GameObject gameObject) //Ư������ ��ũ��Ʈ ���� ��������
    {
        foreach (ZombieController zombie in zombies)
        {
            if (zombie == null) return null;
            if (zombie.gameObject == gameObject)
            {
                return zombie;
            }
        }
        return null;
    }
    public IEnumerator PushBackZombies()//�ڷ� �б�
    {
        foreach (ZombieController zombie in zombies)
        {
            if (!zombie.isFirstZombieHitWall || !zombie.isGround || zombie.currentState == State.Move) continue;

            zombie.SetEventStatus(true);
            StartCoroutine(zombie.PushBackCoroutine(0.7f, 0.3f));
           
        }
        yield return null;
    }
    public IEnumerator PushDownZombies()//�Ʒ��� �б�
    {
        foreach (ZombieController zombie in zombies)
        {
            if (zombie.currentState == State.Move || zombie.isGround) continue;
          
            zombie.SetEventStatus(true);
            StartCoroutine(zombie.PushDownCoroutine(0.7f, 0.1f));
            

        }
        yield return null;
    }
}
