using System.Collections;
using UnityEngine;

public class ZombieController : MonoBehaviour
{

    public State currentState = State.Move;

    private float climbHeight = 0.1f;  // �ö� ����
    private float climbSpeed = 5f;   // �ö󰡴� �ӵ�
    Rigidbody2D rb;
    BoxCollider2D boxCollider2D;
    public bool hasZombieAbove = false;
    public bool ZombieBehind = false;
    public bool isEvent = false;
    public bool isFirstZombieHitWall = false;
    public bool isGround = false;
    private float collisionTime = 0;
    private ZombieManager zombieManager;
    private GameObject forwardObject;
    public ZombieData zombieData;
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        zombieManager = ZombieManager.Instance;
        boxCollider2D = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        switch (currentState)
        {
            case State.Move:
                transform.Translate(Vector2.left * zombieData.speed * Time.deltaTime);
                break;
            case State.Idle:
                if (isFirstZombieHitWall&& hasZombieAbove && isGround || !isFirstZombieHitWall && !hasZombieAbove&& isGround ||
                    hasZombieAbove && !isFirstZombieHitWall && isGround )
                {
                    GameObject chackobj = ChackGamobj(-0.2f, 0.7f, 0.3f);
                    if (chackobj == null)
                    {
                        ChangeState(State.Move);
                        isFirstZombieHitWall = false;
                        isEvent = false;

                    }                   
                }
              
                break;
        }
      
    }
   
    private void OnCollisionStay2D(Collision2D collision)
    {

        if (!collision.transform.CompareTag("Zombie")) return;
       
        ZombieController zombieController = zombieManager.FindZombieByID(collision.gameObject);
        if (zombieController == null) return;
        bool foundZombieAbove = false;
        bool foundZombieBehind = false;
        GameObject go = ChackGamobj(-0.1f, 1.2f, 0.2f);
        if (go != null)    // ���� ���� üũ
        {
            if (go.CompareTag("Zombie"))
            {
                foundZombieAbove = true;
            }
        }
    
        foreach (ContactPoint2D contact in collision.contacts)
        {
            Vector2 contactPoint = contact.point;
            
            if (gameObject.layer != contact.collider.gameObject.layer) continue;
            // ���� ���� üũ
            if (contactPoint.x >= transform.position.x)
            {
                //Debug.Log("contact :" + contact.collider.transform.name);
                foundZombieBehind = true;
            }
            if (isEvent || zombieController.isEvent)
            {
                foundZombieAbove = true;
            }
            
            ZombieBehind = foundZombieBehind; //�ڦU ���� ���翩��
            hasZombieAbove = foundZombieAbove;  // ��� Ȥ�� �̺�Ʈ ��� �ݿ�
            if (gameObject.layer == contact.collider.gameObject.layer &&
                contactPoint.x < transform.position.x &&
                contactPoint.y < transform.position.y + 0.5f &&
                contactPoint.y > transform.position.y - 0.5f) // ���� ���� üũ
            {
               
                if (isGround)
                {
                    if (zombieController.isFirstZombieHitWall)
                    {
                        isFirstZombieHitWall = true;
                        ChangeState(State.Idle);
                    }

                }
                if (!hasZombieAbove) // ���ǿ��ش��ϴ� ���� ���� ���� ����(��ܿ� ����,�̺�Ʈ ������������)
                {
                    if (!zombieController.hasZombieAbove && !ZombieBehind)
                    {
                        if (forwardObject == null)
                        {
                            forwardObject = contact.collider.gameObject;
                        }
                        else if (forwardObject == contact.collider.gameObject)
                        {
                            collisionTime += Time.deltaTime;
                            if (collisionTime > 0.15f)
                            {
                                collisionTime = 0;
                                forwardObject = null;
                                // Debug.Log("���"+collision.gameObject.name + "�� : " + transform.name);
                                JumpZonbie(collision, zombieController);
                            }
                        }
                        else
                        {
                            forwardObject = contact.collider.gameObject;
                            collisionTime = 0;
                        }

                    }

                }
            }
        }


    }
    public GameObject ChackGamobj(float x, float y, float checkRadius)//������ġ ������Ʈ ��ȯ
    {
        Vector2 checkPosition = (Vector2)transform.position + new Vector2(x, y);//Ȯ�� ����

        Collider2D[] hits = Physics2D.OverlapCircleAll(checkPosition, checkRadius, 1 << gameObject.layer);
        if (hits != null)
        {
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Zombie"))
                {
                    if(hit.gameObject != gameObject)
                    {
                        return hit.gameObject;
                    }
                    
                }
            }

        }
        return null;
    }
    public void SetEventStatus(bool status)
    {
        isEvent = status;
    }
    public IEnumerator PushBackCoroutine(float distance, float duration) // ���������� �̴��Լ�
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + Vector3.right * distance; // ���������� �б�
                                                                           // isEvent = true;

        float elapsed = 0f;
        while (elapsed < duration)//Ÿ�ٿ�����Ʈ �ڷ� �о
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
        SetEventStatus(false);

    }
    public IEnumerator PushDownCoroutine(float distance, float duration) // �Ʒ��� �̴� �Լ�
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = transform.position + Vector3.down * distance; // �Ʒ��� �б�
        float elapsed = 0f;
        while (elapsed < duration)//�ڽ��� �Ʒ�����
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
        isFirstZombieHitWall = true;
        SetEventStatus(false);
    }
    void JumpZonbie(Collision2D collision, ZombieController zombieController)//���� �浹�� �̺�Ʈ
    {
        isEvent = true;
        zombieController.isEvent = true;
        boxCollider2D.isTrigger = true;
        isFirstZombieHitWall = false;

        float boxY = collision.collider.bounds.max.y;  // �ݶ��̴� �ڽ��� ���� ���� ��ǥ
        float boxX = collision.collider.bounds.max.x + 0.1f;  // �ݶ��̴� �ڽ��� ���� ���� ��ǥ
        Vector3 targetPosition = new Vector3(boxX, boxY + climbHeight, transform.position.z);
        StartCoroutine(Climb(targetPosition, zombieController)); // ���� ���οö󰡱�

    }
    IEnumerator Climb(Vector3 targetPosition, ZombieController zombieController)
    {
        rb.gravityScale = 0;
        while (transform.position.y < targetPosition.y)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, climbSpeed * Time.deltaTime);
            yield return null;
        }
        rb.gravityScale = 1;  // �߷� �ٽ� Ȱ��ȭ
        boxCollider2D.isTrigger = false;
        isEvent = false;
        zombieController.isEvent = false;
        isGround = false;
        ChangeState(State.Move);

    }
    public void ChangeState(State state)//���º���
    {

        switch (state)
        {
            case State.Move:


                break;
            case State.Idle:


                break;
            case State.Die:
                gameObject.SetActive(false);

                break;
        }
        currentState = state;
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if (gameObject.layer == collision.collider.gameObject.layer)
        {
            if (collision.transform.CompareTag("Ground"))
            {
                isGround = false;
            }
            if (collision.transform.CompareTag("Zombie") && isGround)
            {
                ZombieController zombieController = zombieManager.FindZombieByID(gameObject);
                if (zombieController.isFirstZombieHitWall &&
                    collision.transform.position.x < transform.position.x &&
                    collision.transform.position.y < transform.position.y + 0.5f &&
                    collision.transform.position.y > transform.position.y - 0.5f)
                {
                    isFirstZombieHitWall = false;
                }
                else
                {
                    ChangeState(State.Move);
                }


            }
        }

    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (gameObject.layer == collision.collider.gameObject.layer)
        {
            if (collision.transform.CompareTag("Ground"))
            {
                isGround = true;
            }

        }
        if (collision.transform.CompareTag("Hero"))
        {
            isFirstZombieHitWall = true;
            ChangeState(State.Idle);
        }
    }


/*    void OnDrawGizmos()
    {

        Vector2 checkPosition = (Vector2)transform.position + new Vector2(-0.2f, 0.7f);
        float checkRadius = 0.3f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(checkPosition, checkRadius);
    }*/

}