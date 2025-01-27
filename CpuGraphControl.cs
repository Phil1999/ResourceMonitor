using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ResourceMonitor.Controls
{
    public class CpuGraphControl : Control
    {
        private const int MAX_POINTS = 60; // 1 minute of history at 1 sample/second

        public static readonly DependencyProperty ValuesProperty =
            DependencyProperty.Register(
                "Values",
                typeof(Queue<float>),
                typeof(CpuGraphControl),
                new PropertyMetadata(new Queue<float>(), OnValuesChanged));

        public Queue<float> Values
        {
            get => (Queue<float>)GetValue(ValuesProperty);
            set => SetValue(ValuesProperty, value);
        }

        public static readonly DependencyProperty LineColorProperty =
            DependencyProperty.Register(
                "LineColor",
                typeof(Brush),
                typeof(CpuGraphControl),
                new PropertyMetadata(Brushes.White));

        public Brush LineColor
        {
            get => (Brush)GetValue(LineColorProperty);
            set => SetValue(LineColorProperty, value);
        }

        private Path _graphPath;
        private Canvas _canvas;

        static CpuGraphControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CpuGraphControl),
                new FrameworkPropertyMetadata(typeof(CpuGraphControl)));
        }

        public CpuGraphControl()
        {
            _canvas = new Canvas();
            _graphPath = new Path
            {
                Stroke = LineColor,
                StrokeThickness = 1,
                Data = new PathGeometry()
            };

            AddVisualChild(_canvas);
            AddLogicalChild(_canvas);
            _canvas.Children.Add(_graphPath);
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException();
            return _canvas;
        }

        protected override int VisualChildrenCount => 1;

        protected override Size MeasureOverride(Size constraint)
        {
            _canvas.Measure(constraint);
            return constraint;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            _canvas.Arrange(new Rect(arrangeBounds));
            UpdateGraph();
            return arrangeBounds;
        }

        private static void OnValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CpuGraphControl)d;
            control.UpdateGraph();
        }

        private void UpdateGraph()
        {
            if (ActualWidth <= 0 || ActualHeight <= 0 || Values == null || Values.Count == 0)
                return;

            var values = Values.ToArray();
            var geometry = new PathGeometry();
            var figure = new PathFigure();
            var segment = new PolyLineSegment();

            double xStep = ActualWidth / (MAX_POINTS - 1);
            double x = 0;

            for (int i = 0; i < values.Length; i++)
            {
                var point = new Point(
                    x,
                    ActualHeight * (1 - (values[i] / 100.0)) // Scale to percentage
                );

                if (i == 0)
                    figure.StartPoint = point;
                else
                    segment.Points.Add(point);

                x += xStep;
            }

            if (segment.Points.Count > 0)
            {
                figure.Segments.Add(segment);
                geometry.Figures.Add(figure);
                _graphPath.Data = geometry;
            }
        }
    }
}