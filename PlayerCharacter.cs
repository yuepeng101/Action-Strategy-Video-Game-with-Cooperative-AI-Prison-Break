using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacter : MonoBehaviour
{
    public float speed;
    public float turnSpeed;

    public float launchForce = 10;
    //public AudioSource shootAudioSource;
    GameObject[] castles;
    GameObject[] polices;

    public float health;
    float healthMax;
    bool isAlive;

    public Slider healthSlider;

    CharacterController cc;

    bool attacking = false;
    public float attackTime = 1f;

   public Animator animator;

    void Start ()
    {
        castles = GameObject.FindGameObjectsWithTag("Enemy Castle");
        polices = GameObject.FindGameObjectsWithTag("Police");

        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        healthMax = health;
        isAlive = true;
        RefreshHealthHUD();
    }

    private void Update()
    {
        RefreshHealthHUD();
    }

    public void RefreshHealthHUD()
    {
        healthSlider.value = health * 0.01f;
    }


    public void Move(Vector3 v)
    {
        if (!isAlive) return;
        if (attacking) return;
        Vector3 movement = v * speed;
        cc.SimpleMove(movement);
        if(animator)
        {
            animator.SetFloat("Speed", cc.velocity.magnitude);
        }
    }

    public void Rotate(Vector3 lookDir)
    {
        var targetPos = transform.position + lookDir;
        var characterPos = transform.position;

        characterPos.y = 0;
        targetPos.y = 0;
        Vector3 faceToDir = targetPos - characterPos;
        Quaternion faceToQuat = Quaternion.LookRotation(faceToDir);
        Quaternion slerp = Quaternion.Slerp(transform.rotation, faceToQuat, turnSpeed * Time.deltaTime);

        transform.rotation = slerp;
    }

    public void Fire()
    {
        if (!isAlive) return;
        if (attacking) return;

        //shootAudioSource.Play();

        if (animator)
        {
            animator.SetBool("Attacking", true);
        }
        attacking = true;
        Invoke("RefreshAttack", attackTime);
    }

    void RefreshAttack()
    {
        animator.SetBool("Attacking", false);
        attacking = false;
    }

    public GameObject FindNearestEnemy()
    {
        castles = GameObject.FindGameObjectsWithTag("Enemy Castle");
        polices = GameObject.FindGameObjectsWithTag("Police");

        SortedList<float, GameObject> nearest = new SortedList<float, GameObject>();
        foreach (var castle in castles)
        {
            nearest.Add(Vector3.Distance(castle.transform.position, transform.position), castle);
        }

        foreach (var p in polices)
        {
            nearest.Add(Vector3.Distance(p.transform.position, transform.position), p);
        }

        if (nearest.Keys[0] < 35f)
        {
            return nearest.Values[0];
        }
        else
        {
            return null;
        }
    }
}
