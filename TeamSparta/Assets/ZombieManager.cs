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

    public GameObject zombiePrefab; // 좀비 프리팹
    public Transform spawnPoint;    // 좀비 스폰 위치
    public List<ZombieController> zombies = new List<ZombieController>(); // 좀비 리스트

    public ZombieController SpawnZombie(Vector2 position)    // 좀비 생성
    {
        GameObject newZombie = Instantiate(zombiePrefab, position, Quaternion.identity);
        ZombieController zombieController = newZombie.GetComponent<ZombieController>();

        if (zombieController != null)
        {
            zombies.Add(zombieController);
        }

        return zombieController;
    }

    public void PauseAllZombies(bool isPaused)    // 전체 좀비 행동 일시 정지
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

    public ZombieController FindZombieByID(GameObject gameObject) //특정좀비 스크립트 정보 가져오기
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
    public IEnumerator PushBackZombies()//뒤로 밀기
    {
        foreach (ZombieController zombie in zombies)
        {
            if (!zombie.isFirstZombieHitWall || !zombie.isGround || zombie.currentState == State.Move) continue;

            zombie.SetEventStatus(true);
            StartCoroutine(zombie.PushBackCoroutine(0.7f, 0.3f));
           
        }
        yield return null;
    }
    public IEnumerator PushDownZombies()//아래로 밀기
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
