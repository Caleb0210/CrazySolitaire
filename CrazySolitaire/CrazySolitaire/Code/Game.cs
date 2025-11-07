using CrazySolitaire.Properties;
using System.Diagnostics;
using System.Threading;
using Timer = System.Windows.Forms.Timer;

namespace CrazySolitaire;

public enum CardType
{
    ACE,
    _2,
    _3,
    _4,
    _5,
    _6,
    _7,
    _8,
    _9,
    _10,
    JACK,
    QUEEN,
    KING,
    WILD
}

public enum Suit
{
    DIAMONDS,
    SPADES,
    HEARTS,
    CLUBS,
    BLACK_JOKER,
    RED_JOKER
}

// Interface for objects that a card can be dragged from (tableau, talon)
public interface IDragFrom
{
    public void RemCard(Card card);
    public void AddCard(Card card);
}

// Interface for objects that can determine which cards are movable
public interface IFindMoveableCards
{
    public List<Card> FindMoveableCards();
}

// Interface for objects that can receive cards being dragged
public interface IDropTarget
{
    public void DragOver(Card c);
    public bool CanDrop(Card c);
    public void DragEnded();
    public void Dropped(Card c);
}

public static class MyExtensions
{
    public static void AddCard(this Control control, Card card)
    {
        if (card is not null)
        {
            control.Controls.Add(card.PicBox);
        }
    }

    public static void RemCard(this Control control, Card card)
    {
        if (card is not null)
        {
            control.Controls.Remove(card.PicBox);
        }
    }
}

public class Deck
{
    private Queue<Card> cards; // Queue to hold the cards in the deck

    public Deck()
    {
        RegeneratePool();
    }

    private void RegeneratePool()
    {
        cards = new();

        // add standard 52 card deck
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            if (cardType == CardType.WILD)
                continue;
            foreach (var suit in Enum.GetValues<Suit>())
            {
                if (suit == Suit.BLACK_JOKER || suit == Suit.RED_JOKER)
                    continue;
                cards.Enqueue(new(cardType, suit));
            }
        }

        // Add two wild cards
        cards.Enqueue(new(CardType.WILD, Suit.BLACK_JOKER));
        cards.Enqueue(new(CardType.WILD, Suit.RED_JOKER));

        // Add reverse card 
        Card reverseCard = new(CardType.WILD, Suit.BLACK_JOKER);
        reverseCard.SetAsReverseCard();
        cards.Enqueue(reverseCard);

        // Shuffle
        Random rng = new();
        cards = new(cards.OrderBy(_ => rng.Next()));
    }

    public bool IsEmpty() => cards.Count == 0;
    public Card Acquire() => (cards.Count > 0 ? cards.Dequeue() : null);
    public void Release(Card c) => cards.Enqueue(c);
}

public class Card
{
    public CardType Type { get; private set; }
    public Suit Suit { get; private set; }
    public bool FaceUp { get; private set; }
    public PictureBox PicBox { get; private set; }

    public bool IsReverseCard { get; private set; } = false; // Helps the game to know when to trigger the reverse mode

    // Safe loader that handles Bitmap or byte[] resources
    private static Bitmap LoadBitmapResource(string key)
    {
        object obj = Resources.ResourceManager.GetObject(key);
        if (obj is Bitmap bmp) return bmp;

        if (obj is byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            using (var img = Image.FromStream(ms))
            {
                // clone to detach from stream
                return new Bitmap(img);
            }
        }

        // fallback
        return Resources.back_green;
    }

    public Bitmap PicImg
    {
        get => FaceUp
            ? (IsReverseCard
                ? LoadBitmapResource("reverse_card") // special image for reverse card
                : (Type == CardType.WILD
                    ? Resources.ResourceManager.GetObject($"{Suit.ToString().ToLower()}") as Bitmap
                    : Resources.ResourceManager.GetObject($"{Type.ToString().Replace("_", "").ToLower()}_of_{Suit.ToString().ToLower()}") as Bitmap))
            : Resources.back_green;
    }
    // Fields used for drag-and-drop logic
    private Point dragOffset; // offset between mouse click and card position
    public Point relLocBeforeDrag { get; private set; } // original location before dragging
    private Control conBeforeDrag; // original parent control
    private IDropTarget lastDropTarget; // last potential drop area that was hovered over

    public Card(CardType type, Suit suit)
    {
        Type = type;
        Suit = suit;
        FaceUp = true;
        SetupPicBox();
    }

    // Sets a wildcard into a reverse card
    public void SetAsReverseCard()
    {
        IsReverseCard = true;
        if (PicBox == null) SetupPicBox();

        // shows the reverse card image
        PicBox.BackgroundImage = LoadBitmapResource("reverse_card");
        PicBox.Cursor = Cursors.Hand;

        // when clicked card disappears and toggles reverse mode
        PicBox.Click += (sender, e) =>
        {
            if (IsReverseCard)
            {
                Game.ActivateReverseCard(this);
            }
        };
    }

    private void SetupPicBox()
    {
        PicBox = new()
        {
            Width = 90,
            Height = 126,
            BackgroundImageLayout = ImageLayout.Stretch,
            BorderStyle = BorderStyle.FixedSingle,
            BackgroundImage = PicImg
        };

        PicBox.Click += (sender, e) =>
        {
            // If this is a reverse card, ignore the normal click logic completely
            if (IsReverseCard)
                return;

            if (!FaceUp && Game.CanFlipOver(this))
                FlipOver();
        };

        // Begin dragging when mouse is pressed
        PicBox.MouseDown += (sender, e) => {
            if (e.Button == MouseButtons.Left && Game.IsCardMovable(this))
            {

                List<Card> cardsToMove = new();
                TableauStack tableau = Game.TableauStacks.FirstOrDefault(ts => ts.Cards.Contains(this));

                if (tableau != null) // if the card is in the tableau
                {
                    // get the list of cards starting from the one that is clicked
                    cardsToMove = tableau.GetStackFrom(this);
                    FrmGame.DragCards(cardsToMove);
                }
                else
                {
                    cardsToMove.Add(this);
                    FrmGame.DragCards(cardsToMove);
                }

                // store dragging info from the clicked card
                dragOffset = e.Location;
                conBeforeDrag = PicBox.Parent;
                //relLocBeforeDrag = PicBox.Location;

                // move all cards to the form and maintain their relative locations
                foreach (Card card in cardsToMove)
                {
                    card.relLocBeforeDrag = card.PicBox.Location;
                    conBeforeDrag?.RemCard(card);
                    FrmGame.Instance.AddCard(card);

                    // convert control coords to form coords
                    Point screenPos = conBeforeDrag.PointToScreen(card.relLocBeforeDrag);
                    card.PicBox.Location = card.PicBox.Parent.PointToClient(screenPos);

                    card.PicBox.BringToFront();
                }
            }
        };

        // handle dropping logic when mouse is released
        PicBox.MouseUp += (sender, e) => {
            if (FrmGame.IsDraggingCard(this))
            {
                List<Card> draggedCards = new(FrmGame.CurDragCards);
                FrmGame.StopDragCards();
                Game.CallDragEndedOnAll();

                // handle valid drop
                if (lastDropTarget is not null && lastDropTarget.CanDrop(draggedCards[0]))
                {
                    IDragFrom source = FrmGame.CardDraggedFrom;
                    Card newBottomCard = null;
                    foreach (Card card in draggedCards)
                    {
                        FrmGame.CardDraggedFrom.RemCard(card);
                        lastDropTarget.Dropped(card);
                        card.PicBox.BringToFront();
                        if (source is TableauStack sourceTableauStack && sourceTableauStack.Cards.Count > 0)
                        {
                            newBottomCard = sourceTableauStack.Cards.Last.Value;
                            if (!newBottomCard.FaceUp)
                            {
                                newBottomCard.FlipOver();
                            }
                        }
                    }
                    Game.RecordMove(draggedCards, source, lastDropTarget, newBottomCard);
                }
                else
                {
                    // snap back to original positions
                    for (int i = 0; i < draggedCards.Count; i++)
                    {
                        Card card = draggedCards[i];
                        FrmGame.Instance.RemCard(card);
                        conBeforeDrag?.AddCard(card);

                        // restore orginial positions with vertical offsets of 20 pixels
                        card.PicBox.Location = new Point(relLocBeforeDrag.X, relLocBeforeDrag.Y + (i * 20));
                        card.PicBox.BringToFront();
                    }
                }
            }
        };

        // Move the card as mouse moves while dragging
        PicBox.MouseMove += (sender, e) => {
            if (FrmGame.IsDraggingCard(this))
            {
                List<Card> draggedCards = FrmGame.CurDragCards;

                // the first card drives the movement
                if (draggedCards.IndexOf(this) == 0)
                {
                    var dragged = (Control)sender;
                    Point screenPos = dragged.PointToScreen(e.Location);
                    Point parentPos = dragged.Parent.PointToClient(screenPos);
                    dragged.Left = screenPos.X - dragOffset.X;
                    dragged.Top = screenPos.Y - dragOffset.Y;

                    // Find the control currently under the mouse
                    Control target = FrmGame.Instance.GetChildAtPoint(dragged.Parent.PointToClient(screenPos));

                    // Avoid detecting the dragged control itself
                    if (target is not null && target != dragged)
                    {
                        var dropTarget = Game.FindDropTarget(target);
                        if (dropTarget is null)
                        {
                            Game.CallDragEndedOnAll();
                        }
                        else if (dropTarget != lastDropTarget)
                        {
                            lastDropTarget?.DragEnded();
                        }
                        if (dropTarget != FrmGame.CardDraggedFrom as IDropTarget)
                        {
                            dropTarget?.DragOver(this);
                            lastDropTarget = dropTarget;
                        }
                    }

                    Point newLoc = new Point(
                        parentPos.X - dragOffset.X,
                        parentPos.Y - dragOffset.Y
                    );

                    for (int i = 0; i < draggedCards.Count; i++)
                    {
                        Card card = draggedCards[i];
                        card.PicBox.Location = new Point(newLoc.X, newLoc.Y + (i * 20));
                        card.PicBox.BringToFront();
                    }
                }
            }
        };
    }

    public void FlipOver()
    {
        FaceUp = !FaceUp;
        PicBox.BackgroundImage = PicImg;
    }

    public void AdjustLocation(int left, int top)
    {
        PicBox.Left = left;
        PicBox.Top = top;
    }
}

// A Tableau stack is one of the seven main playing stacks
public class TableauStack : IFindMoveableCards, IDropTarget, IDragFrom
{
    public Panel Panel { get; set; } // UI Panel
    public LinkedList<Card> Cards { get; private set; } // Cards in the stack

    public TableauStack(Panel panel)
    {
        Panel = panel;
        Cards = new();
    }

    public void AddCard(Card c)
    {
        Cards.AddLast(c);
        Panel.AddCard(c);
        c.PicBox.BringToFront();
    }

    // Returns a list of cards starting from card c to the end of the list
    public List<Card> GetStackFrom(Card c)
    {
        List<Card> list = new();
        bool found = false;

        foreach (Card card in Cards)
        {
            if (card == c)
                found = true;
            if (found)
                list.Add(card);
        }
        return list;
    }

    // finds the first face-up card and returns the list of all cards from there to the end of the list
    public List<Card> FindMoveableCards()
    {
        List<Card> movableCards = new();

        bool foundFaceUp = false;
        foreach (Card card in Cards)
        {
            if (card.FaceUp)
            {
                foundFaceUp = true;
            }
            if (foundFaceUp)
            {
                movableCards.Add(card);
            }
        }
        return movableCards;
    }

    // Highlights panel when dragging over
    public void DragOver(Card c)
    {
        Panel.BackColor = CanDrop(c) ? Color.Green : Color.Red;
    }

    // Controls the rules for dropping onto tableau
    public bool CanDrop(Card c)
    {
        if (Cards.Count == 0)
        {
            // King when normal, Ace when reversed (Wild always OK)
            return c.Type == (Game.IsReversed ? CardType.ACE : CardType.KING) || c.Type == CardType.WILD;
        }
        // King or Wild can start an empty pile
        if (Cards.Count == 0)
        {
            return c.Type == CardType.KING || c.Type == CardType.WILD;
        }
        else
        {
            Card lastCard = Cards.Last.Value;

            // if either the card being dragged or the card being dragged over are WILD, return true
            if (lastCard.Type == CardType.WILD || c.Type == CardType.WILD)
                return true;

            bool suitCheck = ((int)lastCard.Suit % 2 != (int)c.Suit % 2);
            bool typeCheck = Game.IsReversed ? lastCard.Type == c.Type - 1 : lastCard.Type == c.Type + 1;
            return (suitCheck && typeCheck);
        }
    }

    // Handles card drop
    public void Dropped(Card c)
    {
        Cards.AddLast(c);
        FrmGame.Instance.RemCard(c);
        Panel.AddCard(c);
        c.AdjustLocation(0, (Cards.Count - 1) * 20);
        c.PicBox.BringToFront();
        Panel.Refresh();
    }

    // Resets Panel color when no longer being dragged over
    public void DragEnded()
    {
        Panel.BackColor = Color.Transparent;
    }

    public Card GetBottomCard()
    {
        return Cards.Count > 0 ? Cards.Last.Value : null;
    }

    public void RemCard(Card card)
    {
        Cards.Remove(card);
    }

    // Re-build the linked list in reverse order and redraw the stack
    public void ReverseOrder()
    {
        // Split into hidden (face-down) and visible (face-up) cards
        var hidden = Cards.Where(c => !c.FaceUp).ToList();
        var visible = Cards.Where(c => c.FaceUp).ToList();

        // Reverse only the visible section
        visible.Reverse();

        // Recombine the cards hidden first, then reversed visible
        Cards = new LinkedList<Card>(hidden.Concat(visible));

        Reflow();
    }

    // Apply the proper vertical offsets to match current order
    public void Reflow()
    {
        int i = 0;
        foreach (var card in Cards)
        {
            card.AdjustLocation(0, i * 20);
            card.PicBox.BringToFront();
            i++;
        }
        Panel.Refresh();
    }

}

// The Talon is stack of face up cards from the draw pile
public class Talon : IFindMoveableCards, IDragFrom
{
    public Panel Panel { get; private set; }
    public Stack<Card> Cards { get; private set; }

    public Talon(Panel pan)
    {
        Panel = pan;
        Cards = new();
    }

    // puts all Talon cards back into the deck Queue
    public void ReleaseIntoDeck(Deck deck)
    {
        foreach (var card in Cards)
        {
            deck.Release(card);
            Panel.RemCard(card);
        }
        Cards.Clear();
    }

    // adds a card to the Talon
    public void AddCard(Card c)
    {
        Cards.Push(c);
        Panel.AddCard(c);
    }

    // returns the list of cards that are movable (the top card)
    public List<Card> FindMoveableCards()
        => (Cards.Count > 0 ? new List<Card> { Cards.Peek() } : new List<Card>());

    // removes the top card if it matches the given card
    public void RemCard(Card card)
    {
        if (Cards.Count > 0 && Cards.Peek() == card)
        {
            Cards.Pop();
        }
    }
}
// a foundation stack is the piles at the top that count up for each suit
public class FoundationStack : IFindMoveableCards, IDropTarget, IDragFrom
{
    public Panel Panel { get; private set; }
    public Stack<Card> Cards { get; private set; }
    public Suit Suit { get; private init; }

    public FoundationStack(Panel panel, Suit suit)
    {
        Panel = panel;
        Cards = new();
        Suit = suit;
    }

    // returns the list of cards that can be moved (the top card)
    public List<Card> FindMoveableCards() => (Cards.Count > 0 ? new List<Card> { Cards.Peek() } : new List<Card>());

    // shows the visual indicator while dragging over
    public void DragOver(Card c)
    {
        Panel.BackColor = CanDrop(c) ? Color.Green : Color.Red;
    }

    // for a card to be able to be dropped in a foundation stack,
    // it must be the same suit as the stack and be the correct next card
    // in ascending order
    public bool CanDrop(Card c)
    {
        Card topCard = Cards.Count > 0 ? Cards.Peek() : null;

        // don't allow wildcards in the foundation stacks
        if (c.Type == CardType.WILD)
            return false;

        // if either the card being dragged or the card being dragged over are WILD, return true
        //if (topCard is not null)
        //    return true;

        bool suitCheck = Suit == c.Suit;

        bool typeCheck;
        if (Game.IsReversed)
        {
            // Reversed mode: (King -> Ace)
            typeCheck = topCard is null
                ? c.Type == CardType.KING
                : topCard.Type == c.Type + 1;
        }
        else
        {
            // Normal mode: (Ace ->  King)
            typeCheck = topCard is null
                ? c.Type == CardType.ACE
                : topCard.Type == c.Type - 1;
        }

        return suitCheck && typeCheck;
    }


    // handles logic when a card is added to the stack
    public void Dropped(Card c)
    {
        Cards.Push(c);
        FrmGame.Instance.RemCard(c);
        Panel.AddCard(c);
        c.AdjustLocation(0, 0);
        c.PicBox.BringToFront();

        // optional win check
        if (Game.HasWon())
        {
            FrmGame.Instance.ShowWinScreen();
        }
    }

    public void DragEnded()
    {
        Panel.BackColor = Color.Transparent;
    }

    // changes the Stack to a List, removes the card, and then goes back to a Stack
    public void RemCard(Card card)
    {
        List<Card> list = Cards.ToList();
        list.Remove(card);
        Cards = new Stack<Card>(list);
    }

    public void AddCard(Card card)
    {
        Dropped(card);
    }
}

// Game manager
public static class Game
{
    public static Form TitleForm { get; set; }
    public static Deck Deck { get; private set; }
    public static Dictionary<Suit, FoundationStack> FoundationStacks { get; set; }
    public static TableauStack[] TableauStacks;
    public static Talon Talon { get; set; }
    public static int StockReloadCount { get; set; }
    private static Stack<ICommand> moveStack { get; set; }

    public static bool IsReversed { get; private set; } = false;

    static Game()
    {
        StockReloadCount = 0;
    }

    public static void Init(Panel panTalon, Panel[] panTableauStacks, Dictionary<Suit, Panel> panFoundationStacks)
    {
        Deck = new();
        moveStack = new();

        // create talon
        Talon = new(panTalon);

        // create tableau stacks
        TableauStacks = new TableauStack[7];
        for (int i = 0; i < TableauStacks.Length; i++)
        {
            TableauStacks[i] = new(panTableauStacks[i]);
        }

        // create foundation stacks
        FoundationStacks = new();
        foreach (var suit in Enum.GetValues<Suit>())
        {
            if (suit == Suit.BLACK_JOKER || suit == Suit.RED_JOKER)
                continue; // skip wild
            FoundationStacks.Add(suit, new(panFoundationStacks[suit], suit));
        }

        // load tableau stacks
        const int VERT_OFFSET = 20;
        for (int i = 0; i < TableauStacks.Length; i++)
        {
            Card c;
            for (int j = 0; j < i; j++)
            {
                c = Deck.Acquire();
                c.FlipOver();
                c.AdjustLocation(0, j * VERT_OFFSET);
                TableauStacks[i].AddCard(c);
            }
            c = Deck.Acquire();
            c.AdjustLocation(0, i * VERT_OFFSET);
            TableauStacks[i].AddCard(c);
        }
    }

    // Toggle reverse mode and flip all tableau cards
    public static void ToggleReverseMode()
    {
        IsReversed = !IsReversed;

        foreach (var tableau in TableauStacks)
        {
            tableau.ReverseOrder(); // reverse existing piles
        }

        FrmGame.Instance?.UpdateReverseStatus(); // Update bottom of UI
    }
    public static void ActivateReverseCard(Card c)
    {
        // Flip the game rules
        ToggleReverseMode();

        // Remove the card from wherever it is (Talon / Tableau / Foundation)
        FindDragFrom(c)?.RemCard(c);

        // Remove the visual from the UI if it still exists (for some reason)
        c.PicBox?.Parent?.RemCard(c);
        c.PicBox.Visible = false;
        c.PicBox.Enabled = false;
    }


    // given a card c, returns true if it can be moved somewhere and false if it can't
    public static bool IsCardMovable(Card c)
    {
        bool isMovable = false;
        isMovable |= Talon.FindMoveableCards().Contains(c);
        foreach (var foundationStack in FoundationStacks)
        {
            isMovable |= foundationStack.Value.FindMoveableCards().Contains(c);
        }
        foreach (var tableauStack in TableauStacks)
        {
            isMovable |= tableauStack.FindMoveableCards().Contains(c);
        }
        return isMovable;
    }

    // finds which pile (tableau, talon, foundation) a card was dragged from. It returns something that
    // implements the IDragFrom interface
    public static IDragFrom FindDragFrom(Card c)
    {
        if (Talon.Cards.Contains(c))
        {
            return Talon;
        }
        foreach (var foundationStack in FoundationStacks)
        {
            if (foundationStack.Value.Cards.Contains(c))
            {
                return foundationStack.Value;
            }
        }
        foreach (var tableauStack in TableauStacks)
        {
            if (tableauStack.Cards.Contains(c))
            {
                return tableauStack;
            }
        }
        return null;
    }

    // finds which control can receive a card drop
    public static IDropTarget FindDropTarget(Control c)
    {
        foreach (var foundationStack in FoundationStacks)
        {
            if (foundationStack.Value.Panel == c)
            {
                return foundationStack.Value;
            }
        }
        foreach (var tableauStack in TableauStacks)
        {
            if (tableauStack.Panel == c)
            {
                return tableauStack;
            }
        }
        return null;
    }

    // resets all highlights after drag ends
    public static void CallDragEndedOnAll()
    {
        foreach (var foundationStack in FoundationStacks)
        {
            foundationStack.Value.DragEnded();
        }
        foreach (var tableauStack in TableauStacks)
        {
            tableauStack.DragEnded();
        }
    }

    public static bool CanFlipOver(Card c)
    {
        foreach (var tableauStack in TableauStacks)
        {
            if (tableauStack.GetBottomCard() == c)
            {
                return true;
            }
        }
        return false;
    }

    public static void RecordMove(List<Card> movedCards, IDragFrom src, IDropTarget dest, Card flipped)
    {
        MoveCommand cmd = new(movedCards, src, dest, flipped);
        {
            moveStack.Push(cmd);
        }
    }

    public static bool CanUndo => moveStack.Count > 0;

    public static void UndoLastMove()
    {
        if (moveStack.Count > 0)
        {
            ICommand cmd = moveStack.Pop();
            cmd.Undo();
        }
    }

    public static bool HasWon()
    {
        // win when all foundations have 13 cards 
        return FoundationStacks.Values.All(f => f.Cards.Count == 13);
    }

    // explosion animation
    public static void Explode()
    {
        FrmGame.stopTime();
        List<Card> allCardsInPlay = new();
        foreach (var foundationStack in FoundationStacks)
        {
            allCardsInPlay.AddRange(foundationStack.Value.Cards);
        }
        foreach (var tableauStack in TableauStacks)
        {
            allCardsInPlay.AddRange(tableauStack.Cards);
        }
        allCardsInPlay.AddRange(Talon.Cards);
        foreach (Card c in allCardsInPlay)
        {
            Point origPos = c.PicBox.Location;
            origPos.X += c.PicBox.Parent.Location.X;
            origPos.Y += c.PicBox.Parent.Location.Y;
            c.PicBox.Parent.RemCard(c);
            FrmGame.Instance.AddCard(c);
            c.AdjustLocation(origPos.X, origPos.Y);
            c.PicBox.BringToFront();
        }
        const int SPEED = 6;
        const int MORE_SPEED = 10;
        Point[] possibleExplodeVectors = [
            new(0, SPEED),
            new(0, -SPEED),

            new(SPEED, 0),
            new(-SPEED, 0),

            new(SPEED, SPEED),
            new(-SPEED, SPEED),

            new(SPEED, -SPEED),
            new(-SPEED, -SPEED),

            new(SPEED, MORE_SPEED),
            new(-SPEED, MORE_SPEED),
            new(SPEED, -MORE_SPEED),
            new(-SPEED, -MORE_SPEED),

            new(MORE_SPEED, SPEED),
            new(-MORE_SPEED, SPEED),
            new(MORE_SPEED, -SPEED),
            new(-MORE_SPEED, -SPEED),
        ];
        Point[] explodeVectors = new Point[allCardsInPlay.Count];
        Random rand = new();
        for (int i = 0; i < explodeVectors.Length; i++)
        {
            explodeVectors[i] = possibleExplodeVectors[rand.Next(possibleExplodeVectors.Length)];
        }
        Timer tmr = new() { Interval = 25 };
        tmr.Tick += (sender, e) =>
        {
            for (int i = 0; i < allCardsInPlay.Count; i++)
            {
                Card c = allCardsInPlay[i];
                c.AdjustLocation(c.PicBox.Location.X + explodeVectors[i].X, c.PicBox.Location.Y + explodeVectors[i].Y);
            }
        };
        tmr.Start();
    }

    //public static void checkIfFlip()
    //{
    //    foreach (var tableauStack in TableauStacks)
    //    {
    //        Card c = tableauStack.GetBottomCard();
    //        FlipOverLastCard(c);
    //    }
    //}

    //public static void FlipOverLastCard(Card c)
    //{
    //    //FaceUp = !FaceUp;
    //    c.PicBox.BackgroundImage = c.PicImg;
    //}
}

// interface for a command that can execute and undo a move
public interface ICommand
{
    void Execute();
    void Undo();
}

// Stores a single move that was made. Every move knows how to execute or undo itself
public class MoveCommand : ICommand
{
    private readonly List<Card> cardsMoved;
    private readonly IDragFrom source;
    private readonly IDropTarget dest;

    private readonly Control previousParent;
    private readonly List<Point> previousLocations;
    private readonly Card flippedCard;

    public MoveCommand(List<Card> cardsMoved, IDragFrom source, IDropTarget dest, Card flippedCard)
    {
        this.cardsMoved = cardsMoved;
        this.source = source;
        this.dest = dest;
        this.flippedCard = flippedCard;
    }

    public void Execute()
    {
        foreach (Card card in cardsMoved)
        {
            source.RemCard(card);
            dest.Dropped(card);
        }
    }

    public void Undo()
    {
        flippedCard?.FlipOver();
        foreach (Card card in cardsMoved)
        {
            if (dest is IDragFrom dragFromDest)
                dragFromDest.RemCard(card);
            source.AddCard(card);
            card.PicBox.Location = card.relLocBeforeDrag;
            card.PicBox.BringToFront();
        }
    }
}