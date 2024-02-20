using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Vector3 _mapSize = new Vector3(28, 0, 31);

    private Vector3[] _energizerLocations;


    void Start()
    {
        NewGame();
    }


    public void NewGame()
    {
        //AddEnergizers();
        //AddFoods();
    }

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
                    GameObject energizer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    energizer.transform.position = currentPos;
                    energizer.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                    energizer.name = "Energizer";
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


                GameObject energizer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                energizer.transform.position = currentPos;
                energizer.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                energizer.name = "Food";

            }
        }
    }

}
