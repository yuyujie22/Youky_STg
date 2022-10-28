using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;
    public float speed = 5.0f;
    public float destroyTime = 1.0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetSpeed(Vector2 direction)
    {
        rb.velocity = direction * speed;
        StartCoroutine(DestroyObj(destroyTime));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            //Destroy(gameObject);
            ObjectPool.Instance.Push(gameObject);
        }
    }

    IEnumerator DestroyObj(float time)
    {
        yield return new WaitForSeconds(time);
        //Destroy(gameObject);
        ObjectPool.Instance.Push(gameObject);
    }
}


