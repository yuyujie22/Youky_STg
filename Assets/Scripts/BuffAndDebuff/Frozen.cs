using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frozen : Debuff
{
    public Animator anim;
    public EnemyAI aiScript;
    
    private void OnEnable()
    {
        anim.enabled = false;
        aiScript.enabled = false;
        StartCoroutine(disableSelf());
    }
    private void OnDisable()
    {
        anim.enabled = true;
        aiScript.enabled = true;
    }
    //冰块效果消失
    IEnumerator disableSelf()
    {
        yield return new WaitForSeconds(effetTime);
        this.gameObject.SetActive(false);
    }
    //再次被击中，重置冷却
    public void resetCool(float time)
    {
        effetTime = time;
        StopCoroutine(disableSelf());
        StartCoroutine(disableSelf());
    }
}

