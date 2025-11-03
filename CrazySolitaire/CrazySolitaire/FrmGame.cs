using CrazySolitaire.Properties;
using System.Diagnostics;

namespace CrazySolitaire {
    public partial class FrmGame : Form
    {
        private static Stopwatch _stopwatch = new Stopwatch();
        private static System.Windows.Forms.Timer _uiTimer = new System.Windows.Forms.Timer();
        public static List<Card> CurDragCards { get; private set; }
        public static IDragFrom CardDraggedFrom { get; private set; }
        public static FrmGame Instance { get; private set; }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;    // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        public FrmGame()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Instance = this;
            Panel[] panTableauStacks = new Panel[7];
            for (int i = 0; i < 7; i++)
            {
                panTableauStacks[i] = (Panel)Controls.Find($"panTableauStack_{i}", false)[0];
            }
            Dictionary<Suit, Panel> panFoundationStacks = new()
            {
                [Suit.DIAMONDS] = panFoundationStack_Diamonds,
                [Suit.SPADES] = panFoundationStack_Spades,
                [Suit.HEARTS] = panFoundationStack_Hearts,
                [Suit.CLUBS] = panFoundationStack_Clubs,
            };
            Game.Init(panTalon, panTableauStacks, panFoundationStacks);

            _stopwatch.Restart();
            _uiTimer.Start();
            _uiTimer.Interval = 100;
            _uiTimer.Tick += UiTimer_Tick;
        }
        private void UiTimer_Tick(object sender, EventArgs e)
        {
            Timer.Text = FormatTime(_stopwatch.Elapsed);
        }

        private string FormatTime(TimeSpan ts)
        {
            int totalSeconds = (int)ts.TotalSeconds;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes}:{seconds:00}";
        }

        public static void stopTime()
        {
            _uiTimer.Stop();
            _stopwatch.Stop();
        }

        private void pbStock_Click(object sender, EventArgs e)
        {
            if (pbStock.BackgroundImage is null)
            {
                Game.StockReloadCount++;
                if (Game.StockReloadCount > 3)
                {
                    Game.Explode();
                    MessageBox.Show("You computer has been infected with ransomware", "You have been infected", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FrmYouLose frmYouLose = new();
                    frmYouLose.Show();
                    Hide();
                }
                else
                {
                    Game.Talon.ReleaseIntoDeck(Game.Deck);
                    pbStock.BackgroundImage = Game.StockReloadCount switch
                    {
                        1 => Resources.back_green,
                        2 => Resources.back_orange,
                        3 => Resources.back_red,
                    };
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    Card c = Game.Deck.Acquire();
                    if (c != null)
                    {
                        Game.Talon.AddCard(c);
                        c.AdjustLocation(i * 20, 0);
                        c.PicBox.BringToFront();
                    }
                }
                if (Game.Deck.IsEmpty())
                {
                    pbStock.BackgroundImage = null;
                }
            }
        }

        public static void DragCards(List<Card> cards)
        {
            if (cards == null || cards.Count == 0)
                return;

            CurDragCards = cards;
            CardDraggedFrom = Game.FindDragFrom(cards[0]);
        }
        public static void StopDragCards()
        {
            CurDragCards.Clear();
            CardDraggedFrom = null;
        }
        public static bool IsDraggingCard(Card c) => CurDragCards.Contains(c);

        public static bool IsDragging => CurDragCards.Count > 0;

        private void FrmGame_FormClosing(object sender, FormClosingEventArgs e)
        {
            Game.TitleForm.Close();
        }
    }
}
