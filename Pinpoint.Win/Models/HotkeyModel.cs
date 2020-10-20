using System;
using System.Text;
using System.Windows.Input;

namespace Pinpoint.Win.Models
{
    public class HotkeyModel
    {
        public HotkeyModel()
        {
        }

        public HotkeyModel(string text)
        {
            Text = text;
            IdentifyKeys(text);
        }

        public HotkeyModel(Key key, ModifierKeys modifiers)
        {
            Key = key;
            Modifiers = modifiers;

            var sb = new StringBuilder();
            if (Modifiers.HasFlag(ModifierKeys.Control))
            {
                sb.Append("Ctrl + ");
            }
            if (Modifiers.HasFlag(ModifierKeys.Shift))
            {
                sb.Append("Shift + ");
            }
            if (Modifiers.HasFlag(ModifierKeys.Alt))
            {
                sb.Append("Alt + ");
            }
            if (Modifiers.HasFlag(ModifierKeys.Windows))
            {
                sb.Append("Win + ");
            }
            sb.Append(Key);

            Text = sb.ToString();
        }

        private void IdentifyKeys(string hotkeys)
        {
            hotkeys = hotkeys.ToLower().Replace(" ", "").Replace("left", "").Replace("right", "");

            bool Check(string identifier)
            {
                if (hotkeys.Contains(identifier))
                {
                    hotkeys = hotkeys.Replace(identifier, "");
                    return true;
                }

                return false;
            }

            IsAlt = Check("alt");
            IsShift = Check("shift");
            IsWin = Check("win");
            IsCtrl = Check("ctrl");

            hotkeys = hotkeys.Replace("+", "");

            Key = hotkeys switch
            {
                "space" => Key.Space,
                "~" => Key.Oem3,
                _ => (Key) Enum.Parse(typeof(Key), hotkeys.ToUpper())
            };
        }

        public Key Key { get; set; }

        public string Text { get; set; }

        public bool IsAlt
        {
            set
            {
                if (value)
                {
                    Modifiers |= ModifierKeys.Alt;
                }
            }
        }

        public bool IsShift
        {
            set
            {
                if (value)
                {
                    Modifiers |= ModifierKeys.Shift;
                }
            }
        }

        public bool IsWin
        {
            set
            {
                if (value)
                {
                    Modifiers |= ModifierKeys.Windows;
                }
            }
        }

        public bool IsCtrl
        {
            set
            {
                if (value)
                {
                    Modifiers |= ModifierKeys.Control;
                }
            }
        }

        public ModifierKeys Modifiers { get; private set; } = ModifierKeys.None;

        public override string ToString()
        {
            return Text;
        }
    }
}
