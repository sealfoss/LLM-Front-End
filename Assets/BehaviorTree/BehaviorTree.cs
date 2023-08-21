using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Organizes and executes scripted behaviors for agents.
/// </summary>
public abstract class BehaviorTree : MonoBehaviour
{
    /// <summary>
    /// Root node of behavior tree. All other nodes are children of this node.
    /// </summary>
    protected BehaviorNode mRoot;

    /// <summary>
    /// Data structure containing all parameters used to govern nodes' behavior.
    /// </summary>
    private Dictionary<string, object> mBlackboard;

    /// <summary>
    /// List of all nodes making up the tree.
    /// </summary>
    private List<BehaviorNode> mNodes;

    /// <summary>
    /// On/off switch for tree.
    /// </summary>
    private bool mActivated;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        mNodes = new List<BehaviorNode>();
        mBlackboard = new Dictionary<string, object>();
        BuildTree();
        SearchTree(mRoot, mNodes);
    }

    /// <summary>
    /// Builds the tree according to desired behavior for a parcticular agent.
    /// </summary>
    protected abstract void BuildTree();

    /// <summary>
    /// Depth first search of entire tree. Builds node lest and sets reference in
    /// each node back to this class instance.
    /// </summary>
    /// <param name="node">Current node to explore.</param>
    /// <param name="list">List to store nodes in.</param>
    private void SearchTree(BehaviorNode node, List<BehaviorNode> list)
    {
        list.Add(node);
        node.Tree = this;

        foreach(BehaviorNode child in node.Children)
        {
            SearchTree(child, list);
        }
    }

    /// <summary>
    /// Executes tree logic, unless deactivated.
    /// </summary>
    public void RunTree()
    {
        if (mActivated)
        {
            mRoot.Run();
        }
    }

    /// <summary>
    /// Turns the tree on.
    /// </summary>
    public void Activate()
    {
        mActivated = true;
    }

    /// <summary>
    /// Turns the tree off.
    /// </summary>
    public void Deactivate()
    {
        mActivated = false;
    }

    /// <summary>
    /// Sets the value of a given key in the blackboard data structure.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">Key to set.</param>
    /// <param name="value">Value to set at given key.</param>
    public void SetBlackboardValue<T>(string key, T value) where T : class
    {
        if(mBlackboard.ContainsKey(key))
            mBlackboard.Remove(key);
            
        mBlackboard.Add(key, value);
    }

    /// <summary>
    /// Gets the value found in blackboard at given key.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">Get to return value of.</param>
    /// <returns></returns>
    public T GetBlackboardValue<T>(string key) where T : class
    {
        return mBlackboard[key] as T;
    }
}
