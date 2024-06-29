using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentAction
{
    public AgentAction(string name)
    {
        this.Name = name;
    }
    public string Name { get; }
    public float Cost { get; private set; }

    public HashSet<AgentBelief> Preconditions { get; } = new();
    public HashSet<AgentBelief> Effects { get; } = new();

    private IActionStrategy Istrategy;

    public bool Complate  => Istrategy.Complate;
    public void Start() => Istrategy.Start();
    public void Stop() => Istrategy.Stop();
    public void Update(float deltaTime)
    {
        if (Istrategy.CanPerform)
        {
            Istrategy.Update(deltaTime);
        }
        if (!Istrategy.Complate) return;

        foreach (var effect in Effects)
        {
            effect.Evaluate();
        }
    }

    public class Builder
    {
        readonly AgentAction action;

        public Builder(string name) 
        {
            action = new AgentAction(name)
            {
                Cost = 1
            };
        }
        public Builder WithCost(float Cost)
        {
            action.Cost = Cost;
            return this;
        }
        public Builder WithStrategy(IActionStrategy strategy)
        {
            action.Istrategy = strategy;
            return this;
        }
        public Builder AddPrecondition(AgentBelief preCondition)
        {
            action.Preconditions.Add(preCondition);
            return this;
        }

        public Builder AddEffect(AgentBelief effect)
        {
            action.Effects.Add(effect);
            return this;
        }
        public AgentAction Build()
        {
            return action;
        }
    }

}
