using UnityEngine;
using System.Collections.Generic;

public class DamagePopupPool : MonoBehaviour
{
    public static DamagePopupPool Instance { get; private set; }

    [SerializeField] private DamagePopup popupPrefab;
    [SerializeField] private int initialPoolSize = 20;

    private readonly Queue<DamagePopup> _pool = new Queue<DamagePopup>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        for (int i = 0; i < initialPoolSize; i++)
        {
            DamagePopup popup = CreateNewPopup();
            _pool.Enqueue(popup);
        }
    }

    private DamagePopup CreateNewPopup()
    {
        DamagePopup popup = Instantiate(popupPrefab, transform);
        popup.gameObject.SetActive(false);
        return popup;
    }

    public DamagePopup Get(Vector3 position, string textContent, Color textColor, float xOffset = 0f, float yOffset = 0f)
    {
        DamagePopup popup = _pool.Count > 0 ? _pool.Dequeue() : CreateNewPopup();

        popup.transform.position = position;
        popup.gameObject.SetActive(true);
        popup.Setup(textContent, textColor, xOffset, yOffset);

        return popup;
    }

    public void Return(DamagePopup popup)
    {
        popup.gameObject.SetActive(false);
        popup.transform.SetParent(transform);
        _pool.Enqueue(popup);
    }
}