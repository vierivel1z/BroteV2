using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 4f;

    [Header("Animation clips (assign when ready)")]
    [SerializeField] private AnimationClip idleFront;
    [SerializeField] private AnimationClip idleBack;
    [SerializeField] private AnimationClip idleLeft;
    [SerializeField] private AnimationClip idleRight;
    [SerializeField] private AnimationClip walkFront;
    [SerializeField] private AnimationClip walkBack;
    [SerializeField] private AnimationClip walkLeft;
    [SerializeField] private AnimationClip walkRight;

    private Rigidbody2D body;
    private Animator animator;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.down;

    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int LastMoveX = Animator.StringToHash("LastMoveX");
    private static readonly int LastMoveY = Animator.StringToHash("LastMoveY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = Mathf.Max(0f, value);
    }

    public Vector2 LastMoveDirection => lastMoveDirection;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        ReadMovementInput();

        if (moveInput.sqrMagnitude > 0.0001f)
            lastMoveDirection = moveInput;

        animator.SetFloat(MoveX, moveInput.x);
        animator.SetFloat(MoveY, moveInput.y);
        animator.SetFloat(LastMoveX, lastMoveDirection.x);
        animator.SetFloat(LastMoveY, lastMoveDirection.y);
        animator.SetBool(IsMoving, moveInput.sqrMagnitude > 0.0001f);
    }

    private void FixedUpdate()
    {
        if(NurseryManagementPanel.IsOpen) return;
        body.MovePosition(body.position + moveInput * (moveSpeed * Time.fixedDeltaTime));
    }

    private void ReadMovementInput()
    {
        moveInput = Vector2.zero;
        if (NurseryManagementPanel.IsOpen) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveInput.y += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveInput.y -= 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveInput.x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveInput.x += 1f;

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
    }
}
