using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }

    public bool JumpPressed { get; private set; }
    public bool TestSkillPressed { get; private set; }
    public bool BasicAttackPressed { get; private set; }
    public bool SecondaryAttackPressed { get; private set; }
    public bool BloodSpearPressed { get; private set; }


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


    public void OnBasicAttack(InputValue value)
    {
        if (value.isPressed)
            BasicAttackPressed = true;
    }

    public bool ConsumeBasicAttack()
    {
        if (!BasicAttackPressed)
            return false;

        BasicAttackPressed = false;
        return true;
    }

    public void OnSecondaryAttack(InputValue value)
    {
        if (value.isPressed)
            SecondaryAttackPressed = true;
    }

    public bool ConsumeSecondaryAttack()
    {
        bool pressed = SecondaryAttackPressed;
        SecondaryAttackPressed = false;
        return pressed;
    }

    public bool ConsumeJump()
    {
        if (!JumpPressed)
            return false;

        JumpPressed = false;
        return true;
    }

    public void OnBloodSpear(InputValue value)
    {
        if (value.isPressed)
            BloodSpearPressed = true;
    }

    public bool ConsumeBloodSpear()
    {
        bool pressed = BloodSpearPressed;
        BloodSpearPressed = false;
        return pressed;
    }

    public bool ConsumeTestSkill()
    {
        if (!TestSkillPressed)
            return false;

        TestSkillPressed = false;
        return true;
    }
}
