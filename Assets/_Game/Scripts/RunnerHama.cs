using UnityEngine;

public class RunnerHama : MonoBehaviour
{
    [Header("Physics & Components")]
    [SerializeField] private float jumpForce = 220f;
    [SerializeField] private float gravity = 480f;
    [SerializeField] private float groundY = 23.5f;
    [SerializeField] private Animator animator;

    private float verticalVelocity = 0f;
    private float currentY;
    private bool isGrounded = true;
    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        currentY = groundY;
    }

    void OnEnable()
    {
        animator.SetInteger("StageState", (int)GameManager.Instance.currentStage);
    }

    public void ResetPosition()
    {
        verticalVelocity = 0f;
        isGrounded = true;
        currentY = groundY;

        rect.anchoredPosition = new Vector2(Mathf.Round(rect.anchoredPosition.x), Mathf.Round(groundY));
    }

    void Update()
    {
        if (!RunnerGameManager.Instance.IsRunning) return;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            verticalVelocity = jumpForce;
            isGrounded = false;
            SoundManager.Instance.PlaySFX(SoundManager.Instance.runnerJumpClip);
        }

        if (!isGrounded)
        {
            verticalVelocity -= gravity * Time.deltaTime;
            currentY += verticalVelocity * Time.deltaTime;

            if (currentY <= groundY)
            {
                currentY = groundY;
                isGrounded = true;
                verticalVelocity = 0f;
            }

            rect.anchoredPosition = new Vector2(
                Mathf.Round(rect.anchoredPosition.x),
                Mathf.Round(currentY)
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle"))
        {
            RunnerGameManager.Instance.TriggerGameOver();
        }
    }
}