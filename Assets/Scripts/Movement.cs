using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace MimicSpace
{
    public class Movement : MonoBehaviour
    {
        [Header("Navigation")]
        public float speed = 5f;
        public float sightRange = 10f;
        public float fieldOfViewAngle = 90f;
        public Vector3 sightOffset = Vector3.zero;
        public float loseSightDelay = 3f;
        public float basePatrolRadius = 10f;  // New base value for easy reference and adjustment
        public float patrolRadius = 10f;
        public float waypointTolerance = 1.5f;
        public float minScoutTime = 3.0f;
        public float chaseSpeed = 10f;
        public float patrolSpeed = 5f;

        [Header("NavMesh Agent")]
        public NavMeshAgent agent;
        public Transform target;
        public Transform eyeObject;

        [Header("Visual Feedback")]
        public Color chaseColor = Color.red;
        public Color patrolColor = Color.green;
        public GameObject[] visualIndicators; // Array of GameObjects to modify
        public float colorTransitionSpeed = 1f;

        [Header("Sounds")]
        public AudioClip[] spottedSounds;
        public AudioClip[] chasingSounds;
        public AudioClip[] scoutingSounds;
        public AudioClip[] attackSounds;
        public AudioSource audioSource;

        [Header("Attack Settings")]
        public float attackRange = 2.0f;
        public float attackCooldown = 5.0f;
        public float attackDamage = 20f;
        private float lastAttackTime = 0;

        [Header("Attack Effects")]
        public GameObject attackEffect;

        private float timeSinceLastSighted = 0f;
        private float timeSinceLastWaypointChange = 0f;
        private Vector3 currentScoutTarget;
        private Vector3 lastKnownPlayerPosition;
        public bool isChasing = false;
        public bool playerSpotted = false;

        private void Start()
        {
            agent.speed = patrolSpeed;
            lastKnownPlayerPosition = transform.position;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            if (agent == null) agent = GetComponent<NavMeshAgent>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (eyeObject == null) eyeObject = transform;  // Default to this GameObject if not set

            if (target == null)
                target = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (visualIndicators == null || visualIndicators.Length == 0)
                visualIndicators = GameObject.FindGameObjectsWithTag("VisualIndicator");
        }

        void Update()
        {
            timeSinceLastWaypointChange += Time.deltaTime;

            if (target != null)
            {
                bool canSeePlayer = CanSeePlayer();
                if (canSeePlayer)
                {
                    HandleFirstSighting();
                }
                else
                {
                    if (isChasing && timeSinceLastSighted < loseSightDelay)
                    {
                        ContinueChase();
                    }
                    else
                    {
                        HandleLostSight();
                    }
                }
                if (isChasing)
                {
                    agent.SetDestination(target.position);

                    if (Vector3.Distance(transform.position, target.position) <= attackRange)
                    {
                        AttackPlayer();
                    }
                }
            }
            else
            {
                HandleLostSight();
            }

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isChasing)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    Debug.Log("Agent is stationary. Finding new location.");
                    FindNewScoutLocation();
                }
            }
            else if (timeSinceLastWaypointChange > minScoutTime)
            {
                FindNewScoutLocation();
            }

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            Color targetColor = isChasing ? chaseColor : patrolColor;
            foreach (GameObject indicator in visualIndicators)
            {
                if (indicator != null)
                {
                    Renderer renderer = indicator.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = Color.Lerp(renderer.material.color, targetColor, Time.deltaTime * colorTransitionSpeed);
                        Color currentEmissionColor = renderer.material.GetColor("_EmissionColor");
                        Color targetEmissionColor = targetColor * Mathf.LinearToGammaSpace(1.0f);
                        renderer.material.SetColor("_EmissionColor", Color.Lerp(currentEmissionColor, targetEmissionColor, Time.deltaTime * colorTransitionSpeed));
                    }
                    Light light = indicator.GetComponent<Light>();
                    if (light != null)
                    {
                        light.color = Color.Lerp(light.color, targetColor, Time.deltaTime * colorTransitionSpeed);
                    }
                }
            }
        }

        private void ChasePlayer()
        {
            isChasing = true;
            agent.speed = chaseSpeed;
        }

        private void ContinueChase()
        {
            timeSinceLastSighted += Time.deltaTime;
        }

        private void FindNewScoutLocation()
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection.y = 0; // Keep the direction horizontal
            Vector3 newTargetPosition = target.position + randomDirection;
            if (NavMesh.SamplePosition(newTargetPosition, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                currentScoutTarget = hit.position;
                agent.SetDestination(currentScoutTarget);
                timeSinceLastWaypointChange = 0;
            }
        }

        private bool CanSeePlayer()
        {
            Vector3 eyePosition = transform.position + sightOffset;
            Vector3 toPlayer = target.position - eyePosition;
            float angle = Vector3.Angle(transform.forward, toPlayer);

            if (toPlayer.magnitude < sightRange && angle < fieldOfViewAngle / 2)
            {
                RaycastHit hit;
                if (!Physics.Linecast(eyePosition, target.position, out hit, ~0, QueryTriggerInteraction.Ignore) || hit.transform == target)
                {
                    return true;
                }
            }
            return false;
        }

        private void HandleFirstSighting()
        {
            if (!isChasing)
            {
                ChasePlayer();
            }
        }

        private void HandleLostSight()
        {
            if (isChasing)
            {
                timeSinceLastSighted = 0;
                isChasing = false;
                agent.speed = patrolSpeed;
            }
        }

        private void AttackPlayer()
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
                Debug.Log("Attacking player now!");
                StartCoroutine(ActivateAttackEffect());
            }
            else
            {
                Debug.Log("Attack on cooldown.");
            }
        }

        private IEnumerator ActivateAttackEffect()
        {
            if (attackEffect != null)
            {
                attackEffect.SetActive(true);
                yield return new WaitForSeconds(0.5f);
                attackEffect.SetActive(false);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Vector3 eyePosition = transform.position + sightOffset;
            Gizmos.DrawWireSphere(eyePosition, 0.2f);
            Vector3 forward = Quaternion.Euler(0, fieldOfViewAngle / 2, 0) * transform.forward;
            Vector3 forwardLeft = Quaternion.Euler(0, -fieldOfViewAngle / 2, 0) * transform.forward;
            Gizmos.DrawRay(eyePosition, forward * sightRange);
            Gizmos.DrawRay(eyePosition, forwardLeft * sightRange);

            if (agent != null && agent.isActiveAndEnabled)
            {
                Gizmos.color = Color.blue;
                Vector3 destination = agent.destination;
                Gizmos.DrawSphere(destination, 0.5f);
                Gizmos.DrawLine(transform.position, destination);
            }
        }
    }
}
