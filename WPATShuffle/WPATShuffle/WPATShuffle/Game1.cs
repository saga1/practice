using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace WPATShuffle
{
    /// <summary>
    /// Boxクラス
    /// </summary>
    class Box
    {
        public enum STATUS : int
        {
            MISS, HIT
        }
        public STATUS status;
        public Rectangle rect;
    }

    /// <summary>
    /// ShuffleAnimation クラス
    /// </summary>
    class ShuffleAnimation
    {
        Texture2D spriteStrip;
        float scale;
        Color color;
        public bool Active;
        public Box LeftBox;
        public Box RightBox;
        private Vector2 left;
        private Vector2 right;
        private int frameCount;
        private int currentFrame;
        private float step;

        public void Initialize(Texture2D texture, ref Box leftBox, ref Box rightBox, int frameCount, Color color, float scale)
        {
            this.color = color;
            this.scale = scale;
            this.LeftBox = leftBox;
            this.RightBox = rightBox;
            this.frameCount = frameCount;

            spriteStrip = texture;
            Active = true;
            currentFrame = 0;
            left = new Vector2(leftBox.rect.X, leftBox.rect.Y);
            right = new Vector2(rightBox.rect.X, rightBox.rect.Y);
            step = (right.X - left.X) / frameCount;
        }

        public void Update(TimeSpan elapsedGameTime)
        {
            left.X += step;
            right.X -= step;

            if (frameCount > 0)
            {
                currentFrame++;
                if (currentFrame >= frameCount)
                {
                    int status = (int)LeftBox.status;
                    LeftBox.status = RightBox.status;
                    RightBox.status = (Box.STATUS)status;
                    Active = false;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
            {
                spriteBatch.Draw(spriteStrip, left, color);
                spriteBatch.Draw(spriteStrip, right, color);
            }
        }

        public Boolean isShuffle(Box target)
        {
            return (LeftBox.Equals(target) || RightBox.Equals(target));
        }
    }

    /// <summary>
    /// 基底 Game クラスから派生した、ゲームのメイン クラスです。
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        enum IMG : int
        {
            CLOSE_BOX, OPEN_BOX
        }

        enum SCENE : int
        {
            INIT, READY, SHUFFLE, SELECT, WIN, LOSE
        }

        Texture2D[] img = new Texture2D[2];
        SpriteFont text;
        Box[] box = new Box[3];
        SCENE scene = SCENE.INIT;
        Queue<ShuffleAnimation> shuffleAnimations;
        ShuffleAnimation animation;
        int level = 1;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // 画面を初期化
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;

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
            img[(int)IMG.CLOSE_BOX] = Content.Load<Texture2D>("IG005");
            img[(int)IMG.OPEN_BOX] = Content.Load<Texture2D>("IG009");
            text = Content.Load<SpriteFont>("Text");
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
                Random rnd = new Random();
                int w = img[(int)IMG.OPEN_BOX].Width;
                int h = img[(int)IMG.OPEN_BOX].Height;
                int y = (graphics.PreferredBackBufferHeight - h) / 2;
                int x = (graphics.PreferredBackBufferWidth - (w * box.Length)) / 2;

                // 状態を初期化
                for (int i = 0; i < box.Length; i++)
                {
                    box[i] = new Box();
                    box[i].status = Box.STATUS.MISS;
                    box[i].rect.Width = w;
                    box[i].rect.Height = h;
                    box[i].rect.X = 60 + ((w + 40) * i);
                    box[i].rect.Y = y;
                }
                box[rnd.Next(0, box.Length - 1)].status = Box.STATUS.HIT;
                shuffleAnimations = new Queue<ShuffleAnimation>();
                scene = SCENE.READY;
            }
            else if (scene == SCENE.READY)
            {
                // タッチを検出
                while (TouchPanel.IsGestureAvailable)
                {
                    GestureSample gs = TouchPanel.ReadGesture();
                    if (gs.GestureType == GestureType.Tap)
                    {
                        // シャッフル
                        int[] shuffle;
                        int frameCount = 120 - ((level - 1) * 30);
                        int times = (3 + level > 8 ? 8 : 3 + level);
                        ShuffleAnimation anim;
                        for (int i = 0; i < times; i++)
                        {
                            anim = new ShuffleAnimation();
                            shuffle = Enumerable.Range(0, box.Length).ToArray().OrderBy(n => Guid.NewGuid()).ToArray();
                            anim.Initialize(img[(int)IMG.CLOSE_BOX], ref box[shuffle[0]], ref box[shuffle[1]], (frameCount < 20 ? 20 : frameCount), Color.White, 1.0f);
                            shuffleAnimations.Enqueue(anim);
                        }
                        animation = shuffleAnimations.Dequeue();
                        scene = SCENE.SHUFFLE;
                    }
                }
            }
            else if (scene == SCENE.SHUFFLE)
            {
                // シャッフルアニメーション処理
                if (animation.Active)
                {
                    animation.Update(gameTime.ElapsedGameTime);
                }
                if (shuffleAnimations.Count <= 0)
                {
                    scene = SCENE.SELECT;
                }
            }
            else if (scene == SCENE.SELECT)
            {
                Rectangle rect;

                // タッチを検出
                while (TouchPanel.IsGestureAvailable)
                {
                    GestureSample gs = TouchPanel.ReadGesture();
                    if (gs.GestureType == GestureType.Tap)
                    {
                        rect = new Rectangle((int)gs.Position.X, (int)gs.Position.Y, 10, 10);
                        for (int i = 0; i < box.Length; i++)
                        {
                            if (rect.Intersects(box[i].rect))
                            {
                                scene = (box[i].status == Box.STATUS.HIT ? SCENE.WIN : SCENE.LOSE);
                            }

                        }
                    }
                }
            }
            else if ((scene == SCENE.WIN) || (scene == SCENE.LOSE))
            {
                // タッチを検出
                while (TouchPanel.IsGestureAvailable)
                {
                    GestureSample gs = TouchPanel.ReadGesture();
                    if (gs.GestureType == GestureType.Tap)
                    {
                        if (scene == SCENE.WIN)
                        {
                            level++;
                        }
                        else
                        {
                            level = 1;
                        }
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
            GraphicsDevice.Clear(Color.DarkGreen);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // テキストを描画
            String level_text = "Level: " + level;
            spriteBatch.DrawString(text, level_text, new Vector2(10, 10), Color.White);

            // シーンによって処理を分岐
            if (scene == SCENE.READY)
            {
                // 宝箱を描画
                for (int i = 0; i < box.Length; i++)
                {
                    spriteBatch.Draw(img[(int)box[i].status], new Vector2(box[i].rect.X, box[i].rect.Y), Color.White);
                }

                // テキストを描画
                String message_text = "Start by tapping shuffle";
                spriteBatch.DrawString(text, message_text, new Vector2(((graphics.PreferredBackBufferWidth - text.MeasureString(message_text).X) / 2), graphics.PreferredBackBufferHeight * 4 / 5), Color.White);
            }
            else if (scene == SCENE.SHUFFLE)
            {

                // 宝箱を描画
                for (int i = 0; i < box.Length; i++)
                {
                    if (!animation.isShuffle(box[i]))
                    {
                        spriteBatch.Draw(img[(int)IMG.CLOSE_BOX], new Vector2(box[i].rect.X, box[i].rect.Y), Color.White);
                    }
                }

                // シャッフルアニメーションを描画
                if (animation.Active)
                {
                    animation.Draw(spriteBatch);
                }
                else
                {
                    animation = shuffleAnimations.Dequeue();
                }
            }
            else if (scene == SCENE.SELECT)
            {
                // 宝箱を描画
                for (int i = 0; i < box.Length; i++)
                {
                    spriteBatch.Draw(img[(int)IMG.CLOSE_BOX], new Vector2(box[i].rect.X, box[i].rect.Y), Color.White);
                }

                // テキストを描画
                String message_text = "Tap a box";
                spriteBatch.DrawString(text, message_text, new Vector2(((graphics.PreferredBackBufferWidth - text.MeasureString(message_text).X) / 2), graphics.PreferredBackBufferHeight * 4 / 5), Color.White);
            }
            else if ((scene == SCENE.WIN) || (scene == SCENE.LOSE))
            {
                // 宝箱を描画
                for (int i = 0; i < box.Length; i++)
                {
                    spriteBatch.Draw(img[(int)box[i].status], new Vector2(box[i].rect.X, box[i].rect.Y), Color.White);
                }

                // テキストを描画
                String message_text = "You " + (scene == SCENE.WIN ? "win!" : "lose!");
                spriteBatch.DrawString(text, message_text, new Vector2(((graphics.PreferredBackBufferWidth - text.MeasureString(message_text).X) / 2), graphics.PreferredBackBufferHeight * 4 / 5), Color.White);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
