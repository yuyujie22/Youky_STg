using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager :   Singleton<GameManager>
{
    public GameObject player;
    public GameObject playerPrefab;
    CinemachineVirtualCamera followCamera;
    override protected void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }   
   
    public void RegisterPlayer(GameObject pl)
    {
        
    }
    void Update()
    {
    }
  
}
