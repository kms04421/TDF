using System.Collections;
using UnityEngine;

public class ZombieController : MonoBehaviour
{
    public enum State
    {
        Move,
        Idle
    }
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
    void Start()
    {
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
                break;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.transform.CompareTag("Zombie")) return;

        ZombieController zombieController = collision.transform.GetComponent<ZombieController>();
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
            if (!hasZombieAbove) // ���ǿ��ش��ϴ� ���� ���� ���� ����(��ܿ� ����,�̺�Ʈ ������������)
            {
                if (!zombieController.hasZombieAbove && !ZombieBehind)
                {
                    // Debug.Log("���"+collision.gameObject.name + "�� : " + transform.name);
                    JumpZonbie(collision, zombieController);
                }

            }
        }

    }
    public GameObject ChackGamobj(float x, float y, float checkRadius)//�Ʒ� ���� ������Ʈ Ȯ�� �̴� �Լ� ����
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
    public void UnderZombiePush()//�Ʒ� ���� ������Ʈ Ȯ�� �̴� �Լ� ���� //����
    {
        GameObject go = ChackGamobj(-0.3f, -0.3f, 0.2f);
        if (go == null) return;
        StartCoroutine(KnockbackRoutine(0.7f, 0.3f, go));
    }
    private IEnumerator KnockbackRoutine(float distance, float duration, GameObject go) //�Ʒ� ���� ���������� �а� ����ġ�� �ڽ��� �ִ� �Լ�
    {

        Vector3 startPosition = go.transform.position;
        Vector3 targetPosition = startPosition + Vector3.right * distance; // ���������� �б�
        ZombieController zombieController = go.GetComponent<ZombieController>();
        isEvent = true;
        zombieController.isEvent = true;
        float elapsed = 0f;
        while (elapsed < duration)//Ÿ�ٿ�����Ʈ �ڷ� �о
        {
            go.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        zombieController.isFirstZombieHitWall = false;
        go.transform.position = targetPosition;
        startPosition = transform.position;
        targetPosition = transform.position + Vector3.down * distance; // �Ʒ��� �б�
        elapsed = 0f;
        duration = duration / 3f;
        while (elapsed < duration)//�ڽ��� �Ʒ�����
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;

        isEvent = false;
        zombieController.isEvent = false;
    }

    void JumpZonbie(Collision2D collision, ZombieController zombieController)//���� �浹�� �̺�Ʈ
    {
        isEvent = true;
        zombieController.isEvent = true;
        float boxY = collision.collider.bounds.max.y - 0.1f;  // �ݶ��̴� �ڽ��� ���� ���� ��ǥ
        float boxX = collision.collider.bounds.max.x + 0.1f;  // �ݶ��̴� �ڽ��� ���� ���� ��ǥ
        Vector2 targetPosition = new Vector2(boxX, boxY + climbHeight);

        StartCoroutine(Climb(targetPosition, zombieController)); // ���� ���οö󰡱�

    }
/*    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollisionWithTag(collision); // �浹�� ������Ʈ Ÿ�Կ� ���� �ٸ��� ó��
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ground"))
        {
            rb.constraints |= RigidbodyConstraints2D.FreezePositionX;
            isGround = false;
        }
    }
    void ProcessCollisionWithTag(Collision2D collision)//�浹�� ������Ʈ Ÿ�Կ� ���� �ٸ��� ó��
    {
        ZombieController zombieController = collision.transform.GetComponent<ZombieController>();
      
        if (collision.transform.CompareTag("Zombie") &&
            collision.transform.position.x < transform.position.x &&
            collision.transform.position.y < transform.position.y + 0.5f &&
            collision.transform.position.y > transform.position.y - 0.5f)
        {
            rb.constraints &= ~RigidbodyConstraints2D.FreezePositionX;

        }
        if (collision.transform.CompareTag("Hero"))
        {
            rb.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
            isFirstZombieHitWall = true;
        }
        if (collision.transform.CompareTag("Ground"))
        {
            rb.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
            isGround = true;
        }

    }*/
    /*    void ChangeState(State state)//���º���
         {

             switch (state)
             {
                 case State.Move:


                     break;
                 case State.Idle:


                     break;
             }
             currentState = state;
         }*/

    IEnumerator Climb(Vector2 targetPosition, ZombieController zombieController)
    {
        boxCollider2D.isTrigger = true;
        while (transform.position.y < targetPosition.y)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, climbSpeed * Time.deltaTime);
            yield return null;

        }

        boxCollider2D.isTrigger = false;

        isEvent = false;
        zombieController.isEvent = false;


    }

    /*  void OnDrawGizmos()
     {
         Vector2 checkPosition = (Vector2)transform.position + new Vector2(0.5f, 0);
         float checkRadius =0.5f;

         Gizmos.color = Color.red;
         Gizmos.DrawWireSphere(checkPosition, checkRadius);
     }*/
}