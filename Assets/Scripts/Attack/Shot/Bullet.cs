using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //爆炸特效
    public GameObject impactParticle;
    //子弹子特效
    public GameObject projectileParticle;
    //生成
    public GameObject muzzleParticle;
    public Rigidbody2D rb;
    [Header("不存在collider时候的半径")]
    public float colliderRadius = 1f;
    [Range(0f, 1f)] // This is an offset that moves the impact effect slightly away from the point of impact to reduce clipping of the impact effect
    public float collideOffset = 0.15f;
    public float speed = 20.0f;
    public float destroyTime = 5.0f;
    public float effectTime = 3.0f;
    //反作用力
    public float reactForce = 1.0f;
    public float beatbackForce = 1.0f;


    private void Start()
    {
        projectileParticle = Instantiate(projectileParticle, transform.position, transform.rotation) as GameObject;
        projectileParticle.transform.parent = transform;
        if (muzzleParticle)
        {
            muzzleParticle = Instantiate(muzzleParticle, transform.position, transform.rotation) as GameObject;
            Destroy(muzzleParticle, 1.5f); // 2nd parameter is lifetime of effect in seconds
        }
    }

    public void SetSpeed(Vector2 direction)
    {
        rb.velocity = direction * speed;
        StartCoroutine(RecycleObj(destroyTime));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            //Debug.Log("Boom" + collision.name);
            GameObject impactP = Instantiate(impactParticle, transform.position, transform.rotation) as GameObject; // Spawns impact effect    
            Destroy(impactP, 3.0f);
            ObjectPool.Instance.Push(gameObject);
            if (collision.CompareTag("Enemy")) {
                Attack(collision);
                collision.GetComponent<StatusRecord>().StopLittle();

                //击退
                //BeatBack(collision);
            }
        }
    }
    virtual public void Attack(Collider2D collision)
    {

    }
    
    //virtual public void BeatBack(Collider2D collision)
    //{

    //}

    IEnumerator RecycleObj(float time)
    {
        yield return new WaitForSeconds(time);
        ObjectPool.Instance.Push(gameObject);
    }
}


