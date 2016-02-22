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

namespace ruksireikä
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D playfield; //contains playfield bg image
        Rectangle playArea; //rectanlge containing all visible gameobjects

        //cross and zero textures
        Texture2D cross;
        Texture2D zero;

        SpriteFont spriteFont;

        //variables for states
        MouseState mouseState;
        KeyboardState keyboardState;
        Point mousePosition;

        Rectangle[] cell; //contains rectangles for each cell in the play grid
        List<Rectangle> P1CellList; //contains a list of cells p1 has clicked on
        List<Rectangle> P2CellList; //contains a list of cells p2 has clicked on

        Rectangle[][] winStates; //contains all possible win states, jagged array because i forgot how arrays work \:D/

        bool playerOneTurn = true;
        bool drawCells = false;
        bool gameEnded = false;
        bool playerOneWon = false;
        bool playerTwoWon = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            IsMouseVisible = true;

            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferHeight = 480;
            graphics.PreferredBackBufferWidth = 480;
        }

        protected override void Initialize()
        {
            // rectangle containing all the visible game elements
            playArea = GraphicsDevice.Viewport.TitleSafeArea;

            // array containing draw positions of each cell in the grid (playfield). using spriteBatch.Draw(cross, cell[0], Color.White) draws a cross at top left etc
            cell = new Rectangle[] { new Rectangle(playArea.X + playArea.Width / 3 * 0, playArea.Y + playArea.Height / 3 * 0, playArea.Width / 3, playArea.Height / 3),  //0,0
                                     new Rectangle(playArea.X + playArea.Width / 3 * 1, playArea.Y + playArea.Height / 3 * 0, playArea.Width / 3, playArea.Height / 3),  //1,0
                                     new Rectangle(playArea.X + playArea.Width / 3 * 2, playArea.Y + playArea.Height / 3 * 0, playArea.Width / 3, playArea.Height / 3),  //2,0
                                     new Rectangle(playArea.X + playArea.Width / 3 * 0, playArea.Y + playArea.Height / 3 * 1, playArea.Width / 3, playArea.Height / 3),  //0,1
                                     new Rectangle(playArea.X + playArea.Width / 3 * 1, playArea.Y + playArea.Height / 3 * 1, playArea.Width / 3, playArea.Height / 3),  //1,1
                                     new Rectangle(playArea.X + playArea.Width / 3 * 2, playArea.Y + playArea.Height / 3 * 1, playArea.Width / 3, playArea.Height / 3),  //2,1
                                     new Rectangle(playArea.X + playArea.Width / 3 * 0, playArea.Y + playArea.Height / 3 * 2, playArea.Width / 3, playArea.Height / 3),  //0,2
                                     new Rectangle(playArea.X + playArea.Width / 3 * 1, playArea.Y + playArea.Height / 3 * 2, playArea.Width / 3, playArea.Height / 3),  //1,2
                                     new Rectangle(playArea.X + playArea.Width / 3 * 2, playArea.Y + playArea.Height / 3 * 2, playArea.Width / 3, playArea.Height / 3)}; //2,2


            // contains each possible cell combination that results in a win state (3 long lines), jagged array because i forgot how arrays work \:D/
            winStates = new Rectangle[][] { new Rectangle[] { cell[0], cell[1], cell[2], },   //top row
                                            new Rectangle[] { cell[3], cell[4], cell[5], },   //middle row
                                            new Rectangle[] { cell[6], cell[7], cell[8], },   //bottom row
                                            new Rectangle[] { cell[0], cell[3], cell[6], },   //left column
                                            new Rectangle[] { cell[1], cell[4], cell[7], },   //middle column
                                            new Rectangle[] { cell[2], cell[5], cell[8], },   //right column
                                            new Rectangle[] { cell[0], cell[4], cell[8], },   //diagonal from top left to bottom right
                                            new Rectangle[] { cell[2], cell[4], cell[6], } }; //diagonal from top right to bottom left


            // lists conaining each players moves (clicked cells)
            P1CellList = new List<Rectangle>();
            P2CellList = new List<Rectangle>();

            playerOneTurn = true;
            playerOneWon = false;
            playerTwoWon = false;
            gameEnded = false;
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            spriteFont = Content.Load<SpriteFont>("SpriteFont1");

            playfield = Content.Load<Texture2D>("playfield");
            cross = Content.Load<Texture2D>("cross");
            zero = Content.Load<Texture2D>("zero");
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();

            mousePosition = new Point(mouseState.X, mouseState.Y);
            
            if (keyboardState.IsKeyDown(Keys.Escape) == true)
            {
                this.Exit();
            }
            
            if (mouseState.LeftButton == ButtonState.Pressed && !gameEnded)
            {
                //only start the logic once mouse button is relased, so there won't be multiple inputs
                while (mouseState.LeftButton == ButtonState.Pressed)
                {
                    mouseState = Mouse.GetState();
                }

                //save cell at clicked location to n players cellList
                foreach (Rectangle r in cell)
                {
                    if (r.Intersects(new Rectangle(mouseState.X, mouseState.Y, 0, 0)))
                    {
                        if (playerOneTurn)
                        {
                            if (!P1CellList.Contains(r) && !P2CellList.Contains(r))
                            {
                                P1CellList.Add(r);
                                playerOneTurn = !playerOneTurn;
                            }
                        }
                        else
                        {
                            if (!P1CellList.Contains(r) && !P2CellList.Contains(r))
                            {
                                P2CellList.Add(r);
                                playerOneTurn = !playerOneTurn;
                            }
                        }
                    }
                }


                if (P1CellList.Count != 0 || P2CellList.Count != 0)
                    drawCells = true;

                if (P1CellList.Count + P2CellList.Count == 9)
                    gameEnded = true;

                CheckWinState();

            }

            else if (mouseState.LeftButton == ButtonState.Pressed && gameEnded)
            {
                //only start the logic once mouse button is relased, so there won't be multiple inputs
                while (mouseState.LeftButton == ButtonState.Pressed)
                {
                    mouseState = Mouse.GetState();
                }

                P1CellList = new List<Rectangle>();
                P2CellList = new List<Rectangle>();

                playerOneTurn = true;
                playerOneWon = false;
                playerTwoWon = false;
                gameEnded = false;
            }
            
            base.Update(gameTime);
        }

        private void CheckWinState()
        {
            int n = 0;

            for (int i = 0; i < 8; i++)
            {
                foreach (Rectangle r in winStates[i])
                {
                    if (P1CellList.Contains(r))
                    {
                        n++;
                        if (n == 3)
                        {
                            n = 0;
                            playerOneWon = true;
                            gameEnded = true;
                            break;
                        }
                    }
                }
                n = 0;
            }

            for (int i = 0; i < 8; i++)
            {
                foreach (Rectangle r in winStates[i])
                {
                    if (P2CellList.Contains(r))
                    {
                        n++;
                        if (n == 3)
                        {
                            n = 0;
                            playerTwoWon = true;
                            gameEnded = true;
                            break;
                        }
                    }
                }
                n = 0;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(playfield, playArea, Color.White);

            if (drawCells)
            {
                foreach (Rectangle r in P1CellList)
                {
                    spriteBatch.Draw(cross, r, Color.White);
                }


                foreach (Rectangle r in P2CellList)
                {
                    spriteBatch.Draw(zero, r, Color.White);
                }
            }
            else
                spriteBatch.Draw(playfield, playArea, Color.White);

            if (playerOneWon && gameEnded)
            {
                spriteBatch.DrawString(spriteFont, "Player 1 won", new Vector2(playArea.X, playArea.Y), Color.MediumBlue);
            }
            else if (playerTwoWon && gameEnded)
            {
                spriteBatch.DrawString(spriteFont, "Player 2 won", new Vector2(playArea.X, playArea.Y), Color.MediumBlue);
            }
            else if (gameEnded && !playerOneWon && !playerTwoWon)
            {
                spriteBatch.DrawString(spriteFont, "Tied", new Vector2(playArea.X, playArea.Y), Color.MediumBlue);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
