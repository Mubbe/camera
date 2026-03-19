using Godot;
using System;

public partial class Camera_Pivoy : Node3D
{
    [Export] Player Player;


    [Export] float FollowSpeed = 5f;
    [Export] public float OrbitSpeed = 2f;


    [Export] RayCast3D Left_Ray;
    [Export] RayCast3D Right_Ray;
    [Export] RayCast3D Middle_Ray;
    [Export] RayCast3D Up_Ray;
    [Export] RayCast3D Down_Ray;

    [Export] Area3D HintArea;

    public float MinPitch = -40f;
    public float MaxPitch = 50f;

    private Node3D leftRightPivot;
    private Node3D upDownPivot;
    private Camera3D camera;

    const string CAM_RIGHT = "cameraright";
    const string CAM_LEFT = "cameraleft";
    const string CAM_UP = "cameraup";
    const string CAM_DOWN = "cameradown";

    public override void _Ready()
	{
        leftRightPivot = GetNode<Node3D>("Left_Right_Pivot");
        upDownPivot = GetNode<Node3D>("Left_Right_Pivot/Up_Down_Pivot");
        camera = GetNode<Camera3D>("Left_Right_Pivot/Up_Down_Pivot/Camera3D");

        HintArea.BodyEntered += BodyEntered;
        HintArea.BodyExited += BodyExited;


    }

	
	public override void _PhysicsProcess(double delta)
	{

        if (inHintArea)
        {
            HintCamera((float)delta);
        }
        else
        {
            FollowPlayer((float)delta);
            HandleOrbit((float)delta);
            HandleObstacleAvoidance((float)delta);
        }


    }
    void FollowPlayer(float delta)
    {
        GlobalPosition = GlobalPosition.MoveToward(Player.GlobalPosition, FollowSpeed * delta);
    }


    void HandleOrbit(float delta)
    {
        Vector3 moveDir = Player.Velocity;
        float inputX = Input.GetActionStrength(CAM_RIGHT) - Input.GetActionStrength(CAM_LEFT);
        if (Mathf.Abs(inputX) < 0.01f) // no horizontal input
        {
            AlignCameraToMovement(delta, moveDir);
        }


        float inputY = Input.GetActionStrength(CAM_UP) - Input.GetActionStrength(CAM_DOWN);
       

        // Horizontal orbit
        leftRightPivot.RotateY(-inputX * OrbitSpeed * delta);

        // Vertical orbit
        upDownPivot.RotateX(inputY * OrbitSpeed * delta);

        ClampVertical();
    }

    void ClampVertical()
    {
        Vector3 rot = upDownPivot.RotationDegrees;

        rot.X = Mathf.Clamp(rot.X, MinPitch, MaxPitch);

        upDownPivot.RotationDegrees = rot;
    }
    void UpdateRaycasts()
    {
        Vector3 cameraTarget = camera.GlobalPosition;

       
        Vector3 dir = (cameraTarget - Player.GlobalPosition).Normalized();
        float dist = (cameraTarget - Player.GlobalPosition).Length();

        Middle_Ray.GlobalPosition = Player.GlobalPosition;
        Middle_Ray.TargetPosition = dir * dist;

        Left_Ray.GlobalPosition = Player.GlobalPosition + Vector3.Left * 0.5f;
        Left_Ray.TargetPosition = dir * dist;

        Right_Ray.GlobalPosition = Player.GlobalPosition + Vector3.Right * 0.5f;
        Right_Ray.TargetPosition = dir * dist;

        Up_Ray.GlobalPosition = Player.GlobalPosition + Vector3.Up * 0.5f;
        Up_Ray.TargetPosition = dir * dist;

        Down_Ray.GlobalPosition = Player.GlobalPosition + Vector3.Down * 0.5f;
        Down_Ray.TargetPosition = dir * dist;
    }
    void HandleObstacleAvoidance(float delta)
    {
        UpdateRaycasts(); // Update rays first

        Vector3 cameraTarget = camera.GlobalPosition;

        if (Middle_Ray.IsColliding())
        {
            float hitDist = Middle_Ray.GetCollisionPoint().DistanceTo(Player.GlobalPosition);
            float safeDist = hitDist - 0.5f;

            Vector3 dir = (cameraTarget - Player.GlobalPosition).Normalized();
            cameraTarget = Player.GlobalPosition + dir * safeDist;
        }


        // IF the rays hit anything small cahnges to it
        Vector3 offset = Vector3.Zero;

        if (Left_Ray.IsColliding()) offset += Vector3.Right;
        if (Right_Ray.IsColliding()) offset += Vector3.Left;
        if (Up_Ray.IsColliding()) offset += Vector3.Down;
        if (Down_Ray.IsColliding()) offset += Vector3.Up;

        cameraTarget += offset * 0.5f * delta;


        //Move the camere to the new target
        camera.GlobalPosition = camera.GlobalPosition.MoveToward(cameraTarget, delta * 10f);
    }
    void AlignCameraToMovement(float delta, Vector3 moveDir)
    {
        if (moveDir.Length() < 0.1f)
            return; //checks if thee is no movement

        Vector3 flatDir = new Vector3(moveDir.X, 0, moveDir.Z).Normalized(); // we only get z,x axes

        Transform3D currentTransform = leftRightPivot.GlobalTransform;

        Transform3D targetTransform = currentTransform.LookingAt(Player.GlobalPosition + flatDir, Vector3.Up);

        Basis newBasis = currentTransform.Basis.Slerp(targetTransform.Basis, delta * 2f);

        leftRightPivot.GlobalTransform = new Transform3D(newBasis, leftRightPivot.GlobalTransform.Origin);
    }
    void HintCamera(float delta)
    {
        Vector3 targetPos = Player.GlobalPosition + new Vector3(0, 5, -5);

        camera.GlobalPosition = camera.GlobalPosition.MoveToward(targetPos, delta * 3f);
        camera.LookAt(Player.GlobalPosition, Vector3.Up);
    }

    bool inHintArea = false;

    void BodyEntered(Node3D body)
    {
        if (body == Player)
        {
            inHintArea = true;
        }
    }

    void BodyExited(Node3D body)
    {
        if (body == Player)
        {
            inHintArea = false;
        }
    }



}
