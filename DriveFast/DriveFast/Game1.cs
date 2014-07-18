using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace DriveFast
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;


        private Texture2D mCar;
        private Texture2D mCar_red;
        private Texture2D mCar_grey;
        private Texture2D mCar_yellow;
        private Texture2D mCar_red_2;
        private Texture2D mCar_grey_2;
        private Texture2D mCar_yellow_2;
        private Texture2D mBackground;
        private Texture2D mRoad;
        private Texture2D mHazard;
        private Texture2D mCoin;

        private KeyboardState mPreviousKeyboardState;

        private Vector2 mCarPosition = new Vector2(290, 440);//车位置
        private Vector2 mCarDrawPos = new Vector2(200, 440);
        private int mMoveCarX = 140;//移动车的距离
        private int mVelocityY;
        private int levels;
        private double mNextHazardAppearsIn;
        private double mNextCoinAppearsIn;
        private int mCarsRemaining;//剩余车辆
        private int mHazardsPassed;//经历障碍
        private int mCoinAte;
        private int scores;
        private int mIncreaseVelocity;
        private double mExitCountDown = 10;//停止倒计时，10s
        private int Carselection = 1;
        private double invisibletime = 5;


        private Song bgm;
        private SoundEffect crashsound;
        private SoundEffect coinsound;
        bool mute = false;
        bool Carinvisible = false;

        private int[] mRoadY = new int[2];
        private List<Hazard> mHazards = new List<Hazard>();
        private List<Coin> mCoins = new List<Coin>();
        // 定义随机数 - 比方用来表示障碍物的位置
        private Random mRandom = new Random();

        private SpriteFont mFont;

        //----------------------- Feng ---------------------
        // 自定义枚举类型，表明不同的游戏状态
        private enum State
        {
            TitleScreen,      // 初始片头
            Running,
            Crash,           // 碰撞
            GameOver,
            Success,
            Paused
        }
        //--------------------- Tian --------------------------


        private State mCurrentState = State.TitleScreen;//当前状态为title

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // 定义游戏窗口大小
            graphics.PreferredBackBufferHeight = 600;//窗口800*600
            graphics.PreferredBackBufferWidth = 800;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            mCar = Content.Load<Texture2D>("Images/Car");
            mCar_grey = Content.Load<Texture2D>("Images/car_grey");
            mCar_red = Content.Load<Texture2D>("Images/car_red");
            mCar_yellow = Content.Load<Texture2D>("Images/car_yellow");
            mCar_grey_2 = Content.Load<Texture2D>("Images/car_grey_2");
            mCar_red_2 = Content.Load<Texture2D>("Images/car_red_2");
            mCar_yellow_2 = Content.Load<Texture2D>("Images/car_yellow_2");
            mBackground = Content.Load<Texture2D>("Images/background2");
            mRoad = Content.Load<Texture2D>("Images/Road");
            mHazard = Content.Load<Texture2D>("Images/Hazard");
            bgm = Content.Load<Song>("Music/the ride");
            crashsound = Content.Load<SoundEffect>("Music/Carcrash");
            coinsound = Content.Load<SoundEffect>("Music/coinsound");
            mCoin = Content.Load<Texture2D>("Images/Coin");
            // 定义字体
            mFont = Content.Load<SpriteFont>("MyFont");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected void StartGame()
        {
            mRoadY[0] = 0;
            mRoadY[1] = -1 * mRoad.Height;


            mHazardsPassed = 0;
            mCoinAte = 0;
            scores = 0;
            levels = 0;
            mCarsRemaining = 3; // 所剩车辆的数量
            mVelocityY = 5;//向下滚动的速度
            mNextHazardAppearsIn = 1.5;//下一个障碍时间
            mNextCoinAppearsIn = 1.5;
            mIncreaseVelocity = 5;  // 速度增加的速度，单位为障碍数

            mHazards.Clear();

            mCurrentState = State.Running;
            if (!mute)
                MediaPlayer.Play(bgm);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState aCurrentKeyboardState = Keyboard.GetState();//键盘状态

            //Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                aCurrentKeyboardState.IsKeyDown(Keys.Escape) == true)
            {
                this.Exit();//ESC,XBOX back退出
            }

            switch (mCurrentState)//状态机
            {
                case State.TitleScreen:
                    {
                        if (aCurrentKeyboardState.IsKeyDown(Keys.Left) == true && mPreviousKeyboardState.IsKeyDown(Keys.Left) == false)
                        {
                            if (Carselection > 1)
                                Carselection -= 1;
                        }
                        if (aCurrentKeyboardState.IsKeyDown(Keys.Right) == true && mPreviousKeyboardState.IsKeyDown(Keys.Right) == false)
                        {
                            if (Carselection < 3)
                                Carselection += 1;
                        }
                        ExitCountdown(gameTime);//退出计时

                        if (aCurrentKeyboardState.IsKeyDown(Keys.Space) == true && mPreviousKeyboardState.IsKeyDown(Keys.Space) == false)//按下space，Δ
                        {
                            StartGame();
                        }
                        break;
                    }

                case State.Success:
                case State.GameOver:
                    {
                        ExitCountdown(gameTime);//退出计时

                        if (aCurrentKeyboardState.IsKeyDown(Keys.Space) == true && mPreviousKeyboardState.IsKeyDown(Keys.Space) == false)//按下space，Δ
                        {
                            StartGame();
                        }
                        break;
                    }

                case State.Running:
                    {
                        //If the user has pressed the Spacebar, then make the car switch lanes
                        /*if (aCurrentKeyboardState.IsKeyDown(Keys.Space) == true && mPreviousKeyboardState.IsKeyDown(Keys.Space) == false)
                        {
                            mCarPosition.X += mMoveCarX;
                            mMoveCarX *= -1;
                        }*/
                        if (MediaPlayer.State == MediaState.Stopped)
                        {
                            MediaPlayer.Play(bgm);
                        }
                        if (aCurrentKeyboardState.IsKeyDown(Keys.P) == true && mPreviousKeyboardState.IsKeyDown(Keys.P) == false)
                        {
                            mCurrentState = State.Paused;
                            MediaPlayer.Pause();
                            //Pause(); 
                        }
                        if (aCurrentKeyboardState.IsKeyDown(Keys.Left) == true && mPreviousKeyboardState.IsKeyDown(Keys.Left) == false)
                        {
                            if (mCarPosition.X > 230)
                                mCarPosition.X -= mMoveCarX;
                            //mMoveCarX *= -1;
                        }
                        if (aCurrentKeyboardState.IsKeyDown(Keys.Right) == true && mPreviousKeyboardState.IsKeyDown(Keys.Right) == false)
                        {
                            if (mCarPosition.X < 560)
                                mCarPosition.X += mMoveCarX;
                            //mMoveCarX *= -1;
                        }

                        ScrollRoad();

                        foreach (Hazard aHazard in mHazards)
                        {
                            if (CheckCollision(aHazard) == true)
                            {
                                break;
                            }

                            MoveHazard(aHazard);
                        }

                        foreach (Coin aCoin in mCoins)
                        {
                            if (aCoin.Visible == true && CheckCollisionCoin(aCoin) == true)
                            {
                                aCoin.Visible = false;
                                mCoinAte++;
                                if (mCoinAte == 10)
                                {
                                    //mCarsRemaining += 1;
                                    Carinvisible = true;
                                    mCoinAte = 0;
                                }


                                //scores += 10*levels;  // when you ate coin, add some scores
                                break;
                            }
                            MoveCoin(aCoin);
                        }
                        UpdateHazards(gameTime);
                        UpdateCoins(gameTime);
                        break;
                    }
                case State.Crash:
                    {
                        //If the user has pressed the Space key, then resume driving
                        if (aCurrentKeyboardState.IsKeyDown(Keys.Space) == true && mPreviousKeyboardState.IsKeyDown(Keys.Space) == false)
                        {
                            mHazards.Clear();
                            mCoins.Clear();
                            mCurrentState = State.Running;
                        }

                        break;
                    }
                case State.Paused:
                    {
                        if (aCurrentKeyboardState.IsKeyDown(Keys.P) == true && mPreviousKeyboardState.IsKeyDown(Keys.P) == false)
                        {
                            mCurrentState = State.Running;
                            MediaPlayer.Resume();
                            //Resume();
                        }
                        break;
                    }
            }

            if (aCurrentKeyboardState.IsKeyDown(Keys.M) == true && mPreviousKeyboardState.IsKeyDown(Keys.M) == false && mute == false)
            {
                mute = true;
                MediaPlayer.IsMuted = true;
            }

            else if (aCurrentKeyboardState.IsKeyDown(Keys.M) == true && mPreviousKeyboardState.IsKeyDown(Keys.M) == false && mute == true)
            {
                mute = false;
                MediaPlayer.IsMuted = false;
            }

            mPreviousKeyboardState = aCurrentKeyboardState;//键盘状态更新
            base.Update(gameTime);
        }

        //----------------------- Feng ---------------------
        // 让路面向后移动（使车辆看起来在往前行）
        private void ScrollRoad()
        {
            //Move the scrolling Road
            for (int aIndex = 0; aIndex < mRoadY.Length; aIndex++)
            {
                if (mRoadY[aIndex] >= this.Window.ClientBounds.Height) // 检测路面有没有移出游戏窗口
                {
                    int aLastRoadIndex = aIndex;
                    for (int aCounter = 0; aCounter < mRoadY.Length; aCounter++)
                    {
                        if (mRoadY[aCounter] < mRoadY[aLastRoadIndex])
                        {
                            aLastRoadIndex = aCounter;
                        }
                    }
                    mRoadY[aIndex] = mRoadY[aLastRoadIndex] - mRoad.Height; // 改变Y坐标，让路移动
                }
            }

            for (int aIndex = 0; aIndex < mRoadY.Length; aIndex++)
            {
                mRoadY[aIndex] += mVelocityY;
            }
        }
        //----------------------- Tian ---------------------

        private void MoveHazard(Hazard theHazard)
        {
            theHazard.Position.Y += mVelocityY;
            if (theHazard.Position.Y > graphics.GraphicsDevice.Viewport.Height && theHazard.Visible == true)
            {
                theHazard.Visible = false;
                mHazardsPassed += 1;
                scores = scores + 10 + mVelocityY;
                if (mHazardsPassed >= 400) // 如果通过400个障碍物，成功！
                {
                    mCurrentState = State.Success;
                    mExitCountDown = 10;
                }

                mIncreaseVelocity -= 1;
                if (mIncreaseVelocity < 0)//每5次加速
                {
                    mIncreaseVelocity = 5;
                    mVelocityY += 1;
                    levels += 1;
                }
                if (mVelocityY > 25) mVelocityY = 25;//限制最大速度
            }
        }
        private void MoveCoin(Coin theCoin)
        {
            int magic = mRandom.Next(0, 10);
            theCoin.Position.Y += mVelocityY + magic;
            if (theCoin.Position.Y > graphics.GraphicsDevice.Viewport.Height && theCoin.Visible == true)
            {
                theCoin.Visible = false;
            }
        }


        private void UpdateHazards(GameTime theGameTime)
        {
            mNextHazardAppearsIn -= theGameTime.ElapsedGameTime.TotalSeconds; //1.5s刷一个障碍
            if (Carinvisible == true)
            {
                invisibletime -= theGameTime.ElapsedGameTime.TotalSeconds;
                if (invisibletime < 0)
                {

                    Carinvisible = false;
                    invisibletime = 5;
                }
            }



            if (mNextHazardAppearsIn < 0)//要出现的话
            {
                int aLowerBound = 24 - (mVelocityY * 2);
                int aUpperBound = 30 - (mVelocityY * 2);

                if (mVelocityY > 10)
                {
                    aLowerBound = 6;
                    aUpperBound = 8;
                }

                // 控制障碍物出现的位置（随机）??是下一个出现的时间吧
                mNextHazardAppearsIn = (double)mRandom.Next(aLowerBound, aUpperBound) / 10;
                AddHazard();
            }
        }

        //@author yang 
        //just like the title, we update the coins apperance
        private void UpdateCoins(GameTime theGameTime)
        {
            mNextCoinAppearsIn -= theGameTime.ElapsedGameTime.TotalSeconds;
            if (mNextCoinAppearsIn < 0)
            {
                int aLowerBound = 24 - (mVelocityY * 2);
                int aUpperBound = 30 - (mVelocityY * 2);

                if (mVelocityY > 10)
                {
                    aLowerBound = 6;
                    aUpperBound = 8;
                }

                mNextCoinAppearsIn = (double)mRandom.Next(aLowerBound, aUpperBound) / 10;
                AddCoin();
            }
        }

        private void AddHazard()
        {
            int aRoadPosition = mRandom.Next(1, 5);//哪条道出障碍
            int aPosition = 10 + 140 * aRoadPosition;


            bool aAddNewHazard = true;
            foreach (Hazard aHazard in mHazards)
            {
                if (aHazard.Visible == false)
                {
                    aAddNewHazard = false;
                    aHazard.Visible = true;
                    aHazard.Position = new Vector2(aPosition, -mHazard.Height);
                    break;
                }
            }

            if (aAddNewHazard == true)
            {
                //Add a hazard to the left side of the Road
                Hazard aHazard = new Hazard();
                aHazard.Position = new Vector2(aPosition, -mHazard.Height);

                mHazards.Add(aHazard);
            }
        }

        //@author yang
        // add coin into the list mCoins
        private void AddCoin()
        {
            int aRoadPosition = mRandom.Next(1, 5);//哪条道出金币
            int aPosition = 30 + 140 * aRoadPosition;


            bool aAddNewCoin = true;
            foreach (Coin aCoin in mCoins)
            {
                if (aCoin.Visible == false)
                {
                    aAddNewCoin = false;
                    aCoin.Visible = true;
                    aCoin.Position = new Vector2(aPosition, -mCoin.Height);
                    break;
                }
            }

            if (aAddNewCoin == true)
            {
                //Add a hazard to the left side of the Road
                Coin aCoin = new Coin();
                aCoin.Position = new Vector2(aPosition, -mCoin.Height);

                mCoins.Add(aCoin);
            }
        }
        //----------------------- Feng ------------------------------------------------
        // 检测车辆是否碰到了障碍物
        private bool CheckCollision(Hazard theHazard)
        {
            // 分别计算并使用封闭（包裹）盒给障碍物和车
            if (Carinvisible == true)
            {
                return false;
            }


            BoundingBox aHazardBox = new BoundingBox(new Vector3(theHazard.Position.X, theHazard.Position.Y, 0), new Vector3(theHazard.Position.X + (mHazard.Width * .4f), theHazard.Position.Y + ((mHazard.Height - 50) * .4f), 0));
            BoundingBox aCarBox = new BoundingBox(new Vector3(mCarPosition.X, mCarPosition.Y, 0), new Vector3(mCarPosition.X + (mCar.Width * .2f), mCarPosition.Y + (mCar.Height * .2f), 0));

            if (aHazardBox.Intersects(aCarBox) == true) // 碰上了吗?
            {
                mCurrentState = State.Crash;
                if (!mute)
                    crashsound.Play();
                mCarsRemaining -= 1;
                if (mCarsRemaining < 0)
                {
                    mCurrentState = State.GameOver;
                    mExitCountDown = 10;
                }
                return true;
            }

            return false;
        }
        //@author yang  
        //detect the coin collision and and calculate the total coins we eat 
        private bool CheckCollisionCoin(Coin theCoin)
        {
            // 分别计算并使用封闭（包裹）盒给障碍物和车
            BoundingBox aCoinBox = new BoundingBox(new Vector3(theCoin.Position.X, theCoin.Position.Y, 0), new Vector3(theCoin.Position.X + (mHazard.Width * .4f), theCoin.Position.Y + ((mHazard.Height - 50) * .4f), 0));
            BoundingBox aCarBox = new BoundingBox(new Vector3(mCarPosition.X, mCarPosition.Y, 0), new Vector3(mCarPosition.X + (mCar.Width * .2f), mCarPosition.Y + (mCar.Height * .2f), 0));

            if (aCoinBox.Intersects(aCarBox) == true) // 碰上了吗?
            {
                if (!mute)
                    coinsound.Play();
                scores += 15;
                return true;
            }

            return false;
        }
        //----------------------- Tian ------------------------------------------------------

        private void ExitCountdown(GameTime theGameTime)
        {
            mExitCountDown -= theGameTime.ElapsedGameTime.TotalSeconds;//退出倒计时
            if (mExitCountDown < 0)
            {
                this.Exit();
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.Draw(mBackground, new Rectangle(graphics.GraphicsDevice.Viewport.X, graphics.GraphicsDevice.Viewport.Y, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height), Color.White);

            switch (mCurrentState)
            {
                case State.TitleScreen:
                    {
                        //Draw the display text for the Title screen
                        //第二参数是Y轴
                        DrawTextCentered("Drive Fast And Avoid the 400 Oncoming Obstacles", 200);
                        DrawTextCentered("Press 'Space' to begin", 260);
                        DrawTextCentered("Press 'Left' and 'Right' to select and control your car", 300);
                        DrawTextCentered("Press 'M' to mute", 340);
                        DrawTextCentered("Press 'P' to pause", 380);
                        DrawTextCentered("Exit in " + ((int)mExitCountDown).ToString(), 475);
                        if (Carselection == 1)
                            spriteBatch.Draw(mCar_red, mCarDrawPos, new Rectangle(0, 0, mCar.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.2f, SpriteEffects.None, 0);
                        else if (Carselection == 2)
                            spriteBatch.Draw(mCar_yellow, mCarDrawPos, new Rectangle(0, 0, mCar.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.2f, SpriteEffects.None, 0);
                        else if (Carselection == 3)
                            spriteBatch.Draw(mCar_grey, mCarDrawPos, new Rectangle(0, 0, mCar.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.2f, SpriteEffects.None, 0);
                        break;
                    }

                default:
                    {
                        DrawRoad();
                        DrawHazards();
                        DrawCoins();
                        //spriteBatch.Draw(mCar, mCarPosition, new Rectangle(0, 0, mCar.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.2f, SpriteEffects.None, 0);
                        if (!Carinvisible)
                        {
                            if (Carselection == 1)
                                spriteBatch.Draw(mCar_red, mCarPosition, new Rectangle(0, 0, mCar.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.2f, SpriteEffects.None, 0);
                            else if (Carselection == 2)
                                spriteBatch.Draw(mCar_yellow, mCarPosition, new Rectangle(0, 0, mCar.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.2f, SpriteEffects.None, 0);
                            else if (Carselection == 3)
                                spriteBatch.Draw(mCar_grey, mCarPosition, new Rectangle(0, 0, mCar.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.2f, SpriteEffects.None, 0);
                        }
                        else 
                        {
                            if (Carselection == 1)
                                spriteBatch.Draw(mCar_red_2, mCarPosition, new Rectangle(0, 0, mCar.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.2f, SpriteEffects.None, 0);
                            else if (Carselection == 2)
                                spriteBatch.Draw(mCar_yellow_2, mCarPosition, new Rectangle(0, 0, mCar.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.2f, SpriteEffects.None, 0);
                            else if (Carselection == 3)
                                spriteBatch.Draw(mCar_grey_2, mCarPosition, new Rectangle(0, 0, mCar.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.2f, SpriteEffects.None, 0);
                        }

                        spriteBatch.DrawString(mFont, "Cars:", new Vector2(28, 520), Color.Brown, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);

                        for (int aCounter = 0; aCounter < mCarsRemaining; aCounter++)//画命的
                        {
                            //spriteBatch.Draw(mCar, new Vector2(25 + (30 * aCounter), 550), new Rectangle(0, 0, mCar.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.05f, SpriteEffects.None, 0);
                            if (Carselection == 1)
                                spriteBatch.Draw(mCar_red, new Vector2(25 + (30 * aCounter), 550), new Rectangle(0, 0, mCar.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.05f, SpriteEffects.None, 0);
                            else if (Carselection == 2)
                                spriteBatch.Draw(mCar_yellow, new Vector2(25 + (30 * aCounter), 550), new Rectangle(0, 0, mCar.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.05f, SpriteEffects.None, 0);
                            else if (Carselection == 3)
                                spriteBatch.Draw(mCar_grey, new Vector2(25 + (30 * aCounter), 550), new Rectangle(0, 0, mCar.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.05f, SpriteEffects.None, 0);
                        }

                        spriteBatch.DrawString(mFont, "Hazards: " + mHazardsPassed.ToString(), new Vector2(5, 25), Color.Brown, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);//过障碍数
                        spriteBatch.DrawString(mFont, "levels: " + levels.ToString(), new Vector2(5, 45), Color.Brown, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);//过障碍数
                        spriteBatch.DrawString(mFont, "Scores: " + scores.ToString(), new Vector2(5, 65), Color.Brown, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
                        spriteBatch.DrawString(mFont, "Mute: " + mute.ToString(), new Vector2(5, 85), Color.Brown, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
                        spriteBatch.DrawString(mFont, "Coins: " + mCoinAte.ToString(), new Vector2(5, 105), Color.Brown, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
                        spriteBatch.DrawString(mFont, "invisible: " + Carinvisible.ToString(), new Vector2(5, 125), Color.Brown, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
                        if (mCurrentState == State.Crash)//撞毁
                        {
                            DrawTextDisplayArea();

                            DrawTextCentered("Crash!", 200);
                            DrawTextCentered("Press 'Space' to continue driving.", 260);
                        }
                        else if (mCurrentState == State.GameOver)//死亡
                        {
                            DrawTextDisplayArea();

                            DrawTextCentered("Game Over.", 200);
                            DrawTextCentered("Press 'Space' to try again.", 260);
                            DrawTextCentered("Exit in " + ((int)mExitCountDown).ToString(), 400);

                        }
                        else if (mCurrentState == State.Success)//胜利
                        {
                            DrawTextDisplayArea();

                            DrawTextCentered("Congratulations!", 200);
                            DrawTextCentered("You get scores: " + scores.ToString(), 240);
                            DrawTextCentered("Press 'Space' to play again.", 300);
                            DrawTextCentered("Exit in " + ((int)mExitCountDown).ToString(), 400);
                        }
                        else if (mCurrentState == State.Paused)
                        {
                            DrawTextDisplayArea();
                            DrawTextCentered("Paused", 200);
                            DrawTextCentered("Press 'P' to resume.", 240);
                        }

                        break;
                    }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawRoad()
        {
            for (int aIndex = 0; aIndex < mRoadY.Length; aIndex++)
            {
                if (mRoadY[aIndex] > mRoad.Height * -1 && mRoadY[aIndex] <= this.Window.ClientBounds.Height)
                {
                    spriteBatch.Draw(mRoad, new Rectangle((int)((this.Window.ClientBounds.Width - mRoad.Width) / 2), mRoadY[aIndex], mRoad.Width, mRoad.Height + 5), Color.White);
                }
            }
        }

        private void DrawHazards()
        {
            foreach (Hazard aHazard in mHazards)
            {
                if (aHazard.Visible == true)
                {
                    spriteBatch.Draw(mHazard, aHazard.Position, new Rectangle(0, 0, mHazard.Width, mHazard.Height), Color.White, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, 0);
                }
            }
        }
        //@author yang
        // draw the coins on screen
        private void DrawCoins()
        {
            foreach (Coin aCoin in mCoins)
            {
                if (aCoin.Visible == true)
                {
                    spriteBatch.Draw(mCoin, aCoin.Position, new Rectangle(0, 0, mCoin.Width, mCoin.Height), Color.White, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
                }
            }
        }
        private void DrawTextDisplayArea()//背景显示出来
        {
            int aPositionX = (int)((graphics.GraphicsDevice.Viewport.Width / 2) - (450 / 2));
            spriteBatch.Draw(mBackground, new Rectangle(aPositionX, 75, 450, 400), Color.White);
        }

        private void DrawTextCentered(string theDisplayText, int thePositionY)//中间写文本
        {
            Vector2 aSize = mFont.MeasureString(theDisplayText);//计算字符串文本像素
            int aPositionX = (int)((graphics.GraphicsDevice.Viewport.Width / 2) - (aSize.X / 2));//放到中间

            spriteBatch.DrawString(mFont, theDisplayText, new Vector2(aPositionX, thePositionY), Color.Beige, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
            spriteBatch.DrawString(mFont, theDisplayText, new Vector2(aPositionX + 1, thePositionY + 1), Color.Brown, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
        }
    }
}
