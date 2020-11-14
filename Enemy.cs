using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float throwbackUp = 0.5f;
    public float throwbackForce = 800.0f;
    public float speed = 10.0f;
    public float doomTime = 1.0f;
    public bool alive = true;
    public ParticleSystem doomAnim;

    private Rigidbody rbEnemy;
    private GameObject player;
    private Vector3 direction;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = transform.Find("Body").GetComponent<Animator>();
        rbEnemy = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");

        animator.speed = .5f;
    }

    // Update is called once per frame
    void Update()
    {
        SetMovementVector();
        //follow plyer if alive
        if (alive)
        {
            TurnToPlayer();
            FollowPlayer();
        }

        if (transform.position.y < -5)
        {
            Doom();
        }
            
    }

    public void enemyHit()
    {
        StartCoroutine("PrepareDoom");
    }

    private IEnumerator PrepareDoom()
    {
        yield return new WaitForSeconds(doomTime);
        //todo: instatiate explosion
        Doom();
    }

    private void Doom()
    {
        Instantiate(doomAnim, transform.position, doomAnim.transform.rotation);
        Destroy(gameObject);
    }

    private void SetMovementVector()
    {
        Vector3 _direction = player.transform.position - transform.position;
        _direction = new Vector3(_direction.x, 0, _direction.z);
        direction = _direction.normalized;
    }

    void TurnToPlayer()
    {
        rotateToDirection(direction);
    }

    void rotateToDirection(Vector3 dir)
    {
        rbEnemy.MoveRotation(
            Quaternion.FromToRotation(Vector3.forward, dir)
        );
    }

    void FollowPlayer()
    {
        Vector3 newPosition =
            transform.position + direction * speed * Time.deltaTime;
        rbEnemy.MovePosition(newPosition);
    }
}
