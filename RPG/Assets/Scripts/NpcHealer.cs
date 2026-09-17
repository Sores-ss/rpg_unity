using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class NpcHealer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator  npcAnimator;

    [Header("Animations")]
    [SerializeField] private AnimationClip idleClip;
    [SerializeField] private AnimationClip healClip;
    [SerializeField] private AnimationClip healEffectClip;

    [Header("Interaction")]
    [Min(0.1f)]
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private Key   healKey = Key.H;

    [Header("Heal")]
    [Tooltip("Amount healed. 0 = full heal.")]
    [Min(0)]
    [SerializeField] private int   healAmount = 0;
    [Tooltip("Gold cost. 0 = free.")]
    [Min(0)]
    [SerializeField] private int   goldCost   = 0;
    [Min(0f)]
    [SerializeField] private float cooldown   = 5f;

    [Header("Audio")]
    [SerializeField] private AudioClip healSound;
    [Range(0f, 1f)]
    [SerializeField] private float     healVolume = 1f;

    [Header("Indicator")]
    [SerializeField] private GameObject interactIndicator;

    private PlayableGraph           graph;
    private AnimationClipPlayable   clipPlayable;
    private AnimationPlayableOutput output;

    private float nextHealTime;
    private bool  isPlaying;

    private void Awake()
    {
        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }
        if (npcAnimator == null)
            npcAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (npcAnimator != null && idleClip != null)
            SetupGraph(idleClip);
    }

    private void OnDisable()
    {
        if (graph.IsValid())
            graph.Destroy();
    }

    private void Update()
    {
        if (player == null || Keyboard.current == null)
            return;

        bool inRange = Vector2.Distance(transform.position, player.position) <= interactionDistance;

        if (interactIndicator != null)
            interactIndicator.SetActive(inRange);

        if (inRange && !isPlaying && Keyboard.current[healKey].wasPressedThisFrame)
            TryHeal();
    }

    private void TryHeal()
    {
        if (Time.time < nextHealTime)
            return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null || playerHealth.CurrentHealth >= playerHealth.MaxHealth)
            return;

        if (goldCost > 0)
        {
            if (PlayerStats.Instance == null || PlayerStats.Instance.Gold < goldCost)
                return;
        }

        StartCoroutine(HealSequence(playerHealth));
    }

    private IEnumerator HealSequence(PlayerHealth playerHealth)
    {
        isPlaying = true;
        nextHealTime = Time.time + cooldown;

        // 1. Heal animation
        if (healClip != null)
        {
            SwitchClip(healClip);
            yield return new WaitForSeconds(healClip.length);
        }

        // 2. Heal Effect animation
        if (healEffectClip != null)
        {
            SwitchClip(healEffectClip);
            yield return new WaitForSeconds(healEffectClip.length);
        }

        // 3. Soigne le joueur
        if (goldCost > 0 && PlayerStats.Instance != null)
            PlayerStats.Instance.SpendGold(goldCost);

        int amount = healAmount > 0 ? healAmount : playerHealth.MaxHealth;
        playerHealth.Heal(amount);

        if (healSound != null)
            AudioSource.PlayClipAtPoint(healSound, transform.position, healVolume);

        // 4. Retour Idle
        if (idleClip != null)
            SwitchClip(idleClip);

        isPlaying = false;
    }

    private void SetupGraph(AnimationClip initial)
    {
        if (graph.IsValid()) graph.Destroy();

        graph = PlayableGraph.Create(gameObject.name + "_HealerGraph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        clipPlayable = AnimationClipPlayable.Create(graph, initial);
        output = AnimationPlayableOutput.Create(graph, "NpcAnim", npcAnimator);
        output.SetSourcePlayable(clipPlayable);
        graph.Play();
    }

    private void SwitchClip(AnimationClip clip)
    {
        if (npcAnimator == null || clip == null) return;

        if (!graph.IsValid())
            SetupGraph(clip);
        else
        {
            clipPlayable.Destroy();
            clipPlayable = AnimationClipPlayable.Create(graph, clip);
            output.SetSourcePlayable(clipPlayable);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.4f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
