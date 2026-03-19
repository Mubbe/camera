using Godot;

public partial class Player : CharacterBody3D
{
    [Export] public float Speed = 4.0f;
    [Export] public float JumpVelocity = 4.5f;

    [Export] public Node3D CameraPivot; // assign Left_Right_Pivot

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        // Gravity
        if (!IsOnFloor())
            velocity += GetGravity() * (float)delta;

        // Jump
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
            velocity.Y = JumpVelocity;

        // Input (controller + keyboard)
        Vector2 input = Input.GetVector("left", "right", "up", "down");

        // Camera directions
        Vector3 forward = -CameraPivot.GlobalTransform.Basis.Z;
        Vector3 right = CameraPivot.GlobalTransform.Basis.X;

        forward.Y = 0;
        right.Y = 0;

        Vector3 direction = (right * input.X + forward * input.Y).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;

            // Face movement direction
            LookAt(GlobalPosition + direction, Vector3.Up);
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0, Speed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}