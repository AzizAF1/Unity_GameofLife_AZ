using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Game_Board : MonoBehaviour
{
    [SerializeField] private Tilemap current_state;
    [SerializeField] private Tilemap next;
    [SerializeField] private Tile Alive_tile;
    [SerializeField] private Tile Dead_tile;
    [SerializeField] private float updateInterval = 0.05f;

    private HashSet<Vector3Int> aliveCells;
    private HashSet<Vector3Int> CellsToCheck;

    private bool isSimulating = false; //  flag to control simulation start

    private void Awake()
    {
        aliveCells = new HashSet<Vector3Int>();
        CellsToCheck = new HashSet<Vector3Int>();
    }

    private void Start()
    {
        // Initially needs to be cleared before the input of the user
        Clear(); 
    }

    private void Update()
    {
        if (!isSimulating)
        {
            // until user gives input : 
            HandleUserInput(); 

            // Start after input spacebar
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isSimulating = true;
                StartCoroutine(Simulate());
            }
        }
    }

    // User input handling
    private void HandleUserInput()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = current_state.WorldToCell(worldPosition);

            if (current_state.GetTile(cellPosition) == Alive_tile)
            {
                // If the cell is alive, toggle to dead
                current_state.SetTile(cellPosition, Dead_tile);
                aliveCells.Remove(cellPosition);
            }
            else
            {
                // If the cell is dead, toggle to alive
                current_state.SetTile(cellPosition, Alive_tile);
                aliveCells.Add(cellPosition);
            }
        }
    }

    private void Clear()
    {
        current_state.ClearAllTiles();
        next.ClearAllTiles();
        aliveCells.Clear(); 
    }

    private IEnumerator Simulate()
    {
        var interval = new WaitForSeconds(updateInterval);
        while (isSimulating)
        {
            UpdateState();
            yield return interval;
        }
    }

    private void UpdateState()
    {
        CellsToCheck.Clear();

        foreach (Vector3Int cell in aliveCells)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    CellsToCheck.Add(cell + new Vector3Int(x, y, 0));
                }
            }
        }

        foreach (Vector3Int cell in CellsToCheck)
        {
            int neighbors = CountNeighbours(cell);
            bool alive = IsAlive(cell);

            if (!alive && neighbors == 3)
            {
                next.SetTile(cell, Alive_tile);
                aliveCells.Add(cell);
            }
            else if (alive && (neighbors < 2 || neighbors > 3))
            {
                next.SetTile(cell, Dead_tile);
                aliveCells.Remove(cell);
            }
            else
            {
                next.SetTile(cell, current_state.GetTile(cell));
            }
        }

        Tilemap temp = current_state;
        current_state = next;
        next = temp;
        next.ClearAllTiles();
    }

    private int CountNeighbours(Vector3Int cell)
    {
        int count = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int neighbor = cell + new Vector3Int(x, y, 0);
                if (x == 0 && y == 0)
                {
                    continue;
                }
                else if (IsAlive(neighbor))
                {
                    count++;
                }
            }
        }
        return count;
    }

    private bool IsAlive(Vector3Int cell)
    {
        return current_state.GetTile(cell) == Alive_tile;
    }
}
