using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    [Header("Configurações de Movimento")]
    [SerializeField] private float playerSpeed = 5.0f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravityValue = -9.81f;

    private Vector2 moveInput;
    GameObject NPCGameObject;
    public GameObject Diag_Box;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; 
        }

        Vector3 move = transform.forward * moveInput.y + transform.right * moveInput.x;


        controller.Move(move * playerSpeed * Time.deltaTime);


        playerVelocity.y += gravityValue * Time.deltaTime;

        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (groundedPlayer)
        {

            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        }
    }

    public void OnInteract(InputValue value)
    {
        
        if (NPCGameObject!= null)
        {
            NPCGameObject.GetComponentInParent<NPC>().Interact();
           
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("NPC"))
        {
            other.transform.GetChild(0).gameObject.SetActive(true);
            NPCGameObject = other.gameObject;
           
        }
    }
  


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("NPC"))
        {
            other.transform.GetChild(0).gameObject.SetActive(false);
            NPCGameObject = null;
            Diag_Box.gameObject.SetActive(false);
        }
    }
}