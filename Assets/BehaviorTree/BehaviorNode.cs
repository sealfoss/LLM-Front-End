using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Node making up a behavior tree. Dictates agent behavior at discrete steps.
/// </summary>
public abstract class BehaviorNode : MonoBehaviour
{
    /// <summary>
    /// Tree that this node is a part of.
    /// </summary>
    protected BehaviorTree mTree;

    /// <summary>
    /// Accessors for tree parameter.
    /// </summary>
    public BehaviorTree Tree { get => mTree; set => mTree = value; }

    /// <summary>
    /// Nodes executed by this node as part of its own behavior.
    /// </summary>
    protected List<BehaviorNode> mChildren;

    /// <summary>
    /// Accessor for children list.
    /// </summary>
    public  List<BehaviorNode> Children { get => mChildren; }

    /// <summary>
    /// Constructor, initializes list to store children.
    /// </summary>
    public BehaviorNode()
    {
        mChildren = new List<BehaviorNode>();
    }

    /// <summary>
    /// Executes the behavior inherent to the owning node.
    /// </summary>
    /// <returns>Returns true if behavior is successfully executed, false if otherwise.</returns>
    public abstract bool Run();

    /// <summary>
    /// Adds a child node to this node.
    /// </summary>
    /// <param name="child">Node to add as child to this node.</param>
    public void AddChild(BehaviorNode child)
    {
        mChildren.Add(child);
    }
}
