using UnityEngine;
using Invector;
using Invector.vCharacterController;
using System;

namespace PixelCrushers.InvectorSupport
{

    /// <summary>
    /// Save System saver for Invector character's stats.
    /// </summary>
    [AddComponentMenu("Pixel Crushers/Save System/Savers/Invector/Invector Stats Saver")]
    //--- Don't require, so vGameController can destroy:
    //[RequireComponent(typeof(vHealthController))]
    public class InvectorStatsSaver : Saver
    {

        public static bool isResettingScene = false;

        [Tooltip("Save health value in vHealthController. If unticked, save health value in vThirdPersonMotor.")]
        public bool saveHealthInHealthController = true;

        [Serializable]
        public class Data
        {
            // vHealthController:
            public float health;
            public float maxHealth;
            // vThirdPersonMotor
            public float maxStamina;
            public float staminaRecovery;
            public float currentStaminaRecovery;
            public float currentStamina;
            public float sprintStamina;
            public float jumpStamina;
            public float rollStamina;
        }

        private vHealthController m_controller = null;
        private vHealthController controller
        {
            get
            {
                if (m_controller == null) m_controller = GetComponent<vHealthController>();
                return m_controller;
            }
        }

        private vThirdPersonMotor m_motor = null;
        private vThirdPersonMotor motor
        {
            get
            {
                if (m_motor == null) m_motor = GetComponent<vThirdPersonMotor>();
                return m_motor;
            }
        }

        public override string RecordData()
        {
            var data = new Data();
            if (saveHealthInHealthController && controller == null) return string.Empty;
            if (!saveHealthInHealthController && motor == null) return string.Empty;
            if (motor != null)
            {
                if (!saveHealthInHealthController)
                {
                    data.health = motor.currentHealth;
                    data.maxHealth = motor.maxHealth;
                }
                data.staminaRecovery = motor.staminaRecovery;
                data.currentStaminaRecovery = motor.currentStaminaRecoveryDelay;
                data.maxStamina = motor.maxStamina;
                data.currentStamina = motor.currentStamina;
                data.sprintStamina = motor.sprintStamina;
                data.jumpStamina = motor.jumpStamina;
                data.rollStamina = motor.rollStamina;
            }
            if (saveHealthInHealthController)
            {
                data.health = controller.currentHealth;
                data.maxHealth = controller.maxHealth;
            }
            return SaveSystem.Serialize(data);
        }

        public override void ApplyData(string s)
        {
            if (isResettingScene)
            {
                isResettingScene = false;
                return;
            }
            if (string.IsNullOrEmpty(s)) return;
            if (saveHealthInHealthController && controller == null) return;
            if (!saveHealthInHealthController && motor == null) return;

            var data = SaveSystem.Deserialize<Data>(s);
            if (data == null) return;

            // Note inconsistency: ChangeMaxHealth/Stamina adds (+=), ChangeHealth/Stamina sets (=).

            if (saveHealthInHealthController)
            {
                controller.ChangeMaxHealth((int)(data.maxHealth - controller.maxHealth));
                controller.ChangeHealth((int)data.health);
            }

            if (motor != null)
            {
                motor.staminaRecovery = data.staminaRecovery;
                motor.currentStaminaRecoveryDelay = data.currentStaminaRecovery;
                motor.sprintStamina = data.sprintStamina;
                motor.jumpStamina = data.jumpStamina;
                motor.rollStamina = data.rollStamina;
                motor.ChangeMaxStamina((int)(data.maxStamina - motor.maxStamina));
                motor.ChangeStamina((int)(data.currentStamina - motor.currentStamina));
                if (!saveHealthInHealthController)
                {
                    motor.ChangeMaxHealth((int)(data.maxHealth - controller.maxHealth));
                    motor.ChangeHealth((int)data.health);
                }
            }
        }

    }

}
