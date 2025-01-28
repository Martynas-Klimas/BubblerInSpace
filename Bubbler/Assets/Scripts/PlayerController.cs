using System.Dynamic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    private Vector3 shrinkModifier;
    private Rigidbody2D circleRigidbody;
    //private float distanceToCollider;
    private GameObject shootingProjectile;
    private Vector2 target;
    private GameObject playerModel;
    private int health;
    private AudioSource audioSource;

    public HealthBar healthBar;
    public GameFinishedScreen gameFinishedScreen;

    public float angleOffset;
    public bool stopMoving = false;
    [Header("Movement Variables")]
    [Tooltip("After what time the player moves/fires")]
    public float moveCooldown = 1f;
    [Tooltip("The force bubble gets every move")]
    public float moveForce;
    [Header("For Scaling")]
    //[Tooltip("Initial/Default scale")]
    //public float defaultScale;
    [Tooltip("How many times can the player move before he gameOvers (Loses when scale reaches 0.1)")]//may change
    public int timesToShrink = 20;
    [Tooltip("When scale reaches this size, gameOver is called")]
    public float gameOverScale;
    [Header("Shooting Projectile Stats")]
    [Tooltip("Shooting projectile force given to shoot out...")]
    public float shootingProjectileForce;
    [Header("Animation")]
    [Tooltip("Time between the animation start and then the push")]
    public float timeBetweenAnimMove;
    public Animator animationLoop;
    void Start()
    {
        //make sure game isnt frozen
        Time.timeScale = 1;
        audioSource = GetComponent<AudioSource>();
        float shrinkModifierF = (transform.GetChild(0).transform.localScale.x - gameOverScale) / timesToShrink;
        shrinkModifier = new Vector3(shrinkModifierF, shrinkModifierF, shrinkModifierF);
        playerModel = transform.GetChild(0).gameObject;
        if (!stopMoving) { InvokeRepeating("moveFunction", 1, moveCooldown); }
        circleRigidbody = GetComponent<Rigidbody2D>();
        shootingProjectile = GameObject.Find("Shooting Projectile");
        //Invoke("runAnimationLoopTrue", timeForFirstAnimation);

        //health bar stuff
        health = timesToShrink;
        healthBar.SetMaxHealth(health);
    }
     void runAnimationLoopTrue()
    {
        animationLoop.SetFloat("AnimationSpeed", 1);
    }
    void runAnimationLoopFalse()
    {
        animationLoop.SetFloat("AnimationSpeed", 0);
    }
    bool isHovered;

    void OnMouseEnter()
    {
        isHovered = true;
    }

    void OnMouseExit()
    {
        isHovered = false;
    }
    private Vector2 direction;
    float angle;
    void Update()
    {

        direction = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        direction -= new Vector2(transform.position.x, transform.position.y);
        //Debug.Log(direction);
        direction.Normalize();
        target = new Vector2(transform.position.x + direction.x, transform.position.y + direction.y);
        angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        //target += new Vector2(transform.position.x, transform.position.y);
        //if (Input.GetMouseButtonDown(0))
        //{
        //    moveFunction();
        //}
    }

    private void moveFunction()
    {
        if (isHovered) { return; }

        playerModel.transform.eulerAngles = new Vector3(0, 0, -angle + angleOffset);
        runAnimationLoopTrue();
        Invoke("moveCharacter", timeBetweenAnimMove);
    }
    private void moveCharacter()
    {
        Invoke("runAnimationLoopFalse", 1-timeBetweenAnimMove);
        

        //shootProjectile();
        if (transform.GetChild(0).transform.localScale.x <= gameOverScale)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(2).gameObject.SetActive(true);
            transform.GetChild(3).gameObject.SetActive(false);
            transform.GetChild(2).GetComponent<Animator>().SetBool("Dead",true);
            CancelInvoke("moveFunction");
            Debug.Log("GameOver");
            audioSource.Play();
            //play bubble pop animation
            Invoke("RestartScene", 2);
        }
        else
        {
            circleRigidbody.AddForce(-direction * moveForce, ForceMode2D.Impulse);
            transform.GetChild(0).transform.localScale -= shrinkModifier;
            //distanceToCollider = transform.GetChild(0).transform.localScale.x / 2;

            //healthBar
            health--;
            healthBar.SetHealth(health);
        }
    }
    private void shootProjectile()
    {
        Rigidbody2D shootingProjectileRigid = shootingProjectile.GetComponent<Rigidbody2D>();
        //shootingProjectile.transform.position = transform.position + new Vector3(direction.x, direction.y, -1) * distanceToCollider;
        shootingProjectileRigid.linearVelocity = Vector2.zero;
        shootingProjectileRigid.AddForce(direction * shootingProjectileForce, ForceMode2D.Impulse);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, target);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.tag == "Finish")
        {
            Debug.Log("Game finished");

            // win animation run
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(3).GetComponent<Animator>().SetBool("Win", true);
            CancelInvoke("moveFunction");

            //need to invoke end because player needs to see the game over animation
            healthBar.gameObject.SetActive(false);
            Invoke("restartGame", 2.5f);//game over screen loads after 2 seconds
            //Time.timeScale = 0; // Causes the anim not to play, i just turned off the movement


            //SceneManager.LoadLevel();

        }
    }
    private void restartGame()
    {
        gameFinishedScreen.GameFinished();

    }
    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

