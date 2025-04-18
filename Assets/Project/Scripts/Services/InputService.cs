using Assets.Project.Scripts.Infrastructure.EventBus;
using Assets.Project.Scripts.Infrastructure.EventBus.EventHandlers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputService
{
    private InputSystem_Actions _actions;
    private IEventBus _eventBus;

    private bool isEnabled = false;

    public InputService(InputSystem_Actions actions, IEventBus eventBus)
    {
        _actions = actions;
        _eventBus = eventBus;
        Enable();
    }

    public void Enable()
    {
        if (isEnabled) return;

        _actions.Enable();
        SubscribeAllInput();
    }

    public void Disable()
    {
        if(!isEnabled) return;

        _actions.Disable();
        UnsubcribeAllInput();
    }

    private void Player_Action_Move(InputAction.CallbackContext obj)
    {
        Vector2 value = obj.ReadValue<Vector2>();
        _eventBus.RaiseEvent<IPlayerMoveEventHandler>(h => h.HandleMove(value));
    }

    private void Player_Action_MouseDelta(InputAction.CallbackContext obj)
    {
    }

    private void Player_Action_Attack(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void Player_Action_AlternativeAttack(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void Player_Action_Shift(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void SubscribeAllInput()
    {
        _actions.Player.Move.performed += Player_Action_Move;
        _actions.Player.Move.canceled += Player_Action_Move;

        _actions.Player.Look.performed += Player_Action_MouseDelta;
    }

    private void UnsubcribeAllInput()
    {
        _actions.Player.Move.performed -= Player_Action_Move;
        _actions.Player.Move.canceled -= Player_Action_Move;
        _actions.Player.Look.performed -= Player_Action_MouseDelta;
    }
}