using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.PathFinder.PathFinder
{
    public static class BestFirstSearch
    {
        #region Methods

        public static List<Node> FindPath(GridPos start, GridPos end, GridPos[,] Grid)
        {
            if (Grid.GetLength(0) < start.X || Grid.GetLength(1) < start.Y || start.X < 0 || start.Y < 0)
            {
                return new List<Node>();
            }
            Node[,] grid = new Node[Grid.GetLength(0), Grid.GetLength(1)];
            if (grid[start.X, start.Y] == null)
            {
                grid[start.X, start.Y] = new Node(Grid[start.X, start.Y]);
            }
            Node Start = grid[start.X, start.Y];
            MinHeap path = new MinHeap();

            // push the start node into the open list
            path.Push(Start);
            Start.Opened = true;

            // while the open list is not empty
            while (path.Count > 0)
            {
                // pop the position of node which has the minimum `f` value.
                Node node = path.Pop();
                if (grid[node.X, node.Y] == null)
                {
                    grid[node.X, node.Y] = new Node(Grid[node.X, node.Y]);
                }
                grid[node.X, node.Y].Closed = true;

                //if reached the end position, construct the path and return it
                if (node.X == end.X && node.Y == end.Y)
                {
                    return Backtrace(node);
                }

                // get neigbours of the current node
                List<Node> neighbors = GetNeighbors(grid, node, Grid);

                for (int i = 0, l = neighbors.Count; i < l; ++i)
                {
                    Node neighbor = neighbors[i];

                    if (neighbor.Closed)
                    {
                        continue;
                    }

                    // check if the neighbor has not been inspected yet, or can be reached with
                    // smaller cost from the current node
                    if (neighbor.Opened)
                    {
                        continue;
                    }
                    if (neighbor.F == 0)
                    {
                        neighbor.F = Heuristic.Octile(Math.Abs(neighbor.X - end.X), Math.Abs(neighbor.Y - end.Y));
                    }

                    neighbor.Parent = node;

                    if (!neighbor.Opened)
                    {
                        path.Push(neighbor);
                        neighbor.Opened = true;
                    }
                    else
                    {

                        neighbor.Parent = node;
                    }
                }
            }
            return new List<Node>();
        }

        public static Node[,] LoadBrushFire(GridPos user, GridPos[,] mapGrid, short MaxDistance = 22)

        {
            Node[,] grid = new Node[mapGrid.GetLength(0), mapGrid.GetLength(1)];

            if (grid[user.X, user.Y] == null)
            {
                grid[user.X, user.Y] = new Node(mapGrid[user.X, user.Y]);
            }
            Node start = grid[user.X, user.Y];
            MinHeap path = new MinHeap();


            // push the start node into the open list
            path.Push(start);
            start.Opened = true;

            // while the open list is not empty
            while (path.Count > 0)
            {
                // pop the position of node which has the minimum `f` value.
                Node node = path.Pop();
                if (grid[node.X, node.Y] == null)
                {
                    grid[node.X, node.Y] = new Node(mapGrid[node.X, node.Y]);
                }

                grid[node.X, node.Y].Closed = true;

                // get neighbors of the current node
                List<Node> neighbors = GetNeighbors(grid, node, mapGrid);

                for (int i = 0, l = neighbors.Count; i < l; ++i)
                {
                    Node neighbor = neighbors[i];

                    if (neighbor.Closed)
                    {
                        continue;
                    }

                    // check if the neighbor has not been inspected yet, or can be reached with
                    // smaller cost from the current node
                    if (neighbor.Opened)
                    {
                        continue;
                    }
                    if (neighbor.F == 0)
                    {
                        double distance = Heuristic.Octile(Math.Abs(neighbor.X - node.X), Math.Abs(neighbor.Y - node.Y)) + node.F;
                        if (distance > MaxDistance)
                        {
                            neighbor.Value = 1;
                            continue;
                        }
                        else
                        {
                            neighbor.F = distance;
                        }
                        grid[neighbor.X, neighbor.Y].F = neighbor.F;
                    }

                    neighbor.Parent = node;

                    if (!neighbor.Opened)
                    {
                        path.Push(neighbor);
                        neighbor.Opened = true;
                    }
                    else
                    {
                        neighbor.Parent = node;
                    }
                }
            }
            return grid;
        }

        public static List<Node> GetNeighbors(Node[,] grid, Node node, GridPos[,] mapGrid)
        {
            short x = node.X,
                y = node.Y;
            List<Node> neighbors = new List<Node>();
            bool s0 = false,
                s1 = false,
                s2 = false,
                s3 = false;

            // ↑
            int indexX = x;
            int indexY = y - 1;
            if (grid.GetLength(0) > indexX && grid.GetLength(1) > indexY && indexX >= 0 && indexY >= 0 && mapGrid[indexX, indexY].IsWalkable())
            {
                if (grid[indexX, indexY] == null)
                {
                    grid[indexX, indexY] = new Node(mapGrid[indexX, indexY]);
                }
                neighbors.Add(grid[indexX, indexY]);
                s0 = true;
            }

            // →
            indexX = x + 1;
            indexY = y;
            if (grid.GetLength(0) > indexX && grid.GetLength(1) > indexY && indexX >= 0 && indexY >= 0 && mapGrid[indexX, indexY].IsWalkable())
            {
                if (grid[indexX, indexY] == null)
                {
                    grid[indexX, indexY] = new Node(mapGrid[indexX, indexY]);
                }
                neighbors.Add(grid[indexX, indexY]);
                s1 = true;
            }

            // ↓
            indexX = x;
            indexY = y + 1;
            if (grid.GetLength(0) > indexX && grid.GetLength(1) > indexY && indexX >= 0 && indexY >= 0 && mapGrid[indexX, indexY].IsWalkable())
            {
                if (grid[indexX, indexY] == null)
                {
                    grid[indexX, indexY] = new Node(mapGrid[indexX, indexY]);
                }
                neighbors.Add(grid[indexX, indexY]);
                s2 = true;
            }

            // ←
            indexX = x - 1;
            indexY = y;
            if (grid.GetLength(0) > indexX && grid.GetLength(1) > indexY && indexX >= 0 && indexY >= 0 && mapGrid[indexX, indexY].IsWalkable())
            {
                if (grid[indexX, indexY] == null)
                {
                    grid[indexX, indexY] = new Node(mapGrid[indexX, indexY]);
                }
                neighbors.Add(grid[indexX, indexY]);
                s3 = true;
            }

            bool d0 = s3 || s0;
            bool d1 = s0 || s1;
            bool d2 = s1 || s2;
            bool d3 = s2 || s3;

            // ↖
            indexX = x - 1;
            indexY = y - 1;
            if (grid.GetLength(0) > indexX && grid.GetLength(1) > indexY && indexX >= 0 && indexY >= 0 && d0 && mapGrid[indexX, indexY].IsWalkable())
            {
                if (grid[indexX, indexY] == null)
                {
                    grid[indexX, indexY] = new Node(mapGrid[indexX, indexY]);
                }
                neighbors.Add(grid[indexX, indexY]);
            }

            // ↗
            indexX = x + 1;
            indexY = y - 1;
            if (grid.GetLength(0) > indexX && grid.GetLength(1) > indexY && indexX >= 0 && indexY >= 0 && d1 && mapGrid[indexX, indexY].IsWalkable())
            {
                if (grid[indexX, indexY] == null)
                {
                    grid[indexX, indexY] = new Node(mapGrid[indexX, indexY]);
                }
                neighbors.Add(grid[indexX, indexY]);
            }

            // ↘
            indexX = x + 1;
            indexY = y + 1;
            if (grid.GetLength(0) > indexX && grid.GetLength(1) > indexY && indexX >= 0 && indexY >= 0 && d2 && mapGrid[indexX, indexY].IsWalkable())
            {
                if (grid[indexX, indexY] == null)
                {
                    grid[indexX, indexY] = new Node(mapGrid[indexX, indexY]);
                }
                neighbors.Add(grid[indexX, indexY]);
            }

            // ↙
            indexX = x - 1;
            indexY = y + 1;
            if (grid.GetLength(0) <= indexX || grid.GetLength(1) <= indexY || indexX < 0 || indexY < 0 || !d3 || !mapGrid[indexX, indexY].IsWalkable())
            {
                return neighbors;
            }
            if (grid[indexX, indexY] == null)
            {
                grid[indexX, indexY] = new Node(mapGrid[indexX, indexY]);
            }
            neighbors.Add(grid[indexX, indexY]);

            return neighbors;
        }

        public static List<Node> Backtrace(Node end)
        {
            List<Node> path = new List<Node>();
            while (end.Parent != null)
            {
                end = end.Parent;
                path.Add(end);
            }
            path.Reverse();
            return path;
        }

        public static List<Node> TracePath(Node node, Node[,] Grid, GridPos[,] MapGrid)
        {
            List<Node> list = new List<Node>();
            if (MapGrid == null || Grid == null || node.X >= Grid.GetLength(0) || node.Y >= Grid.GetLength(1) || node.X < 0 || node.Y < 0 || Grid[node.X, node.Y] == null)
            {
                node.F = 100;
                list.Add(node);
                return list;
            }
            Node currentnode = Grid[node.X, node.Y];
            while (currentnode.F != 1 && currentnode.F != 0)
            {
                Node newnode = GetNeighbors(Grid, currentnode, MapGrid)?.OrderBy(s => s.F).FirstOrDefault();
                if (newnode == null)
                {
                    continue;
                }
                list.Add(newnode);
                currentnode = newnode;
            }
            return list;
        }
        #endregion
    }
}