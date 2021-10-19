using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Skill3 : MonoBehaviour
{
    public int enemyDamage;
    public float enemySlowDown;
    public int playerHealthRecoverRate;
    public ParticleSystem particleSystem; 
    float oldAgentSpeed;
    

    void Update() {
        if (particleSystem.isStopped) {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Invector.vHealthController HealthController = other.gameObject.GetComponent<Invector.vHealthController>();
            HealthController.healthRecovery = -enemyDamage;
                if (this.gameObject.activeInHierarchy && !HealthController.inHealthRecovery)
                    StartCoroutine(HealthController.RecoverHealth());
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Invector.vHealthController HealthController = other.gameObject.GetComponent<Invector.vHealthController>();
            HealthController.healthRecovery = playerHealthRecoverRate;
                if (this.gameObject.activeInHierarchy && !HealthController.inHealthRecovery)
                    StartCoroutine(HealthController.RecoverHealth());
        }
    }
    private void OnTriggerStay(Collider other)
    {

        if (particleSystem.isStopped)
        {
            OnTriggerExit(other);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Invector.vHealthController HealthController = other.gameObject.GetComponent<Invector.vHealthController>();
            HealthController.healthRecovery = 0f;
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Invector.vHealthController HealthController = other.gameObject.GetComponent<Invector.vHealthController>();
            HealthController.healthRecovery = 0f;
        }
    }
}
