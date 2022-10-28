using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoDestroyOnload : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

}
