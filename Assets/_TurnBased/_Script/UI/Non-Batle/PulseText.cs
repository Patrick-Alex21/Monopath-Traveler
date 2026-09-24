using UnityEngine;
using TMPro;

public class PulseText : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    public float speed = 2f; 
    public float minAlpha = 0.3f; 
    public float maxAlpha = 1f; 

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (textMesh != null)
        {
            Color color = textMesh.color;
            
            color.a = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * speed) + 1f) / 2f);
            textMesh.color = color;
        }
    }
}