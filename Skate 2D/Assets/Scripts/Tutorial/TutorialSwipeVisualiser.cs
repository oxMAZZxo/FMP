using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialSwipeVisualiser : MonoBehaviour
{
    [SerializeField]private TutorialSkateboard skateboard;
    [SerializeField]private GameObject arrowVisualiser;
    private int[] trickDirections = {0,90,180,225,270};
    private int[] grindDirections = {90,180,270};
    private int index = 0;

    public void ShowArrow()
    {
        arrowVisualiser.SetActive(true);
        arrowVisualiser.transform.rotation = TutorialManager.Instance.partB ? GetGrindRotations() : GetTrickRotations();
        
    }

    private void OnSwipeInput(object sender, TouchEventArgs e)
    {
        index++;
        if(TutorialManager.Instance.partB && index > 3)
        {
            index = 0;
        }
        if(index > 5) {index = 0;}
        arrowVisualiser.SetActive(false);
    }

    private void UpdateCounter()
    {
        
    }

    private Quaternion GetGrindRotations()
    {
        if(index > grindDirections.Length - 1) {index = 0;}
        Quaternion rotation = Quaternion.Euler(0,0,grindDirections[index]);

        return rotation;
    }

    private Quaternion GetTrickRotations()
    {
        if(index > trickDirections.Length - 1) {index = 0;}
        Quaternion rotation = Quaternion.Euler(0,0,trickDirections[index]);

        return rotation;
    }
    

    void OnEnable()
    {
        TouchControls.touchEvent += OnSwipeInput;
    }

    void OnDisable()
    {
        TouchControls.touchEvent -= OnSwipeInput;
    }
}
