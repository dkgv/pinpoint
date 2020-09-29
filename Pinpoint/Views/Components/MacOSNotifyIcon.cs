using System;
using MonoMac.AppKit;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;

namespace Pinpoint.Views.Components
{
    public class NotifyIcon : INotifyIcon
    {
        private NSStatusItem _item;
        private NSStatusItem statusBarItem
        {
            get => _item; set
            {
                _item = value;
                UpdateMenu();
            }
        }

#pragma warning disable 67 // Get rid of event "not used" warnings
        public event EventHandler<EventArgs> Click;
        public event EventHandler<EventArgs> DoubleClick;
        public event EventHandler<EventArgs> RightClick;
#pragma warning restore 67


        /// <summary>
        /// Updates the Tray Menu Item settings, ToolTip, ContextMenu, Image etc on MacOSX
        /// </summary>
        private void UpdateMenu()
        {
            if (_item != null)
            {
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                _item.Image = NSImage.FromStream(assets.Open(new Uri(IconPath)));
                _item.ToolTip = this.ToolTipText;
                if (statusBarItem.Menu == null)
                    statusBarItem.Menu = new NSMenu();
                else
                {
                    statusBarItem.Menu.RemoveAllItems();
                }
                foreach (var x in _menu.Items.Cast<MenuItem>())
                {
                    NSMenuItem menuItem = new NSMenuItem(x.Header.ToString());
                    menuItem.Activated += (s, e) => { x.Command.Execute(null); };
                    statusBarItem.Menu.AddItem(menuItem);
                }
                statusBarItem.DoubleClick += (s, e) => { DoubleClick?.Invoke(this, new EventArgs()); };
            }
        }
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
        private ContextMenu _menu;
        /// <summary>
        /// Gets or sets the context menu for the notify icon.
        /// </summary>
        public ContextMenu ContextMenu
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
            this.statusBarItem.Dispose();
        }


        /// <summary>
        /// Creates a new <c>NotifyIcon</c> instance and sets up some 
        /// required resources.
        /// </summary>

        public NotifyIcon()
        {
            Dispatcher.UIThread.Post(() =>
            {
                var systemStatusBar = NSStatusBar.SystemStatusBar;
                statusBarItem = systemStatusBar.CreateStatusItem(30);
                statusBarItem.ToolTip = this.ToolTipText;
            }, DispatcherPriority.MaxValue);
        }
    }
}
