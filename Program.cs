using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    public static readonly int worldSize = 10;
    static readonly object[,] world = new object[worldSize, worldSize];
    static readonly Random random = new Random();
    static bool isRunning = true;

    static void Main()
    {
        List<Ant> ants = new List<Ant>
        {
            new Ant(3, 3, "Worker"),
            new Ant(5, 7, "Builder"),
            new Ant(6, 6, "QueenCaretaker")
        };

        List<Task> tasks = new List<Task>();
        foreach (var ant in ants)
        {
            tasks.Add(Task.Run(() => ant.Live()));
        }

        while (isRunning)
        {
            RenderWorld(ants);
            Thread.Sleep(1000);

            // Проверка на умерших муравьев и их замена через 3 обновления
            for (int i = 0; i < ants.Count; i++)
            {
                if (!ants[i].IsAlive)
                {
                    ants[i].deathTickCount++;

                    if (ants[i].deathTickCount >= 3)
                    {
                        Console.WriteLine($"Создание нового муравья {ants[i].Role}.");

                        // Создаем нового муравья того же типа
                        var newAnt = new Ant(random.Next(0, worldSize), random.Next(0, worldSize), ants[i].Role);
                        ants[i] = newAnt;

                        // Добавляем новую задачу для нового муравья в список задач
                        tasks.Add(Task.Run(() => newAnt.Live()));
                    }
                }
            }
        }

        Task.WaitAll(tasks.ToArray());
    }

    static void RenderWorld(List<Ant> ants)
    {
        Console.Clear();
        char[,] render = new char[worldSize, worldSize];

        for (int i = 0; i < worldSize; i++)
        {
            for (int j = 0; j < worldSize; j++)
            {
                render[i, j] = '*';
            }
        }

        foreach (var ant in ants)
        {
            if (ant.IsAlive)
                render[ant.X, ant.Y] = '8';  // 8 для живого
            else
                render[ant.X, ant.Y] = 'X';  // X для мертвого
        }

        for (int i = 0; i < worldSize; i++)
        {
            for (int j = 0; j < worldSize; j++)
            {
                char symbol = render[i, j];

                if (symbol == '8')
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;  // Живой муравей - желтый
                }
                else if (symbol == 'X')
                {
                    Console.ForegroundColor = ConsoleColor.Red;     // Мертвый муравей - красный
                }
                else
                {
                    Console.ResetColor();
                }

                Console.Write(symbol + " ");
            }
            Console.WriteLine();
        }

        Console.ResetColor();
    }

}

class Ant
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public string Role { get; }
    public int Energy { get; private set; } = 100;
    public int Age { get; private set; } = 0;
    public bool IsAlive { get; private set; } = true;
    public int deathTickCount { get; set; } = 0;  // Счётчик с момента смерти

    private static readonly Random random = new Random();
    private static readonly object locker = new object();

    public Ant(int x, int y, string role)
    {
        X = x;
        Y = y;
        Role = role;
    }

    public void Live()
    {
        while (Energy > 0)
        {
            Move();
            PerformAction();
            Age++;
            Energy -= random.Next(1, 10);

            Thread.Sleep(random.Next(500, 1000));
        }

        IsAlive = false;  // Когда энергия достигает 0, муравей умирает
        Console.WriteLine($"Муравей {Role} умер.");
    }

    private void Move()
    {
        lock (locker)
        {
            if (IsAlive) // Двигаемся, только если муравей жив
            {
                X = Math.Clamp(X + random.Next(-1, 2), 0, Program.worldSize - 1);
                Y = Math.Clamp(Y + random.Next(-1, 2), 0, Program.worldSize - 1);
            }
        }
    }

    private void PerformAction()
    {
        if (IsAlive)  // Действие выполняется только, если муравей жив
        {
            if (Role == "Worker")
            {
                Console.WriteLine($"Муравей {Role} ищет еду.");
            }
            else if (Role == "Builder")
            {
                Console.WriteLine($"Муравей {Role} строит муравейник.");
            }
            else if (Role == "QueenCaretaker")
            {
                Console.WriteLine($"Муравей {Role} ухаживает за маткой.");
            }
        }
    }
}
