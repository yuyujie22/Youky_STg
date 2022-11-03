using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusRecord : MonoBehaviour
{
    public int HP = 5;
    public Frozen frozenComp;
    public Fired firedComp;
    public GameObject dieParticularPrefab;
    public float dieDestryTime = 10.0f;
    //private Material m_material;
    //private Color m_color = Color.white;
    Animator anim;
    private void Awake()
    {
        //m_material = GetComponent<Renderer>().material;
        anim = GetComponentInChildren<Animator>();
        
    }
    public bool isFrozen()
    {
        return frozenComp.isFrozen;
    }
    public bool isFired()
    {
        return firedComp.isFired;
    }
    private void Update()
    {
        //m_material.SetColor("_Color", Color.Lerp(m_material.GetColor("_Color"), m_color, Time.deltaTime));
        if (HP <= 0)
        {
            Die();
        }
        
    }

    public void Die()
    {
        
        
        GetComponent<EnemyAI>().enabled = false;
        anim.SetBool("isDie", true);
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, transform.rotation.y, -86));
        //transform.rotation = new Vector3(transform.rotation.x, transform.rotation.y ,-86);
        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(DieWait());
    }
    public void StopFire()
    {
        StartCoroutine(_StopFire());
    }
    IEnumerator _StopFire()
    {
        yield return new WaitForSeconds(0.3f);
        firedComp.curTime = -0.1f;
    }
    public void StopFrozen()
    {
        StartCoroutine(_StopFrozen());
    }
    IEnumerator _StopFrozen()
    {
        yield return new WaitForSeconds(0.3f);
        frozenComp.curTime = -0.1f;
    }
    public void StopLittle()
    {
        GetComponent<EnemyAI>().enabled = false;
        StartCoroutine(_StopLittle());
    }
    IEnumerator _StopLittle()
    {
        yield return new WaitForSeconds(0.3f);
        if (!frozenComp.isFrozen)
        {
            GetComponent<EnemyAI>().enabled = true;
        }
    }
    public void OnHit()
    {
        //m_color = Color.red;
        //StartCoroutine(ColorWait());
    }

   //IEnumerator ColorWait()
   // {
   //     yield return new WaitForSeconds(0.5f);
   //     m_color = Color.white;
   // }
    IEnumerator DieWait()
    {
        yield return new WaitForSeconds(dieDestryTime);
        
        if(UnityEngine.Random.Range(1, 10) <= 3)
        {
            GameObject impactP = Instantiate(dieParticularPrefab, transform.position, transform.rotation) as GameObject;
            Destroy(impactP, 3.0f);
        }
        GameManager.Instance.Spawnenemy();
        Destroy(gameObject);
    }


}



