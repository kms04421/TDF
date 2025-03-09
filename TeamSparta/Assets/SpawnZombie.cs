using System.Collections;
using UnityEngine;

public class SpawnZombie : MonoBehaviour
{
    WaitForSeconds spawnDelayTime;
    ZombieManager zombieManager;
    int[] layerMapping = { 0, 0, 0, 0, 0, 0, 0, 3, 6 };
    // Start is called before the first frame update
    void Start()
    {
        zombieManager = ZombieManager.Instance;
        spawnDelayTime = new WaitForSeconds(0.5f);
        zombieManager.SpawnZombie(transform.position);
        StartCoroutine(SpawnStart());
    }
    IEnumerator SpawnStart()
    {
        GameObject zombiego;
        while (true)
        {
            for (int i = 0; i < zombieManager.zombies.Count; i++)
            {
                zombiego = zombieManager.GetZombieInfoByIndex(i);
                if (zombiego == null) continue;
                if (zombiego.activeSelf) continue;
                int randomLayer = Random.Range(6, 9);
                zombiego.layer = randomLayer;
         
                zombiego.transform.position = new Vector3( zombiego.transform.position.x,zombiego.transform.position.y,layerMapping[randomLayer]);
                zombiego.SetActive(true);
                // 자식 오브젝트의 레이어를 동일하게 변경
                foreach (Transform child in zombiego.transform.GetComponentsInChildren<Transform>(true))
                {
                    child.gameObject.layer = randomLayer;

                }
                yield return spawnDelayTime;
            }
            yield return spawnDelayTime;
        }

    }
}
