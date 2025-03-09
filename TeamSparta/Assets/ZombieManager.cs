using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum State
{
    Move,
    Idle,
    Die
}
public class ZombieManager : Singleton<ZombieManager>
{

    public GameObject zombiePrefab; // 좀비 프리팹
    public List<ZombieController> zombies = new List<ZombieController>(); // 좀비 리스트

    public void SpawnZombie(Vector2 position)    // 좀비 생성
    {
        for(int i = 0; i < 30; i++)
        {
            GameObject newZombie = Instantiate(zombiePrefab, position, Quaternion.identity);
            ZombieController zombieController = newZombie.GetComponent<ZombieController>();
            zombieController.zombieData = new ZombieData(10, 10, 1);//hp ,max hp ,speed
            newZombie.SetActive(false);
            if (zombieController != null)
            {
                zombies.Add(zombieController);
            }
        }
    }
    public GameObject GetZombieInfoByIndex(int index)
    {
        if (index >= 0 && index < zombies.Count)
        {
            ZombieController zombie = zombies[index];
            if (zombie != null)
            {
                return zombie.gameObject;
            }
        }
        return null;
      
    }

    public void PauseAllZombies(ZombieController zombieController, bool isPaused)    // 전체 좀비 행동 일시 정지
    {
        foreach (ZombieController zombie in zombies)
        {
            if (!zombie.isGround || !zombie.isFirstZombieHitWall || zombieController.gameObject.layer != zombie.gameObject.layer) continue;

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
    public IEnumerator PushBackZombies(GameObject gameObject)//뒤로 밀기
    {
        foreach (ZombieController zombie in zombies)
        {
            if (!zombie.isFirstZombieHitWall || !zombie.isGround || zombie.currentState == State.Move || gameObject.layer != zombie.gameObject.layer) continue;

            zombie.SetEventStatus(true);
            StartCoroutine(zombie.PushBackCoroutine(0.7f, 0.3f));
           
        }
        yield return null;
    }
    public IEnumerator PushDownZombies(GameObject gameObject)//아래로 밀기
    {
        foreach (ZombieController zombie in zombies)
        {
            if (zombie.currentState == State.Move || zombie.isGround || gameObject.layer != zombie.gameObject.layer) continue;
          
            zombie.SetEventStatus(true);
            StartCoroutine(zombie.PushDownCoroutine(0.7f, 0.1f));
            

        }
        yield return null;
    }
}
