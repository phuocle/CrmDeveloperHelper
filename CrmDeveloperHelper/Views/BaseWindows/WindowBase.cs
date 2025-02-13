﻿using Microsoft.Win32;
using Microsoft.Xrm.Sdk;
using Nav.Common.VSPackages.CrmDeveloperHelper.Helpers;
using Nav.Common.VSPackages.CrmDeveloperHelper.Interfaces;
using Nav.Common.VSPackages.CrmDeveloperHelper.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Nav.Common.VSPackages.CrmDeveloperHelper.Views
{
    public abstract class WindowBase : Window
    {
        private object _syncObject = new object();

        private bool loaded = false;

        private int _initCounter = 0;

        protected WindowBase()
        {
            var winConfig = this.GetWindowsSettings();

            LoadConfiguration(winConfig);

            var binding = new CommandBinding(NavigationCommands.Refresh);
            binding.Executed += this.Refresh_Executed;

            this.CommandBindings.Add(binding);
        }

        protected void SetInputLanguageEnglish()
        {
            CultureInfo cultureInfo = GetEnglishCultureInfo();

            if (cultureInfo != null)
            {
                InputLanguageManager.SetInputLanguage(this, cultureInfo);
            }
        }

        private static CultureInfo _englishCultureInfo;
        private static CultureInfo GetEnglishCultureInfo()
        {
            if (_englishCultureInfo != null)
            {
                return _englishCultureInfo;
            }

            if (InputLanguageManager.Current != null)
            {
                var availableInputLanguages = InputLanguageManager.Current.AvailableInputLanguages.OfType<CultureInfo>().ToList();

                foreach (var cultureInfo in availableInputLanguages)
                {
                    if (cultureInfo.Name.StartsWith("en-", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (_englishCultureInfo == null)
                        {
                            _englishCultureInfo = cultureInfo;
                        }

                        return cultureInfo;
                    }
                }
            }

            return null;
        }

        private async void Refresh_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await OnRefreshList(e);
        }

        protected virtual Task OnRefreshList(ExecutedRoutedEventArgs e)
        {
            return Task.CompletedTask;
        }

        protected bool IsControlsEnabled => this._initCounter <= 0;

        protected void ChangeInitByEnabled(bool enabled)
        {
            lock (_syncObject)
            {
                if (enabled)
                {
                    if (this._initCounter > 0)
                    {
                        this._initCounter--;
                    }
                }
                else
                {
                    this._initCounter++;
                }
            }
        }

        protected void IncreaseInit()
        {
            lock (_syncObject)
            {
                this._initCounter++;
            }
        }

        protected void DecreaseInit()
        {
            lock (_syncObject)
            {
                if (this._initCounter > 0)
                {
                    this._initCounter--;
                }
            }
        }

        protected WindowSettings GetWindowsSettings()
        {
            var name = this.Name;

            if (string.IsNullOrEmpty(name))
            {
                name = this.GetType().Name;
            }

            return FileOperations.GetWindowConfiguration(name);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (!loaded)
            {
                loaded = true;

                var winConfig = this.GetWindowsSettings();

                LoadConfiguration(winConfig);

                LoadConfigurationInternal(winConfig);
            }
        }

        private void LoadConfiguration(WindowSettings winConfig)
        {
            if (winConfig.Top.HasValue)
            {
                this.Top = winConfig.Top.Value;
                this.WindowStartupLocation = WindowStartupLocation.Manual;
            }

            if (winConfig.Left.HasValue)
            {
                this.Left = winConfig.Left.Value;
                this.WindowStartupLocation = WindowStartupLocation.Manual;
            }

            if (winConfig.WindowState != WindowState.Minimized)
            {
                this.WindowState = winConfig.WindowState;
            }

            if (this.ResizeMode != System.Windows.ResizeMode.CanMinimize
                && this.ResizeMode != System.Windows.ResizeMode.NoResize
            )
            {
                if (winConfig.Width.HasValue)
                {
                    this.Width = winConfig.Width.Value;
                    this.WindowStartupLocation = WindowStartupLocation.Manual;
                }

                if (winConfig.Height.HasValue)
                {
                    this.Height = winConfig.Height.Value;
                    this.WindowStartupLocation = WindowStartupLocation.Manual;
                }
            }

            foreach (var item in FindChildren<ListView>(this))
            {
                LoadListViewColumnsWidths(item, winConfig);
            }

            foreach (var item in FindChildren<DataGrid>(this))
            {
                LoadDataGridColumnsWidths(item, winConfig);
            }
        }

        protected static IEnumerable<T> FindChildren<T>(DependencyObject source) where T : DependencyObject
        {
            if (source != null)
            {
                var childs = GetChildObjects(source);
                foreach (DependencyObject child in childs)
                {
                    //analyze if children match the requested type
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    //recurse tree
                    foreach (T descendant in FindChildren<T>(child))
                    {
                        yield return descendant;
                    }
                }
            }
        }

        protected static IEnumerable<DependencyObject> GetChildObjects(DependencyObject parent)
        {
            if (parent == null) yield break;

            if (parent is ContentElement || parent is FrameworkElement)
            {
                //use the logical tree for content / framework elements
                foreach (object obj in LogicalTreeHelper.GetChildren(parent))
                {
                    if (obj is DependencyObject depObj)
                    {
                        yield return depObj;
                    }
                }
            }
            else
            {
                //use the visual tree per default
                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    yield return VisualTreeHelper.GetChild(parent, i);
                }
            }
        }

        private void SaveConfiguration()
        {
            var winConfig = this.GetWindowsSettings();

            winConfig.WindowState = this.WindowState;

            if (this.WindowState == System.Windows.WindowState.Normal)
            {
                winConfig.Top = this.Top;
                winConfig.Left = this.Left;

                if (this.ResizeMode != System.Windows.ResizeMode.CanMinimize
                    && this.ResizeMode != System.Windows.ResizeMode.NoResize
                )
                {
                    winConfig.Width = this.ActualWidth;
                    winConfig.Height = this.ActualHeight;
                }
            }

            SaveConfigurationInternal(winConfig);

            foreach (var item in FindChildren<ListView>(this))
            {
                SaveListViewColumnsWidths(item, winConfig);
            }

            foreach (var item in FindChildren<DataGrid>(this))
            {
                SaveDataGridColumnsWidths(item, winConfig);
            }

            winConfig.Save();
        }

        private void LoadListViewColumnsWidths(ListView listView, WindowSettings winConfig)
        {
            if (winConfig.GridViewColumnsWidths == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(listView.Name))
            {
                return;
            }

            if (!(listView.View is GridView gridView))
            {
                return;
            }

            foreach (var column in gridView.Columns)
            {
                if (column.Header is string)
                {
                    if (!string.IsNullOrEmpty(column.Header.ToString()))
                    {
                        var name = string.Format("{0}.{1}", listView.Name, column.Header.ToString());

                        if (winConfig.GridViewColumnsWidths.ContainsKey(name))
                        {
                            column.Width = winConfig.GridViewColumnsWidths[name];
                        }
                    }
                }
            }
        }

        private void SaveListViewColumnsWidths(ListView listView, WindowSettings winConfig)
        {
            if (string.IsNullOrEmpty(listView.Name))
            {
                return;
            }

            if (!(listView.View is GridView gridView))
            {
                return;
            }

            foreach (var column in gridView.Columns)
            {
                if (column.Header is string)
                {
                    if (!string.IsNullOrEmpty(column.Header.ToString()))
                    {
                        var name = string.Format("{0}.{1}", listView.Name, column.Header.ToString());

                        winConfig.GridViewColumnsWidths[name] = column.ActualWidth;
                    }
                }
            }
        }

        protected void LoadDataGridColumnsWidths(DataGrid dataGrid, WindowSettings winConfig)
        {
            if (winConfig.GridViewColumnsWidths == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(dataGrid.Name))
            {
                return;
            }

            foreach (var column in dataGrid.Columns)
            {
                if (column.Header is string)
                {
                    if (!string.IsNullOrEmpty(column.Header.ToString()))
                    {
                        var name = string.Format("{0}.{1}", dataGrid.Name, column.Header.ToString());

                        if (winConfig.GridViewColumnsWidths.ContainsKey(name))
                        {
                            column.Width = winConfig.GridViewColumnsWidths[name];
                        }
                    }
                }
            }
        }

        protected void SaveDataGridColumnsWidths(DataGrid dataGrid, WindowSettings winConfig)
        {
            if (string.IsNullOrEmpty(dataGrid.Name))
            {
                return;
            }

            foreach (var column in dataGrid.Columns)
            {
                if (column.Header is string)
                {
                    if (!string.IsNullOrEmpty(column.Header.ToString()))
                    {
                        var name = string.Format("{0}.{1}", dataGrid.Name, column.Header.ToString());

                        winConfig.GridViewColumnsWidths[name] = column.ActualWidth;
                    }
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            SaveConfiguration();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if (e.Key == Key.Escape
                    || (e.Key == Key.W && e.KeyboardDevice != null && (e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0)
                    )
                {
                    if (CanCloseWindow(e))
                    {
                        e.Handled = true;

                        this.Close();

                        return;
                    }
                }
            }

            base.OnKeyDown(e);
        }

        protected virtual bool CanCloseWindow(KeyEventArgs e)
        {
            return true;
        }

        protected virtual void LoadConfigurationInternal(WindowSettings winConfig)
        {

        }

        protected virtual void SaveConfigurationInternal(WindowSettings winConfig)
        {

        }

        public static void ActivateControls(IEnumerable<Control> items, bool isEnabled, params string[] uidList)
        {
            if (uidList == null || uidList.Length == 0)
            {
                return;
            }

            HashSet<string> hash = new HashSet<string>(uidList, StringComparer.InvariantCultureIgnoreCase);

            foreach (var item in items)
            {
                if (hash.Contains(item.Uid))
                {
                    item.IsEnabled = isEnabled;
                    item.Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public static void CheckSeparatorVisible(IEnumerable<Control> items)
        {
            int index = 0;

            foreach (var item in items)
            {
                if (item is Separator separator
                    && separator.Visibility == Visibility.Visible
                )
                {
                    bool hasNextVisible = items.Skip(index + 1).Any(c => c.Visibility == Visibility.Visible);

                    if (!hasNextVisible)
                    {
                        separator.Visibility = Visibility.Collapsed;
                    }
                }

                index++;
            }
        }

        public static void SetControlsName(IEnumerable<Control> items, string name, params string[] uidList)
        {
            if (uidList == null || uidList.Length == 0)
            {
                return;
            }

            HashSet<string> hash = new HashSet<string>(uidList, StringComparer.InvariantCultureIgnoreCase);

            foreach (var item in items.OfType<MenuItem>())
            {
                if (hash.Contains(item.Uid))
                {
                    item.Header = name;
                }
            }
        }

        public static void AddMenuItemClickHandler(ContextMenu contextMenu, RoutedEventHandler clickHandler, params string[] uidList)
        {
            if (uidList == null || uidList.Length == 0)
            {
                return;
            }

            var items = contextMenu.Items.OfType<Control>();

            AddMenuItemClickHandler(items, clickHandler, uidList);
        }

        public static void AddMenuItemClickHandler(IEnumerable<Control> items, RoutedEventHandler clickHandler, params string[] uidList)
        {
            if (uidList == null || uidList.Length == 0)
            {
                return;
            }

            HashSet<string> hash = new HashSet<string>(uidList, StringComparer.InvariantCultureIgnoreCase);

            IEnumerable<MenuItem> source = GetMenuItems(items);

            foreach (var item in source)
            {
                if (hash.Contains(item.Uid))
                {
                    item.Click -= clickHandler;
                    item.Click -= clickHandler;

                    item.Click += clickHandler;
                }
            }
        }

        public static void FillLastSolutionItems(ConnectionData connectionData, IEnumerable<Control> items, bool isEnabled, RoutedEventHandler clickHandler, params string[] uidList)
        {
            if (uidList == null || uidList.Length == 0)
            {
                return;
            }

            HashSet<string> hash = new HashSet<string>(uidList, StringComparer.InvariantCultureIgnoreCase);

            IEnumerable<MenuItem> source = GetMenuItems(items);

            foreach (var item in source)
            {
                if (hash.Contains(item.Uid))
                {
                    item.IsEnabled = false;
                    item.Visibility = Visibility.Collapsed;

                    item.Items.Clear();

                    if (isEnabled)
                    {
                        if (connectionData != null
                            && connectionData.LastSelectedSolutionsUniqueName != null
                            && connectionData.LastSelectedSolutionsUniqueName.Any()
                        )
                        {
                            item.IsEnabled = true;
                            item.Visibility = Visibility.Visible;

                            foreach (var uniqueName in connectionData.LastSelectedSolutionsUniqueName)
                            {
                                var menuItem = new MenuItem()
                                {
                                    Header = uniqueName.Replace("_", "__"),
                                    Tag = uniqueName,
                                };

                                menuItem.Click += clickHandler;

                                item.Items.Add(menuItem);
                            }
                        }
                    }
                }
            }
        }

        public static IEnumerable<MenuItem> GetMenuItems(IEnumerable<Control> items)
        {
            foreach (var item in items.OfType<MenuItem>())
            {
                yield return item;

                foreach (var child in GetMenuItems(item.Items.OfType<Control>()))
                {
                    yield return child;
                }
            }
        }

        protected void FocusOnComboBoxTextBox(ComboBox comboBox)
        {
            comboBox.Loaded -= ComboBox_Loaded;
            comboBox.Loaded -= ComboBox_Loaded;
            comboBox.Loaded += ComboBox_Loaded;
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var comboBox = sender as ComboBox;

            PerformFocus(comboBox);
        }

        private void PerformFocus(ComboBox comboBox)
        {
            if (comboBox == null)
            {
                return;
            }

            comboBox.Focus();
            Keyboard.Focus(comboBox);

            if (comboBox.Template.FindName("PART_EditableTextBox", comboBox) is TextBox textBox)
            {
                textBox.SelectionStart = textBox.Text.Length;
                textBox.SelectionLength = 0;
                textBox.Focus();
            }
        }

        protected void ToggleControl(IEnumerable<UIElement> controlsArray)
        {
            ToggleControlInternal(IsControlsEnabled, controlsArray);
        }

        protected void ToggleControl(bool enabled, IEnumerable<UIElement> controlsArray)
        {
            ToggleControlInternal(enabled, controlsArray);
        }

        protected void ToggleControl(params UIElement[] controlsArray)
        {
            ToggleControlInternal(IsControlsEnabled, controlsArray);
        }

        protected void ToggleControl(bool enabled, params UIElement[] controlsArray)
        {
            ToggleControlInternal(enabled, controlsArray);
        }

        private void ToggleControlInternal(bool enabled, IEnumerable<UIElement> controlsArray)
        {
            if (controlsArray == null || !controlsArray.Any())
            {
                return;
            }

            foreach (var control in controlsArray)
            {
                if (control == null)
                {
                    continue;
                }

                control.Dispatcher.Invoke(() =>
                {
                    if (control is TextBox textBox)
                    {
                        textBox.IsReadOnly = !enabled;
                        textBox.IsReadOnlyCaretVisible = !enabled;
                    }
                    else if (control is ProgressBar progressBar)
                    {
                        progressBar.IsIndeterminate = !enabled;
                    }
                    else
                    {
                        control.IsEnabled = enabled;
                    }
                });
            }
        }

        protected void SetCurrentConnection(IWriteToOutput iWriteToOutput, ConnectionData connectionData)
        {
            if (connectionData == null || connectionData.ConnectionConfiguration == null)
            {
                return;
            }

            if (connectionData.ConnectionConfiguration.CurrentConnectionData?.ConnectionId == connectionData.ConnectionId)
            {
                return;
            }

            connectionData.ConnectionConfiguration.SetCurrentConnection(connectionData.ConnectionId);
            connectionData.ConnectionConfiguration.Save();
            iWriteToOutput.WriteToOutput(null, Properties.OutputStrings.CurrentConnectionFormat1, connectionData.Name);
        }

        protected void SwitchEntityDatesToLocalTime(IEnumerable<Entity> entities)
        {
            foreach (var item in entities)
            {
                foreach (var attrKey in item.Attributes.Keys.ToList())
                {
                    if (item.Attributes[attrKey] != null
                        && item.Attributes[attrKey] is DateTime dateTime
                    )
                    {
                        item.Attributes[attrKey] = dateTime.ToLocalTime();
                    }
                }
            }
        }

        public static bool IsExcelInstalled()
        {
            RegistryKey hkcr = Registry.ClassesRoot;
            RegistryKey excelKey = hkcr.OpenSubKey("Excel.Application");

            return excelKey != null;
        }

        protected void LoadEntityNames(ComboBox comboBox, ConnectionData connectionData)
        {
            string text = comboBox.Text;

            comboBox.Dispatcher.Invoke(() =>
            {
                comboBox.Items.Clear();

                if (connectionData != null
                    && connectionData.EntitiesIntellisenseData != null
                    && connectionData.EntitiesIntellisenseData.Entities != null
                )
                {
                    var entityList = connectionData.EntitiesIntellisenseData.Entities.Values.OrderBy(e => e.IsIntersectEntity).ThenBy(e => e.EntityLogicalName).ToList();

                    foreach (var entityData in entityList)
                    {
                        comboBox.Items.Add(entityData.EntityLogicalName);
                    }
                }

                comboBox.Text = text;
            });
        }

        protected void LoadEntityRelationshipsManyToMany(ComboBox cmBRelationshipName, ComboBox cmBEntityName, ConnectionData connectionData, string entityName)
        {
            string textRelationshipName = cmBRelationshipName.Text;
            string textEntityName = cmBEntityName.Text;

            cmBRelationshipName.Dispatcher.Invoke(() =>
            {
                cmBRelationshipName.Items.Clear();

                if (connectionData != null
                    && connectionData.EntitiesIntellisenseData != null
                    && connectionData.EntitiesIntellisenseData.Entities != null
                    && connectionData.EntitiesIntellisenseData.Entities.ContainsKey(entityName)
                )
                {
                    var entityData = connectionData.EntitiesIntellisenseData.Entities[entityName];

                    if (entityData != null && entityData.ManyToManyRelationships != null)
                    {
                        var relationshipNamesList = entityData.ManyToManyRelationships.Keys.OrderBy(r => r).ToList();

                        foreach (var relationshipName in relationshipNamesList)
                        {
                            cmBRelationshipName.Items.Add(relationshipName);
                        }

                        var hash = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

                        foreach (var relationship in entityData.ManyToManyRelationships.Values)
                        {
                            if (!string.Equals(entityName, relationship.Entity1Name, StringComparison.InvariantCultureIgnoreCase))
                            {
                                hash.Add(relationship.Entity1Name);
                            }

                            if (!string.Equals(entityName, relationship.Entity2Name, StringComparison.InvariantCultureIgnoreCase))
                            {
                                hash.Add(relationship.Entity2Name);
                            }

                            if (string.Equals(entityName, relationship.Entity1Name, StringComparison.InvariantCultureIgnoreCase) 
                                && string.Equals(entityName, relationship.Entity2Name, StringComparison.InvariantCultureIgnoreCase)
                            )
                            {
                                hash.Add(entityName);
                            }
                        }

                        foreach (var targetEntityName in hash.OrderBy(r => r))
                        {
                            cmBEntityName.Items.Add(targetEntityName);
                        }
                    }
                }

                cmBRelationshipName.Text = textRelationshipName;
                cmBEntityName.Text = textEntityName;
            });
        }

        protected void LoadEntityNames(ComboBox comboBox, ConnectionData connectionData1, ConnectionData connectionData2)
        {
            string text = comboBox.Text;

            HashSet<string> entities = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            comboBox.Items.Clear();

            if (connectionData1 != null
                && connectionData1.EntitiesIntellisenseData != null
                && connectionData1.EntitiesIntellisenseData.Entities != null
            )
            {
                foreach (var item in connectionData1.EntitiesIntellisenseData.Entities.Keys)
                {
                    entities.Add(item);
                }
            }

            if (connectionData2 != null
                && connectionData2.EntitiesIntellisenseData != null
                && connectionData2.EntitiesIntellisenseData.Entities != null
            )
            {
                var entityList = connectionData2.EntitiesIntellisenseData.Entities.Values.OrderBy(e => e.IsIntersectEntity).ThenBy(e => e.EntityLogicalName).ToList();

                foreach (var entityData in entityList)
                {
                    if (entities.Contains(entityData.EntityLogicalName))
                    {
                        comboBox.Items.Add(entityData.EntityLogicalName);
                    }
                }
            }

            comboBox.Text = text;
        }

        public static T GetItemFromRoutedDataContext<T>(RoutedEventArgs e) where T : class
        {
            if (e.OriginalSource is FrameworkElement frameworkElement)
            {
                if (frameworkElement.DataContext != null && frameworkElement.DataContext is T result)
                {
                    return result;
                }
            }
            else if (e.OriginalSource is FrameworkContentElement frameworkContentElement)
            {
                if (frameworkContentElement.DataContext != null && frameworkContentElement.DataContext is T result)
                {
                    return result;
                }
            }

            return null;
        }

        public static string CorrectFolderIfEmptyOrNotExists(IWriteToOutput iWriteToOutput, string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                iWriteToOutput.WriteToOutput(null, Properties.OutputStrings.FolderForExportIsEmpty);
                folder = FileOperations.GetDefaultFolderForExportFilePath();
            }
            else if (!Directory.Exists(folder))
            {
                iWriteToOutput.WriteToOutput(null, Properties.OutputStrings.FolderForExportDoesNotExistsFormat1, folder);
                folder = FileOperations.GetDefaultFolderForExportFilePath();
            }

            return folder;
        }

        protected static string GetPluginConfigurationFilePath(IWriteToOutput iWriteToOutput)
        {
            string selectedPath = string.Empty;

            var thread = new Thread(() =>
            {
                try
                {
                    var openFileDialog1 = new Microsoft.Win32.OpenFileDialog
                    {
                        Filter = "Plugin Configuration (.xml)|*.xml",
                        FilterIndex = 1,
                        RestoreDirectory = true
                    };

                    if (openFileDialog1.ShowDialog().GetValueOrDefault())
                    {
                        selectedPath = openFileDialog1.FileName;
                    }
                }
                catch (Exception ex)
                {
                    iWriteToOutput.WriteErrorToOutput(null, ex);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            thread.Join();

            return selectedPath;
        }

        protected static void AddFileToVSProject(EnvDTE.SelectedItem _selectedItem, string filePath)
        {
            if (_selectedItem == null)
            {
                return;
            }

            if (_selectedItem.ProjectItem != null)
            {
                _selectedItem.ProjectItem.ProjectItems.AddFromFileCopy(filePath);

                _selectedItem.ProjectItem.ContainingProject.Save();
            }
            else if (_selectedItem.Project != null)
            {
                _selectedItem.Project.ProjectItems.AddFromFile(filePath);

                _selectedItem.Project.Save();
            }
        }

        protected static void GetEntityViewItemAndCopyToClipboard<T>(RoutedEventArgs e, Func<T, string> valueGetter) where T : class
        {
            T entity = GetItemFromRoutedDataContext<T>(e);

            if (entity == null)
            {
                return;
            }

            string value = valueGetter(entity);

            ClipboardHelper.SetText(value);
        }

        protected static void FillComboBoxTrueFalse(ComboBox comboBox)
        {
            comboBox.Items.Clear();
            comboBox.Items.Add(string.Empty);
            comboBox.Items.Add(false);
            comboBox.Items.Add(true);
            comboBox.SelectedIndex = 0;
        }

        protected static void CorrectHeaderToLabel(string header, DataGridTextColumn column)
        {
            if (string.Equals(header, "IsManaged", StringComparison.InvariantCultureIgnoreCase))
            {
                column.Header = new System.Windows.Controls.Label()
                {
                    Content = "M",
                    ToolTip = "IsManaged",
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                };

                column.Width = DataGridLength.Auto;
                column.CanUserResize = false;
            }
            else if (string.Equals(header, "IsCustomizable", StringComparison.InvariantCultureIgnoreCase))
            {
                column.Header = new System.Windows.Controls.Label()
                {
                    Content = "C",
                    ToolTip = "IsCustomizable",
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                };

                column.Width = DataGridLength.Auto;
                column.CanUserResize = false;
            }
        }

        public static void SetElementsEnabledAndVisible(bool enabled, params UIElement[] elementsArray)
        {
            if (elementsArray == null || elementsArray.Length == 0)
            {
                return;
            }

            foreach (var element in elementsArray)
            {
                element.IsEnabled = enabled;
                element.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public static void SetElementsVisible(bool enabled, params UIElement[] elementsArray)
        {
            if (elementsArray == null || elementsArray.Length == 0)
            {
                return;
            }

            foreach (var element in elementsArray)
            {
                element.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
