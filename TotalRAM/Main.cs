using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Timers;
using Microsoft.VisualBasic.Devices;

namespace TotalRAM
{
    public partial class Main : Form
    {
        //Declarando as variaveis
        private int X = 0;
        private int Y = 0;

        private ComputerInfo info;
        private System.Timers.Timer time;

        /*
            Metodo Construtor
        */
        public Main()
        {
            InitializeComponent();

            //Adicionando eventos no Windowns form Main
            this.ShowInTaskbar = false;
            this.MouseDown += new MouseEventHandler(Form1_MouseDown);
            this.MouseMove += new MouseEventHandler(Form1_MouseMove);
            label1.MouseDown += new MouseEventHandler(Form1_MouseDown);
            label1.MouseMove += new MouseEventHandler(Form1_MouseMove);
            progressBar1.MouseDown += new MouseEventHandler(Form1_MouseDown);
            progressBar1.MouseMove += new MouseEventHandler(Form1_MouseMove);
            progressBar1.MouseEnter += new EventHandler(Main_MouseEnter);
            label1.MouseEnter += new EventHandler(Main_MouseEnter);

            //Adicionando Cor, Em cima de tudo e posição
            this.BackColor = System.Drawing.Color.Green;
            this.TopMost = true;
            this.Location = new System.Drawing.Point((Screen.PrimaryScreen.Bounds.Width / 2) - (this.Width / 2), -50);
        }

        /*
            Evento quando o Form é iniciado
        */
        private void Form1_Load(object sender, EventArgs e)
        {
            //Instanciando uma variavel do Objeto ComputerInfo;
            info = new ComputerInfo();

            //Iniciando meu Timer Para poder atualizar o percentual de Ram utilizada
            time = new System.Timers.Timer(1000);
            time.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            time.Enabled = true;
            time.Start();
        }

        /*
            Metodo que atualiza as informações no Form
        */
        private void getInformationWindows()
        {
            try
            {
                this.Invoke((Action) delegate () {
                    int value = progressValue(info.TotalPhysicalMemory, info.AvailablePhysicalMemory);
                    if (value <= 35)
                    {
                        this.BackColor = System.Drawing.Color.Blue;
                    }
                    else if (value <= 50)
                    {
                        this.BackColor = System.Drawing.Color.DarkGreen;
                    }
                    else if (value >= 51)
                    {
                        this.BackColor = System.Drawing.Color.DarkOrange;
                    }
                    else if (value >= 80)
                    {
                        this.BackColor = System.Drawing.Color.DarkRed;
                    }
                    label1.Text = value + "%";
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = 100;
                    progressBar1.Value = value;
                    Application.DoEvents();
                });
            }
            catch (Win32Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /*
            Converter byte em giga
        */
        private Double byte2giga(ulong ram)
        {
            Double gb = 0;

            gb = ram / 1024;
            gb = gb / 1024;
            gb = gb / 1024;

            return gb;
        }

        /*
            Metodo para cortar a string para poder fazer as contas
                do percentual de Ram
        */
        private String cropString(Double ram)
        {
            return ram.ToString().Substring(0, 5).Replace(",", "");
        }

        /*
            Convertendo ulong em Int32
                para poder setar as informações no Form
        */
        private Int32 progressValue(ulong ram, ulong virtua)
        {
            Int32 total = 0;
            Int32 r1 = Int32.Parse(cropString(byte2giga(ram)));
            Int32 r2 = Int32.Parse(cropString(byte2giga(virtua)));

            total = (r2 * 100) / r1;

            return 100 - total;
        }

        /*
            Metodo que é executando quando o após o Timer ser executado
                Faz com que as informações no Form sejam atualizadas de
                1 em 1 Segundo
        */
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            getInformationWindows();
        }

        /*
            Metodo que verifica o que deve ser feito quando for clicado com o mouse
                BT Direito:
                    Mover a tela (Implementação em andamento)
                BT Esquerdo:
                    Mostrar um ContextMenu com algumas informações como
                        Sobre o programa ou Sair do programa.
        */
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                ContextMenu menu = new ContextMenu();
                this.ContextMenu = menu;

                MenuItem item1 = new MenuItem();
                item1.Text = "Sobre o TOTALRam";
                item1.Click += new EventHandler(menuItem1_Click);

                MenuItem item2 = new MenuItem();
                item2.Text = "Sair";
                item2.Click += new EventHandler(menuItem2_Click);

                menu.MenuItems.Add(item1);
                menu.MenuItems.Add(item2);

                menu.Show(this, new System.Drawing.Point(e.Location.X, e.Location.Y));
            }
            else
            {
                X = this.Left - MousePosition.X;
                Y = this.Top - MousePosition.Y;
            }
        }

        /*
            Para poder mover a tela (Implementação em andamento)
        */
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            this.Left = X + MousePosition.X;
            this.Top = Y + MousePosition.Y;
        }

        /*
            Quando o MenuItem do Context menu form Clicado mostrar a tela de informações
        */
        private void menuItem1_Click(object sender, System.EventArgs e)
        {
            new Information().Show();
        }

        /*
            Quando o MenuItem do Context menu form Clicado fechar o programa
        */
        private void menuItem2_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        /*
            Verificar que quando o mouse entrar no form ele exibir o form inteiro
        */
        private void Main_MouseEnter(object sender, EventArgs e)
        {
            this.Location = new System.Drawing.Point((Screen.PrimaryScreen.Bounds.Width / 2) - (this.Width / 2), 0);
        }

        /*
            Verificar que quando o mouse entrar no form ele esconder o form 
                e exibir somente a ProgressBar
        */
        private void Main_MouseLeave(object sender, EventArgs e)
        {
            this.Location = new System.Drawing.Point((Screen.PrimaryScreen.Bounds.Width / 2) - (this.Width / 2), -50);
        }

        /*
            Quando o Form estiver fechado ele executa essas verificações para poder fechar as threads
                e poder fechar o Programa
        */
        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                time = null;
                time.EndInit();
                time.Stop();
                System.Threading.Thread.CurrentThread.Abort();
                System.Diagnostics.Process.GetCurrentProcess().Close();
            }
            catch (Exception ex)
            {
                if (ex is System.Threading.ThreadStateException || ex is System.Security.SecurityException
                 || ex is System.Threading.ThreadAbortException || ex is System.NullReferenceException)
                {
                    Console.WriteLine("TOTALRAM Error:" + ex.Message);
                }
            }
        }

        /*
            Classe genérica para poder exibir a ProgressBar na Vertical
        */
        public class VerticalProgressBar : ProgressBar
        {
            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    // 0x04 Vertical
                    // 0x01 Horizontal
                    cp.Style |= 0x01;
                    return cp;
                }
            }
        }
    }
}
