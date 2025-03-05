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
        inOutPanel.SetActive(true);
        partA = false;
        partB = true;
        StartCoroutine(SwitchPanel(partAPanel,partBPanel));
    }

    public void StartPartC()
    {
        inOutPanel.SetActive(true);
        partB = false;
        partC = true;
        StartCoroutine(SwitchPanel(partBPanel,partCPanel));
    }

    public void StartPartD()
    {
        inOutPanel.SetActive(true);
        partC = false;
        partD = true;
        StartCoroutine(SwitchPanel(partCPanel,partDPanel));
    }

    private IEnumerator SwitchPanel(GameObject from, GameObject to)
    {
        yield return new WaitForSeconds(0.2f);
        from.SetActive(false);
        to.SetActive(true);
    }
}
