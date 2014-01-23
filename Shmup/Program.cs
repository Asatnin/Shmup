using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Shmup
{
    class Program : GameWindow
    {
        const int SCREEN_WIDTH = 799;
        const int SCREEN_HEIGHT = 599;
        const int SCREEN_FPS = 60;
        const string TITLE = "Shmup";

        long time, simulationTime;
        int delta;
        static long gameTime = 0;
        Stopwatch stopwatch = new Stopwatch();

        static int width = SCREEN_WIDTH, height = SCREEN_HEIGHT;

        ShaderProgram mainProgram = new ShaderProgram();

        // состояния приложения
        public enum GameState
        {
            menu, game, gameOver, newGame, pause
        }

        // текущее состояние приложения
        static GameState currentState = GameState.menu;

        // Бэкграунд
        Background background;

        // Персонаж
        Player player;

        // Оппонент
        Opponent opponent;

        // Текст
        FontSheet font;

        ThreadStart job;
        Thread thread;

        public Program()
            : base(SCREEN_WIDTH, SCREEN_HEIGHT, GraphicsMode.Default, TITLE,
            GameWindowFlags.Default, DisplayDevice.Default, 2, 1, GraphicsContextFlags.Default)
        {
            this.WindowBorder = WindowBorder.Fixed;
            this.Icon = new System.Drawing.Icon("smallIcon.ico");
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, width, height);

            mainProgram.setProjection(Matrix4.CreateOrthographicOffCenter(0.0f, width, height,
                0.0f, 1.0f, -1.0f));
            mainProgram.updateProjection();
        }


        protected override void OnLoad(EventArgs e)
        {
            Console.WriteLine(GL.GetString(StringName.Version));
            Console.WriteLine(GL.GetString(StringName.Renderer));
            Console.WriteLine(GL.GetString(StringName.ShadingLanguageVersion));

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            GL.Viewport(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);

            GL.Enable(EnableCap.Texture2D);

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            if (!mainProgram.loadProgram())
                Console.WriteLine("Unable to load shader program!");

            mainProgram.bind();

            // projection matrix
            mainProgram.setProjection(Matrix4.CreateOrthographicOffCenter(0.0f, SCREEN_WIDTH,
                SCREEN_HEIGHT, 0.0f, 1.0f, -1.0f));
            mainProgram.updateProjection();

            // modelview matrix
            mainProgram.setModelView(Matrix4.Identity);
            mainProgram.updateModelView();

            mainProgram.setTextureUnit(0);

            Background.attachProgram(mainProgram);
            background = new Background();

            Player.attachProgram(mainProgram);
            Player.attachKeyboard(Keyboard);
            player = new Player();

            Bullet.attachProgram(mainProgram);

            Enemy.attachProgram(mainProgram);
            
            opponent = new Opponent();

            ScreenBullets.attachProgram(mainProgram);

            Explosion.attachProgram(mainProgram);

            StaticSprite.attachProgram(mainProgram);

            FontSheet.attachProgram(mainProgram);
            font = new FontSheet(0.4f);
            font.loadBitmap("Sprites/Information/font.png");

            SoundClass.prepare();

            SoundClass.startLoopMusic();

            stopwatch.Start();

            if (GL.GetError() != ErrorCode.NoError)
                Console.WriteLine("Unable to init OpenGL!");
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            switch (currentState)
            {
                case GameState.menu:
                    //ScreenText.updateStartMenu();
                    break;

                case GameState.game:
                    time = stopwatch.ElapsedMilliseconds;
                    delta = 16;
                    while (simulationTime < time)
                    {
                        gameTime += delta;
                        background.update(delta);
                        opponent.update(delta);
                        ScreenEffects.update(delta);
                        ScreenBullets.update(delta);
                        ScreenInformation.update(delta);
                        player.update(delta);
                        simulationTime += delta;
                    }
                    break;

                case GameState.pause:                    
                    break;

                case GameState.gameOver:
                    ScreenEffects.update(16);
                    if (thread == null)
                    {
                        job = new ThreadStart(ThreadJob);
                        thread = new Thread(job);
                        thread.Start();
                    }
                    break;

                case GameState.newGame:
                    player.restart();
                    gameTime = 0;
                    currentState = GameState.game;
                    SoundClass.startLoopMusic();
                    ScreenBullets.restart();
                    ScreenEffects.restart();
                    ScreenInformation.restart();
                    opponent.restart();
                    thread = null;
                    stopwatch.Reset();
                    stopwatch.Start();
                    simulationTime = 0;
                    break;

                default:
                    break;
            }

            if (currentState != GameState.gameOver && !Player.Alive)
            {
                currentState = GameState.gameOver;
                SoundClass.stopLoopMusic();
            }
        }

        void ThreadJob()
        {
            Thread.Sleep(800);
            DialogResult result = MessageBox.Show("Вы продержались " + 
                String.Format("{0:f1}", gameTime / 1000.0f) +
                " секунд и набрали " + Player.Points + " очков! Хотите попробовать ещё раз?",
                "Игра окончена!", MessageBoxButtons.YesNo);
            switch (result)
            {
                case DialogResult.No:
                    Environment.Exit(0);
                    break;
                case DialogResult.Yes:
                    Program.setState(GameState.newGame);
                    break;
                default:
                    break;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // нулевое смещение текстуры
            mainProgram.setOffsetX(0.0f);

            switch (currentState)
            {
                case GameState.menu:
                    //background.render();
                    //ScreenText.renderStartMenu();
                    break;

                case GameState.game:
                case GameState.pause:
                    background.render();
                    opponent.render();
                    ScreenInformation.render();
                    ScreenEffects.render();
                    ScreenBullets.render();
                    player.render();
                    font.renderText(1, 16, String.Format("Time: {0:f1}", gameTime / 1000.0f),
                        new Vector4(132 / 255.0f, 89 / 255.0f, 107 / 255.0f, 1));
                    font.renderText(700, 1, "Score: " + Player.Points,
                        new Vector4(1, 0, 0, 1));
                    if (currentState == GameState.pause)
                        font.renderText(width / 2 - 20, height / 2 - 20, "Pause",
                            new Vector4(1, 0, 0, 1));
                    break;

                case GameState.gameOver:
                    background.render();
                    opponent.render();
                    ScreenEffects.render();
                    ScreenBullets.render();
                    font.renderText(width / 2 - 50, height / 2 - 200, "Game Over",
                            new Vector4(1, 0, 0, 1));
                    break;

                default:
                    break;
            }

            SwapBuffers();
        }

        protected override void OnKeyPress(OpenTK.KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case 'p':
                case 'P':
                case 'З':
                case 'з':
                    if (currentState == GameState.game)
                    {
                        currentState = GameState.pause;
                        stopwatch.Stop();
                        SoundClass.pauseAllSounds();
                        break;
                    }

                    if (currentState == GameState.pause)
                    {
                        currentState = GameState.game;
                        stopwatch.Start();
                        SoundClass.resumeAllSounds();
                    }
                    break;

                // кнопка Escape
                case (char)27:
                    Console.WriteLine("YES!");
                    if (currentState == GameState.game || currentState == GameState.pause)
                    {
                        currentState = GameState.pause;
                        stopwatch.Stop();
                        SoundClass.pauseAllSounds();
                        DialogResult result = MessageBox.Show("Вы действительно хотите выйти?",
                            "Выход", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            freeResources();
                            Environment.Exit(0);
                        }
                        else
                        {
                            currentState = GameState.game;
                            stopwatch.Start();
                            SoundClass.resumeAllSounds();
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (thread != null)
                e.Cancel = true;
        }

        [STAThread]
        static void Main(string[] args)
        {
            Menu menuForm = new Menu();
            DialogResult result = menuForm.ShowDialog();
            if (result == DialogResult.Abort)
                Environment.Exit(0);
            using (Program program = new Program())
            {
                program.Run(SCREEN_FPS);
            }
        }

        void freeResources()
        {
            this.Dispose();
            SoundClass.dispose();
        }

        public static void setState(GameState state)
        {
            currentState = state;
        }

        public static int WIDTH
        {
            get
            {
                return width;
            }
        }

        public static int HEIGHT
        {
            get
            {
                return height;
            }
        }
    }
}
