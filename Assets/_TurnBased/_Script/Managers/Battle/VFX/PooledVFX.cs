using UnityEngine;
using System.Collections;

public class PooledVFX : MonoBehaviour
{
    [SerializeField] private string poolKey;
    [SerializeField] private float lifetime = 0.5f;

    private Coroutine returnRoutine;

    private void OnEnable()
    {
        returnRoutine = StartCoroutine(ReturnAfterLifetime());
    }

    private void OnDisable()
    {
        if (returnRoutine != null) StopCoroutine(returnRoutine);
    }

    private IEnumerator ReturnAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);
        VFXPool.Instance?.Return(poolKey, gameObject);
    }
}