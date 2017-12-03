using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    public int rows, cols;
    public int startRow, startCol;
    public int endRow, endCol;

    public GameObject cellPrefab;

    public Transform[] heartsObjs;

    public Sprite emptyHeart;
    public Sprite fillHeart;

    private Level[] _levels;
    private int _currentLevel;
    private CellController[,] _cells;
    private Position _position;
    private int _hearts;
    private Position? _lastDir;

    private bool _finished;

    private Stack<GameState> _states;

    void Start()
    {
        _levels = GetLevels();
        _currentLevel = 0;

        InitLevel();
    }

    void InitLevel()
    {
        if (_cells != null)
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    GameObject.Destroy(_cells[i, j].gameObject);
                }
            }

            rows = cols = 0;
            startRow = startCol = 0;
            endRow = endCol = 0;
            _cells = null;
        }

        _states = new Stack<GameState>();

        var level = _levels[_currentLevel];

        rows = level.cells.GetLength(0);
        cols = level.cells.GetLength(1);

        _cells = new CellController[rows, cols];
        _hearts = 0;

        for (int i = 0; i < 6; i++)
        {
            var image = heartsObjs[i].GetComponent<Image>();
            image.sprite = emptyHeart;
            image.enabled = (i < level.hearts);
        }

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var cell = GameObject.Instantiate(cellPrefab);
                var cellController = cell.GetComponent<CellController>();
                cellController.transform.position = new Vector3(j, rows - 1 - i, 0.0f);
                cellController.transform.parent = gameObject.transform;
                cellController.type = (CellType)level.cells[i, j];
                cellController.changed = true;

                if (level.cells[i, j] == (int)CellType.Player)
                {
                    cellController.hasStairs = true;
                    _position = new Position(i, j);
                }
                else if (level.cells[i, j] == (int)CellType.End)
                {
                    cellController.hasStairs = true;
                }

                _cells[i, j] = cellController;
            }
        }

        PushState();

        var camera = Camera.main;
        camera.transform.position = new Vector3(cols * 0.5f - 0.5f, rows * 0.5f - 0.5f, -10.0f);
    }

    void Update()
    {
        if (_finished)
        {
            return;
        }

        var zDown = Input.GetKeyDown(KeyCode.Z);
        var upDown = Input.GetKeyDown(KeyCode.UpArrow);
        var rightDown = Input.GetKeyDown(KeyCode.RightArrow);
        var downDown = Input.GetKeyDown(KeyCode.DownArrow);
        var leftDown = Input.GetKeyDown(KeyCode.LeftArrow);

        if (zDown && _lastDir != null)
        {
            TryGoInDir(Position.Inverse(_lastDir.Value));
        }

        if (upDown)
        {
            TryGoInDir(Position.Up);
        }

        if (rightDown)
        {
            TryGoInDir(Position.Right);
        }

        if (downDown)
        {
            TryGoInDir(Position.Down);
        }

        if (leftDown)
        {
            TryGoInDir(Position.Left);
        }
    }

    void TryGoInDir(Position dir)
    {
        var currentCell = _cells[_position.r, _position.c];
        var newPosition = _position + dir;
        if (IsInside(newPosition))
        {
            if (_lastDir != null && _lastDir == Position.Inverse(dir))
            {
                PopState();
                return;
            }

            var newCell = _cells[newPosition.r, newPosition.c];
            Debug.Log(newCell.type);
            switch (newCell.type)
            {
                case CellType.Empty:
                {
                    PushState();

                    SetCurrentCellPath(currentCell, dir);
                    currentCell.changed = true;

                    _position = newPosition;

                    newCell.type = CellType.Player;
                    newCell.changed = true;

                    _lastDir = dir;
                    break;
                }
                case CellType.Heart:
                {
                    PushState();

                    SetCurrentCellPath(currentCell, dir);
                    currentCell.changed = true;

                    _position = newPosition;
                    _hearts++;

                    var image = heartsObjs[_hearts - 1].GetComponent<Image>();
                    image.sprite = fillHeart;

                    newCell.type = CellType.Player;
                    newCell.changed = true;

                    var monsterPos = newPosition + dir;
                    if (IsInside(monsterPos))
                    {
                        var monsterCell = _cells[monsterPos.r, monsterPos.c];
                        if (monsterCell.type == CellType.Empty)
                        {
                            monsterCell.type = CellType.Monster;
                            monsterCell.changed = true;
                        }
                    }

                    _lastDir = dir;
                    break;
                }
                case CellType.Monster:
                {
                    PushState();

                    SetCurrentCellPath(currentCell, dir);
                    currentCell.changed = true;

                    _position = newPosition;
                    _hearts--;
                    Debug.Log("Lives: " + _hearts);

                    newCell.type = CellType.Player;
                    newCell.changed = true;

                    _lastDir = dir;
                    break;
                }
                case CellType.End:
                {
                    PushState();

                    SetCurrentCellPath(currentCell, dir);
                    currentCell.changed = true;

                    _position = newPosition;

                    newCell.type = CellType.Player;
                    newCell.changed = true;

                    _lastDir = dir;

                    var level = _levels[_currentLevel];
                    if (_hearts == level.hearts)
                    {
                        _currentLevel++;
                        if (_currentLevel >= _levels.Length)
                        {
                            _finished = true;
                            return;
                        }

                        InitLevel();
                    }
                    break;
                }
            }
        }
    }

    void SetCurrentCellPath(CellController cell, Position dir)
    {
        if (_lastDir != null)
        {
            if (dir == Position.Right)
            {
                if (_lastDir.Value == Position.Right)
                {
                    cell.type = CellType.PathH;
                }
                else if (_lastDir.Value == Position.Up)
                {
                    cell.type = CellType.Path11;
                }
                else if (_lastDir.Value == Position.Down)
                {
                    cell.type = CellType.Path01;
                }
            }
            else if (dir == Position.Up)
            {
                if (_lastDir.Value == Position.Up)
                {
                    cell.type = CellType.PathV;
                }
                else if (_lastDir.Value == Position.Right)
                {
                    cell.type = CellType.Path00;
                }
                else if (_lastDir.Value == Position.Left)
                {
                    cell.type = CellType.Path01;
                }
            }
            else if (dir == Position.Left)
            {
                if (_lastDir.Value == Position.Left)
                {
                    cell.type = CellType.PathH;
                }
                else if (_lastDir.Value == Position.Up)
                {
                    cell.type = CellType.Path10;
                }
                else if (_lastDir.Value == Position.Down)
                {
                    cell.type = CellType.Path00;
                }
            }
            else if (dir == Position.Down)
            {
                if (_lastDir.Value == Position.Down)
                {
                    cell.type = CellType.PathV;
                }
                else if (_lastDir.Value == Position.Right)
                {
                    cell.type = CellType.Path10;
                }
                else if (_lastDir.Value == Position.Left)
                {
                    cell.type = CellType.Path11;
                }
            }
        }
        else
        {
            cell.type = CellType.Start;
        }
    }

    void PushState()
    {
        var state = new GameState()
        {
            cells = new CellType[rows, cols],
            hearts = _hearts,
            player = _position,
            lastDir = _lastDir
        };
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                state.cells[i, j] = _cells[i, j].type;
            }
        }

        _states.Push(state);
    }

    void PopState()
    {
        if (_states.Count > 0)
        {
            var state = _states.Pop();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    _cells[i, j].type = state.cells[i, j];
                    _cells[i, j].changed = true;
                }
            }

            _hearts = state.hearts;
            _lastDir = state.lastDir;
            _position = state.player;

            var level = _levels[_currentLevel];
            for (int i = 0; i < 6; i++)
            {
                var image = heartsObjs[i].GetComponent<Image>();
                image.sprite = i < _hearts ? fillHeart : emptyHeart;
                image.enabled = (i < level.hearts);
            }
        }
    }

    bool IsInside(Position pos)
    {
        return IsInside(pos.r, pos.c);
    }

    bool IsInside(int row, int col)
    {
        return row >= 0 && row < rows && col >= 0 && col < cols;
    }

    Level[] GetLevels()
    {
        return new Level[]
        {
            new Level()
            {
                cells = new int[3,3]
                {
                    { 10, 0, 0 },
                    { 0, 8, 0 },
                    { 0, 0, 12 },
                },
                hearts = 1
            },
            new Level()
            {
                cells = new int[4, 4]
                {
                    { 10, 0, 0, 0 },
                    { 0, 0, 0, 0 },
                    { 0, 8, 0, 0 },
                    { 0, 0, 8, 12 },
                },
                hearts = 2
            },
            new Level()
            {
                cells = new int[4, 4]
                {
                    { 0, 10, 0, 0 },
                    { 0, 0, 0, 8 },
                    { 0, 8, 0, 0 },
                    { 0, 0, 8, 12 },
                },
                hearts = 3
            },
            new Level()
            {
                cells = new int[5, 5]
                {
                    { 10, 0, 0, 0, 0 },
                    { 0, 0, 0, 0, 0 },
                    { 0, 8, 0, 8, 0 },
                    { 0, 0, 0, 0, 0 },
                    { 0, 8, 0, 8, 12 },
                },
                hearts = 4
            },
            new Level()
            {
                cells = new int[5, 5]
                {
                    { 10, 0, 0, 0, 0 },
                    { 0, 0, 0, 0, 0 },
                    { 0, 8, 0, 8, 0 },
                    { 0, 0, 8, 0, 0 },
                    { 0, 8, 0, 8, 12 },
                },
                hearts = 4
            },
            new Level()
            {
                cells = new int[5, 5]
                {
                    { 10, 0, 8, 0, 0 },
                    { 0, 0, 0, 8, 0 },
                    { 0, 8, 0, 0, 8 },
                    { 0, 0, 0, 0, 0 },
                    { 0, 0, 8, 0, 12 },
                },
                hearts = 4
            }
        };
    }
}

public enum CellType
{
    Empty = 0,
    Block = 1,
    PathH = 2,
    PathV = 3,
    Path00 = 4,
    Path01 = 5,
    Path10 = 6,
    Path11 = 7,
    Heart = 8,
    Monster = 9,
    Player = 10,
    Start = 11,
    End = 12
}

public struct GameState
{
    public CellType[,] cells;
    public int hearts;
    public Position player;
    public Position? lastDir;
}

public struct Level
{
    public int[,] cells;
    public int hearts;
}
