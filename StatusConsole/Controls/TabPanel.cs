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
		private Color colActive = new Color(25, 54, 65);
		private Color textCol = new Color(0, 0, 0);

		public IControl Header { get; }
		public Background Content { get; }

		public Tab(string name, IControl content, Color? colA = null, Color? colI = null, Color? tC = null) {
			colActive = colA ?? colActive;
			colInactive = colI ?? colInactive;
			textCol = tC ?? textCol;

			hederBackground = new Background {
				Content = new Margin {
					Offset = new Offset(1, 0, 1, 0),
					Content = new Style() { Content = new TextBlock { Text = name }, Foreground = textCol }
				}
			};

			Header = new Margin {
					Offset = new Offset(0, 0, 1, 0),
					Content = hederBackground
				};
			Content = new Background {
				//Content = new Style() {
					Content = content,
//					Foreground = textCol
				//}, 
			    Color = colActive
			};
									
			MarkAsInactive();
		}

		public void MarkAsActive() { 
			hederBackground.Color = colActive;
			//Content.Color = colActive;
		}
		public void MarkAsInactive() {
			hederBackground.Color = colInactive;
			//Content.Color = colInactive;

		} 
	}

	private readonly List<Tab> tabs = new List<Tab>();
	private readonly DockPanel wrapper;
	private readonly HorizontalStackPanel tabsPanel;

	private Tab? currentTab;

	public event EventHandler<TabSwitchedArgs> TabSwitched;

	public TabPanel(Color? unselectedCol = null) {
		tabsPanel = new HorizontalStackPanel();
		
		wrapper = new DockPanel {
			Placement = DockPanel.DockedControlPlacement.Top,
			DockedControl = new Background {
				Color = unselectedCol ?? new Color(25, 25, 52),
				Content = new Boundary {
					MinHeight = 1,
					MaxHeight = 1,
					Content = tabsPanel
				}
			}
		};

		Content = wrapper;
	}

	public void AddTab(string name, IControl content, Color? headerCol = null, Color? headerColInactive = null, Color? textCol = null ) {
		var newTab = new Tab(name, content, headerCol, headerColInactive, textCol);
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
		TabSwitched?.Invoke(this, new TabSwitchedArgs(tab));
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

public class TabSwitchedArgs {
	public int selectedIdx;

    public TabSwitchedArgs(int tab) {
		selectedIdx = tab;
    }
}