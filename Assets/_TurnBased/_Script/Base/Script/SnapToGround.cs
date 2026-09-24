using UnityEngine;

public class SnapToGround : MonoBehaviour
{
    [SerializeField] private float offset = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    public void Snap()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out hit, 10f, groundLayer))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + offset, transform.position.z);
        }
    }
}