using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityGenerator : MonoBehaviour
{
    public Transform entityPrefab;
    GameObject prefabCache;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            prefabCache = GameObject.Instantiate(entityPrefab).gameObject;
			prefabCache.transform.parent = transform;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(prefabCache);

        }
    }
}
