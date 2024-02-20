using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class Unit : MonoBehaviour
{
    #region Private Variables
    private NavMeshAgent _agent;
    private bool _isDead = false;
    #endregion

    #region Properties    
    public NavMeshAgent Agent { get { return _agent; } }
    public bool IsDead { get { return _isDead; } }
    public PathNode CurrentNode;
    public PathNode NextNode;

    #endregion

    public void InitilaizeUnit()
    {
        _agent = GetComponent<NavMeshAgent>();
        CurrentNode = new PathNode(transform);
        NextNode = null;
    }

    //Path finder base method looks for all available paths
    public virtual void FindNode()
    {
        CurrentNode.PrepareForNewCalculation();

        foreach (var item in CurrentNode.DirectionList)
        {
            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(CurrentNode.Position, CurrentNode.Position + item, NavMesh.AllAreas, path);
            if (path.status == NavMeshPathStatus.PathComplete)
                CurrentNode.OpenRoads.Add(CurrentNode.Position + item);
        }
    }

    //Moves agent based on NextNode
    public void ExecuteNode()
    {
        if (NextNode != null)
        {
            Agent.SetDestination(NextNode.Position);
            CurrentNode = NextNode;
            NextNode = null;
        }
    }

    public void Dead()
    {
        _isDead = true;
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
    }

    #region Abstract Methods
    public abstract void Movement();        //Abstract method for defining how unit moves
    public abstract bool GetRoad();         //Abstract method for defining huw unit acquires next location to move
    #endregion


    //Node class for any path calculation
    public class PathNode
    {
        public Vector3 Position = Vector3.zero;
        public PathNode PrevNode = null;
        public Vector3 Direction = Vector3.zero;
        public int Cost = 1;
        public bool IsEndNode = false;
        public bool IsRoadsReady = false;

        public List<Vector3> OpenRoads = new List<Vector3>();
        public Queue<Vector3> Roads = new Queue<Vector3>();

        public Vector3[] DirectionList = new Vector3[] { Vector3.forward, Vector3.left, Vector3.back, Vector3.right };

        public static Vector3[] MainDirections = new Vector3[] { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };

        public PathNode() { }

        public PathNode(Transform transform)
        {
            Position = Vector3Int.RoundToInt(transform.position);
            Position.y = 0.5f;
        }

        public void LimitMoveDirections(Vector3[] directionList) => DirectionList = directionList;

        public void PrepareForNewCalculation()
        {
            OpenRoads.Clear();
            Roads.Clear();
            IsRoadsReady = false;
        }

        public void PrepareRoads()
        {
            if (!IsRoadsReady)
            {
                IsRoadsReady = true;
                for (int i = 0; i < OpenRoads.Count; i++)
                    Roads.Enqueue(OpenRoads[i]);
            }
        }

        public void RandomizeRoads() => OpenRoads = OpenRoads.OrderBy(x => Random.Range(0, 100)).ToList();

        public bool IsPathEnded()
        {
            List<Vector3> tmpRoads = new List<Vector3>();

            foreach (var item in DirectionList)
            {
                NavMeshPath path = new NavMeshPath();
                NavMesh.CalculatePath(Position, Position + item, NavMesh.AllAreas, path);
                if (path.status == NavMeshPathStatus.PathComplete)
                    tmpRoads.Add(Position + item);
            }

            tmpRoads.Remove(PrevNode.Position);

            if (tmpRoads.Count == 0)
                return true;

            return false;
        }

    }
}
