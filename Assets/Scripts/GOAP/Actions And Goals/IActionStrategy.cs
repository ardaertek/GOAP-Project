using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface IActionStrategy
{
    public bool CanPerform {  get; }
    public bool Complate { get; }

    public void Start()
    {

    }
    public void Stop()
    {

    }
    public void Update(float deltaTime) 
    {

    }
}

public class IdleStrategy : IActionStrategy
{
    public bool CanPerform => true;

    public bool Complate { get; private set; }

    readonly CountdownTimer _countdownTimer;
    public IdleStrategy(float duration)
    {
        _countdownTimer = new CountdownTimer(duration);
        _countdownTimer.OnTimerStart += () => Complate = false;
        _countdownTimer.OnTimerStop += () => Complate = true;

    }

    public void Start()=> _countdownTimer.Start();
    public void Update(float deltaTime) => _countdownTimer.Tick(deltaTime);
}

public class WanderStrategy : IActionStrategy
{
    readonly NavMeshAgent _agent;
    readonly float wanderRadius;
    public bool CanPerform => true;

    public bool Complate => _agent.remainingDistance <= 2f && !_agent.pathPending;

    public WanderStrategy(NavMeshAgent agent,float radius)
    {
        this._agent = agent;
        this.wanderRadius = radius;
    }

    public void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomDirection = (UnityEngine.Random.insideUnitCircle * wanderRadius);
            NavMeshHit hit;

            if(NavMesh.SamplePosition(_agent.transform.position + randomDirection,out hit, wanderRadius, 1))
            {
                _agent.SetDestination(hit.position);
                return;
            }
        }
        
    }
}

public class MoveStrategy : IActionStrategy
{
    readonly NavMeshAgent _agent;
    readonly Func<Vector3> destination;
    public bool CanPerform => !Complate;

    public bool Complate => _agent.remainingDistance <= 2f && !_agent.pathPending;

    public MoveStrategy(NavMeshAgent agent, Func<Vector3> destination)
    {
        this._agent = agent;
        this.destination = destination;
    }

    public void Start() => _agent.SetDestination(destination());
    public void Stop() => _agent.ResetPath();
}

public class EatStrategy : IActionStrategy
{
    readonly GoapAgent _agent;
    public bool CanPerform => !Complate;

    public bool Complate => _agent.Health > 80;

    public EatStrategy(GoapAgent health)
    {
        _agent = health;
    }


}
