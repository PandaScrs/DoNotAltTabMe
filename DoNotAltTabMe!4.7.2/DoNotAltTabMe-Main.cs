using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace DoNotAltTabMe_4._7._2
{
    public partial class Main : Form
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        public Main()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Main
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Name = "Main";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Do Not AltTab Me!";
            this.ResumeLayout(false);
        }

        private void InitializeCustomComponents()
        {
            // Configuración de la ventana
            this.FormBorderStyle = FormBorderStyle.None;
            this.MinimumSize = new Size(400, 300);
            this.BackColor = ColorTranslator.FromHtml("#0d1117");

            // Panel de Ventana Personalizado
            Panel titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = ColorTranslator.FromHtml("#21262d")
            };

            // Icono de la aplicación
            string iconPath = System.IO.Path.Combine(Application.StartupPath, "ico", "DoNotAltTabMe.ico");
            this.Icon = new Icon(iconPath);


            // Botones en el Panel
            Button closeButton = new Button
            {
                Size = new Size(46, 32),
                Location = new Point(titleBar.Width - 46, 0),
                FlatStyle = FlatStyle.Flat,
                Text = "×",
                Font = new Font("Segoe UI", 12f),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => Application.Exit();

            Button minimizeButton = new Button
            {
                Size = new Size(46, 32),
                Location = new Point(titleBar.Width - 92, 0),
                FlatStyle = FlatStyle.Flat,
                Text = "−",
                Font = new Font("Segoe UI", 12f),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            minimizeButton.FlatAppearance.BorderSize = 0;
            minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            // Efectos de Hover a los Botones
            Action<Button> setupHoverEffects = (btn) =>
            {
                Color originalForeColor = btn.ForeColor;
                Color originalBackColor = btn.BackColor;

                btn.MouseEnter += (s, e) =>
                {
                    btn.ForeColor = Color.White;
                    btn.BackColor = ColorTranslator.FromHtml("#7f5ea7");
                };
                btn.MouseLeave += (s, e) =>
                {
                    btn.ForeColor = originalForeColor;
                    btn.BackColor = originalBackColor;
                };
            };
            setupHoverEffects(closeButton);
            setupHoverEffects(minimizeButton);

            // Permitir mover la ventana
            titleBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };

            // Agregar controles al Panel
            titleBar.Controls.Add(minimizeButton);
            titleBar.Controls.Add(closeButton);

            // Panel principal para el contenido
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Panel para el encabezado (ícono, título y subtítulo)
            TableLayoutPanel headerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 2,
                Height = 80,  // Aumentamos la altura
                Padding = new Padding(5),
                BackColor = Color.Transparent,
                AutoSize = false
            };

            // Configurar columnas
            headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            headerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            headerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            // Ícono grande (Logo de la aplicación)
            PictureBox largeLogo = new PictureBox
            {
                Size = new Size(64, 64),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = this.Icon.ToBitmap(),
                Anchor = AnchorStyles.None
            };

            // Container del Icon
            Panel logoContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            logoContainer.Controls.Add(largeLogo);
            largeLogo.Location = new Point(
                (logoContainer.Width - largeLogo.Width) / 2,
                (logoContainer.Height - largeLogo.Height) / 2
            );

            // Título y subtítulo
            Label mainTitle = new Label
            {
                Text = "Do Not AltTab Me",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                TextAlign = ContentAlignment.BottomLeft,
                Dock = DockStyle.Fill,
                AutoSize = false,
                Padding = new Padding(20, 0, 0, 0)
            };

            Label subtitle = new Label
            {
                Text = "¿Qué opción desea elegir?",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f),
                TextAlign = ContentAlignment.TopLeft,
                Dock = DockStyle.Fill,
                AutoSize = false,
                Padding = new Padding(20, 0, 0, 0)
            };

            // Panel para los botones
            TableLayoutPanel buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 2,
                Height = 100,
                Padding = new Padding(50, 10, 50, 10),
                AutoSize = false
            };
            buttonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            buttonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            // Botones de selección de modo
            Button btnAltTab = new Button
            {
                Text = "Selector AltTab",
                BackColor = ColorTranslator.FromHtml("#21262d"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 35,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10f),
                Margin = new Padding(0, 0, 0, 5)
            };
            btnAltTab.FlatAppearance.BorderSize = 0;
            btnAltTab.Click += (s, e) =>
            {
                var mainScreen = new AltTabSelector();
                mainScreen.Show();
                this.Hide();
            };

            Button btnShortcut = new Button
            {
                Text = "Selector por Atajo",
                BackColor = ColorTranslator.FromHtml("#21262d"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 35,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10f),
                Margin = new Padding(0, 5, 0, 0)
            };
            btnShortcut.FlatAppearance.BorderSize = 0;
            btnShortcut.Click += (s, e) =>
            {
                var mainScreen = new ShortcutSelector();
                mainScreen.Show();
                this.Hide();
            };

            // Efectos hover
            Action<Button> setupMainButtonHoverEffects = (btn) =>
            {
                btn.MouseEnter += (s, e) => btn.BackColor = ColorTranslator.FromHtml("#2f363d");
                btn.MouseLeave += (s, e) => btn.BackColor = ColorTranslator.FromHtml("#21262d");
            };

            setupMainButtonHoverEffects(btnAltTab);
            setupMainButtonHoverEffects(btnShortcut);

            // Agregar controles en base a su orden
            headerPanel.Controls.Add(logoContainer, 0, 0);
            headerPanel.SetRowSpan(logoContainer, 2);
            headerPanel.Controls.Add(mainTitle, 1, 0);
            headerPanel.Controls.Add(subtitle, 1, 1);

            buttonPanel.Controls.Add(btnAltTab, 0, 0);
            buttonPanel.Controls.Add(btnShortcut, 0, 1);

            mainPanel.Controls.Add(buttonPanel);
            mainPanel.Controls.Add(headerPanel);

            // Agregar el panel principal a la ventana
            this.Controls.Add(mainPanel);
            this.Controls.Add(titleBar);
        }
    }
}
