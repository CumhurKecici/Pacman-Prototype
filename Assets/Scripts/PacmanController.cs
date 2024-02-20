using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PacmanController : Unit
{

    private PlayerInput _inputController;

    void Start()
    {
        InitilaizeUnit();
        CurrentNode.Direction = Vector3.right;
        _inputController = GetComponent<PlayerInput>();

    }

    void Update()
    {
        Movement();
    }

    public override void Movement()
    {
        if (Agent.hasPath)
            return;

        FindNode();
        if (GetRoad())
            ExecuteNode();
    }

    public override void FindNode()
    {
        var direction = _inputController.actions.FindAction("Move").ReadValue<Vector3>();
        direction = Vector3Int.RoundToInt(direction);

        var DirectionList = new List<Vector3>();

        if (direction.sqrMagnitude == 0)
            return;

        if (direction.x != 0) DirectionList.Add(direction.x > 0 ? Vector3.right : Vector3.left);
        if (direction.z != 0) DirectionList.Add(direction.z > 0 ? Vector3.forward : Vector3.back);

        CurrentNode.LimitMoveDirections(DirectionList.ToArray());
        base.FindNode();
    }

    public override bool GetRoad()
    {
        CurrentNode.PrepareRoads();

        if (CurrentNode.Roads.Count == 0)
        {
            NextNode = null;
            return false;
        }

        PathNode _nextNode = new PathNode();

        if (CurrentNode.Roads.Count > 1)
        {
            _nextNode.Position = CurrentNode.Roads.Dequeue();
            if (_nextNode.Position - CurrentNode.Position == CurrentNode.Direction)
                _nextNode.Position = CurrentNode.Roads.Dequeue();
        }
        else
            _nextNode.Position = CurrentNode.Roads.Dequeue();

        _nextNode.Direction = _nextNode.Position - CurrentNode.Position;
        _nextNode.PrevNode = CurrentNode;
        CurrentNode.Roads.Clear();

        NextNode = _nextNode;

        return true;
    }
}
