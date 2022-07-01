using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPScene : MonoBehaviour
{
    [Header("要去的坐标")]
    public Transform target;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = target.position;
            other.transform.rotation = target.rotation;
        }
    }
}
