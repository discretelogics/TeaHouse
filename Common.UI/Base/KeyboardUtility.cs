using System.Windows.Input;

namespace TeaTime
{
    public static class KeyboardUtility
    {
        public static bool IsCtrlPressed
        {
            get
            {
                return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            }
        }
        public static bool IsShiftPressed
        {
            get
            {
                return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            }
        }
        public static bool IsAltPressed
        {
            get
            {
                return Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            }
        }

        public static bool IsCtrl(this Key key)
        {
            return (key == Key.LeftCtrl) || (key == Key.RightCtrl);
        }

        public static bool IsShift(this Key key)
        {
            return (key == Key.LeftShift) || (key == Key.RightShift);
        }

        public static bool IsAlt(this Key key)
        {
            return (key == Key.LeftAlt) || (key == Key.RightAlt);
        }
    }
}
