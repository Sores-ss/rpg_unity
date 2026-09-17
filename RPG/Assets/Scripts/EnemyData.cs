using System;
using UnityEngine;

public enum EnemyAnimationType
{
    Single,       // One clip per state, no flipping (e.g. static turret)
    Mirrored,     // One clip per state, sprite flips for left (e.g. knight)
    AngledAttack  // Mirrored idle/run + 5-angle attack clips mirrored (e.g. lancer)
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "RPG/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Enemy";
    public bool   isBoss    = false;

    [Header("Stats")]
    [Min(1)]     public int   maxHealth       = 100;
    [Min(1)]     public int   attackDamage    = 10;
    [Min(0.05f)] public float attackCooldown  = 1.2f;
    [Min(0.1f)]  public float attackDistance  = 1.5f;
    [Min(0.1f)]  public float visionDistance  = 4f;
    [Min(0f)]    public float loseTargetDelay = 2f;

    [Header("Movement")]
    [Min(0f)]    public float moveSpeed                = 1.2f;
    [Min(0f)]    public float returnSpeed              = 1.5f;
    [Min(0f)]    public float wanderRadius             = 1.5f;
    [Min(0.01f)] public float wanderPointReachDistance = 0.12f;
    [Min(0f)]    public float wanderPauseDuration      = 1.4f;

    [Header("Knockback")]
    [Range(0f, 1f)] public float knockbackResistance = 0.2f;
    [Min(0.02f)]    public float knockbackDuration    = 0.12f;
    [Min(0f)]       public float knockbackDecay       = 22f;

    [Header("Animation")]
    public EnemyAnimationType animationType = EnemyAnimationType.Mirrored;
    [Min(0.1f)] public float attackPlaybackSpeed = 1f;

    [Header("Clips — idle / run / attack (Single & Mirrored modes)")]
    [Tooltip("Idle animation. For AngledAttack, also used as fallback.")]
    public AnimationClip idleClip;
    [Tooltip("Run/walk animation. For AngledAttack, also used as fallback.")]
    public AnimationClip runClip;
    [Tooltip("Attack animation (Single / Mirrored). Fallback for AngledAttack.")]
    public AnimationClip attackClip;

    [Header("Attack clips — AngledAttack mode (right-side, auto-mirrored for left)")]
    [Tooltip("Attack aimed straight up (~90°).")]
    public AnimationClip attackUp;
    [Tooltip("Attack aimed diagonally up (~45°).")]
    public AnimationClip attackDiagUp;
    [Tooltip("Attack aimed to the side (~0°).")]
    public AnimationClip attackSide;
    [Tooltip("Attack aimed diagonally down (~-45°).")]
    public AnimationClip attackDiagDown;
    [Tooltip("Attack aimed straight down (~-90°).")]
    public AnimationClip attackDown;

    [Header("Loot")]
    public LootEntry[] lootTable = Array.Empty<LootEntry>();
}
