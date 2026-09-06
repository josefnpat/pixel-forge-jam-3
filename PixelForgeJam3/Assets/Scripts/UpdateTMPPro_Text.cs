using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdateTMPPro_Text : MonoBehaviour
{
    [SerializeField]
    private List<TMP_Text> tmpTexts;
    private Thing _thing;

    public void Awake()
    {
        _thing = GetComponent<Thing>();
        _thing.OnSpriteRendererSortingOrder.AddListener(SpriteRendererSortingOrder);
    }

    private void SpriteRendererSortingOrder(int sortingOrder)
    {
        foreach (TMP_Text tmpText in tmpTexts)
        {
            tmpText.GetComponent<TextMeshPro>().sortingOrder = sortingOrder;
        }
    }
}
