using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityGenerator : MonoBehaviour
{
    public Transform entityPrefab;
    Transform prefabCache;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            prefabCache = GameObject.Instantiate(entityPrefab);

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
