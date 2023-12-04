using System;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    public static event Action OnPlayerDestroyed = delegate { };
    public static event Action<int> OnHealthChanged = delegate { };
    [Header("Movement")]
    [SerializeField] float moveSpeed;
    Rigidbody2D rb;
    [Header("Combat")]
    //PlayerModes currentMode = PlayerModes.Cross;
    bool isMeleeMode = false;
    [SerializeField] float rangedAttackSpeed = 0.75f;
    [SerializeField] GameObject projectile;
    int health = K.PlayerStartingHealth;
    [Header("Melee")]
    [SerializeField] float meleeAttackSpeed = 1.5f;
    [SerializeField] float meleeRange = 1.5f;
    [SerializeField] int meleeDamage = 1000;
    [SerializeField] LayerMask attackMask;
    float time = 0;
    [SerializeField] float healthRegen = 1f;
    float healthRegenTimer = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleMode();
        }
        time += Time.deltaTime;
        
        HandleMode();          
    }

    void FixedUpdate()
    {
        Movement();
        HealthRegen();
    }

    private void HealthRegen()
    {      
        if (health < K.PlayerStartingHealth)
        {
            healthRegenTimer += Time.fixedDeltaTime;
            if(healthRegenTimer > healthRegen) 
            {
                health++;
                healthRegenTimer = 0;
                OnHealthChanged?.Invoke(health);
            }        
        }      
    }

    private void Movement()
    {
        float xInput = Input.GetAxisRaw("Horizontal");
        float zInput = Input.GetAxisRaw("Vertical");

        Vector3 tempVect = new Vector3(xInput, zInput, 0);
        tempVect = tempVect.normalized * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.transform.position + tempVect);
    }


    private void ToggleMode()
    {
        isMeleeMode = !isMeleeMode;
        time = 0;
    }

    private void HandleMode()
    {
        if (isMeleeMode)
        {
            if (time > meleeAttackSpeed)
            {
                Melee();
            }
        }
        else
        {
            if (time > rangedAttackSpeed)
            {
                CrossFire();
            }          
        }
    }

    private void Melee()
    {
        //Debug.Log("melee attack");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, meleeRange, attackMask);
        if (colliders.Length > 0)
        {
            IDamageable damageable = colliders[UnityEngine.Random.Range(0, colliders.Length - 1)].GetComponent<IDamageable>();
            if(damageable != null)
            {
                damageable.CalculateDamage(meleeDamage);
            }          
        }
        time = 0;
    }

    private void CrossFire()
    {
        for (int i = 45; i < 360; i+=90)
        {
            GameObject newProjectile = Instantiate(projectile);
            newProjectile.transform.position = transform.position;
            newProjectile.transform.eulerAngles = new Vector3(0, 0, i);
        }
        time = 0;
    }

    public void CalculateDamage(int damage)
    {
        health -= damage;
        OnHealthChanged?.Invoke(health);
        if (health <= 0)
        {
            Time.timeScale = 0;
            OnPlayerDestroyed?.Invoke();
            Destroy(gameObject);
        }
    }
}