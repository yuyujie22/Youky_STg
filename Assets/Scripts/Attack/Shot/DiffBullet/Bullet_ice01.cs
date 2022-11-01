using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_ice01 : Bullet
{
    public string frozenDebuffName = "frozen";
    public float effectTime = 3.0f;

    public override void Attack(Collider2D collision)
    {
        base.Attack(collision);
        Debug.Log("11::" + collision.gameObject);
        GameObject t =  collision.transform.Find("BuffAndDebuff").Find(frozenDebuffName).gameObject;
        Debug.Log("11::" + t.gameObject);

        if (t.activeSelf)
            t.GetComponent<Frozen>().resetCool(effectTime);
        else
            t.GetComponent<Frozen>().gameObject.SetActive(true);

    }
}
