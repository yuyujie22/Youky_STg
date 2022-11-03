using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frozen : Debuff
{
    public Animator anim;
    public EnemyAI aiScript;
    public bool isFrozen;
    public float curTime;

    private void Update()
    {
        if (curTime >= 0)
        {
            isFrozen = true;
            curTime -= Time.deltaTime;
            
        }
        else
        {
            isFrozen = false;
            this.gameObject.SetActive(false);
        }
    }
    private void OnEnable()
    {
        anim.enabled = false;
        aiScript.enabled = false;
        curTime = effetTime;
    }
    private void OnDisable()
    {
        anim.enabled = true;
        aiScript.enabled = true;
    }

    //再次被击中，重置冷却
    public void resetCool(float time)
    {
        curTime = time;
    }
}

