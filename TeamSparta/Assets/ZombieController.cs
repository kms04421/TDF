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

    private float climbHeight = 0.1f;  // 올라갈 높이
    private float climbSpeed = 5f;   // 올라가는 속도
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
        if (go != null)    // 위쪽 좀비 체크
        {
            if (go.CompareTag("Zombie")) foundZombieAbove = true;
        }
        foreach (ContactPoint2D contact in collision.contacts)
        {
            Vector2 contactPoint = contact.point;

            // 뒤쪽 좀비 체크
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
        ZombieBehind = foundZombieBehind; //뒤쪾 좀비 존재여부
        hasZombieAbove = foundZombieAbove;  // 상단 혹은 이벤트 결과 반영

        if (collision.transform.position.x < transform.position.x &&
            collision.transform.position.y < transform.position.y + 0.5f &&
            collision.transform.position.y > transform.position.y - 0.5f) // 점프 조건 체크
        {
            if (!hasZombieAbove) // 조건에해당하는 좀비가 없을 때만 점프(상단에 좀비,이벤트 실행중인좀비)
            {
                if (!zombieController.hasZombieAbove && !ZombieBehind)
                {
                    // Debug.Log("대상"+collision.gameObject.name + "나 : " + transform.name);
                    JumpZonbie(collision, zombieController);
                }

            }
        }

    }
    public GameObject ChackGamobj(float x, float y, float checkRadius)//아래 좀비 오브젝트 확인 미는 함수 실행
    {
        Vector2 checkPosition = (Vector2)transform.position + new Vector2(x, y);//확인 방향

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
    public void UnderZombiePush()//아래 좀비 오브젝트 확인 미는 함수 실행 //수정
    {
        GameObject go = ChackGamobj(-0.3f, -0.3f, 0.2f);
        if (go == null) return;
        StartCoroutine(KnockbackRoutine(0.7f, 0.3f, go));
    }
    private IEnumerator KnockbackRoutine(float distance, float duration, GameObject go) //아래 좀비 오른쪽으로 밀고 그위치에 자신을 넣는 함수
    {

        Vector3 startPosition = go.transform.position;
        Vector3 targetPosition = startPosition + Vector3.right * distance; // 오른쪽으로 밀기
        ZombieController zombieController = go.GetComponent<ZombieController>();
        isEvent = true;
        zombieController.isEvent = true;
        float elapsed = 0f;
        while (elapsed < duration)//타겟오브젝트 뒤로 밀어냄
        {
            go.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        zombieController.isFirstZombieHitWall = false;
        go.transform.position = targetPosition;
        startPosition = transform.position;
        targetPosition = transform.position + Vector3.down * distance; // 아래로 밀기
        elapsed = 0f;
        duration = duration / 3f;
        while (elapsed < duration)//자신을 아래으로
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;

        isEvent = false;
        zombieController.isEvent = false;
    }

    void JumpZonbie(Collision2D collision, ZombieController zombieController)//좀비 충돌시 이벤트
    {
        isEvent = true;
        zombieController.isEvent = true;
        float boxY = collision.collider.bounds.max.y - 0.1f;  // 콜라이더 박스의 가장 위쪽 좌표
        float boxX = collision.collider.bounds.max.x + 0.1f;  // 콜라이더 박스의 가장 앞쪽 좌표
        Vector2 targetPosition = new Vector2(boxX, boxY + climbHeight);

        StartCoroutine(Climb(targetPosition, zombieController)); // 좀비 위로올라가기

    }
/*    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollisionWithTag(collision); // 충돌한 오브젝트 타입에 따라 다르게 처리
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ground"))
        {
            rb.constraints |= RigidbodyConstraints2D.FreezePositionX;
            isGround = false;
        }
    }
    void ProcessCollisionWithTag(Collision2D collision)//충돌한 오브젝트 타입에 따라 다르게 처리
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
    /*    void ChangeState(State state)//상태변경
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