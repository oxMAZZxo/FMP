using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UITrailPoint : MonoBehaviour
{
    private float lifetime;
    private Image myImage;
    [SerializeField] private AnimationCurve widthOverTime = AnimationCurve.Linear(0, 1, 1, 0); // Controls width scaling
    [SerializeField] private float maxWidth = 100f; // Max width in pixels
    private float currentTime;   // Time passed since this point was rendered
    private RectTransform rectTransform;

    void Start()
    {
        myImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        myImage.enabled = false;
    }

    public void Init(float newPointAliveTime)
    {
        lifetime = newPointAliveTime;
    }

    void FixedUpdate()
    {
        if (!myImage.enabled) return;

        currentTime += Time.deltaTime;
        float t = currentTime / lifetime;

        // Evaluate width and apply it to the RectTransform
        float width = widthOverTime.Evaluate(t) * maxWidth;
        Vector2 size = rectTransform.sizeDelta;
        size.y = width;
        rectTransform.sizeDelta = size;

        // OPTIONAL: Also fade out
        Color c = myImage.color;
        c.a = 1f - t; // Linear fade (you could use another curve)
        myImage.color = c;
    }

    public void Render()
    {
        myImage.enabled = true;
        Color c = myImage.color;
        c.a = 1f;
        myImage.color = c;
        currentTime = 0f;
        StopAllCoroutines();
        StartCoroutine(DisableMe());
    }


    IEnumerator DisableMe()
    {
        yield return new WaitForSeconds(lifetime);
        myImage.enabled = false;
    }
}
