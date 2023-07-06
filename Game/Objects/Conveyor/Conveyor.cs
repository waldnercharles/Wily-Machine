using System.Linq;
using Godot;

namespace WilyMachine;

[SceneTree] [Tool]
public partial class Conveyor : StaticBody2D
{
    // StartRight, StartLeft, MiddleRight, MiddleLeft, EndRight, EndLeft, SingleRight, SingleLeft 
    private const int CONVEYOR_PART_LENGTH = 8;

    private enum ConveyorPart
    {
        Start = 0,
        Middle = 2,
        End = 4,
        Single = 6
    }


    public CollisionShape2D CollisionShape2D => _.CollisionShape2D;

    public ConstantForce Force => _.Force;
    public CollisionShape2D ForceCollisionShape2D => _.Force.CollisionShape2D;

    public VisibleOnScreenEnabler2D VisibleOnScreenEnabler2D => _.VisibleOnScreenEnabler2D;

    public Sprite2D Sprite2D => _.Sprite2D;

    [Export] public Texture2D Texture
    {
        get => Sprite2D.Texture;
        set
        {
            void UpdateTexture()
            {
                Sprite2D.Texture = value;

                Sprite2D.Frame = 0;
                Sprite2D.Hframes = value.GetWidth() / Global.TileSize / CONVEYOR_PART_LENGTH;

                Sprite2D.Vframes = 1;
                Sprite2D.RegionEnabled = true;
                Sprite2D.RegionRect = new Rect2(0, 0, Sprite2D.Hframes * Global.TileSize, Global.TileSize);
                UpdateSprites();
            }

            if (!IsInsideTree())
            {
                ToSignal(this, "ready").OnCompleted(UpdateTexture);
            }
            else
            {
                UpdateTexture();
            }
        }
    }

    private Vector2 m_ConveyorSpeed;
    private Vector2 m_Direction = Vector2.Right;

    [Export] public bool Enabled
    {
        get => Force.Enabled;
        set
        {
            if (!IsInsideTree())
            {
                ToSignal(this, "ready").OnCompleted(() => Force.Enabled = value);
            }
            else
            {
                Force.Enabled = value;
            }
        }
    }

    [Export] public Vector2 ConveyorSpeed
    {
        get => m_ConveyorSpeed;
        set
        {
            m_ConveyorSpeed = value;

            if (!IsInsideTree())
            {
                ToSignal(this, "ready").OnCompleted(() => Force.Velocity = ConveyorSpeed * m_Direction);
            }
            else
            {
                Force.Velocity = ConveyorSpeed * m_Direction;
            }
        }
    }

    [Export] public bool Flip
    {
        get => m_Direction == Vector2.Left;
        set
        {
            m_Direction = value ? Vector2.Left : Vector2.Right;

            if (!IsInsideTree())
            {
                ToSignal(this, "ready").OnCompleted(() => Force.Velocity = ConveyorSpeed * m_Direction);
            }
            else
            {
                Force.Velocity = ConveyorSpeed * m_Direction;
            }

            UpdateSprites();
        }
    }

    private int m_Length = 1;

    [Export(PropertyHint.Range, "1,128,")] public int Length
    {
        get => m_Length;
        set
        {
            m_Length = value;
            CallDeferred(nameof(UpdateLength));
        }
    }

    [Export(PropertyHint.Range, "1,60,")] public int FramesPerSecond = 5;

    private Sprite2D[]? m_Sprites;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var shouldUpdateFrame = (Engine.GetPhysicsFrames() + 1) % (ulong)(1.0 / delta / FramesPerSecond) == 0;

        if (shouldUpdateFrame)
        {
            Sprite2D.Frame = (Sprite2D.Frame + 1) % Sprite2D.Hframes;

            foreach (var sprite in GetChildren().OfType<Sprite2D>())
            {
                sprite.Frame = Sprite2D.Frame;
            }
        }
    }

    private void UpdateLength()
    {
        var sprites = GetChildren().OfType<Sprite2D>().OrderBy(c => c.GlobalPosition.X).ToArray();

        if (sprites.Length > Length)
        {
            for (var i = 1; i <= sprites.Length - Length; i++)
            {
                sprites[^i].QueueFree();
            }
        }
        else if (Length > sprites.Length)
        {
            var previousSprite = sprites[^1];

            for (var i = 1; i <= Length - sprites.Length; i++)
            {
                var newSprite = (previousSprite.Duplicate() as Sprite2D)!;
                AddChild(newSprite);

                newSprite.Position = previousSprite.Position with { X = previousSprite.Position.X + Global.TileSize };
                previousSprite = newSprite;
            }
        }

        var collisionRectangle = CollisionShape2D.Shape as RectangleShape2D;
        collisionRectangle!.Size = new Vector2(Global.TileSize * Length, Global.TileSize);
        CollisionShape2D.Position = new Vector2((Length - 1) * Global.TileSize * 0.5f, 0f);

        var forceRectangle = ForceCollisionShape2D.Shape as RectangleShape2D;
        forceRectangle!.Size = collisionRectangle.Size + new Vector2(-2, 1);
        ForceCollisionShape2D.Position = CollisionShape2D.Position;

        VisibleOnScreenEnabler2D.Rect =
            new Rect2(CollisionShape2D.Position - collisionRectangle.Size * 0.5f, collisionRectangle.Size);
    }

    public override void _Notification(int what)
    {
        base._Notification(what);

        if (what == NotificationChildOrderChanged)
        {
            CallDeferred(nameof(UpdateSprites));
        }
    }

    private void UpdateSprites()
    {
        bool IsSpriteConnected(Node2D a, Node2D b)
        {
            return Mathf.IsEqualApprox(a.GlobalPosition.Y, b.GlobalPosition.Y) &&
                   Mathf.IsEqualApprox(Mathf.Abs(a.GlobalPosition.X - b.GlobalPosition.X), Global.TileSize);
        }

        void UpdateSpriteRegion(Sprite2D sprite, ConveyorPart conveyorPart)
        {
            var offset = (int)conveyorPart;

            var directionOffset = m_Direction == Vector2.Left ? 1 : 0;

            var size = new Vector2(sprite.Hframes * Global.TileSize, Global.TileSize);
            var position = new Vector2((offset + directionOffset) * size.X, 0f);

            sprite.RegionRect = new Rect2(position, size);
        }

        Sprite2D? previousSprite = null;
        var currentLength = 1;

        foreach (var currentSprite in GetChildren().OfType<Sprite2D>().OrderBy(c => c.GlobalPosition.X))
        {
            if (previousSprite == null || !IsSpriteConnected(previousSprite, currentSprite))
            {
                currentLength = 1;
                UpdateSpriteRegion(currentSprite, ConveyorPart.Single);
            }
            else
            {
                UpdateSpriteRegion(previousSprite, currentLength == 1 ? ConveyorPart.Start : ConveyorPart.Middle);
                UpdateSpriteRegion(currentSprite, ConveyorPart.End);

                currentLength++;
            }

            previousSprite = currentSprite;
        }
    }
}
