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

    private int _eatenFood = 0;
    public int EatenFood { get { return _eatenFood; } }


    void Start()
    {
        InitilaizeUnit();
        CurrentNode.Direction = Vector3.right;
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

        _scoreText.text = "Score: " + _score;
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

    public void ResetScore()
    {
        _score = 0;
        _eatenFood = 0;
    }

    public void EatFood()
    {
        var results = Physics.OverlapSphere(transform.position, 0.3f);
        if (results.Where(x => x.gameObject.name.Contains("Food")).Count() != 0)
        {
            GameObject food = results.Where(x => x.gameObject.name.Contains("Food")).First().gameObject;
            Destroy(food);
            _score += 10;
            _eatenFood++;
        }
    }
}
