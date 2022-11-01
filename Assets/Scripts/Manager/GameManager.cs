using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public Transform player;
    public GameObject playerPrefab;

    override protected void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        
    }

    public void RegisterPlayer(Transform pl)
    {
        player = pl;
        
    }
   
}
