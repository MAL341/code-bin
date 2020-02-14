using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScarabController : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;
    public float jumpForce;
    public float los;

    GameObject player;
    Rigidbody rb;

    private Vector3 target = new Vector3(0f, 0f, 0f);
    private Quaternion lookRotation;
    private Vector3 direction;
    private Vector3 jumpVector;
    
    private float randX;
    private float randZ;
    private float angle;
    private bool rotatingToLook = false;
    private bool wandering = false;
    private bool rotating = false;
    private bool jumping = false;
    private bool rotatingToAttack = false;
    private bool attacking = false;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        rb = GetComponent<Rigidbody>();
        jumpVector = new Vector3(0f, 1f, 0f);

        beginWaiting();
    }

    void FixedUpdate()
    {
        if(rotatingToLook) {
            direction = (target - transform.position).normalized;
            lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.fixedDeltaTime);
            angle = Quaternion.Angle(transform.rotation, lookRotation);
            if(angle < 1f) {
                rotatingToLook = false;
                if(!rotating) {
                    wandering = true;
                } else {
                    rotating = false;
                }
            }
        }

        if(wandering) {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.fixedDeltaTime);
            if(transform.position == target) {
                wandering = false;
            }
        }

        if(attacking) {
            direction = (player.transform.position - transform.position).normalized;
            lookRotation = Quaternion.LookRotation(direction);
            angle = Quaternion.Angle(transform.rotation, lookRotation);

            if(rotatingToAttack) {
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.fixedDeltaTime);
                if(angle < 1f) {
                    rotatingToAttack = false;
                }
            } else {
                transform.LookAt(player.transform);
            }

            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.fixedDeltaTime);
            if(Vector3.Distance(player.transform.position, transform.position) <= 10f
                && !jumping) {
                jump();
            }

            if(Vector3.Distance(player.transform.position, transform.position) > los) {
                rotatingToAttack = false;
                attacking = false;
                beginWaiting();
            }
        } else {
            if(Vector3.Distance(player.transform.position, transform.position) <= los) {
                attack();
            }
        }

        if(jumping) {
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        }
    }

    void OnCollisionEnter(Collision other) {
        if(other.gameObject.tag == "Floor") {
            jumping = false;
        }
    }

    // Starts the coroutine
    void beginWaiting() {
        StartCoroutine(wait());
    }

    // Determines a random idling action for the scarab
    // to take
    void chooseNewAction() {
        int choice = Random.Range(1, 4); // Max exclusive
        if(choice == 1) {
            wander();
        } else if(choice == 2) {
            rotate();
        } else {
            // Prevents jumping while in midair
            if(transform.position.y < 0.4f) {
                jump();
            } else {
                beginWaiting();
            }
        }
    }

    void wander() {
        // Set target and walk to it over time
        randX = Random.Range(-10f, 10f);
        randZ = Random.Range(-10f, 10f);
        target = new Vector3(transform.position.x + randX, transform.position.y, transform.position.z + randZ);
        rotatingToLook = true;
        beginWaiting();
    }

    void rotate() {
        // Set target to rotate towards
        randX = Random.Range(-10f, 10f);
        randZ = Random.Range(-10f, 10f);
        target = new Vector3(transform.position.x + randX, transform.position.y, transform.position.z + randZ);
        rotatingToLook = true;
        rotating = true;
        beginWaiting();
    }

    void jump() {
        rb.AddForce(jumpVector * jumpForce, ForceMode.Impulse);
        jumping = true;
        if(!attacking) {
            beginWaiting();
        }
    }

    void attack() {
        StopCoroutine(wait());
        rotatingToLook = false;
        wandering = false;
        rotating = false;
        rotatingToAttack = true;
        attacking = true;
    }

    IEnumerator wait() {
        float waitTime = Random.Range(1f, 4f);
        yield return new WaitForSeconds(waitTime);
        chooseNewAction();
    }
}
