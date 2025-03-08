using System.Collections;
using UnityEngine;

public class ZombieController : MonoBehaviour
{

    public State currentState = State.Move;

    private float climbHeight = 0.1f;  // �ö� ����
    private float climbSpeed = 5f;   // �ö󰡴� �ӵ�
    public float speed = 1f;
    Rigidbody2D rb;
    BoxCollider2D boxCollider2D;
    public bool hasZombieAbove = false;
    public bool ZombieBehind = false;
    public bool isEvent = false;
    public bool isFirstZombieHitWall = false;
    public bool isGround = false;
    private float collisionTime = 0;
    private ZombieManager zombieManager;
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
                transform.Translate(Vector2.left * speed * Time.deltaTime);
                break;
            case State.Idle:
                if(!isFirstZombieHitWall && !isEvent)
                {
                    ChangeState(State.Move);
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
            if (go.CompareTag("Zombie")) foundZombieAbove = true;
        }
        
        foreach (ContactPoint2D contact in collision.contacts)
        {
            Vector2 contactPoint = contact.point;

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

        }
        ZombieBehind = foundZombieBehind; //�ڦU ���� ���翩��
        hasZombieAbove = foundZombieAbove;  // ��� Ȥ�� �̺�Ʈ ��� �ݿ�

        if (collision.transform.position.x < transform.position.x &&
            collision.transform.position.y < transform.position.y + 0.5f &&
            collision.transform.position.y > transform.position.y - 0.5f) // ���� ���� üũ
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
                if (!zombieController.hasZombieAbove && !ZombieBehind )
                {
                    collisionTime += Time.deltaTime;
                    if (collisionTime > 0.15f)
                    {
                        collisionTime = 0;
                        // Debug.Log("���"+collision.gameObject.name + "�� : " + transform.name);
                        JumpZonbie(collision, zombieController);
                    }                   
                }

            }
        }

    }
    public GameObject ChackGamobj(float x, float y, float checkRadius)//������ġ ������Ʈ ��ȯ
    {
        Vector2 checkPosition = (Vector2)transform.position + new Vector2(x, y);//Ȯ�� ����

        Collider2D[] hits = Physics2D.OverlapCircleAll(checkPosition, checkRadius);
        if (hits != null)
        {
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Zombie"))
                {
                    return hit.gameObject;
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
        Vector2 targetPosition = new Vector2(boxX, boxY + climbHeight);
        StartCoroutine(Climb(targetPosition, zombieController)); // ���� ���οö󰡱�

    }
    IEnumerator Climb(Vector2 targetPosition, ZombieController zombieController)
    {
        rb.gravityScale = 0;
        while (transform.position.y < targetPosition.y)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, climbSpeed * Time.deltaTime);
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
        }
        currentState = state;
    }

    public void OnCollisionExit2D(Collision2D collision)
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
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ground"))
        {
            isGround = true;
        }
        if (collision.transform.CompareTag("Hero"))
        {
            /*if (isGround)
            {
                isFirstZombieHitWall = true;
            }*/
            isFirstZombieHitWall = true;
            ChangeState(State.Idle);
        }
       
    }
 

    /*  void OnDrawGizmos()
     {
         Vector2 checkPosition = (Vector2)transform.position + new Vector2(0.5f, 0);
         float checkRadius =0.5f;

         Gizmos.color = Color.red;
         Gizmos.DrawWireSphere(checkPosition, checkRadius);
     }*/

    /*   private IEnumerator PushBackCoroutine(float distance, float duration, GameObject go) // ���������� �̴��Լ�
       {
           Vector3 startPosition = go.transform.position;
           Vector3 targetPosition = startPosition + Vector3.right * distance; // ���������� �б�
           ZombieController zombieController = zombieManager.FindZombieByID(go);
           isEvent = true;
           zombieController.isEvent = true;
           float elapsed = 0f;
           while (elapsed < duration)//Ÿ�ٿ�����Ʈ �ڷ� �о
           {
               go.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
               elapsed += Time.deltaTime;
               yield return null;
           }
           go.transform.position = targetPosition;


       }
       public IEnumerator PushDownCoroutine(float distance, float duration, GameObject go) // �Ʒ��� �̴� �Լ�
       {
           Vector3 startPosition = transform.position;
           Vector3 targetPosition = transform.position + Vector3.down * distance; // �Ʒ��� �б�
           ZombieController zombieController = zombieManager.FindZombieByID(go);
           float elapsed = 0f;
           while (elapsed < duration)//�ڽ��� �Ʒ�����
           {
               transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
               elapsed += Time.deltaTime;
               yield return null;
           }
           transform.position = targetPosition;
           isFirstZombieHitWall = true;
           isEvent = false;
           zombieController.isEvent = false;
       }*/
}