using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitOnEnable : MonoBehaviour
{
    public Behaviour[] Components = null;

    private IEnumerator Wait()
    {
        for (var i = 0; i < 5; i++)
            yield return null;

        foreach (var component in Components)
            component.enabled = true;

        Destroy(this);
    }

    private void Start()
    {
        StartCoroutine(Wait());
    }
}
