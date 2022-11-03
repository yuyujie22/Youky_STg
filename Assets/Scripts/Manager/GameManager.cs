using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public Transform player;
    public GameObject playerPrefab;
    public GameObject EnemyPrefab;
    public ShakeCamera shakeCam;
    public Texture2D cursorTex;
    public Transform[] spawnPos;

    override protected void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        Cursor.SetCursor(cursorTex, new Vector2(16, 16), CursorMode.Auto);

    }

    public void RegisterPlayer(Transform pl)
    {
        player = pl;
        
    }

   public void ShakeCam()
    {
        shakeCam.enabled = true;
    }
    private int flag = 0;
    public void Spawnenemy()
    {
        flag++;
        GameObject enemy =  Instantiate(EnemyPrefab, spawnPos[flag% spawnPos.Length].position, spawnPos[flag % spawnPos.Length].rotation) as GameObject;
    }
    public void quit()
    {
        Application.Quit();
    }
}
