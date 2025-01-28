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
    private Vector2 target;
    private Vector2 direction;
    private Rigidbody2D circleRigidbody;
    private AudioSource audioSource;
    private float angle;
    private int health;

    
    public HealthBar healthBar;
    public GameFinishedScreen gameFinishedScreen;

    public float angleOffset;
    public bool stopMoving = false;

    [Header("Child Objects")]

    [SerializeField]
    [Tooltip("Sprite of the main circle")]
    private GameObject circleModel;

    [SerializeField]
    [Tooltip("Sprite for loose animation")]
    private GameObject loseHumanModel;

    [SerializeField]
    [Tooltip("Sprite for win animation")]
    private GameObject winHumanModel;

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

    [Header("Restart and finish delays")]
    public float restartDelay = 2f;
    public float gameFinishedDelay = 2.5f;
    void Start()
    {
        //make sure game isnt frozen
        Time.timeScale = 1;
        audioSource = GetComponent<AudioSource>();
        circleRigidbody = GetComponent<Rigidbody2D>();

        //health bar stuff
        health = timesToShrink;
        healthBar.SetMaxHealth(health);

        float shrinkModifierF = (circleModel.transform.localScale.x - gameOverScale) / (timesToShrink - 1);
        shrinkModifier = new Vector3(shrinkModifierF, shrinkModifierF, shrinkModifierF);
        
        if (!stopMoving) { InvokeRepeating("MoveFunction", 1, moveCooldown); }
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
    
    void Update()
    {

        direction = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        direction -= new Vector2(transform.position.x, transform.position.y);
        direction.Normalize();
        target = new Vector2(transform.position.x + direction.x, transform.position.y + direction.y);
        angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        //target += new Vector2(transform.position.x, transform.position.y);
        //if (Input.GetMouseButtonDown(0))
        //{
        //    moveFunction();
        //}
    }

    private void MoveFunction()
    {
        if (isHovered) { return; }

        circleModel.transform.eulerAngles = new Vector3(0, 0, -angle + angleOffset);
        runAnimationLoopTrue();
        Invoke("MoveCharacter", timeBetweenAnimMove);
    }
    private void MoveCharacter()
    {
        Invoke("runAnimationLoopFalse", 1-timeBetweenAnimMove);
        
        //shootProjectile();
        if (circleModel.transform.localScale.x <= gameOverScale)
        {
            circleModel.gameObject.SetActive(false);
            loseHumanModel.gameObject.SetActive(true);
            winHumanModel.gameObject.SetActive(false);

            audioSource.Play();
            loseHumanModel.GetComponent<Animator>().SetBool("Dead",true);
            CancelInvoke("MoveFunction");
            Debug.Log("GameOver");
            Invoke("RestartScene", restartDelay);
        }
        else
        {
            circleRigidbody.AddForce(-direction * moveForce, ForceMode2D.Impulse);
            circleModel.transform.localScale -= shrinkModifier;
            //distanceToCollider = transform.GetChild(0).transform.localScale.x / 2;

            //healthBar
            health--;
            healthBar.SetHealth(health);
        }
    }
   
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, target);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.CompareTag("Finish"))
        {
            Debug.Log("Game finished");

            // win animation run
            circleModel.gameObject.SetActive(false);
            winHumanModel.GetComponent<Animator>().SetBool("Win", true);
            CancelInvoke("MoveFunction");

            //need to invoke end because player needs to see the game over animation
            healthBar.gameObject.SetActive(false);
            Invoke("FinishGame", gameFinishedDelay);//game over screen loads after 2.5 seconds

        }
        //pickup an air bubble
        if(collision.transform.CompareTag("PickUp"))
        {
            health += AirPickup.airAdded;
            healthBar.SetHealth(health);
            circleModel.transform.localScale += shrinkModifier * AirPickup.airAdded;
        }
    }
    private void FinishGame()
    {
        gameFinishedScreen.GameFinished();

    }
    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

