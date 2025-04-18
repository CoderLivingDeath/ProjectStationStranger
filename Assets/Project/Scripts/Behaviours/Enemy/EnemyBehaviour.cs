using Cysharp.Threading.Tasks;
using EditorAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBehaviour : MonoBehaviour
{
    public enum State { Idle, Chase}

    public State currentState = State.Idle;

    public float detectionRange = 10f;
    public float closeEnoughDistance = 2f;

    [SerializeField] private Vector2 _smoothMoveInputVector;
    [SerializeField] private Vector2 _moveInputVector;

    [SerializeField] private float _velocityScale = 1f;
    [SerializeField] private float _smoothScale = 0.5f;

    [SerializeField] private float _rotationAngleCorrection = 90;

    private Transform player;

    private Rigidbody2D _rigidbody;

    private Vector2 GetVectorToPlayer()
    {
        return player.position - transform.position;
    }

    private void RotationProcess()
    {
        Vector2 target = _smoothMoveInputVector;

        float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg + _rotationAngleCorrection;

        _rigidbody.MoveRotation(angle);
    }

    private void MovementProcess()
    {
        _smoothMoveInputVector = Vector2.Lerp(_smoothMoveInputVector, _moveInputVector, _smoothScale);

        Vector2 offset = _smoothMoveInputVector * _velocityScale;

        _rigidbody.MovePosition(_rigidbody.position + offset);
    }

    private void Idle()
    {

    }

    private void Chase()
    {
        _moveInputVector = GetVectorToPlayer().normalized;

        MovementProcess();
        RotationProcess();
    }

    #region Unity Methods
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Chase:
                Chase();
                break;
        }
    }
    #endregion
}
