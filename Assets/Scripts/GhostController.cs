using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class GhostController : Unit
{
    private GameObject _pacmanObj;
    private Unit _pacmanUnit;
    private PacmanController _pacman;

    private MeshRenderer _unitRenderer;

    [SerializeField] private GhostType _ghostType;
    [SerializeField] private Material _NormalMat;
    [SerializeField] private Material _FrightenedMat;
    [SerializeField] private Material _EatenMat;
    [SerializeField] private GhostState _ghostState;
    [SerializeField] private GameObject _home;
    [SerializeField] private bool _isActive = false;
    [SerializeField] private int _wave = 1;
    [SerializeField] private float _waveTimer = 0;

    [SerializeField] private GhostState _prevState;
    [SerializeField] private float _frightenedTimer = 0;

    void Start()
    {
        InitilaizeUnit();
        _pacmanObj = GameObject.Find("Pacman");
        _pacman = _pacmanObj.GetComponent<PacmanController>();
        _pacmanUnit = _pacmanObj.GetComponent<Unit>();
        _unitRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (GameManager.Instance.State != GameState.Playing)
        {
            Agent.isStopped = true;
            return;
        }
        else
            Agent.isStopped = false;

        if (_isActive)
        {
            Movement();
            StateSwitcher();

            if (_ghostState == GhostState.Frightened)
                _unitRenderer.material = _FrightenedMat;
            else if (_ghostState == GhostState.Eaten)
                _unitRenderer.material = _EatenMat;
            else
                _unitRenderer.material = _NormalMat;

            EatOrGetEaten();
        }
        else
        {
            switch (_ghostType)
            {
                case GhostType.SpeedyPinky:
                    if (_pacman.EatenFood > 30)
                    {
                        Agent.Warp(new Vector3(14, 0.5f, -11f));
                        CurrentNode = new PathNode(transform);
                        NextNode = null;
                        _isActive = true;
                    }
                    break;
                case GhostType.BashfulInky:
                    if (_pacman.EatenFood > 50)
                    {
                        Agent.Warp(new Vector3(14, 0.5f, -11f));
                        CurrentNode = new PathNode(transform);
                        NextNode = null;
                        _isActive = true;
                    }
                    break;
                case GhostType.PokeyClyde:
                    if (_pacman.EatenFood > 80)
                    {
                        Agent.Warp(new Vector3(14, 0.5f, -11f));
                        CurrentNode = new PathNode(transform);
                        NextNode = null;
                        _isActive = true;
                    }
                    break;
            }
        }
    }

    public override void Movement()
    {
        //CheckPoint for enemy movement if enemy has path no need for further works
        if (Agent.hasPath)
            return;

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
    }

    public override void FindNode()
    {
        base.FindNode();
        if (CurrentNode.PrevNode != null)
            CurrentNode.OpenRoads.Remove(CurrentNode.PrevNode.Position);
    }

    public override bool GetRoad()
    {
        if (_ghostState == GhostState.Frightened)
            return GetFrightenedRoad();
        else if (_ghostState == GhostState.Eaten)
            return false;
        else if (_ghostState == GhostState.Chase || _ghostState == GhostState.Scatter)
            return GetRoadWithOriginalLogic();

        //This section currently not used in game
        switch (_ghostType)
        {
            case GhostType.ShadowBlinky:
                return GetShadowRoad();
            case GhostType.SpeedyPinky:
                return GetRoadWithOriginalLogic();
            case GhostType.BashfulInky:
                return GetRoadWithOriginalLogic();
            case GhostType.PokeyClyde:
                return GetRoadWithOriginalLogic();
        }

        return false;
    }

    public Vector3 GetTarget()
    {
        if (_ghostState == GhostState.Scatter)
            return _home.transform.position;
        else if (_ghostState == GhostState.Chase)
        {
            switch (_ghostType)
            {
                case GhostType.ShadowBlinky:
                    return GetShadowTarget();
                case GhostType.SpeedyPinky:
                    return GetSpeedyTarget();
                case GhostType.BashfulInky:
                    return GetBashfulTarget();
                case GhostType.PokeyClyde:
                    return GetPokeyTarget();
            }
        }
        else if (_ghostState == GhostState.Frightened)
        {

        }
        else if (_ghostState == GhostState.Eaten)
        {

        }

        return Vector3.zero;
    }

    public void Reset()
    {
        if (_ghostType == GhostType.ShadowBlinky)
            _isActive = true;
        else
            _isActive = false;

        _wave = 1;
        _waveTimer = 0;
        _ghostState = GhostState.Scatter;
        _frightenedTimer = 0;
    }

    public void Runaway()
    {
        if (_isActive && _ghostState != GhostState.Frightened && _ghostState != GhostState.Eaten)
        {
            Agent.speed = 2f;
            _frightenedTimer = 0;
            _prevState = _ghostState;
            _ghostState = GhostState.Frightened;
            NextNode = CurrentNode.PrevNode;
            NextNode.PrevNode = CurrentNode;
            NextNode.Direction = NextNode.Position - CurrentNode.Position;
            ExecuteNode();
        }
    }

    private void StateSwitcher()
    {
        if (_ghostState == GhostState.Frightened)
        {
            if (_frightenedTimer >= 10f)
            {
                Agent.speed = 3.5f;
                _ghostState = _prevState;
            }

            _frightenedTimer += Time.deltaTime;
            return;
        }
        else if (_ghostState == GhostState.Scatter || _ghostState == GhostState.Chase)
            _waveTimer += Time.deltaTime;
        else
            return;

        switch (_wave)
        {
            case 1:
                if (_ghostState == GhostState.Scatter)
                {
                    if (_waveTimer >= 7f)
                    {
                        _waveTimer = 0;
                        _ghostState = GhostState.Chase;
                        NextNode = CurrentNode.PrevNode;
                        NextNode.PrevNode = CurrentNode;
                        NextNode.Direction = NextNode.Position - CurrentNode.Position;
                        ExecuteNode();
                    }
                }
                else if (_ghostState == GhostState.Chase)
                {
                    if (_waveTimer >= 20f)
                    {
                        _waveTimer = 0;
                        _wave++;
                        _ghostState = GhostState.Scatter;
                        NextNode = CurrentNode.PrevNode;
                        NextNode.PrevNode = CurrentNode;
                        NextNode.Direction = NextNode.Position - CurrentNode.Position;
                        ExecuteNode();
                    }
                }
                return;
            case 2:
                if (_ghostState == GhostState.Scatter)
                {
                    if (_waveTimer >= 7f)
                    {
                        _waveTimer = 0;
                        _ghostState = GhostState.Chase;
                        NextNode = CurrentNode.PrevNode;
                        NextNode.PrevNode = CurrentNode;
                        NextNode.Direction = NextNode.Position - CurrentNode.Position;
                        ExecuteNode();
                    }
                }
                else if (_ghostState == GhostState.Chase)
                {
                    if (_waveTimer >= 20f)
                    {
                        _waveTimer = 0;
                        _wave++;
                        _ghostState = GhostState.Scatter;
                        NextNode = CurrentNode.PrevNode;
                        NextNode.PrevNode = CurrentNode;
                        NextNode.Direction = NextNode.Position - CurrentNode.Position;
                        ExecuteNode();
                    }
                }
                return;
            case 3:
                if (_ghostState == GhostState.Scatter)
                {
                    if (_waveTimer >= 5f)
                    {
                        _waveTimer = 0;
                        _ghostState = GhostState.Chase;
                        NextNode = CurrentNode.PrevNode;
                        NextNode.PrevNode = CurrentNode;
                        NextNode.Direction = NextNode.Position - CurrentNode.Position;
                        ExecuteNode();
                    }
                }
                else if (_ghostState == GhostState.Chase)
                {
                    if (_waveTimer >= 20f)
                    {
                        _waveTimer = 0;
                        _wave++;
                        _ghostState = GhostState.Scatter;
                        NextNode = CurrentNode.PrevNode;
                        NextNode.PrevNode = CurrentNode;
                        NextNode.Direction = NextNode.Position - CurrentNode.Position;
                        ExecuteNode();
                    }
                }
                return;
            case 4:
                if (_ghostState == GhostState.Scatter)
                {
                    if (_waveTimer >= 5f)
                    {
                        _waveTimer = 0;
                        _ghostState = GhostState.Chase;
                        NextNode = CurrentNode.PrevNode;
                        NextNode.PrevNode = CurrentNode;
                        NextNode.Direction = NextNode.Position - CurrentNode.Position;
                        ExecuteNode();
                    }
                }
                return;
        }
    }

    public void EatOrGetEaten()
    {
        if (_ghostState == GhostState.Chase || _ghostState == GhostState.Scatter)
        {
            var results = Physics.OverlapSphere(transform.position, 0.3f);
            if (results.Where(x => x.gameObject.name.Contains("Pacman")).Count() != 0)
            {
                _pacman.Dead();
            }
        }
        else if (_ghostState == GhostState.Frightened)
        {
            var results = Physics.OverlapSphere(transform.position, 0.3f);
            if (results.Where(x => x.gameObject.name.Contains("Pacman")).Count() != 0)
                _ghostState = GhostState.Eaten;
        }
    }

    #region Shadow - Blinky Movement
    private Vector3 GetShadowTarget() => _pacmanUnit.CurrentNode.Position;

    private bool GetShadowRoad()
    {
        CurrentNode.PrepareRoads();

        if (CurrentNode.OpenRoads.Count > 1)
        {
            ChooseShadowRoad();
        }
        else
        {
            PathNode _nextNode = new PathNode();
            _nextNode.Position = CurrentNode.Roads.Dequeue();
            _nextNode.Direction = _nextNode.Position - CurrentNode.Position;
            _nextNode.PrevNode = CurrentNode;

            NextNode = _nextNode;
        }
        return true;
    }

    //Improved Targeting algortihm
    private bool ChooseShadowRoad()
    {
        List<PathNode> _availableRoads = new List<PathNode>();

        CurrentNode.Cost = 0;
        _availableRoads.Add(CurrentNode);

        while (_availableRoads.Where(x => x.IsEndNode).Count() == 0)
        {
            List<PathNode> tmpList = new List<PathNode>();
            tmpList.AddRange(_availableRoads);

            foreach (var item in tmpList)
            {
                if (!item.IsEndNode)
                {
                    var result = SearchPath(item, _availableRoads, GetTarget());
                    _availableRoads.AddRange(result);
                }
            }
        }

        var shortestRoad = _availableRoads.Where(x => x.IsEndNode).OrderBy(x => x.Cost).First();

        while (shortestRoad.Cost != 0)
        {
            NextNode = shortestRoad;
            shortestRoad = shortestRoad.PrevNode;
        }

        return true;

    }
    #endregion

    #region Speedy - Pinky Movement
    private Vector3 GetSpeedyTarget() => _pacmanUnit.CurrentNode.Position + (_pacmanUnit.CurrentNode.Direction * 4);
    #endregion

    #region Bashful - Inky Movement
    private Vector3 GetBashfulTarget()
    {
        GhostController blinkyObj = GameObject.Find("Shadow - Blinky").GetComponent<GhostController>();
        Vector3 offset = (_pacmanUnit.CurrentNode.Position + (_pacmanUnit.CurrentNode.Direction * 2)) - blinkyObj.CurrentNode.Position;
        offset += (_pacmanUnit.CurrentNode.Position + (_pacmanUnit.CurrentNode.Direction * 2));
        return offset;
    }
    #endregion

    #region Pokey - Clyde Movement
    private Vector3 GetPokeyTarget()
    {
        if (PokeyDistanceToPacman() < 8)
            return _home.transform.position;
        else
            return _pacmanUnit.CurrentNode.Position;
    }

    private int PokeyDistanceToPacman()
    {
        List<PathNode> _availableRoads = new List<PathNode>();

        CurrentNode.Cost = 0;
        _availableRoads.Add(CurrentNode);

        while (_availableRoads.Where(x => x.IsEndNode).Count() == 0)
        {
            List<PathNode> tmpList = new List<PathNode>();
            tmpList.AddRange(_availableRoads);

            foreach (var item in tmpList)
            {
                if (!item.IsEndNode)
                {
                    var result = SearchPath(item, _availableRoads, _pacmanUnit.CurrentNode.Position);
                    _availableRoads.AddRange(result);
                }
            }
        }

        var shortestRoad = _availableRoads.Where(x => x.IsEndNode).OrderBy(x => x.Cost).First();

        return shortestRoad.Cost;
    }
    #endregion

    //Original logic for obtaining road usable on all ghost types
    private bool GetRoadWithOriginalLogic()
    {
        CurrentNode.PrepareRoads();

        float distance = float.PositiveInfinity;
        PathNode _nextNode = new PathNode();

        foreach (var item in CurrentNode.OpenRoads)
        {
            float currentDistance = Vector3.Distance(GetTarget(), item);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                _nextNode.Position = item;
                _nextNode.Direction = _nextNode.Position - CurrentNode.Position;
                _nextNode.PrevNode = CurrentNode;
            }
        }

        NextNode = _nextNode;

        return true;
    }

    private bool GetFrightenedRoad()
    {
        CurrentNode.RandomizeRoads();
        CurrentNode.PrepareRoads();

        PathNode _nextNode = new PathNode();
        _nextNode.Position = CurrentNode.Roads.Dequeue();
        _nextNode.Direction = _nextNode.Position - CurrentNode.Position;
        _nextNode.PrevNode = CurrentNode;

        NextNode = _nextNode;
        return true;
    }

    //Method used to get shortest path to reach target
    private List<PathNode> SearchPath(PathNode node, List<PathNode> roadList, Vector3 goalLocation)
    {
        List<PathNode> _availableRoads = new List<PathNode>();

        foreach (var item in PathNode.MainDirections)
        {
            if (node.PrevNode != null)
            {
                if (node.PrevNode.Position == node.Position + item)
                    continue;
            }

            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(node.Position, node.Position + item, NavMesh.AllAreas, path);
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                PathNode subNode = new PathNode();
                subNode.Position = node.Position + item;
                subNode.PrevNode = node;
                subNode.Direction = subNode.Position - subNode.PrevNode.Position;
                subNode.Cost += subNode.PrevNode.Cost;
                if (subNode.Position == goalLocation)
                    subNode.IsEndNode = true;
                _availableRoads.Add(subNode);
            }
        }

        roadList.Remove(node);

        return _availableRoads;
    }
}

public enum GhostType
{
    ShadowBlinky,
    SpeedyPinky,
    BashfulInky,
    PokeyClyde
}

public enum GhostState
{
    Chase,
    Scatter,
    Frightened,
    Eaten
}