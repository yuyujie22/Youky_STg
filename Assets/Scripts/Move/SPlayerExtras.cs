using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YoukyController
{
    //结构体FrameInput 负责玩家键盘的xy轴的输入
    public struct FrameInput
    {
        public float X, Y;
        public bool JumpDown;
        public bool JumpUp;
        public Vector2 mousePos;
    }
    //接口 负责监控玩家的键盘输入命令
    public interface SIPlayerController
    {
        public Vector3 Velocity { get; }
        public FrameInput Input { get; }
        public bool JumpingThisFrame { get; }
        public bool AttackingThisFrame { get; }
        public bool StopAttackingThisFrame { get; }
        public bool LandingThisFrame { get; }
        public Vector3 RawMovement { get; }
        public bool Grounded { get; }
    }

    public interface IExtendedPlayerController : SIPlayerController
    {
        public bool DoubleJumpingThisFrame { get; set; }
        public bool Dashing { get; set; }
    }
    //结构体RayRange 负责射线检测
    public struct RayRange
    {
        public RayRange(float x1, float y1, float x2, float y2, Vector2 dir)
        {
            Start = new Vector2(x1, y1);
            End = new Vector2(x2, y2);
            Dir = dir;
        }

        public readonly Vector2 Start, End, Dir;
    }
}