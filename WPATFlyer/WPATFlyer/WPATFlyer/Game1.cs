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

namespace WPATFlyer
{
    class Flayer
    {
        public Vector2 pos = new Vector2();
        public Texture2D img = null;
        public Texture2D imgBomb = null;
    }

    class Gold
    {
        public Vector2 pos = new Vector2();
        public Texture2D img = null;
        public const int SCORE_POINT = 30;
    }
    
    class Silver
    {
        public Vector2 pos = new Vector2();
        public Texture2D img = null;
        public const int SCORE_POINT = 10;
    }

    class Enemy
    {
        public Vector2 pos = new Vector2();
        public Texture2D img = null;
    }

    enum SCENE : int
    {
        INIT, TITLE, PLAY, GAMEOVER
    }

    /// <summary>
    /// 基底 Game クラスから派生した、ゲームのメイン クラスです。
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont text;
        Accelerometer acc = new Accelerometer();
        Flayer flayer = new Flayer();
        Gold gold = new Gold();
        Silver[] silver = new Silver[] {new Silver(), new Silver()};
        Enemy enemy = new Enemy();
        SCENE scene = SCENE.INIT;
        int score = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // 画面を初期化
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
            graphics.ApplyChanges();

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
            // ジェスチャーの有効化
            TouchPanel.EnabledGestures = GestureType.Tap;

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
            flayer.img = Content.Load<Texture2D>("Flayer");
            flayer.imgBomb = Content.Load<Texture2D>("Bomb");
            gold.img = Content.Load<Texture2D>("Gold");
            silver[0].img = Content.Load<Texture2D>("Silver");
            silver[1].img = Content.Load<Texture2D>("Silver");
            enemy.img = Content.Load<Texture2D>("Enemy");
            text = Content.Load<SpriteFont>("Text");

            // 初期位置を設定
            gold.pos = this.GetRandomPos();
            silver[0].pos = this.GetRandomPos();
            silver[1].pos = this.GetRandomPos();
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

            if (scene == SCENE.INIT)
            {
                // 初期化
                flayer.pos.X = graphics.PreferredBackBufferWidth / 2;
                flayer.pos.Y = graphics.PreferredBackBufferHeight / 2;
                score = 0;
                scene = SCENE.TITLE;
            }
            else if (scene == SCENE.TITLE)
            {
                // タッチを検出
                while (TouchPanel.IsGestureAvailable)
                {
                    GestureSample gs = TouchPanel.ReadGesture();
                    if (gs.GestureType == GestureType.Tap)
                    {
                        scene = SCENE.PLAY;
                    }
                }
            }
            else if (scene == SCENE.PLAY)
            {
                // 衝突判定
                Rectangle[] rect = new Rectangle[2];
                rect[0] = new Rectangle((int)flayer.pos.X, (int)flayer.pos.Y, flayer.img.Width, flayer.img.Height);
                rect[1] = new Rectangle((int)gold.pos.X, (int)gold.pos.Y, gold.img.Width, gold.img.Height);
                if (rect[0].Intersects(rect[1]))
                {
                    score += Gold.SCORE_POINT;
                    gold.pos = this.GetRandomPos();
                    enemy.pos = this.GetRandomPos();
                }
                rect[1] = new Rectangle((int)silver[0].pos.X, (int)silver[0].pos.Y, silver[0].img.Width, silver[0].img.Height);
                if (rect[0].Intersects(rect[1]))
                {
                    score += Silver.SCORE_POINT;
                    silver[0].pos = this.GetRandomPos();
                    enemy.pos = this.GetRandomPos();
                }
                rect[1] = new Rectangle((int)silver[1].pos.X, (int)silver[1].pos.Y, silver[1].img.Width, silver[1].img.Height);
                if (rect[0].Intersects(rect[1]))
                {
                    score += Silver.SCORE_POINT;
                    silver[1].pos = this.GetRandomPos();
                    enemy.pos = this.GetRandomPos();
                }
                rect[1] = new Rectangle((int)enemy.pos.X, (int)enemy.pos.Y, enemy.img.Width, enemy.img.Height);
                if (rect[0].Intersects(rect[1]))
                {
                    scene = SCENE.GAMEOVER;
                }
            }
            else if (scene == SCENE.GAMEOVER)
            {
                // タッチを検出
                while (TouchPanel.IsGestureAvailable)
                {
                    GestureSample gs = TouchPanel.ReadGesture();
                    if (gs.GestureType == GestureType.Tap)
                    {
                        scene = SCENE.INIT;
                    }
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// ゲームが自身を描画するためのメソッドです。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkBlue);
            spriteBatch.Begin();

            if (scene == SCENE.TITLE)
            {
                spriteBatch.Draw(flayer.img, new Vector2(((graphics.PreferredBackBufferWidth - flayer.img.Width) / 2), graphics.PreferredBackBufferHeight / 3), Color.White);
                String title_text = "Start by tapping game";
                spriteBatch.DrawString(text, title_text, new Vector2(((graphics.PreferredBackBufferWidth - text.MeasureString(title_text).X) / 2), graphics.PreferredBackBufferHeight * 2 / 3), Color.Yellow);
            }
            else if (scene == SCENE.PLAY || scene == SCENE.GAMEOVER)
            {
                // 自機とコインと敵を描画
                spriteBatch.Draw(gold.img, gold.pos, Color.White);
                spriteBatch.Draw(silver[0].img, silver[0].pos, Color.White);
                spriteBatch.Draw(silver[1].img, silver[1].pos, Color.White);
                spriteBatch.Draw(enemy.img, enemy.pos, Color.White);
                if (scene == SCENE.PLAY)
                {
                    spriteBatch.Draw(flayer.img, flayer.pos, Color.White);
                }
                else
                {
                    spriteBatch.Draw(flayer.imgBomb, flayer.pos, Color.White);
                    String gameover_text = "GAME OVER";
                    spriteBatch.DrawString(text, gameover_text, new Vector2(((graphics.PreferredBackBufferWidth - text.MeasureString(gameover_text).X) / 2), graphics.PreferredBackBufferHeight / 2), Color.Yellow);
                }

                // テキストを描画
                String score_text = "Score: " + score;
                spriteBatch.DrawString(text, score_text, new Vector2(10, 10), Color.White);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        protected Vector2 GetRandomPos()
        {
            Vector2 ret = new Vector2();
            Random rnd = new Random();
            Rectangle[] rect = new Rectangle[2];

            for (int i = 0; i < 10; i++)
            {
                ret.X = rnd.Next(graphics.PreferredBackBufferWidth);
                ret.Y = rnd.Next(graphics.PreferredBackBufferHeight);

                rect[0] = new Rectangle((int)ret.X, (int)ret.Y, flayer.img.Width, flayer.img.Height);
                rect[1] = new Rectangle((int)flayer.pos.X, (int)flayer.pos.Y, flayer.img.Width, flayer.img.Height);
                if (rect[0].Intersects(rect[1])) {
                    continue;
                }

                rect[0] = new Rectangle((int)ret.X, (int)ret.Y, gold.img.Width, gold.img.Height);
                rect[1] = new Rectangle((int)gold.pos.X, (int)gold.pos.Y, gold.img.Width, gold.img.Height);
                if (rect[0].Intersects(rect[1]))
                {
                    continue;
                }

                rect[0] = new Rectangle((int)ret.X, (int)ret.Y, silver[0].img.Width, silver[0].img.Height);
                rect[1] = new Rectangle((int)silver[0].pos.X, (int)silver[0].pos.Y, silver[0].img.Width, silver[0].img.Height);
                if (rect[0].Intersects(rect[1]))
                {
                    continue;
                }

                rect[0] = new Rectangle((int)ret.X, (int)ret.Y, silver[1].img.Width, silver[1].img.Height);
                rect[1] = new Rectangle((int)silver[1].pos.X, (int)silver[1].pos.Y, silver[1].img.Width, silver[1].img.Height);
                if (rect[0].Intersects(rect[1]))
                {
                    continue;
                }

                rect[0] = new Rectangle((int)ret.X, (int)ret.Y, enemy.img.Width, enemy.img.Height);
                rect[1] = new Rectangle((int)enemy.pos.X, (int)enemy.pos.Y, enemy.img.Width, enemy.img.Height);
                if (rect[0].Intersects(rect[1]))
                {
                    continue;
                }
                break;
            }

            return ret;
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
            }
            else
            {
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
                if (scene != SCENE.PLAY)
                {
                    return;
                }

                // 自機を移動
                flayer.pos.X += a.X;
                if (flayer.pos.X < 0)
                {
                    flayer.pos.X = graphics.PreferredBackBufferWidth;
                }
                else if (flayer.pos.X > graphics.PreferredBackBufferWidth)
                {
                    flayer.pos.X = 0;
                }
                flayer.pos.Y -= a.Y;
                if (flayer.pos.Y < 0)
                {
                    flayer.pos.Y = graphics.PreferredBackBufferHeight;
                }
                else if (flayer.pos.Y > graphics.PreferredBackBufferHeight)
                {
                    flayer.pos.Y = 0;
                }
            });
        }
    }
}
