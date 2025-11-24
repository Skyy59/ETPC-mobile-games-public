using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float acceleration = 10f;
    public float jumpForce = 12f;
    public float friction = 1f;

    public float rayLength = 1f;
    public LayerMask rayMask;

    public float waterSpeed = 1f;
    public float waterJumpForce = 2f;
    public float waterGravityScale = 0.3f;
    private bool _isInWater = false;
    private float _maxVerticalSpeed = 2f;

    private Rigidbody2D _rigidbody;
    private Vector2 _velocity = Vector2.zero;
    private float _input;
    private bool _grounded;

    private int _health = 5;
    private int _checkpoints = 0;
    private int _maxCheckpoints = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        // Cogemos todos los checkpoints de la escena, para saber su numero total
        Checkpoint[] checkponts = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        _maxCheckpoints = checkponts.Length;
        _checkpoints = _maxCheckpoints;

        UIController.instance.SetHealths(_health);
        UIController.instance.SetCheckpoints(_maxCheckpoints);
    }

    // Update is called once per frame
    private void Update()
    {
        _input = UnityEngine.Input.GetAxisRaw("Horizontal");


        if (!_isInWater)
        {
            _grounded = Physics2D.Raycast(transform.position, Vector2.down, rayLength, rayMask);
        }
        else
        {
            _grounded = false;
        }

        if (UnityEngine.Input.GetButtonDown("Jump"))
        {
            if (_isInWater)
            {
                _rigidbody.AddForce(Vector2.up * waterJumpForce, ForceMode2D.Impulse);
            }
            else if (_grounded)
            {
                _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }

    private void FixedUpdate()
    {
        _velocity = _rigidbody.linearVelocity;

        if (_input != 0)
        {
            _velocity.x = Mathf.MoveTowards(_velocity.x, _input * speed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            _velocity.x = Mathf.MoveTowards(_velocity.x, 0, friction * Time.fixedDeltaTime);
        }

        if (_isInWater)
        {
            _velocity.y = Mathf.Clamp(_velocity.y, -_maxVerticalSpeed, _maxVerticalSpeed);
        }

        _rigidbody.linearVelocity = _velocity;
    }

    public void Respawn()
    {
        if (Checkpoint.current != null)
        {
            this.transform.position = Checkpoint.current.transform.position;
            Time.timeScale = 1f;
            _rigidbody.linearVelocity = Vector2.zero;
        }
    }

    public void Kill()
    {
        _health = _health - 1;

        UIController.instance.SetHealths(_health);
        Debug.Log(_health);

        if (_health > 0)
        {
            GameStateManager.instance.ChangeGameState(GameStateManager.GameState.OVER);
        }
        else
        {
            GameStateManager.instance.ChangeGameState(GameStateManager.GameState.OVERMAIN);
        }
    }

    public int GetCheckpointCount()
    {
        return _maxCheckpoints;
    }

    public int GetCheckpointObtained()
    {
        return _checkpoints;
    }

    public void SetCheckpoint(Checkpoint chk)
    {
        // Actualizo variable interna de numero de checkpoints
        _checkpoints = _checkpoints - 1;

        // Actualizo variable visible de numro de checkpoints de la UI
        UIController.instance.SetCheckpoints(_checkpoints);
    }

    public int GetHealth()
    {
        return _health;
    }

    public void SetHealth(int health)
    {
        _health = health;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Water"))
        {
            _isInWater = true;
            _rigidbody.gravityScale = waterGravityScale;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Water"))
        {
            _isInWater = false;
            _rigidbody.gravityScale = 1f;
        }
    }
}
