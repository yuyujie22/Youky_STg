using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YoukyController
{
    /// <summary>
    /// 不使用其它多余组件实现多种移动
    /// </summary>
    public class SPlayerController : MonoBehaviour, SIPlayerController
    {
        // 先实现extras的接口
        public Vector3 Velocity { get; private set; }
        public FrameInput Input { get; private set; }
        public bool JumpingThisFrame { get; private set; }
        public bool LandingThisFrame { get; private set; }
        public bool AttackingThisFrame { get; private set; }
        public bool StopAttackingThisFrame { get; private set; }
        public Vector3 RawMovement { get; private set; }
        public bool Grounded => colDown;
        [Header("子弹种类和按键")]
        //各种子弹的预制体
        public GameObject[] bulletPre;
        //各种子弹的使用按键
        public KeyCode[] bulletKeyCodes;
        // 弹幕间隔时间
        public float intervalTime = 0.1f;
        // 弹幕计时
        private float invokeTime;
        private Rigidbody2D rb;
        // start方法中赋初值
        //下面也可以这样写void Awake() => Invoke("Activate", 0.5f);
        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            Invoke(nameof(Activate), 0.5f);
        }
        void Start()
        {
            Invoke(nameof(Register), 0.7f);
            invokeTime = intervalTime;
        }

        private Vector3 lastPosition;
        private float currentHorizontalSpeed, currentVerticalSpeed;

        // 为了防止游戏运行时碰撞体还没有完成生成而延迟0.5s使用Awake（）函数
        private bool active;
        void Activate() => active = true;
        private void Register()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.RegisterPlayer(transform);
        }

        private void Update()
        {
            if (!active) return;
            // 按照每帧的位置计算速度
            Velocity = (transform.position - lastPosition) / Time.deltaTime;
            lastPosition = transform.position;
            if (!Grounded)
            {
                
            }

            GatherInput();
            RunCollisionChecks();
            CalculateWalk(); // Horizontal movement
            CalculateJumpApex(); // Affects fall speed, so calculate before gravity
            CalculateGravity(); // Vertical movement
            CalculateJump(); // Possibly overrides vertical

            MoveCharacter(); // Actually perform the axis movement

            Attack();

        }


        #region Gather Input

        private void GatherInput()
        {
            Input = new FrameInput
            {
                //按下抬起 起跳键
                JumpDown = UnityEngine.Input.GetButtonDown("Jump"),
                JumpUp = UnityEngine.Input.GetButtonUp("Jump"),
                X = UnityEngine.Input.GetAxisRaw("Horizontal"),
                mousePos = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition)
            };
            //记录当前跳跃的时间点
            if (Input.JumpDown)
            {
                lastJumpPressed = Time.time;
            }
        }

        #endregion

        #region Collisions

        [Header("COLLISION")] [SerializeField] private Bounds characterBounds;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private int detectorCount = 3;
        [SerializeField] private float detectionRayLength = 0.1f;
        [SerializeField] [Range(0.1f, 0.3f)] private float rayBuffer = 0.1f; // Prevents side detectors hitting the ground

        private RayRange raysUp, raysRight, raysDown, raysLeft;
        private bool colUp, colRight, colDown, colLeft;

        private float timeLeftGrounded;

        // 射线检测实现物理分析
        private void RunCollisionChecks()
        {
            // 定义数据存储变量
            CalculateRayRanged();

            // 地面
            LandingThisFrame = false;
            var groundedCheck = RunDetection(raysDown);
            // 跳跃离开地面的一帧
            if (colDown && !groundedCheck) timeLeftGrounded = Time.time; // Only trigger when first leaving
            else if (!colDown && groundedCheck)
            {
                // 跳跃回到地面的一帧
                coyoteUsable = true;
                LandingThisFrame = true;
            }
            // 同步
            colDown = groundedCheck;

            // 其他方向
            colUp = RunDetection(raysUp);
            colLeft = RunDetection(raysLeft);
            colRight = RunDetection(raysRight);


        }
        //获取数据
        private bool RunDetection(RayRange range)
        {
            return EvaluateRayPositions(range).Any(point => Physics2D.Raycast(point, range.Dir, detectionRayLength, groundLayer));
        }
        private void CalculateRayRanged()
        {
            var b = new Bounds(transform.position, characterBounds.size);

            raysDown = new RayRange(b.min.x + rayBuffer, b.min.y, b.max.x - rayBuffer, b.min.y, Vector2.down);
            raysUp = new RayRange(b.min.x + rayBuffer, b.max.y, b.max.x - rayBuffer, b.max.y, Vector2.up);
            raysLeft = new RayRange(b.min.x, b.min.y + rayBuffer, b.min.x, b.max.y - rayBuffer, Vector2.left);
            raysRight = new RayRange(b.max.x, b.min.y + rayBuffer, b.max.x, b.max.y - rayBuffer, Vector2.right);
        }


        private IEnumerable<Vector2> EvaluateRayPositions(RayRange range)
        {
            for (var i = 0; i < detectorCount; i++)
            {
                var t = (float)i / (detectorCount - 1);
                yield return Vector2.Lerp(range.Start, range.End, t);
            }
        }

        private void OnDrawGizmos()
        {
            // Bounds
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + characterBounds.center, characterBounds.size);

            // Rays
            if (!Application.isPlaying)
            {
                CalculateRayRanged();
                Gizmos.color = Color.blue;
                foreach (var range in new List<RayRange> { raysUp, raysRight, raysDown, raysLeft })
                {
                    foreach (var point in EvaluateRayPositions(range))
                    {
                        Gizmos.DrawRay(point, range.Dir * detectionRayLength);
                    }
                }
            }

            if (!Application.isPlaying) return;

            // Draw 未来的pos
            Gizmos.color = Color.red;
            var move = new Vector3(currentHorizontalSpeed, currentVerticalSpeed) * Time.deltaTime;
            Gizmos.DrawWireCube(transform.position + move, characterBounds.size);
        }

        #endregion


        #region Walk

        [Header("WALKING")] [SerializeField] private float acceleration = 90;
        [SerializeField] private float moveClamp = 13;
        [SerializeField] private float deAcceleration = 60f;
        [SerializeField] private float apexBonus = 2;

        //X轴输入
        private void CalculateWalk()
        {
            if (Input.X != 0)
            {
                currentHorizontalSpeed += Input.X * acceleration * Time.deltaTime;
                //控制最大速度
                currentHorizontalSpeed = Mathf.Clamp(currentHorizontalSpeed, -moveClamp, moveClamp);

                //跳跃时速度调整。计算0到最高点的比例值，在最高点时apexPoint为1
                //Mathf.Sign() 如果是0或正数就返回1，负数则返回-1
                var _apexBonus = Mathf.Sign(Input.X) * apexBonus * apexPoint;
                currentHorizontalSpeed += _apexBonus * Time.deltaTime;
            }
            else
            {
                //没有输入X的速度->减速
                currentHorizontalSpeed = Mathf.MoveTowards(currentHorizontalSpeed, 0, deAcceleration * Time.deltaTime);
            }
            //撞墙
            if (currentHorizontalSpeed > 0 && colRight || currentHorizontalSpeed < 0 && colLeft)
            {
                currentHorizontalSpeed = 0;
            }
        }

        #endregion

        #region Gravity

        [Header("GRAVITY")] [SerializeField] private float fallClamp = -40f;
        [SerializeField] private float minFallSpeed = 80f;
        [SerializeField] private float maxFallSpeed = 120f;
        private float fallSpeed;

        private void CalculateGravity()
        {
            //地面上
            if (colDown)
            {
                //重置二段跳
                canDoubleJump = true;
                if (currentVerticalSpeed < 0) currentVerticalSpeed = 0;
            }
            else
            {
                // 提前送键的小跳，加速下落
                var fallSpeed = endedJumpEarly && currentVerticalSpeed > 0 ? this.fallSpeed * jumpEndEarlyGravityModifier : this.fallSpeed;

                // 正常
                currentVerticalSpeed -= fallSpeed * Time.deltaTime;

                // 限制速度
                if (currentVerticalSpeed < fallClamp) currentVerticalSpeed = fallClamp;
            }
        }

        #endregion

        #region Jump

        [Header("JUMPING")] [SerializeField] private float jumpHeight = 30;
        [SerializeField] private float jumpApexThreshold = 10f;
        //二段跳的最长时间间隔
        [SerializeField] private float coyoteTimeThreshold = 0.1f;
        [SerializeField] private float jumpBuffer = 0.1f;
        //如果endedJumpEarly为true，下落重力会乘jumpEndEarlyGravityModifier
        [SerializeField] private float jumpEndEarlyGravityModifier = 3;
        [SerializeField] private bool doubleJumpUsable = false;
        //一次在空中最多跳两次，到达地面重置
        private bool canDoubleJump = true;
        private bool coyoteUsable;
        private bool endedJumpEarly = true;
        private float apexPoint;
        private float lastJumpPressed;
        private bool CanUseCoyote => coyoteUsable && !colDown && timeLeftGrounded + coyoteTimeThreshold > Time.time;
        //lastJumpPressed + jumpBuffer > Time.time  （按下跳跃键后的jumpBuffer秒内）
        private bool HasBufferedJump => colDown && lastJumpPressed + jumpBuffer > Time.time;

        private void CalculateJumpApex()
        {
            if (!colDown)
            {
                //跳跃时速度调整。计算0到最高点的比例值，在最高点时apexPoint为1
                apexPoint = Mathf.InverseLerp(jumpApexThreshold, 0, Mathf.Abs(Velocity.y));
                fallSpeed = Mathf.Lerp(minFallSpeed, maxFallSpeed, apexPoint);
            }
            else
            {
                apexPoint = 0;
            }
        }

        private void CalculateJump()
        {
            // 在地面或者二段跳时间限制内 || 在跳跃缓冲内
            if (Input.JumpDown && CanUseCoyote || HasBufferedJump)
            {
                currentVerticalSpeed = jumpHeight;
                endedJumpEarly = false;
                coyoteUsable = false;
                timeLeftGrounded = float.MinValue;
                JumpingThisFrame = true;
            }
            else
            {
                JumpingThisFrame = false;
            }

            // 按键松开-> 加快下落（相当于小跳）
            if (!colDown && Input.JumpUp && !endedJumpEarly && Velocity.y > 0)
            {
                endedJumpEarly = true;
            }
            //二段跳
            if (doubleJumpUsable && canDoubleJump && endedJumpEarly && Input.JumpDown)
            {
                currentVerticalSpeed = jumpHeight;
                endedJumpEarly = false;
                coyoteUsable = false;
                timeLeftGrounded = float.MinValue;
                JumpingThisFrame = true;
                canDoubleJump = false;
            }

            if (colUp)
            {
                if (currentVerticalSpeed > 0) currentVerticalSpeed = 0;
            }
        }

        #endregion

        #region Move

        [Header("MOVE")]
        [SerializeField, Tooltip("提高该值会以性能为代价，增加碰撞检测精度")]
        private int freeColliderIterations = 10;

        // 我们在移动检测边界，以避免将来发生碰撞
        private void MoveCharacter()
        {
            var pos = transform.position;
            RawMovement = new Vector3(currentHorizontalSpeed, currentVerticalSpeed);
            var move = RawMovement * Time.deltaTime;
            var furthestPoint = pos + move;

            // 检查之后的会移动的 最远！ 位置。如果没有命中，则移动，且不进行额外检查
            var hit = Physics2D.OverlapBox(furthestPoint, characterBounds.size, 0, groundLayer);
            if (!hit)
            {
                //正常移动
                transform.position += move;
                return;
            }

            // 否则远离当前位置；看看我们能移动到什么最接近的位置
            var positionToMoveTo = transform.position;
            for (int i = 1; i < freeColliderIterations; i++)
            {
                // 不断递增检测 除了最远的那个点（我们已经检测过了）
                var t = (float)i / freeColliderIterations;
                var posToTry = Vector2.Lerp(pos, furthestPoint, t);

                if (Physics2D.OverlapBox(posToTry, characterBounds.size, 0, groundLayer))
                {
                    transform.position = positionToMoveTo;

                    // 我们落在一个角落或撞到了一个壁上。轻推玩家
                    if (i == 1)
                    {
                        if (currentVerticalSpeed < 0) currentVerticalSpeed = 0;
                        var dir = transform.position - hit.transform.position;
                        transform.position += dir.normalized * move.magnitude;
                    }

                    return;
                }

                positionToMoveTo = posToTry;
            }
        }

        #endregion

        #region Attack
        [SerializeField, Tooltip("提高该值会以性能为代价，增加碰撞检测精度")]
        private Transform attackPoint;
        int flag = -1;
        private void Attack()
        {
            //从player手持的攻击种类数组获取相关信息

            if (flag == 1 || flag == -1)
            {
                magicAttack(1);
            }
            if (flag == 0 || flag == -1)
            {
                magicAttack(0);
            }
            if (flag == -1)
            {
                StopAttackingThisFrame = true;
            }
        }


        private void magicAttack(int i)
        {
            Debug.Log("StopAttackingThisFrame: " + StopAttackingThisFrame);
            if (StopAttackingThisFrame) StopAttackingThisFrame = false;
            if (AttackingThisFrame)
            {
                Vector2 pos = transform.position;
                Vector2 direction = (Input.mousePos - pos).normalized;
                //GameObject bullet = Instantiate(bulletPre, pos, Quaternion.identity);
                GameObject bullet = ObjectPool.Instance.Get(bulletPre[i]);

                if (attackPoint == null)
                    bullet.transform.position = transform.position;
                else
                    bullet.transform.position = attackPoint.position;
                bullet.GetComponent<Bullet>().SetSpeed(direction);
                //反作用力

                rb.AddForce(bulletPre[i].GetComponent<Bullet>().reactForce * ((direction.x > 0) ? Vector2.left : Vector2.right), ForceMode2D.Impulse);
                //currentVerticalSpeed = bulletPre[i].GetComponent<Bullet>().reactForce * ((direction.x > 0) ? -1 : 1);

                AttackingThisFrame = false;
            }
            if (UnityEngine.Input.GetKey(bulletKeyCodes[i]))
            {
                flag = i;
                // 按键按下时进行计时
                invokeTime += Time.deltaTime;
                // 间隔时间大于自定义时间才执行
                if (invokeTime - intervalTime > 0)
                {
                    Debug.Log("click");
                    // 进行实例化弹幕、子弹等操作
                    //Instantiate(*****);
                    AttackingThisFrame = true;
                    // 实例化一次后计时归零
                    invokeTime = 0;
                }
            }

            if (UnityEngine.Input.GetKeyUp(bulletKeyCodes[i]))
            {
                Debug.Log("Up :" + bulletKeyCodes[i]);
                flag = -1;
                AttackingThisFrame = false;
                StopAttackingThisFrame = true;
                invokeTime = intervalTime;
            }
        }

        #endregion

    }
}