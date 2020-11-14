using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    //how fast the player moves
    public float speed = 8.0f;
    //threshold until rotation happens
    public float rotationThreshold = 0.0f;
    //jump force
    public float jumpForce = 800.0f;
    //gravityFactor
    public float gravityModifier = 4.0f;
    //how much the player should fly back when being hit by enemy
    public float throwbackForce;
    //thorwback up
    public float throwbackUp = 0.5f;
    //factor throwback strength in air
    public float throwbackInAirFactor = 0.75f;
    //how much faster when running
    public float runFactor = 2.0f;
    //running charge steps
    public int runChargeSteps = 10;
    //magnitude threshold to start walk animation
    public float walkThresholdAnim = 0.1f;
    //regain control after so much seconds when hit
    public float regainControlAfter = 0.5f;
    //how often the hero can be hit
    public int hp = 5;
    //how many kills the hero has
    private int kills;

    //ui elements
    public TextMeshProUGUI hpView;
    public TextMeshProUGUI killsView;
    public Menu menu;

    //privates lol
    //player will not move with physics, only the jump
    private Rigidbody rbPlayer;
    public bool onGround = true;
    public bool controlsEnabled = true;
    public bool attacking = false;
    private GameObject mainCamera;
    private Animator pAnim;
    public bool invincible = false;

    //animation triggers
    public bool jumpAnim = false;
    public bool attackAnim = false;
    public bool damagedAnim = false;

    //audio
    private AudioSource sound;
    public AudioClip jumpSound;
    public AudioClip hurtSound;
    public float soundVolume = 1.0f;

    //private float initialSpeed;
    //private bool isRunning = false;
    //private float runCharge = 0;

    //bounds
    private float topBound;
    private float bottomBound;
    private float leftBound;
    private float rightBound;

    //input
    private float horizontal;
    private float vertical;
    private Vector3 movementVector;

    //child objects
    private GameObject swordObject;
    private GameObject body;
    private GameObject dirt;

    //body and sword controller for collisions and more
    private BodyController bodyController;
    private SwordController swordController;

    // Start is called before the first frame update
    void Start()
    {
        body = transform.Find("Body").gameObject;
        bodyController = body.GetComponent<BodyController>();
        pAnim = body.GetComponent<Animator>();
        rbPlayer = GetComponent<Rigidbody>();
        dirt = transform.Find("Dirt").gameObject;
        mainCamera = GameObject.Find("Main Camera");
        swordObject = transform.Find("Sword").gameObject;
        swordController = swordObject.GetComponent<SwordController>();
        swordController.pScript = this;
        sound = GetComponent<AudioSource>();
        hpView = GameObject.Find("HP View").GetComponent<TextMeshProUGUI>();

        killsView = GameObject.Find("Kills View").GetComponent<TextMeshProUGUI>();


        Physics.gravity = Menu.gravity * gravityModifier;
        updateView();

        //initialSpeed = speed;

        SetBounds();
    }

    public void loseHealth()
    {
        hp -= 1;
        if (hp <= 0)
        {
            Menu.score = kills;
            GameOver();
        }
        updateView();
    }

    public void GameOver()
    {
        SceneManager.LoadScene("Game Over");
    }

    public void addKill()
    {
        kills += 1;
        updateView();
    }

    void updateView()
    {
        hpView.text = hp + " HP";
        killsView.text = kills + " Kills";
    }

    void SetBounds()
    {
        BoxCollider box = GameObject.Find("Ground").GetComponent<BoxCollider>();

        topBound = box.size.z * 2;
        bottomBound = -box.size.z * 2;
        rightBound = box.size.x * 2;
        leftBound = -box.size.x * 2;
    }

    // Update is called once per frame
    void Update()
    {
        //movement input
        updateMovementInput();

        //jump & attack
        handleMoves();

        //rotate in direction
        rotatePlayer();

        //movement
        applyLegalMovement();

        //apply animation
        applyAnimation();
    }

    void applyAnimation()
    {
        var walkThresholdReached = movementVector.magnitude > walkThresholdAnim;

        if (walkThresholdReached && onGround)
        {
            dirt.SetActive(true);
        } else
        {
            dirt.SetActive(false);
        }

        //run animation
        if (walkThresholdReached)
        {
            pAnim.SetFloat("Speed_f", 0.6f);
        } else
        {
            pAnim.SetFloat("Speed_f", 0.0f);
        }

        //jump animation
        pAnim.SetBool("Jump_b", jumpAnim);
        jumpAnim = false;

        //attack Animation
        pAnim.SetBool("Shoot_b", attackAnim);
        pAnim.SetInteger("WeaponType_int", attackAnim ? 7 : 0);
        attackAnim = false;

        //damage/falling Animation
        pAnim.SetBool("Grounded", !damagedAnim || onGround);
        damagedAnim = false;

        //stay in position
        body.transform.localPosition = new Vector3(
            0, body.transform.localPosition.y, 0
        );
    }

    void handleMoves()
    {
        //do nothing if controls disabled
        if (!controlsEnabled)
        {
            return;
        }

        //jump
        if (Input.GetKeyDown(KeyCode.C) && onGround)
        {
            jump();
        }

        //attack
        if (Input.GetKeyDown(KeyCode.X))
        {
            attack();
        }

        //damaged

        ////run
        //isRunning = Input.GetKey(KeyCode.X);
        //speed = timedRun();
    }

    void attack()
    {
        if (!attacking)
        {
            swordController.attack();
        }
    }

    //float timedRun()
    //{
    //    if (isRunning && runCharge < runChargeSteps)
    //    {
    //        ++runCharge;
    //    }

    //    if (!isRunning && runCharge > 0)
    //    {
    //        --runCharge;
    //    }
    //    return initialSpeed + initialSpeed * (runCharge/runChargeSteps) * Time.deltaTime * runFactor; 
    //}

    void jump()
    {
        sound.PlayOneShot(jumpSound, soundVolume);
        onGround = false;
        rbPlayer.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        jumpAnim = true;
    }

    void updateMovementInput()
    {
        //keep framerate and speed in mind
        float factor = Time.deltaTime * speed;

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        movementVector = factor * GetCameraDependantDirectionVector(
                new Vector3(horizontal, vertical,0)
        );
    }

    Vector3 GetCameraDependantDirectionVector(Vector3 vec)
    {
        Vector3[] camSys = GetCameraCoordinateSystem();

        Vector3 y;

        // camera looks forward -> user moves from his view forward on up
        // camera looks down -> user moves from his view up on up
        if ( Vector3.Dot(camSys[2], Vector3.forward) > 0)
        {
            y = FlattenTo(camSys[2], 0).normalized;
        } else
        {
            y = FlattenTo(camSys[1], 0).normalized;
        }

        Vector3 x = camSys[0];
        return (x * vec.x + y * vec.y).normalized;
    }

    Vector3[] GetCameraCoordinateSystem()
    {
        Matrix4x4 mat = mainCamera.transform.localToWorldMatrix;
        Vector3 e1 = mat * Vector3.right;
        Vector3 e2 = mat * Vector3.up;
        Vector3 e3 = mat * Vector3.forward;

        return new Vector3[] { e1, e2, e3 };
    }

    private Vector3 FlattenTo(Vector3 vec, float y)
    {
        return new Vector3(vec.x, y, vec.z);
    }

    float From0To1(float x)
    {
        x = Mathf.Abs(x) % (Mathf.PI / 2);
        return 0.1f * Mathf.Tan(x) / (2 * Mathf.PI);
    }

    void applyLegalMovement()
    {
        if (controlsEnabled)
        {
            rbPlayer.MovePosition(
                transform.position + movementVector
            );
        }

        keepInBounds();
    }

    private void OnCollisionEnter(Collision collision)
    {
        bodyController.OnCollision(collision.gameObject);
    }

    void keepInBounds()
    {
        if (transform.position.x > rightBound)
        {
            transform.position = (new Vector3(
                rightBound,
                transform.position.y,
                transform.position.z
            ));
        }
        else if (transform.position.x < leftBound)
        {
            transform.position = (new Vector3(
                leftBound,
                transform.position.y,
                transform.position.z
            ));
        }

        if (transform.position.z > topBound)
        {
            transform.position = (new Vector3(
                transform.position.x,
                transform.position.y,
                topBound
            ));
        }
        else if (transform.position.z < bottomBound)
        {
            transform.position = (new Vector3(
                transform.position.x,
                transform.position.y,
                bottomBound
            ));
        }
    }

    void rotatePlayer()
    {
        if (
            movementVector.magnitude > rotationThreshold &&
            arrowsPressed() &&
            controlsEnabled
        )
        {
            //swordObject.transform.rotation =
            //    Quaternion.FromToRotation(Vector3.forward, movementVector);
            //rbPlayer.MoveRotation(
            //    Quaternion.FromToRotation(Vector3.forward, movementVector)
            //);

            rotateToDirection(movementVector);
        }
    }

    void rotateToDirection(Vector3 dir)
    {
        float angle = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
        

        rbPlayer.MoveRotation(
            Quaternion.AngleAxis(angle, Vector3.up)
        );
    }

    bool arrowsPressed()
    {
        //TODO: if controller is used, just return true
        return
            Input.GetKey(KeyCode.UpArrow) ||
            Input.GetKey(KeyCode.DownArrow) ||
            Input.GetKey(KeyCode.LeftArrow) ||
            Input.GetKey(KeyCode.RightArrow) 
        ;
    }
}
