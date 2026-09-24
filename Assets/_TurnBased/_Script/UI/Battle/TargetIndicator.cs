using UnityEngine;

public class TargetIndicator : MonoBehaviour
{
    [SerializeField] private float bobbingSpeed = 5f;
    [SerializeField] private float bobbingHeight = 0.3f;

    private float baseY;
    private bool isSet = false;

    public void SetPivot(Vector3 localPosition)
    {
        transform.localPosition = localPosition;
        baseY = localPosition.y;
        isSet = true;
    }

    void Update()
    {
        if (!isSet) return; 
        
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            baseY + (Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight),
            transform.localPosition.z
        );
    }
}