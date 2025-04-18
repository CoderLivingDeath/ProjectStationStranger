using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GravityBehaviour : MonoBehaviour
{
    [SerializeField] private float _gravityRadius = 1;
    [SerializeField] private float _mass = 1;

    private HashSet<Rigidbody2D> _rigidbodies;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent<Rigidbody2D>(out var rigidbody))
        {
            _rigidbodies.Add(rigidbody);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Rigidbody2D>(out var rigidbody))
        {
            _rigidbodies.Remove(rigidbody);
        }
    }

    private void GravityProcess()
    {
        foreach(var rigidbody in _rigidbodies)
        {
            float mass = rigidbody.mass;
            float radius = Vector2.Distance(rigidbody.position, transform.position);
            float f = 6.67f * Mathf.Pow(10, -3) * (_mass * mass)/ Mathf.Pow(radius, 2);

            Vector2 offset = ((Vector2)transform.position - rigidbody.position).normalized * f;
            rigidbody.MovePosition(rigidbody.position + offset);
        }
    }

    private void Start()
    {
        _rigidbodies = new();
    }

    private void FixedUpdate()
    {
        GravityProcess();
    }
}
