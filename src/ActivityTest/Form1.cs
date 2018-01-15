using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;

namespace ActivityTest
{
    public partial class Form1 : Form
    {
        private bool _isMouseMoveActive;
        private bool _isMouseClickActive;
        private bool _isMouseWheelkActive;
        private bool _isKeyouardActive;
        private readonly object _lockObjets = new object();

        private MouseEventArgs _lastMouseEventArgs;
        private MouseEventArgs _prevMouseEventArgs;

        IKeyboardMouseEvents _globalHook;

        private bool _interrupted;


        public Form1()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _globalHook = Hook.GlobalEvents();
            _globalHook.MouseClick += GlobalHook_MouseClick;
            _globalHook.MouseMove += GlobalHook_MouseMove;
            _globalHook.MouseWheel += GlobalHook_MouseWheel;
            _globalHook.KeyPress += GlobalHook_KeyPress;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _interrupted = true;
            _globalHook.MouseClick -= GlobalHook_MouseClick;
            _globalHook.MouseMove -= GlobalHook_MouseMove;
            _globalHook.MouseWheel -= GlobalHook_MouseWheel;
            _globalHook.KeyPress -= GlobalHook_KeyPress;

            _globalHook.Dispose();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;

            var thread = new Thread(() =>
            {
                try
                {
                    Test();
                }
                catch (Exception)
                {
                    // ignored
                }
            });

            MessageBox.Show(@"Please try to NOT MOVE YOUR MOUSE or press keys before this test is done", @"Start",
            MessageBoxButtons.OK, MessageBoxIcon.Information);

            thread.Start();
        }


        private void GlobalHook_KeyPress(object sender, KeyPressEventArgs e)
        {
            lock (_lockObjets)
                _isKeyouardActive = true;
        }

        private void GlobalHook_MouseMove(object sender, MouseEventArgs e)
        {
            lock (_lockObjets)
            {
                _isMouseMoveActive = true;
                _lastMouseEventArgs = e;
            }
        }

        private void GlobalHook_MouseWheel(object sender, MouseEventArgs e)
        {
            lock (_lockObjets)
                _isMouseWheelkActive = true;
        }

        private void GlobalHook_MouseClick(object sender, MouseEventArgs e)
        {
            lock (_lockObjets)
                _isMouseClickActive = true;
        }


        private void Test()
        {
            double counter = 59;
            var sBuilder = new StringBuilder();

            var startTime = DateTime.Now;
            var startText =
                $"{DateTime.Now:G}: Starting...\r\n---------------------------------------------------\r\n";
            textBox1.BeginInvoke((MethodInvoker)delegate { textBox1.AppendText(startText); });
            sBuilder.Append(startText);

            var systemInfo = $"System info: {Environment.OSVersion}/{Environment.Version}/64bit:{Environment.Is64BitOperatingSystem}\r\n";
            textBox1.BeginInvoke((MethodInvoker)delegate { textBox1.AppendText(systemInfo); });
            sBuilder.Append(systemInfo);

            textBox1.BeginInvoke((MethodInvoker)delegate { textBox1.AppendText("---------------------------------------------------\r\n"); });
            sBuilder.Append("---------------------------------------------------\r\n");


            textBox1.BeginInvoke((MethodInvoker)delegate { textBox1.AppendText("Processes...\r\n"); });
            sBuilder.Append("Processes...\r\n");

            foreach (var process in System.Diagnostics.Process.GetProcesses().OrderBy(p => p.ProcessName).GroupBy(p => p.ProcessName))
            {
                textBox1.BeginInvoke((MethodInvoker)delegate { textBox1.AppendText($"{process.Key}\r\n"); });
                sBuilder.Append($"{process.Key}\r\n");
            }

            textBox1.BeginInvoke((MethodInvoker)delegate { textBox1.AppendText("---------------------------------------------------\r\n"); });
            sBuilder.Append("---------------------------------------------------\r\n");

            while (counter >= 0 && !_interrupted)
            {
                var count = counter;
                //label1.BeginInvoke((MethodInvoker)delegate { label1.Text = $@"00:{(int)count:D2}"; });

                textBox1.BeginInvoke((MethodInvoker)delegate
               {
                   lock (_lockObjets)
                   {
                       var mousePositionInfo = "/";
                       if (_isMouseMoveActive && _lastMouseEventArgs != null)
                       {
                           if (_prevMouseEventArgs != null)
                               mousePositionInfo =
                                   $"(Δx:{Math.Abs(_lastMouseEventArgs.X - _prevMouseEventArgs.X)}/Δy:{Math.Abs(_lastMouseEventArgs.Y - _prevMouseEventArgs.Y)})/";

                           _prevMouseEventArgs = _lastMouseEventArgs;
                       }

                       var info =
                           $"{LastInputInfo.GetUserInactiveTime()} / {LastInputInfo.GetUserInactiveTime2()} / {LastInputInfo.TickCount()} / {LastInputInfo.UnsignedTickCount()} / {LastInputInfo.InputInfo()} / {LastInputInfo.GetTickCount_32()} / {LastInputInfo.GetTickCount_64()} /  {_isKeyouardActive} / {_isMouseClickActive} / {_isMouseMoveActive} {mousePositionInfo}  {_isMouseWheelkActive}\r\n";
                       textBox1.AppendText(info);
                       sBuilder.Append(info);

                       _isKeyouardActive = _isMouseClickActive = _isMouseMoveActive = _isMouseWheelkActive = false;
                   }

               });

                Thread.Sleep(500);
                counter -= 0.5;
            }

            var endText =
                $"{DateTime.Now:G}: End test...\r\n---------------------------------------------------\r\n";
            textBox1.BeginInvoke((MethodInvoker)delegate { textBox1.AppendText(endText); });
            sBuilder.Append(endText);


            textBox1.BeginInvoke((MethodInvoker)delegate { textBox1.AppendText("Sending report...\r\n"); });

            var fileName = ReportSender(startTime, sBuilder.ToString());

            if (fileName == null)
                textBox1.BeginInvoke((MethodInvoker)delegate
               {
                   textBox1.AppendText("Report sent...OK.\n\nFinished.");
               });
            else
                textBox1.BeginInvoke((MethodInvoker)delegate
               {
                   textBox1.AppendText(
                       "Report sent...ERROR.\r\n" +
                       $"Please send  '{fileName}' file to ScreenshotmMonitor support.\r\n");
               });
        }

        private static string ReportSender(DateTime startTime, string data)
        {
            var fileName = $@"{Environment.CurrentDirectory}\{
                    startTime.ToString("g").Replace(".", "-").Replace("/", "-").Replace(":", "-")
                }.log";

            File.WriteAllText(fileName, data);

            using (var client = new WebClient())
            {
                try
                {
                    var text = $@"Activity test. Auto. [{Environment.MachineName}/{Environment.UserName}]";
                    client.UploadFile(new Uri(
                        $@"http://screenshotmonitor.com/ReportAppError?version=0&log={text}"), fileName);
                }
                catch
                {
                    return fileName;
                }
            }

            if (File.Exists(fileName))
                File.Delete(fileName);

            return null;
        }
    }
}
