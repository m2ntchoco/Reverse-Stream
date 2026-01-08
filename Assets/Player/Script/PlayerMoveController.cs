using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using Unity.VisualScripting;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMoveController : MonoBehaviour
{
    [Header("이동 & 액션")]
    [SerializeField] public float speed = 3f; 			 // 걷기 속도
    [SerializeField] private float jumpForce = 7.5f; 	 // 점프 힘

    [Header("중력 설정 (수동)")]
    [SerializeField] private float gravity = 20f; 		 // 기본 중력
    public const float origingravityMultiplier = 1.0f;
    public float currentGravityMultiplier { get; private set; } = origingravityMultiplier; 		 // 💡 [추가됨] 현재 적용 중인 중력 배율 (1.0f이 기본)

    [Header("물리 설정 (점프 궤적 튜닝)")]
    [Tooltip("하강 시 중력 가속도 배율")]
    [SerializeField] private float fallMultiplier = 2.5f;

    [Header("하향 점프 & 점프 스루")]
    [Tooltip("아래 + 점프로 바닥을 통과할 때 충돌 무시 시간")]
    [SerializeField] private float platformDropTime = 0.5f;
    [Tooltip("점프 직후 점프스루(위로 통과) 충돌 무시 시간")]
    [SerializeField] private float jumpThroughTime = 0.3f;

    [Header("점프 버그 방지")]
    [Tooltip("점프 직후, 점프 카운트를 바로 리셋하지 않는 쿨타임")]
    [SerializeField] private float jumpResetCooldown = 0.2f;

    private float lastJumpTime = -99f;

    public int GetFacingDir => facingDir;

    // ===== 상태 플래그 =====
    [SerializeField] public int jumpCount = 0;
    private const int maxJumps = 2;
    private int facingDir = 1;
    private float moveInput;
    private bool attackLocked = false;
    private bool isIgnoringPlatform = false;

    // 대쉬 관련 (DashSkill이 처리한다고 가정)
    public float dashDuration = 0.25f;
    public float dashSpeed = 3f;
    public float dashCooldown = 5.0f;
    public bool isjump = false; // 애니메이션용 플래그

    //private Coroutine animationSlowRoutine;
    private Coroutine stepRoutine;
    private Coroutine platformIgnoreRoutine;
    private Coroutine jumpRoutine;

    // ===== 참조 (컴포넌트니까 _ 붙임) =====
    private PlayerRef _ref;

    public float y = 0;
    private void Start()
    {
        _ref = GetComponent<PlayerRef>();
        if (_ref == null)
        {
            Debug.LogError("PlayerMoveController: PlayerRef 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        // AddForce 방식 사용 시, Unity 기본 중력을 끄고 스크립트에서 수동 중력 구현
        if (_ref._Rb != null)
        {
            _ref._Rb.gravityScale = 0f;
        }
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        HandlePhysicsAndMovement();
        y = _ref._Rb.linearVelocity.y; // linearVelocity는 Rigidbody2D에서 velocity로 사용
    }

    /// <summary>
    /// 수직 속도 계산 및 Rigidbody2D를 이용한 최종 이동 처리.
    /// </summary>
    public void HandlePhysicsAndMovement()
    {
        if (_ref == null || _ref._Rb == null) return;

        // 대시 중이면 이동/중력은 Dash에서 처리한다고 가정하고 스킵
        if (_ref._Dash != null && _ref._Dash.IsDashing) return;

        // StepForward 코루틴이 돌고 있으면 수동 이동 로직 무시 (공격 스텝 중)
        if (stepRoutine != null) return;

        float dt = Time.fixedDeltaTime;
        Vector2 currentVelocity = _ref._Rb.linearVelocity;
        float verticalVelocity = currentVelocity.y; // 현재 Y 속도를 가져옴

        // 1. 지면 체크
        bool isGrounded = false;
        if (_ref._Ground != null)
        {
            isGrounded = _ref._Ground.isGrounded;
        }

        if (isGrounded && !isIgnoringPlatform && verticalVelocity < 0f)
        {
            // 땅에 닿는 순간, 점프 쿨타임을 무시하고 즉시 리셋
            ResetJumpCount(true);
        }

        // 2. 중력/점프 Y속도 계산 (AddForce 방식을 위한 수동 중력 적용)
        if (isGrounded && verticalVelocity <= 0f)
        {
            // 바닥에 붙어 있을 때 약간의 음수값으로 접지 유지
            _ref._Rb.linearVelocity = new Vector2(currentVelocity.x, -0.01f);
        }
        else
        {
            // 🚨 [수정됨] 현재 설정된 중력 배율(currentGravityMultiplier)을 사용
            float gravityMultiplier = currentGravityMultiplier;

            if (verticalVelocity < 0f)
            {
                // 낙하 중
                gravityMultiplier *= fallMultiplier;
            }
            // else if (verticalVelocity > 0f && !Input.GetKey(KeyCode.Space))
            // {
            //     // 상승 중인데 스페이스 떼면 빠르게 낙하 전환
            //     gravityMultiplier = ascentMultiplier;
            // }

            // 🚨 [수정됨] 중력 배율이 0보다 클 때만 속도에 영향을 줍니다.
            if (gravityMultiplier > 0f)
            {
                // Rigidbody.velocity를 직접 조작하는 것이 더 정확한 제어를 제공함
                verticalVelocity -= gravity * gravityMultiplier * dt;
            }
        }

        // 점프 스루 (위로 관통)
        if (_ref._Ground != null && !string.IsNullOrEmpty(_ref._Ground.FloatGroundLayerName))
        {
            // 위로 상승하는 순간 + 아직 점프스루 코루틴 안 돌고 있을 때 한 번만
            if (verticalVelocity > 0f && platformIgnoreRoutine == null)
            {
                int floatGroundLayer = LayerMask.NameToLayer(_ref._Ground.FloatGroundLayerName);
                if (floatGroundLayer >= 0)
                {
                    StartPlatformIgnore(floatGroundLayer, jumpThroughTime);
                }
            }
        }

        // 3. 수평 속도 계산 (공격 락 걸리면 X 이동 0)
        float finalMoveInput = attackLocked ? 0f : moveInput;

        // Y 속도는 중력 계산 결과로 업데이트
        Vector2 vel = currentVelocity;
        vel.x = finalMoveInput * speed;
        vel.y = verticalVelocity;
        _ref._Rb.linearVelocity = vel; // velocity 대신 linearVelocity를 사용했던 원본 코드와의 통일성을 위해 주석 처리

        // 4. 바라보는 방향
        if (finalMoveInput != 0f)
        {
            facingDir = (int)Mathf.Sign(finalMoveInput);
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDir;
            transform.localScale = scale;
        }

        // 5. 애니메이션 싱크
        if (_ref._AnimSync != null)
        {
            _ref._AnimSync.AirSpeedY(_ref._Rb.linearVelocity.y);
            _ref._AnimSync.IsWalking(finalMoveInput != 0f);
        }
    }

    public void HandleInput()
    {
        if (_ref == null) return;

        // 대시 중에는 입력 처리 X
        if (_ref._Dash != null && _ref._Dash.IsDashing) return;

        // ─ 이동 입력 ─
        moveInput = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) moveInput = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow)) moveInput = -1f;

        bool isDownJumpInput = Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.Space);

        // ─ 대시 ─
        if (_ref._Dash != null && Input.GetKeyDown(KeyCode.D))
        {
            if (!_ref._Dash.IsDashing)
            {
                Vector2 dashDir = (moveInput != 0f)
                    ? new Vector2(moveInput, 0f)
                    : new Vector2(GetFacingDir, 0f);

                // 플랫폼 무시 상태가 남아 있으면 초기화
                if (platformIgnoreRoutine != null)
                {
                    StopCoroutine(platformIgnoreRoutine);
                    platformIgnoreRoutine = null;
                    if (_ref._Ground != null)
                    {
                        int floatGroundLayer = LayerMask.NameToLayer(_ref._Ground.FloatGroundLayerName);
                        if (floatGroundLayer >= 0)
                        {
                            Physics2D.IgnoreLayerCollision(gameObject.layer, floatGroundLayer, false);
                            _ref._Ground.ResumeFloatGroundLayer();
                        }
                    }
                }

                _ref._Dash.TryDash(dashDir);
                return;
            }
        }

        // ─ 하향 점프 (아래 + 점프) ─
        bool grounded = (_ref._Ground != null) && _ref._Ground.isGrounded;
        if (isDownJumpInput && grounded && _ref._Ground != null && _ref._Ground.isOnFloatGround)
        {
            int floatLayer = LayerMask.NameToLayer(_ref._Ground.FloatGroundLayerName);
            if (floatLayer >= 0)
            {
                StartPlatformIgnore(floatLayer, platformDropTime);
                return;
            }
        }

        // ─ 일반/이단 점프 ─
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            // AddForce 방식으로 변경
            // 기존의 점프 힘(jumpForce)을 속도 변화량으로 보고, AddForce(ForceMode2D.Impulse)를 사용합니다.
            // Rigidbody.velocity.y를 0으로 만들고 AddForce를 적용하여 일관된 점프 높이를 얻습니다.

            // 현재 Rigidbody2D.velocity.y가 양수이면 (점프 중이면) 그 값을 상쇄하고 AddForce 적용
            Vector2 currentVelocity = _ref._Rb.linearVelocity;
            if (currentVelocity.y > 0)
            {
                _ref._Rb.linearVelocity = new Vector2(currentVelocity.x, 0f);
            }

            // 점프 힘을 Impulse 모드로 적용하여 즉각적인 속도 변화를 줍니다.
            _ref._Rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);


            if (_ref._AnimSync != null && jumpCount >= 1)
            {
                _ref._AnimSync.Jump();
                _ref._AnimSync.JumpEffect();
            }
            else _ref._AnimSync.Jump();
            if (_ref._Ground != null) _ref._Ground.isGrounded = false;

            isjump = true;
            jumpCount++;
            lastJumpTime = Time.time;

            if (jumpRoutine != null) StopCoroutine(jumpRoutine);
            jumpRoutine = StartCoroutine(JumpRoutine());
        }
    }

    public void StartPlatformIgnore(int layerToIgnore, float duration)
    {
        Physics2D.IgnoreLayerCollision(gameObject.layer, layerToIgnore, true);

        isIgnoringPlatform = true;
        if (_ref._Ground != null)
        {
            _ref._Ground.IgnoreFloatGroundLayer();
        }

        if (platformIgnoreRoutine != null)
        {
            StopCoroutine(platformIgnoreRoutine);
        }
        platformIgnoreRoutine = StartCoroutine(PlatformDropResetRoutine(layerToIgnore, duration));
    }

    /// <summary>
    /// 갈고리 등에서 강제로 점프 카운트 초기화 할 때 ignoreCooldown = true 사용.
    /// </summary>
    public void ResetJumpCount(bool ignoreCooldown = false)
    {
        if (!ignoreCooldown && Time.time < lastJumpTime + jumpResetCooldown)
        {
            return;
        }
        jumpCount = 0;
    }

    public void SetAttackLock(bool locked)
    {
        attackLocked = locked;
    }

    // 💡 [추가됨] 외부(Hook)에서 중력 배율을 설정하는 메서드
    public void SetGravityScale(float scale)
    {
        currentGravityMultiplier = scale;

        // 🚀 [추가됨] 중력이 0이 될 때 플레이어의 수직 속도를 0으로 만들어야 부자연스럽게 떨어지지 않습니다.
        if (scale == 0f && _ref._Rb != null)
        {
            Vector2 currentVelocity = _ref._Rb.linearVelocity;
            _ref._Rb.linearVelocity = new Vector2(currentVelocity.x, 0f);
        }
    }

    public void StepForward(float distance, float duration, AnimationCurve curve = null)
    {
        if (stepRoutine != null) StopCoroutine(stepRoutine);
        stepRoutine = StartCoroutine(StepRoutine(distance, duration, curve));
    }

    /// <summary>
    /// 공격 스텝용 슬라이드 이동.
    /// Y축은 고정, X축만 부드럽게 이동 (공중에서 쓰면 살짝 부자연스러울 수 있음)
    /// </summary>
    private IEnumerator StepRoutine(float distance, float duration, AnimationCurve curve)
    {
        if (_ref._Rb == null)
        {
            stepRoutine = null;
            yield break;
        }

        float signedDistance = distance * Mathf.Sign(GetFacingDir);
        Vector2 start = _ref._Rb.position;
        Vector2 target = start + new Vector2(signedDistance, 0f);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = duration > 0f ? Mathf.Clamp01(t / duration) : 1f;
            float eased = (curve != null) ? curve.Evaluate(lerp) : lerp;

            float newX = Mathf.Lerp(start.x, target.x, eased);
            Vector2 nextPos = new Vector2(newX, start.y); // Y는 시작 높이 유지
            _ref._Rb.MovePosition(nextPos);

            yield return new WaitForFixedUpdate();
        }

        _ref._Rb.MovePosition(target);
        stepRoutine = null;
    }

    // public IEnumerator RecoverAfterAnimationEnd(int stateHash)
    // {
    //     yield return null;
    //     animationSlowRoutine = null;
    // }

    public IEnumerator JumpRoutine()
    {
        yield return null;
        isjump = false;
        jumpRoutine = null;
    }

    private IEnumerator PlatformDropResetRoutine(int layerToIgnore, float duration)
    {
        yield return new WaitForSeconds(duration);

        Physics2D.IgnoreLayerCollision(gameObject.layer, layerToIgnore, false);

        if (_ref._Ground != null)
        {
            _ref._Ground.ResumeFloatGroundLayer();
        }
        isIgnoringPlatform = false;

        platformIgnoreRoutine = null;
    }
}