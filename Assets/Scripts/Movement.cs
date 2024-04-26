using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms;
using System.Reflection;

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
        public AudioSource audioSource; // Enemy's own AudioSource
        // public SoundManager soundManager;
        // public EnemySoundManager enemySoundManager;

        [Header("Attack Settings")]
        public float attackRange = 2.0f; // Distance within which the AI will attack
        public float attackCooldown = 5.0f; // Time between attacks
        public float attackDamage = 20f;
        // public PlayerStats playerStats;
        private float lastAttackTime = 0; // When the last attack happened

        [Header("Attack Effects")]
        public GameObject attackEffect;  // The GameObject to activate during attacks

        // private Mimic myMimic;
        private float timeSinceLastSighted = 0f;
        private float timeSinceLastWaypointChange = 0f;
        private Vector3 currentScoutTarget;
        private Vector3 lastKnownPlayerPosition;
        public bool isChasing = false;
        public bool playerSpotted = false;  // Track if the player was recently spotted



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

            // Assign target as Player by default, adjust accordingly
            if (target == null)
                target = GameObject.FindGameObjectWithTag("Player")?.transform;


            // Validate or populate the visualIndicators array
            if (visualIndicators == null || visualIndicators.Length == 0)
                visualIndicators = GameObject.FindGameObjectsWithTag("VisualIndicator");

            /*
            // Ensure there is a PlayerStats reference
            if (playerStats == null)
                playerStats = FindObjectOfType<PlayerStats>();
            */

            /*
            if (attackEffect == null)
            {
                attackEffect = GameObject.FindGameObjectWithTag("Lightning");
            }
            */
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

                    // Check if player is within attack range
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

            // UpdateMimicVelocity();
            UpdateVisuals();

            // AdjustPatrolRadius();
        }

        /*
        private void AdjustPatrolRadius()
        {
            if (patrolRadius > 1f)
            {
                patrolRadius = basePatrolRadius * (1 - GameManager.Instance.difficulty / GameManager.Instance.maxDifficulty);
            }
            else
            {
                patrolRadius = 1f;
            }
        }
        */

        private void UpdateVisuals()
        {
            Color targetColor = isChasing ? chaseColor : patrolColor;
            foreach (GameObject indicator in visualIndicators)
            {
                if (indicator != null)
                {
                    // Update Material Color
                    Renderer renderer = indicator.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        // Update base color
                        renderer.material.color = Color.Lerp(renderer.material.color, targetColor, Time.deltaTime * colorTransitionSpeed);

                        // Update emission color
                        Color currentEmissionColor = renderer.material.GetColor("_EmissionColor");
                        Color targetEmissionColor = targetColor * Mathf.LinearToGammaSpace(1.0f); // Adjust brightness here if necessary
                        renderer.material.SetColor("_EmissionColor", Color.Lerp(currentEmissionColor, targetEmissionColor, Time.deltaTime * colorTransitionSpeed));
                    }

                    // Example to change Light color
                    Light light = indicator.GetComponent<Light>();
                    if (light != null)
                    {
                        light.color = Color.Lerp(light.color, targetColor, Time.deltaTime * colorTransitionSpeed);
                    }

                    // Add more component changes as needed
                }
            }
        }

        private void ChasePlayer()
        {
            isChasing = true;
            agent.speed = chaseSpeed;
            // soundManager.FadeInMusic();
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

            // Try to find a valid NavMesh position within the patrol radius
            if (NavMesh.SamplePosition(newTargetPosition, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                currentScoutTarget = hit.position;
                agent.SetDestination(currentScoutTarget);
                timeSinceLastWaypointChange = 0;
            }
        }

        /*
        private void UpdateEyeDirection()
        {
            Quaternion targetRotation;
            if (isChasing)
            {
                // Look directly at the target
                targetRotation = Quaternion.LookRotation(target.position - eyeObject.position);
            }
            else
            {
                // Reset to the initial rotation or a neutral rotation
                targetRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0); // Use this if you want to align with the main object's y rotation
            }

            // Smoothly interpolate the rotation using Slerp or Lerp
            eyeObject.rotation = Quaternion.Slerp(eyeObject.rotation, targetRotation, Time.deltaTime * 5); // Adjust the 5 to your preferred speed
        }
        */

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
                //audioSource.PlayOneShot(spottedSounds[Random.Range(0, spottedSounds.Length)]);
                // soundManager.PlayJumpScareSound();
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
                // soundManager.FadeOutMusic();
            }
        }
        
        /*
        private void UpdateMimicVelocity()
        {

        }
        */

        /*
        // Helper method to play random sound from an array
        public void PlayRandomSound(AudioClip[] clips, float delay, float pitchAdded, bool randomPitch, float spatialBlend)
        {
            if (clips.Length == 0) return; // If no clips are available, exit

            AudioClip clipToPlay = clips[Random.Range(0, clips.Length)];
            if (cowsins.SoundManager.Instance != null)
            {
            }
        }
        */

        private void AttackPlayer()
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time; // Update last attack time
                Debug.Log("Attacking player now!");
                // playerStats.Damage(attackDamage);  // Deal damage to the player
                // enemySoundManager.PlayRandomSoundForDuration(attackSounds, 0.5f);
                StartCoroutine(ActivateAttackEffect());  // Start the coroutine to show and hide the attack effect
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
                attackEffect.SetActive(true);  // Activate the GameObject
                yield return new WaitForSeconds(0.5f);  // Wait for half a second
                attackEffect.SetActive(false);  // Deactivate the GameObject
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

            // Draw a destination gizmo
            if (agent != null && agent.isActiveAndEnabled)
            {
                Gizmos.color = Color.blue;
                Vector3 destination = agent.destination;
                Gizmos.DrawSphere(destination, 0.5f); // Draws a blue sphere at the agent's destination
                Gizmos.DrawLine(transform.position, destination); // Optional: draw a line from the agent to the destination
            }
        }
    }
}
