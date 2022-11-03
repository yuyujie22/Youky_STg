using UnityEngine;
using Pathfinding;
using System;

public class EnemyAI : MonoBehaviour
{
    public const float trackTime = 15;
    //是否可以看到玩家
    public bool findPlayer;
    // 感知范围
    public float radius = 10;
    // 激活时间
    public float curTrackTime;
    // 最大速度
    public float maxSpeed = 2;
    // 更新动画器的速度阈值
    public float speedLimit = 0.01f;
    // 是否AI行动
    public bool useAI;
    public Animator anim;
    public Transform sprite;
    public Transform[] groundCheck;
    public float jumpXForce = 1;
    public float jumpYForce = 5;
    // 当前要去的点下标
    Transform player;
    // 路径
    Rigidbody2D rb;
    // 每隔一段时间刷新玩家的位置
    Vector2 target;
    int layer;
    float jumpTimer;
    bool isRight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();


    }
    private void Start()
    {
        //防止检测到enemy
        layer = ~(1 << LayerMask.NameToLayer("Enemy"));
        useAI = false;
 

    }

    public bool isGround;
    //bool isJump;
    private void Update()
    {
        float dt = Time.deltaTime;
        //累计时间
        jumpTimer += dt;
        RaycastHit2D hitInfo;
        //是否到地面 用三个groundCheck来检测 
        for (int i = 0; i < 3; i++)
        {
            hitInfo = Physics2D.Linecast(transform.position, groundCheck[i].position, 1 << LayerMask.NameToLayer("Ground"));
            isGround = hitInfo;
            if (isGround) break;
        }
            
     

    }
    private void FixedUpdate()
    {
        if (player != null)
        {
            if (Time.frameCount % 10 == 0)
                target = player.position;
            if (Time.frameCount % 15 == 0)
                findPlayer = Tools.PerceiveObject(transform.position, player.gameObject, radius, layer);
        }
        else if(GameManager.Instance != null && GameManager.Instance.player != null){
            player = GameManager.Instance.player;
        }
        if (!isGround)
        {
            anim.SetBool("isJump", true);
        }
        else
        {
            anim.SetBool("isJump", false);
        }

        // 找到并向玩家移动，刷新搜索时间
        if (findPlayer)
        {
            curTrackTime = trackTime;
            // 结束Idle
            idleTime = 0;
        }
        // 否则触发自动寻路
        if (idleTime > 0 && curTrackTime < 0.1)
        {
            MoveRandom();
            Debug.Log("随机走");
        }
        if (curTrackTime > 0)
        {
           
                prePos = transform.position;
         
                MoveToTarget();
            Debug.Log("往角色走");


            CheckIsStatic();
            curTrackTime -= Time.deltaTime;
            if (curTrackTime <= 0)
            {
                Debug.Log("追逐时间结束，未追踪到player");
            }
        }
        else
        {
            if (idleTime <= 0)
            {
                randomV = Tools.RandomDirection();
                randomV.y = 0;
                idleTime = timeLen;
            }
        }

    }


    void MoveToTarget()
    {
        if (!isGround) return;
        Debug.Log("MoveTarget");
        Vector2 vector = (target - (Vector2)transform.position).x > 0 ? Vector2.right : Vector2.left;
        Vector2 delta = Time.deltaTime * vector * maxSpeed;
        RefreshAnimation(delta);
        Vector2 pos = (Vector2)transform.position + delta;
        pos.y = transform.position.y;
        if(anim.GetBool("isRun"))
            rb.MovePosition(pos);
    }

    Vector2 prePos = new Vector2(-1e9f, -1e9f);
    Vector2 randomV;
    public float idleTime;
    float timeLen = 1f;
    public Vector2 vector;


    private void Jump()
    {
        //Debug.Log("Jump");
        Vector2 jumpForce = new Vector2(jumpXForce, jumpYForce);
        if (isRight)
        {
            rb.AddForce(jumpForce, ForceMode2D.Impulse);

        }
    }

 

    void MoveRandom()
    {
        anim.SetBool("isRun", false);
        //Vector2 delta = Time.deltaTime * randomV * maxSpeed / 2;
        //RefreshAnimation(delta);
        //delta.y = 0;
        //Vector2 pos = (Vector2)transform.position + delta;
        //rb.MovePosition(pos);

        idleTime -= Time.deltaTime;
    }


    void CheckIsStatic()
    {
        if (Time.frameCount % 20 == 19 && Vector2.Distance(prePos, transform.position) < 0.1f)
        {

            //Debug.Log("检测静止");
            //randomV = Tools.RandomDirection();
            //randomV.y = 0;
            //Jump();
            idleTime = timeLen;
        }
    }



    void RefreshAnimation(Vector2 vector)
    {

        if (Mathf.Abs(transform.position.x - player.position.x) < 0.1)
        {
            anim.SetBool("isRun", false);
            return;
        }
        //Debug.Log((Mathf.Abs(transform.position.x - player.position.x )));
        if (Time.frameCount % 10 == 0)
        {
            if (vector.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
            
        if (isGround && curTrackTime > 0)
            anim.SetBool("isRun", true);
    }


}


