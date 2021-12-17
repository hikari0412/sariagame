using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoliekaileManage : MonoBehaviour
{
    public static WoliekaileManage instance;
    public vThirdPersonController player;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
