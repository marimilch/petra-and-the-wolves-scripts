using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyController : MonoBehaviour
{
    private GameObject player;
    private PlayerController pController;
    private Rigidbody rbPlayer;
    private AudioSource sound;
    private GameObject blood;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.root.gameObject;
        pController = player.GetComponent<PlayerController>();
        rbPlayer = player.GetComponent<Rigidbody>();
        sound = player.GetComponent<AudioSource>();
        blood = player.transform.Find("Blood").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (pController.onGround)
        {
            pController.controlsEnabled = true;
        }
    }

    public void OnCollision(GameObject other)
    {
        rbPlayer.velocity = Vector3.zero;
        if (other.CompareTag("Ground"))
        {
            pController.onGround = true;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        var enemy = collision.gameObject.transform.root;
        rbPlayer.velocity = Vector3.zero;

        if (enemy.CompareTag("EnemyBite"))
        {
            pController.damagedAnim = true;
            if (!enemy.GetComponent<Enemy>().alive || pController.invincible)
            {
                Debug.Log("Enemy dead or Hero invincible. No damage taken.");
                return;
            }

            Debug.Log("Player hit.");
            pController.loseHealth();
            pController.invincible = true;

            sound.PlayOneShot(pController.hurtSound, pController.soundVolume);
            blood.SetActive(true);
            Vector3 throwbackDirection =
                (transform.position - enemy.transform.position)
            ;

            Vector3 throwbackDirectionTilted = new Vector3(
                throwbackDirection.x,
                throwbackDirection.y + pController.throwbackUp,
                throwbackDirection.z
            ).normalized;

            pController.controlsEnabled = false;

            pController.onGround = false;

            rbPlayer.AddForce(
                pController.throwbackForce * throwbackDirectionTilted, ForceMode.Impulse
            );

            StartCoroutine("RegainControl");
        }
    }

    
    IEnumerator RegainControl()
    {
        yield return new WaitForSeconds(pController.regainControlAfter);
        pController.controlsEnabled = true;
        pController.invincible = false;
        blood.SetActive(false);
    }
}
