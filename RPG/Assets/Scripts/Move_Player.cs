using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class Move_Player : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Player movement speed.")]
    [Min(0f)]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Interact")]
    [Tooltip("Interact animation playback speed multiplier.")]
    [Min(0.1f)]
    [SerializeField] private float interactSpeedMultiplier = 1.8f;

    [Header("Animations")]
    [Tooltip("ScriptableObject holding all animation sets per item type.")]
    [SerializeField] private PlayerAnimations playerAnimations;

    private enum InputDirection { None, Up, Down, Left, Right }

    private Rigidbody2D      rb;
    private Animator         animator;
    private SpriteRenderer   spriteRenderer;
    private Vector2          moveInput;
    private Vector2          lastDirection   = Vector2.down;
    private InputDirection   activeDirection = InputDirection.None;

    private PlayableGraph           playableGraph;
    private AnimationClipPlayable   clipPlayable;
    private AnimationPlayableOutput playableOutput;
    private AnimationClip           currentClip;
    private bool                    graphReady;

    private float   interactTimer;
    private Vector2 knockbackVelocity;
    private float   knockbackTimer;
    private bool    movementLocked;

    private ItemAnimationType currentItemType = ItemAnimationType.None;

    public Vector2 FacingDirection => lastDirection;
    public bool    IsBlocking      => false;
    public bool    MovementLocked  => movementLocked;

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0.1f, speed);
    }

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;
        if (locked && rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    public void ApplyKnockback(Vector2 knockbackDir, float duration = 0.15f)
    {
        if (knockbackDir.sqrMagnitude > 0.01f)
        {
            knockbackVelocity = knockbackDir;
            knockbackTimer    = duration;
        }
    }

    private void Start()
    {
        rb             = GetComponent<Rigidbody2D>();
        animator       = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (rb == null)
            Debug.LogError("Rigidbody2D not found on " + gameObject.name);

        SetupPlayableGraph();
    }

    private void OnDisable()
    {
        if (playableGraph.IsValid())
            playableGraph.Destroy();
        graphReady  = false;
        currentClip = null;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        HandleDirectionInput();
        ApplyActiveDirectionToInput();

        if (moveInput.sqrMagnitude > 0f)
            lastDirection = moveInput.normalized;

        currentItemType = InventoryUI.SelectedAnimationType;

        UpdateSpriteFlip();

        if ((Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.jKey.wasPressedThisFrame)
            && interactTimer <= 0f)
            StartInteract();

        if (interactTimer > 0f)
            interactTimer -= Time.deltaTime;

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        if (movementLocked)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (knockbackTimer > 0f)
        {
            rb.linearVelocity  = knockbackVelocity;
            knockbackVelocity  = Vector2.MoveTowards(knockbackVelocity, Vector2.zero, 12f * Time.fixedDeltaTime);
            knockbackTimer    -= Time.fixedDeltaTime;
            return;
        }

        if (interactTimer > 0f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }

    // ─── Animation ───────────────────────────────────────────────────────────

    private void UpdateAnimation()
    {
        if (!graphReady || interactTimer > 0f) return;

        PlayItemClip(run: moveInput.sqrMagnitude > 0f);
    }

    private void StartInteract()
    {
        if (playerAnimations == null) return;
        if (!playerAnimations.TryGetSet(currentItemType, out PlayerAnimations.AnimSet set)) return;
        if (set.interact == null) return;

        float speed   = Mathf.Max(interactSpeedMultiplier, 0.1f);
        interactTimer = Mathf.Max(set.interact.length / speed, 0.05f);
        PlayClip(set.interact, speed);
    }

    private void PlayItemClip(bool run)
    {
        if (playerAnimations == null) return;
        if (!playerAnimations.TryGetSet(currentItemType, out PlayerAnimations.AnimSet set)) return;

        AnimationClip clip = run
            ? (set.run  != null ? set.run  : set.idle)
            : (set.idle != null ? set.idle : set.run);

        PlayClip(clip, 1f);
    }

    private void UpdateSpriteFlip()
    {
        if (spriteRenderer == null) return;
        if (Mathf.Abs(lastDirection.x) > 0.1f)
            spriteRenderer.flipX = lastDirection.x < 0f;
    }

    // ─── Playable graph ───────────────────────────────────────────────────────

    private void SetupPlayableGraph()
    {
        if (animator == null || playerAnimations == null) return;

        playerAnimations.TryGetSet(ItemAnimationType.None, out PlayerAnimations.AnimSet defaultSet);
        AnimationClip initial = defaultSet.idle != null ? defaultSet.idle : defaultSet.run;
        if (initial == null) return;

        playableGraph = PlayableGraph.Create(gameObject.name + "_AnimGraph");
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        clipPlayable   = AnimationClipPlayable.Create(playableGraph, initial);
        playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
        playableOutput.SetSourcePlayable(clipPlayable);

        playableGraph.Play();
        graphReady  = true;
        currentClip = initial;
    }

    private void PlayClip(AnimationClip clip, float speed = 1f)
    {
        if (!graphReady || clip == null || currentClip == clip) return;

        clipPlayable.Destroy();
        clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
        clipPlayable.SetSpeed(Mathf.Max(speed, 0.1f));
        playableOutput.SetSourcePlayable(clipPlayable);
        currentClip = clip;
    }

    // ─── Direction input ──────────────────────────────────────────────────────

    private void HandleDirectionInput()
    {
        bool upPressed    = Keyboard.current.wKey.isPressed || Keyboard.current.zKey.isPressed;
        bool downPressed  = Keyboard.current.sKey.isPressed;
        bool leftPressed  = Keyboard.current.aKey.isPressed || Keyboard.current.qKey.isPressed;
        bool rightPressed = Keyboard.current.dKey.isPressed;

        bool upReleased    = Keyboard.current.wKey.wasReleasedThisFrame || Keyboard.current.zKey.wasReleasedThisFrame;
        bool downReleased  = Keyboard.current.sKey.wasReleasedThisFrame;
        bool leftReleased  = Keyboard.current.aKey.wasReleasedThisFrame || Keyboard.current.qKey.wasReleasedThisFrame;
        bool rightReleased = Keyboard.current.dKey.wasReleasedThisFrame;

        if ((activeDirection == InputDirection.Up    && upReleased)    ||
            (activeDirection == InputDirection.Down  && downReleased)  ||
            (activeDirection == InputDirection.Left  && leftReleased)  ||
            (activeDirection == InputDirection.Right && rightReleased))
        {
            activeDirection = InputDirection.None;

            if      (upPressed)    activeDirection = InputDirection.Up;
            else if (downPressed)  activeDirection = InputDirection.Down;
            else if (leftPressed)  activeDirection = InputDirection.Left;
            else if (rightPressed) activeDirection = InputDirection.Right;
        }

        if (activeDirection == InputDirection.None)
        {
            bool upJust    = Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.zKey.wasPressedThisFrame;
            bool downJust  = Keyboard.current.sKey.wasPressedThisFrame;
            bool leftJust  = Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.qKey.wasPressedThisFrame;
            bool rightJust = Keyboard.current.dKey.wasPressedThisFrame;

            if      (upJust    && upPressed)    activeDirection = InputDirection.Up;
            else if (downJust  && downPressed)  activeDirection = InputDirection.Down;
            else if (leftJust  && leftPressed)  activeDirection = InputDirection.Left;
            else if (rightJust && rightPressed) activeDirection = InputDirection.Right;
        }
    }

    private void ApplyActiveDirectionToInput()
    {
        moveInput = activeDirection switch
        {
            InputDirection.Up    => Vector2.up,
            InputDirection.Down  => Vector2.down,
            InputDirection.Left  => Vector2.left,
            InputDirection.Right => Vector2.right,
            _                    => Vector2.zero,
        };
    }
}
