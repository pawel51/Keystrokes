using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Keystrokes.Helpers
{
    public class KeyToCharHelper
    {
        public static char KeyToChar(Key key, bool shiftPushed)
        {

            int intKey = (int)key;

            if (shiftPushed)
            {
                if (intKey <= 69 && intKey >= 44)
                {
                    return (char)(intKey + 21);
                }
            }

            if (intKey <= 69 && intKey >= 44)
            {
                return (char)(intKey + 53);
            }

            if (intKey == (int)Key.Space)
                return (char)32;

            if (intKey == (int)Key.Back)
                return (char)8;

            if (intKey == (int)Key.OemComma)
                return (char)44;

            if (intKey == (int)Key.OemPeriod)
                return (char)46;

            if (intKey == (int)Key.LeftShift)
                return (char)14; // dla ulatwieni lewy shift przyjmuje jako 14 w tablicy ascii

            if (intKey == (int)Key.RightShift)
                return (char)15; // prawy shift przyjmuje jako 15

            return (char)123;
        }
        public static string KeyToString(Key key, bool shiftPushed)
        {

            int intKey = (int)key;

            if (shiftPushed)
            {
                if (intKey <= 69 && intKey >= 44)
                {
                    return key.ToString();
                }
            }

            if (intKey <= 69 && intKey >= 44)
            {
                return key.ToString();
            }

            if (intKey == (int)Key.Space)
                return key.ToString();

            if (intKey == (int)Key.Back)
                return key.ToString();

            if (intKey == (int)Key.OemComma)
                return key.ToString();

            if (intKey == (int)Key.OemPeriod)
                return key.ToString();

            if (intKey == (int)Key.LeftShift)
                return "LSHIFT"; 

            if (intKey == (int)Key.RightShift)
                return "RSHIFT"; 

            return "";
        }

    }
}
