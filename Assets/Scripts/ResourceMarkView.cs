using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceMarkView : MonoBehaviour
{
    [SerializeField] private TMP_Text _resourceMarkText;
    [SerializeField] private Image _resourceFillImage;
    [SerializeField] private int markIndex;

    public void SetResourceMarkInfo(int maxResource, int currentResource, ResourceType resourceType, int index)
    {
        markIndex = index;
        if ((float)currentResource / maxResource > 0.1f)
        {
            gameObject.SetActive(true);
        _resourceMarkText.text = currentResource + "/"+maxResource + "\n <sprite name=" + resourceType + ">";
        _resourceFillImage.fillAmount = (float)currentResource / maxResource;
        }
    }

    public void CollectResources()
    {
        gameObject.SetActive(false);
        MetaManager.Instance.CollectResourcesFromMark(markIndex);
    }
}
