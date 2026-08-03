using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public static class InputCompat
{
    public static bool GetKey(KeyCode keyCode)
    {
        ButtonControl control = Resolve(keyCode);
        return control != null && control.isPressed;
    }

    public static bool GetKeyDown(KeyCode keyCode)
    {
        ButtonControl control = Resolve(keyCode);
        return control != null && control.wasPressedThisFrame;
    }

    public static bool AnyKeyDown
    {
        get
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.anyKey.wasPressedThisFrame)
            {
                return true;
            }
            Mouse mouse = Mouse.current;
            return mouse != null && (mouse.leftButton.wasPressedThisFrame ||
                                     mouse.rightButton.wasPressedThisFrame ||
                                     mouse.middleButton.wasPressedThisFrame);
        }
    }

    public static char TypedCharThisFrame
    {
        get
        {
            EnsureTextHook();
            return typedFrame == Time.frameCount ? typedChar : '\0';
        }
    }

    public static Vector3 MousePosition
    {
        get
        {
            Mouse mouse = Mouse.current;
            return mouse != null ? (Vector3)mouse.position.ReadValue() : Vector3.zero;
        }
    }

    public static float GetAxisRaw(string axisName)
    {
        switch (axisName)
        {
            case "Horizontal": return KeyAxis(Key.A, Key.D, Key.LeftArrow, Key.RightArrow);
            case "Vertical": return KeyAxis(Key.S, Key.W, Key.DownArrow, Key.UpArrow);

            case "Mouse X": return Mouse.current != null ? Mouse.current.delta.ReadValue().x * 0.05f : 0f;
            case "Mouse Y": return Mouse.current != null ? Mouse.current.delta.ReadValue().y * 0.05f : 0f;
            default: return 0f;
        }
    }

    private static Keyboard subscribedKeyboard;
    private static char typedChar;
    private static int typedFrame = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void HookTextInput() => EnsureTextHook();

    private static void EnsureTextHook()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == subscribedKeyboard)
        {
            return;
        }
        if (subscribedKeyboard != null)
        {
            subscribedKeyboard.onTextInput -= OnTextInput;
        }
        subscribedKeyboard = keyboard;
        if (keyboard != null)
        {
            keyboard.onTextInput += OnTextInput;
        }
    }

    private static void OnTextInput(char c)
    {
        if (typedFrame == Time.frameCount)
        {
            return;
        }
        typedFrame = Time.frameCount;
        typedChar = c;
    }

    private static float KeyAxis(Key negative, Key positive, Key negativeAlt, Key positiveAlt)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return 0f;
        }
        float value = 0f;
        if (keyboard[positive].isPressed || keyboard[positiveAlt].isPressed) value += 1f;
        if (keyboard[negative].isPressed || keyboard[negativeAlt].isPressed) value -= 1f;
        return value;
    }

    private static ButtonControl Resolve(KeyCode keyCode)
    {
        if (keyCode >= KeyCode.Mouse0 && keyCode <= KeyCode.Mouse6)
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return null;
            }
            switch (keyCode)
            {
                case KeyCode.Mouse0: return mouse.leftButton;
                case KeyCode.Mouse1: return mouse.rightButton;
                case KeyCode.Mouse2: return mouse.middleButton;
                case KeyCode.Mouse3: return mouse.backButton;
                case KeyCode.Mouse4: return mouse.forwardButton;
                default: return null;
            }
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return null;
        }
        Key key = ToKey(keyCode);
        return key == Key.None ? null : keyboard[key];
    }

    private static Key ToKey(KeyCode keyCode)
    {
        if (keyCode >= KeyCode.A && keyCode <= KeyCode.Z) return Key.A + (keyCode - KeyCode.A);
        if (keyCode >= KeyCode.Alpha1 && keyCode <= KeyCode.Alpha9) return Key.Digit1 + (keyCode - KeyCode.Alpha1);
        if (keyCode >= KeyCode.Keypad0 && keyCode <= KeyCode.Keypad9) return Key.Numpad0 + (keyCode - KeyCode.Keypad0);
        if (keyCode >= KeyCode.F1 && keyCode <= KeyCode.F12) return Key.F1 + (keyCode - KeyCode.F1);

        switch (keyCode)
        {
            case KeyCode.Alpha0: return Key.Digit0;
            case KeyCode.Space: return Key.Space;
            case KeyCode.Return: return Key.Enter;
            case KeyCode.KeypadEnter: return Key.NumpadEnter;
            case KeyCode.Escape: return Key.Escape;
            case KeyCode.Tab: return Key.Tab;
            case KeyCode.Backspace: return Key.Backspace;
            case KeyCode.Delete: return Key.Delete;
            case KeyCode.Insert: return Key.Insert;
            case KeyCode.Home: return Key.Home;
            case KeyCode.End: return Key.End;
            case KeyCode.PageUp: return Key.PageUp;
            case KeyCode.PageDown: return Key.PageDown;
            case KeyCode.UpArrow: return Key.UpArrow;
            case KeyCode.DownArrow: return Key.DownArrow;
            case KeyCode.LeftArrow: return Key.LeftArrow;
            case KeyCode.RightArrow: return Key.RightArrow;
            case KeyCode.LeftShift: return Key.LeftShift;
            case KeyCode.RightShift: return Key.RightShift;
            case KeyCode.LeftControl: return Key.LeftCtrl;
            case KeyCode.RightControl: return Key.RightCtrl;
            case KeyCode.LeftAlt: return Key.LeftAlt;
            case KeyCode.RightAlt: return Key.RightAlt;
            case KeyCode.CapsLock: return Key.CapsLock;
            case KeyCode.Comma: return Key.Comma;
            case KeyCode.Period: return Key.Period;
            case KeyCode.Slash: return Key.Slash;
            case KeyCode.Backslash: return Key.Backslash;
            case KeyCode.Semicolon: return Key.Semicolon;
            case KeyCode.Quote: return Key.Quote;
            case KeyCode.LeftBracket: return Key.LeftBracket;
            case KeyCode.RightBracket: return Key.RightBracket;
            case KeyCode.Minus: return Key.Minus;
            case KeyCode.Equals: return Key.Equals;
            case KeyCode.BackQuote: return Key.Backquote;
            default: return Key.None;
        }
    }
}
