using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MyCalculator
{
    public partial class YozoCalculator : Form
    {
        decimal num1, num2, result;
        string opertr;
        bool AutoCompu;
        bool isOpertrClick;
        bool btnNumClick;


        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;


        [DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr hWnd);

        public YozoCalculator()
        {
            InitializeComponent();


            this.KeyPreview = true; // Make sure the form processes key events first
            this.KeyDown += new KeyEventHandler(MyCalculatorForm_KeyPress);

            panelTitleBar.MouseDown += panelTitleBar_MouseDown;

            panelTitleBar.MouseMove += panelTitleBar_MouseMove;

            panelTitleBar.MouseUp += panelTitleBar_MouseUp;

            txtDisplay.GotFocus += (sender, e) => HideCaret(txtDisplay.Handle);
            txtPreview.GotFocus += (sender, e) => HideCaret(txtPreview.Handle);

            txtPreview.TextChanged += new EventHandler(txtPreview_TextChanged);

        }

        #region PanelControl
        private void panelTitleBar_MouseDown(object sender, EventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void panelTitleBar_MouseMove(object sender, EventArgs e)
        {
            if (dragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(diff));
            }
        }

        private void panelTitleBar_MouseUp(object sender, EventArgs e)
        {
            dragging = false;
        }
        #endregion


        private void btnNumber_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            foreach (char c in txtDisplay.Text)
            {
                if (btnNumClick == true)
                {
                    if (num2 != 0)
                    {
                        num1 = result;
                        num2 = decimal.Parse(txtDisplay.Text);
                        txtDisplay.Clear();
                    }
                }
                btnNumClick = false;

                if (txtDisplay.Text == "0" || !char.IsDigit(c) && c != '.')
                    txtDisplay.Clear();
                btnCompute.Focus();
                break;

            }

            if (isOpertrClick)
            {
                txtDisplay.Clear();
                AutoCompu = true;
                isOpertrClick = false;
            }
            txtDisplay.Text += button.Text;
            btnCompute.Focus();
        }

        private void btnOpertr_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            if (AutoCompu)
            {
                btnNumClick = true;
                num2 = decimal.Parse(txtDisplay.Text);
                result = Compute(num1, num2, opertr);
                opertr = button.Text;
                result = Math.Abs(result);

                //txtDisplay.Text = result.ToString();
                //txtPreview.Text = $"{result} {opertr}";

                //num1 = result;
                txtDisplay.Text = result % 1 == 0 ? result.ToString("#,###0") : result.ToString("#,##0.##########");
                txtPreview.Text = $"{FormatNumber(num1)} {opertr} ";
                isOpertrClick = true;
                //UpdateDisplay();
                //AutoCompu = false;
                btnCompute.Focus();
                return;
            }

            try
            {
                num1 = Convert.ToDecimal(txtDisplay.Text);
            }
            catch
            {
                ClearAfter();
            }

            isOpertrClick = true;
            opertr = button.Text;
            btnCompute.Focus();
            UpdateDisplay();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearAfter();
        }

        private void btnNeg_Click(object sender, EventArgs e)
        {

            if (decimal.TryParse(txtDisplay.Text, out decimal num1))
            {
                num1 = Convert.ToDecimal(txtDisplay.Text);

                if (num1 > 0)
                {
                    num1 = -num1; // Negate the positive number
                    txtDisplay.Text = num1.ToString();
                    txtPreview.Text = $"negate ({num1})";
                    btnCompute.Focus();
                }
                else if (num1 < 0)
                {
                    num1 = Math.Abs(num1); // Remove the negation by taking the absolute value
                    txtDisplay.Text = num1.ToString();
                    txtPreview.Text = $"{num1}";
                    btnCompute.Focus();
                }
                btnCompute.Focus();
            }
            else
            {
                txtDisplay.Text = "Syntax Error";
                txtPreview.Clear();
                btnCompute.Focus();
            }
            btnCompute.Focus();
        }


        #region EventHandlers
        private void txtDisplay_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            AdjustFontSize(txtDisplay);
            LimitTextLength(txtDisplay);

            string value = txtDisplay.Text.Replace(",", "");
            long txtBxNum;

            if (long.TryParse(value, out txtBxNum))
            {
                txtDisplay.TextChanged -= txtDisplay_TextChanged;
                txtDisplay.Text = string.Format("{0:#,#0}", txtBxNum);
                txtDisplay.SelectionStart = txtDisplay.Text.Length;
                txtDisplay.TextChanged += txtDisplay_TextChanged;
            }
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            delNum();
            btnCompute.Focus();
        }

        private void btnSq2_Click(object sender, EventArgs e)
        {
            try
            {
                num1 = decimal.Parse(txtDisplay.Text);
                if (num1 != 0)
                {
                    result = num1 * num1;

                    txtDisplay.Text = result.ToString();
                    txtPreview.Text = $"sqr({num1})";
                    result = num1;
                    btnCompute.Focus();
                }
                else txtPreview.Clear(); btnCompute.Focus();
            }
            catch
            {
                txtDisplay.Text = $"Overflow";
                txtPreview.Text = result.ToString();
                num1 = 0;
                num2 = 0;
                result = 0;
                btnCompute.Focus();
            }
        }

        private void btnDot_Click(object sender, EventArgs e)
        {
            if (!txtDisplay.Text.Contains("."))
                txtDisplay.Text += ".";
            btnCompute.Focus();
        }

        private void btnCompute_Click(object sender, EventArgs e)
        {
            btnNumClick = true;
            if (num2 == 0)
            {
                if (decimal.TryParse(txtDisplay.Text, out num2))
                {
                    num2 = decimal.Parse(txtDisplay.Text);
                    result = Compute(num1, num2, opertr);

                    if (!(num1 == 0 && num2 == 0))
                    {
                        txtDisplay.Text = result.ToString();
                        txtPreview.Text = $"{FormatNumber(num1)} {opertr} {FormatNumber(num2)} = ";

                        txtDisplay.Text = result % 1 == 0 ? result.ToString("#,##0") : result.ToString("#,##0.##########");
                        btnCompute.Focus();
                    }
                    else txtPreview.Text = "0"; btnCompute.Focus();
                }
            }
            else
            {
                num1 = decimal.Parse(txtDisplay.Text);
                result = Compute(num1, num2, opertr);

                if (!(num1 == 0 && num2 == 0))
                {
                    txtDisplay.Text = result.ToString();
                    txtPreview.Text = $"{FormatNumber(num1)} {opertr} {FormatNumber(num2)} = ";

                    txtDisplay.Text = result % 1 == 0 ? result.ToString("#,##0") : result.ToString("#,##0.##########");
                    btnCompute.Focus();
                }
                else txtPreview.Text = "0"; btnCompute.Focus();
            }
        }

        private void txtPreview_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            LimitTextLength(txtPreview);
            //AdjustFontSize(textBox);
            //string input = txtPreview.Text.Replace(",", "");
            //if (decimal.TryParse(input, out decimal number))
            //{
            //    txtPreview.TextChanged -= txtPreview_TextChanged;
            //    txtPreview.TextChanged += txtPreview_TextChanged;
            //}

            //if (decimal.TryParse(txtPreview.Text.Replace(",", ""), out decimal number))
            //{
            //    txtPreview.TextChanged -= txtPreview_TextChanged; // Temporarily remove handler
            //    txtPreview.Text = number > 1_000_000 ? FormatScientificNotation(number) : FormatNumber(number); // Switch format based on threshold
            //    txtPreview.SelectionStart = txtPreview.Text.Length; // Set cursor to the end
            //    txtPreview.TextChanged += txtPreview_TextChanged; // Reattach handler
            //}
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
            btnCompute.Focus();
        }

        private void btnClrExistence_Click(object sender, EventArgs e)
        {
            txtDisplay.Clear();
            txtDisplay.Text = "0";
            num2 = 0;
        }

        private void MyCalculatorForm_KeyPress(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.NumPad0:
                case Keys.D0:
                    btnNumber_Click(btn0, EventArgs.Empty);
                    btnCompute.Focus();
                    break;
                case Keys.NumPad1:
                case Keys.D1:
                    btnNumber_Click(btn1, EventArgs.Empty);
                    btnCompute.Focus();
                    break;
                case Keys.NumPad2:
                case Keys.D2:
                    btnNumber_Click(btn2, EventArgs.Empty);
                    btnCompute.Focus();
                    break;
                case Keys.NumPad3:
                case Keys.D3:
                    btnNumber_Click(btn3, EventArgs.Empty);
                    btnCompute.Focus();
                    break;
                case Keys.NumPad4:
                case Keys.D4:
                    btnNumber_Click(btn4, EventArgs.Empty);
                    btnCompute.Focus();
                    break;
                case Keys.NumPad5:
                case Keys.D5:
                    btnNumber_Click(btn5, EventArgs.Empty);
                    btnCompute.Focus();
                    break;
                case Keys.NumPad6:
                case Keys.D6:
                    btnNumber_Click(btn6, EventArgs.Empty);
                    btnCompute.Focus();
                    break;
                case Keys.NumPad7:
                case Keys.D7:
                    btnNumber_Click(btn7, EventArgs.Empty);
                    btnCompute.Focus();
                    break;
                case Keys.NumPad8:
                case Keys.D8:
                    btnNumber_Click(btn8, EventArgs.Empty);
                    btnCompute.Focus();
                    break;
                case Keys.NumPad9:
                case Keys.D9:
                    btnNumber_Click(btn9, EventArgs.Empty);
                    btnCompute.Focus();
                    break;
                case Keys.Add:
                    btnOpertr_Click(btnAdd, EventArgs.Empty);
                    btnCompute.Focus();
                    break;
                case Keys.Subtract:
                    btnOpertr_Click(btnSubt, EventArgs.Empty);
                    btnCompute.Focus();
                    break;
                case Keys.Multiply:
                    btnOpertr_Click(btnMult, EventArgs.Empty);
                    btnCompute.Focus();
                    break;
                case Keys.Divide:
                    btnOpertr_Click(btnDiv, EventArgs.Empty);
                    btnCompute.Focus();
                    break;
                case Keys.E:
                    btnCompute_Click(btnCompute, EventArgs.Empty);
                    btnCompute.Focus();
                    break;
                case Keys.Back:
                    delNum();
                    btnCompute.Focus();
                    break;
                case Keys.Delete:
                    ClearAfter();
                    btnCompute.Focus();
                    break;
                case Keys.Decimal:
                    btnDot_Click(btnDot, EventArgs.Empty);
                    btnCompute.Focus();
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Private Methods
        private void AdjustFontSize(TextBox textBox)
        {
            int minFontSize = 31;  // Define the minimum font size
            int maxFontSize = 34; // Define the maximum font size
            int currentFontSize = maxFontSize;

            // Set initial font size to the maximum
            textBox.Font = new Font(textBox.Font.FontFamily, currentFontSize);

            // Measure the text size
            Size textSize = TextRenderer.MeasureText(textBox.Text, textBox.Font);

            // Reduce font size until text fits within the TextBox width
            while (textSize.Width > textBox.Width && currentFontSize > minFontSize)
            {
                currentFontSize--;
                textBox.Font = new Font(textBox.Font.FontFamily, currentFontSize);
                textSize = TextRenderer.MeasureText(textBox.Text, textBox.Font);
            }
        }

        private void LimitTextLength(TextBox textBox)
        {
            while (TextRenderer.MeasureText(textBox.Text, textBox.Font).Width > textBox.Width)
            {
                textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1);
            }
        }

        private string FormatScientificNotation(decimal number)
        {
            return number.ToString("E2", CultureInfo.InvariantCulture); // "E2" means scientific notation with 2 decimal places
        }

        private void delNum()
        {
            txtDisplay.Text = txtDisplay.Text.Substring(0, txtDisplay.Text.Length - 1);
            if (string.IsNullOrEmpty(txtDisplay.Text))
            {
                txtDisplay.Text = "0";
            }
        }

        private void UpdateDisplay()
        {
            string formattedNum1 = FormatNumber(num1);
            string formattedNum2 = num2 != 0 ? FormatNumber(num2) : string.Empty;
            //string formattedResult = result != 0 ? FormatNumber(result) : txtDisplay.Text = "0";
            //Debug.WriteLine($"num1: {formattedNum1}, num2: {formattedNum2}, opertr: {opertr}");
            //txtPreview.Text = $"{formattedNum1} {opertr} {formattedNum2}";
            if (string.IsNullOrEmpty(opertr))
            {
                txtPreview.Text = $"{formattedNum1}";
            }
            else
            {
                txtPreview.Text = $"{formattedNum1} {opertr} {formattedNum2}";
            }
        }

        private string FormatNumber(decimal number)
        {
            return number.ToString("#,##0.##", CultureInfo.InvariantCulture); // Format with comma separator
        }

        private void ClearAfter()
        {
            txtDisplay.Clear();
            txtDisplay.Text = "0";
            txtPreview.Clear();
            AutoCompu = false;
            isOpertrClick = false;
            num1 = 0;
            num2 = 0;
            result = 0;
        }

        private decimal Compute(decimal num1, decimal num2, string opertr)
        {
            decimal result = 0;
            try
            {
                switch (opertr)
                {
                    case "+":
                        result = num1 + num2;
                        break;
                    case "-":
                        result = num1 - num2;
                        break;
                    case "×":
                        result = num1 * num2;
                        break;
                    case "÷":
                        result = num1 / num2;
                        break;
                    default: throw new InvalidOperationException("Syntax Error");
                }
            }
            catch (Exception ex)
            {
                txtPreview.Text = "Syntax Error " + ex.Message;
                num1 = 0;
                num2 = 0;
                result = 0;
            }
            return result;
        }

        #endregion
    }
}