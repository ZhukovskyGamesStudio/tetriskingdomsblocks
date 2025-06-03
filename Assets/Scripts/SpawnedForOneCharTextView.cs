using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class SpawnedForOneCharTextView : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _needText;

    private void Start()
    {
        _needText.text = "";
    }

    public IEnumerator StartSpawnText(string needString)
    {
        string iconText = "";
        bool isStartingIconText = false;
        for (int i = 0; i < needString.Length; i++)
        {
            if (needString[i] == '<')
            {
                yield return new WaitForSeconds(0.05f);
                isStartingIconText = true;
                iconText += needString[i];
            }
            else if (isStartingIconText )
            {
                iconText += needString[i];
                if (needString[i] == '>')
                {
                _needText.text += iconText;
                isStartingIconText = false;
                iconText = "";
                }
            }
            else
            {
                _needText.text += needString[i];
                yield return new WaitForSeconds(0.15f);
            }
        }
    }
}
