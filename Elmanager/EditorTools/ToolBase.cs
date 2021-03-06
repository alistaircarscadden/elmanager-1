using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Elmanager.Forms;

namespace Elmanager.EditorTools
{
    internal abstract class ToolBase : IEditorToolBase
    {
        protected Vector CurrentPos;
        private Control _editorControl;
        protected LevelEditor LevEditor;
        internal Polygon NearestPolygon;
        protected ElmaRenderer Renderer;
        private Cursor _hand;

        protected ToolBase(LevelEditor editor)
        {
            LevEditor = editor;
            Renderer = editor.Renderer;
            _editorControl = editor.EditorControl;
        }

        protected Level Lev => LevEditor.Lev;

        public abstract bool Busy { get; }

        internal int GetNearestObjectIndex(Vector p)
        {
            int index = -1;
            double smallest = double.MaxValue;
            var appleFilter = LevEditor.EffectiveAppleFilter;
            var killerFilter = LevEditor.EffectiveKillerFilter;
            var flowerFilter = LevEditor.EffectiveFlowerFilter;
            for (int i = 0; i < Lev.Objects.Count; i++)
            {
                Vector z = Lev.Objects[i].Position;
                ObjectType type = Lev.Objects[i].Type;
                if (((type == ObjectType.Apple && appleFilter) ||
                     (type == ObjectType.Killer && killerFilter) ||
                     (type == ObjectType.Flower && flowerFilter) || type == ObjectType.Start) &&
                    (p - z).LengthSquared < smallest)
                {
                    smallest = (p - z).LengthSquared;
                    index = i;
                }

                if (Lev.Objects[i].Type != ObjectType.Start) continue;
                Vector v1 = p - new Vector(z.X + Level.RightWheelDifferenceFromLeftWheelX, z.Y);
                Vector v2 = p -
                            new Vector(z.X + Level.HeadDifferenceFromLeftWheelX,
                                z.Y + Level.HeadDifferenceFromLeftWheelY);
                if (v1.LengthSquared < smallest)
                {
                    smallest = v1.LengthSquared;
                    index = i;
                }

                if (v2.LengthSquared >= smallest) continue;
                smallest = v2.LengthSquared;
                index = i;
            }

            if (Math.Sqrt(smallest) <
                Math.Max(Renderer.ZoomLevel * Global.AppSettings.LevelEditor.CaptureRadius, ElmaRenderer.ObjectRadius))
                return index;
            return -1;
        }

        internal int GetNearestPictureIndex(Vector p)
        {
            var pictureFilter = LevEditor.EffectivePictureFilter;
            var textureFilter = LevEditor.EffectiveTextureFilter;
            int found = -1;
            if (Global.AppSettings.LevelEditor.CapturePicturesAndTexturesFromBordersOnly)
            {
                var limit = Renderer.ZoomLevel * Global.AppSettings.LevelEditor.CaptureRadius;
                for (int j = 0; j < Lev.Pictures.Count; j++)
                {
                    Picture z = Lev.Pictures[j];
                    if ((z.IsPicture && pictureFilter) || (!z.IsPicture && textureFilter))
                    {
                        if (GeometryUtils.DistanceFromSegment(z.Position.X, z.Position.Y, z.Position.X + z.Width,
                                z.Position.Y, p.X, p.Y) < limit
                            || GeometryUtils.DistanceFromSegment(z.Position.X, z.Position.Y, z.Position.X,
                                z.Position.Y + z.Height, p.X, p.Y) < limit
                            || GeometryUtils.DistanceFromSegment(z.Position.X + z.Width, z.Position.Y,
                                z.Position.X + z.Width, z.Position.Y + z.Height, p.X, p.Y) < limit
                            || GeometryUtils.DistanceFromSegment(z.Position.X, z.Position.Y + z.Height,
                                z.Position.X + z.Width, z.Position.Y + z.Height, p.X, p.Y) < limit)
                        {
                            found = j;
                            if (z.Position.Mark == VectorMark.Selected)
                            {
                                return found;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < Lev.Pictures.Count; j++)
                {
                    Picture z = Lev.Pictures[j];
                    if ((z.IsPicture && pictureFilter) || (!z.IsPicture && textureFilter))
                    {
                        if (p.X > z.Position.X && p.X < z.Position.X + z.Width && p.Y > z.Position.Y &&
                            p.Y < z.Position.Y + z.Height)
                        {
                            found = j;
                            if (z.Position.Mark == VectorMark.Selected)
                            {
                                return found;
                            }
                        }
                    }
                }
            }

            return found;
        }

        internal int GetNearestVertexIndex(Vector p)
        {
            bool nearestVertexFound = false;
            bool nearestSelectedVertexFound = false;
            int nearestIndex = -1;
            int nearestSelectedIndex = -1;
            double smallestDistance = double.MaxValue;
            double smallestSelectedDistance = double.MaxValue;
            Polygon nearestSelectedPolygon = null;
            var grassFilter = LevEditor.EffectiveGrassFilter;
            var groundFilter = LevEditor.EffectiveGroundFilter;
            foreach (Polygon poly in Lev.Polygons)
            {
                double currentDistance;
                if (((poly.IsGrass && grassFilter) || (!poly.IsGrass && groundFilter)) &&
                    (currentDistance = poly.GetNearestVertexDistance(p)) <
                    Renderer.ZoomLevel * Global.AppSettings.LevelEditor.CaptureRadius)
                {
                    int currentIndex = poly.GetNearestVertexIndex(p);
                    if (currentDistance < smallestDistance)
                    {
                        NearestPolygon = poly;
                        nearestIndex = currentIndex;
                        nearestVertexFound = true;
                        smallestDistance = currentDistance;
                    }

                    //prefer selected vectors
                    if (poly[currentIndex].Mark == VectorMark.Selected &&
                        currentDistance < smallestSelectedDistance)
                    {
                        nearestSelectedPolygon = poly;
                        nearestSelectedIndex = currentIndex;
                        nearestSelectedVertexFound = true;
                        smallestSelectedDistance = currentDistance;
                    }
                }
            }

            if (nearestSelectedVertexFound)
            {
                NearestPolygon = nearestSelectedPolygon;
                return nearestSelectedIndex;
            }

            if (nearestVertexFound)
            {
                return nearestIndex;
            }

            smallestDistance = double.MaxValue;
            bool nearestSegmentFound = false;
            foreach (Polygon x in Lev.Polygons)
            {
                double currentDistance;
                if (((x.IsGrass && grassFilter) || (!x.IsGrass && groundFilter)) &&
                    (currentDistance = x.DistanceFromPoint(p)) <
                    Renderer.ZoomLevel * Global.AppSettings.LevelEditor.CaptureRadius)
                {
                    if (currentDistance < smallestDistance)
                    {
                        nearestSegmentFound = true;
                        smallestDistance = currentDistance;
                        NearestPolygon = x;
                    }
                }
            }

            if (nearestSegmentFound)
            {
                return -1;
            }

            return -2; //Indicates that mouse wasn't near any edge nor vertex
        }

        protected void ResetHighlight()
        {
            if (!Global.AppSettings.LevelEditor.UseHighlight)
                return;
            foreach (Polygon x in Lev.Polygons)
            {
                if (x.Mark == PolygonMark.Highlight)
                    x.Mark = PolygonMark.None;
                foreach (Vector z in x.Vertices)
                    if (z.Mark != VectorMark.Selected)
                        z.Mark = VectorMark.None;
            }

            foreach (LevObject x in Lev.Objects)
                if (x.Position.Mark == VectorMark.Highlight)
                    x.Position.Mark = VectorMark.None;
            foreach (Picture z in Lev.Pictures)
                if (z.Position.Mark == VectorMark.Highlight)
                    z.Position.Mark = VectorMark.None;
        }

        protected void ResetPolygonMarks()
        {
            foreach (Polygon x in Lev.Polygons)
                x.Mark = PolygonMark.None;
        }

        protected void AdjustForGrid(Vector p)
        {
            if (!Global.AppSettings.LevelEditor.RenderingSettings.ShowGrid ||
                !Global.AppSettings.LevelEditor.SnapToGrid)
                return;
            var gridSize = Global.AppSettings.LevelEditor.RenderingSettings.GridSize;
            double x = (p.X + Renderer.GridOffset.X) % gridSize;
            if (Math.Abs(x) > gridSize / 2)
            {
                x -= gridSize * Math.Sign(x);
            }

            p.X -= x;
            double y = (p.Y - Renderer.GridOffset.Y) % gridSize;
            if (Math.Abs(y) > gridSize / 2)
            {
                y -= gridSize * Math.Sign(y);
            }

            p.Y -= y;
        }

        private Cursor Hand => _hand ?? (_hand = CreateHandCursor());

        private Cursor CreateHandCursor()
        {
            var ctor = typeof(Cursor).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                new[] {typeof(int), typeof(int)}, null);
            const int idcHand = 32649;
            return (Cursor) ctor.Invoke(new object[] {idcHand, 0});
        }

        protected void ChangeCursorToHand()
        {
            if (Global.AppSettings.LevelEditor.UseHighlight)
                _editorControl.Cursor = Hand;
        }

        protected void ChangeToDefaultCursorIfHand()
        {
            if (_editorControl.Cursor == Hand)
                LevEditor.ChangeToDefaultCursor();
        }

        protected void MarkAllAs(VectorMark mark)
        {
            foreach (Vector t in Lev.Polygons.SelectMany(x => x.Vertices))
            {
                t.Mark = mark;
            }

            foreach (LevObject x in Lev.Objects)
                x.Position.Mark = mark;
            foreach (Picture x in Lev.Pictures)
                x.Position.Mark = mark;
        }
    }
}