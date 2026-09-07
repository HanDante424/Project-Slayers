using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }

    public bool JumpPressed { get; private set; }
    public bool TestSkillPressed { get; private set; }


    public void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }


    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            JumpPressed = true;
        }
    }


    public void OnTestSkill(InputValue value)
    {
        if (value.isPressed)
        {
            TestSkillPressed = true;
        }
    }


    public bool ConsumeJump()
    {
        if (!JumpPressed)
            return false;

        JumpPressed = false;
        return true;
    }


    public bool ConsumeTestSkill()
    {
        if (!TestSkillPressed)
            return false;

        TestSkillPressed = false;
        return true;
    }
}