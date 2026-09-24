using UnityEngine;

public class TargetIndicator : MonoBehaviour
{
    public void SetPivot(Vector3 localPosition)
    {
        transform.localPosition = localPosition;
    }
}