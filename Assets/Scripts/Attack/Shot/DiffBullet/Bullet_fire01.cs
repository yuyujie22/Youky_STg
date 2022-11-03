using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_fire01 : Bullet
{
    public string frozenDebuffName = "fire";

    public int attackPoint = 2;
    //冻结后攻击
    public int iceFireAttackPoint = 6;
    private StatusRecord statusRecord;

    public override void Attack(Collider2D collision)
    {
        base.Attack(collision);
        //Debug.Log("11::" + collision.gameObject);
        //Debug.Log("11::" + t.gameObject);
        statusRecord = collision.GetComponent<StatusRecord>();
        Fired firedComp = statusRecord.firedComp;
        GameObject t = firedComp.gameObject;
        if (statusRecord.isFrozen())
        {
            t.SetActive(true);
            statusRecord.HP -= iceFireAttackPoint;
            //结束冻结
            statusRecord.StopFrozen();
            GameManager.Instance.ShakeCam();
            //Destroy(collision.gameObject, firedComp.killTime);
        }
        else
        {
            
            t.SetActive(true);
            statusRecord.HP -= attackPoint;
            statusRecord.OnHit();
        }
        BeatBack(collision);

    }
   
    public void BeatBack(Collider2D collision)
    {
        collision.GetComponent<Rigidbody2D>().AddForce(beatbackForce * ((rb.velocity.x > 0)? Vector2.right:Vector2.left), ForceMode2D.Impulse);
    }
}
