using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireIcePlus_Effect : MonoBehaviour
{
    private AudioSource Aus;
    public float _volume = 1;
    private void Awake()
    {
        Aus.GetComponent<AudioSource>();
    }
    private void OnEnable()
    {

        GameManager.Instance.ShakeCam();
        Aus.volume = _volume;

    }
    private void OnDisable()
    {
        Aus.volume = 0.7f;
    }
}
