using Godot;
using System;

public partial class Player : CharacterBody2D
{
  [Export] private Node2D GunRotationNode;
  [Export] public PackedScene BulletScene;
  [Export] public Node2D Node2DBulletSpawnNode;
  [Export] public MultiplayerSynchronizer MPSyncNode;
  [Export] public Label PlayerNameLabel;

  private Vector2 syncPosition = new Vector2(0, 0);
  private float SyncSpeed = 0.1f;
  private float syncGunRotation = 0f;
  public const float Speed = 300.0f;
  public const float JumpVelocity = -400.0f;

  public override void _Ready() { MPSyncNode.SetMultiplayerAuthority(int.Parse(Name)); }

  public override void _PhysicsProcess(double delta)
  {
    if (MPSyncNode.GetMultiplayerAuthority() == Multiplayer.GetUniqueId()) { ProcessPhysics(delta); } // Handle the local (controlled) player
    else { SyncVars(); } // Move other players on local screen
  }
  private void ProcessPhysics(double delta)
  {
    Vector2 velocity = Velocity;

    // Add the gravity.
    if (!IsOnFloor())
    {
      velocity += GetGravity() * (float)delta;
    }

    GunRotationNode.LookAt(GetViewport().GetMousePosition());

    if (Input.IsActionJustPressed("fire")){Rpc("fire");}

    // Handle Jump.
    if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
    {
      velocity.Y = JumpVelocity;
    }

    // Get the input direction and handle the movement/deceleration.
    // As good practice, you should replace UI actions with custom gameplay actions.
    Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
    if (direction != Vector2.Zero)
    {
      velocity.X = direction.X * Speed;
    }
    else
    {
      velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
    }

    Velocity = velocity;
    MoveAndSlide();
    syncPosition = GlobalPosition;
    syncGunRotation = GunRotationNode.RotationDegrees;
  }

  private void SyncVars()
  {
    GlobalPosition = GlobalPosition.Lerp(syncPosition, SyncSpeed);
    GunRotationNode.RotationDegrees = Mathf.Lerp(GunRotationNode.RotationDegrees, syncGunRotation, SyncSpeed);
  }

  [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
  private void fire()
  {
      Node2D b = BulletScene.Instantiate<Node2D>();
      b.RotationDegrees = GunRotationNode.RotationDegrees;
      b.GlobalPosition = GunRotationNode.GlobalPosition;
      GetTree().Root.AddChild(b);
  }

  public void SetupPlayer(string name)
  {
    PlayerNameLabel.Text = name;
  }
}
