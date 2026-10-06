using UnityEngine;

// Movement that temporarily replaces the player's own locomotion (Mundo Superior climbing and
// flight). PlayerController keeps sole authority over the CharacterController: while a motor is
// set it calls Tick instead of its walk/jump code (guide 4.2).
public interface IPlayerMotor
{
    void Tick(CharacterController controller, float deltaTime);
}

// What the body shows while a motor moves it; PlayerAnimator picks the clip from it.
public enum PlayerMotorPose { Climb, ClimbTop, Fly }

public interface IPlayerMotorPose
{
    PlayerMotorPose Pose { get; }
}
