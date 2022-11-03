using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fired : Debuff
{

    public float curTime;
    public bool isFired;
    public float killTime;
    public GameObject enemy;
    public float boomPower = 10;
    public StatusRecord statusRecord;
    public int fireBurnPoint = 1;

    private void Update()
    {
        if (curTime >= 0)
        {
            curTime -= Time.deltaTime;
            isFired = true;
            fireBurn();

        }
        else
        {
            //结束效果
            this.gameObject.SetActive(false);
            isFired = false;
        }
    }
    private void OnEnable()
    {
        //开始效果
        curTime = effetTime;
        bool isRight = (GameManager.Instance.player.position.x < enemy.transform.position.x);
        Vector2 vec = (isRight) ? Vector2.right : Vector2.left;
        enemy.GetComponent<Rigidbody2D>().AddForce(vec * boomPower, ForceMode2D.Impulse);
        fireBurnTime = 0;
    }
    private void OnDisable()
    {
        //结束效果的实现
        
    }

    //再次被击中，重置冷却
    public void resetCool(float time)
    {
        curTime = time;
    }
    private float fireBurnTime;
    public void fireBurn()
    {

        fireBurnTime -= Time.deltaTime;
        if (fireBurnTime <= 0)
        {
            //执行扣血
            statusRecord.HP -= fireBurnPoint;
            fireBurnTime = 1;
        }
    }
}
