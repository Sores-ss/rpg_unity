using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class EnemyAnimation2D : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("Enemy configuration asset. Required.")]
    [SerializeField] private EnemyData enemyData;

    [Header("Settings")]
    [Tooltip("Fallback movement threshold when EnemyCombat is missing.")]
    [Min(0f)]
    [SerializeField] private float movementThreshold = 0.005f;

    private Animator                animator;
    private SpriteRenderer          spriteRenderer;
    private EnemyCombat             combat;
    private PlayableGraph           graph;
    private AnimationPlayableOutput output;
    private AnimationClipPlayable   clipPlayable;
    private AnimationClip           currentClip;
    private Vector3                 previousPosition;
    private float                   attackTimer;
    private Vector2                 lastFacing = Vector2.right;

    private EnemyAnimationType animationType;
    private float              attackPlaybackSpeed;

    private AnimationClip idleClip;
    private AnimationClip runClip;
    private AnimationClip attackClip;
    private AnimationClip attackUp;
    private AnimationClip attackDiagUp;
    private AnimationClip attackSide;
    private AnimationClip attackDiagDown;
    private AnimationClip attackDown;

    private void Awake()
    {
        if (enemyData == null)
        {
            Debug.LogError($"EnemyAnimation2D on '{gameObject.name}' has no EnemyData assigned.", this);
            enabled = false;
            return;
        }

        animationType       = enemyData.animationType;
        attackPlaybackSpeed = enemyData.attackPlaybackSpeed;
        idleClip            = enemyData.idleClip;
        runClip             = enemyData.runClip;
        attackClip          = enemyData.attackClip;
        attackUp            = enemyData.attackUp;
        attackDiagUp        = enemyData.attackDiagUp;
        attackSide          = enemyData.attackSide;
        attackDiagDown      = enemyData.attackDiagDown;
        attackDown          = enemyData.attackDown;

        animator       = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        combat         = GetComponent<EnemyCombat>();
        previousPosition = transform.position;
    }

    private void OnEnable()
    {
        SetupGraph();
        if (combat != null)
            combat.AttackPerformed += OnAttackPerformed;
    }

    private void Start()
    {
        PlayClip(idleClip != null ? idleClip : runClip, 1f);
    }

    private void Update()
    {
        if (!graph.IsValid()) return;

        Vector2 facing = combat != null ? combat.FacingDirection : EstimateFacingFromMovement();
        if (facing.sqrMagnitude > 0.001f)
            lastFacing = facing;

        UpdateSpriteFlip();

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
            return;
        }

        bool isMoving = combat != null ? combat.IsMoving : IsMovingByPosition();
        PlayClip(isMoving ? GetRunClip() : GetIdleClip(), 1f);
    }

    private void OnDisable()
    {
        if (combat != null)
            combat.AttackPerformed -= OnAttackPerformed;

        if (graph.IsValid())
            graph.Destroy();
    }

    private void SetupGraph()
    {
        if (animator == null || graph.IsValid()) return;

        AnimationClip initial = idleClip != null ? idleClip : runClip;
        if (initial == null) return;

        graph = PlayableGraph.Create(gameObject.name + "_EnemyAnimGraph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        clipPlayable = AnimationClipPlayable.Create(graph, initial);
        output = AnimationPlayableOutput.Create(graph, "EnemyAnimation", animator);
        output.SetSourcePlayable(clipPlayable);
        graph.Play();

        currentClip = initial;
    }

    private void OnAttackPerformed()
    {
        AnimationClip clip = GetAttackClip();
        if (clip == null) return;

        float speed = Mathf.Max(attackPlaybackSpeed, 0.1f);
        attackTimer = Mathf.Max(clip.length / speed, 0.05f);
        PlayClip(clip, speed);
    }

    // ─── Clip selection ──────────────────────────────────────────────────────

    private AnimationClip GetIdleClip()
    {
        return idleClip != null ? idleClip : runClip;
    }

    private AnimationClip GetRunClip()
    {
        return runClip != null ? runClip : idleClip;
    }

    private AnimationClip GetAttackClip()
    {
        if (animationType != EnemyAnimationType.AngledAttack)
            return attackClip;

        return GetAngledAttackClip(lastFacing);
    }

    // Selects one of 5 angle clips based on the Y component of the (right-side) direction.
    // Thresholds split the semicircle into 5 equal 36° bands.
    private AnimationClip GetAngledAttackClip(Vector2 dir)
    {
        float y = dir.y;

        AnimationClip clip;
        if      (y >  0.588f) clip = attackUp;
        else if (y >  0.0f)   clip = attackDiagUp;
        else if (y > -0.588f) clip = attackDiagDown;
        else                   clip = attackDown;

        // Treat perfectly horizontal as side
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            clip = attackSide;

        return clip != null ? clip : attackClip;
    }

    // ─── Sprite flipping ─────────────────────────────────────────────────────

    private void UpdateSpriteFlip()
    {
        if (animationType == EnemyAnimationType.Single) return;
        if (spriteRenderer == null) return;

        // Flip when facing left; don't change when facing straight up/down
        if (Mathf.Abs(lastFacing.x) > 0.1f)
            spriteRenderer.flipX = lastFacing.x < 0f;
    }

    // ─── Fallback helpers (no EnemyCombat) ───────────────────────────────────

    private Vector2 EstimateFacingFromMovement()
    {
        Vector2 delta = (Vector2)transform.position - (Vector2)previousPosition;
        return delta;
    }

    private bool IsMovingByPosition()
    {
        float moved = Vector2.Distance(previousPosition, transform.position);
        previousPosition = transform.position;
        return moved > movementThreshold;
    }

    // ─── Playable graph ───────────────────────────────────────────────────────

    private void PlayClip(AnimationClip clip, float speed)
    {
        if (!graph.IsValid() || clip == null) return;

        if (currentClip == clip)
        {
            clipPlayable.SetSpeed(speed);
            return;
        }

        clipPlayable.Destroy();
        clipPlayable = AnimationClipPlayable.Create(graph, clip);
        clipPlayable.SetSpeed(speed);
        output.SetSourcePlayable(clipPlayable);
        currentClip = clip;
    }
}
