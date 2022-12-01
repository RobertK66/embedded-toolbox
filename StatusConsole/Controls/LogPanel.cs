using ConsoleGUI.Controls;
using StatusConsole;
using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.UserDefined;
using System;
using System.Linq;
using System.Threading;
using ConsoleGUI;
using StatusConsole.Controls;
using System.Collections.Generic;

public class LogPanel : SimpleControl, IInputListener {
	private readonly VerticalStackPanel _stackPanel;
	private readonly VerticalScrollPanel _scrollPanel;
	protected readonly Object monitorObject;
    private Color? timeColor = ConsoleColor.Blue.GetGuiColor();

	public LogPanel(Object monitor, Color? timeCol = null)  {
		_stackPanel = new VerticalStackPanel();
		_scrollPanel = new VerticalScrollPanel() {
			Content = _stackPanel,
			Top = 0

		};
		Content = _scrollPanel;
		monitorObject = monitor;
		this.timeColor = timeCol;
	}


	public void Add(string message, Color? textColor = null) {
		Monitor.Enter(monitorObject);               // This has to be Thread Save! its used by all Logger instances! --> ??? for serial also!?
		List<IControl> childs = new List<IControl>();
		if (timeColor != null) {
			childs.Add(new TextBlock { Text = $"[{DateTime.Now.ToLongTimeString()}] ", Color = timeColor });
		}
		childs.Add(new TextBlock { Text = message, Color = textColor });

		_stackPanel.Add( new WrapPanel {
			Content = 
			new HorizontalStackPanel {
				Children = childs
			}
		});
		_scrollPanel.Top = _stackPanel.Children.Sum(x => x.Size.Height) - this.Size.Height;
		Monitor.Exit(monitorObject);
	}

	public void Clear() {
		Monitor.Enter(monitorObject);                // This has to be Thread Save! its used by all Logger instances! --> ??? for serioal also!?
		var c = _stackPanel.Children.ToList();
		foreach (var item in c) {
			_stackPanel.Remove(item);
		}
		_scrollPanel.Top = _stackPanel.Children.Sum(x => x.Size.Height) - this.Size.Height;
		Monitor.Exit(monitorObject);
	}

	public void OnInput(InputEvent inputEvent) {
		if (inputEvent.Key.Key == ConsoleKey.DownArrow) {
			_scrollPanel.Top += 1;
		} else if (inputEvent.Key.Key == ConsoleKey.UpArrow) {
			_scrollPanel.Top -= 1;
		} else if (inputEvent.Key.Key == ConsoleKey.PageDown) {
			_scrollPanel.Top += this.Size.Height;
		} else if (inputEvent.Key.Key == ConsoleKey.PageUp) {
			_scrollPanel.Top -= this.Size.Height;
		}
    }
}

