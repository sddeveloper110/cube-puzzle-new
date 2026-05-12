using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHandler : MonoBehaviour
{
    public UnityEvent OnAnimationEvent1;
    public UnityEvent OnAnimationEvent2;
    public UnityEvent OnAnimationEvent3;
    
    public void AnimationEvent1()
    {
        OnAnimationEvent1?.Invoke();
    }
    public void AnimationEvent2()
    {
        OnAnimationEvent2?.Invoke();
    }
    public void AnimationEvent3()
    {
        OnAnimationEvent3?.Invoke();
    }
}
