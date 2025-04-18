using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using static MovementStateMachine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementBehaviour : MonoBehaviour
{
    [Header("States")]
    [SerializeField] private State _currentState;

    public Vector2 Input => _input;

    [Header("Movement input")]
    [SerializeField] private Vector2 _input;

    public float InputMovementSmoothingFactor => _inputMovementSmoothingFactor;
    [SerializeField] float _inputMovementSmoothingFactor = 1f;

    public Vector2 SmoothingInput => _smoothingInput;
    [SerializeField] private Vector2 _smoothingInput;

    private Vector2 _prevSmoothingInput;

    public bool InputIsActive => _inputIsActive;
    [SerializeField] private bool _inputIsActive = false;

    public float DecelerationScale => _decelerationScale;
    [SerializeField] private float _decelerationScale = 5f;


    public Vector2 MoveDelta => _moveDelta;

    [Header("Delta Move")]
    [SerializeField] private Vector2 _moveDelta;
    public float SmoothingDeltaFactor => _smoothingDeltaFactor;
    [SerializeField] private float _smoothingDeltaFactor = 0.1f; // 0..1
    public Vector2 SmoothedMoveDelta => _smoothedMoveDelta;
    [SerializeField] private Vector2 _smoothedMoveDelta;

    public float Velocity => _velocity;

    [Header("Velocity")]
    [SerializeField] private float _velocity;

    public float NormalizedVelocity => _normalizedVelocity;
    [SerializeField] private float _normalizedVelocity;

    public float MaxVelocity => _maxVelocity;
    [SerializeField] private float _maxVelocity;

    [SerializeField] private AnimationCurve _accelerationCurve;
    [SerializeField] private float _accelerationDuration;

    [SerializeField] private AnimationCurve _brakingCurve;
    [SerializeField] private float _brakingDuration;

    private Rigidbody2D _rigidbody;

    private MovementStateMachine _movementStateMachine;

    private CancellationTokenSource _movementCancellationTokenSource;
    private UniTask _currentMovementTask;

    private CancellationTokenSource _decelerationCancellationTokenSource;

    private CancellationTokenSource _rotationCancellationTokenSource;


    private Vector3 GetSmoothedMoveDelta(Vector3 newDelta)
    {
        _smoothedMoveDelta = Vector3.Lerp(_smoothedMoveDelta, newDelta, _smoothingDeltaFactor);
        return _smoothedMoveDelta;
    }

    private float FindCurveTByValue(AnimationCurve curve, float value, int steps = 100)
    {
        float closestT = 0f;
        float minDelta = float.MaxValue;

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            float v = curve.Evaluate(t);
            float delta = Mathf.Abs(v - value);

            if (delta < minDelta)
            {
                minDelta = delta;
                closestT = t;
            }
        }

        return closestT;
    }

    private void SwitchTask(Func<CancellationToken, UniTask> taskFactory)
    {
        _movementCancellationTokenSource?.Cancel();
        _movementCancellationTokenSource?.Dispose();

        _movementCancellationTokenSource = new CancellationTokenSource();
        CancellationToken ct = _movementCancellationTokenSource.Token;

        _currentMovementTask = taskFactory(ct);
    }

    private float CalculateDecelerationFactor(float scale)
    {
        //// Вычисляем угол между двумя направлениями (от 0 до 180)
        //float angle = Vector2.Angle(_direction, _prevDirection) / 180f; // 0-1

        //// Чем больше угол, тем больше замедление (квадрат для чувствительности)
        //float diff = angle * angle;

        //// Масштабируем и ограничиваем от 0 до 1 (фактор всегда неотрицателен)
        //float decelerationFactor = 1f - diff * scale;
        //// Не даём ускориться случайно, clamp к 1 минимум
        //return Mathf.Clamp(decelerationFactor, 0f, 1f);

        // Длина сглаженного дельта-направления (0 - нет изменения, 1 - резкое изменение)
        float deltaMagnitude = Mathf.Clamp01(_smoothedMoveDelta.magnitude);

        // Масштабируем степень замедления
        float deceleration = Mathf.Lerp(1f, 0f, Mathf.Pow(deltaMagnitude, 1.5f) * scale);

        // Фактор замедления между 0 и 1
        return deceleration;
    }

    private async UniTask RotationTask(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var angle = MathF.Atan2(_smoothingInput.y, _smoothingInput.x) * Mathf.Rad2Deg;

            transform.localRotation = Quaternion.Euler(0, 0f, angle - 90);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }
    }

    private async UniTask DecelerationTask(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _normalizedVelocity *= CalculateDecelerationFactor(_decelerationScale);
            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate);
        }
    }

    private async UniTask IdleTask(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }
    }

    private async UniTask AccelerationTask(CancellationToken cancellationToken)
    {
        float startT = FindCurveTByValue(_accelerationCurve, _normalizedVelocity);
        float elapsed = startT * _accelerationDuration;

        while (!cancellationToken.IsCancellationRequested && _normalizedVelocity < 1f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _accelerationDuration);

            _normalizedVelocity = _accelerationCurve.Evaluate(t);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }
    }

    private async UniTask PerformedTask(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }
    }

    private async UniTask BrakingTask(CancellationToken cancellationToken)
    {
        // Получить t для brakingCurve, исходя из текущей _normalizedVelocity
        float startT = FindCurveTByValue(_brakingCurve, _normalizedVelocity);
        float elapsed = startT * _brakingDuration;

        while (!cancellationToken.IsCancellationRequested && elapsed < _brakingDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _brakingDuration);

            _normalizedVelocity = _brakingCurve.Evaluate(t);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }
    }

    private float CalculateVelocity()
    {
        return _maxVelocity * _normalizedVelocity;
    }

    public Vector2 CalculateOffset()
    {
        return SmoothingInput.normalized * CalculateVelocity();
    }

    private void OnIdle()
    {

    }

    private void OnAcceleration()
    {
        _rigidbody.MovePosition(_rigidbody.position + CalculateOffset());
    }

    private void OnPerformed()
    {
        _rigidbody.MovePosition(_rigidbody.position + CalculateOffset());
    }

    private void OnBraking()
    {
        _rigidbody.MovePosition(_rigidbody.position + CalculateOffset());
    }

    private void MovementProcces()
    {
        switch (_movementStateMachine.CurrentState)
        {
            case State.Idle: OnIdle(); break;
            case State.Accelerate: OnAcceleration(); break;
            case State.Performed: OnPerformed(); break;
            case State.Braking: OnBraking(); break;
        }
    }

    private Vector2 SmoothingDirection(Vector2 newDirection, float smooth)
    {
        return Vector2.Lerp(_prevSmoothingInput, newDirection, smooth);
    }

    private void MovementStateMachine_StateChanged(State obj)
    {
        switch (obj)
        {
            case State.Idle:
                SwitchTask(IdleTask);
                break;
            case State.Accelerate:
                SwitchTask(AccelerationTask);
                break;
            case State.Performed:
                SwitchTask(PerformedTask);
                break;
            case State.Braking:
                SwitchTask(BrakingTask);
                break;
        }
    }

    public void Move(Vector2 direction, bool inputActive)
    {
        _input = direction;
        _inputIsActive = inputActive;
    }

    #region Unity Methods

    private void Awake()
    {
        _movementStateMachine = new();

        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _decelerationCancellationTokenSource = new CancellationTokenSource();
        var decelarationTask = DecelerationTask(_decelerationCancellationTokenSource.Token);
        decelarationTask.Forget();

        _rotationCancellationTokenSource = new CancellationTokenSource();
        var rotationTask = RotationTask(_rotationCancellationTokenSource.Token);
        rotationTask.Forget();
    }

    private void FixedUpdate()
    {
        _currentState = _movementStateMachine.CurrentState;
        _velocity = CalculateVelocity();

        _movementStateMachine.StateUpdate(_normalizedVelocity, _inputIsActive);

        _prevSmoothingInput = _smoothingInput;
        _smoothingInput = SmoothingDirection(_input, _inputMovementSmoothingFactor);

        _moveDelta = _smoothingInput - _prevSmoothingInput;
        _smoothedMoveDelta = GetSmoothedMoveDelta(_moveDelta);

        MovementProcces();
    }

    private void OnEnable()
    {
        _movementStateMachine.StateChanged += MovementStateMachine_StateChanged;
    }

    private void OnDisable()
    {
        _movementStateMachine.StateChanged -= MovementStateMachine_StateChanged;
    }

    private void OnDestroy()
    {
        _movementCancellationTokenSource?.Cancel();
        _movementCancellationTokenSource?.Dispose();

        _decelerationCancellationTokenSource?.Cancel();
        _rotationCancellationTokenSource?.Cancel();

        _decelerationCancellationTokenSource?.Dispose();
        _rotationCancellationTokenSource?.Dispose();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position, (Vector2)transform.position + CalculateOffset().normalized * 3);

        Gizmos.color = Color.green;

        Gizmos.DrawLine(transform.position, (Vector2)transform.position + _input.normalized * 3);
    }

    #endregion
}
