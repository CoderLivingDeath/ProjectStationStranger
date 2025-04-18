using UnityEngine;
using Assets.Project.Scripts.Infrastructure.EventBus.EventHandlers;
using Assets.Project.Scripts.Infrastructure.EventBus;
using Zenject;

[RequireComponent(typeof(Rigidbody2D), typeof(MovementBehaviour))]
public class PlayerBehaviour : MonoBehaviour, IPlayerMoveEventHandler
{
    [SerializeField] private MovementBehaviour _movementBehaviour;

    private IEventBus _eventBus;

    [Inject]
    private void Construct(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    #region Event Hanlders
    public void HandleMove(Vector2 direction)
    {
        _movementBehaviour.Move(direction, direction != Vector2.zero);
    }
    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        _eventBus.Subscribe(this);
    }

    private void OnDisable()
    {
        _eventBus.Unsubscribe(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 3);
    }

    #endregion
}
