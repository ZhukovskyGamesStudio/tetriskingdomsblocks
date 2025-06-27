using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class SpawnedForOneCharTextView : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _needText;

    private void Awake()
    {
        _needText.text = "";
    }
    
    public void SetText(string needText) => _needText.text = needText;

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
                _needText.text += needString[i].ToString();
                yield return new WaitForSeconds(0.05f);
            }
        }
        
        
    }
}
