using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DoNotAltTabMe_4._7._2
{
    public partial class ShortcutKeyForm : Form
    {
        public string SelectedKeys { get; private set; }
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private Label lblKeys;

        public ShortcutKeyForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
            this.KeyPreview = true;
            this.KeyDown += ShortcutKeyForm_KeyDown;
            this.KeyUp += ShortcutKeyForm_KeyUp;
        }

        private void InitializeCustomComponents()
        {
            // Configuraciones básicas
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(400, 300);
            this.BackColor = ColorTranslator.FromHtml("#0d1117");

            // Panel Principal
            TableLayoutPanel mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(20),
                BackColor = ColorTranslator.FromHtml("#0d1117")
            };

            // Título
            Label lblTitle = new Label
            {
                Text = "Cambiar Atajo",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            // Instrucciones
            Label lblInstructions = new Label
            {
                Text = "Por favor, mantén pulsadas las teclas que usarás como atajo, cuando hayas terminado, sólo deja de pulsarlas y quedarán grabadas",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            // Texto rojo de advertencia
            Label lblWarning = new Label
            {
                Text = "Si la combinación de teclas no funciona, posiblemente se deba a que este atajo ya está usado por otro programa, así que te tocará elegir otro!",
                ForeColor = ColorTranslator.FromHtml("#ff7b72"),
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            // Label que muestra las teclas presionadas
            lblKeys = new Label
            {
                Text = "Esperando teclas...",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            // Configuración del Layout del Panel
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

            // Controles en el Panel
            mainPanel.Controls.Add(lblTitle, 0, 0);
            mainPanel.Controls.Add(lblInstructions, 0, 1);
            mainPanel.Controls.Add(lblWarning, 0, 2);
            mainPanel.Controls.Add(lblKeys, 0, 3);

            // Añadir panel a la ventana
            this.Controls.Add(mainPanel);

            // Botón de cerrar
            Button closeButton = new Button
            {
                Size = new Size(32, 32),
                Location = new Point(this.Width - 32, 0),
                FlatStyle = FlatStyle.Flat,
                Text = "×",
                Font = new Font("Segoe UI", 12f),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => this.Close();

            // Hover para el botón cerrar
            closeButton.MouseEnter += (s, e) => closeButton.ForeColor = ColorTranslator.FromHtml("#ff7b72");
            closeButton.MouseLeave += (s, e) => closeButton.ForeColor = Color.White;

            this.Controls.Add(closeButton);


        }

        private void ShortcutKeyForm_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            if (!pressedKeys.Contains(e.KeyCode))
            {
                pressedKeys.Add(e.KeyCode);
                UpdateKeysDisplay();
            }
        }

        private void ShortcutKeyForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (pressedKeys.Count > 0)
            {
                var normalizedKeys = new List<string>();
                foreach (var key in pressedKeys)
                {
                    string keyName = key.ToString();

                    // Convertir nombres específicos a genéricos para guardar
                    if (keyName == "LControlKey" || keyName == "RControlKey")
                        keyName = "Control";
                    else if (keyName == "LShiftKey" || keyName == "RShiftKey")
                        keyName = "Shift";
                    else if (keyName == "LMenu" || keyName == "RMenu")
                        keyName = "Alt";

                    normalizedKeys.Add(keyName);
                }

                SelectedKeys = string.Join(" + ", normalizedKeys);
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void UpdateKeysDisplay()
        {
            var keyNames = new List<string>();
            foreach (var key in pressedKeys)
            {
                string keyName = key.ToString();

                // Convertir nombres específicos a genéricos para mostrar
                if (keyName == "LControlKey" || keyName == "RControlKey")
                    keyName = "Control";
                else if (keyName == "LShiftKey" || keyName == "RShiftKey")
                    keyName = "Shift";
                else if (keyName == "LMenu" || keyName == "RMenu")
                    keyName = "Alt";

                keyNames.Add(keyName);
            }

            lblKeys.Text = string.Join(" + ", keyNames);
        }
    }
}

