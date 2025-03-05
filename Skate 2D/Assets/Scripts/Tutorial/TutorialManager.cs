using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance {get; private set;}
    public bool partA{get; private set;}
    public bool partB{get; private set;}
    public bool partC{get; private set;}
    public bool partD{get; private set;}
    public bool shouldRoll {get; private set;}
    public bool isPaused {get; private set;}
    [SerializeField]private GameObject inOutPanel;
    [SerializeField]private GameObject partAPanel;
    [SerializeField]private GameObject partBPanel;
    [SerializeField]private GameObject partCPanel;
    [SerializeField]private GameObject partDPanel;
    [SerializeField]private GameObject trickCounterDisplay;
    [SerializeField]private GameObject wellDoneDisplay;
    
    void Awake()
    {
        if(Instance == null && Instance != this)
        {
            Instance = this;
        }else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        partA = true;
        partB = false;
        partC = false;
        partD = false;
    }

    public void StartPartB()
    {
        partA = false;
        partB = true;
        StartCoroutine(SwitchPanel(partAPanel,partBPanel));
    }

    public void StartPartC()
    {
        partB = false;
        partC = true;
        StartCoroutine(SwitchPanel(partBPanel,partCPanel));
    }

    public void StartPartD()
    {
        partC = false;
        partD = true;
        StartCoroutine(SwitchPanel(partCPanel,partDPanel));
    }

    private IEnumerator SwitchPanel(GameObject from, GameObject to)
    {
        from.SetActive(false);
        trickCounterDisplay.SetActive(false);
        wellDoneDisplay.SetActive(true);
        yield return new WaitForSeconds(1f);
        inOutPanel.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        wellDoneDisplay.SetActive(false);
        to.SetActive(true);
    }
}
