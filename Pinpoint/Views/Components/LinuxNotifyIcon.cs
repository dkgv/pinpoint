using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Pinpoint.Views.Components
{
    public class LinuxNotifyIcon : INotifyIcon
    {
#pragma warning disable 67
        public event EventHandler<EventArgs> Click;
        public event EventHandler<EventArgs> DoubleClick;
        public event EventHandler<EventArgs> RightClick;
#pragma warning restore 67

        private LinuxTrayIcon lti;
        private LinuxTrayIcon ltiProp { get => lti; set { lti = value; } }

        private Task trayIconTask;
        private System.Threading.CancellationTokenSource canTok;
        /// <summary>
        /// Gets or sets the icon for the notify icon. Either a file system path
        /// or a <c>resm:</c> manifest resource path can be specified.
        /// </summary>
        private string _iconPath = "";
        public string IconPath { get => _iconPath; set { _iconPath = value; UpdateMenu(); } }
        private string _toolTip = "";

        /// <summary>
        /// Gets or sets the tooltip text for the notify icon.
        /// </summary>
        public string ToolTipText { get => _toolTip; set { _toolTip = value; UpdateMenu(); } }
        private Avalonia.Controls.ContextMenu _menu;
        /// <summary>
        /// Gets or sets the context menu for the notify icon.
        /// </summary>
        public Avalonia.Controls.ContextMenu ContextMenu
        {
            get => _menu; set
            {
                _menu = value;
                UpdateMenu();
            }
        }

        /// <summary>
        /// Gets or sets if the notify icon is visible in the 
        /// taskbar notification area or not.
        /// </summary>
        public bool Visible { get; set; }


        public void Remove()
        {
            Dispatcher.UIThread.Post(() =>
            {
                lti._tray.Hide();
                canTok.Cancel();
            });
        }

        private void UpdateMenu()
        {
            if (!string.IsNullOrEmpty(this.IconPath) && !string.IsNullOrEmpty(this.ToolTipText) && this.ContextMenu != null)
            {
                canTok = new System.Threading.CancellationTokenSource();
                Dispatcher.UIThread.Post(() =>
                {
                    //Because of the way that Linux works this needs to run on its own Thread.
                    trayIconTask = Task.Factory.StartNew(() =>
                    {
                        new Eto.Forms.Application(Eto.Platform.Detect).Run(ltiProp = new LinuxTrayIcon(this.ToolTipText, this.IconPath, this.ContextMenu));
                    }
               , canTok.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                }, DispatcherPriority.MaxValue);
            }
        }
    }
}
