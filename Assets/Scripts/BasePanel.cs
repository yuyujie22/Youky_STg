using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePanel : MonoBehaviour
{

    private KeyCode esc = KeyCode.Escape;
    public GameObject UIPanel;
  

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(esc))
        {
            UIPanel.SetActive(!UIPanel.activeSelf);
        }
    }
}
