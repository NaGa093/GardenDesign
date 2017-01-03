﻿namespace Assets.Controls
{
    using Assets.Enums;

    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// Summary description for RulerControl.
    /// </summary>
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(RulerControl), "Ruler.bmp")]
    public class RulerControl : Control, IMessageFilter
    {
        #region Internal Variables

        private int _Scale;
        private bool _bDrawLine = false;
        private bool _bInControl = false;
        private int _iMousePosition = 1;
        private int _iOldMousePosition = -1;
        private Bitmap _Bitmap = null;

        #endregion Internal Variables

        #region Property variable

        private Orientations _Orientation;
        private ScaleModes _ScaleMode;
        private RulerAlignments _RulerAlignment = RulerAlignments.raBottomOrRight;
        private Border3DStyle _i3DBorderStyle = Border3DStyle.Etched;
        private int _iMajorInterval = 100;
        private int _iNumberOfDivisions = 10;
        private int _DivisionMarkFactor = 5;
        private int _MiddleMarkFactor = 3;
        private double _ZoomFactor = 1;
        private double _StartValue = 0;
        private bool _bMouseTrackingOn = false;
        private bool _VerticalNumbers = true;
        private bool _bActualSize = true;
        private float _DpiX = 96;

        #endregion Property variable

        #region Event Arguments

        public class ScaleModeChangedEventArgs : EventArgs
        {
            public ScaleModes Mode;

            public ScaleModeChangedEventArgs(ScaleModes Mode) : base()
            {
                this.Mode = Mode;
            }
        }

        public class HooverValueEventArgs : EventArgs
        {
            public double Value;

            public HooverValueEventArgs(double Value) : base()
            {
                this.Value = Value;
            }
        }

        #endregion Event Arguments

        #region Delegates

        public delegate void ScaleModeChangedEvent(object sender, ScaleModeChangedEventArgs e);

        public delegate void HooverValueEvent(object sender, HooverValueEventArgs e);

        #endregion Delegates

        #region Events

        public event ScaleModeChangedEvent ScaleModeChanged;

        public event HooverValueEvent HooverValue;

        #endregion Events

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        #region Constrcutors/Destructors

        public RulerControl()
        {
            base.BackColor = System.Drawing.Color.White;
            base.ForeColor = System.Drawing.Color.Black;

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            System.Windows.Forms.MenuItem mnuPoints = new System.Windows.Forms.MenuItem("Points", new EventHandler(Popup_Click));
            System.Windows.Forms.MenuItem mnuPixels = new System.Windows.Forms.MenuItem("Pixels", new EventHandler(Popup_Click));
            System.Windows.Forms.MenuItem mnuCentimetres = new System.Windows.Forms.MenuItem("Centimetres", new EventHandler(Popup_Click));
            System.Windows.Forms.MenuItem mnuInches = new System.Windows.Forms.MenuItem("Inches", new EventHandler(Popup_Click));
            System.Windows.Forms.MenuItem mnuMillimetres = new System.Windows.Forms.MenuItem("Millimetres", new EventHandler(Popup_Click));
            ContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { mnuPoints, mnuPixels, mnuCentimetres, mnuInches, mnuMillimetres });

            Graphics g = this.CreateGraphics();
            _DpiX = g.DpiX;
            ScaleMode = ScaleModes.smPoints;
        }

        #endregion Constrcutors/Destructors

        #region Methods

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool PreFilterMessage(ref Message m)
        {
            if (!this._bMouseTrackingOn) return false;

            if (m.Msg == (int)Msg.WM_MOUSEMOVE)
            {
                int X = 0;
                int Y = 0;

                // The mouse coordinate are measured in screen coordinates because thats what
                // Control.MousePosition returns.  The Message,LParam value is not used because
                // it returns the mouse position relative to the control the mouse is over.
                // Chalk and cheese.

                Point pointScreen = Control.MousePosition;

                // Get the origin of this control in screen coordinates so that later we can
                // compare it against the mouse point to determine it we've hit this control.

                Point pointClientOrigin = new Point(X, Y);
                pointClientOrigin = PointToScreen(pointClientOrigin);

                _bDrawLine = false;
                _bInControl = false;

                HooverValueEventArgs eHoover = null;

                // Work out whether the mouse is within the Y-axis bounds of a vertital ruler or
                // within the X-axis bounds of a horizontal ruler

                if (this.Orientation == Orientations.orHorizontal)
                {
                    _bDrawLine = (pointScreen.X >= pointClientOrigin.X) && (pointScreen.X <= pointClientOrigin.X + this.Width);
                }
                else
                {
                    _bDrawLine = (pointScreen.Y >= pointClientOrigin.Y) && (pointScreen.Y <= pointClientOrigin.Y + this.Height);
                }

                // If the mouse is in valid position...
                if (_bDrawLine)
                {
                    // ...workout the position of the mouse relative to the
                    X = pointScreen.X - pointClientOrigin.X;
                    Y = pointScreen.Y - pointClientOrigin.Y;

                    // Determine whether the mouse is within the bounds of the control itself
                    _bInControl = (this.ClientRectangle.Contains(new Point(X, Y)));

                    // Make the relative mouse position available in pixel relative to this control's origin
                    ChangeMousePosition((this.Orientation == Orientations.orHorizontal) ? X : Y);
                    eHoover = new HooverValueEventArgs(CalculateValue(_iMousePosition));
                }
                else
                {
                    ChangeMousePosition(-1);
                    eHoover = new HooverValueEventArgs(_iMousePosition);
                }

                // Paint directly by calling the OnPaint() method.  This way the background is not
                // hosed by the call to Invalidate() so paining occurs without the hint of a flicker
                PaintEventArgs e = null;
                try
                {
                    e = new PaintEventArgs(this.CreateGraphics(), this.ClientRectangle);
                    OnPaint(e);
                }
                finally
                {
                    e.Graphics.Dispose();
                }

                OnHooverValue(eHoover);
            }

            if ((m.Msg == (int)Msg.WM_MOUSELEAVE) ||
                (m.Msg == (int)Msg.WM_NCMOUSELEAVE))
            {
                _bDrawLine = false;
                PaintEventArgs paintArgs = null;
                try
                {
                    paintArgs = new PaintEventArgs(this.CreateGraphics(), this.ClientRectangle);
                    this.OnPaint(paintArgs);
                }
                finally
                {
                    paintArgs.Graphics.Dispose();
                }
            }

            return false;  // Whether or not the message is filtered
        }

        public double PixelToScaleValue(int iOffset)
        {
            return this.CalculateValue(iOffset);
        }

        public int ScaleValueToPixel(double nScaleValue)
        {
            return CalculatePixel(nScaleValue);
        }

        #endregion Methods

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // RulerControl
            //
            this.Name = "RulerControl";
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RulerControl_MouseUp);

            base.SetStyle(System.Windows.Forms.ControlStyles.DoubleBuffer, true);

            this.ContextMenu = new ContextMenu();
            this.ContextMenu.Popup += new EventHandler(ContextMenu_Popup);
        }

        #endregion Component Designer generated code

        #region Overrides

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // Take private resize actions here
            _Bitmap = null;
            this.Invalidate();
        }

        public override void Refresh()
        {
            base.Refresh();
            this.Invalidate();
        }

        [Description("Draws the ruler marks in the scale requested.")]
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawControl(e.Graphics);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (this.Visible)
            {
                if (_bMouseTrackingOn) Application.AddMessageFilter(this);
            }
            else
            {
                // DOn't change the tracking state so that the filter will be added again when the control is made visible again;
                if (_bMouseTrackingOn) RemoveMessageFilter();
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            RemoveMessageFilter();
            _bMouseTrackingOn = false;
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            RemoveMessageFilter();
            _bMouseTrackingOn = false;
        }

        private void RemoveMessageFilter()
        {
            try
            {
                if (_bMouseTrackingOn)
                {
                    Application.RemoveMessageFilter(this);
                }
            }
            catch { }
        }

        #endregion Overrides

        #region Event Handlers

        private void RulerControl_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //          if (e.Button.Equals(MouseButtons.Right))
        }

        private void RulerControl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((Control.MouseButtons & MouseButtons.Right) != 0)
            {
                this.ContextMenu.Show(this, PointToClient(Control.MousePosition));
            }
            else
            {
                EventArgs eClick = new EventArgs();
                this.OnClick(eClick);
            }
        }

        private void Popup_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MenuItem item = (System.Windows.Forms.MenuItem)sender;
            ScaleMode = (ScaleModes)item.Index;
            _Bitmap = null;
            Invalidate();
        }

        protected void OnHooverValue(HooverValueEventArgs e)
        {
            if (HooverValue != null) HooverValue(this, e);
        }

        protected void OnScaleModeChanged(ScaleModeChangedEventArgs e)
        {
            if (ScaleModeChanged != null) ScaleModeChanged(this, e);
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            _bDrawLine = false;
            Invalidate();
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            Invalidate();
        }

        private void ContextMenu_Popup(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Popup");
        }

        #endregion Event Handlers

        #region Properties

        [
        DefaultValue(typeof(Border3DStyle), "Etched"),
        Description("The border style use the Windows.Forms.Border3DStyle type"),
        Category("Ruler"),
        ]
        public Border3DStyle BorderStyle
        {
            get
            {
                return _i3DBorderStyle;
            }
            set
            {
                _i3DBorderStyle = value;
                _Bitmap = null;
                Invalidate();
            }
        }

        [Description("Horizontal or vertical layout")]
        [Category("Ruler")]
        public Orientations Orientation
        {
            get { return _Orientation; }
            set
            {
                _Orientation = value;
                _Bitmap = null;
                Invalidate();
            }
        }

        [Description("A value from which the ruler marking should be shown.  Default is zero.")]
        [Category("Ruler")]
        public double StartValue
        {
            get { return _StartValue; }
            set
            {
                _StartValue = value;
                _Bitmap = null;
                Invalidate();
            }
        }

        [Description("The scale to use")]
        [Category("Ruler")]
        public ScaleModes ScaleMode
        {
            get { return _ScaleMode; }
            set
            {
                ScaleModes iOldScaleMode = _ScaleMode;
                _ScaleMode = value;

                if (_iMajorInterval == DefaultMajorInterval(iOldScaleMode))
                {
                    // Set the default Scale and MajorInterval value
                    _Scale = DefaultScale(_ScaleMode);
                    _iMajorInterval = DefaultMajorInterval(_ScaleMode);
                }
                else
                {
                    MajorInterval = _iMajorInterval;
                }

                // Use the current start value (if there is one)
                this.StartValue = this._StartValue;

                // Setup the menu
                for (int i = 0; i <= 4; i++)
                    ContextMenu.MenuItems[i].Checked = false;

                ContextMenu.MenuItems[(int)value].Checked = true;

                ScaleModeChangedEventArgs e = new ScaleModeChangedEventArgs(value);
                this.OnScaleModeChanged(e);
            }
        }

        [Description("The value of the major interval.  When displaying inches, 1 is a typical value.  When displaying Points, 36 or 72 might good values.")]
        [Category("Ruler")]
        public int MajorInterval
        {
            get { return _iMajorInterval; }
            set
            {
                if (value <= 0) throw new Exception("The major interval value cannot be less than one");
                _iMajorInterval = value;
                _Scale = DefaultScale(_ScaleMode) * _iMajorInterval / DefaultMajorInterval(_ScaleMode);
                _Bitmap = null;
                Invalidate();
            }
        }

        [Description("How many divisions should be shown between each major interval")]
        [Category("Ruler")]
        public int Divisions
        {
            get { return _iNumberOfDivisions; }
            set
            {
                if (value <= 0) throw new Exception("The number of divisions cannot be less than one");
                _iNumberOfDivisions = value;
                _Bitmap = null;
                Invalidate();
            }
        }

        [Description("The height or width of this control multiplied by the reciprocal of this number will be used to compute the height of the non-middle division marks.")]
        [Category("Ruler")]
        public int DivisionMarkFactor
        {
            get { return _DivisionMarkFactor; }
            set
            {
                if (value <= 0) throw new Exception("The Division Mark Factor cannot be less than one");
                _DivisionMarkFactor = value;
                _Bitmap = null;
                Invalidate();
            }
        }

        [Description("The height or width of this control multiplied by the reciprocal of this number will be used to compute the height of the middle division mark.")]
        [Category("Ruler")]
        public int MiddleMarkFactor
        {
            get { return _MiddleMarkFactor; }
            set
            {
                if (value <= 0) throw new Exception("The Middle Mark Factor cannot be less than one");
                _MiddleMarkFactor = value;
                _Bitmap = null;
                Invalidate();
            }
        }

        [Description("The value of the current mouse position expressed in unit of the scale set (centimetres, inches, etc.")]
        [Category("Ruler")]
        public double ScaleValue
        {
            get { return CalculateValue(_iMousePosition); }
        }

        [Description("TRUE if a line is displayed to track the current position of the mouse and events are generated as the mouse moves.")]
        [Category("Ruler")]
        public bool MouseTrackingOn
        {
            get { return _bMouseTrackingOn; }
            set
            {
                if (value == _bMouseTrackingOn) return;

                if (value)
                {
                    // Tracking is being enabled so add the message filter hook
                    if (this.Visible) Application.AddMessageFilter(this);
                }
                else
                {
                    // Tracking is being disabled so remove the message filter hook
                    Application.RemoveMessageFilter(this);
                    ChangeMousePosition(-1);
                }

                _bMouseTrackingOn = value;

                _Bitmap = null;
                Invalidate();
            }
        }

        [Description("The font used to display the division number")]
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                _Bitmap = null;
                Invalidate();
            }
        }

        [Description("Return the mouse position as number of pixels from the top or left of the control.  -1 means that the mouse is positioned before or after the control.")]
        [Category("Ruler")]
        public int MouseLocation
        {
            get { return _iMousePosition; }
        }

        [DefaultValue(typeof(Color), "ControlDarkDark")]
        [Description("The color used to lines and numbers on the ruler")]
        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
                _Bitmap = null;
                Invalidate();
            }
        }

        [DefaultValue(typeof(Color), "White")]
        [Description("The color used to paint the background of the ruler")]
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
                _Bitmap = null;
                Invalidate();
            }
        }

        [Description("")]
        [Category("Ruler")]
        public bool VerticalNumbers
        {
            get { return _VerticalNumbers; }
            set
            {
                _VerticalNumbers = value;
                _Bitmap = null;
                Invalidate();
            }
        }

        [Description("A factor between 0.1 and 10 by which the displayed scale will be zoomed.")]
        [Category("Ruler")]
        public double ZoomFactor
        {
            get { return _ZoomFactor; }
            set
            {
                if ((value < 0.1) || (value > 10)) throw new Exception("Zoom factor can be between 10% and 1000%");
                if (_ZoomFactor == value) return;
                _ZoomFactor = value;
                this.ScaleMode = _ScaleMode;
                _Bitmap = null;
                Invalidate();
            }
        }

        [Description("True if the ruler measurement is to be based on the systems pixels per inch figure")]
        [Category("Ruler")]
        public bool ActualSize
        {
            get { return _bActualSize; }
            set
            {
                if (_bActualSize == value) return;
                _bActualSize = value;
                this.ScaleMode = _ScaleMode;
                _Bitmap = null;
                Invalidate();
            }
        }

        [Description("Determines how the ruler markings are displayed")]
        [Category("Ruler")]
        public RulerAlignments RulerAlignment
        {
            get { return _RulerAlignment; }
            set
            {
                if (_RulerAlignment == value) return;
                _RulerAlignment = value;
                _Bitmap = null;
                Invalidate();
            }
        }

        #endregion Properties

        #region Private functions

        private double CalculateValue(int iOffset)
        {
            if (iOffset < 0) return 0;

            double nValue = ((double)iOffset - Start()) / (double)_Scale * (double)_iMajorInterval;
            return nValue + this._StartValue;
        }

        [Description("May not return zero even when a -ve scale number is given as the returned value needs to allow for the border thickness")]
        private int CalculatePixel(double nScaleValue)
        {
            double nValue = nScaleValue - this._StartValue;
            if (nValue < 0) return Start();  // Start is the offset to the actual display area to allow for the border (if any)

            int iOffset = Convert.ToInt32(nValue / (double)_iMajorInterval * (double)_Scale);

            return iOffset + Start();
        }

        public void RenderTrackLine(Graphics g)
        {
            if (_bMouseTrackingOn & _bDrawLine)
            {
                int iOffset = Offset();

                // Optionally render Mouse tracking line
                switch (Orientation)
                {
                    case Orientations.orHorizontal:
                        Line(g, _iMousePosition, iOffset, _iMousePosition, Height - iOffset);
                        break;

                    case Orientations.orVertical:
                        Line(g, iOffset, _iMousePosition, Width - iOffset, _iMousePosition);
                        break;
                }
            }
        }

        private void DrawControl(Graphics graphics)
        {
            Graphics g = null;

            if (!this.Visible) return;

            // Bug reported by Kristoffer F
            if (this.Width < 1 || this.Height < 1)
            {
                return;
            }

            if (_Bitmap == null)
            {
                int iValueOffset = 0;
                int iScaleStartValue;

                // Create a bitmap
                _Bitmap = new Bitmap(this.Width, this.Height);

                g = Graphics.FromImage(_Bitmap);

                try
                {
                    // Wash the background with BackColor
                    g.FillRectangle(new SolidBrush(this.BackColor), 0, 0, _Bitmap.Width, _Bitmap.Height);

                    if (this.StartValue >= 0)
                    {
                        iScaleStartValue = Convert.ToInt32(_StartValue * _Scale / _iMajorInterval);  // Convert value to pixels
                    }
                    else
                    {
                        // If the start value is -ve then assume that we are starting just above zero
                        // For example if the requested value -1.1 then make believe that the start is
                        // +0.9.  We can fix up the printing of numbers later.
                        double dStartValue = Math.Ceiling(Math.Abs(_StartValue)) - Math.Abs(_StartValue);

                        // Compute the offset that is to be used with the start point is -ve
                        // This will be subtracted from the number calculated for the display numeral
                        iScaleStartValue = Convert.ToInt32(dStartValue * _Scale / _iMajorInterval);  // Convert value to pixels
                        iValueOffset = Convert.ToInt32(Math.Ceiling(Math.Abs(_StartValue)));
                    };

                    // Paint the lines on the image
                    int iScale = _Scale;

                    int iStart = Start();  // iStart is the pixel number on which to start.
                    int iEnd = (this.Orientation == Orientations.orHorizontal) ? Width : Height;

                    for (int j = iStart; j <= iEnd; j += iScale)
                    {
                        int iLeft = _Scale;  // Make an assumption that we're starting at zero or on a major increment
                        int jOffset = j + iScaleStartValue;

                        iScale = ((jOffset - iStart) % _Scale);  // Get the mod value to see if this is "big line" opportunity

                        // If it is, draw big line
                        if (iScale == 0)
                        {
                            if (_RulerAlignment != RulerAlignments.raMiddle)
                            {
                                if (this.Orientation == Orientations.orHorizontal)
                                    Line(g, j, 0, j, Height);
                                else
                                    Line(g, 0, j, Width, j);
                            }

                            iLeft = _Scale;     // Set the for loop increment
                        }
                        else
                        {
                            iLeft = _Scale - Math.Abs(iScale);     // Set the for loop increment
                        }

                        iScale = iLeft;

                        int iValue = (((jOffset - iStart) / _Scale) + 1) * _iMajorInterval;

                        // Accommodate the offset if the starting point is -ve
                        iValue -= iValueOffset;
                        DrawValue(g, iValue, j - iStart, iScale);

                        int iUsed = 0;

                        // TO DO: This must be wrong when the start is negative and not a whole number
                        //Draw small lines
                        for (int i = 0; i < _iNumberOfDivisions; i++)
                        {
                            // Get the increment for the next mark
                            int iX = Convert.ToInt32(Math.Round((double)(_Scale - iUsed) / (double)(_iNumberOfDivisions - i), 0)); // Use a spreading algorithm rather that using expensive floating point numbers

                            // So the next mark will have used up
                            iUsed += iX;

                            if (iUsed >= (_Scale - iLeft))
                            {
                                iX = iUsed + j - (_Scale - iLeft);

                                // Is it an even number and, if so, is it the middle value?
                                bool bMiddleMark = ((_iNumberOfDivisions & 0x1) == 0) & (i + 1 == _iNumberOfDivisions / 2);
                                bool bShowMiddleMark = bMiddleMark;
                                bool bLastDivisionMark = (i + 1 == _iNumberOfDivisions);
                                bool bLastAlignMiddleDivisionMark = bLastDivisionMark & (_RulerAlignment == RulerAlignments.raMiddle);
                                bool bShowDivisionMark = !bMiddleMark & !bLastAlignMiddleDivisionMark;

                                if (bShowMiddleMark)
                                {
                                    DivisionMark(g, iX, _MiddleMarkFactor);  // Height or Width will be 1/3
                                }
                                else if (bShowDivisionMark)
                                {
                                    DivisionMark(g, iX, _DivisionMarkFactor);  // Height or Width will be 1/5
                                }
                            }
                        }
                    }

                    if (_i3DBorderStyle != Border3DStyle.Flat)
                        ControlPaint.DrawBorder3D(g, this.ClientRectangle, this._i3DBorderStyle);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                finally
                {
                    g.Dispose();
                }
            }

            g = graphics;

            try
            {
                // Always draw the bitmap
                g.DrawImage(_Bitmap, this.ClientRectangle);

                RenderTrackLine(g);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                GC.Collect();
            }
        }

        private void DivisionMark(Graphics g, int iPosition, int iProportion)
        {
            // This function is affected by the RulerAlignment setting

            int iMarkStart = 0, iMarkEnd = 0;

            if (this.Orientation == Orientations.orHorizontal)
            {
                switch (_RulerAlignment)
                {
                    case RulerAlignments.raBottomOrRight:
                        {
                            iMarkStart = Height - Height / iProportion;
                            iMarkEnd = Height;
                            break;
                        }
                    case RulerAlignments.raMiddle:
                        {
                            iMarkStart = (Height - Height / iProportion) / 2 - 1;
                            iMarkEnd = iMarkStart + Height / iProportion;
                            break;
                        }
                    case RulerAlignments.raTopOrLeft:
                        {
                            iMarkStart = 0;
                            iMarkEnd = Height / iProportion;
                            break;
                        }
                }

                Line(g, iPosition, iMarkStart, iPosition, iMarkEnd);
            }
            else
            {
                switch (_RulerAlignment)
                {
                    case RulerAlignments.raBottomOrRight:
                        {
                            iMarkStart = Width - Width / iProportion;
                            iMarkEnd = Width;
                            break;
                        }
                    case RulerAlignments.raMiddle:
                        {
                            iMarkStart = (Width - Width / iProportion) / 2 - 1;
                            iMarkEnd = iMarkStart + Width / iProportion;
                            break;
                        }
                    case RulerAlignments.raTopOrLeft:
                        {
                            iMarkStart = 0;
                            iMarkEnd = Width / iProportion;
                            break;
                        }
                }

                Line(g, iMarkStart, iPosition, iMarkEnd, iPosition);
            }
        }

        private void DrawValue(Graphics g, int iValue, int iPosition, int iSpaceAvailable)
        {
            // The sizing operation is common to all options
            StringFormat format = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
            if (_VerticalNumbers)
                format.FormatFlags |= StringFormatFlags.DirectionVertical;

            SizeF size = g.MeasureString((iValue).ToString(), this.Font, iSpaceAvailable, format);

            Point drawingPoint;
            int iX = 0;
            int iY = 0;

            if (this.Orientation == Orientations.orHorizontal)
            {
                switch (_RulerAlignment)
                {
                    case RulerAlignments.raBottomOrRight:
                        {
                            iX = iPosition + iSpaceAvailable - (int)size.Width - 2;
                            iY = 2;
                            break;
                        }
                    case RulerAlignments.raMiddle:
                        {
                            iX = iPosition + iSpaceAvailable - (int)size.Width / 2;
                            iY = (Height - (int)size.Height) / 2 - 2;
                            break;
                        }
                    case RulerAlignments.raTopOrLeft:
                        {
                            iX = iPosition + iSpaceAvailable - (int)size.Width - 2;
                            iY = Height - 2 - (int)size.Height;
                            break;
                        }
                }

                drawingPoint = new Point(iX, iY);
            }
            else
            {
                switch (_RulerAlignment)
                {
                    case RulerAlignments.raBottomOrRight:
                        {
                            iX = 2;
                            iY = iPosition + iSpaceAvailable - (int)size.Height - 2;
                            break;
                        }
                    case RulerAlignments.raMiddle:
                        {
                            iX = (Width - (int)size.Width) / 2 - 1;
                            iY = iPosition + iSpaceAvailable - (int)size.Height / 2;
                            break;
                        }
                    case RulerAlignments.raTopOrLeft:
                        {
                            iX = Width - 2 - (int)size.Width;
                            iY = iPosition + iSpaceAvailable - (int)size.Height - 2;
                            break;
                        }
                }

                drawingPoint = new Point(iX, iY);
            }

            // The drawstring function is common to all operations

            g.DrawString(iValue.ToString(), this.Font, new SolidBrush(this.ForeColor), drawingPoint, format);
        }

        private void Line(Graphics g, int x1, int y1, int x2, int y2)
        {
            using (SolidBrush brush = new SolidBrush(this.ForeColor))
            {
                using (Pen pen = new Pen(brush))
                {
                    g.DrawLine(pen, x1, y1, x2, y2);
                    pen.Dispose();
                    brush.Dispose();
                }
            }
        }

        private int DefaultScale(ScaleModes iScaleMode)
        {
            int iScale = 100;

            // Set scaling
            switch (iScaleMode)
            {
                // Determines the *relative* proportions of each scale
                case ScaleModes.smPoints:
                    iScale = 660; // 132;
                    break;

                case ScaleModes.smPixels:
                    iScale = 100;
                    break;

                case ScaleModes.smCentimetres:
                    iScale = 262; // 53;
                    break;

                case ScaleModes.smInches:
                    iScale = 660; // 132;
                    break;

                case ScaleModes.smMillimetres:
                    iScale = 27;
                    break;
                    /*
                                    case ScaleModes.smPoints:
                                        iScale = 96;
                                        break;

                                    case ScaleModes.smPixels:
                                        iScale = 100;
                                        break;

                                    case ScaleModes.smCentimetres:
                                        iScale = 38;
                                        break;

                                    case ScaleModes.smInches:
                                        iScale = 96;
                                        break;

                                    case ScaleModes.smMillimetres:
                                        iScale = 4;
                                        break;
                    */
            }

            if (iScaleMode == ScaleModes.smPixels)
                return Convert.ToInt32((double)iScale * _ZoomFactor);
            else
                return Convert.ToInt32((double)iScale * _ZoomFactor * (double)(_bActualSize ? (double)_DpiX / (float)480 : 0.2));
        }

        private int DefaultMajorInterval(ScaleModes iScaleMode)
        {
            int iInterval = 10;

            // Set scaling
            switch (iScaleMode)
            {
                // Determines the *relative* proportions of each scale
                case ScaleModes.smPoints:
                    iInterval = 72;
                    break;

                case ScaleModes.smPixels:
                    iInterval = 100;
                    break;

                case ScaleModes.smCentimetres:
                    iInterval = 1;
                    break;

                case ScaleModes.smInches:
                    iInterval = 1;
                    break;

                case ScaleModes.smMillimetres:
                    iInterval = 1;
                    break;
            }

            return iInterval;
        }

        private int Offset()
        {
            int iOffset = 0;

            switch (this._i3DBorderStyle)
            {
                case Border3DStyle.Flat: iOffset = 0; break;
                case Border3DStyle.Adjust: iOffset = 0; break;
                case Border3DStyle.Sunken: iOffset = 2; break;
                case Border3DStyle.Bump: iOffset = 2; break;
                case Border3DStyle.Etched: iOffset = 2; break;
                case Border3DStyle.Raised: iOffset = 2; break;
                case Border3DStyle.RaisedInner: iOffset = 1; break;
                case Border3DStyle.RaisedOuter: iOffset = 1; break;
                case Border3DStyle.SunkenInner: iOffset = 1; break;
                case Border3DStyle.SunkenOuter: iOffset = 1; break;
                default: iOffset = 0; break;
            }

            return iOffset;
        }

        private int Start()
        {
            int iStart = 0;

            switch (this._i3DBorderStyle)
            {
                case Border3DStyle.Flat: iStart = 0; break;
                case Border3DStyle.Adjust: iStart = 0; break;
                case Border3DStyle.Sunken: iStart = 1; break;
                case Border3DStyle.Bump: iStart = 1; break;
                case Border3DStyle.Etched: iStart = 1; break;
                case Border3DStyle.Raised: iStart = 1; break;
                case Border3DStyle.RaisedInner: iStart = 0; break;
                case Border3DStyle.RaisedOuter: iStart = 0; break;
                case Border3DStyle.SunkenInner: iStart = 0; break;
                case Border3DStyle.SunkenOuter: iStart = 0; break;
                default: iStart = 0; break;
            }
            return iStart;
        }

        private void ChangeMousePosition(int iNewPosition)
        {
            this._iOldMousePosition = this._iMousePosition;
            this._iMousePosition = iNewPosition;
        }
    }

    #endregion Private functions
}