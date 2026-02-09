namespace Compilador
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        //VARIABLES GLOBALES
        public string archivo;
        public string archivoback;
        public string archivotrad;
        public int i_caracter;
        public int N_caracter;
        public int N_linea;
        public char c_caracter;
        public StreamWriter Escribir;
        public StreamReader Leer;
        public int N_error;
        public int prueba = 0;
        public string token;          // Guarda el token actual leído del archivo .back
        public StreamReader LeerBack; // Para leer el archivo .back
        public bool finArchivo;       // Marca si ya se llegó al final del archivo
        public int N_linea_sintactico;


        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            aRCHIVOToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripMenuItem();
            nuevoToolStripMenuItem = new ToolStripMenuItem();
            gUAToolStripMenuItem = new ToolStripMenuItem();
            gUAToolStripMenuItem1 = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            salirToolStripMenuItem1 = new ToolStripMenuItem();
            cOMPILARToolStripMenuItem = new ToolStripMenuItem();
            aNALIZARToolStripMenuItem = new ToolStripMenuItem();
            richTextBox1 = new RichTextBox();
            Rtbx_salida = new TextBox();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { aRCHIVOToolStripMenuItem, cOMPILARToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.RightToLeft = RightToLeft.No;
            menuStrip1.Size = new Size(800, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // aRCHIVOToolStripMenuItem
            // 
            aRCHIVOToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem1, nuevoToolStripMenuItem, gUAToolStripMenuItem, gUAToolStripMenuItem1, toolStripSeparator1, salirToolStripMenuItem1 });
            aRCHIVOToolStripMenuItem.Name = "aRCHIVOToolStripMenuItem";
            aRCHIVOToolStripMenuItem.Size = new Size(70, 20);
            aRCHIVOToolStripMenuItem.Text = "ARCHIVO";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(150, 22);
            toolStripMenuItem1.Text = "Nuevo";
            toolStripMenuItem1.Click += toolStripMenuItem1_Click;
            // 
            // nuevoToolStripMenuItem
            // 
            nuevoToolStripMenuItem.Name = "nuevoToolStripMenuItem";
            nuevoToolStripMenuItem.Size = new Size(150, 22);
            nuevoToolStripMenuItem.Text = "Abrir";
            nuevoToolStripMenuItem.Click += AbrirToolStripMenuItem_Click;
            // 
            // gUAToolStripMenuItem
            // 
            gUAToolStripMenuItem.Name = "gUAToolStripMenuItem";
            gUAToolStripMenuItem.Size = new Size(150, 22);
            gUAToolStripMenuItem.Text = "Guardar";
            gUAToolStripMenuItem.Click += gUAToolStripMenuItem_Click;
            // 
            // gUAToolStripMenuItem1
            // 
            gUAToolStripMenuItem1.Name = "gUAToolStripMenuItem1";
            gUAToolStripMenuItem1.Size = new Size(150, 22);
            gUAToolStripMenuItem1.Text = "Guardar como";
            gUAToolStripMenuItem1.Click += GuardarComoToolStripMenuItem1_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(147, 6);
            // 
            // salirToolStripMenuItem1
            // 
            salirToolStripMenuItem1.Name = "salirToolStripMenuItem1";
            salirToolStripMenuItem1.Size = new Size(150, 22);
            salirToolStripMenuItem1.Text = "Salir";
            salirToolStripMenuItem1.Click += salirToolStripMenuItem1_Click;
            // 
            // cOMPILARToolStripMenuItem
            // 
            cOMPILARToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aNALIZARToolStripMenuItem });
            cOMPILARToolStripMenuItem.Name = "cOMPILARToolStripMenuItem";
            cOMPILARToolStripMenuItem.Size = new Size(78, 20);
            cOMPILARToolStripMenuItem.Text = "COMPILAR";
            cOMPILARToolStripMenuItem.Click += cOMPILARToolStripMenuItem_Click;
            // 
            // aNALIZARToolStripMenuItem
            // 
            aNALIZARToolStripMenuItem.Name = "aNALIZARToolStripMenuItem";
            aNALIZARToolStripMenuItem.Size = new Size(130, 22);
            aNALIZARToolStripMenuItem.Text = "ANALIZAR";
            aNALIZARToolStripMenuItem.Click += aNALIZARToolStripMenuItem_Click;
            // 
            // richTextBox1
            // 
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Location = new Point(0, 24);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(800, 426);
            richTextBox1.TabIndex = 1;
            richTextBox1.Text = "";
            richTextBox1.TextChanged += richTextBox1_TextChanged;
            // 
            // Rtbx_salida
            // 
            Rtbx_salida.BackColor = SystemColors.GrayText;
            Rtbx_salida.Dock = DockStyle.Bottom;
            Rtbx_salida.Location = new Point(0, 383);
            Rtbx_salida.Multiline = true;
            Rtbx_salida.Name = "Rtbx_salida";
            Rtbx_salida.Size = new Size(800, 67);
            Rtbx_salida.TabIndex = 2;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(Rtbx_salida);
            Controls.Add(richTextBox1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "Form1";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem aRCHIVOToolStripMenuItem;
        private ToolStripMenuItem nuevoToolStripMenuItem;
        private RichTextBox richTextBox1;
        private ToolStripMenuItem gUAToolStripMenuItem;
        private ToolStripMenuItem gUAToolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem salirToolStripMenuItem1;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem cOMPILARToolStripMenuItem;
        private ToolStripMenuItem aNALIZARToolStripMenuItem;
        private TextBox Rtbx_salida;
    }
}
