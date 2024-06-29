using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBelief
{
    public string BliefName;

    public Func<bool> _condition = () => false;
    public Func<Vector3> _targetLocation = () => Vector3.zero;

    public AgentBelief(string bliefName) 
    {
        BliefName = bliefName;
    }

    public bool Evaluate () => _condition();

    public class Builder
    {
        readonly AgentBelief _agentBelief;

        public Builder(string agentBeliefName)
        {
            _agentBelief = new AgentBelief(agentBeliefName);
        }

        public Builder BliefWithCondition(Func<bool> condition)
        {
            _agentBelief._condition = condition;
            return this;
        }

        public Builder BliefWithLocation(Func<Vector3> location)
        {
            _agentBelief._targetLocation = location;
            return this;
        }

        public AgentBelief Build()
        {
            return _agentBelief;
        }
    }
}


