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
    private Rigidbody2D rb;
    [Header("不存在collider时候的半径")]
    public float colliderRadius = 1f;
    [Range(0f, 1f)] // This is an offset that moves the impact effect slightly away from the point of impact to reduce clipping of the impact effect
    public float collideOffset = 0.15f;
    CircleCollider2D cirCol;
    public float speed = 5.0f;
    public float destroyTime = 1.0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cirCol = GetComponent<CircleCollider2D>();


    }

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
            Debug.Log("Boom" + collision.name);
            GameObject impactP = Instantiate(impactParticle, transform.position, transform.rotation) as GameObject; // Spawns impact effect    
            Destroy(impactP, 3.0f);
            ObjectPool.Instance.Push(gameObject);
        }
    }

    IEnumerator RecycleObj(float time)
    {
        yield return new WaitForSeconds(time);
        ObjectPool.Instance.Push(gameObject);
    }
}


