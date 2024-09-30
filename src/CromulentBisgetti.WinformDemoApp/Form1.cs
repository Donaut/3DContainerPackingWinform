using CromulentBisgetti.ContainerPacking.Algorithms;
using CromulentBisgetti.ContainerPacking.Entities;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace CromulentBisgetti.WinformDemoApp
{
    public partial class Form1 : Form
    {
        private Timer _timer;
        private float _rotationY = 0;
        public float _initalAspectRatio;

        public float ZoomLevel { get; set; } = 600;

        

        public Form1()
        {
            InitializeComponent();

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);

            _initalAspectRatio = (float)ClientRectangle.Width / ClientRectangle.Height;

            var packingAlgorithm = new EB_AFIT();

            var container = new Container(0, 100, 100, 100);
            
            var items = new List<Item>();
            items.Add(new Item(0, 5, 5, 5, 3));

            var result = packingAlgorithm.Run(container, items);

            SetStyle(ControlStyles.ResizeRedraw, true);

            _timer = new Timer(components);
            _timer.Interval = (int)(1000 / 60);
            _timer.Tick += TimerRotation;
            _timer.Start();


        }

        private void TimerRotation(object? sender, EventArgs e)
        {
            //_rotationY += MathF.Tau / 360;
            _rotationY += .1f;
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            ZoomLevel += e.Delta;

           
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var clientRectangle = ClientRectangle;

            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-1, -1, -1), // Vertex 0 (bottom, front, left)
                new Vector3(1, -1, -1),  // Vertex 1 (bottom, front, right)
                new Vector3(1, -1, 1),   // Vertex 2 (bottom, back, right)
                new Vector3(-1, -1, 1),  // Vertex 3 (bottom, back, left)
                new Vector3(-1, 1, -1),  // Vertex 4 (top, front, left)
                new Vector3(1, 1, -1),   // Vertex 5 (top, front, right)
                new Vector3(1, 1, 1),    // Vertex 6 (top, back, right)
                new Vector3(-1, 1, 1)    // Vertex 7 (top, back, left)
            };


            var model = Matrix4x4.Identity;
            model *= Matrix4x4.CreateScale(100);
            model *= Matrix4x4.CreateRotationY(_rotationY);

            var view = Matrix4x4.CreateLookAt(new Vector3(0, 0, ZoomLevel), Vector3.Zero, Vector3.UnitY);

            // WARNING: If you uncomment this you should no longer divide the vector4 by the w field
            //var projection = Matrix4x4.CreatePerspective(clientRectangle.Width, clientRectangle.Height, 1f, float.PositiveInfinity);
            var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4, (float)clientRectangle.Width / clientRectangle.Height, .1f, float.PositiveInfinity);

            float t = projection.M11;
            const float Rad2Deg = 180 / MathF.PI;
            float fov = MathF.Atan(1.0f / t) * 2f * Rad2Deg;

            var MVP = Matrix4x4.Identity;
            MVP *= model;
            MVP *= view;
            MVP *= projection;
            

            var first = Vector4.Transform(vertices[0], MVP);
            var previous = first;

            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];

                var point = Vector4.Transform(vertex, MVP);

                var newPoint = point / point.W;

                // https://registry.khronos.org/OpenGL-Refpages/gl4/html/glViewport.xhtml
                var x = (point.X + 1) * (clientRectangle.Width / 2) + 0;
                var y = (point.Y + 1) * (clientRectangle.Height / 2) + 0;

                var x1 = (newPoint.X + 1) * (clientRectangle.Width / 2) + 0;
                var y1 = (newPoint.Y + 1) * (clientRectangle.Height / 2) + 0;

                if(!e.ClipRectangle.Contains((int)x1, (int)y1))
                    continue;

                e.Graphics.DrawRectangle(Pens.Black, x1, y1, 5, 5);
            }
        }
    }
}