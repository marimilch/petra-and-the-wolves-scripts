using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    private GameObject body;
    private GameObject player;
    public PlayerController pScript { get; set; }
    public ParticleSystem hit;
    public float swordRange = 1.5f;
    public float attackDuration = .2f;
    public float startAnglePercentage = 0.75f;
    public float endAnglePercentage = 0.25f;

    private float timeElapsed;
    private float yDistance;
    private float start;
    private float dist;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.root.gameObject;
        body = player.transform.Find("Body").gameObject;

        Debug.Log("Sword Y" + yDistance);

        start = startAnglePercentage * Mathf.PI;
        dist = (endAnglePercentage - startAnglePercentage) * Mathf.PI;

        init(true);
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (enabled)
        {
            timeElapsed += Time.deltaTime;
            swordMovement();
        }
    }

    public void swordMovement()
    {
        var rotationAngle = start + (timeElapsed / attackDuration * dist);

        transform.localPosition = swordRange * new Vector3(
            Mathf.Cos(rotationAngle),
            yDistance,
            Mathf.Sin(rotationAngle)
        );

        transform.localRotation = Quaternion.AngleAxis(
            -rotationAngle * 180/Mathf.PI + 90, Vector3.up
        );
    }

    IEnumerator attackCoroutine()
    {
        yield return new WaitForSeconds(attackDuration);
        gameObject.SetActive(false);
        pScript.attacking = false;
    }

    void init(bool setYDistance = false)
    {
        if (setYDistance)
        {
            yDistance = transform.localPosition.y;
        }
        timeElapsed = 0;
        swordMovement();
    }

    public void attack()
    {
        gameObject.SetActive(true);
        init();
        pScript.attacking = true;
        pScript.attackAnim = true;
        StartCoroutine("attackCoroutine");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (other.gameObject.CompareTag("Enemy"))
        {
            var enemy = other.gameObject.transform.root;
            var rbEnemy = enemy.GetComponent<Rigidbody>();
            var enemyScript = enemy.GetComponent<Enemy>();
            var animator = enemy.Find("Body").GetComponent<Animator>();
            Debug.Log("Enemy hit.");

            //add kill
            pScript.addKill();

            rbEnemy.constraints = RigidbodyConstraints.None;
            rbEnemy.isKinematic = false;
            enemyScript.alive = false;
            animator.SetBool("Sit_b", true);

            Vector3 throwbackDirection =
                (enemy.transform.position - body.transform.position)
            ;

            Vector3 throwbackDirectionTilted = new Vector3(
                throwbackDirection.x,
                enemyScript.throwbackUp,
                throwbackDirection.z
            ).normalized;

            rbEnemy.velocity = Vector3.zero;
            var torqueVector = enemyScript.throwbackForce * new Vector3(
                1, Random.Range(-1, 1), Random.Range(-1, 1)
            );

            rbEnemy.AddForce(
                enemyScript.throwbackForce * throwbackDirectionTilted, ForceMode.Impulse
            );
            rbEnemy.AddTorque(torqueVector, ForceMode.Impulse);

            //hit effect
            HitEffect(.5f * throwbackDirection + transform.position);

            //prepare doom by explosion (ideally somewhere mid air)
            enemyScript.enemyHit();
        }
    }

    void HitEffect(Vector3 position)
    {
        var hitInstance = Instantiate(hit, position, hit.transform.rotation);
    }
}
