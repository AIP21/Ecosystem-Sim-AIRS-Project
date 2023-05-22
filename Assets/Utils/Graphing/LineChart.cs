using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
using Utilities;

namespace Graphing
{
    class Line : VisualElement
    {
        private VisualElement _lines;
        private VisualElement _labels;

        private Label label;
        // private List<Label> _labelItems;
        private List<VisualElement> _lineItems;

        private List<float> values = new List<float>();
        private StyleColor color;

        int maxPoints = 100;

        public float min, max;

        public bool ownMinMax;

        public Line(VisualElement background, int maxPoints, bool ownMinMax, Color color)
        {
            this.maxPoints = maxPoints;
            this.color = new StyleColor(color);
            this.ownMinMax = ownMinMax;

            _lineItems = new List<VisualElement>();
            _lines = new VisualElement()
            {
                name = "lines",
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    width = new StyleLength(new Length(100, LengthUnit.Percent)),
                    height = new StyleLength(new Length(100, LengthUnit.Percent)),
                }
            };
            background.Add(_lines);

            // _labelItems = new List<Label>();
            // _labels = new VisualElement()
            // {
            //     name = "labels",
            //     style =
            //     {
            //         position = new StyleEnum<Position>(Position.Absolute),
            //         width = new StyleLength(new Length(100, LengthUnit.Percent)),
            //         height = new StyleLength(new Length(100, LengthUnit.Percent)),
            //     }
            // };
            // background.Add(_labels);
        }

        #region Update Graph
        public void AddValue(float value)
        {
            Utils.AddToAverageList<float>(values, value, maxPoints);

            min = float.MaxValue;
            max = float.MinValue;

            foreach (float val in values)
            {
                if (val < min)
                    min = val;

                if (val > max)
                    max = val;
            }
        }

        public void UpdateLine(float width, float height)
        {
            // if (ownMinMax)
            // {
            //     min = float.MaxValue;
            //     max = float.MinValue;

            //     foreach (float val in values)
            //     {
            //         if (val < min)
            //             min = val;

            //         if (val > max)
            //             max = val;
            //     }
            // }

            for (int i = 0; i < values.Count - 1; i++)
            {
                Vector2 point1 = GetLinePoint(i, values[i], min, max, width, height);
                Vector2 point2 = GetLinePoint(i + 1, values[i + 1], min, max, width, height);

                if (i > _lineItems.Count - 1)
                    SetUpLine(i, point1, point2);
                else
                {
                    VisualElement line = _lineItems[i];
                    SetLinePosition(ref line, point1, point2);
                }
            }

            // Draw a label at the end of the graph
            // if (label == null)
            // {
            //     label = new Label
            //     {
            //         name = "graph-label-" + 0,
            //         style =
            //         {
            //             position = new StyleEnum<Position>(Position.Absolute),
            //             bottom = new StyleLength(new Length(0, LengthUnit.Pixel)),
            //             left = new StyleLength(new Length(0, LengthUnit.Pixel)),
            //             color = this.color,
            //             fontSize = new StyleLength(new Length(10, LengthUnit.Pixel)),
            //         }
            //     };
            //     _labels.Add(label);
            // }
            // else if (values.Count != 0)
            // {
            //     float val = values[values.Count - 1];
            //     Vector2 point = new Vector2(width - 20, Remap(val, min, max, 10, height));

            //     SetLabelPosition(ref label, point);
            //     label.text = val.ToString("0.00");
            // }
        }

        // remaps the point from the given min and max to fit within the height and width of the graph
        private Vector2 GetLinePoint(int index, float value, float min, float max, float w, float h)
        {
            float width = w - 20;
            float height = h - 20;
            float newX = Remap(index, 0, this.values.Count, 10, width);
            float newY = Remap(value, min, max, 10, height);

            return new Vector2(newX, newY);
        }

        private Vector2 GetLabelPoint(int index, float w, float h)
        {
            float width = w - 20;
            float height = h - 20;
            float newX = Remap(index, 0, this.values.Count, 10, width);

            return new Vector2(newX, 10);
        }

        private float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
        #endregion

        #region Linear Chart
        private void SetUpLine(int id, Vector2 point1, Vector2 point2)
        {
            VisualElement line = new VisualElement
            {
                name = "graph-line-" + id
            };

            SetLinePosition(ref line, point1, point2);

            line.AddToClassList("graph-line");

            _lineItems.Add(line);
            _lines.Add(line);
        }

        private void SetLinePosition(ref VisualElement line, Vector2 point1, Vector2 point2)
        {
            float length = Mathf.Sqrt((point2.x - point1.x) * (point2.x - point1.x) + (point2.y - point1.y) * (point2.y - point1.y));
            float inclDeg = Mathf.Atan2(point2.y - point1.y, point2.x - point1.x) * Mathf.Rad2Deg;

            if (float.IsNaN(length) || float.IsNaN(inclDeg))
                return;

            line.style.position = new StyleEnum<Position>(Position.Absolute);
            line.style.bottom = new StyleLength(new Length(point1.y, LengthUnit.Pixel));
            line.style.left = new StyleLength(new Length(point1.x, LengthUnit.Pixel));
            line.style.rotate = new StyleRotate(new Rotate(-inclDeg));
            line.style.width = new StyleLength(new Length(length, LengthUnit.Pixel));
            line.style.color = this.color;
            line.style.backgroundColor = this.color;
        }
        #endregion

        #region Labels
        // private void SetUpLabel(int id, Vector2 point, string message)
        // {
        //     Label messageLabel = new Label
        //     {
        //         text = message
        //     };
        //     SetLabelPosition(ref messageLabel, point);
        //     messageLabel.text = message;

        //     messageLabel.AddToClassList("graph-dot-message");
        //     _labelItems.Add(messageLabel);
        // }

        // private void SetLabelPosition(ref Label label, Vector2 point)
        // {
        //     label.style.position = new StyleEnum<Position>(Position.Absolute);
        //     label.style.bottom = new StyleLength(new Length(point.y - 1, LengthUnit.Percent));
        //     label.style.left = new StyleLength(new Length(point.x - 1, LengthUnit.Percent));
        // }
        #endregion
    }

    public class LineChart : VisualElement
    {
        private VisualElement _bar;
        private VisualElement _axes;
        private VisualElement _grid;
        private VisualElement _externalBackground;
        private VisualElement _background;

        private Dictionary<string, Line> lines = new Dictionary<string, Line>();

        private float min, max;
        private int maxPoints;

        #region UXML
        [Preserve]
        public new class UxmlFactory : UxmlFactory<LineChart, UxmlTraits> { }

        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits { }
        #endregion

        public LineChart()
        {
            SetupExternalBackground();
            SetupBackground();

            _axes = new VisualElement
            {
                name = "axes",
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    width = new StyleLength(new Length(100, LengthUnit.Percent)),
                    height = new StyleLength(new Length(100, LengthUnit.Percent)),
                }
            };
            _background.Add(_axes);
            SetUpAxe("horizontal-graph-axe");
            SetUpAxe("vertical-graph-axe");

            _grid = new VisualElement()
            {
                name = "grid",
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    width = new StyleLength(new Length(100, LengthUnit.Percent)),
                    height = new StyleLength(new Length(100, LengthUnit.Percent)),
                }
            };
            _background.Add(_grid);
            SetupGrid();
        }

        public void EnableGraph(int maxPoints)
        {
            this.maxPoints = maxPoints;
        }

        public void AddLine(string name, Color color, bool ownMinMax = false)
        {
            lines.Add(name, new Line(_background, maxPoints, ownMinMax, color));

            // Add a label to the key
            Label label = new Label 
            {
                text = name
            };
            label.AddToClassList("graph-label");
            label.style.color = new StyleColor(color);
            _axes.Add(label);
        }

        public void AddValue(string line, float value)
        {
            if (lines.ContainsKey(line))
            {
                lines[line].AddValue(value);

                min = int.MaxValue;
                max = int.MinValue;

                // Get new min and max values
                foreach (string name in lines.Keys)
                {
                    if (lines[name].min < min)
                    {
                        min = lines[name].min;
                    }
                    if (lines[name].max > max)
                    {
                        max = lines[name].max;
                    }
                }

                // Update all lines
                foreach (string name in lines.Keys)
                {
                    if (!lines[name].ownMinMax)
                    {
                        lines[name].min = min;
                        lines[name].max = max;
                    }

                    float width = _background.resolvedStyle.width;
                    float height = _background.resolvedStyle.height;

                    lines[name].UpdateLine(width, height);
                }
            }
        }

        #region Graph Layout
        private void SetupExternalBackground()
        {
            _externalBackground = new VisualElement()
            {
                name = "background",
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    width = new StyleLength(new Length(100, LengthUnit.Percent)),
                    height = new StyleLength(new Length(100, LengthUnit.Percent)),
                }
            };
            _externalBackground.name = "graph-external-background";
            _externalBackground.AddToClassList("graph-external-background");
            Add(_externalBackground);
        }

        private void SetupBackground()
        {
            _background = new VisualElement()
            {
                name = "background",
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    width = new StyleLength(new Length(100, LengthUnit.Percent)),
                    height = new StyleLength(new Length(100, LengthUnit.Percent)),
                }
            };
            _background.name = "graph-background";
            _background.AddToClassList("graph-background");
            _externalBackground.Add(_background);
        }

        private void SetUpAxe(string className)
        {
            VisualElement axe = new VisualElement
            {
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    bottom = new StyleLength(new Length(0, LengthUnit.Percent)),
                    left = new StyleLength(new Length(0, LengthUnit.Percent)),
                },
                name = "axe"
            };

            axe.AddToClassList(className);
            _axes.Add(axe);
        }

        private void SetupGrid()
        {
            for (int i = 1; i <= 10; i++)
            {
                SetUpGridLine(0, i * 10, "vertical-grid-line");
            }

            for (int j = 1; j <= 10; j++)
            {
                SetUpGridLine(j * 10, 0, "horizontal-grid-line");
            }
        }

        private void SetUpGridLine(float leftPos, float bottomPos, string className)
        {
            VisualElement gridLine = new VisualElement
            {
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    bottom = new StyleLength(new Length(leftPos, LengthUnit.Percent)),
                    left = new StyleLength(new Length(bottomPos, LengthUnit.Percent)),
                },
                name = "grid-line"
            };
            gridLine.AddToClassList(className);
            _grid.Add(gridLine);
        }
        #endregion
    }
}