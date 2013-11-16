using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Elmanager.Forms;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace Elmanager.EditorTools
{
    internal class SmoothenTool : ToolBase, IEditorTool
    {
        private Polygon _currentPolygon;
        private bool _smoothAll;
        private List<Polygon> _smoothPolys;
        private int _smoothSteps = 3;
        private double _smoothVertexOffset = 0.7;
        private bool _smoothing;
        private bool _unsmooth;
        private double _unsmoothAngle = 10;
        private double _unsmoothLength = 1.0;

        internal SmoothenTool(LevelEditor editor) : base(editor)
        {
        }

        private bool Smoothing
        {
            get { return _smoothing; }
            set
            {
                _smoothing = value;
                _Busy = value;
            }
        }

        public void Activate()
        {
            UpdateHelp();
            Renderer.RedrawScene();
        }

        public void ExtraRendering()
        {
            if (Smoothing)
                foreach (Polygon x in _smoothPolys)
                    Renderer.DrawPolygon(x, Color.Red);
        }

        public void InActivate()
        {
            CancelSmoothing();
        }

        public void KeyDown(KeyEventArgs key)
        {
            if (Smoothing)
            {
                switch (key.KeyCode)
                {
                    case Constants.Increase:
                        if (!_unsmooth)
                        {
                            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                            {
                                if (_smoothVertexOffset < 0.95)
                                    _smoothVertexOffset += 0.05;
                            }
                            else
                            {
                                if (_smoothSteps < 20)
                                    _smoothSteps++;
                            }
                        }
                        else
                        {
                            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                            {
                                if (_unsmoothAngle < 180)
                                    _unsmoothAngle += 2;
                            }
                            else
                            {
                                if (_unsmoothLength < 20)
                                    _unsmoothLength += 0.1;
                            }
                        }
                        break;
                    case Constants.Decrease:
                        if (!_unsmooth)
                        {
                            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                            {
                                if (_smoothVertexOffset > 0.55)
                                    _smoothVertexOffset -= 0.05;
                            }
                            else
                            {
                                if (_smoothSteps > 2)
                                    _smoothSteps--;
                            }
                        }
                        else
                        {
                            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                            {
                                if (_unsmoothAngle > 0)
                                    _unsmoothAngle -= 2;
                            }
                            else
                            {
                                if (_unsmoothLength > 0.1)
                                    _unsmoothLength -= 0.1;
                            }
                        }
                        break;
                }
                UpdateHelp();
                UpdatePolygonSmooth();
            }
            else
            {
                switch (key.KeyCode)
                {
                    case Keys.Space:
                        if (!Smoothing)
                        {
                            Smoothing = true;
                            _smoothAll = true;
                            _unsmooth = Keyboard.IsKeyDown(Key.LeftCtrl);
                            UpdateHelp();
                            UpdatePolygonSmooth();
                        }
                        break;
                }
            }
        }

        public void MouseDown(MouseEventArgs mouseData)
        {
            int nearestVertexIndex = GetNearestVertexIndex(CurrentPos);
            switch (mouseData.Button)
            {
                case MouseButtons.Left:
                    if (Smoothing)
                    {
                        if (_smoothAll)
                        {
                            for (int i = Lev.Polygons.Count - 1; i >= 0; i--)
                            {
                                Polygon x = Lev.Polygons[i];
                                if (IsSmoothable(x))
                                    Lev.Polygons.RemoveAt(i);
                            }
                        }
                        else
                            Lev.Polygons.Remove(_currentPolygon);
                        Lev.Polygons.AddRange(_smoothPolys);
                        Smoothing = false;
                        LevEditor.Modified = true;
                        LevEditor.UpdateSelectionInfo();
                        foreach (Polygon x in _smoothPolys)
                            x.UpdateDecomposition();
                        Renderer.RedrawScene();
                    }
                    else if (nearestVertexIndex >= -1)
                    {
                        Smoothing = true;
                        _smoothAll = false;
                        _currentPolygon = NearestPolygon;
                        ResetHighlight();
                        _unsmooth = Keyboard.IsKeyDown(Key.LeftCtrl);
                        UpdateHelp();
                        UpdatePolygonSmooth();
                    }
                    break;
                case MouseButtons.Right:
                    CancelSmoothing();
                    break;
            }
            UpdateHelp();
        }

        public void MouseMove(Vector p)
        {
            CurrentPos = p;
            if (!Smoothing)
            {
                int nearest = GetNearestVertexIndex(p);
                ResetHighlight();
                if (nearest >= -1)
                {
                    NearestPolygon.Mark = PolygonMark.Highlight;
                    ChangeCursorToHand();
                }
                else
                    ChangeToDefaultCursor();
                Renderer.RedrawScene();
            }
            else
                ChangeToDefaultCursor();
        }

        public void MouseOutOfEditor()
        {
            ResetHighlight();
            Renderer.RedrawScene();
        }

        public void MouseUp(MouseEventArgs mouseData)
        {
        }

        public void UpdateHelp()
        {
            if (Smoothing)
            {
                LevEditor.InfoLabel.Text = "Left click: apply; right click: cancel; (Ctrl) + +/-: adjust parameters.";
                if (!_unsmooth)
                    LevEditor.InfoLabel.Text += " (" + _smoothSteps + ", " + _smoothVertexOffset.ToString("F2") + ")";
                else
                    LevEditor.InfoLabel.Text += " (" + _unsmoothLength.ToString("F2") + ", " +
                                                _unsmoothAngle.ToString("F2") + ")";
            }
            else
                LevEditor.InfoLabel.Text = "Click a polygon or press Space to smooth selected. Hold Ctrl to unsmooth.";
        }

        private static bool IsSmoothable(Polygon p)
        {
            for (int i = 0; i < p.Count; i++)
                if (p[i].Mark == Geometry.VectorMark.Selected && p[i + 1].Mark == Geometry.VectorMark.Selected &&
                    p[i + 2].Mark == Geometry.VectorMark.Selected)
                    return true;
            return false;
        }

        private void CancelSmoothing()
        {
            if (!Smoothing) return;
            Smoothing = false;
            ResetHighlight();
            Renderer.RedrawScene();
        }

        private void UpdatePolygonSmooth()
        {
            _smoothPolys = new List<Polygon>();
            if (_smoothAll)
            {
                foreach (Polygon x in Lev.Polygons.Where(IsSmoothable))
                {
                    _smoothPolys.Add(!_unsmooth
                                         ? x.Smoothen(_smoothSteps, _smoothVertexOffset, true)
                                         : x.Unsmoothen(_unsmoothAngle, _unsmoothLength, true));
                }
            }
            else
            {
                _smoothPolys.Add(_unsmooth
                                     ? _currentPolygon.Unsmoothen(_unsmoothAngle, _unsmoothLength, false)
                                     : _currentPolygon.Smoothen(_smoothSteps, _smoothVertexOffset, false));
            }
            Renderer.RedrawScene();
        }
    }
}