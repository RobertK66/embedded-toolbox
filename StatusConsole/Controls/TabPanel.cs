using ConsoleGUI;
using ConsoleGUI.Controls;
using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;
using ConsoleGUI.UserDefined;
using System;
using System.Collections.Generic;

internal class TabPanel : SimpleControl, IInputListener {
	private class Tab {
		private readonly Background hederBackground;
		private Color colInactive = new Color(65, 24, 25);
		private Color	colActive = new Color(25, 54, 65);

		public IControl Header { get; }
		public IControl Content { get; }

		public Tab(string name, IControl content, Color? colA = null, Color? colI = null) {
			colActive = colA ?? colActive;
			colInactive = colI ?? colInactive;
			hederBackground = new Background {
				Content = new Margin {
					Offset = new Offset(1, 0, 1, 0),
					Content = new TextBlock { Text = name }
				}
			};

			Header = new Margin {
				Offset = new Offset(0, 0, 1, 0),
				Content = hederBackground
			};
			Content = content;

			MarkAsInactive();
		}

		public void MarkAsActive() => hederBackground.Color = colActive;
		public void MarkAsInactive() => hederBackground.Color = colInactive;
	}

	private readonly List<Tab> tabs = new List<Tab>();
	private readonly DockPanel wrapper;
	private readonly HorizontalStackPanel tabsPanel;

	private Tab? currentTab;

	public TabPanel() {
		tabsPanel = new HorizontalStackPanel();

		wrapper = new DockPanel {
			Placement = DockPanel.DockedControlPlacement.Top,
			DockedControl = new Background {
				Color = new Color(25, 25, 52),
				Content = new Boundary {
					MinHeight = 1,
					MaxHeight = 1,
					Content = tabsPanel
				}
			}
		};

		Content = wrapper;
	}

	public void AddTab(string name, IControl content, Color? headerCol = null) {
		var newTab = new Tab(name, content, headerCol);
		tabs.Add(newTab);
		tabsPanel.Add(newTab.Header);
		if (tabs.Count == 1)
			SelectTab(0);
	}

	public void SelectTab(int tab) {
		currentTab?.MarkAsInactive();
		currentTab = tabs[tab];
		currentTab.MarkAsActive();
		wrapper.FillingControl = currentTab.Content;
	}

	public void OnInput(InputEvent inputEvent) {
		if (inputEvent.Key.Key == ConsoleKey.Tab) {
			if (currentTab != null) {
				SelectTab((tabs.IndexOf(currentTab) + 1) % tabs.Count);
				inputEvent.Handled = true;
			}
		} 
	}
}

