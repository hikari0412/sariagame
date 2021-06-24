using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
#if UNITY_EDITOR
    [vFSMHelpbox("Requires a ControlAIShooter", UnityEditor.MessageType.Info)]
#endif
    public class vAIShooterAttack : vStateAction
    {
        public override string categoryName
        {
            get { return "Combat/"; }
        }
        public override string defaultName
        {
            get { return "Trigger ShooterAttack"; }
        }

        public vAIShooterAttack()
        {
            executionType = vFSMComponentExecutionType.OnStateEnter | vFSMComponentExecutionType.OnStateExit | vFSMComponentExecutionType.OnStateUpdate;
        }
     
        [vHelpBox("Use this to ignore attack time")]
        public bool forceCanAttack;

        public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
        {
            if (fsmBehaviour.aiController is vIControlAIShooter)
            {
                ControlAttack(fsmBehaviour, fsmBehaviour.aiController as vIControlAIShooter);
            }
        }

        protected virtual void ControlAttack(vIFSMBehaviourController fsmBehaviour, vIControlAIShooter combat, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
        {
            switch (executionType)
            {
                case vFSMComponentExecutionType.OnStateEnter:
                    InitAttack(combat);
                    break;
                case vFSMComponentExecutionType.OnStateUpdate:
                    HandleAttack(fsmBehaviour, combat);
                    break;
                case vFSMComponentExecutionType.OnStateExit:
                    FinishAttack(combat);
                    break;
            }
        }

        protected virtual void InitAttack(vIControlAIShooter combat)
        {
            combat.isInCombat = true;
            combat.InitAttackTime();
        }

        protected virtual void HandleAttack(vIFSMBehaviourController fsmBehaviour, vIControlAIShooter combat)
        {
            combat.AimToTarget();
            if (!combat.isAiming) return;
            combat.Attack(forceCanAttack: forceCanAttack);
        }
      
        protected virtual void FinishAttack(vIControlAIShooter combat)
        {
            combat.isInCombat = false;
            combat.ResetAttackTime();
        }
    }
}