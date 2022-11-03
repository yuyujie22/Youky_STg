using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//冰弹无伤害，但冰住再打伤害3倍
public class Bullet_ice01 : Bullet
{
    public string frozenDebuffName = "frozen";
    private void Awake()
    {
        effectTime = 3.0f;
        reactForce = 1.0f;
    }

    public override void Attack(Collider2D collision)
    {
        base.Attack(collision);
        //Debug.Log("11::" + collision.gameObject);
        //Debug.Log("11::" + t.gameObject);
        StatusRecord statusRecord = collision.GetComponent<StatusRecord>();
        GameObject t = statusRecord.frozenComp.gameObject;
        if (t.activeSelf)
            t.GetComponent<Frozen>().resetCool(effectTime);
        else
            t.GetComponent<Frozen>().gameObject.SetActive(true);

        BeatBack(collision);
        if (statusRecord.isFired())
        {
            statusRecord.StopFire();
        }

    }
    public void BeatBack(Collider2D collision)
    {
        collision.GetComponent<Rigidbody2D>().AddForce(beatbackForce * ((rb.velocity.x > 0) ? Vector2.right : Vector2.left), ForceMode2D.Impulse);
    }
}
