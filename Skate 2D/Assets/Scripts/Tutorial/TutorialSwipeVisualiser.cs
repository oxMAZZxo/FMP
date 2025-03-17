using UnityEngine;

public class TutorialSwipeVisualiser : MonoBehaviour
{
    [SerializeField]private TutorialSkateboard skateboard;
    [SerializeField]private GameObject arrowVisualiser;
    private int[] trickDirections = {0,90,180,225,270};
    private int[] grindDirections = {90,180,270};
    private int index = 0;

    private void OnTouchStarted(object sender, Vector2 touchStart)
    {
        if(TutorialManager.Instance.partB && skateboard.isGrounded)
        {
            return;
        }
        Debug.Log($"Visualising swipe at position {touchStart}");
        arrowVisualiser.transform.position = touchStart;
        arrowVisualiser.transform.rotation = TutorialManager.Instance.partB ? GetGrindRotations() : GetTrickRotations();
        arrowVisualiser.SetActive(true);
    }

    private Quaternion GetGrindRotations()
    {
        if(index > grindDirections.Length - 1) {index = 0;}
        Quaternion rotation = Quaternion.Euler(0,0,grindDirections[index]);
        index++;

        return rotation;
    }

    private Quaternion GetTrickRotations()
    {
        if(index > trickDirections.Length - 1) {index = 0;}
        Quaternion rotation = Quaternion.Euler(0,0,trickDirections[index]);
        index++;

        return rotation;
    }
    
    private void OnTouchEnded(object sender, Vector2 e)
    {
        arrowVisualiser.SetActive(false);
    }

    void OnEnable()
    {
        TouchControls.touchStarted += OnTouchStarted;
        TouchControls.touchEnded += OnTouchEnded;
    }

    void OnDisable()
    {
        TouchControls.touchStarted -= OnTouchStarted;
        TouchControls.touchEnded -= OnTouchEnded;
    }
}
