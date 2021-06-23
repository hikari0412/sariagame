
namespace Invector.vCharacterController.AI.FSMBehaviour
{
#if UNITY_EDITOR
    [vFSMHelpbox("This Custom State is DEPRECATED, use the Action Chase instead", UnityEditor.MessageType.Info)]
#endif
    public class vAIChaseState : vFSMState
    {
        public bool chaseInStrafe = false;

        public vAIChaseState()
        {
#if UNITY_EDITOR
            description = "Custom Chase State";
#endif
        }
        public vAIMovementSpeed chaseSpeed = vAIMovementSpeed.Running;
        public override void UpdateState(vIFSMBehaviourController fsmBehaviour)
        {
            base.UpdateState(fsmBehaviour);
            
            if (fsmBehaviour != null && fsmBehaviour.aiController.currentTarget.transform != null)            {

                if (chaseInStrafe)
                    fsmBehaviour.aiController.StrafeMoveTo(fsmBehaviour.aiController.lastTargetPosition, (fsmBehaviour.aiController.lastTargetPosition - fsmBehaviour.aiController.transform.position).normalized,chaseSpeed);
                else
                    fsmBehaviour.aiController.MoveTo(fsmBehaviour.aiController.lastTargetPosition,chaseSpeed);
            }
        }
    }
}