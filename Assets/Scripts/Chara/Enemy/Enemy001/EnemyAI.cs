using UnityEngine;
using Pathfinding;

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
    public Transform groundCheck;
    // 当前要去的点下标
    int nextP = 0;
    // 路径
    Path path;
    Seeker seeker;
    AIPath aiPath;
    Rigidbody2D rb;
    // 每隔一段时间刷新玩家的位置
    Vector2 target;
    int layer;
    float jumpTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
        aiPath = GetComponent<AIPath>();
    }
    private void Start()
    {
        //防止检测到enemy
        layer = ~(1 << LayerMask.NameToLayer("Enemy"));
        useAI = false;
        aiPath.enabled = false;

    }

    public bool isGround;
    //bool isJump;
    private void Update()
    {
        float dt = Time.deltaTime;
        //累计时间
        jumpTimer += dt;
        //是否到地面 用三个groundCheck来检测 

        RaycastHit2D hitInfo = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));
        Debug.DrawLine(transform.position, groundCheck.position);
        if (hitInfo)
        {
            Debug.Log("hit: " + hitInfo.collider.gameObject);
            isGround = true;
        }
        else
        {
            isGround = false;
        }

    }
    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            if (Time.frameCount % 10 == 0)
                target = GameManager.Instance.player.position;
            if (Time.frameCount % 15 == 0)
                findPlayer = Tools.PerceiveObject(transform.position, GameManager.Instance.player.gameObject, radius, layer);
            if (Time.frameCount % 30 == 0)
                aiPath.enabled = useAI;
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
            EndAI("随机走");
        }
        if (curTrackTime > 0)
        {
            if (Time.frameCount % 20 == 0)
                prePos = transform.position;
            else if (findPlayer)
            {
                MoveToTarget();
                EndAI("往角色走");
            }
            else
            {
                if (!useAI) StartAI();
                Debug.Log("MOVE AI");
                MoveAI();
            }
            CheckIsStatic();
            curTrackTime -= Time.deltaTime;
            if (curTrackTime <= 0)
            {
                EndAI("追逐时间结束，未追踪到player");
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
        Debug.Log("MoveTarget");
        Vector2 vector = (target - (Vector2)transform.position).normalized;
        Vector2 delta = Time.deltaTime * vector * maxSpeed;
        RefreshAnimation(delta);
        Vector2 pos = (Vector2)transform.position + delta;
        pos.y = transform.position.y;
        rb.MovePosition(pos);
    }

    Vector2 prePos = new Vector2(-1e9f, -1e9f);
    Vector2 randomV;
    public float idleTime;
    float timeLen = 1f;


    void MoveAI()
    {
        if (path == null || !useAI)
        {
            return;
        }
        
        //// 获取向量
        //Vector2 vector = (path.vectorPath[nextP] - transform.position);
        //if (vector.magnitude > speedLimit)
        //{
        //    Vector2 delta = Time.deltaTime * vector.normalized * maxSpeed;
        //    Vector2 pos = (Vector2)transform.position + delta;
        //    RefreshAnimation(delta);
        //    rb.MovePosition(pos);
        //    Debug.Log("11");
        //    //由于使用的是移动而不是施加力，所以可能出现超过的情况
        //    //第一种处理办法是修改移动向量的长度使得当好落在目标点
        //    //      但是这种办法会导致移动过程速度时快时慢，镜头移动不流畅
        //    // 第二种办法是循环检测下一个目标点是否在当前移动后，已经越过了
        //    while (delta.magnitude > Vector2.Distance(transform.position, path.vectorPath[nextP]))
        //    {
        //        Debug.Log("22");
        //        nextP++;
        //        if (nextP == path.vectorPath.Count)
        //        {
        //            EndAI("已到达终点");
        //            break;
        //        }
        //    }
        //}
        //else
        //{
        //    nextP++;
        //    if (nextP == path.vectorPath.Count)
        //    {
        //        EndAI("已到达终点");
        //    }
        //}
    }



    void MoveRandom()
    {
        Vector2 delta = Time.deltaTime * randomV * maxSpeed / 2;
        RefreshAnimation(delta);
        delta.y = 0;
        Vector2 pos = (Vector2)transform.position + delta;
        rb.MovePosition(pos);

        idleTime -= Time.deltaTime;
    }


    void CheckIsStatic()
    {
        if (Time.frameCount % 20 == 19 && Vector2.Distance(prePos, transform.position) < 0.01f)
        {
            EndAI("检测静止");
            randomV = Tools.RandomDirection();
            randomV.y = 0;
            idleTime = timeLen;
        }
    }

    public void StartAI()
    {
        path = null;
        useAI = true;
        SeekPath();
    }
    public void EndAI(string info)
    {
        useAI = false;
        path = null;
        Debug.Log("END AI: " + info);
    }
    public void EndAI()
    {
        useAI = false;
        path = null;
    }


    void SeekPath()
    {
        if (useAI && path != null && !seeker.IsDone()) return;
        seeker.StartPath(transform.position, target, OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            Debug.Log("Success");
            path = p;
            nextP = 0;
        }
        else
        {
            EndAI("");
        }
    }


    void RefreshAnimation(Vector2 vector)
    {

        if (Mathf.Abs(vector.x) < 0.01) return;
        if (vector.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        if (isGround && trackTime > 0)
            anim.SetBool("isRun", true);
    }


}


