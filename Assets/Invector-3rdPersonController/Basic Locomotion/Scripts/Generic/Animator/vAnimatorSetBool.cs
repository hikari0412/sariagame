namespace Invector
{
    public class vAnimatorSetBool : vAnimatorSetValue<bool>
    {      
        [vHelpBox("Random Value between True and False")]
        public bool randomEnter;       
        public bool randomExit;      

        public bool useEnterInverseValue = false;

        protected override bool GetEnterValue()
        {
            return randomEnter ? UnityEngine.Random.Range(0,100)>50 : base.GetEnterValue();
        }
        protected override bool GetExitValue()
        {
            return randomExit ? UnityEngine.Random.Range(0, 100)>50 : base.GetExitValue();
        }
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(UnityEngine.Animator animator, UnityEngine.AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (setOnEnter)
            {
                var enterValue = GetEnterValue();
                enterValue = useEnterInverseValue ? !animator.GetBool(animatorParameter) : enterValue;
                animator.SetBool(animatorParameter, enterValue);
            }
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}
    }
}