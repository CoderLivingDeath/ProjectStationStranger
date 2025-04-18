using Stateless;
using System;
public class MovementStateMachine
{
    public enum State { Idle, Accelerate, Performed, Braking }
    public enum Trigger { Idle, Accelerate, Performed, Braking }

    public State CurrentState => _stateMachine.State;

    public event Action<State> StateChanged;

    private StateMachine<State, Trigger> _stateMachine;

    public MovementStateMachine()
    {
        ConfigureStateMachine();
    }

    private void FireTrigger(Trigger trigger)
    {
        if (_stateMachine.CanFire(trigger)) _stateMachine.Fire(trigger);
    }

    private void OnIdleEntry()
    {
        StateChanged.Invoke(CurrentState);
    }

    private void OnAccelerationEntry()
    {
        StateChanged.Invoke(CurrentState);
    }

    private void OnPerformedEntry()
    {
        StateChanged.Invoke(CurrentState);
    }

    private void OnBrakingEntry()
    {
        StateChanged.Invoke(CurrentState);
    }



    private void ConfigureStateMachine()
    {
        _stateMachine = new StateMachine<State, Trigger>(State.Idle);

        _stateMachine.Configure(State.Idle)
            .Permit(Trigger.Accelerate, State.Accelerate)
            .OnEntry(h => OnIdleEntry());

        _stateMachine.Configure(State.Accelerate)
            .Permit(Trigger.Performed, State.Performed)
            .Permit(Trigger.Braking, State.Braking)
            .OnEntry(h => OnAccelerationEntry());

        _stateMachine.Configure(State.Performed)
            .Permit(Trigger.Braking, State.Braking)
            .Permit(Trigger.Accelerate, State.Accelerate)
            .OnEntry(h => OnPerformedEntry());

        _stateMachine.Configure(State.Braking)
            .Permit(Trigger.Idle, State.Idle)
            .Permit(Trigger.Accelerate, State.Accelerate)
            .OnEntry(h => OnBrakingEntry());
    }

    public void StateUpdate(float normilizedVelocity, bool inputActive)
    {
        switch (normilizedVelocity, inputActive)
        {
            case (0, false):
                FireTrigger(Trigger.Idle);
                break;
            case ( > 0, false):
                FireTrigger(Trigger.Braking);
                break;
            case ( < 1, true):
                FireTrigger(Trigger.Accelerate);
                break;
            case (1, true):
                FireTrigger(Trigger.Performed);
                break;
        }
    }
}
