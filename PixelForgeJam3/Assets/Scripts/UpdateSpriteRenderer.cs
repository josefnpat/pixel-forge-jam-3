using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdateSpriteRenderer : MonoBehaviour
{
    [SerializeField]
    private List<SpriteRenderer> spriteRenderers;
    private Thing _thing;

    public void Awake()
    {
        _thing = GetComponent<Thing>();
        _thing.OnSpriteRendererSortingOrder.AddListener(SpriteRendererSortingOrder);
    }

    private void SpriteRendererSortingOrder(int sortingOrder)
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sortingOrder = sortingOrder + 1;
        }
    }
}
