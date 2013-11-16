using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Input;
using Elmanager.Forms;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace Elmanager.EditorTools
{
    internal class PipeTool : ToolBase, IEditorTool
    {
        private const double AppleDistanceStep = 0.25;

        private int _appleAmount = 20;
        private double _appleDistance = 3.0;
        private List<Level.Object> _apples;
        private bool _creatingPipe;
        private Polygon _pipe;
        private PipeMode _pipeMode = PipeMode.NoApples;
        private double _pipeRadius = 1.0;
        private Polygon _pipeline;

        public enum PipeMode
        {
            NoApples = 0,
            ApplesAmount = 1,
            ApplesDistance = 2
        }

        internal PipeTool(LevelEditor editor)
            : base(editor)
        {
        }

        private bool CreatingPipe
        {
            get { return _creatingPipe; }
            set
            {
                _creatingPipe = value;
                _Busy = value;
            }
        }

        public void Activate()
        {
            UpdateHelp();
            Renderer.AdditionalPolys = ExtraPolys;
            Renderer.RedrawScene();
        }

        public void ExtraRendering()
        {
            if (CreatingPipe)
            {
                Renderer.DrawLineStrip(_pipeline, Color.Blue);
                if (Global.AppSettings.LevelEditor.RenderingSettings.ShowGroundEdges)
                    Renderer.DrawPolygon(_pipe, Global.AppSettings.LevelEditor.RenderingSettings.GroundEdgeColor);
                foreach (Level.Object x in _apples)
                {
                    if (Global.AppSettings.LevelEditor.RenderingSettings.ShowObjectFrames)
                        Renderer.DrawCircle(x.Position, ElmaRenderer.ObjectRadius,
                                            Global.AppSettings.LevelEditor.RenderingSettings.AppleColor);
                    if (Global.AppSettings.LevelEditor.RenderingSettings.ShowObjects)
                        Renderer.DrawApple(x.Position);
                }
            }
        }

        public void InActivate()
        {
            CreatingPipe = false;
            Renderer.AdditionalPolys = null;
        }

        public void KeyDown(KeyEventArgs key)
        {
            switch (key.KeyCode)
            {
                case Constants.Increase:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        switch (_pipeMode)
                        {
                            case PipeMode.NoApples:
                                break;

                            case PipeMode.ApplesDistance:
                                _appleDistance += AppleDistanceStep;
                                break;
                            case PipeMode.ApplesAmount:
                                _appleAmount++;
                                break;
                        }
                    }
                    else
                        _pipeRadius += 0.05;
                    break;
                case Constants.Decrease:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        switch (_pipeMode)
                        {
                            case PipeMode.NoApples:
                                break;

                            case PipeMode.ApplesDistance:
                                if (_appleDistance > AppleDistanceStep)
                                    _appleDistance -= AppleDistanceStep;
                                break;
                            case PipeMode.ApplesAmount:
                                if (_appleAmount > 1)
                                    _appleAmount--;
                                break;
                        }
                    }
                    else if (_pipeRadius > 0.05)
                        _pipeRadius -= 0.05;
                    break;
                case Keys.Space:
                    switch (_pipeMode)
                    {
                        case PipeMode.NoApples:
                            _pipeMode = PipeMode.ApplesDistance;
                            break;
                        case PipeMode.ApplesDistance:
                            _pipeMode = PipeMode.ApplesAmount;
                            break;
                        case PipeMode.ApplesAmount:
                            _pipeMode = PipeMode.NoApples;
                            break;
                    }
                    break;
            }
            if (CreatingPipe)
            {
                UpdatePipe(_pipeline);
                Renderer.RedrawScene();
            }
            UpdateHelp();
        }

        public void MouseDown(MouseEventArgs mouseData)
        {
            switch (mouseData.Button)
            {
                case MouseButtons.Left:
                    if (CreatingPipe)
                        _pipeline.Add(CurrentPos);
                    else
                    {
                        CreatingPipe = true;
                        _pipeline = new Polygon();
                        _apples = new List<Level.Object>();
                        _pipeline.Add(CurrentPos);
                        _pipeline.Add(CurrentPos);
                    }
                    break;
                case MouseButtons.Right:
                    if (CreatingPipe)
                    {
                        _pipeline.RemoveLastVertex();
                        UpdatePipe(_pipeline);
                        CreatingPipe = false;
                        if (_pipeline.Count > 1)
                        {
                            Lev.Polygons.Add(_pipe);
                            Lev.Objects.AddRange(_apples);
                            LevEditor.Modified = true;
                        }
                        Renderer.RedrawScene();
                    }
                    break;
            }
            UpdateHelp();
        }

        public void MouseMove(Vector p)
        {
            CurrentPos = p;
            AdjustForGrid(CurrentPos);
            if (CreatingPipe)
            {
                _pipeline.Vertices[_pipeline.Vertices.Count - 1] = CurrentPos;
                UpdatePipe(_pipeline);
                Renderer.RedrawScene();
            }
        }

        public void MouseOutOfEditor()
        {
        }

        public void MouseUp(MouseEventArgs mouseData)
        {
        }

        public void UpdateHelp()
        {
            LevEditor.InfoLabel.Text = "Space: change mode - ";
            switch (_pipeMode)
            {
                case PipeMode.NoApples:
                    LevEditor.InfoLabel.Text += "Mode: No apples - Pipe radius: " + _pipeRadius.ToString("F2");
                    break;
                case PipeMode.ApplesDistance:
                    LevEditor.InfoLabel.Text += "Mode: Apples (distance: " + _appleDistance.ToString("F2") +
                                                ") - Pipe radius: " + _pipeRadius.ToString("F2");
                    break;
                case PipeMode.ApplesAmount:
                    LevEditor.InfoLabel.Text += "Mode: " + _appleAmount + " apples - Pipe radius: " +
                                                _pipeRadius.ToString("F2");
                    break;
            }
        }

        private List<Level.Object> CalculateApples(double distance)
        {
            List<Level.Object> apples = new List<Level.Object>();
            double currentDistanceToApple = distance;
            for (int i = 0; i <= _pipeline.Count - 2; i++)
            {
                Vector z = _pipeline[i + 1] - _pipeline[i];
                Vector zUnit = z.Unit();
                double currVectorLength = z.Length;
                if (currentDistanceToApple < currVectorLength)
                {
                    double currVectorTrip = currentDistanceToApple;
                    while (!(currVectorTrip > currVectorLength))
                    {
                        apples.Add(new Level.Object(_pipeline[i] + zUnit * currVectorTrip, Level.ObjectType.Apple,
                                                         Level.AppleTypes.Normal, 0));
                        currVectorTrip += distance;
                    }
                    currentDistanceToApple = currVectorTrip - currVectorLength;
                }
                else
                    currentDistanceToApple -= currVectorLength;
            }
            return apples;
        }

        private void ExtraPolys()
        {
            if (CreatingPipe)
                Renderer.DrawFilledTriangles(_pipe.Decomposition);
        }

        private void UpdatePipe(Polygon pipeLine)
        {
            Polygon p = new Polygon();
            if (pipeLine.Count < 2)
                return;
            double angle = (pipeLine[1] - pipeLine[0]).Angle;
            p.Add(pipeLine[0] + new Vector(angle + 90) * _pipeRadius);
            p.Add(pipeLine[0] - new Vector(angle + 90) * _pipeRadius);
            for (int i = 1; i <= pipeLine.Count - 2; i++)
            {
                angle = (pipeLine[i + 1] - pipeLine[i]).Angle;
                Vector point = Geometry.FindPoint(pipeLine[i - 1], pipeLine[i], pipeLine[i + 1], -_pipeRadius);
                p.Add(point);
            }
            p.Add(pipeLine.GetLastVertex() - new Vector(angle + 90) * _pipeRadius);
            p.Add(pipeLine.GetLastVertex() + new Vector(angle + 90) * _pipeRadius);
            for (int i = pipeLine.Count - 2; i >= 1; i--)
            {
                Vector point = Geometry.FindPoint(pipeLine[i - 1], pipeLine[i], pipeLine[i + 1], _pipeRadius);
                p.Add(point);
            }
            _pipe = p;
            _pipe.UpdateDecomposition();
            switch (_pipeMode)
            {
                case PipeMode.ApplesDistance:
                    _apples = CalculateApples(_appleDistance);
                    break;
                case PipeMode.ApplesAmount:
                    double pipelineLength = 0.0;
                    for (int i = 0; i <= pipeLine.Count - 2; i++)
                        pipelineLength += (pipeLine[i + 1] - pipeLine[i]).Length;
                    pipelineLength += 0.1;
                    _apples = CalculateApples(pipelineLength / (_appleAmount + 1));
                    break;
                case PipeMode.NoApples:
                    _apples = new List<Level.Object>();
                    break;
            }
        }
    }
}