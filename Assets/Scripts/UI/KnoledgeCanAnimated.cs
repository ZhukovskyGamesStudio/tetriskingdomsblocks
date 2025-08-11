using UnityEngine;

public class KnoledgeCanAnimated : MonoBehaviour
{
    [SerializeField]
    private Animator _mouthanimation;
    [SerializeField]
    private Animator _eyesanimation;

    public void SetAnimationState(int type, bool isSpeaking) {
        _mouthanimation.SetBool("IsSpeaking", isSpeaking);
        _mouthanimation.SetInteger("Type", type);
        _eyesanimation.SetInteger("Type", type);
    }
}
