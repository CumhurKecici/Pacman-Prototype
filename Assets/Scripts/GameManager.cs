using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance { get { return _instance; } }

    private Vector3 _mapSize = new Vector3(28, 0, 31);
    private Vector3[] _energizerLocations;

    [SerializeField] private GameObject _food;
    [SerializeField] private GameObject _energizer;

    public GameState State;

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            State = GameState.Paused;
    }

    public void NewGame()
    {
        AddEnergizers();
        AddFoods();
        State = GameState.Playing;
    }

    public void Restart()
    {
        var units = GameObject.FindObjectsOfType<Unit>();
        foreach (var item in units)
        {
            item.Agent.ResetPath();
            if (item.gameObject.name.Contains("Pacman"))
            {
                item.Agent.Warp(item.StartLocation);
                item.CurrentNode = new Unit.PathNode(item.transform);
                item.CurrentNode.Direction = Vector3.right;
                item.NextNode = null;
                item.gameObject.GetComponent<PacmanController>().ResetScore();
            }
            else
            {
                item.Agent.Warp(item.StartLocation);
                item.CurrentNode = new Unit.PathNode(item.transform);
                item.NextNode = null;
                if (item.gameObject.name.Contains("Shadow"))
                    item.gameObject.GetComponent<GhostController>().IsActive = true;
                else
                    item.gameObject.GetComponent<GhostController>().IsActive = false;
            }
        }

        NewGame();
    }

    public void Unpause() => State = GameState.Playing;

    private void AddEnergizers()
    {
        _energizerLocations = new Vector3[] {
            new Vector3(1, 0.5f, -3),
            new Vector3(27, 0.5f, -3),
            new Vector3(1, 0.5f, -23),
            new Vector3(27, 0.5f, -23)
        };

        for (int i = 0; i < _mapSize.x; i++)
        {
            for (int j = 0; j < _mapSize.z; j++)
            {
                var currentPos = new Vector3(i, 0.5f, -j);
                if (_energizerLocations.Contains(currentPos))
                {
                    Instantiate(_energizer, currentPos, Quaternion.identity);

                    //GameObject energizer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //energizer.transform.position = currentPos;
                    //energizer.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                    //energizer.GetComponent<MeshRenderer>().material.color = new Color(204, 153, 102);
                    //energizer.name = "Energizer";
                }
            }
        }
    }

    private void AddFoods()
    {
        BoxCollider[] _zones = GameObject.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None);

        for (int i = 0; i < _mapSize.x; i++)
        {
            for (int j = 0; j < _mapSize.z; j++)
            {
                var currentPos = new Vector3(i, 0.5f, -j);
                if (_energizerLocations.Contains(currentPos))
                    continue;

                if (_zones.Where(x => x.bounds.Contains(currentPos)).Count() != 0)
                    continue;


                Instantiate(_food, currentPos, Quaternion.identity);
                //GameObject food = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //food.transform.position = currentPos;
                //food.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                //food.GetComponent<MeshRenderer>().material.color = new Color(204, 153, 102);
                //food.name = "Food";

            }
        }
    }

}

public enum GameState
{
    OnMenu,
    Playing,
    Paused
}
