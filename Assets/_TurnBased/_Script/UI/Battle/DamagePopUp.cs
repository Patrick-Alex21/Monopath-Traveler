using UnityEngine;
using TMPro;
using System.Collections;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh; 
    [SerializeField] private float fadeDuration = 1f;

    private Coroutine animCoroutine;

    public void Setup(string textContent, Color textColor, float xOffset = 0f, float yOffset = 0f)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
        
        textMesh.text = textContent;
        textMesh.color = textColor;

        transform.position += new Vector3(xOffset, yOffset, 0);

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimatePopup());
    }

    private IEnumerator AnimatePopup()
    {
        float elapsed = 0f;
        Color startColor = textMesh.color;
        
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0, 1.5f, 0);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            transform.position = Vector3.Lerp(startPos, endPos, t);
            
            if (t > 0.5f)
            {
                startColor.a = Mathf.Lerp(1f, 0f, (t - 0.5f) * 2f);
                textMesh.color = startColor;
            }
            yield return null;
        }

        if (DamagePopupPool.Instance != null)
            DamagePopupPool.Instance.Return(this);
        else
            Destroy(gameObject); 
    }
}