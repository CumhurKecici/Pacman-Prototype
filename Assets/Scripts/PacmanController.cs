using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInput))]
public class PacmanController : Unit
{

    private PlayerInput _inputController;

    [SerializeField] private int _score = 0;
    private Text _scoreText;

    private bool _isDead = false;
    private int _eatenFood = 0;
    public int EatenFood { get { return _eatenFood; } }


    void Start()
    {
        InitilaizeUnit();
        CurrentNode.Direction = Vector3.left;
        _inputController = GetComponent<PlayerInput>();
        _scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
    }

    void Update()
    {
        if (GameManager.Instance.State != GameState.Playing)
        {
            Agent.isStopped = true;
            return;
        }
        else
        {
            Agent.isStopped = false;
        }

        Movement();
        EatFood();
        EatEnergizer();

        _scoreText.text = "Score: " + _score;
    }

    public override void Movement()
    {
        if (_isDead)
            return;
        if (Agent.hasPath)
        {
            var direction = _inputController.actions.FindAction("Move").ReadValue<Vector3>();
            direction = Vector3Int.RoundToInt(direction);

            if (direction + CurrentNode.Direction == Vector3.zero)
            {
                NextNode = CurrentNode.PrevNode;
                NextNode.PrevNode = CurrentNode;
                NextNode.Direction = -CurrentNode.Direction;
                ExecuteNode();
            }
            return;
        }

        if (!Agent.hasPath)
        {
            if (CurrentNode.Position == LeftGate)
            {
                CurrentNode.Position = RightGate;
                NextNode = new PathNode();
                NextNode.Position = CurrentNode.Position + CurrentNode.Direction;
                NextNode.PrevNode = CurrentNode;
                NextNode.Direction = NextNode.Position - CurrentNode.Position;
                Agent.Warp(RightGate);
                ExecuteNode();
            }
            else if (CurrentNode.Position == RightGate)
            {
                CurrentNode.Position = LeftGate;
                NextNode = new PathNode();
                NextNode.Position = CurrentNode.Position + CurrentNode.Direction;
                NextNode.PrevNode = CurrentNode;
                NextNode.Direction = NextNode.Position - CurrentNode.Position;
                Agent.Warp(LeftGate);
                ExecuteNode();
            }
        }

        FindNode();
        if (GetRoad())
            ExecuteNode();
        else
        {
            ContinueDirectionIfPossible();
            ExecuteNode();
        }
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

    public void ContinueDirectionIfPossible()
    {
        NextNode = new PathNode();
        NextNode.Position = CurrentNode.Position + CurrentNode.Direction;
        NextNode.PrevNode = CurrentNode;
        NextNode.Direction = NextNode.Position - CurrentNode.Position;

        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(CurrentNode.Position, NextNode.Position, NavMesh.AllAreas, path);
        if (path.status != NavMeshPathStatus.PathComplete)
            NextNode = null;
    }

    public void Reset()
    {
        _score = 0;
        _eatenFood = 0;
        _isDead = false;
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = true;
    }

    public void Dead()
    {
        _isDead = true;
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
    }

    public void EatFood()
    {
        var results = Physics.OverlapSphere(transform.position, 0.3f);
        if (results.Where(x => x.gameObject.name.Contains("Food") && !x.gameObject.name.Contains("Zone")).Count() != 0)
        {
            GameObject food = results.Where(x => x.gameObject.name.Contains("Food") && !x.gameObject.name.Contains("Zone")).First().gameObject;
            Destroy(food);
            _score += 10;
            _eatenFood++;
        }
    }

    public void EatEnergizer()
    {
        var results = Physics.OverlapSphere(transform.position, 0.3f);
        if (results.Where(x => x.gameObject.name.Contains("Energizer")).Count() != 0)
        {
            GameObject energizer = results.Where(x => x.gameObject.name.Contains("Energizer")).First().gameObject;
            Destroy(energizer);
            _score += 100;

            GhostController[] ghosts = GameObject.FindObjectsOfType<GhostController>();
            foreach (var item in ghosts)
                item.Runaway();
        }
    }
}
