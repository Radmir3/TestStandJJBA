using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public TMP_Text uiTMPText;
    public TMP_Text CdBarrage;
    [SerializeField] float cooldownTime = 2f; // ����� �������� � ��������
    private float nextAttackTime = 0f; // �����, ����� ����� ����� ����� ���������
    [SerializeField] public int punched = 0;
    private bool isOnCooldown = false;  // ��������, ������� �� �������
    private float cooldown = 10f;  // ������������ �������� (10 ������)
    private float cooldownTimer = 0f;
    public GameObject[] HandsActivate;
    public float radius = 5.0f;
    public float angle = 45.0f;
    public LayerMask detectionMask;
    public bool isHolding = false;  // ��������, ������ �� ������
    private float holdTime = 0f;  // ������ �����������
    private float maxHoldTime = 2f;  // ����� ��������� (2 �������)

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
        float moveHorizontal = Input.GetAxis("Horizontal") * currentSpeed;
        float moveVertical = Input.GetAxis("Vertical") * currentSpeed;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
        direction = new Vector3(moveHorizontal, rb.velocity.y, moveVertical);
        Vector3 move = transform.right * moveHorizontal + transform.forward * moveVertical;
        Vector3 newVelocity = new Vector3(move.x, rb.velocity.y, move.z);
        rb.velocity = newVelocity;
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            isGrounded = false;
            anim.SetBool("Jump", true);
        }
        if (uiTMPText != null)
        {
            uiTMPText.text = "�����: " + punched.ToString();
        }
        if (cooldownTimer > 0 && punched >= 50)
        {
            cooldownTimer += Time.deltaTime;

            // ���� �� ������ ���������� ���������� ����� �� ������, ��������� �����
            if (CdBarrage != null)
            {
                CdBarrage.gameObject.SetActive(true);
                CdBarrage.text = "Cooldown: " + Mathf.Ceil(cooldownTimer).ToString() + "�";
            }
            if (cooldownTimer >= 9)
            {
                CdBarrage.gameObject.SetActive(false);
            }
        }
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime) // ���
        {
            // ���������� ��������� ����� 0 ��� 1
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
            // ���� ������ �� ������, ��� ��������� ������ � false
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
        if (isOnCooldown)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= cooldown)
            {
                isOnCooldown = false;  // ������� ����������
                cooldownTimer = 0f;  // ���������� ������ ��������
            }
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
        if (Input.GetKey(KeyCode.E) && !isOnCooldown && punched >= 50)
        {
            if (!isHolding)
            {
                isBoosted = false;
                currentSpeed = movementSpeed;
                anim.SetBool("Run", false);
                isHolding = true;
                holdTime = 0f;
                anim.SetBool("Barrage", true);
                ActivateObjects();
                Invoke("PunchBarrage", 0f);
            }

            // ����������� ����� ���������
            holdTime += Time.deltaTime;

            // ���� ������ ������������ ������ 2 ������, ��������� ��������
            if (holdTime >= maxHoldTime)
            {
                StopAnimation();
            }
        }

        // ���� ������ ��������� �� ���������� 2 ������
        if (Input.GetKeyUp(KeyCode.E))
        {
            StopAnimation();
        }
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
    void StopAnimation()
    {
        isHolding = false;
        anim.SetBool("Barrage", false);  // ������������� ��������
        isOnCooldown = true;  // �������� ������� ����� ��������� ��������
        holdTime = 0f;  // ���������� ������ ���������
        DeactivateObjects();
    }

    public void Punch()
    {
        GameObject[] enemies = CheckObjectsInCone(transform.position, transform.forward);
        foreach (GameObject enemy in enemies)
        {
            punched++;
            enemy.GetComponent<Dummy>().dummy();
        }
    }
    public void PunchBarrage()
    {
        if (Input.GetKey(KeyCode.E) && !isOnCooldown)
        {
            GameObject[] enemies = CheckObjectsInCone(transform.position, transform.forward);
            foreach (GameObject enemy in enemies)
            {
                enemy.GetComponent<Dummy>().startbarrage();
            }
            Invoke("PunchBarrage", 0.1f);
        }
    }
    void ActivateObjects()
    {
        foreach (GameObject obj in HandsActivate)
        {
            obj.SetActive(true);  // �������� ������ ������
        }
    }

    // ������� ��� ����������� ���� ��������
    void DeactivateObjects()
    {
        foreach (GameObject obj in HandsActivate)
        {
            obj.SetActive(false);  // ��������� ������ ������
        }
    }
}
