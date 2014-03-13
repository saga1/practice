using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Microsoft.Devices.Sensors;

namespace WPATBall
{
    class Ball
    {
        public Vector2 pos = new Vector2();
        public Texture2D img = null;
    }

    class Goal
    {
        public Vector2 pos = new Vector2
    }

    /// <summary>
    /// 基底 Game クラスから派生した、ゲームのメイン クラスです。
    /// </summary>h
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Accelerometer acc = new Accelerometer();
        Ball ball = null;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // 画面を初期化
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;

            // 加速度センサーを初期化
            acc.TimeBetweenUpdates = TimeSpan.FromMilliseconds(20);
            acc.CurrentValueChanged += new EventHandler<SensorReadingEventArgs<AccelerometerReading>>(acc_CurrentValueChanged);
            acc.Start();

            // Windows Phone のフレーム レートは既定で 30 fps です。
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // ロック中のバッテリ寿命を延長する。
            InactiveSleepTime = TimeSpan.FromSeconds(1);
        }

        /// <summary>
        /// ゲームの開始前に実行する必要がある初期化を実行できるようにします。
        /// ここで、要求されたサービスを問い合わせて、非グラフィック関連のコンテンツを読み込むことができます。
        /// base.Initialize を呼び出すと、任意のコンポーネントが列挙され、
        /// 初期化もされます。
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent はゲームごとに 1 回呼び出され、ここですべてのコンテンツを
        /// 読み込みます。
        /// </summary>
        protected override void LoadContent()
        {
            // 新規の SpriteBatch を作成します。これはテクスチャーの描画に使用できます。
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // コンテンツの読み込み
            ball = new Ball();
            ball.pos.X = graphics.PreferredBackBufferWidth / 2;
            ball.pos.Y = graphics.PreferredBackBufferHeight / 2;
            ball.img = Content.Load<Texture2D>("Ball");
        }

        /// <summary>
        /// UnloadContent はゲームごとに 1 回呼び出され、ここですべてのコンテンツを
        /// アンロードします。
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: ここで ContentManager 以外のすべてのコンテンツをアンロードします。
        }

        /// <summary>
        /// ワールドの更新、衝突判定、入力値の取得、オーディオの再生などの
        /// ゲーム ロジックを、実行します。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        protected override void Update(GameTime gameTime)
        {
            // ゲームの終了条件をチェックします。
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: ここにゲームのアップデート ロジックを追加します。

            base.Update(gameTime);
        }

        /// <summary>
        /// ゲームが自身を描画するためのメソッドです。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // ボールを描画
            spriteBatch.Begin();
            spriteBatch.Draw(ball.img, ball.pos, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected void acc_CurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            Vector3 a = e.SensorReading.Acceleration;
            Vector2 v = new Vector2();

            // 値を計算
            a.X *= 10;
            if (a.X > 0)
            {
                v.X = (a.X > 5 ? 5f : a.X);
            } else {
                v.X = (a.X > -5 ? -5f : a.X);
            }
            a.Y *= 10;
            if (a.Y > 0)
            {
                v.Y = (a.Y > 5 ? 5f : a.Y);
            }
            else
            {
                v.Y = (a.Y > -5 ? -5f : a.Y);
            }

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                // ボールを移動
                ball.pos.X += a.X;
                if (ball.pos.X < 0)
                {
                    ball.pos.X = graphics.PreferredBackBufferWidth;
                }
                else if (ball.pos.X > graphics.PreferredBackBufferWidth)
                {
                    ball.pos.X = 0;
                }
                ball.pos.Y -= a.Y;
                if (ball.pos.Y < 0)
                {
                    ball.pos.Y = graphics.PreferredBackBufferHeight;
                }
                else if (ball.pos.Y > graphics.PreferredBackBufferHeight)
                {
                    ball.pos.Y = 0;
                }
            });
        }
    }
}