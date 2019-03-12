using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;

namespace Hm2Flac3D.Utility
{

    /// <summary> 自定义控件：只能输入数值的文本框。
    /// 当文本框中的数值发生变化时，会触发 ValueNumberChanged 事件，可以通过此事件中的 double 类型的参数来获取最新的数值。
    /// </summary>
    public class TextBoxNum : TextBox
    {
        #region ---   Properties

        /// <summary> double 类型的参数即表示文本框中所代表的最新的数值。 </summary>
        [Browsable(true), DefaultValue(false), Category("数值"), Description("当文本框中的所对应的数值发生变化时触发此事件")]
        public event Action<object, double> ValueNumberChanged = delegate (object sender, double d) { };

        public event EventHandler e;

        private double _valueNumber;
        /// <summary> 文本框中所对应的数值 </summary>
        [Browsable(true), DefaultValue(false), Category("数值"), Description("文本框中所对应的数值")]
        public double ValueNumber
        {
            get { return _valueNumber; }
            private set
            {
                // 触发事件
                ValueNumberChanged(this, value);
                _valueNumber = value;
            }
        }

        private bool _integerOnly;
        /// <summary> 文本框中是否只允许输入整数 </summary>
        [Browsable(true), DefaultValue(false), Category("数值"), Description("文本框中是否只允许输入整数")]
        public bool IntegerOnly
        {
            get { return _integerOnly; }
            set
            {
                _integerOnly = value;
                ConstructRegexPattern();
            }
        }

        private bool _positiveOnly;
        /// <summary> 文本框中是否只允许输入正值（包括0） </summary>
        [Browsable(true), DefaultValue(false), Category("数值"), Description("文本框中是否只允许输入正值（包括0）")]
        public bool PositiveOnly
        {
            get { return _positiveOnly; }
            set
            {
                _positiveOnly = value;
                ConstructRegexPattern();
            }
        }


        #endregion

        #region ---   Fields

        /// <summary> 正则表达式的匹配模式 </summary>
        private string _textPattern;

        private Keys[] _charCollection;

        #endregion

        /// <summary> 构造函数 </summary>
        public TextBoxNum()
        {//
            IntegerOnly = false;
            PositiveOnly = false;

            // 事件关联
            KeyDown += OnKeyDown;
            TextChanged += OnTextChanged;
        }

        /// <summary> 根据是否只允许正数或者整数来设置对应的匹配选项 </summary>
        private void ConstructRegexPattern()
        {
            if (_integerOnly && _positiveOnly)
            {
                _charCollection = new Keys[]
                {
                    Keys.NumPad0, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, Keys.NumPad7, Keys.NumPad8, Keys.NumPad9,
                    Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9,
                    Keys.Back,Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Home, Keys.End,
                };
                _textPattern = @"^[0-9]*$"; // 只允许非负整数
            }
            else if (!_integerOnly && _positiveOnly)
            {
                _charCollection = new Keys[]
                {
                    Keys.NumPad0, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, Keys.NumPad7, Keys.NumPad8, Keys.NumPad9,
                    Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9,
                    Keys.Back,Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Home, Keys.End,
                    Keys.OemPeriod, Keys.Decimal,
                };
                _textPattern = @"^[0-9]*\.?[0-9]*$"; // 不允许非正数
            }
            else if (_integerOnly && !_positiveOnly)
            {
                _charCollection = new Keys[]
              {
                    Keys.NumPad0, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, Keys.NumPad7, Keys.NumPad8, Keys.NumPad9,
                    Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9,
                    Keys.Back,Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Home, Keys.End,
                    Keys.OemMinus, Keys.Subtract,
              };
                _textPattern = @"^-?[0-9]*$";  // 不允许小数
            }
            else if (!_integerOnly && !_positiveOnly)
            {
                _charCollection = new Keys[]
               {
                    Keys.NumPad0, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, Keys.NumPad7, Keys.NumPad8, Keys.NumPad9,
                    Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9,
                    Keys.Back,Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Home, Keys.End,
                    Keys.OemPeriod, Keys.Decimal, Keys.Subtract, Keys.OemMinus,
               };
                _textPattern = @"^-?[0-9]*\.?[0-9]*$";  // 同时允许负数与小数
            }
        }

        #region ---   事件处理
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!ValidateChar(e.KeyCode))
            {
                e.SuppressKeyPress = true;
            }
        }

        private string stringBeforeChanged;
        private void OnTextChanged(object sender, EventArgs e)
        {
            if (ValidateString(Text))
            {
                // 刷新
                stringBeforeChanged = Text;
            }
            else
            {
                // 还原
                Text = stringBeforeChanged;
            }
        }

        private bool ValidateChar(Keys k)
        {
            if (_charCollection.Contains(k))
            {
                return true;
            }
            return false;
        }

        private bool ValidateString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            var m = Regex.Match(text, _textPattern);
            if (m.Success)
            {
                //
                double v;
                ValueNumber = double.TryParse(text, out v) ? v : 0;
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
