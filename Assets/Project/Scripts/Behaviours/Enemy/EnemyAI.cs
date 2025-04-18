using UnityEngine;

public class EnemyAI
{
    public enum State { Idle, Patrol, Chase, Attack }

    public State currentState = State.Idle;

    public float detectionRange = 10f;
    public float attackRange = 2f;

    private Transform player;

    private void Idle()
    {
    }

    private void Patrol()
    {
    }

    private void Chase()
    {
    }

    private void Attack()
    {
    }

    public void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Patrol:
                Patrol();
                break;
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
        }
    }


}
