using System;
using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering;

public class GrapplingHook : MonoBehaviour, ISpecialAbility
{
    [Header("References")]
    [SerializeField, Tooltip("the eye the line is cast from - auto-found from Camera.main if left empty")]
    private Transform mainCamera;
    [SerializeField, Tooltip("auto-found from the PlayerMovement singleton if left empty")]
    private PlayerMovement playerMovement;
    [SerializeField, Tooltip("what the line can catch on - keep the Player layer out of it")]
    private LayerMask hookLayers = ~0;

    [Header("Parameters")]
    [SerializeField] private float range = 30f;
    [SerializeField, Tooltip("how fast the pull reels the player in")]
    private float pullSpeed = 45f;
    [SerializeField, Tooltip("seconds before the hook can fire again (φ)")]
    private float cooldown = PhiMath.PHI;
    [SerializeField, Tooltip("shorter cooldown after a throw that catches nothing - still long enough that a miss cannot be spammed (1/φ)")]
    private float missCooldown = PhiMath.INV_PHI;
    [SerializeField, Tooltip("hard cap on the ride - if the hook point is not reached within this many seconds the line lets go (φ·2)")]
    private float maxPullSeconds = PhiMath.PHI * 2f;
    [SerializeField, Tooltip("range around the hook point on the NPC - the pull only has to carry the player this close for the touch hit to land")]
    private float touchDistance = 3f;
    [SerializeField, Tooltip("stamina drained while the line carries the player, as a multiple of the sprint drain")]
    private float staminaCostMultiplier = 2f;

    [Header("Damage")]
    [SerializeField, Tooltip("HP the hook tags an NPC for the instant the line lands on it")]
    private int tagDamage = 1;
    [SerializeField, Tooltip("HP dealt when the pull actually carries the player into the tagged NPC - lands in full when touching at pull speed, scales with the player's actual speed at the moment of touch")]
    private int touchDamage = 40;

    [Header("Line")]
    [SerializeField, Tooltip("line renderer drawn from the hand to the hook point while the pull lasts - left empty, a simple one is built on this object at startup")]
    private LineRenderer line;
    [SerializeField, Tooltip("where the line leaves the player - the camera with a small hand-side offset when left empty")]
    private Transform lineOrigin;
    [SerializeField] private float lineWidth = 0.035f;
    [SerializeField] private Color lineColor = new(0.92f, 0.88f, 0.78f);

    [Header("Audio")]
    [SerializeField, Tooltip("played at the player when the hook fires")]
    private EventReference fireSound;
    [SerializeField, Tooltip("played at the anchor point when the line lands on something")]
    private EventReference hitSound;
    [SerializeField, Tooltip("played on the NPC when the pull carries the player into it")]
    private EventReference slamSound;
    [SerializeField, Tooltip("played at the player when the line finds nothing to catch")]
    private EventReference missSound;

    private static readonly RaycastHit[] hitBuffer = new RaycastHit[16];
    private static readonly Vector3 handOffset = new(0.21f, -0.13f, 0.3f);

    private AiReferences hookedTarget;
    private Vector3 targetOffset;
    private Vector3 anchorPoint;
    private float inversePullSpeed;
    private float readyTime;
    private float pullDeadline;
    private bool pulling;

    public bool IsReady => !pulling && Time.time >= readyTime;
    public event Action ReadinessChanged;

    private Coroutine readyNotifyRoutine;

    public SpecialAbilityId Id => SpecialAbilityId.GrapplingHook;
    public bool IsUsable => BlastfoolUpgrades.GrapplingHookUnlocked && IsReady;
    public bool TryUse() => TryFire();
    public bool CancelUse() => CancelPull();

    public bool CancelPull()
    {
        if (!pulling) return false;
        ReleasePull();
        return true;
    }

    private void Awake()
    {
        inversePullSpeed = 1f / pullSpeed;
        PrepareLine();
    }

    private void Start() => Resolve();

    // the slot only learns the hook came back off cooldown from ReadinessChanged, so a
    // wait interrupted by OnDisable has to be picked back up rather than dropped
    private void OnEnable()
    {
        float remaining = readyTime - Time.time;
        if (remaining > 0f)
        {
            readyNotifyRoutine = StartCoroutine(NotifyWhenReady(remaining));
            return;
        }

        NotifyIfReady();
    }

    private void OnDisable()
    {
        if (readyNotifyRoutine != null)
        {
            StopCoroutine(readyNotifyRoutine);
            readyNotifyRoutine = null;
        }

        if (!pulling) return;
        pulling = false;
        if (playerMovement) playerMovement.StopGrapple();
        if (line) line.enabled = false;
        NotifyIfReady();
    }

    private void PrepareLine()
    {
        if (!line)
        {
            line = gameObject.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
        }
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth * 0.5f;
        line.startColor = lineColor;
        line.endColor = lineColor;
        line.enabled = false;
    }

    private bool Resolve()
    {
        if (!playerMovement) playerMovement = PlayerMovement.Instance;
        if (!mainCamera && Camera.main) mainCamera = Camera.main.transform;

        if (!playerMovement || !mainCamera)
        {
            DevLog.Error($"{nameof(GrapplingHook)}: needs both a camera and PlayerMovement - the hook cannot fire.", this);
            return false;
        }
        if (!isActiveAndEnabled)
        {
            DevLog.Error($"{nameof(GrapplingHook)}: the component is disabled, so the pull could never tick - enable it on the player.", this);
            return false;
        }
        return true;
    }

    public bool TryFire()
    {
        if (!IsReady || !Resolve()) return false;
        if (!CastForHook(out RaycastHit hit))
        {
            StartCooldown(missCooldown);
            if (!missSound.IsNull) RuntimeManager.PlayOneShot(missSound, mainCamera.position);
            return false;
        }

        hookedTarget = ResolveTarget(hit.collider);
        if (hookedTarget)
        {
            targetOffset = hit.point - hookedTarget.transform.position;
            hookedTarget.Health.TakeQuietDamage(tagDamage);
            if (hookedTarget.damageTakenSmall) hookedTarget.damageTakenSmall.Play();
        }

        if (!fireSound.IsNull) RuntimeManager.PlayOneShot(fireSound, mainCamera.position);
        if (!hitSound.IsNull) RuntimeManager.PlayOneShot(hitSound, hit.point);

        anchorPoint = hit.point;
        StartCooldown(cooldown);
        pullDeadline = Time.time + maxPullSeconds;
        pulling = true;
        line.enabled = true;
        DrawLine();
        playerMovement.StartGrapple(hit.point, pullSpeed);
        return true;
    }

    private void StartCooldown(float seconds)
    {
        readyTime = Time.time + seconds;

        if (readyNotifyRoutine != null) StopCoroutine(readyNotifyRoutine);
        readyNotifyRoutine = seconds > 0f ? StartCoroutine(NotifyWhenReady(seconds)) : null;
    }

    private IEnumerator NotifyWhenReady(float delay)
    {
        yield return new WaitForSeconds(delay);
        readyNotifyRoutine = null;
        NotifyIfReady();
    }

    private void NotifyIfReady()
    {
        if (IsReady) ReadinessChanged?.Invoke();
    }

    private void Update()
    {
        if (!pulling) return;

        bool targetAlive = hookedTarget && hookedTarget.Health && hookedTarget.Health.IsAlive;
        if (targetAlive) anchorPoint = hookedTarget.transform.position + targetOffset;

        if (targetAlive && InTouchRange)
        {
            Slam();
            ReleasePull();
            return;
        }

        if (playerMovement.IsGrappling && Time.time < pullDeadline)
        {
            if (targetAlive) playerMovement.UpdateGrappleTarget(anchorPoint);
            playerMovement.DrainStaminaAtSprintRate(staminaCostMultiplier);
            DrawLine();
            return;
        }

        ReleasePull();
    }

    private bool InTouchRange =>
        (playerMovement.transform.position - anchorPoint).sqrMagnitude <= touchDistance * touchDistance;

    private void Slam()
    {
        float speedRatio = playerMovement.MeasuredVelocity.magnitude * inversePullSpeed;
        int damage = Mathf.Max(1, Mathf.RoundToInt(touchDamage * speedRatio));
        hookedTarget.Health.TakeDamage(damage, false, true);
        if (!slamSound.IsNull) RuntimeManager.PlayOneShot(slamSound, hookedTarget.transform.position);
        if (hookedTarget.damageTakenBig) hookedTarget.damageTakenBig.Play();
    }

    private void ReleasePull()
    {
        playerMovement.StopGrapple();
        pulling = false;
        line.enabled = false;
        NotifyIfReady();
    }

    private void DrawLine()
    {
        line.SetPosition(0, lineOrigin ? lineOrigin.position : mainCamera.TransformPoint(handOffset));
        line.SetPosition(1, anchorPoint);
    }

    private bool CastForHook(out RaycastHit best)
    {
        best = default;
        int count = Physics.RaycastNonAlloc(mainCamera.position, mainCamera.forward, hitBuffer, range, hookLayers, QueryTriggerInteraction.Collide);
        float nearest = float.MaxValue;
        bool found = false;

        for (int i = 0; i < count; i++)
        {
            Collider collider = hitBuffer[i].collider;
            if (collider.isTrigger && !IsNpcHitbox(collider)) continue;
            if (hitBuffer[i].distance >= nearest) continue;

            nearest = hitBuffer[i].distance;
            best = hitBuffer[i];
            found = true;
        }
        return found;
    }

    private static AiReferences ResolveTarget(Collider collider)
    {
        if (collider.CompareTag("Angel")) return null;
        AiReferences owner = collider.GetComponentInParent<AiReferences>();
        if (!owner || !owner.Health || !owner.Health.IsAlive) return null;
        return owner.GetComponent<AngelMind>() ? null : owner;
    }

    private static bool IsNpcHitbox(Collider collider) =>
        collider.CompareTag("Hitbox_Head") || collider.CompareTag("NPC_Hitbox");
}
