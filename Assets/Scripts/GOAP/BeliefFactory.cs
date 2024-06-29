using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeliefFactory
{
    readonly GoapAgent agent;
    readonly Dictionary<string, AgentBelief> beliefs = new Dictionary<string, AgentBelief>();
    public BeliefFactory(GoapAgent agent, Dictionary<string, AgentBelief> beliefs)
    {
        this.agent = agent;
        this.beliefs = beliefs;
    }

    public void AddBelief(string key,Func<bool> condition)
    {
        beliefs.Add(key , new AgentBelief.Builder(key)
            .BliefWithCondition(condition)
            .Build());
    }

    public void AddSensorBelief(string key,Sensor sensor)
    {
        beliefs.Add(key,new AgentBelief.Builder(key)
            .BliefWithCondition(()=> sensor.IsTargetInRange)
            .BliefWithLocation(()=>sensor.TargetPosition)
            .Build());
    }

    public void AddLocationBelief(string key,float distance,Transform locationCondition)
    {
        AddLocationBelief(key,distance,locationCondition.position);
    }
    public void AddLocationBelief(string key,float distance,Vector3 locationCondition)
    {
        beliefs.Add(key, new AgentBelief.Builder(key)
            .BliefWithCondition(() => InRangeOf(locationCondition, distance))
            .BliefWithLocation(() => locationCondition)
            .Build());
    }

    public bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(agent.transform.position,pos) < range;
}
