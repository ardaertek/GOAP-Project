using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public interface IGoapPlanner
{
    ActionPlanning Plan(GoapAgent agent,HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null);
}

public class GoapPlanner : IGoapPlanner
{
    public ActionPlanning Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null)
    {
        List<AgentGoal> orderedGoals = goals
            .Where(g => g.DesiredEffects.Any(b => !b.Evaluate()))
            .OrderByDescending(g => g == mostRecentGoal ? g.Priorty - 0.01 : g.Priorty)
            .ToList();


        foreach (var item in orderedGoals)
        {
            Node goalNode = new Node(null, null, item.DesiredEffects, 0);

            //if any path can be found return plan

            if (FindPath(goalNode, agent.Actions))
            {
                if (goalNode.IsAnyLeaves) continue;

                Stack<AgentAction> actionStack = new Stack<AgentAction>();
                while(goalNode.Leaves.Count > 0)
                {
                    var cheapestLeaf = goalNode.Leaves.OrderBy(leaf => leaf.Cost).First();
                    goalNode = cheapestLeaf;
                    actionStack.Push(cheapestLeaf.Action);
                }
                return new ActionPlanning(item, actionStack, goalNode.Cost);
            }
        }

        //if not any pplan
        Debug.LogError("No Plan Found");
        return null;

    }

    private bool FindPath(Node parent, HashSet<AgentAction> actions)
    {
        foreach (var item in actions)
        {
            var requiredEffects = parent.RequiredEffects;

            //there is no acvtion to take
            requiredEffects.RemoveWhere(b =>  b.Evaluate());

            //if no required effects to fulfill so we have a plan
            if (requiredEffects.Count == 0) return true;

            if (item.Effects.Any(requiredEffects.Contains))
            {
                var newRequiredEffects = new HashSet<AgentBelief>(requiredEffects);
                newRequiredEffects.ExceptWith(item.Effects);
                newRequiredEffects.UnionWith(item.Preconditions);

                var newAvaibleAction = new HashSet<AgentAction>(actions);
                newAvaibleAction.Remove(item);

                var newNode = new Node(parent,item,newRequiredEffects,parent.Cost + item.Cost);

                if (FindPath(newNode, newAvaibleAction))
                {
                    parent.Leaves.Add(newNode);
                    newRequiredEffects.ExceptWith(newNode.Action.Preconditions);
                }

                if(newRequiredEffects.Count == 0)
                {
                    return true;
                }
            }
        }
        return false;
    }
}

public class Node
{
    public Node Pareent { get; }
    public AgentAction Action { get; }
    public HashSet<AgentBelief> RequiredEffects { get; }
    public List<Node> Leaves { get; }
    public float Cost { get; }

    public bool IsAnyLeaves => Leaves.Count == 0 && Action == null;

    public Node(Node pareent, AgentAction action, HashSet<AgentBelief> requiredEffects, float cost)
    {
        Pareent = pareent;
        Action = action;
        RequiredEffects = new HashSet<AgentBelief>(requiredEffects);
        Leaves = new List<Node>();
        Cost = cost;
    }
}

public class ActionPlanning
{
    public AgentGoal AgentGoal { get; }
    public Stack<AgentAction> Actions { get; }
    public float TotalCost { get; set; }

    public ActionPlanning(AgentGoal agentGoal, Stack<AgentAction> actions, float totalCost)
    {
        AgentGoal = agentGoal;
        Actions = actions;
        TotalCost = totalCost;
    }


}
