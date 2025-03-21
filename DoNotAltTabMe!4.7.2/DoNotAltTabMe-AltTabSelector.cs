using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DoNotAltTabMe_4._7._2
{
    public partial class AltTabSelector : Form
    {
        // Lista de handles de ventanas permitidas
        private List<IntPtr> allowedWindows = new List<IntPtr>();

        // API de Windows para manipular ventanas
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        // Necesario para mover la ventana
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        // Constantes de estilos de ventana
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        private bool functionActive = false;
        private Label lblStatus;

        public AltTabSelector()
        {
            InitializeComponent();
            InitializeCustomComponents();
            RefreshWindowList();
            functionActive = false;
            UpdateStatusAndButtons();
        }

        // Actualizar la lista de ventanas
        private void RefreshWindowList()
        {
            // Guardar las ventanas que estaban marcadas
            HashSet<IntPtr> checkedHandles = new HashSet<IntPtr>();
            foreach (ListViewItem item in listViewWindows.Items)
            {
                if (item.Checked)
                {
                    checkedHandles.Add((IntPtr)item.Tag);
                }
            }

            listViewWindows.Items.Clear();
            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd) && !IsSystemWindow(hWnd))
                {
                    var sbTitle = new StringBuilder(256);
                    GetWindowText(hWnd, sbTitle, 256);
                    string title = sbTitle.ToString();
                    if (!string.IsNullOrEmpty(title) && !IsExcludedWindow(title))
                    {
                        var item = new ListViewItem(title);
                        item.Tag = hWnd;
                        item.Checked = checkedHandles.Contains(hWnd);
                        listViewWindows.Items.Add(item);
                    }
                }
                return true;
            }, IntPtr.Zero);
        }

        // Método para verificar si una ventana debe ser excluida
        private bool IsExcludedWindow(string title)
        {
            string[] excludedTitles = {
                "Configuración",
                "Calculadora"
            };

            return excludedTitles.Any(excludedTitle => title.IndexOf(excludedTitle, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        // Botón: Seleccionar ventanas permitidas
        private void btnSelect_Click(object sender, EventArgs e)
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
        }

        private bool IsSystemWindow(IntPtr hWnd)
        {
            // Obtener el nombre de clase de la ventana
            var className = new StringBuilder(256);
            GetClassName(hWnd, className, className.Capacity);

            // Lista expandida de nombres de clase de ventanas del sistema
            string[] systemClasses = {
                "Shell_TrayWnd",     // Barra de tareas
                "Button",            // Botones en la barra de tareas
                "WorkerW",           // Ventanas auxiliares del escritorio
                "Progman",           // Program Manager
                "DV2ControlHost",    // Elementos del escritorio
                "Windows.UI.Core.CoreWindow", // Elementos modernos de Windows
                "MultitaskingViewFrame", // Vista de tareas
                "Shell_SecondaryTrayWnd", // Barra de tareas secundaria
                "NotifyIconOverflowWindow", // Área de notificación
                "Windows.UI.Composition.DesktopWindowContentBridge" // Elementos UWP
            };

            // Verificar si es una ventana del sistema
            if (systemClasses.Contains(className.ToString()))
                return true;

            // Obtener el título de la ventana
            var title = new StringBuilder(256);
            GetWindowText(hWnd, title, 256);
            string windowTitle = title.ToString().ToLower();

            // Lista de títulos de ventanas del sistema a ignorar
            string[] systemTitles = {
                "program manager",
                "start",
                "cortana",
                "task view",
                "windows shell experience host"
            };

            return systemTitles.Any(t => windowTitle.Contains(t));
        }

        // Aplicar reglas: Ocultar todas excepto las permitidas
        private void ApplyWindowRules()
        {
            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd) && !IsSystemWindow(hWnd))
                {
                    int style = GetWindowLong(hWnd, GWL_EXSTYLE);
                    if (allowedWindows.Contains(hWnd))
                    {
                        SetWindowLong(hWnd, GWL_EXSTYLE, style & ~WS_EX_TOOLWINDOW);
                    }
                    else
                    {
                        SetWindowLong(hWnd, GWL_EXSTYLE, style | WS_EX_TOOLWINDOW);
                    }
                }
                return true;
            }, IntPtr.Zero);
        }

        // Botón: Restaurar todas las ventanas
        private void btnRestore_Click(object sender, EventArgs e)
        {
            allowedWindows.Clear();
            foreach (ListViewItem item in listViewWindows.Items)
            {
                item.Checked = false;
            }

            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd) && !IsSystemWindow(hWnd))
                {
                    int style = GetWindowLong(hWnd, GWL_EXSTYLE);
                    SetWindowLong(hWnd, GWL_EXSTYLE, style & ~WS_EX_TOOLWINDOW);
                }
                return true;
            }, IntPtr.Zero);

            functionActive = false;
            UpdateStatusAndButtons();
            MessageBox.Show("Hecho! ahora todas las ventanas vuelven a estar visibles");
        }

        private void UpdateStatusAndButtons()
        {
            if (functionActive)
            {
                lblStatus.Text = "Estado: Activado";
                lblStatus.ForeColor = ColorTranslator.FromHtml("#7f5ea7");
                btnSelect.Text = "Actualizar ventanas";
                btnRestore.Enabled = true;
            }
            else
            {
                lblStatus.Text = "Estado: Desactivado";
                lblStatus.ForeColor = Color.White;
                btnSelect.Text = "Permitir ventanas";
                btnRestore.Enabled = false;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshWindowList();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}


