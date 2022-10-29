using UnityEngine;
using Random = UnityEngine.Random;

namespace YoukyController
{
    /// <summary>
    /// 角色动画控制
    /// </summary>
    public class SPlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator anim;
        [SerializeField] private AudioSource source;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private ParticleSystem jumpParticles;
        [SerializeField] private ParticleSystem launchParticles;
        [SerializeField] private ParticleSystem moveParticles, landParticles;
        [SerializeField] private GameObject jumpTail;
        [SerializeField] private AudioClip[] footsteps;
        [SerializeField] private float maxTilt = .1f;
        [SerializeField] private float tiltSpeed = 1;
        [SerializeField, Range(1f, 3f)] private float maxIdleSpeed = 2;
        [SerializeField] private float maxParticleFallSpeed = -40;

        private SIPlayerController playerCon;
        private bool playerGrounded;
        private ParticleSystem.MinMaxGradient currentGradient;
        private Vector2 _movement;
        private Vector2 lastMousePos;

        void Awake() => playerCon = GetComponentInParent<SIPlayerController>();

        void LateUpdate()
        {
            if (playerCon == null) return;
            bool lookRight = playerCon.Input.mousePos.x > transform.position.x;
            // Flipif(playerCon.Input.mousePos.x > transform.position.x)
            if ((lastMousePos - playerCon.Input.mousePos).magnitude > 0.3 || playerCon.AttackingThisFrame)
            {
                transform.localScale = new Vector3(lookRight ? 1 : -1, 1, 1);
            }
            if (!playerCon.AttackingThisFrame)
            {
                
                if (playerCon.Input.X != 0)
                {
                    lookRight = playerCon.Input.X > 0;
                    transform.localScale = new Vector3(lookRight ? 1 : -1, 1, 1);
                }

            }else Debug.Log("skip");
            lastMousePos = playerCon.Input.mousePos;

            // 跑步的身体倾斜
            var targetRotVector = new Vector3(0, 0, Mathf.Lerp(-maxTilt, maxTilt, Mathf.InverseLerp(-1, 1, (lookRight ? 1 : -1) * Mathf.Abs(playerCon.Input.X))));
            anim.transform.rotation = Quaternion.RotateTowards(anim.transform.rotation, Quaternion.Euler(targetRotVector), tiltSpeed * Time.deltaTime);


            // 跑步
            anim.SetFloat(IdleSpeedKey, Mathf.Lerp(1, maxIdleSpeed, Mathf.Abs(playerCon.Input.X)));
            if (Mathf.Abs(playerCon.Input.X) < 0.1f) anim.SetBool("isRun", false);
            else anim.SetBool("isRun", true);

            // 降落
            if (playerCon.LandingThisFrame)
            {
                anim.SetTrigger(GroundedKey);
                source.PlayOneShot(footsteps[Random.Range(0, footsteps.Length)]);
                jumpTail.SetActive(false);

            }

            // 跳跃
            if (playerCon.JumpingThisFrame)
            {
                anim.SetTrigger(JumpKey);
                anim.ResetTrigger(GroundedKey);

                // 只还在地面那时
                if (playerCon.Grounded)
                {
                    SetColor(jumpParticles);
                    SetColor(launchParticles);
                    jumpParticles.Play();
                    jumpTail.SetActive(true);

                }
            }


            if (!playerGrounded && playerCon.Grounded)
            {
                playerGrounded = true;
                moveParticles.Play();
                landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, maxParticleFallSpeed, _movement.y);
                SetColor(landParticles);
                landParticles.Play();
            }
            else if (playerGrounded && !playerCon.Grounded)
            {
                playerGrounded = false;
                moveParticles.Stop();
            }

            if (playerCon.AttackingThisFrame)
            {
                anim.SetBool("Attack", true);
            }
            if (playerCon.StopAttackingThisFrame)
            {
                anim.SetBool("Attack", false);
            }


            var groundHit = Physics2D.Raycast(transform.position, Vector3.down, 2, groundMask);
            if (groundHit && groundHit.transform.TryGetComponent(out SpriteRenderer r))
            {
                currentGradient = new ParticleSystem.MinMaxGradient(r.color * 0.9f, r.color * 1.2f);
                SetColor(moveParticles);
            }

            _movement = playerCon.RawMovement;

        }

        private void OnDisable()
        {
            moveParticles.Stop();
        }

        private void OnEnable()
        {
            moveParticles.Play();
        }

        void SetColor(ParticleSystem ps)
        {
            var main = ps.main;
            main.startColor = currentGradient;
        }

        #region Animation Keys

        private static readonly int GroundedKey = Animator.StringToHash("Grounded");
        private static readonly int IdleSpeedKey = Animator.StringToHash("IdleSpeed");
        private static readonly int JumpKey = Animator.StringToHash("Jump");

        #endregion
    }
}