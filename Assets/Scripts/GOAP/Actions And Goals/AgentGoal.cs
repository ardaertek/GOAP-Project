using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentGoal
{
    public string Name { get; }
    public float Priorty {  get; private set; }

    public HashSet<AgentBelief> DesiredEffects { get; } = new();

    public AgentGoal(string name)
    {
        Name = name;
    }

    public class Builder
    {
        readonly AgentGoal goal;
        public Builder(string name) 
        {
            goal = new AgentGoal(name);
        }

        public Builder WithPriorty(float priorty)
        {
            goal.Priorty = priorty;
            return this;
        }

        public Builder WithDesiredEffect(AgentBelief effect)
        {
            goal.DesiredEffects.Add(effect);
            return this;
        }

        public AgentGoal Build()
        {
            return goal;
        }
    }
}
