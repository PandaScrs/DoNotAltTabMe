using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DoNotAltTabMe_4._7._2
{
    public partial class VisualOverlay : Form
    {
        private List<IntPtr> windows;
        private int currentIndex;
        private Timer displayTimer;   // Timer para mostrar la ventana
        private Timer fadingTimer;    // Timer para el efecto de fading
        private double opacity = 1.0; // Opacidad inicial

        public VisualOverlay(List<IntPtr> windows, int currentIndex)
        {
            InitializeComponent();
            this.windows = windows;
            this.currentIndex = currentIndex;
            InitializeOverlay();
        }

        private void InitializeOverlay()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(33, 38, 45);
            this.TransparencyKey = Color.FromArgb(33, 38, 45);
            this.Opacity = 0.95;      // Opacidad inicial
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Enabled = false;
            this.ControlBox = false;

            // Posicionar en el borde izquierdo centrado verticalmente
            var screenWorkingArea = Screen.PrimaryScreen.WorkingArea;
            int totalHeight = (windows.Count * 40) + (currentIndex == -1 ? 0 : 10);
            this.Location = new Point(
                0,
                screenWorkingArea.Height / 2 - totalHeight / 2
            );

            CreateCards();

            // Configurar timer para mostrar la ventana
            displayTimer = new Timer
            {
                Interval = 700 // 0.7 segundos
            };
            displayTimer.Tick += (s, e) => {
                displayTimer.Stop();
                StartFading();
            };
            displayTimer.Start();
        }

        private void StartFading()
        {
            // Configurar timer para el efecto de fading
            fadingTimer = new Timer
            {
                Interval = 15 // Intervalo corto para animación suave
            };
            fadingTimer.Tick += (s, e) => {
                // Reducir opacidad gradualmente
                opacity -= 0.05;
                if (opacity <= 0)
                {
                    this.Close();
                }
                else
                {
                    this.Opacity = opacity;
                }
            };
            fadingTimer.Start();
        }

        private void CreateCards()
        {
            int yPos = 0;

            // Crear primero las cartas no seleccionadas para asegurar que la seleccionada esté encima
            for (int i = 0; i < windows.Count; i++)
            {
                bool isSelected = i == currentIndex;
                int cardHeight = isSelected ? 50 : 40;

                // Crear el panel principal de la carta
                Panel card = new Panel
                {
                    Size = new Size(250, cardHeight),
                    BackColor = isSelected ? ColorTranslator.FromHtml("#7f5ea7") : ColorTranslator.FromHtml("#21262d"),
                    Location = new Point(0, yPos),
                    Margin = new Padding(0),
                    Padding = new Padding(10),
                    Cursor = Cursors.Default
                };

                // Crear la etiqueta de texto directamente en el panel
                Label label = new Label
                {
                    Text = GetWindowTitle(windows[i]),
                    ForeColor = Color.White, // Asegurar que el texto sea blanco
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft, // Centrar verticalmente el texto
                    Font = new Font("Segoe UI", 10f, isSelected ? FontStyle.Bold : FontStyle.Regular),
                    AutoEllipsis = true,
                    Cursor = Cursors.Default,
                    BackColor = Color.Transparent // Hacer el fondo de la etiqueta transparente
                };

                // Desactivar interacción
                card.Enabled = false;
                label.Enabled = false;
                card.Controls.Add(label);
                this.Controls.Add(card);

                // Mover a la siguiente posición
                yPos += cardHeight;

                // Solapamiento para tarjeta seleccionada
                if (isSelected && i < windows.Count - 1)
                {
                    yPos -= 10;
                }
            }

            this.Size = new Size(250, yPos + 5);
        }

        // Añadir esto para ignorar todos los eventos del mouse
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTTRANSPARENT = -1;

            if (m.Msg == WM_NCHITTEST)
            {
                m.Result = (IntPtr)HTTRANSPARENT;
                return;
            }
            base.WndProc(ref m);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                // Crear ventana sin activación para evitar el efecto de parpadeo
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                return cp;
            }
        }

        private string GetWindowTitle(IntPtr hWnd)
        {
            var sb = new StringBuilder(256);
            GetWindowText(hWnd, sb, 256);
            return sb.ToString();
        }

        // Importaciones necesarias
        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
    }
}
