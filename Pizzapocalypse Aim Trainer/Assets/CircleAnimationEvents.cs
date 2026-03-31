using UnityEngine;

public class CircleAnimationEvents : MonoBehaviour
{
    private ClickMinigame clickMinigame;
    private SequenceMinigame sequenceMinigame;

    public void Initialize(ClickMinigame game)
    {
        clickMinigame = game;
    }
    
    public void InitializeForSequence(SequenceMinigame game)
    {
        sequenceMinigame = game;
    }

    public void OnCircleAnimationComplete()
    {
        if (clickMinigame != null)
        {
            clickMinigame.OnCircleAnimationComplete(gameObject);
        }
        else if (sequenceMinigame != null)
        {
            sequenceMinigame.OnCircleAnimationComplete(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }

    }
}
