using System;
using System.Numerics;
using System.Threading;
using System.IO;
using System.Media;
using System.Windows;

namespace TetrisProj
{
    public enum Dir { UP, DOWN, LEFT, RIGHT };

    static public class Draw
    {
        public static void One(char c, int X, int Y)
        {
            Console.SetCursorPosition(X, Y);
            Console.Write(c);
        }

        public static void Few(string s, int X, int Y)
        {
            Console.SetCursorPosition(X, Y);
            Console.Write(s);
        }

        public static void ClearSpecificConsoleArea(int Index0X, int Index0Y, int Width, int Hight)
        {
            for (int y = Index0Y; y < Index0Y + Hight; y++)
                for (int x = Index0X; x < Index0X + Width; x++)
                    One(' ', x, y);

        }
    }

    public class Shape
    {
        public Shape()
        {
            SetShape();
            ShapePattern = '■';
        }

        protected virtual void SetShape() { }

        public char ShapePattern;
        protected int Center = 14;
        public Complex[] Coords;
    }

    public class Shape_1 : Shape
    {
        /*
            x
            x
            x
            x
        */

        protected override void SetShape()
        {
            Coords = new Complex[4];

            Coords[0] = new Complex(Center, -1);
            Coords[1] = new Complex(Center, -2);
            Coords[2] = new Complex(Center, -3);
            Coords[3] = new Complex(Center, -4);

            base.SetShape();
        }

    }

    public class Shape_2 : Shape
    {
        /*

          x 
          x x
            x
            x
        */

        protected override void SetShape()
        {
            Coords = new Complex[5];

            Coords[0] = new Complex(Center, -1);
            Coords[1] = new Complex(Center, -2);
            Coords[2] = new Complex(Center, -3);
            Coords[3] = new Complex(Center - 1, -3);
            Coords[4] = new Complex(Center - 1, -4);

            base.SetShape();

        }

    }

    public class Shape_3 : Shape
    {
        /*
           x
         x x
           x
           x
       */
        protected override void SetShape()
        {
            Coords = new Complex[5];

            Coords[0] = new Complex(Center, -1);
            Coords[1] = new Complex(Center, -2);
            Coords[2] = new Complex(Center, -3);
            Coords[3] = new Complex(Center - 1, -3);
            Coords[4] = new Complex(Center, -4);

            base.SetShape();
        }

    }

    public class Shape_4 : Shape
    {
        /*
           x x
             x
             x
        */
        protected override void SetShape()
        {
            Coords = new Complex[4];

            Coords[0] = new Complex(Center, -1);
            Coords[1] = new Complex(Center, -2);
            Coords[2] = new Complex(Center, -3);
            Coords[3] = new Complex(Center - 1, -3);

            base.SetShape();
        }

    }

    public class Shape_5 : Shape
    {
        /*
          x x
          x x
        */

        protected override void SetShape()
        {
            Coords = new Complex[4];

            Coords[0] = new Complex(Center, -1);
            Coords[1] = new Complex(Center, -2);
            Coords[2] = new Complex(Center - 1, -2);
            Coords[3] = new Complex(Center - 1, -1);

            base.SetShape();
        }

    }


    public class Logic
    {
        public Logic(bool NewGame)
        {
            PresentShape = randomShape();
            NextShape = randomShape();
            Saving.LoadHighestScore();

            GP = new GamePanel();

            if (!NewGame)
            {
                Saving.LoadGame(out BusySlots, out Score);

                Refresh();
            }
            else
                Score = 0;

            Draw.Few($"[{(Score).ToString()}]", 32, 10);

            DrawNextShape();
        }

        public void MainLoop()
        {
            bool HasTouchedGround;

            while (true)
            {
                switch (_pressedkey.Key)
                {
                    case ConsoleKey.A: Rotate(Dir.LEFT); break;
                    case ConsoleKey.D: Rotate(Dir.RIGHT); break;
                    case ConsoleKey.Spacebar: Mirror(); break;
                    case ConsoleKey.LeftArrow: Move(Dir.LEFT, 0); break;
                    case ConsoleKey.RightArrow: Move(Dir.RIGHT, 0); break;
                    case ConsoleKey.Q: Move(Dir.LEFT, -1); break;
                    case ConsoleKey.E: Move(Dir.RIGHT, 1); break;
                    case ConsoleKey.DownArrow: RushDown(); break;
                    default: break;
                }

                HasTouchedGround = checkBlockSurroundings();

                if (!HasTouchedGround)
                    DrawNextGameState();
                else
                {
                    CheckReduce();

                    if (gameOver = GameOver() == true)
                        break;

                    switchShapes();
                    DrawNextGameState();

                    HasTouchedGround = false;

                    Refresh();
                }

                if (_pressedkey.Key == ConsoleKey.P)
                    if (PAUSE()) break;

                Thread.Sleep(400);
            }
        }

        private bool PAUSE()
        {
            PauseOn = true;

            Pause = new PauseScreen();

            if (Pause.TriState == 0 || Pause.TriState == 1)
            {
                if (Pause.TriState == 0)
                    Saving.SaveGame(BusySlots, Score);

                Quit = true;

                return true;
            }

            Console.Clear();

            Refresh();
            GP.DrawGamePanelInvoker();
            Draw.Few($"[{(Score).ToString()}]", 35, 9);
            DrawNextShape();

            Console.CursorVisible = false;

            PauseOn = false;

            return false;
        }

        private void switchShapes()
        {
            PresentShape = NextShape;
            NextShape = randomShape();

            Draw.ClearSpecificConsoleArea(29, 14, 9, 5);
            DrawNextShape();
        }

        private bool GameOver()
        {
            int i;

            for (i = 0; i < PresentShape.Coords.Length; i++)
                if (PresentShape.Coords[i].Imaginary < 0)
                    break;

            if (i == PresentShape.Coords.Length) return false;
            else return true;

        }

        private void CheckReduce()
        {
            bool Reduce = false;
            int j;

            for (int i = 19; i >= 0; i--)
            {
                for (j = 1; j < 27; j++)
                    if (BusySlots[i, j] == false)
                        break;

                if (j == 27)
                {
                    LayerToReduce[i] = true;
                    Reduce = true;
                }
            }

            if (Reduce)
                ReduceAndMergeLayers();

        }

        private void ReduceAndMergeLayers()
        {

            for (int i = 19; i >= 0; i--)
            {
                if (LayerToReduce[i] == true)
                {
                    Score += 5;

                    for (int R = 19; R >= 0; R--)
                        for (int C = 1; C < 27; C++)
                            BusySlots[R + 1, C] = BusySlots[R, C];

                    Refresh();
                }
            }
        }

        private void Refresh()
        {
            for (int i = 1; i < 27; i++)
                for (int j = 0; j < 20; j++)
                {
                    if (BusySlots[j, i] == false)
                        Draw.One(' ', i, j);
                    else
                        Draw.One('■', i, j);
                }
        }

        private bool checkBlockSurroundings()
        {
            for (int i = 0; i < PresentShape.Coords.Length; i++)
            {
                if (PresentShape.Coords[i].Imaginary >= 0)
                    if (PresentShape.Coords[i].Imaginary + 1 >= 20 || BusySlots[(int)PresentShape.Coords[i].Imaginary + 1, (int)PresentShape.Coords[i].Real] == true)
                    {
                        for (int b = 0; b < PresentShape.Coords.Length; b++)
                            if (PresentShape.Coords[b].Imaginary >= 0)
                                BusySlots[(int)PresentShape.Coords[b].Imaginary, (int)PresentShape.Coords[b].Real] = true;

                        Console.Beep(200, 500);
                        Draw.Few($"[{(++Score).ToString()}]", 32, 10);
                        return true;
                    }
            }

            return false;
        }

        private void DrawNextGameState()
        {

            for (int i = 0; i < PresentShape.Coords.Length; i++)
            {
                if (PresentShape.Coords[i].Imaginary + 1 >= 0)
                {
                    if (PresentShape.Coords[i].Imaginary == -1)
                    {
                        PresentShape.Coords[i] = new Complex(PresentShape.Coords[i].Real, PresentShape.Coords[i].Imaginary + 1);
                        Draw.One(PresentShape.ShapePattern, (int)PresentShape.Coords[i].Real, (int)PresentShape.Coords[i].Imaginary);
                    }
                    else
                    {
                        Draw.One(' ', (int)PresentShape.Coords[i].Real, (int)PresentShape.Coords[i].Imaginary);
                        PresentShape.Coords[i] = new Complex(PresentShape.Coords[i].Real, PresentShape.Coords[i].Imaginary + 1);
                    }

                }
                else
                {
                    PresentShape.Coords[i] = new Complex(PresentShape.Coords[i].Real, PresentShape.Coords[i].Imaginary + 1);
                }
            }

            for (int i = 0; i < PresentShape.Coords.Length; i++)
                if (PresentShape.Coords[i].Imaginary >= 0)
                    Draw.One(PresentShape.ShapePattern, (int)PresentShape.Coords[i].Real, (int)PresentShape.Coords[i].Imaginary);

        }

        private void Move(Dir Direction, int Dash)
        {
            int Mover = (Direction == Dir.RIGHT) ? (1 + Dash) : (-1 + Dash);
            bool WallBeside = false;

            for (int i = 0; i < PresentShape.Coords.Length; i++)
                if (PresentShape.Coords[i].Real + Mover == 27 || PresentShape.Coords[i].Real + Mover == 0)
                {
                    WallBeside = true;
                    break;
                }

            if (!WallBeside)
            {
                for (int i = 0; i < PresentShape.Coords.Length; i++)
                {
                    if (PresentShape.Coords[i].Imaginary >= 0)
                    {
                        Draw.One(' ', (int)PresentShape.Coords[i].Real, (int)PresentShape.Coords[i].Imaginary);
                        PresentShape.Coords[i] = new Complex(PresentShape.Coords[i].Real + Mover, PresentShape.Coords[i].Imaginary);
                    }
                    else
                    {
                        PresentShape.Coords[i] = new Complex(PresentShape.Coords[i].Real + Mover, PresentShape.Coords[i].Imaginary);
                    }
                }

                for (int i = 0; i < PresentShape.Coords.Length; i++)
                    if (PresentShape.Coords[i].Imaginary >= 0)
                        Draw.One(PresentShape.ShapePattern, (int)PresentShape.Coords[i].Real, (int)PresentShape.Coords[i].Imaginary);
            }


            _pressedkey = new ConsoleKeyInfo();
        }

        private void Rotate(Dir Direction)
        {


            double rotator = (Direction == Dir.RIGHT) ? 3 : 1;
            int corrector = (Direction == Dir.RIGHT) ? -1 : 1;

            int MaxImagineryValue = -10;
            Complex CenterOfRotation = new Complex(0, 0);
            Complex ShiftVector;

            for (int i = 0; i < PresentShape.Coords.Length; i++)
                if (PresentShape.Coords[i].Imaginary > MaxImagineryValue)
                {
                    MaxImagineryValue = (int)PresentShape.Coords[i].Imaginary;
                    CenterOfRotation = PresentShape.Coords[i];
                }

            for (int i = 0; i < PresentShape.Coords.Length; i++)
            {
                if (PresentShape.Coords[i].Imaginary >= 0)
                    Draw.One(' ', (int)PresentShape.Coords[i].Real, (int)PresentShape.Coords[i].Imaginary);

                ShiftVector = Complex.Multiply(Complex.Add(CenterOfRotation, Complex.Negate(PresentShape.Coords[i])), Complex.Pow(Complex.ImaginaryOne, rotator));
                PresentShape.Coords[i] = Complex.Add(ShiftVector, Complex.Add(CenterOfRotation, new Complex(corrector, -1)));
            }

            _pressedkey = new ConsoleKeyInfo();
        }

        private void Mirror()
        {
            int maxImagineryValue = -1;
            int minImaginaryValue = 25;

            for (int i = 0; i < PresentShape.Coords.Length; i++)
            {
                if (maxImagineryValue < PresentShape.Coords[i].Imaginary)
                    maxImagineryValue = (int)PresentShape.Coords[i].Imaginary;

                else if (minImaginaryValue > PresentShape.Coords[i].Imaginary)
                    minImaginaryValue = (int)PresentShape.Coords[i].Imaginary;

            }


            for (int i = 0; i < PresentShape.Coords.Length; i++)
            {
                if (PresentShape.Coords[i].Imaginary >= 0)
                    Draw.One(' ', (int)PresentShape.Coords[i].Real, (int)PresentShape.Coords[i].Imaginary);

                PresentShape.Coords[i] = Complex.Conjugate(PresentShape.Coords[i]);
                PresentShape.Coords[i] = Complex.Add(PresentShape.Coords[i], (minImaginaryValue + maxImagineryValue + 1) * Complex.ImaginaryOne);
            }

            for (int i = 0; i < PresentShape.Coords.Length; i++)
            {
                if (PresentShape.Coords[i].Imaginary >= 0)
                    Draw.One(PresentShape.ShapePattern, (int)PresentShape.Coords[i].Real, (int)PresentShape.Coords[i].Imaginary);
            }

            _pressedkey = new ConsoleKeyInfo();
        }

        private void RushDown()
        {
            for (int i = 0; i < PresentShape.Coords.Length; i++)
            {
                if (PresentShape.Coords[i].Imaginary >= 0)
                    Draw.One(' ', (int)PresentShape.Coords[i].Real, (int)PresentShape.Coords[i].Imaginary);
            }

            while (!checkBlockSurroundings())
            {
                for (int i = 0; i < PresentShape.Coords.Length; i++)
                {
                    PresentShape.Coords[i] = Complex.Add(PresentShape.Coords[i], Complex.ImaginaryOne);
                }
            }

            for (int i = 0; i < PresentShape.Coords.Length; i++)
            {
                if (PresentShape.Coords[i].Imaginary >= 0)
                    Draw.One(PresentShape.ShapePattern, (int)PresentShape.Coords[i].Real, (int)PresentShape.Coords[i].Imaginary);
            }

            _pressedkey = new ConsoleKeyInfo();

            Score--;
        }

        private Shape randomShape()
        {
            Random rnd = new Random();

            switch (rnd.Next(1, 6))
            {
                case 1: return new Shape_1();
                case 2: return new Shape_2();
                case 3: return new Shape_3();
                case 4: return new Shape_4();
                case 5: return new Shape_5();
            }

            return new Shape();
        }

        private void DrawNextShape()
        {
            for (int i = 0; i < NextShape.Coords.Length; i++)
                Draw.One(
                            NextShape.ShapePattern,
                            (int)NextShape.Coords[i].Real + 19,
                            (int)NextShape.Coords[i].Imaginary + 19
                         );

        }


        private Shape PresentShape;
        private Shape NextShape;
        private bool[,] BusySlots = new bool[25, 27];
        private bool[] LayerToReduce = new bool[20];
        private PauseScreen Pause;
        private GamePanel GP;

        public int Score;
        public ConsoleKeyInfo _pressedkey;
        public bool gameOver;
        public bool PauseOn = false;
        public bool Quit = false;
    }

    public static class Saving
    {
        static public void LoadGame(out bool[,] _BusySlots, out int _Score)
        {
            StreamReader reader = new StreamReader("PreviousGame.txt");

            _BusySlots = new bool[25, 27];
            _Score = new int();

            for (int i = 0; i < 25; i++)
                for (int j = 0; j < 27; j++)
                {
                    if (reader.Read() == '0')
                        _BusySlots[i, j] = false;
                    else
                        _BusySlots[i, j] = true;
                }

            reader.ReadLine();

            _Score = Int32.Parse(reader.ReadLine());

            reader.Dispose();
        }

        static public void SaveGame(bool[,] _BusySlots, int _Score)
        {
            StreamWriter writer = new StreamWriter("PreviousGame.txt");

            for (int i = 0; i < 25; i++)
                for (int j = 0; j < 27; j++)
                {
                    if (_BusySlots[i, j] == false)
                        writer.Write('0');
                    else
                        writer.Write('1');
                }

            writer.Write('\n');
            writer.Write(_Score.ToString());

            writer.Dispose();
        }

        static public void NewHighestScore(int _Score)
        {
            if (Int32.Parse(HighestScore) < _Score)
            {
                StreamWriter writer = new StreamWriter("Highest_Score.txt");
                writer.Write(_Score);

                writer.Dispose();
            }

        }

        static public void LoadHighestScore()
        {
            StreamReader reader = new StreamReader("Highest_Score.txt");
            HighestScore = reader.ReadLine();

            reader.Dispose();
        }

        public static string HighestScore;
    }

    public class Panel
    {
        public Panel()
        {
            DrawGamePanel();
        }

        protected virtual void DrawGamePanel()
        {
            Console.Clear();

            for (int i = 0; i < 40; i++)
                Draw.One('─', i, 0);

            for (int j = 1; j < 20; j++)
                for (int i = 0; i < 40; i += 39)
                    Draw.One('│', i, j);

            for (int i = 1; i < 39; i++)
                Draw.One('─', i, 20);

            Draw.One('┌', 0, 0);
            Draw.One('┐', 39, 0);
            Draw.One('┘', 39, 20);
            Draw.One('└', 0, 20);
        }
    }

    public class GamePanel : Panel
    {
        public GamePanel()
        {
            DrawGamePanel();
        }

        public void DrawGamePanelInvoker() => DrawGamePanel();

        protected override void DrawGamePanel()
        {
            for (int i = 28; i < 39; i++)
            {
                Draw.One('─', i, 0);
                Draw.One('─', i, 8);
                Draw.One('─', i, 11);
                Draw.One('─', i, 4);
            }

            Draw.Few("──■■■■■■■──", 28, 1);
            Draw.Few("─■──■■■──■─", 28, 2);
            Draw.Few("──■──■──■──", 28, 3);

            for (int i = 1; i < 39; i++)
                Draw.One('─', i, 20);

            for (int j = 1; j < 20; j++)
            {
                for (int i = 0; i < 40; i += 39)
                {
                    Draw.One('│', i, j);
                    Draw.One('│', 27, j);
                }
            }

            Draw.One('└', 27, 20);
            Draw.One('│', 0, 0);
            Draw.One('┌', 27, 0);
            Draw.One('┐', 39, 0);
            Draw.One('┘', 39, 20);
            Draw.One('└', 0, 20);

            Draw.Few("N E X T", 30, 12);
            Draw.Few("S H A P E", 29, 13);

            Draw.Few("HIGHEST", 30, 5);
            Draw.Few("SCORE", 31, 6); Draw.Few($"[{Saving.HighestScore}]", 32, 7);
            Draw.Few("SCORE", 31, 9);

        }
    }

    public class GameOverScreen : Panel
    {
        public GameOverScreen()
        {
            DrawGamePanel();
        }

        ~GameOverScreen() => Console.Clear();

        protected override void DrawGamePanel()
        {
            base.DrawGamePanel();
            Draw.Few("───────G  A  M  E    O  V  E  R───────", 1, 10);


        }
    }

    public class PauseScreen : Panel
    {
        public PauseScreen()
        {
            PauseControl();
        }

        protected override void DrawGamePanel()
        {
            base.DrawGamePanel();

            Draw.Few(">>RESUME<<<", 14, 8);
            Draw.Few(">SAVE&QUIT<", 14, 10);
            Draw.Few(">>>QUIT<<<<", 14, 12);
        }

        private void PauseControl()
        {
            DrawGamePanel();

            int CursorYposition = 10;
            Console.CursorVisible = true;

            Console.SetCursorPosition(25, CursorYposition);

            do
            {
                _key = Console.ReadKey(true);

                if (_key.Key == ConsoleKey.W && CursorYposition - 2 >= 8)
                {
                    CursorYposition -= 2;
                    Console.SetCursorPosition(25, CursorYposition);
                    Console.Beep(100, 200);
                }
                else if (_key.Key == ConsoleKey.S && CursorYposition + 2 <= 12)
                {
                    CursorYposition += 2;
                    Console.SetCursorPosition(25, CursorYposition);
                    Console.Beep(200, 200);
                }
            }
            while (_key.Key != ConsoleKey.Enter);

            if (CursorYposition == 8) TriState = -1;
            else if (CursorYposition == 10) TriState = 0;
            else TriState = 1;

        }

        ConsoleKeyInfo _key;
        public int TriState;
    }

    public class ControlsScreen : Panel
    {
        public ControlsScreen()
        {
            DrawGamePanel();
        }

        protected override void DrawGamePanel()
        {
            base.DrawGamePanel();

            Draw.Few("──────C O N T R O L S──────", 1, 3);
            Draw.Few("* Move + Dash -> Q/E", 3, 5);
            Draw.Few("* Move -> LeftArrow/RightArrow", 3, 7);
            Draw.Few("* Rotate -> A/D", 3, 9);
            Draw.Few("* Rush -> DownArrow", 3, 11);
            Draw.Few("* Mirror Shape -> SpaceBar", 3, 13);
            Draw.Few("* Pause -> P", 3, 15);
            Draw.Few(" PRESS 'Q' TO ESCAPE ", 18, 19);
        }
    }

    public class Menu : Panel
    {
        public Menu()
        {
            while (MenuControl() != true) ;
        }

        protected override void DrawGamePanel()
        {
            Console.SetWindowSize(41, 21);

            base.DrawGamePanel();

            Draw.Few("T  E  T  R  I  S", 12, 6);
            Draw.Few(">>New─Game<<".ToUpper(), 14, 10);
            Draw.Few(">Load──Game<".ToUpper(), 14, 12);
            Draw.Few(">>Controls<<".ToUpper(), 14, 14);

            Console.SetCursorPosition(26, 12);
        }

        private bool MenuControl()
        {
            DrawGamePanel();

            int CursorYposition = 12;
            Console.CursorVisible = true;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.W && CursorYposition - 2 >= 10)
                {
                    CursorYposition -= 2;
                    Console.SetCursorPosition(26, CursorYposition);
                    Console.Beep(100, 200);
                }
                else if (key.Key == ConsoleKey.S && CursorYposition + 2 <= 14)
                {
                    CursorYposition += 2;
                    Console.SetCursorPosition(26, CursorYposition);
                    Console.Beep(200, 200);
                }



            }
            while (key.Key != ConsoleKey.Enter);

            Console.Clear();

            if (CursorYposition == 14)
            {
                Console.CursorVisible = false;
                CS = new ControlsScreen();

                key = Console.ReadKey(true);

                while (key.Key != ConsoleKey.Q) ;

                Console.Clear();

                return false;

            }
            else if (CursorYposition == 12)
                _NewGame = false;


            return true;
        }

        private ConsoleKeyInfo key;
        private ControlsScreen CS;
        public bool _NewGame = true;
    }

    class Program
    {
        

        static void Main(string[] args)
        {
            SoundPlayer player = new SoundPlayer();
           
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "No Death - Final Communication.wav";
            player.Play();
            
            Menu _Menu = new Menu();
            GameOverScreen GOS;

            Logic Game = new Logic(_Menu._NewGame);

            Thread t = new Thread(new ThreadStart(Game.MainLoop));

            t.Start();

            ConsoleKeyInfo pressedkey;
            Console.CursorVisible = false;

            do
            {
                if (!Game.PauseOn)
                {
                    pressedkey = Console.ReadKey(true);
                    Game._pressedkey = pressedkey;
                }

            } while (Game.gameOver != true && Game.Quit != true);



            if (Game.gameOver == true)
                GOS = new GameOverScreen();

            Saving.NewHighestScore(Game.Score);

            t.Join();


            Console.SetCursorPosition(0, 21);
        }
    }
}
