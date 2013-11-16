using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Elmanager.Forms;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace Elmanager.EditorTools
{
    internal class SelectionTool : ToolBase, IEditorTool
    {
        private bool _anythingMoved;
        private Vector _lockCenter; //for lock lines -mode
        private Vector _lockNext; //for lock lines -mode
        private Vector _lockPrev; //for lock lines -mode
        private bool _lockingLines;
        private Vector _moveStartPosition;
        private bool _moving;
        private bool _rectSelecting;
        private Vector _selectionStartPoint;

        internal SelectionTool(LevelEditor editor)
            : base(editor)
        {
        }

        private bool Moving
        {
            get { return _moving; }
            set
            {
                _moving = value;
                UpdateBusy();
            }
        }

        private bool RectSelecting
        {
            get { return _rectSelecting; }
            set
            {
                _rectSelecting = value;
                UpdateBusy();
            }
        }

        public void Activate()
        {
            UpdateHelp();
            Renderer.RedrawScene();
        }

        public void ExtraRendering()
        {
            if (RectSelecting)
                Renderer.DrawRectangle(_selectionStartPoint, CurrentPos, Color.Blue);
        }

        public void InActivate()
        {
            Moving = false;
            RectSelecting = false;
        }

        public void KeyDown(KeyEventArgs key)
        {
            if (key.KeyCode == Keys.Space)
                LevEditor.TransformMenuItemClick();
        }

        public void MouseDown(MouseEventArgs mouseData)
        {
            Vector p = CurrentPos;
            _anythingMoved = false;
            int nearestVertexIndex = GetNearestVertexIndex(p);
            int nearestObjectIndex = GetNearestObjectIndex(p);
            int nearestPictureIndex = GetNearestPictureIndex(p);
            switch (mouseData.Button)
            {
                case MouseButtons.Left:
                    if (nearestVertexIndex >= 0)
                    {
                        HandleMark(NearestPolygon[nearestVertexIndex]);
                        if (Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            _lockCenter = NearestPolygon[nearestVertexIndex];
                            _lockPrev = NearestPolygon[nearestVertexIndex - 1];
                            _lockNext = NearestPolygon[nearestVertexIndex + 1];
                            _lockingLines = true;
                            _moveStartPosition = _lockCenter;
                        }
                    }
                    else if (nearestVertexIndex == -1)
                    {
                        int nearestSegmentIndex = NearestPolygon.GetNearestSegmentIndex(p);
                        AdjustForGrid(p);
                        _moveStartPosition = p;
                        Moving = true;
                        if (Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            MarkAllAs(Geometry.VectorMark.None);
                            p.Mark = Geometry.VectorMark.Selected;
                            NearestPolygon.Insert(nearestSegmentIndex + 1, p);
                        }
                        else
                        {
                            if (
                                !(NearestPolygon[nearestSegmentIndex].Mark == Geometry.VectorMark.Selected &&
                                  NearestPolygon[nearestSegmentIndex + 1].Mark == Geometry.VectorMark.Selected))
                            {
                                if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                                    MarkAllAs(Geometry.VectorMark.None);
                                NearestPolygon.MarkVectorsAs(Geometry.VectorMark.Selected);
                            }
                        }
                        Renderer.RedrawScene();
                    }
                    else if (nearestObjectIndex >= 0)
                        HandleMark(Lev.Objects[nearestObjectIndex].Position);
                    else if (nearestPictureIndex >= 0)
                        HandleMark(Lev.Pictures[nearestPictureIndex].Position);
                    else
                    {
                        if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                            MarkAllAs(Geometry.VectorMark.None);
                        _selectionStartPoint = p;
                        RectSelecting = true;
                        Renderer.RedrawScene();
                    }
                    LevEditor.UpdateSelectionInfo();
                    break;
                case MouseButtons.Right:
                    break;

                case MouseButtons.Middle:
                    break;
            }
        }

        public void MouseMove(Vector p)
        {
            CurrentPos = p;
            if (Moving)
            {
                AdjustForGrid(p);
                if (_lockingLines)
                {
                    p = Geometry.OrthogonalProjection(_lockCenter, Geometry.DistanceFromLine(_lockCenter, _lockNext, p) <
                                                                   Geometry.DistanceFromLine(_lockCenter, _lockPrev, p) ? _lockNext : _lockPrev, p);
                }
                Vector delta = p - _moveStartPosition;
                if (delta.Length > 0)
                    _anythingMoved = true;
                Vector.MarkDefault = Geometry.VectorMark.Selected;
                foreach (Polygon x in Lev.Polygons)
                {
                    bool anythingMoved = false;
                    for (int i = 0; i < x.Vertices.Count; i++)
                    {
                        if (x.Vertices[i].Mark != Geometry.VectorMark.Selected) continue;
                        x.Vertices[i] += delta;
                        anythingMoved = true;
                    }
                    if (anythingMoved)
                        x.UpdateDecomposition();
                }
                foreach (Level.Object x in Lev.Objects.Where(x => x.Position.Mark == Geometry.VectorMark.Selected))
                {
                    x.Position += delta;
                }
                foreach (Level.Picture z in Lev.Pictures.Where(z => z.Position.Mark == Geometry.VectorMark.Selected))
                {
                    z.Position += delta;
                }
                Vector.MarkDefault = 0;
                _moveStartPosition = p;
            }
            else if (!Busy)
            {
                ResetHighlight();
                int nearestVertex = GetNearestVertexIndex(p);
                int nearestObject = GetNearestObjectIndex(p);
                int nearestTextureIndex = GetNearestPictureIndex(p);
                if (nearestVertex == -1)
                {
                    ChangeCursorToHand();
                    NearestPolygon.Mark = PolygonMark.Highlight;
                    LevEditor.HighlightLabel.Text = NearestPolygon.IsGrass ? "Grass" : "Ground";
                    LevEditor.HighlightLabel.Text += " polygon, " + NearestPolygon.Count + " vertices";
                }
                else if (nearestVertex >= 0)
                {
                    ChangeCursorToHand();
                    Vector b = NearestPolygon.Vertices[nearestVertex];
                    if (b.Mark == Geometry.VectorMark.None)
                        b.Mark = Geometry.VectorMark.Highlight;
                    LevEditor.HighlightLabel.Text = NearestPolygon.IsGrass ? "Grass" : "Ground";
                    LevEditor.HighlightLabel.Text += " polygon, vertex " + (nearestVertex + 1) + " of " + NearestPolygon.Count + " vertices";
                }
                else if (nearestObject >= 0)
                {
                    ChangeCursorToHand();
                    if (Lev.Objects[nearestObject].Position.Mark == Geometry.VectorMark.None)
                        Lev.Objects[nearestObject].Position.Mark = Geometry.VectorMark.Highlight;
                    ShowObjectInfo(nearestObject);
                }
                else if (nearestTextureIndex >= 0)
                {
                    ChangeCursorToHand();
                    if (Lev.Pictures[nearestTextureIndex].Position.Mark == Geometry.VectorMark.None)
                        Lev.Pictures[nearestTextureIndex].Position.Mark = Geometry.VectorMark.Highlight;
                    ShowTextureInfo(nearestTextureIndex);
                }
                else
                {
                    ChangeToDefaultCursor();
                    LevEditor.HighlightLabel.Text = "";
                }
            }
            Renderer.RedrawScene();
        }

        public void MouseOutOfEditor()
        {
            ResetHighlight();
            Renderer.RedrawScene();
        }

        public void MouseUp(MouseEventArgs mouseData)
        {
            if (RectSelecting)
            {
                RectSelecting = false;
                double selectionxMin = Math.Min(CurrentPos.X, _selectionStartPoint.X);
                double selectionxMax = Math.Max(CurrentPos.X, _selectionStartPoint.X);
                double selectionyMax = Math.Max(CurrentPos.Y, _selectionStartPoint.Y);
                double selectionyMin = Math.Min(CurrentPos.Y, _selectionStartPoint.Y);
                foreach (Polygon x in Lev.Polygons)
                {
                    if ((x.IsGrass && LevEditor.GrassFilter) || (!x.IsGrass && LevEditor.GroundFilter))
                    {
                        foreach (Vector t in x.Vertices)
                            MarkSelectedInArea(t, selectionxMin, selectionxMax, selectionyMin, selectionyMax);
                    }
                }
                foreach (Level.Object t in Lev.Objects)
                {
                    Level.ObjectType type = t.Type;
                    if (type == Level.ObjectType.Start || (type == Level.ObjectType.Apple && LevEditor.AppleFilter) ||
                        (type == Level.ObjectType.Killer && LevEditor.KillerFilter) ||
                        (type == Level.ObjectType.Flower && LevEditor.FlowerFilter))
                        MarkSelectedInArea(t.Position, selectionxMin, selectionxMax, selectionyMin,
                                           selectionyMax);
                }
                foreach (Level.Picture z in Lev.Pictures)
                {
                    if ((z.IsPicture && LevEditor.PictureFilter) || (!z.IsPicture && LevEditor.TextureFilter))
                        MarkSelectedInArea(z.Position, selectionxMin, selectionxMax, selectionyMin, selectionyMax);
                }
                LevEditor.UpdateSelectionInfo();
            }
            else if (Moving)
            {
                Moving = false;
                _lockingLines = false;
                if (_anythingMoved)
                    LevEditor.Modified = true;
            }
            Renderer.RedrawScene();
        }

        public void UpdateHelp()
        {
            LevEditor.InfoLabel.Text = "Left mouse button: select level elements.";
        }

        private static void MarkSelectedInArea(Vector z, double selectionxMin, double selectionxMax,
                                               double selectionyMin, double selectionyMax)
        {
            if (z.X < selectionxMax && z.X > selectionxMin && z.Y < selectionyMax && z.Y > selectionyMin)
            {
                switch (z.Mark)
                {
                    case Geometry.VectorMark.None:
                        z.Mark = Geometry.VectorMark.Selected;
                        break;
                    case Geometry.VectorMark.Selected:
                        z.Mark = Geometry.VectorMark.None;
                        break;
                }
            }
            else if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                z.Mark = Geometry.VectorMark.None;
        }

        private void HandleMark(Vector v)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (v.Mark != Geometry.VectorMark.Selected)
                    MarkAllAs(Geometry.VectorMark.None);
                v.Mark = Geometry.VectorMark.Selected;
                Moving = true;
            }
            else
            {
                v.Mark = v.Mark != Geometry.VectorMark.Selected
                             ? Geometry.VectorMark.Selected
                             : Geometry.VectorMark.None;
                if (v.Mark == Geometry.VectorMark.Selected)
                    Moving = true;
            }
            AdjustForGrid(CurrentPos);
            _moveStartPosition = CurrentPos;
            Renderer.RedrawScene();
        }

        private void ShowObjectInfo(int currentObjectIndex)
        {
            Level.Object currObj = Lev.Objects[currentObjectIndex];
            switch (currObj.Type)
            {
                case Level.ObjectType.Apple:
                    switch (currObj.AppleType)
                    {
                        case Level.AppleTypes.Normal:
                            LevEditor.HighlightLabel.Text = "Apple: Normal";
                            break;
                        case Level.AppleTypes.GravityUp:
                            LevEditor.HighlightLabel.Text = "Apple: Gravity up";
                            break;
                        case Level.AppleTypes.GravityDown:
                            LevEditor.HighlightLabel.Text = "Apple: Gravity down";
                            break;
                        case Level.AppleTypes.GravityLeft:
                            LevEditor.HighlightLabel.Text = "Apple: Gravity left";
                            break;
                        case Level.AppleTypes.GravityRight:
                            LevEditor.HighlightLabel.Text = "Apple: Gravity right";
                            break;
                    }
                    break;
                case Level.ObjectType.Killer:
                    LevEditor.HighlightLabel.Text = "Killer";
                    break;
                case Level.ObjectType.Flower:
                    LevEditor.HighlightLabel.Text = "Flower";
                    break;
                case Level.ObjectType.Start:
                    LevEditor.HighlightLabel.Text = "Start";
                    break;
            }
        }

        private void ShowTextureInfo(int index)
        {
            Level.Picture picture = Lev.Pictures[index];
            if (picture.IsPicture)
                LevEditor.HighlightLabel.Text = "Picture: " + picture.Name +
                                                ", distance: " +
                                                picture.Distance + ", clipping: " + picture.Clipping;
            else
            {
                LevEditor.HighlightLabel.Text = "Texture: " + picture.TextureName + ", mask: " + picture.Name +
                                                ", distance: " +
                                                picture.Distance + ", clipping: " + picture.Clipping;
            }
        }

        private void UpdateBusy()
        {
            _Busy = RectSelecting || Moving;
        }
    }
}