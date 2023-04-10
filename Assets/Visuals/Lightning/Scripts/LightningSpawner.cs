using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningSpawner : MonoBehaviour
{

    #region Lightning spawning
    // [Header("Lightning spawning")]
    [Range(0, 100)]
    [SerializeField][HideInInspector] private float LightningSpawnChance = 3;
    [SerializeField][HideInInspector] private Bounds LightningSpawnBounds;
    [SerializeField][HideInInspector] private GameObject LightningPrefab;
    [SerializeField][HideInInspector] private Transform LightningParent;
    [SerializeField][HideInInspector] private int LightningPoolSize = 25;
    private List<GameObject> pooledLightning = new List<GameObject>();
    #endregion

    public bool spawnLightning = false;

    public void Start()
    {
        pooledLightning = new List<GameObject>();
        for (int i = 0; i < LightningPoolSize; i++)
        {
            GameObject obj = (GameObject)Instantiate(LightningPrefab);
            obj.transform.parent = LightningParent;
            obj.SetActive(false);
            pooledLightning.Add(obj);
        }
    }

    public void Update()
    {
        if (spawnLightning)
        {
            // Maybe increase spawn chances based on difficulty
            if (LightningSpawnChance > Random.Range(0f, 100f))
            {
                Vector3 randomVec = RandomPointInBox();
                GameObject bolt = GetPooledObject();
                if (bolt != null)
                {
                    Debug.Log("Spawned new lightning");
                    bolt.transform.position = new Vector3(bolt.transform.position.x + Random.Range(-50, 50), 150, bolt.transform.position.z + Random.Range(-50, 50));
                    bolt.SetActive(true);
                    bolt.GetComponent<LightningGenerator>().Generate(Random.Range(1, 3));
                }
            }
        }
    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pooledLightning.Count; i++)
        {
            if (!pooledLightning[i].activeInHierarchy)
            {
                return pooledLightning[i];
            }
        }
        return null;
    }

    public Vector3 RandomPointInBox()
    {
        return new Vector3(
            Random.Range(LightningSpawnBounds.min.x, LightningSpawnBounds.max.x),
            Random.Range(LightningSpawnBounds.min.y, LightningSpawnBounds.max.y),
            Random.Range(LightningSpawnBounds.min.z, LightningSpawnBounds.max.z)
        );
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(LightningSpawnBounds.center, LightningSpawnBounds.extents);
    }

}
