using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DoNotAltTabMe_4._7._2
{
    partial class AltTabSelector
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

        private void InitializeCustomComponents()
        {
            // Eliminar borde nativo de Windows
            this.FormBorderStyle = FormBorderStyle.None;

            // Centrar la ventana por defecto
            this.StartPosition = FormStartPosition.CenterScreen;

            // Tamaño de la ventana
            this.MinimumSize = new Size(510, 400);

            // Ícono de DoNotAltTabMe
            string iconPath = System.IO.Path.Combine(Application.StartupPath, "ico", "DoNotAltTabMe.ico");
            this.Icon = new Icon(iconPath);

            // Panel de Ventana Personalizado
            Panel titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = ColorTranslator.FromHtml("#21262d")
            };

            // Botón de regresar (←)
            Button backButton = new Button
            {
                Size = new Size(46, 32),
                Location = new Point(0, 0),
                FlatStyle = FlatStyle.Flat,
                Text = "←",
                Font = new Font("Segoe UI", 12f),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            backButton.FlatAppearance.BorderSize = 0;
            backButton.Click += (s, e) =>
            {
                // Restaurar todas las ventanas si la función sigue activa y se pulsa este botón (La gente no decepciona, ultra necesario)
                if (functionActive)
                {
                    EnumWindows((hWnd, lParam) =>
                    {
                        if (IsWindowVisible(hWnd) && !IsSystemWindow(hWnd))
                        {
                            int style = GetWindowLong(hWnd, GWL_EXSTYLE);
                            SetWindowLong(hWnd, GWL_EXSTYLE, style & ~WS_EX_TOOLWINDOW);
                        }
                        return true;
                    }, IntPtr.Zero);

                    allowedWindows.Clear();
                    functionActive = false;
                }
                this.Hide();
                var mainForm = Application.OpenForms.OfType<Main>().FirstOrDefault();
                if (mainForm != null)
                {
                    mainForm.Show();
                }
                else
                {
                    var newMainForm = new Main();
                    newMainForm.Show();
                }
                this.Close();
            };
            titleBar.Controls.Add(backButton);

            // Icono de la aplicación
            PictureBox appIcon = new PictureBox
            {
                Size = new Size(16, 16),
                Location = new Point(10, 8),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = this.Icon.ToBitmap()
            };

            // Título "Do Not AltTab Me!"
            Label appTitle = new Label
            {
                Text = "Do Not AltTab Me!",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f),
                Location = new Point(32, 8),
                AutoSize = true
            };

            appIcon.Location = new Point(56, 8);
            appTitle.Location = new Point(78, 8);

            // Botón de cerrar (×)
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
            closeButton.Click += (s, e) =>
            {
                // Restaurar todas las ventanas en Alt+Tab si la función está activa (La gente no decepciona, ultra necesario)
                if (functionActive)
                {
                    EnumWindows((hWnd, lParam) =>
                    {
                        if (IsWindowVisible(hWnd) && !IsSystemWindow(hWnd))
                        {
                            int style = GetWindowLong(hWnd, GWL_EXSTYLE);
                            SetWindowLong(hWnd, GWL_EXSTYLE, style & ~WS_EX_TOOLWINDOW);
                        }
                        return true;
                    }, IntPtr.Zero);
                    allowedWindows.Clear();
                    functionActive = false;
                }
                Application.Exit();
            };

            // Botón de minimizar (-)
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
            setupHoverEffects(backButton);
            setupHoverEffects(closeButton);
            setupHoverEffects(minimizeButton);

            // Permitir mover la ventana (Desde el Panel)
            titleBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };

            // Agregar controles a la barra de título
            titleBar.Controls.Add(appIcon);
            titleBar.Controls.Add(appTitle);
            titleBar.Controls.Add(minimizeButton);
            titleBar.Controls.Add(closeButton);

            // Crear panel principal que contendrá todo excepto la barra de título
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(50)
            };

            // Hacer la aplicación responsiva usando TableLayoutPanel (NoHaceNada, pero es placeholder)
            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0)
            };

            // Etiqueta de estado en el Panel
            lblStatus = new Label
            {
                Text = "Estado: Desactivado",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(300, 8),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            titleBar.Controls.Add(lblStatus);

            // Extensión de la función de arrastrar la ventana
            Action<Control> enableDrag = (control) =>
            {
                control.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        ReleaseCapture();
                        SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                    }
                };
            };
            enableDrag(appIcon);
            enableDrag(appTitle);
            enableDrag(lblStatus);

            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 90F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

            this.BackColor = ColorTranslator.FromHtml("#0d1117");

            // Estilo de los botones
            btnSelect.BackColor = ColorTranslator.FromHtml("#21262d");
            btnSelect.TextAlign = ContentAlignment.MiddleCenter;
            btnSelect.ForeColor = Color.White;
            btnSelect.FlatStyle = FlatStyle.Flat;
            btnSelect.FlatAppearance.BorderSize = 0;
            btnSelect.Size = new Size(150, 30);

            btnRestore.BackColor = ColorTranslator.FromHtml("#21262d");
            btnRestore.ForeColor = Color.White;
            btnRestore.TextAlign = ContentAlignment.MiddleCenter;
            btnRestore.FlatStyle = FlatStyle.Flat;
            btnRestore.FlatAppearance.BorderSize = 0;
            btnRestore.Size = new Size(150, 30);

            btnRefresh.BackColor = ColorTranslator.FromHtml("#21262d");
            btnRefresh.ForeColor = Color.White;
            btnRestore.TextAlign = ContentAlignment.MiddleCenter;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Size = new Size(150, 30);

            // Configurar el ListView
            listViewWindows.Dock = DockStyle.Fill;
            listViewWindows.View = View.Details;
            listViewWindows.FullRowSelect = true;
            listViewWindows.HeaderStyle = ColumnHeaderStyle.None;
            listViewWindows.BackColor = ColorTranslator.FromHtml("#0d1117");
            listViewWindows.ForeColor = Color.White;
            listViewWindows.OwnerDraw = true;
            listViewWindows.BorderStyle = BorderStyle.None;
            listViewWindows.Scrollable = false;
            listViewWindows.CheckBoxes = true; // Habilitar checkboxes
            listViewWindows.Columns.Add("", -2, HorizontalAlignment.Left);
            listViewWindows.HideSelection = true; // Ocultar la selección por defecto de Windows
            listViewWindows.ItemChecked -= ListViewWindows_ItemChecked;
            listViewWindows.MouseClick += (sender, e) =>
            {
                ListViewHitTestInfo hitTest = listViewWindows.HitTest(e.Location);
                if (hitTest.Item != null)
                {
                    hitTest.Item.Checked = !hitTest.Item.Checked;
                }
            };

            // Eventos del ListView
            listViewWindows.DrawColumnHeader += (sender, e) => e.DrawDefault = true;
            listViewWindows.DrawItem += ListViewWindows_DrawItem;

            // Placeholder para responsibidad (A futuro)
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Padding = new Padding(0,32,0,0);
            tableLayoutPanel.ColumnCount = 1;
            tableLayoutPanel.RowCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 90F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

            // Panel para los botones
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 40, 0, 0)
            };
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.LeftToRight;
            buttonPanel.Padding = new Padding(10, 0, 10, 0);
            buttonPanel.AutoSize = true;
            buttonPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            buttonPanel.Controls.Add(btnSelect);
            buttonPanel.Controls.Add(btnRestore);
            buttonPanel.Controls.Add(btnRefresh);

            tableLayoutPanel.Controls.Add(listViewWindows, 0, 0);
            tableLayoutPanel.Controls.Add(buttonPanel, 0, 1);

            mainPanel.Controls.Add(tableLayoutPanel);

            this.Controls.Add(titleBar);
            this.Controls.Add(tableLayoutPanel);
        }

        private void ListViewWindows_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;

            // Colores
            Color backgroundColor = listViewWindows.BackColor;
            Color selectionColor = ColorTranslator.FromHtml("#7f5ea7");
            Color textColor = listViewWindows.ForeColor;
            Color checkboxColor = Color.White;

            // Determinar el color de fondo
            if (e.Item.Selected)
            {
                e.Graphics.FillRectangle(new SolidBrush(selectionColor), e.Bounds);
            }
            else if ((e.State & ListViewItemStates.Hot) != 0)
            {
                // 30% Opacidad
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(77, 127, 94, 167)), e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(backgroundColor), e.Bounds);
            }

            // Checkbox Personalizado
            Rectangle checkBoxBounds = new Rectangle(
                e.Bounds.X + 4,
                e.Bounds.Y + (e.Bounds.Height - 12) / 2,
                12,
                12
            );
            using (Pen checkboxPen = new Pen(checkboxColor))
            {
                e.Graphics.DrawRectangle(checkboxPen, checkBoxBounds);
            }

            // Check del Checkbox
            if (e.Item.Checked)
            {
                using (Pen checkPen = new Pen(checkboxColor, 2))
                {
                    Point[] checkmarkPoints = new Point[]
                    {
                new Point(checkBoxBounds.X + 2, checkBoxBounds.Y + 6),
                new Point(checkBoxBounds.X + 4, checkBoxBounds.Y + 8),
                new Point(checkBoxBounds.X + 9, checkBoxBounds.Y + 3)
                    };
                    e.Graphics.DrawLines(checkPen, checkmarkPoints);
                }
            }

            // Texto con padding para el checkbox
            Rectangle textBounds = new Rectangle(
                checkBoxBounds.Right + 8,
                e.Bounds.Y,
                e.Bounds.Width - checkBoxBounds.Right - 12,
                e.Bounds.Height
            );

            TextRenderer.DrawText(
                e.Graphics,
                e.Item.Text,
                listViewWindows.Font,
                textBounds,
                textColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left
            );
        }

        private void ListViewWindows_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            // Actualizar la lista de ventanas permitidas en base al Checkbox
            allowedWindows.Clear();
            foreach (ListViewItem item in listViewWindows.CheckedItems)
            {
                IntPtr handle = (IntPtr)item.Tag;
                allowedWindows.Add(handle);
            }
            ApplyWindowRules();
        }

        private void ListViewWindows_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void InitializeComponent()
        {
            this.listViewWindows = new System.Windows.Forms.ListView();
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnRestore = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // listViewWindows
            this.listViewWindows.Location = new System.Drawing.Point(12, 12);
            this.listViewWindows.Name = "listViewWindows";
            this.listViewWindows.Size = new System.Drawing.Size(400, 290);
            this.listViewWindows.TabIndex = 1;
            this.listViewWindows.UseCompatibleStateImageBehavior = false;

            // Botón "Seleccionar Ventanas" -- AltTabSelector
            this.btnSelect.Location = new System.Drawing.Point(12, 320);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(150, 30);
            this.btnSelect.TextAlign = ContentAlignment.MiddleCenter;
            this.btnSelect.TabIndex = 2;
            this.btnSelect.Text = "Permitir ventanas";
            btnSelect.Click -= btnSelect_Click;
            btnSelect.Click += (sender, e) =>
            {
                allowedWindows.Clear();
                foreach (ListViewItem item in listViewWindows.CheckedItems)
                {
                    IntPtr handle = (IntPtr)item.Tag;
                    allowedWindows.Add(handle);
                }

                if (allowedWindows.Count > 0)
                {
                    ApplyWindowRules();
                    functionActive = true;
                    UpdateStatusAndButtons();
                    MessageBox.Show("Reglas Aplicadas! ahora sólo las opciones marcadas aparecerán en Alt+Tab!");
                }
                else
                {
                    MessageBox.Show("Debes seleccionar al menos una ventana para aplicar las reglas.");
                }
            };

            // Botón "Restaurar Todas las Ventanas" -- AltTabSelector
            this.btnRestore.Location = new System.Drawing.Point(172, 320);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(150, 30);
            this.btnRestore.TabIndex = 3;
            this.btnRestore.Text = "Restaurar todas";
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);

            // Botón "Actualizar la lista" -- AltTabSelector
            this.btnRefresh.Location = new System.Drawing.Point(332, 320);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(150, 30);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "Actualizar Lista";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            // Título "Do Not AltTab Me!" -- AltTabSelector
            this.ClientSize = new System.Drawing.Size(494, 360);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.listViewWindows);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnRestore);
            this.Name = "Form1";
            this.Text = "Do Not AltTab Me!";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        private ListView listViewWindows;
        private Button btnSelect;
        private Button btnRestore;
        private Button btnRefresh;
    }
}


