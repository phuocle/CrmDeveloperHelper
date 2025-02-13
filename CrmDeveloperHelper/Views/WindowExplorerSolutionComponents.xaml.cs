using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Nav.Common.VSPackages.CrmDeveloperHelper.Controllers;
using Nav.Common.VSPackages.CrmDeveloperHelper.Entities;
using Nav.Common.VSPackages.CrmDeveloperHelper.Helpers;
using Nav.Common.VSPackages.CrmDeveloperHelper.Interfaces;
using Nav.Common.VSPackages.CrmDeveloperHelper.Model;
using Nav.Common.VSPackages.CrmDeveloperHelper.Repository;
using Nav.Common.VSPackages.CrmDeveloperHelper.UserControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Nav.Common.VSPackages.CrmDeveloperHelper.Views
{
    public partial class WindowExplorerSolutionComponents : WindowWithSingleConnection
    {
        private readonly SolutionComponentDescriptor _descriptor;

        private readonly CommonConfiguration _commonConfig;

        private readonly SolutionComponentConverter _converter;

        private readonly Popup _optionsPopup;

        private Solution _solution;

        private readonly ObservableCollection<SolutionComponentViewItem> _itemsSource;

        public WindowExplorerSolutionComponents(
             IWriteToOutput iWriteToOutput
            , CommonConfiguration commonConfig
            , IOrganizationServiceExtented service
            , SolutionComponentDescriptor descriptor
            , string solutionUniqueName
            , string selection
        ) : base(iWriteToOutput, service)
        {
            IncreaseInit();

            InitializeComponent();

            SetInputLanguageEnglish();

            cmBComponentType.ItemsSource = new EnumBindingSourceExtension(typeof(ComponentType?))
            {
                SortByName = true,
            }.ProvideValue(null) as IEnumerable;

            this._descriptor = descriptor;
            this._commonConfig = commonConfig;

            this.Title = string.Format("{0} Solution Components", solutionUniqueName);

            if (this._descriptor == null)
            {
                this._descriptor = new SolutionComponentDescriptor(_service);
            }

            this._converter = new SolutionComponentConverter(this._descriptor);

            {
                var child = new ExportXmlOptionsControl(_commonConfig, XmlOptionsControls.SolutionComponentXmlOptions);
                child.CloseClicked += Child_CloseClicked;
                this._optionsPopup = new Popup
                {
                    Child = child,

                    PlacementTarget = toolBarHeader,
                    Placement = PlacementMode.Bottom,
                    StaysOpen = false,
                    Focusable = true,
                };
            }

            this.tSSLblConnectionName.Content = _service.ConnectionData.Name;

            FillDataGridColumns();

            LoadFromConfig();

            LoadConfiguration();

            if (!string.IsNullOrEmpty(selection))
            {
                txtBFilter.Text = selection;
            }

            txtBFilter.SelectionLength = 0;
            txtBFilter.SelectionStart = txtBFilter.Text.Length;

            txtBFilter.Focus();

            this._itemsSource = new ObservableCollection<SolutionComponentViewItem>();

            this.lstVSolutionComponents.ItemsSource = _itemsSource;

            FillExplorersMenuItems();

            DecreaseInit();

            var task = ShowExistingSolutionComponents(solutionUniqueName);
        }

        private void FillExplorersMenuItems()
        {
            var explorersHelper = new ExplorersHelper(_iWriteToOutput, _commonConfig, () => Task.FromResult(_service));

            explorersHelper.FillExplorers(miExplorers);
        }

        private void LoadFromConfig()
        {
            cmBFileAction.DataContext = _commonConfig;
        }

        private const string paramComponentType = "ComponentType";

        private void LoadConfiguration()
        {
            WindowSettings winConfig = this.GetWindowsSettings();

            {
                var categoryValue = winConfig.GetValueInt(paramComponentType);

                if (categoryValue != -1)
                {
                    var item = cmBComponentType.Items.OfType<ComponentType?>().FirstOrDefault(e => (int)e == categoryValue);
                    if (item.HasValue)
                    {
                        cmBComponentType.SelectedItem = item.Value;

                        FillDataGridColumns();
                    }
                }
            }
        }

        protected override void SaveConfigurationInternal(WindowSettings winConfig)
        {
            base.SaveConfigurationInternal(winConfig);

            var categoryValue = -1;

            if (cmBComponentType.SelectedItem is ComponentType selected)
            {
                categoryValue = (int)selected;
            }

            winConfig.DictInt[paramComponentType] = categoryValue;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _commonConfig.Save();
        }

        private enum SolutionComponentsType
        {
            SolutionComponents = 0,
            MissingComponents = 1,
            UninstallComponents = 2,
        }

        private SolutionComponentsType GetSolutionComponentsType()
        {
            var result = SolutionComponentsType.SolutionComponents;

            cmBSolutionComponentsType.Dispatcher.Invoke(() =>
            {
                if (cmBSolutionComponentsType.SelectedIndex != -1)
                {
                    result = (SolutionComponentsType)cmBSolutionComponentsType.SelectedIndex;
                }
            });

            return result;
        }

        private async Task ShowExistingSolutionComponents(string solutionUniqueName = null)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            int? category = null;

            this.Dispatcher.Invoke(() =>
            {
                this._itemsSource.Clear();

                if (cmBComponentType.SelectedItem is ComponentType comp)
                {
                    category = (int)comp;
                }
            });

            var solutionComponents = GetSolutionComponentsType();

            var list = new List<SolutionComponent>();

            string formatResult = Properties.OutputStrings.LoadingRequiredComponentsCompletedFormat1;

            try
            {
                if (this._solution == null && !string.IsNullOrEmpty(solutionUniqueName))
                {
                    var rep = new SolutionRepository(this._service);

                    this._solution = await rep.GetSolutionByUniqueNameAsync(solutionUniqueName);

                    var isManaged = this._solution?.IsManaged ?? false;

                    var description = this._solution?.Description;

                    var hasDescription = !string.IsNullOrEmpty(description);

                    this.Dispatcher.Invoke(() =>
                    {
                        miClearUnManagedSolution.IsEnabled = sepClearUnManagedSolution.IsEnabled = !isManaged;
                        miClearUnManagedSolution.Visibility = sepClearUnManagedSolution.Visibility = !isManaged ? Visibility.Visible : Visibility.Collapsed;

                        miSelectAsLastSelected.IsEnabled = sepClearUnManagedSolution2.IsEnabled = !isManaged;
                        miSelectAsLastSelected.Visibility = sepClearUnManagedSolution2.Visibility = !isManaged ? Visibility.Visible : Visibility.Collapsed;

                        miChangeInEditor.IsEnabled = sepChangeInEditor.IsEnabled = !isManaged;
                        miChangeInEditor.Visibility = sepChangeInEditor.Visibility = !isManaged ? Visibility.Visible : Visibility.Collapsed;

                        miSolutionDescription.IsEnabled = sepSolutionDescription.IsEnabled = hasDescription;
                        miSolutionDescription.Visibility = sepSolutionDescription.Visibility = hasDescription ? Visibility.Visible : Visibility.Collapsed;

                        miSolutionDescription.ToolTip = description;
                    });
                }

                if (this._solution != null)
                {
                    switch (solutionComponents)
                    {
                        case SolutionComponentsType.SolutionComponents:
                        default:
                            {
                                ToggleControls(false, Properties.OutputStrings.LoadingSolutionComponents);
                                formatResult = Properties.OutputStrings.LoadingSolutionComponentsCompletedFormat1;

                                var repository = new SolutionComponentRepository(this._service);

                                list = await repository.GetSolutionComponentsByTypeAsync(_solution.Id, category, new ColumnSet(SolutionComponent.Schema.Attributes.objectid, SolutionComponent.Schema.Attributes.componenttype, SolutionComponent.Schema.Attributes.rootcomponentbehavior));
                            }
                            break;

                        case SolutionComponentsType.MissingComponents:
                            {
                                ToggleControls(false, Properties.OutputStrings.LoadingMissingComponents);
                                formatResult = Properties.OutputStrings.LoadingMissingComponentsCompletedFormat1;

                                var repository = new DependencyRepository(this._service);

                                var temp = (await repository.GetSolutionMissingDependenciesAsync(_solution.UniqueName)).Select(e => e.RequiredToSolutionComponent());

                                temp = temp.Where(en => en.ComponentType != null && en.ObjectId.HasValue);

                                if (category.HasValue)
                                {
                                    temp = temp.Where(en => en.ComponentType?.Value == category.Value);
                                }

                                var hash = new HashSet<Tuple<int, Guid>>();

                                foreach (var item in temp)
                                {
                                    if (hash.Add(Tuple.Create(item.ComponentType.Value, item.ObjectId.Value)))
                                    {
                                        list.Add(item);
                                    }
                                }
                            }
                            break;

                        case SolutionComponentsType.UninstallComponents:
                            {
                                ToggleControls(false, Properties.OutputStrings.LoadingUninstallComponents);
                                formatResult = Properties.OutputStrings.LoadingUninstallComponentsCompletedFormat1;

                                var repository = new DependencyRepository(this._service);

                                var temp = (await repository.GetSolutionDependenciesForUninstallAsync(_solution.UniqueName)).Select(en => en.RequiredToSolutionComponent());

                                temp = temp.Where(en => en.ComponentType != null && en.ObjectId.HasValue);

                                if (category.HasValue)
                                {
                                    temp = temp.Where(en => en.ComponentType?.Value == category.Value);
                                }

                                var hash = new HashSet<Tuple<int, Guid>>();

                                foreach (var item in temp)
                                {
                                    if (hash.Add(Tuple.Create(item.ComponentType.Value, item.ObjectId.Value)))
                                    {
                                        list.Add(item);
                                    }
                                }
                            }
                            break;
                    }

                    await _descriptor.GetSolutionComponentsDescriptionAsync(list);
                }
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);
            }

            var convertedList = new List<SolutionComponentViewItem>();

            foreach (var entity in list
                    .OrderBy(ent => ent.ComponentType.Value)
                    .ThenBy(ent => ent.ObjectId)
                )
            {
                string name = _descriptor.GetName(entity);
                string displayName = _descriptor.GetDisplayName(entity);
                string managed = _descriptor.GetManagedName(entity);
                string customizable = _descriptor.GetCustomizableName(entity);
                string behavior = _descriptor.GetRootComponentBehaviorName(entity);

                var item = new SolutionComponentViewItem(entity, name, displayName, entity.ComponentTypeName, managed, customizable, behavior);

                convertedList.Add(item);
            }

            var enumerable = convertedList.AsEnumerable();

            string textName = string.Empty;

            txtBFilter.Dispatcher.Invoke(() =>
            {
                textName = txtBFilter.Text.Trim().ToLower();
            });

            enumerable = FilterList(enumerable, textName);

            LoadSolutionComponents(enumerable);

            ToggleControls(true, formatResult, enumerable.Count());
        }

        private static IEnumerable<SolutionComponentViewItem> FilterList(IEnumerable<SolutionComponentViewItem> list, string textName)
        {
            if (!string.IsNullOrEmpty(textName))
            {
                textName = textName.ToLower();

                if (int.TryParse(textName, out int tempInt))
                {
                    list = list.Where(ent => ent.SolutionComponent.ComponentType?.Value == tempInt);
                }
                else
                {
                    if (Guid.TryParse(textName, out Guid tempGuid))
                    {
                        list = list.Where(ent =>
                            ent.SolutionComponent.ObjectId == tempGuid
                        );
                    }
                    else
                    {
                        list = list.Where(ent =>
                        {
                            var name = ent.Name ?? string.Empty;
                            var nameUnique = ent.DisplayName ?? string.Empty;

                            return name.IndexOf(textName, StringComparison.InvariantCultureIgnoreCase) > -1 || nameUnique.IndexOf(textName, StringComparison.InvariantCultureIgnoreCase) > -1;
                        });
                    }
                }
            }

            return list;
        }

        private void LoadSolutionComponents(IEnumerable<SolutionComponentViewItem> results)
        {
            this.lstVSolutionComponents.Dispatcher.Invoke(() =>
            {
                foreach (var item in results.OrderBy(en => en.SolutionComponent.ComponentType.Value).ThenBy(en => en.Name))
                {
                    _itemsSource.Add(item);
                }

                if (this.lstVSolutionComponents.Items.Count == 1)
                {
                    this.lstVSolutionComponents.SelectedItem = this.lstVSolutionComponents.Items[0];
                }
            });
        }

        private void UpdateStatus(string format, params object[] args)
        {
            string message = format;

            if (args != null && args.Length > 0)
            {
                message = string.Format(format, args);
            }

            _iWriteToOutput.WriteToOutput(_service.ConnectionData, message);

            this.stBIStatus.Dispatcher.Invoke(() =>
            {
                this.stBIStatus.Content = message;
            });
        }

        protected override void ToggleControls(bool enabled, string statusFormat, params object[] args)
        {
            this.ChangeInitByEnabled(enabled);

            UpdateStatus(statusFormat, args);

            ToggleControl(this.tSProgressBar, this.btnExportAll, this.tSDDBExportSolutionComponent, this.cmBComponentType, this.mISolutionInformation, this.cmBSolutionComponentsType);

            UpdateButtonsEnable();
        }

        private void UpdateButtonsEnable()
        {
            this.lstVSolutionComponents.Dispatcher.Invoke(() =>
            {
                try
                {
                    bool enabled = this.lstVSolutionComponents.SelectedItems.Count > 0;

                    UIElement[] list = { tSDDBExportSolutionComponent, btnExportAll };

                    foreach (var button in list)
                    {
                        button.IsEnabled = enabled;
                    }
                }
                catch (Exception)
                {
                }
            });
        }

        private async void txtBFilterEnitity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await ShowExistingSolutionComponents();
            }
        }

        private SolutionComponentViewItem GetSelectedSolutionComponent()
        {
            return this.lstVSolutionComponents.SelectedItems.OfType<SolutionComponentViewItem>().Count() == 1
                ? this.lstVSolutionComponents.SelectedItems.OfType<SolutionComponentViewItem>().SingleOrDefault() : null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void lstVwEntities_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                SolutionComponentViewItem item = GetItemFromRoutedDataContext<SolutionComponentViewItem>(e);

                if (item != null)
                {
                    await ExecuteAction(item, PerformExportMouseDoubleClick);
                }
            }
        }

        private Task PerformExportMouseDoubleClick(string folder, SolutionComponentViewItem item)
        {
            return Task.Run(() => _service.UrlGenerator.OpenSolutionComponentInWeb((ComponentType)item.SolutionComponent.ComponentType.Value, item.SolutionComponent.ObjectId.Value));
        }

        private void lstVwEntities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonsEnable();
        }

        private async Task ExecuteAction(SolutionComponentViewItem item, Func<string, SolutionComponentViewItem, Task> action)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            _commonConfig.CheckFolderForExportExists(_iWriteToOutput);

            await action(_commonConfig.FolderForExport, item);
        }

        private async void btnExportAll_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedSolutionComponent();

            if (entity == null)
            {
                return;
            }

            await ExecuteAction(entity, PerformExportAllXml);
        }

        private async Task PerformExportAllXml(string folder, SolutionComponentViewItem solutionComponentViewItem)
        {
            await PerformExportEntityDescription(folder, solutionComponentViewItem);
        }

        private async void mICreateEntityDescription_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedSolutionComponent();

            if (entity == null)
            {
                return;
            }

            await ExecuteAction(entity, PerformExportEntityDescription);
        }

        private async Task PerformExportEntityDescription(string folder, SolutionComponentViewItem solutionComponentViewItem)
        {
            ToggleControls(false, Properties.OutputStrings.CreatingEntityDescription);

            string fileName = _descriptor.GetFileName(_service.ConnectionData.Name, solutionComponentViewItem.SolutionComponent.ComponentType.Value, solutionComponentViewItem.SolutionComponent.ObjectId.Value, EntityFileNameFormatter.Headers.EntityDescription, FileExtension.txt);

            string filePath = Path.Combine(folder, FileOperations.RemoveWrongSymbols(fileName));

            var stringBuilder = new StringBuilder();

            var desc = await EntityDescriptionHandler.GetEntityDescriptionAsync(solutionComponentViewItem.SolutionComponent, _service.ConnectionData);

            if (!string.IsNullOrEmpty(desc))
            {
                if (stringBuilder.Length > 0) { stringBuilder.AppendLine().AppendLine().AppendLine().AppendLine(); }

                stringBuilder.AppendLine(desc);
            }

            var entity = _descriptor.GetEntity<Entity>(solutionComponentViewItem.SolutionComponent.ComponentType.Value, solutionComponentViewItem.SolutionComponent.ObjectId.Value);

            if (entity != null)
            {
                desc = await EntityDescriptionHandler.GetEntityDescriptionAsync(entity, _service.ConnectionData);

                if (!string.IsNullOrEmpty(desc))
                {
                    if (stringBuilder.Length > 0) { stringBuilder.AppendLine().AppendLine().AppendLine().AppendLine(); }

                    stringBuilder.AppendLine(desc);
                }
            }

            File.WriteAllText(filePath, stringBuilder.ToString(), new UTF8Encoding(false));

            this._iWriteToOutput.WriteToOutput(_service.ConnectionData, "{0} {1} Entity Description exported to {2}", solutionComponentViewItem.ComponentType, solutionComponentViewItem.Name, filePath);

            this._iWriteToOutput.PerformAction(_service.ConnectionData, filePath);

            ToggleControls(true, Properties.OutputStrings.CreatingEntityDescriptionCompleted);
        }

        protected override async Task OnRefreshList(ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            await ShowExistingSolutionComponents();
        }

        private void mIOpenDependentComponentsInWeb_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedSolutionComponent();

            if (entity == null)
            {
                return;
            }

            this._service.ConnectionData.OpenSolutionComponentDependentComponentsInWeb((ComponentType)entity.SolutionComponent.ComponentType.Value, entity.SolutionComponent.ObjectId.Value);
        }

        private void mIOpenInWeb_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedSolutionComponent();

            if (entity == null)
            {
                return;
            }

            _service.UrlGenerator.OpenSolutionComponentInWeb((ComponentType)entity.SolutionComponent.ComponentType.Value, entity.SolutionComponent.ObjectId.Value);
        }

        private async void AddToSolution_Click(object sender, RoutedEventArgs e)
        {
            await AddToSolution(true, null);
        }

        private async void AddToSolutionLast_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
                && menuItem.Tag != null
                && menuItem.Tag is string solutionUniqueName
            )
            {
                await AddToSolution(false, solutionUniqueName);
            }
        }

        private async void AddToCrmSolutionIncludeSubcomponents_Click(object sender, RoutedEventArgs e)
        {
            await AddToSolution(true, null, SolutionComponent.Schema.OptionSets.rootcomponentbehavior.Include_Subcomponents_0);
        }

        private async void AddToCrmSolutionDoNotIncludeSubcomponents_Click(object sender, RoutedEventArgs e)
        {
            await AddToSolution(true, null, SolutionComponent.Schema.OptionSets.rootcomponentbehavior.Do_not_include_subcomponents_1);
        }

        private async void AddToCrmSolutionIncludeAsShellOnly_Click(object sender, RoutedEventArgs e)
        {
            await AddToSolution(true, null, SolutionComponent.Schema.OptionSets.rootcomponentbehavior.Include_As_Shell_Only_2);
        }

        private async void AddToCrmSolutionLastIncludeSubcomponents_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
               && menuItem.Tag != null
               && menuItem.Tag is string solutionUniqueName
            )
            {
                await AddToSolution(false, solutionUniqueName, SolutionComponent.Schema.OptionSets.rootcomponentbehavior.Include_Subcomponents_0);
            }
        }

        private async void AddToCrmSolutionLastDoNotIncludeSubcomponents_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
               && menuItem.Tag != null
               && menuItem.Tag is string solutionUniqueName
            )
            {
                await AddToSolution(false, solutionUniqueName, SolutionComponent.Schema.OptionSets.rootcomponentbehavior.Do_not_include_subcomponents_1);
            }
        }

        private async void AddToCrmSolutionLastIncludeAsShellOnly_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
               && menuItem.Tag != null
               && menuItem.Tag is string solutionUniqueName
            )
            {
                await AddToSolution(false, solutionUniqueName, SolutionComponent.Schema.OptionSets.rootcomponentbehavior.Include_As_Shell_Only_2);
            }
        }

        private async void AddToCurrentSolution_Click(object sender, RoutedEventArgs e)
        {
            if (GetSolutionComponentsType() == SolutionComponentsType.SolutionComponents
                || _solution == null
                || _solution.IsManaged.GetValueOrDefault()
            )
            {
                return;
            }

            await AddToSolution(false, _solution.UniqueName);
        }

        private async Task AddToSolution(bool withSelect, string solutionUniqueName)
        {
            var solutionComponents = lstVSolutionComponents.SelectedItems.OfType<SolutionComponentViewItem>().Select(en => en.SolutionComponent).ToList();

            if (!solutionComponents.Any())
            {
                return;
            }

            await AddComponentsToSolution(withSelect, solutionUniqueName, solutionComponents);
        }

        private async Task AddToSolution(bool withSelect, string solutionUniqueName, SolutionComponent.Schema.OptionSets.rootcomponentbehavior behavior)
        {
            var solutionComponents = lstVSolutionComponents.SelectedItems.OfType<SolutionComponentViewItem>().Select(en => en.SolutionComponent).ToList();

            if (!solutionComponents.Any())
            {
                return;
            }

            solutionComponents = solutionComponents.Select(en => new SolutionComponent()
            {
                ComponentType = en.ComponentType,
                ObjectId = en.ObjectId,
                RootComponentBehaviorEnum = behavior,
            }).ToList();

            await AddComponentsToSolution(withSelect, solutionUniqueName, solutionComponents);
        }

        private async Task AddComponentsToSolution(bool withSelect, string solutionUniqueName, IEnumerable<SolutionComponent> solutionComponents)
        {
            if (!solutionComponents.Any())
            {
                return;
            }

            _commonConfig.Save();

            try
            {
                this._iWriteToOutput.ActivateOutputWindow(_service.ConnectionData);

                await SolutionController.AddSolutionComponentsCollectionToSolution(_iWriteToOutput, _service, _descriptor, _commonConfig, solutionUniqueName, solutionComponents, withSelect);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);
            }
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (!(sender is ContextMenu contextMenu))
            {
                return;
            }

            var items = contextMenu.Items.OfType<Control>();

            {
                var enabledAdd = GetSolutionComponentsType() != SolutionComponentsType.SolutionComponents && this._solution != null && !this._solution.IsManaged.GetValueOrDefault();
                var enabledRemove = GetSolutionComponentsType() == SolutionComponentsType.SolutionComponents && this._solution != null && !this._solution.IsManaged.GetValueOrDefault();

                ActivateControls(items, enabledRemove, "contMnRemoveFromSolution");

                ActivateControls(items, enabledAdd, "contMnAddToCurrentSolution");
            }

            FillLastSolutionItems(_service.ConnectionData, items, true, AddToSolutionLast_Click, "contMnAddToSolutionLast");

            FillLastSolutionItems(_service.ConnectionData, items, true, AddToCrmSolutionLastIncludeSubcomponents_Click, "contMnAddToSolutionWithBehaviourLastIncludeSubcomponents");

            FillLastSolutionItems(_service.ConnectionData, items, true, AddToCrmSolutionLastDoNotIncludeSubcomponents_Click, "contMnAddToSolutionWithBehaviourLastDoNotIncludeSubcomponents");

            FillLastSolutionItems(_service.ConnectionData, items, true, AddToCrmSolutionLastIncludeAsShellOnly_Click, "contMnAddToSolutionWithBehaviourLastIncludeAsShellOnly");

            ActivateControls(items, true && _service.ConnectionData.LastSelectedSolutionsUniqueName != null && _service.ConnectionData.LastSelectedSolutionsUniqueName.Any(), "contMnAddToSolutionWithBehaviour");

            var selectedSolutionComponent = GetSelectedSolutionComponent();

            var hasExplorer = false;

            if (selectedSolutionComponent != null)
            {
                hasExplorer = WindowHelper.HasExplorer(selectedSolutionComponent.SolutionComponent.ComponentType?.Value);

                if (hasExplorer)
                {
                    var componentType = (ComponentType)selectedSolutionComponent.SolutionComponent.ComponentType.Value;

                    string componentName = string.Format("{0} Explorer", componentType.ToString());

                    SetControlsName(items, componentName, "contMnExplorer");
                }
            }

            ActivateControls(items, hasExplorer, "contMnExplorer");

            var menuItemLinkedComponent = items.OfType<MenuItem>().FirstOrDefault(i => string.Equals(i.Uid, "contMnLinkedComponents", StringComparison.InvariantCultureIgnoreCase));

            if (menuItemLinkedComponent != null)
            {
                menuItemLinkedComponent.Items.Clear();

                if (selectedSolutionComponent != null)
                {
                    var linkedComponents = _descriptor.GetLinkedComponents(selectedSolutionComponent.SolutionComponent);

                    if (linkedComponents != null && linkedComponents.Any())
                    {
                        var componentsWithNames = linkedComponents.Select(c => new { Component = c, Name = _descriptor.GetName(c) }).OrderBy(c => c.Component.ComponentType?.Value).ThenBy(c => c.Name);

                        foreach (var item in componentsWithNames)
                        {
                            var menuItem = new MenuItem()
                            {
                                Header = string.Format("{0} - {1}", item.Component.ComponentTypeName, item.Name).Replace("_", "__"),
                            };

                            FillLinkedSolutionComponentActions(menuItem.Items, item.Component);

                            menuItemLinkedComponent.Items.Add(menuItem);
                        }
                    }
                }

                ActivateControls(items, menuItemLinkedComponent.Items.Count > 0, "contMnLinkedComponents");
            }
        }

        private void FillLinkedSolutionComponentActions(ItemCollection itemCollection, SolutionComponent solutionComponent)
        {
            MenuItem mILinkedComponentOpenInWeb = new MenuItem()
            {
                Header = "Open in Browser",
                Tag = solutionComponent,
            };
            mILinkedComponentOpenInWeb.Click += mILinkedComponentOpenInWeb_Click;

            MenuItem mILinkedComponentOpenEntityListInWeb = new MenuItem()
            {
                Header = "Open Entity List in Browser",
                Tag = solutionComponent,
            };
            mILinkedComponentOpenEntityListInWeb.Click += mILinkedComponentOpenEntityListInWeb_Click;

            MenuItem mILinkedComponentOpenExplorer = new MenuItem()
            {
                Header = "Open Explorer",
                Tag = solutionComponent,
            };
            mILinkedComponentOpenExplorer.Click += mILinkedComponentOpenExplorer_Click;

            MenuItem mILinkedComponentAddToCurrentSolution = new MenuItem()
            {
                Header = "Add to Current Solution",
                Tag = solutionComponent,
            };
            mILinkedComponentAddToCurrentSolution.Click += mILinkedComponentAddToCurrentSolution_Click;

            MenuItem mILinkedComponentAddToSolutionLast = new MenuItem()
            {
                Header = "Add to Last Crm Solution",
                Tag = solutionComponent,
                Uid = "mILinkedComponentAddToSolutionLast",
            };

            FillLastSolutionItems(_service.ConnectionData, new[] { mILinkedComponentAddToSolutionLast }, true, mILinkedComponentAddToSolutionLast_Click, "mILinkedComponentAddToSolutionLast");

            MenuItem mILinkedComponentAddToSolution = new MenuItem()
            {
                Header = "Add to Crm Solution",
                Tag = solutionComponent,
            };
            mILinkedComponentAddToSolution.Click += miLinkedComponentAddToSolution_Click;

            MenuItem mILinkedComponentOpenDependentComponentsInWeb = new MenuItem()
            {
                Header = "Open Dependent Components in Browser",
                Tag = solutionComponent,
            };
            mILinkedComponentOpenDependentComponentsInWeb.Click += mILinkedComponentOpenDependentComponentsInWeb_Click;

            MenuItem mILinkedComponentOpenDependentComponentsInExplorer = new MenuItem()
            {
                Header = "Open Dependent Components in Explorer",
                Tag = solutionComponent,
            };
            mILinkedComponentOpenDependentComponentsInExplorer.Click += mILinkedComponentOpenDependentComponentsInExplorer_Click;

            MenuItem mILinkedComponentOpenSolutionsContainingComponentInExplorer = new MenuItem()
            {
                Header = "Open Solutions Containing Component in Explorer",
                Tag = solutionComponent,
            };
            mILinkedComponentOpenSolutionsContainingComponentInExplorer.Click += mILinkedComponentOpenSolutionsContainingComponentInExplorer_Click;

            //MenuItem mILinkedComponent = new MenuItem()
            //{
            //    Header = "Open Entity List in Browser",
            //    Tag = solutionComponent,
            //};

            itemCollection.Add(mILinkedComponentOpenInWeb);

            if (solutionComponent.ComponentType?.Value == (int)ComponentType.Entity)
            {
                itemCollection.Add(new Separator());
                itemCollection.Add(mILinkedComponentOpenEntityListInWeb);
            }

            if (WindowHelper.HasExplorer(solutionComponent.ComponentType?.Value))
            {
                itemCollection.Add(new Separator());
                itemCollection.Add(mILinkedComponentOpenExplorer);
            }

            itemCollection.Add(new Separator());

            if (this._solution != null && !this._solution.IsManaged.GetValueOrDefault())
            {
                itemCollection.Add(mILinkedComponentAddToCurrentSolution);
            }

            itemCollection.Add(mILinkedComponentAddToSolutionLast);
            itemCollection.Add(mILinkedComponentAddToSolution);

            itemCollection.Add(new Separator());
            itemCollection.Add(mILinkedComponentOpenDependentComponentsInWeb);
            itemCollection.Add(mILinkedComponentOpenDependentComponentsInExplorer);

            itemCollection.Add(new Separator());
            itemCollection.Add(mILinkedComponentOpenSolutionsContainingComponentInExplorer);
        }

        private void mILinkedComponentOpenInWeb_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem menuItem)
                || menuItem.Tag == null
                || !(menuItem.Tag is SolutionComponent solutionComponent)
                )
            {
                return;
            }

            _service.UrlGenerator.OpenSolutionComponentInWeb((ComponentType)solutionComponent.ComponentType.Value, solutionComponent.ObjectId.Value);
        }

        private void mILinkedComponentOpenEntityListInWeb_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem menuItem)
               || menuItem.Tag == null
               || !(menuItem.Tag is SolutionComponent solutionComponent)
               )
            {
                return;
            }

            var entityMetadata = _descriptor.MetadataSource.GetEntityMetadata(solutionComponent.ObjectId.Value);

            if (entityMetadata == null
                || string.IsNullOrEmpty(entityMetadata.LogicalName)
            )
            {
                return;
            }

            _service.ConnectionData.OpenEntityInstanceListInWeb(entityMetadata.LogicalName);
        }

        private void mILinkedComponentOpenExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem menuItem)
                || menuItem.Tag == null
                || !(menuItem.Tag is SolutionComponent solutionComponent)
                )
            {
                return;
            }

            if (!WindowHelper.HasExplorer(solutionComponent.ComponentType?.Value))
            {
                return;
            }

            var componentType = (ComponentType)solutionComponent.ComponentType.Value;

            WindowHelper.OpenComponentExplorer(_iWriteToOutput, _service, _commonConfig, _descriptor, componentType, solutionComponent.ObjectId.Value);
        }

        private async void mILinkedComponentAddToCurrentSolution_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem menuItem)
                || menuItem.Tag == null
                || !(menuItem.Tag is SolutionComponent solutionComponent)
            )
            {
                return;
            }

            if (_solution == null
                || _solution.IsManaged.GetValueOrDefault()
            )
            {
                return;
            }

            await AddComponentsToSolution(false, _solution.UniqueName, new[] { solutionComponent });
        }

        private async void mILinkedComponentAddToSolutionLast_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
                && menuItem.Tag != null
                && menuItem.Tag is string solutionUniqueName
                && menuItem.Parent is MenuItem menuItemParent
                && menuItemParent.Tag != null
                && menuItemParent.Tag is SolutionComponent solutionComponent
            )
            {
                await AddComponentsToSolution(false, solutionUniqueName, new[] { solutionComponent });
            }
        }

        private async void miLinkedComponentAddToSolution_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem menuItem)
               || menuItem.Tag == null
               || !(menuItem.Tag is SolutionComponent solutionComponent)
            )
            {
                return;
            }

            await AddComponentsToSolution(true, null, new[] { solutionComponent });
        }

        private void mILinkedComponentOpenSolutionsContainingComponentInExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem menuItem)
               || menuItem.Tag == null
               || !(menuItem.Tag is SolutionComponent solutionComponent)
            )
            {
                return;
            }

            _commonConfig.Save();

            WindowHelper.OpenExplorerSolutionExplorer(
                _iWriteToOutput
                , _service
                , _commonConfig
                , solutionComponent.ComponentType.Value
                , solutionComponent.ObjectId.Value
                , null
            );
        }

        private void mILinkedComponentOpenDependentComponentsInWeb_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem menuItem)
               || menuItem.Tag == null
               || !(menuItem.Tag is SolutionComponent solutionComponent)
            )
            {
                return;
            }

            this._service.ConnectionData.OpenSolutionComponentDependentComponentsInWeb((ComponentType)solutionComponent.ComponentType.Value, solutionComponent.ObjectId.Value);
        }

        private void mILinkedComponentOpenDependentComponentsInExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem menuItem)
               || menuItem.Tag == null
               || !(menuItem.Tag is SolutionComponent solutionComponent)
            )
            {
                return;
            }

            _commonConfig.Save();

            WindowHelper.OpenSolutionComponentDependenciesExplorer(
                _iWriteToOutput
                , _service
                , _descriptor
                , _commonConfig
                , solutionComponent.ComponentType.Value
                , solutionComponent.ObjectId.Value
                , null
            );
        }

        private async void cmBComponentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            FillDataGridColumns();

            await ShowExistingSolutionComponents();
        }

        private async void cmBSolutionComponentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            await ShowExistingSolutionComponents();
        }

        private void FillDataGridColumns()
        {
            int? category = null;

            cmBComponentType.Dispatcher.Invoke(() =>
            {
                if (cmBComponentType.SelectedItem is ComponentType comp)
                {
                    category = (int)comp;
                }
            });

            TupleList<string, string> columnsForComponentType = null;

            if (category.HasValue)
            {
                columnsForComponentType = _descriptor.GetComponentColumns(category.Value);
            }

            WindowSettings winConfig = this.GetWindowsSettings();

            foreach (var item in FindChildren<DataGrid>(this))
            {
                SaveDataGridColumnsWidths(item, winConfig);
            }

            lstVSolutionComponents.Columns.Clear();

            if (columnsForComponentType != null && columnsForComponentType.Any())
            {
                foreach (var item in columnsForComponentType)
                {
                    var column = new DataGridTextColumn()
                    {
                        Header = item.Item2,
                        Binding = new Binding()
                        {
                            Converter = this._converter,
                            ConverterParameter = item.Item1,
                        },
                        Width = 120,
                    };

                    CorrectHeaderToLabel(item.Item2, column);

                    lstVSolutionComponents.Columns.Add(column);
                }
            }
            else
            {
                //< DataGridTextColumn Header = "Name" Width = "120" Binding = "{Binding Name}" />
                //< DataGridTextColumn Header = "DisplayName" Width = "120" Binding = "{Binding DisplayName}" />
                //< DataGridTextColumn Header = "ComponentType" Width = "120" Binding = "{Binding ComponentType}" />
                //< DataGridTextColumn Header = "IsManaged" Width = "120" Binding = "{Binding IsManaged}" />
                //< DataGridTextColumn Header = "IsCustomizable" Width = "120" Binding = "{Binding IsCustomizable}" />

                string[] columns = { "Name", "DisplayName", "ComponentType", "Behavior", "IsManaged", "IsCustomizable" };

                foreach (var item in columns)
                {
                    var column = new DataGridTextColumn()
                    {
                        Header = item,
                        Binding = new Binding(item),
                        Width = 120,
                    };

                    CorrectHeaderToLabel(item, column);

                    lstVSolutionComponents.Columns.Add(column);
                }
            }

            foreach (var item in FindChildren<DataGrid>(this))
            {
                LoadDataGridColumnsWidths(item, winConfig);
            }
        }

        private void mIOpenDependentComponentsInExplorer_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedSolutionComponent();

            if (entity == null)
            {
                return;
            }

            _commonConfig.Save();

            WindowHelper.OpenSolutionComponentDependenciesExplorer(
                _iWriteToOutput
                , _service
                , _descriptor
                , _commonConfig
                , entity.SolutionComponent.ComponentType.Value
                , entity.SolutionComponent.ObjectId.Value
                , null
            );
        }

        private void mIOpenSolutionsContainingComponentInExplorer_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedSolutionComponent();

            if (entity == null)
            {
                return;
            }

            _commonConfig.Save();

            WindowHelper.OpenExplorerSolutionExplorer(
                _iWriteToOutput
                , _service
                , _commonConfig
                , entity.SolutionComponent.ComponentType.Value
                , entity.SolutionComponent.ObjectId.Value
                , null
            );
        }

        private void OpenSolutionInWeb_Click(object sender, RoutedEventArgs e)
        {
            if (_solution != null)
            {
                _service.ConnectionData.OpenSolutionInWeb(_solution.Id);
            }
        }

        private async void RemoveComponentFromSolution_Click(object sender, RoutedEventArgs e)
        {
            if (GetSolutionComponentsType() != SolutionComponentsType.SolutionComponents
                || this._solution == null
                || this._solution.IsManaged.GetValueOrDefault()
            )
            {
                return;
            }

            var componentsToRemove = lstVSolutionComponents.SelectedItems.OfType<SolutionComponentViewItem>().ToList();

            var solutionComponents = componentsToRemove.Select(en => en.SolutionComponent).ToList();

            if (!solutionComponents.Any())
            {
                return;
            }

            string question = string.Format(Properties.MessageBoxStrings.AreYouSureDeleteComponentsFormat2, solutionComponents.Count, _solution.UniqueName);

            if (MessageBox.Show(question, Properties.MessageBoxStrings.QuestionTitle, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            try
            {
                ToggleControls(false, Properties.OutputStrings.InConnectionRemovingSolutionComponentsFromSolutionFormat2, _service.ConnectionData.Name, _solution.UniqueName);

                _commonConfig.Save();

                SolutionDescriptor solutionDescriptor = new SolutionDescriptor(_iWriteToOutput, _service, _descriptor);

                {
                    string fileName = EntityFileNameFormatter.GetSolutionFileName(
                        _service.ConnectionData.Name
                        , _solution.UniqueName
                        , "SolutionImage Components Backup before removing"
                        , FileExtension.txt
                    );

                    string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

                    await solutionDescriptor.CreateFileWithSolutionComponentsAsync(filePath, _solution.Id);

                    this._iWriteToOutput.WriteToOutput(_service.ConnectionData, "Created backup Solution Components in '{0}': {1}", _solution.UniqueName, filePath);
                    this._iWriteToOutput.WriteToOutputFilePathUri(_service.ConnectionData, filePath);
                }

                {
                    string fileName = EntityFileNameFormatter.GetSolutionFileName(
                        _service.ConnectionData.Name
                        , _solution.UniqueName
                        , "SolutionImage Components Backup before removing"
                        , FileExtension.xml
                    );

                    string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

                    SolutionImage solutionImage = await solutionDescriptor.CreateSolutionImageAsync(_solution.Id, _solution.UniqueName);

                    await solutionImage.SaveAsync(filePath);
                }

                {
                    string fileName = EntityFileNameFormatter.GetSolutionFileName(
                        _service.ConnectionData.Name
                        , _solution.UniqueName
                        , "SolutionImage Removing Components"
                        , FileExtension.xml
                    );

                    string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

                    SolutionImage solutionImage = await solutionDescriptor.CreateSolutionImageWithComponentsAsync(_solution.UniqueName, solutionComponents);

                    await solutionImage.SaveAsync(filePath);
                }

                SolutionComponentRepository repository = new SolutionComponentRepository(this._service);

                await repository.RemoveSolutionComponentsAsync(_solution.UniqueName, solutionComponents);

                lstVSolutionComponents.Dispatcher.Invoke(() =>
                {
                    foreach (var item in componentsToRemove)
                    {
                        _itemsSource.Remove(item);
                    }
                });

                ToggleControls(true, Properties.OutputStrings.InConnectionRemovingSolutionComponentsFromSolutionCompletedFormat2, _service.ConnectionData.Name, _solution.UniqueName);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.InConnectionRemovingSolutionComponentsFromSolutionFailedFormat2, _service.ConnectionData.Name, _solution.UniqueName);
            }
        }

        private async void miClearUnManagedSolution_Click(object sender, RoutedEventArgs e)
        {
            if (_solution == null || _solution.IsManaged.GetValueOrDefault())
            {
                return;
            }

            string question = string.Format(Properties.MessageBoxStrings.ClearSolutionFormat1, _solution.UniqueName);

            if (MessageBox.Show(question, Properties.MessageBoxStrings.QuestionTitle, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            try
            {
                ToggleControls(false, Properties.OutputStrings.InConnectionClearingSolutionFormat2, _service.ConnectionData.Name, _solution.UniqueName);

                _commonConfig.Save();

                SolutionDescriptor solutionDescriptor = new SolutionDescriptor(_iWriteToOutput, _service, _descriptor);

                {
                    string fileName = EntityFileNameFormatter.GetSolutionFileName(
                        _service.ConnectionData.Name
                        , _solution.UniqueName
                        , "Components Backup"
                        , FileExtension.txt
                    );

                    string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

                    await solutionDescriptor.CreateFileWithSolutionComponentsAsync(filePath, _solution.Id);

                    this._iWriteToOutput.WriteToOutput(_service.ConnectionData, "Created backup Solution Components in '{0}': {1}", _solution.UniqueName, filePath);
                    this._iWriteToOutput.WriteToOutputFilePathUri(_service.ConnectionData, filePath);
                }

                {
                    string fileName = EntityFileNameFormatter.GetSolutionFileName(
                        _service.ConnectionData.Name
                        , _solution.UniqueName
                        , "SolutionImage Backup before Clearing"
                        , FileExtension.xml
                    );

                    string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

                    SolutionImage solutionImage = await solutionDescriptor.CreateSolutionImageAsync(_solution.Id, _solution.UniqueName);

                    await solutionImage.SaveAsync(filePath);
                }

                SolutionComponentRepository repository = new SolutionComponentRepository(this._service);

                await repository.ClearSolutionAsync(_solution.UniqueName);

                lstVSolutionComponents.Dispatcher.Invoke(() =>
                {
                    _itemsSource.Clear();
                });

                ToggleControls(true, Properties.OutputStrings.InConnectionClearingSolutionCompletedFormat2, _service.ConnectionData.Name, _solution.UniqueName);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.InConnectionClearingSolutionFailedFormat2, _service.ConnectionData.Name, _solution.UniqueName);
            }
        }

        private void miSelectAsLastSelected_Click(object sender, RoutedEventArgs e)
        {
            if (_solution == null || _solution.IsManaged.GetValueOrDefault())
            {
                return;
            }

            _service.ConnectionData.AddLastSelectedSolution(_solution.UniqueName);

            _iWriteToOutput.WriteToOutputSolutionUri(_service.ConnectionData, _solution.UniqueName, _solution.Id);
        }

        private void miChangeSolutionInEditor_Click(object sender, RoutedEventArgs e)
        {
            if (_solution == null || _solution.IsManaged.GetValueOrDefault())
            {
                return;
            }

            WindowHelper.OpenEntityEditor(_iWriteToOutput, _service, _commonConfig, Solution.EntityLogicalName, _solution.Id);
        }

        private void ExecuteActionOnSingleSolution(Solution solution, Func<string, Solution, Task> action)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            if (solution == null)
            {
                return;
            }

            _commonConfig.CheckFolderForExportExists(_iWriteToOutput);

            try
            {
                action(_commonConfig.FolderForExport, solution);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);
            }
        }

        private void mICreateSolutionEntityDescription_Click(object sender, RoutedEventArgs e)
        {
            if (_solution != null)
            {
                ExecuteActionOnSingleSolution(_solution, PerformSolutionEntityDescription);
            }
        }

        private void mIUsedEntitiesInWorkflows_Click(object sender, RoutedEventArgs e)
        {
            if (_solution != null)
            {
                ExecuteActionOnSingleSolution(_solution, PerformCreateFileWithUsedEntitiesInWorkflows);
            }
        }

        private void mIUsedNotExistsEntitiesInWorkflows_Click(object sender, RoutedEventArgs e)
        {
            if (_solution != null)
            {
                ExecuteActionOnSingleSolution(_solution, PerformCreateFileWithUsedNotExistsEntitiesInWorkflows);
            }
        }

        private async Task PerformSolutionEntityDescription(string folder, Solution solution)
        {
            ToggleControls(false, Properties.OutputStrings.CreatingEntityDescription);

            try
            {
                SolutionRepository repository = new SolutionRepository(_service);

                var solutionFull = await repository.GetSolutionByIdAsync(solution.Id);

                string fileName = EntityFileNameFormatter.GetSolutionFileName(
                    _service.ConnectionData.Name
                    , solution.UniqueName
                    , EntityFileNameFormatter.Headers.EntityDescription
                    , FileExtension.txt
                );

                string filePath = Path.Combine(folder, FileOperations.RemoveWrongSymbols(fileName));

                await EntityDescriptionHandler.ExportEntityDescriptionAsync(filePath, solutionFull, _service.ConnectionData);

                this._iWriteToOutput.WriteToOutput(_service.ConnectionData
                    , Properties.OutputStrings.InConnectionExportedEntityDescriptionFormat3
                    , _service.ConnectionData.Name
                    , solutionFull.LogicalName
                    , filePath
                );

                this._iWriteToOutput.PerformAction(_service.ConnectionData, filePath);

                ToggleControls(true, Properties.OutputStrings.CreatingEntityDescriptionCompleted);
            }
            catch (Exception ex)
            {
                _iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.CreatingEntityDescriptionFailed);
            }
        }

        private async Task PerformCreateFileWithUsedEntitiesInWorkflows(string folder, Solution solution)
        {
            try
            {
                ToggleControls(false, Properties.OutputStrings.CreatingFileWithUsedEntitiesInWorkflowsFormat1, solution.UniqueName);

                var workflowDescriptor = new WorkflowUsedEntitiesDescriptor(_iWriteToOutput, _service, _descriptor);

                string fileName = EntityFileNameFormatter.GetSolutionFileName(
                    _service.ConnectionData.Name
                    , solution.UniqueName
                    , "UsedEntitiesInWorkflows"
                    , FileExtension.txt
                );

                string filePath = Path.Combine(folder, FileOperations.RemoveWrongSymbols(fileName));

                var stringBuider = new StringBuilder();

                await workflowDescriptor.GetDescriptionWithUsedEntitiesInSolutionWorkflowsAsync(stringBuider, solution.Id);

                File.WriteAllText(filePath, stringBuider.ToString(), new UTF8Encoding(false));

                this._iWriteToOutput.WriteToOutput(_service.ConnectionData, "Solution Used Entities was export into file '{0}'", filePath);

                this._iWriteToOutput.PerformAction(_service.ConnectionData, filePath);

                ToggleControls(true, Properties.OutputStrings.CreatingFileWithUsedEntitiesInWorkflowsCompletedFormat1, solution.UniqueName);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.CreatingFileWithUsedEntitiesInWorkflowsFailedFormat1, solution.UniqueName);
            }
        }

        private async Task PerformCreateFileWithUsedNotExistsEntitiesInWorkflows(string folder, Solution solution)
        {
            try
            {
                ToggleControls(false, Properties.OutputStrings.CreatingFileWithUsedNotExistsEntitiesInWorkflowsFormat1, solution.UniqueName);

                var workflowDescriptor = new WorkflowUsedEntitiesDescriptor(_iWriteToOutput, _service, _descriptor);

                string fileName = EntityFileNameFormatter.GetSolutionFileName(
                    _service.ConnectionData.Name
                    , solution.UniqueName
                    , "UsedNotExistsEntitiesInWorkflows"
                    , FileExtension.txt
                );

                string filePath = Path.Combine(folder, FileOperations.RemoveWrongSymbols(fileName));

                var stringBuider = new StringBuilder();

                await workflowDescriptor.GetDescriptionWithUsedNotExistsEntitiesInSolutionWorkflowsAsync(stringBuider, solution.Id);

                File.WriteAllText(filePath, stringBuider.ToString(), new UTF8Encoding(false));

                this._iWriteToOutput.WriteToOutput(_service.ConnectionData, "Solution Used Not Exists Entities was export into file '{0}'", filePath);

                this._iWriteToOutput.PerformAction(_service.ConnectionData, filePath);

                ToggleControls(true, Properties.OutputStrings.CreatingFileWithUsedNotExistsEntitiesInWorkflowsCompletedFormat1, solution.UniqueName);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.CreatingFileWithUsedNotExistsEntitiesInWorkflowsFailedFormat1, solution.UniqueName);
            }
        }

        private void mICreateSolutionImage_Click(object sender, RoutedEventArgs e)
        {
            if (_solution != null)
            {
                ExecuteActionOnSingleSolution(_solution, PerformCreateSolutionImage);
            }
        }

        private void mICreateSolutionImageAndOpenOrganizationComparer_Click(object sender, RoutedEventArgs e)
        {
            if (_solution != null)
            {
                ExecuteActionOnSingleSolution(_solution, PerformCreateSolutionImageAndOpenOrganizationComparer);
            }
        }

        private void mILoadSolutionImage_Click(object sender, RoutedEventArgs e)
        {
            if (_solution != null)
            {
                ExecuteActionOnSingleSolution(_solution, PerformLoadFromSolutionImage);
            }
        }

        private void mILoadSolutionZip_Click(object sender, RoutedEventArgs e)
        {
            if (_solution != null)
            {
                ExecuteActionOnSingleSolution(_solution, PerformLoadFromSolutionZipFile);
            }
        }

        private void mIComponentsIn_Click(object sender, RoutedEventArgs e)
        {
            if (_solution != null)
            {
                ExecuteActionOnSingleSolution(_solution, PerformCreateFileWithSolutionComponents);
            }
        }

        private void mIMissingComponentsIn_Click(object sender, RoutedEventArgs e)
        {
            if (_solution != null)
            {
                ExecuteActionOnSingleSolution(_solution, PerformShowingMissingDependencies);
            }
        }

        private void mIUninstallComponentsIn_Click(object sender, RoutedEventArgs e)
        {
            if (_solution != null)
            {
                ExecuteActionOnSingleSolution(_solution, PerformShowingDependenciesForUninstall);
            }
        }

        private async Task PerformCreateSolutionImage(string folder, Solution solution)
        {
            try
            {
                ToggleControls(false, Properties.OutputStrings.CreatingFileWithSolutionImageFormat1, solution.UniqueName);

                SolutionDescriptor solutionDescriptor = new SolutionDescriptor(_iWriteToOutput, _service, _descriptor);

                string fileName = EntityFileNameFormatter.GetSolutionFileName(
                    _service.ConnectionData.Name
                    , solution.UniqueName
                    , "SolutionImage"
                    , FileExtension.xml
                );

                string filePath = Path.Combine(folder, FileOperations.RemoveWrongSymbols(fileName));

                SolutionImage solutionImage = await solutionDescriptor.CreateSolutionImageAsync(solution.Id, solution.UniqueName);

                await solutionImage.SaveAsync(filePath);

                this._iWriteToOutput.WriteToOutput(_service.ConnectionData, Properties.OutputStrings.InConnectionExportedSolutionImageFormat2, _service.ConnectionData.Name, filePath);

                this._iWriteToOutput.PerformAction(_service.ConnectionData, filePath);

                ToggleControls(true, Properties.OutputStrings.CreatingFileWithSolutionImageCompletedFormat1, solution.UniqueName);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.CreatingFileWithSolutionImageFailedFormat1, solution.UniqueName);
            }
        }

        private async Task PerformCreateSolutionImageAndOpenOrganizationComparer(string folder, Solution solution)
        {
            try
            {
                ToggleControls(false, Properties.OutputStrings.CreatingFileWithSolutionImageFormat1, solution.UniqueName);

                SolutionDescriptor solutionDescriptor = new SolutionDescriptor(_iWriteToOutput, _service, _descriptor);

                SolutionImage solutionImage = await solutionDescriptor.CreateSolutionImageAsync(solution.Id, solution.UniqueName);

                _commonConfig.Save();

                WindowHelper.OpenOrganizationComparerWindow(_iWriteToOutput, _service.ConnectionData.ConnectionConfiguration, _commonConfig, solutionImage);

                ToggleControls(true, Properties.OutputStrings.CreatingFileWithSolutionImageCompletedFormat1, solution.UniqueName);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.CreatingFileWithSolutionImageFailedFormat1, solution.UniqueName);
            }
        }

        private async Task PerformLoadFromSolutionImage(string folder, Solution solution)
        {
            try
            {
                string selectedPath = string.Empty;
                var thread = new Thread(() =>
                {
                    try
                    {
                        var openFileDialog1 = new Microsoft.Win32.OpenFileDialog
                        {
                            Filter = "SolutionImage (.xml)|*.xml",
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
                        _iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);
                    }
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                thread.Join();

                if (string.IsNullOrEmpty(selectedPath) || !File.Exists(selectedPath))
                {
                    return;
                }

                ToggleControls(false, Properties.OutputStrings.LoadingComponentsFromSolutionImage);

                SolutionImage solutionImage = null;

                try
                {
                    solutionImage = await SolutionImage.LoadAsync(selectedPath);
                }
                catch (Exception ex)
                {
                    _iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);

                    solutionImage = null;
                }

                if (solutionImage == null)
                {
                    ToggleControls(true, Properties.OutputStrings.LoadingSolutionImageFailed);
                    return;
                }

                UpdateStatus(Properties.OutputStrings.LoadedComponentsFromSolutionImageFormat1, solutionImage.Components.Count);

                if (solutionImage.Components.Count == 0)
                {
                    ToggleControls(true, Properties.OutputStrings.NoComponentsToAdd);
                    return;
                }

                var solutionComponents = await _descriptor.GetSolutionComponentsListAsync(solutionImage.Components);

                UpdateStatus(Properties.OutputStrings.InConnectionAddingComponentsToSolutionFormat3, _service.ConnectionData.Name, solutionComponents.Count, _solution.UniqueName);

                if (solutionComponents.Count == 0)
                {
                    ToggleControls(true, Properties.OutputStrings.NoComponentsToAdd);
                    return;
                }

                _commonConfig.Save();

                this._iWriteToOutput.ActivateOutputWindow(_service.ConnectionData);

                await SolutionController.AddSolutionComponentsCollectionToSolution(_iWriteToOutput, _service, _descriptor, _commonConfig, _solution.UniqueName, solutionComponents, false);

                ToggleControls(true, Properties.OutputStrings.LoadingComponentsFromSolutionImageCompleted);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.LoadingComponentsFromSolutionImageFailed);
            }
        }

        private async Task PerformLoadFromSolutionZipFile(string folder, Solution solution)
        {
            try
            {
                string selectedPath = string.Empty;
                var thread = new Thread(() =>
                {
                    try
                    {
                        var openFileDialog1 = new Microsoft.Win32.OpenFileDialog
                        {
                            Filter = "Solution (.zip)|*.zip",
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
                        _iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);
                    }
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                thread.Join();

                if (string.IsNullOrEmpty(selectedPath) || !File.Exists(selectedPath))
                {
                    return;
                }

                ToggleControls(false, Properties.OutputStrings.LoadingComponentsFromZipFile);

                List<SolutionComponent> solutionComponents = await _descriptor.LoadSolutionComponentsFromZipFileAsync(selectedPath);

                UpdateStatus(Properties.OutputStrings.LoadedComponentsFromZipFileFormat1, solutionComponents.Count);

                if (solutionComponents.Count == 0)
                {
                    ToggleControls(true, Properties.OutputStrings.NoComponentsToAdd);
                    return;
                }

                UpdateStatus(Properties.OutputStrings.InConnectionAddingComponentsToSolutionFormat3, _service.ConnectionData.Name, solutionComponents.Count, _solution.UniqueName);

                this._iWriteToOutput.ActivateOutputWindow(_service.ConnectionData);

                await SolutionController.AddSolutionComponentsCollectionToSolution(_iWriteToOutput, _service, _descriptor, _commonConfig, _solution.UniqueName, solutionComponents, false);

                ToggleControls(true, Properties.OutputStrings.LoadingComponentsFromZipFileCompleted);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.LoadingComponentsFromZipFileFailed);
            }
        }

        private async Task PerformCreateFileWithSolutionComponents(string folder, Solution solution)
        {
            try
            {
                ToggleControls(false, Properties.OutputStrings.CreatingTextFileWithComponentsFormat1, solution.UniqueName);

                var solutionDescriptor = new SolutionDescriptor(_iWriteToOutput, _service, _descriptor);

                string fileName = EntityFileNameFormatter.GetSolutionFileName(
                    _service.ConnectionData.Name
                    , solution.UniqueName
                    , "Components"
                    , FileExtension.txt
                );

                string filePath = Path.Combine(folder, FileOperations.RemoveWrongSymbols(fileName));

                await solutionDescriptor.CreateFileWithSolutionComponentsAsync(filePath, solution.Id);

                this._iWriteToOutput.WriteToOutput(_service.ConnectionData, "Solution Components was export into file '{0}'", filePath);

                this._iWriteToOutput.PerformAction(_service.ConnectionData, filePath);

                ToggleControls(true, Properties.OutputStrings.CreatingTextFileWithComponentsCompletedFormat1, solution.UniqueName);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.CreatingTextFileWithComponentsFailedFormat1, solution.UniqueName);
            }
        }

        private async Task PerformShowingMissingDependencies(string folder, Solution solution)
        {
            try
            {
                ToggleControls(false, Properties.OutputStrings.CreatingTextFileWithMissingDependenciesFormat1, solution.UniqueName);

                SolutionDescriptor solutionDescriptor = new SolutionDescriptor(_iWriteToOutput, _service, _descriptor);

                ComponentsGroupBy showComponents = _commonConfig.ComponentsGroupBy;

                string showString = null;

                if (showComponents == ComponentsGroupBy.DependentComponents)
                {
                    showString = "dependent";
                }
                else
                {
                    showString = "required";
                }

                showString = string.Format("Missing Dependencies {0}", showString);

                string fileName = EntityFileNameFormatter.GetSolutionFileName(
                    _service.ConnectionData.Name
                    , solution.UniqueName
                    , showString
                    , FileExtension.txt
                );

                string filePath = Path.Combine(folder, FileOperations.RemoveWrongSymbols(fileName));

                await solutionDescriptor.CreateFileWithSolutionMissingDependenciesAsync(filePath, solution.Id, showComponents, showString);

                this._iWriteToOutput.WriteToOutput(_service.ConnectionData, "Solution {0} was export into file '{1}'", showString, filePath);

                this._iWriteToOutput.PerformAction(_service.ConnectionData, filePath);

                ToggleControls(true, Properties.OutputStrings.CreatingTextFileWithMissingDependenciesCompletedFormat1, solution.UniqueName);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.CreatingTextFileWithMissingDependenciesFailedFormat1, solution.UniqueName);
            }
        }

        private async Task PerformShowingDependenciesForUninstall(string folder, Solution solution)
        {
            try
            {
                ToggleControls(false, Properties.OutputStrings.CreatingTextFileWithDependenciesForUninstallFormat1, solution.UniqueName);

                SolutionDescriptor solutionDescriptor = new SolutionDescriptor(_iWriteToOutput, _service, _descriptor);

                ComponentsGroupBy showComponents = _commonConfig.ComponentsGroupBy;

                string showString = null;

                if (showComponents == ComponentsGroupBy.DependentComponents)
                {
                    showString = "dependent";
                }
                else
                {
                    showString = "required";
                }

                showString = string.Format("Dependencies for Uninstall {0}", showString);

                string fileName = EntityFileNameFormatter.GetSolutionFileName(
                    _service.ConnectionData.Name
                    , solution.UniqueName
                    , showString
                    , FileExtension.txt
                );

                string filePath = Path.Combine(folder, FileOperations.RemoveWrongSymbols(fileName));

                await solutionDescriptor.CreateFileWithSolutionDependenciesForUninstallAsync(filePath, solution.Id, showComponents, showString);

                this._iWriteToOutput.WriteToOutput(_service.ConnectionData, "Solution {0} was export into file '{1}'", showString, filePath);

                this._iWriteToOutput.PerformAction(_service.ConnectionData, filePath);

                ToggleControls(true, Properties.OutputStrings.CreatingTextFileWithDependenciesForUninstallCompletedFormat1, solution.UniqueName);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.CreatingTextFileWithDependenciesForUninstallFailedFormat1, solution.UniqueName);
            }
        }

        private void mIOpenSolutionImage_Click(object sender, RoutedEventArgs e)
        {
            _commonConfig.Save();

            WindowHelper.OpenSolutionImageWindow(this._iWriteToOutput, _service.ConnectionData, _commonConfig);
        }

        private void mIOpenSolutionDifferenceImage_Click(object sender, RoutedEventArgs e)
        {
            _commonConfig.Save();

            WindowHelper.OpenSolutionDifferenceImageWindow(this._iWriteToOutput, _service.ConnectionData, _commonConfig);
        }

        private void mIOpenOrganizationDifferenceImage_Click(object sender, RoutedEventArgs e)
        {
            _commonConfig.Save();

            WindowHelper.OpenOrganizationDifferenceImageWindow(this._iWriteToOutput, _service.ConnectionData, _commonConfig);
        }

        private void mIOpenExplorer_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedSolutionComponent();

            if (entity == null)
            {
                return;
            }

            if (!WindowHelper.HasExplorer(entity.SolutionComponent.ComponentType?.Value))
            {
                return;
            }

            var componentType = (ComponentType)entity.SolutionComponent.ComponentType.Value;

            WindowHelper.OpenComponentExplorer(_iWriteToOutput, _service, _commonConfig, _descriptor, componentType, entity.SolutionComponent.ObjectId.Value);
        }

        private void miDescriptionOptions_Click(object sender, RoutedEventArgs e)
        {
            this._optionsPopup.IsOpen = true;
            this._optionsPopup.Child.Focus();
        }

        private void Child_CloseClicked(object sender, EventArgs e)
        {
            if (_optionsPopup.IsOpen)
            {
                _optionsPopup.IsOpen = false;
            }

            this.Focus();
        }

        private void miSolutionDescription_Click(object sender, RoutedEventArgs e)
        {
            if (_solution == null || string.IsNullOrEmpty(_solution.Description))
            {
                return;
            }

            var thread = new System.Threading.Thread(() =>
            {
                try
                {
                    var title = _solution.UniqueName + " Description";

                    var form = new WindowTextField(title, title, _solution.Description, true);

                    form.ShowDialog();
                }
                catch (Exception ex)
                {
                    _iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);
                }
            });

            thread.SetApartmentState(System.Threading.ApartmentState.STA);

            thread.Start();
        }

        private void miOpenSolutionExplorer_Click(object sender, RoutedEventArgs e)
        {
            _commonConfig.Save();

            WindowHelper.OpenExplorerSolutionExplorer(
                _iWriteToOutput
                , _service
                , _commonConfig
                , null
                , null
                , null
            );
        }

        private void btnOpenInWebCustomization_Click(object sender, RoutedEventArgs e)
        {
            _service.ConnectionData.OpenCrmWebSite(OpenCrmWebSiteType.Customization);
        }

        private void btnOpenInWebSolutionList_Click(object sender, RoutedEventArgs e)
        {
            _service.ConnectionData.OpenCrmWebSite(OpenCrmWebSiteType.Solutions);
        }

        private void btnOpenInWebDefaultSolution_Click(object sender, RoutedEventArgs e)
        {
            _service.ConnectionData.OpenSolutionInWeb(Solution.Schema.InstancesUniqueId.DefaultId);
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.IsControlsEnabled;
            e.ContinueRouting = false;
        }

        private void lstVSolutionComponentsDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            RemoveComponentFromSolution_Click(null, null);
        }

        #region Clipboard

        private void mIClipboardCopyName_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<SolutionComponentViewItem>(e, ent => ent.Name);
        }

        private void mIClipboardCopyDisplayName_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<SolutionComponentViewItem>(e, ent => ent.DisplayName);
        }

        private void mIClipboardCopyObjectId_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<SolutionComponentViewItem>(e, ent => ent.SolutionComponent.ObjectId.ToString());
        }

        private void mIClipboardCopyComponentTypeCode_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<SolutionComponentViewItem>(e, ent => ent.SolutionComponent.ComponentType?.Value.ToString());
        }

        private void mIClipboardCopyComponentTypeName_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<SolutionComponentViewItem>(e, ent => ent.ComponentType);
        }

        private void mIClipboardCopyEntityId_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<SolutionComponentViewItem>(e, ent => ent.SolutionComponent.Id.ToString());
        }

        #endregion Clipboard

        private async void mIClearCache_Click(object sender, RoutedEventArgs e)
        {
            _descriptor.ClearCache();

            await ShowExistingSolutionComponents();
        }

        private async void mIOpenWebResources_Click(object sender, RoutedEventArgs e)
        {
            if (_solution != null)
            {
                await PerformOpeningSolutionWebResourcesAsync(false, OpenFilesType.All);
            }
        }

        private async void mIOpenWebResourcesInTextEditor_Click(object sender, RoutedEventArgs e)
        {
            if (_solution != null)
            {
                await PerformOpeningSolutionWebResourcesAsync(true, OpenFilesType.All);
            }
        }

        private async Task PerformOpeningSolutionWebResourcesAsync(bool openInTextEditor, OpenFilesType openFilesType)
        {
            try
            {
                ToggleControls(false, Properties.OutputStrings.OpeningSolutionWebResourcesFormat1, _solution.UniqueName);

                await SolutionController.OpenSolutionWebResources(
                    _iWriteToOutput
                    , _service
                    , _descriptor
                    , _commonConfig
                    , _solution.Id
                    , _solution.UniqueName
                    , openInTextEditor
                    , openFilesType
                );

                ToggleControls(true, Properties.OutputStrings.OpeningSolutionWebResourcesCompletedFormat1, _solution.UniqueName);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(_service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.OpeningSolutionWebResourcesFailedFormat1, _solution.UniqueName);
            }
        }
    }
}