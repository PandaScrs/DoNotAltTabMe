using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DoNotAltTabMe_4._7._2
{
    partial class ShortcutSelector
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                keyboardHook?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeCustomComponents()
        {
            // Eliminar borde nativo de Windows
            this.FormBorderStyle = FormBorderStyle.None;

            // Centrar la ventana en la pantalla
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
                // Desactivar la función de atajos antes de salir (Porque sí, la gente lo olvidará)
                orderedWindows.Clear();
                currentWindowIndex = -1;
                functionActive = false;

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

            // Ajustar posición del icono y título para hacer espacio al botón de regresar
            appIcon.Location = new Point(56, 8);
            appTitle.Location = new Point(78, 8);

            // Agregar el botón de regresar a la barra de título
            titleBar.Controls.Add(backButton);

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
            closeButton.Click += (s, e) => Application.Exit();

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

            // Etiqueta de estado en el Panel
            lblStatus = new Label
            {
                Text = "Estado: Desactivado",
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(300, 8),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = ColorTranslator.FromHtml("#21262d")
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

            // Permitir mover la ventana (Desde el Panel)
            titleBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };



            // Agregar controles al Panel
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

            // Label para mostrar el atajo actual
            lblShortcut.BackColor = Color.Transparent;
            lblShortcut.ForeColor = Color.White;

            // Botón de Cambio de Atajo
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 90F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

            // Cambiar el color de fondo a #0d1117 (color del Panel)
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

            // Botón de cambiar atajo
            btnChangeShortcut.BackColor = ColorTranslator.FromHtml("#21262d");
            btnChangeShortcut.ForeColor = Color.White;
            btnChangeShortcut.TextAlign = ContentAlignment.MiddleCenter;
            btnChangeShortcut.FlatStyle = FlatStyle.Flat;
            btnChangeShortcut.FlatAppearance.BorderSize = 0;
            btnChangeShortcut.Size = new Size(150, 30);
            btnChangeShortcut.Text = "Cambiar Atajo";
            btnChangeShortcut.Margin = new Padding(0, -5, 0, 0);
            btnChangeShortcut.Click += btnChangeShortcut_Click; // Evento de Cambio de Atajo

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
            listViewWindows.CheckBoxes = true; // Checkboxes habilitadas
            listViewWindows.Columns.Add("", -2, HorizontalAlignment.Left);
            listViewWindows.HideSelection = true;
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
            tableLayoutPanel.Padding = new Padding(0, 32, 0, 0);
            tableLayoutPanel.ColumnCount = 1;
            tableLayoutPanel.RowCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 90F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

            // FlowLayoutPanel para los botones principales
            FlowLayoutPanel mainButtonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 10, 0, 10),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            // Panel adicional para el atajo y su Botón
            FlowLayoutPanel shortcutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 5, 0, 0),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            // TableLayoutPanel principal con la nueva fila de botones (atajos)
            tableLayoutPanel.RowCount = 3;
            tableLayoutPanel.RowStyles.Clear();
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

            // Agregar los controles a los paneles
            mainButtonPanel.Controls.Add(btnSelect);
            mainButtonPanel.Controls.Add(btnRestore);
            mainButtonPanel.Controls.Add(btnRefresh);

            shortcutPanel.Controls.Add(lblShortcut);
            shortcutPanel.Controls.Add(btnChangeShortcut);

            tableLayoutPanel.Controls.Clear();
            tableLayoutPanel.Controls.Add(listViewWindows, 0, 0);
            tableLayoutPanel.Controls.Add(mainButtonPanel, 0, 1);
            tableLayoutPanel.Controls.Add(shortcutPanel, 0, 2);

            mainPanel.Controls.Add(tableLayoutPanel);

            this.Controls.Add(titleBar);
            this.Controls.Add(tableLayoutPanel);

            // Efectos de Hover a los Botones
            Action<Button> setupButtonHoverEffects = (btn) =>
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
            setupButtonHoverEffects(backButton);
            setupButtonHoverEffects(closeButton);
            setupButtonHoverEffects(minimizeButton);
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
            this.btnChangeShortcut = new System.Windows.Forms.Button();
            this.lblShortcut = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // listViewWindows
            this.listViewWindows.Location = new System.Drawing.Point(12, 12);
            this.listViewWindows.Name = "listViewWindows";
            this.listViewWindows.Size = new System.Drawing.Size(470, 290);
            this.listViewWindows.TabIndex = 1;
            this.listViewWindows.UseCompatibleStateImageBehavior = false;

            // Botón "Establecer Ventana" -- ShortcutSelector
            this.btnSelect.Location = new System.Drawing.Point(12, 320);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(470, 30);
            this.btnSelect.TabIndex = 2;
            this.btnSelect.Text = "Establecer Ventana";
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);

            // Botón "Deshabilitar Función" -- ShortCutSelector
            this.btnRestore.Location = new System.Drawing.Point(12, 356);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(470, 30);
            this.btnRestore.TabIndex = 3;
            this.btnRestore.Text = "Deshabilitar Función";
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);

            // Botón "Actualizar Lista" -- ShortcutSelector
            this.btnRefresh.Location = new System.Drawing.Point(12, 392);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(470, 30);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "Actualizar Lista";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            // Label con el texto del atajo -- ShortcutSelector
            this.lblShortcut.AutoSize = true;
            this.lblShortcut.Location = new System.Drawing.Point(12, 432);
            this.lblShortcut.Name = "lblShortcut";
            this.lblShortcut.Size = new System.Drawing.Size(250, 15);
            this.lblShortcut.Text = $"Tecla de Atajos: {currentShortcut}";
            this.lblShortcut.ForeColor = Color.White;
            this.lblShortcut.Font = new Font("Segoe UI", 9f);
            this.lblShortcut.BackColor = Color.Transparent;
            this.lblShortcut.Padding = new Padding(10, 7, 10, 5);

            // Botón "Selector de Atajos" -- ShortcutSelector
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(494, 470);
            this.Controls.Add(this.btnChangeShortcut);
            this.Controls.Add(this.lblShortcut);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnRestore);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.listViewWindows);
            this.Name = "ShortcutSelector";
            this.Text = "Selector de Atajos";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ListView listViewWindows;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnChangeShortcut;
        private System.Windows.Forms.Label lblShortcut;

    }
}


