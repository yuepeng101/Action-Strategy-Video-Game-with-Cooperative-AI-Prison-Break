using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Assasin : MonoBehaviour
{
    //variables visible in the inspector
    public float lives;
    public float damage;
    public GameObject ragdoll;
    public AudioClip attackAudio;
    public AudioClip runAudio;
    public GameObject target;
    public GameObject tower;
    public Vector3 towerPosition;
    public float minAttackDistance;
    public bool isCalledByPlayer;

    private NavMeshAgent agent;
    private GameObject[] enemies;
    private GameObject health;
    private GameObject healthbar;

    public bool isStunAvaliable;
    public bool isSkillReady;
    private float CDTimer;
    public float StunCoolDown=15.0f;

    [HideInInspector]
    private float startLives;
    private Animator[] animators;
    private Vector3 targetPosition;
    private AudioSource source;

    private ParticleSystem dustEffect;

    //private WaypointTactic tactic;

    // Start is called before the first frame update
    void Start()
    {
        //allow assasin to stun enemy
        CDTimer = 0.0f;
        isStunAvaliable = true;
        isSkillReady = true;

        //tactic = GetComponent<WaypointTactic>();
        source = GetComponent<AudioSource>();

        //find navmesh agent component
        agent = gameObject.GetComponent<NavMeshAgent>();
        animators = gameObject.GetComponentsInChildren<Animator>();

        //find objects attached to this character
        health = transform.Find("Health").gameObject;
        healthbar = health.transform.Find("Healthbar").gameObject;
        health.SetActive(false);

        //set healtbar value
        healthbar.GetComponent<Slider>().maxValue = lives;
        startLives = lives;

        //if there's a dust effect (cavalry characters), find and assign it
        if (transform.Find("dust"))
            dustEffect = transform.Find("dust").gameObject.GetComponent<ParticleSystem>();

        // at the very beginning, the npcs are not called by the player; 
        isCalledByPlayer = false;
    }

    void Update()
    {

        if (Input.GetKeyUp(KeyCode.E))
        {
            isCalledByPlayer = true;
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            isCalledByPlayer = false;
        }

        if (!isCalledByPlayer)
        {
            AssasinDecisionMaking();
        }

        //play dusteffect when running and stop it when the character is not running
        if (dustEffect && animators[0].GetBool("Attacking") == false && !dustEffect.isPlaying)
        {
            dustEffect.Play();
        }
        if (dustEffect && dustEffect.isPlaying && animators[0].GetBool("Attacking") == true)
        {
            dustEffect.Stop();
        }

    }

    void AssasinDecisionMaking()
    {
        // first find the guard tower
        if (tower == null)
        {
            findClosestTower();
            //tower = tactic.findTargetCastle();
            //towerPosition = tower.transform.position;

        }
        //activate health bar if under attack
        if (lives < startLives)
        {
            if (!health.activeSelf)
                health.SetActive(true);
            health.transform.LookAt(2 * transform.position - Camera.main.transform.position);
            healthbar.GetComponent<Slider>().value = lives;
        }
        if (target == null)
        {
            findCurrentTarget();
        }

        //dead condition
        if (lives < 1)
        {
            StartCoroutine(Die());
        }

        if (target != null && Vector3.Distance(transform.position, target.transform.position) < minAttackDistance)
        {
            if (lives < startLives * 0.4)
            {

                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance < 25)
                {
                    Vector3 dirToEnemy = transform.position - target.transform.position;
                    Vector3 newPos = transform.position + dirToEnemy;

                    agent.destination = newPos;
                }
                //recover hp if run off
                else
                {
                    lives = startLives * 0.75f;
                }

                ////find nearest waypoint
                //tactic.FindSecuritySpot(this.gameObject.transform);

            }
            else
            {
                agent.destination = target.transform.position;
            }
            if (Vector3.Distance(target.transform.position, transform.position) <= agent.stoppingDistance)
            {
                transform.LookAt(target.transform.position);

                //Use skill to stun enemy
                StunEnemy();

                foreach (Animator animator in animators)
                {
                    animator.SetBool("Attacking", true);
                }
                source.clip = attackAudio;
                source.Play();

                // Fighter Id=0 shooter=1 assasin=2 player=3 guard=4 Ranger=5 Boss=6

                //sorting dmg with different class here
                if (target.gameObject.GetComponent<CharacterID>().CharacterNum == 5)
                {
                    target.gameObject.GetComponent<Ranger>().lives -= Time.deltaTime * damage;
                }
                if (target.gameObject.GetComponent<CharacterID>().CharacterNum == 4)
                {
                    target.gameObject.GetComponent<Guard>().lives -= Time.deltaTime * damage;
                }
                else if (target.gameObject.GetComponent<CharacterID>().CharacterNum == 6)
                {
                    target.gameObject.GetComponent<Boss>().lives -= Time.deltaTime * damage;
                }
            }
            if (animators[0].GetBool("Attacking") && Vector3.Distance(target.transform.position, transform.position) > agent.stoppingDistance)
            {
                foreach (Animator animator in animators)
                {
                    animator.SetBool("Attacking", false);
                }

                if (source.clip != runAudio)
                {
                    source.clip = runAudio;
                    source.Play();
                }
            }



        }
        else
        {
            if (target == null || Vector3.Distance(transform.position, target.transform.position) > minAttackDistance)
            {
                agent.destination = towerPosition;
                if (tower != null && Vector3.Distance(transform.position, towerPosition) <= 3.0f + tower.GetComponent<Castle>().size)
                {
                    foreach (Animator animator in animators)
                    {
                        animator.SetBool("Attacking", true);
                    }
                    if (tower != null)
                    {
                        tower.GetComponent<Castle>().lives -= Time.deltaTime * damage;
                    }

                    if (source.clip != attackAudio)
                    {
                        source.clip = attackAudio;
                        source.Play();
                    }
                }
                else
                {
                    foreach (Animator animator in animators)
                    {
                        animator.SetBool("Attacking", false);
                    }

                    if (source.clip != runAudio)
                    {
                        source.clip = runAudio;
                        source.Play();
                    }
                }
            }

        }
    }

    public void findClosestTower()
    {
        //find the castles that should be attacked by this character
        GameObject[] Towers = GameObject.FindGameObjectsWithTag("Enemy Castle");

        //distance between character and its nearest castle
        float closestTower = Mathf.Infinity;

        foreach (GameObject GuardTower in Towers)
        {
            //check if there are castles left to attack and check per castle if its closest to this character
            if (Vector3.Distance(transform.position, GuardTower.transform.position) < closestTower && GuardTower != null)
            {
                //if this castle is closest to character, set closest distance to distance between character and this castle
                closestTower = Vector3.Distance(transform.position, GuardTower.transform.position);
                //also set current target to closest target (this castle)
                tower = GuardTower;
            }
        }

        //Define a position to attack the castles(to spread characters when they are attacking the castle)
        if (tower != null)
            towerPosition = tower.transform.position;
    }

    public void findCurrentTarget()
    {
        enemies = GameObject.FindGameObjectsWithTag("Police");


        //distance between character and its nearest enemy
        float closestDistance = Mathf.Infinity;

        foreach (GameObject potentialEnemy in enemies)
        {
            //check if there are enemies left to attack and check per enemy if its closest to this character
            if (Vector3.Distance(transform.position, potentialEnemy.transform.position) < closestDistance && potentialEnemy != null)
            {
                //if this enemy is closest to character, set closest distance to distance between character and enemy
                closestDistance = Vector3.Distance(transform.position, potentialEnemy.transform.position);
                //also set current target to closest target (this enemy)
                if (!target || (target && Vector3.Distance(transform.position, target.transform.position) > 2))
                {
                    target = potentialEnemy;
                }
            }
        }
    }

    

    private void StunEnemy()
    {
        
        if (isSkillReady)
        {
            if (target.gameObject.GetComponent<CharacterID>().CharacterNum == 4)
            {
                //if enemy is not been stun, you can stun
                if (target.gameObject.GetComponent<Guard>().getBoolFreeze() == false)
                {
                    target.gameObject.GetComponent<Guard>().setBoolFreeze(true);
                    isSkillReady = false;
                }
                
            }
            else if (target.gameObject.GetComponent<CharacterID>().CharacterNum == 5)
            {
                //if enemy is not been stun, you can stun
                if (target.gameObject.GetComponent<Ranger>().freeze == false)
                {
                    target.gameObject.GetComponent<Ranger>().freeze=true;
                    isSkillReady = false;
                }

            }
            //else if (target.gameObject.GetComponent<CharacterID>().CharacterNum == 6)
            //{
            //    //if enemy is not been stun, you can stun
            //    if (target.gameObject.GetComponent<Boss>().freeze == false)
            //    {
            //        target.gameObject.GetComponent<Boss>().freeze = true;
            //        isSkillReady = false;
            //    }

            //}

        }
        else
        {
            StunCD();
        }

    }

    //Stun skill cool down
    private void StunCD()
    {
        
        if (isSkillReady == false)
        {
            CDTimer += Time.deltaTime;
            if (CDTimer >= StunCoolDown)
            {
                CDTimer = 0.0f;
               isSkillReady = true;
            }
        }
    }

    private IEnumerator Die()
    {
        Instantiate(ragdoll, transform.position, transform.rotation);
        
        yield return new WaitForEndOfFrame();
        Destroy(this.gameObject);

    }
}
