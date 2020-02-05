using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using SysTimer = System.Timers;

//Evento Control.Resize
//    http://msdn.microsoft.com/pt-br/library/system.windows.forms.control.resize(v=vs.110).aspx
//LifeCycle
//    http://msdn.microsoft.com/en-us/library/86faxx0d(v=vs.110).aspx
//    http://www.c-sharpcorner.com/uploadfile/mamta_m/windows-forms-events-lifecycle
//    http://stackoverflow.com/questions/1575508/in-what-order-do-net-windows-forms-events-fire

namespace TimeSheet
{
    /// <summary>
    /// A hora que você chegar, você executa o App, ele inicia no Tray, guardando a hora que foi iniciado.
    /// Quando ele completar uma hora de execução ele mostra o Form solicitando a descrição da atividade e continua
    /// com o mesmo processo até que o CheckBox "Última Atividade" seja checkado, assim após uma hora de execução
    /// ele sincroniza todas as atividades com a planilha e se fecha automaticamente.
    /// OBS: Este App levou 1 dia (8 horas) para ser desenvolvido, com duas pesquisas:
    ///     Como para manipular o Timer
    ///     Como minimizar Apps no System Tray
    /// </summary>
    public partial class Form1 : Form
    {
        SysTimer.Timer timerDinamico = new SysTimer.Timer();
        bool flagCarregamentoSecundario = false;
        int intervalo = 15000;

        //Para ocultar completamente o Form da Task Bar tem que desabilitar a propriedade ShowInTaskBar,
        //senão com a propriedade WindowsState ele ficará somente minimizado! Só que sempre que o form
        //for minimizado ele ficará oculto da Task Bar!
        public Form1()
        {
            InitializeComponent();
            AtribuirLabels();
            CriarMenuNotify();
        }

        void AtribuirLabels()
        {
            lblData.Text = DateTime.Now.ToShortDateString();
            lblHora.Text = DateTime.Now.ToShortTimeString();
        }

        void IniciarTimer(int intervaloTimer)
        {
            timerDinamico.Interval = intervaloTimer;
            timerDinamico.Start();
        }

        void PararTimer(int intervaloTimer)
        {
            timerDinamico.Interval = intervaloTimer;
            timerDinamico.Enabled = false;
            timerDinamico.Stop();
        }

        void CriarMenuNotify()
        {
            MenuItem configurarMenuItem = new MenuItem("Configuração");
            MenuItem sairMenuItem = new MenuItem("Sair", new EventHandler(exitToolStripMenuItem_Click));
            notifyIcon1.ContextMenu = new ContextMenu(new MenuItem[] { configurarMenuItem, sairMenuItem });
        }

        void MostrarForm()
        {
            //flagCarregamentoSecundario = true;
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            //this.Visible = true;
            this.Activate();
            //this.Show();
        }

        void EventoElapsed(object sender, SysTimer.ElapsedEventArgs e)
        {
            //lblResultado.Text = DateTime.Now.ToString("o");
            //MessageBox.Show("Informe a descrição da atividade!", "Definir Atividade", MessageBoxButtons.OK, MessageBoxIcon.Question);

            //Se usar this.Show() neste ponto vai ocorrer o erro:
            //  Cross-thread operation not valid: Control accessed from a thread other than the thread it was created on
            //Portanto é necessário fazer o tratamento abaixo:
            this.Invoke(new MethodInvoker(MostrarForm));
            //http://stackoverflow.com/questions/142003/cross-thread-operation-not-valid-control-accessed-from-a-thread-other-than-the
        }

        #region System Tray
        //http://www.codeproject.com/Articles/27599/Minimize-window-to-system-tray
        //http://www.codeproject.com/Articles/18683/Creating-a-Tasktray-Application
        //http://stackoverflow.com/questions/7625421/minimize-app-to-system-tray
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// Toda hora que ocorre o evento "Resize" do Form este método é executado!
        ///     Quando o Form é minimizado!
        ///     Application.Run(new Form1());
        ///     this.ShowInTaskbar = true; - O método TrayMinimizerForm_Resize() é executado duas vezes!
        ///     this.WindowState = FormWindowState.Normal;
        ///     MessageBox.Show();
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrayMinimizerForm_Resize(object sender, EventArgs e)
        {
            //notifyIcon1.Visible = true;
            notifyIcon1.BalloonTipTitle = "Time Sheet minimizado no Tray App!";
            notifyIcon1.BalloonTipText = "A cada uma hora será solicitada a descrição da atividade.";
            notifyIcon1.ShowBalloonTip(500);

            //Se o Form foi minimizado e o Timer ainda não foi iniciado...
            if (this.WindowState == FormWindowState.Minimized && timerDinamico.Interval == 100)
            {
                //http://stackoverflow.com/questions/1329900/net-event-every-minute-on-the-minute-is-a-timer-the-best-option
                timerDinamico.AutoReset = false;
                timerDinamico.Elapsed += new SysTimer.ElapsedEventHandler(EventoElapsed);

                IniciarTimer(intervalo);
            }
            else if (this.WindowState == FormWindowState.Normal)
                PararTimer(100);
            //else if (this.WindowState == FormWindowState.Minimized)
            //    this.ShowInTaskbar = false;

            //if (!flagCarregamentoSecundario)
            //{
            //    if (FormWindowState.Minimized == this.WindowState)
            //    {
            //        notifyIcon1.Visible = true;
            //        notifyIcon1.ShowBalloonTip(500);
            //        this.Hide();
            //    }
            //    else if (FormWindowState.Normal == this.WindowState)
            //    {
            //        notifyIcon1.Visible = false;
            //    }
            //}
        }
        #endregion

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
        }
    }
}