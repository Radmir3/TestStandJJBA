using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float cooldownTime = 2f; // Время кулдауна в секундах
    private float nextAttackTime = 0f; // Время, когда можно будет снова атаковать

    public float radius = 5.0f;
    public float angle = 45.0f;
    public LayerMask detectionMask;

    [SerializeField] Animator anim;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] float shiftSpeed = 10f;
    [SerializeField] float movementSpeed = 5f;
    private bool isBoosted = false;
    float currentSpeed;
    Rigidbody rb;
    Vector3 direction;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    bool isGrounded;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentSpeed = movementSpeed;
    }

    void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
        anim.SetBool("Jump", false);
    }
    // Update is called once per frame
    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
        direction = new Vector3(moveHorizontal, 0.0f, moveVertical);
        direction = transform.TransformDirection(direction);
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            isGrounded = false;
            anim.SetBool("Jump", true);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            
        }
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime) // ЛКМ
        {
            // Генерируем случайное число 0 или 1
            int randomAnimation = Random.Range(0, 2);
            Invoke("Punch", 0f);
            isBoosted = false;
            currentSpeed = movementSpeed;
            anim.SetBool("Run", false);

            if (randomAnimation == 0)
            {
                anim.SetBool("punch1", true);
                anim.SetBool("punch2", false);
            }
            else
            {
                anim.SetBool("punch1", false);
                anim.SetBool("punch2", true);
            }
            nextAttackTime = Time.time + cooldownTime;
        }
        else
        {
            // Если кнопка не нажата, оба параметра ставим в false
            anim.SetBool("punch1", false);
            anim.SetBool("punch2", false);
        }

        if (direction.x != 0 || direction.z != 0)
        {
            anim.SetBool("Walk", true);
            if (isBoosted == true)
            {
                anim.SetBool("Run", true);
            }
        }

        if (direction.x == 0 && direction.z == 0)
        {
            anim.SetBool("Walk", false);
            anim.SetBool("Run", false);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isBoosted = !isBoosted;
            if (isBoosted)
            {
                currentSpeed = shiftSpeed;
                anim.SetBool("Run", true);
            }
            else
            {
                currentSpeed = movementSpeed;
                anim.SetBool("Run", false);
            }
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(transform.position + direction * currentSpeed * Time.deltaTime);
    }

    public GameObject[] CheckObjectsInCone(Vector3 origin, Vector3 direction)
    {
        List<GameObject> objectsInCone = new List<GameObject>();

        Collider[] colliders = Physics.OverlapSphere(origin, radius, detectionMask);

        foreach (Collider collider in colliders)
        {
            Vector3 directionToTarget = (collider.transform.position - origin).normalized;
            float angleToTarget = Vector3.Angle(direction, directionToTarget);

            if (angleToTarget <= angle / 2.0f)
            {
                objectsInCone.Add(collider.gameObject);
            }
        }

        return objectsInCone.ToArray();
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);

        Gizmos.color = Color.red;

        Vector3 forward = transform.forward * radius;
        Quaternion leftRotation = Quaternion.AngleAxis(-angle / 2, Vector3.up);
        Quaternion rightRotation = Quaternion.AngleAxis(angle / 2, Vector3.up);

        Vector3 leftLimit = leftRotation * forward;
        Vector3 rightLimit = rightRotation * forward;

        Gizmos.DrawLine(transform.position, transform.position + leftLimit);
        Gizmos.DrawLine(transform.position, transform.position + rightLimit);
    }

    public void Punch()
    {
        GameObject[] enemies = CheckObjectsInCone(transform.position, transform.forward);
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<Dummy>().dummy();
        }
    }
}
