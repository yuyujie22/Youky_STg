using UnityEngine;
using Random = UnityEngine.Random;

namespace YoukyController
{
    /// <summary>
    /// 角色动画控制
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator anim;
        [SerializeField] private AudioSource source;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private ParticleSystem jumpParticles, launchParticles;
        [SerializeField] private ParticleSystem moveParticles, landParticles;
        [SerializeField] private AudioClip[] footsteps;
        [SerializeField] private float maxTilt = .1f;
        [SerializeField] private float tiltSpeed = 1;
        [SerializeField, Range(1f, 3f)] private float maxIdleSpeed = 2;
        [SerializeField] private float maxParticleFallSpeed = -40;

        private IPlayerController playerCon;
        private bool playerGrounded;
        private ParticleSystem.MinMaxGradient currentGradient;
        private Vector2 _movement;

        void Awake() => playerCon = GetComponentInParent<IPlayerController>();

        void Update()
        {
            if (playerCon == null) return;

            // Flip the sprite
            if (playerCon.Input.X != 0) transform.localScale = new Vector3(playerCon.Input.X > 0 ? 1 : -1, 1, 1);

            // Lean while running
            var targetRotVector = new Vector3(0, 0, Mathf.Lerp(-maxTilt, maxTilt, Mathf.InverseLerp(-1, 1, playerCon.Input.X)));
            anim.transform.rotation = Quaternion.RotateTowards(anim.transform.rotation, Quaternion.Euler(targetRotVector), tiltSpeed * Time.deltaTime);
            if (Mathf.Abs(playerCon.Input.X) < 0.1f) anim.SetBool("isRun", false);
            else anim.SetBool("isRun", true);

            // Speed up idle while running
            anim.SetFloat(IdleSpeedKey, Mathf.Lerp(1, maxIdleSpeed, Mathf.Abs(playerCon.Input.X)));

            // Splat
            if (playerCon.LandingThisFrame)
            {
                anim.SetTrigger(GroundedKey);
                source.PlayOneShot(footsteps[Random.Range(0, footsteps.Length)]);
            }

            // Jump effects
            if (playerCon.JumpingThisFrame)
            {
                anim.SetTrigger(JumpKey);
                anim.ResetTrigger(GroundedKey);

                // Only play particles when grounded (avoid coyote)
                if (playerCon.Grounded)
                {
                    SetColor(jumpParticles);
                    SetColor(launchParticles);
                    jumpParticles.Play();
                }
            }

            // Play landing effects and begin ground movement effects
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

            // Detect ground color
            var groundHit = Physics2D.Raycast(transform.position, Vector3.down, 2, groundMask);
            if (groundHit && groundHit.transform.TryGetComponent(out SpriteRenderer r))
            {
                currentGradient = new ParticleSystem.MinMaxGradient(r.color * 0.9f, r.color * 1.2f);
                SetColor(moveParticles);
            }

            _movement = playerCon.RawMovement; // Previous frame movement is more valuable
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