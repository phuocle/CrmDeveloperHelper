using Microsoft.Xrm.Sdk.Metadata;
using Nav.Common.VSPackages.CrmDeveloperHelper.Controllers;
using Nav.Common.VSPackages.CrmDeveloperHelper.Entities;
using Nav.Common.VSPackages.CrmDeveloperHelper.Helpers;
using Nav.Common.VSPackages.CrmDeveloperHelper.Interfaces;
using Nav.Common.VSPackages.CrmDeveloperHelper.Model;
using Nav.Common.VSPackages.CrmDeveloperHelper.Repository;
using Nav.Common.VSPackages.CrmDeveloperHelper.UserControls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Nav.Common.VSPackages.CrmDeveloperHelper.Views
{
    public partial class WindowExplorerEntityKey : WindowWithSolutionComponentDescriptor
    {
        private readonly Popup _popupEntityMetadataFilter;
        private readonly EntityMetadataFilter _entityMetadataFilter;

        private readonly ObservableCollection<EntityMetadataListViewItem> _itemsSourceEntityList;

        private readonly ObservableCollection<EntityKeyMetadataViewItem> _itemsSourceEntityKeyList;

        private readonly Dictionary<Guid, IEnumerable<EntityMetadata>> _cacheEntityMetadata = new Dictionary<Guid, IEnumerable<EntityMetadata>>();

        private readonly Dictionary<Guid, ConcurrentDictionary<string, Task>> _cacheMetadataTask = new Dictionary<Guid, ConcurrentDictionary<string, Task>>();

        private readonly Dictionary<Guid, ConcurrentDictionary<string, IEnumerable<EntityKeyMetadataViewItem>>> _cacheEntityKeyMetadata = new Dictionary<Guid, ConcurrentDictionary<string, IEnumerable<EntityKeyMetadataViewItem>>>();

        public WindowExplorerEntityKey(
            IWriteToOutput iWriteToOutput
            , CommonConfiguration commonConfig
            , IOrganizationServiceExtented service
            , string filterEntity
        ) : base(iWriteToOutput, commonConfig, service)
        {
            this.IncreaseInit();

            InitializeComponent();

            SetInputLanguageEnglish();

            LoadFromConfig();

            _entityMetadataFilter = new EntityMetadataFilter();
            _entityMetadataFilter.CloseClicked += this.entityMetadataFilter_CloseClicked;
            this._popupEntityMetadataFilter = new Popup
            {
                Child = _entityMetadataFilter,

                PlacementTarget = lblFilterEnitity,
                Placement = PlacementMode.Bottom,
                StaysOpen = false,
                Focusable = true,
            };
            _popupEntityMetadataFilter.Closed += this.popupEntityMetadataFilter_Closed;

            txtBFilterEnitity.Text = filterEntity;
            txtBFilterEnitity.SelectionLength = 0;
            txtBFilterEnitity.SelectionStart = txtBFilterEnitity.Text.Length;

            txtBFilterEnitity.Focus();

            _itemsSourceEntityList = new ObservableCollection<EntityMetadataListViewItem>();
            lstVwEntities.ItemsSource = _itemsSourceEntityList;

            _itemsSourceEntityKeyList = new ObservableCollection<EntityKeyMetadataViewItem>();
            lstVwEntityKeys.ItemsSource = _itemsSourceEntityKeyList;

            UpdateButtonsEnable();

            cmBCurrentConnection.ItemsSource = service.ConnectionData.ConnectionConfiguration.Connections;
            cmBCurrentConnection.SelectedItem = service.ConnectionData;

            FillExplorersMenuItems();

            this.DecreaseInit();

            var task = ShowExistingEntities();
        }

        private void FillExplorersMenuItems()
        {
            var explorersHelper = new ExplorersHelper(_iWriteToOutput, _commonConfig, GetService
                , getEntityName: GetEntityName
                , getEntityMetadataList: GetEntityMetadataList
            );

            var compareWindowsHelper = new CompareWindowsHelper(_iWriteToOutput, _commonConfig, () => Tuple.Create(GetSelectedConnection(), GetSelectedConnection())
                , getEntityName: GetEntityName
            );

            explorersHelper.FillExplorers(miExplorers);
            compareWindowsHelper.FillCompareWindows(miCompareOrganizations);

            if (this.Resources.Contains("listContextMenu")
                && this.Resources["listContextMenu"] is ContextMenu listContextMenu
            )
            {
                explorersHelper.FillExplorers(listContextMenu, nameof(miExplorers));

                compareWindowsHelper.FillCompareWindows(listContextMenu, nameof(miCompareOrganizations));
            }
        }

        private string GetEntityName()
        {
            var entity = GetSelectedEntity();

            return entity?.LogicalName;
        }

        private IEnumerable<EntityMetadata> GetEntityMetadataList(Guid connectionId)
        {
            if (_cacheEntityMetadata.ContainsKey(connectionId))
            {
                return _cacheEntityMetadata[connectionId];
            }

            return null;
        }

        private void LoadFromConfig()
        {
            WindowSettings winConfig = GetWindowsSettings();

            LoadFormSettings(winConfig);
        }

        protected override void LoadConfigurationInternal(WindowSettings winConfig)
        {
            base.LoadConfigurationInternal(winConfig);

            LoadFormSettings(winConfig);
        }

        private const string paramColumnEntityWidth = "ColumnEntityWidth";

        private void LoadFormSettings(WindowSettings winConfig)
        {
            if (winConfig.DictDouble.ContainsKey(paramColumnEntityWidth))
            {
                columnEntity.Width = new GridLength(winConfig.DictDouble[paramColumnEntityWidth]);
            }
        }

        protected override void SaveConfigurationInternal(WindowSettings winConfig)
        {
            base.SaveConfigurationInternal(winConfig);

            winConfig.DictDouble[paramColumnEntityWidth] = columnEntity.Width.Value;
        }

        protected override void OnClosed(EventArgs e)
        {
            BindingOperations.ClearAllBindings(cmBCurrentConnection);

            cmBCurrentConnection.Items.DetachFromSourceCollection();

            cmBCurrentConnection.DataContext = null;
            cmBCurrentConnection.ItemsSource = null;

            base.OnClosed(e);
        }

        private void btnEntityMetadataFilter_Click(object sender, RoutedEventArgs e)
        {
            _popupEntityMetadataFilter.IsOpen = true;
            _popupEntityMetadataFilter.Child.Focus();
        }

        private async void popupEntityMetadataFilter_Closed(object sender, EventArgs e)
        {
            if (_entityMetadataFilter.FilterChanged)
            {
                await ShowExistingEntities();
            }
        }

        private void entityMetadataFilter_CloseClicked(object sender, EventArgs e)
        {
            if (_popupEntityMetadataFilter.IsOpen)
            {
                _popupEntityMetadataFilter.IsOpen = false;
                this.Focus();
            }
        }

        private ConnectionData GetSelectedConnection()
        {
            ConnectionData connectionData = null;

            cmBCurrentConnection.Dispatcher.Invoke(() =>
            {
                connectionData = cmBCurrentConnection.SelectedItem as ConnectionData;
            });

            return connectionData;
        }

        private Task<IOrganizationServiceExtented> GetService()
        {
            return GetOrganizationService(GetSelectedConnection());
        }

        private async Task ShowExistingEntities()
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            var connectionData = GetSelectedConnection();

            ToggleControls(connectionData, false, Properties.OutputStrings.LoadingEntities);

            this.Dispatcher.Invoke(() =>
            {
                _itemsSourceEntityList.Clear();
                _itemsSourceEntityKeyList.Clear();
            });

            IEnumerable<EntityMetadataListViewItem> list = Enumerable.Empty<EntityMetadataListViewItem>();

            try
            {
                var service = await GetService();

                if (service != null)
                {
                    if (!_cacheEntityMetadata.ContainsKey(service.ConnectionData.ConnectionId))
                    {
                        EntityMetadataRepository repository = new EntityMetadataRepository(service);

                        var temp = await repository.GetEntitiesForEntityAttributeExplorerAsync(EntityFilters.Entity);

                        _cacheEntityMetadata.Add(service.ConnectionData.ConnectionId, temp);
                    }

                    list = _cacheEntityMetadata[service.ConnectionData.ConnectionId].Select(e => new EntityMetadataListViewItem(e)).ToList();
                }
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(connectionData, ex);
            }

            string textName = string.Empty;

            this.Dispatcher.Invoke(() =>
            {
                textName = txtBFilterEnitity.Text.Trim().ToLower();
            });

            list = FilterEntityList(list, textName);

            this.lstVwEntities.Dispatcher.Invoke(() =>
            {
                foreach (var entity in list
                    .OrderBy(s => s.IsIntersect)
                    .ThenBy(s => s.LogicalName)
                )
                {
                    _itemsSourceEntityList.Add(entity);
                }

                if (this.lstVwEntities.Items.Count == 1)
                {
                    this.lstVwEntities.SelectedItem = this.lstVwEntities.Items[0];
                }
                else
                {
                    var entity = list.FirstOrDefault(e => string.Equals(e.LogicalName, textName, StringComparison.InvariantCultureIgnoreCase));

                    if (entity != null)
                    {
                        this.lstVwEntities.SelectedItem = entity;
                    }
                }
            });

            ToggleControls(connectionData, true, Properties.OutputStrings.LoadingEntitiesCompletedFormat1, list.Count());

            await ShowExistingEntityKeys();
        }

        private IEnumerable<EntityMetadataListViewItem> FilterEntityList(IEnumerable<EntityMetadataListViewItem> list, string textName)
        {
            list = _entityMetadataFilter.FilterList(list, i => i.EntityMetadata);

            if (!string.IsNullOrEmpty(textName))
            {
                textName = textName.ToLower();

                if (int.TryParse(textName, out int tempInt))
                {
                    list = list.Where(ent => ent.EntityMetadata.ObjectTypeCode == tempInt);
                }
                else if (Guid.TryParse(textName, out Guid tempGuid))
                {
                    list = list.Where(ent => ent.EntityMetadata.MetadataId == tempGuid);
                }
                else
                {
                    list = list
                    .Where(ent =>
                        ent.LogicalName.IndexOf(textName, StringComparison.InvariantCultureIgnoreCase) > -1
                        ||
                        (
                            ent.DisplayName != null && ent.EntityMetadata.DisplayName.LocalizedLabels
                                .Where(l => !string.IsNullOrEmpty(l.Label))
                                .Any(lbl => lbl.Label.IndexOf(textName, StringComparison.InvariantCultureIgnoreCase) > -1)
                        )

                    //|| (ent.Description != null && ent.Description.LocalizedLabels
                    //    .Where(l => !string.IsNullOrEmpty(l.Label))
                    //    .Any(lbl => lbl.Label.ToLower().Contains(textName)))

                    //|| (ent.DisplayCollectionName != null && ent.DisplayCollectionName.LocalizedLabels
                    //    .Where(l => !string.IsNullOrEmpty(l.Label))
                    //    .Any(lbl => lbl.Label.ToLower().Contains(textName)))
                    );
                }
            }

            return list;
        }

        private async Task ShowExistingEntityKeys()
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            var connectionData = GetSelectedConnection();

            ToggleControls(connectionData, false, Properties.OutputStrings.LoadingEntityKeys);

            string entityLogicalName = string.Empty;

            this.Dispatcher.Invoke(() =>
            {
                _itemsSourceEntityKeyList.Clear();

                entityLogicalName = GetSelectedEntity()?.LogicalName;
            });

            IEnumerable<EntityKeyMetadataViewItem> list = Enumerable.Empty<EntityKeyMetadataViewItem>();

            if (!string.IsNullOrEmpty(entityLogicalName))
            {
                try
                {
                    var service = await GetService();

                    if (service != null)
                    {
                        if (!_cacheEntityKeyMetadata.ContainsKey(service.ConnectionData.ConnectionId))
                        {
                            _cacheEntityKeyMetadata.Add(service.ConnectionData.ConnectionId, new ConcurrentDictionary<string, IEnumerable<EntityKeyMetadataViewItem>>(StringComparer.InvariantCultureIgnoreCase));
                        }

                        if (!_cacheMetadataTask.ContainsKey(service.ConnectionData.ConnectionId))
                        {
                            _cacheMetadataTask.Add(service.ConnectionData.ConnectionId, new ConcurrentDictionary<string, Task>(StringComparer.InvariantCultureIgnoreCase));
                        }

                        var cacheEntityKey = _cacheEntityKeyMetadata[service.ConnectionData.ConnectionId];
                        var cacheMetadataTask = _cacheMetadataTask[service.ConnectionData.ConnectionId];

                        if (!cacheEntityKey.ContainsKey(entityLogicalName))
                        {
                            if (cacheMetadataTask.ContainsKey(entityLogicalName))
                            {
                                if (cacheMetadataTask.TryGetValue(entityLogicalName, out var task))
                                {
                                    await task;
                                }
                            }
                            else
                            {
                                var task = GetEntityMetadataInformationAsync(service, entityLogicalName, cacheEntityKey, cacheMetadataTask);

                                cacheMetadataTask.TryAdd(entityLogicalName, task);

                                await task;
                            }
                        }

                        if (cacheEntityKey.ContainsKey(entityLogicalName))
                        {
                            list = cacheEntityKey[entityLogicalName];
                        }
                    }
                }
                catch (Exception ex)
                {
                    this._iWriteToOutput.WriteErrorToOutput(connectionData, ex);
                }
            }

            string textName = string.Empty;

            txtBFilterEntityKey.Dispatcher.Invoke(() =>
            {
                textName = txtBFilterEntityKey.Text.Trim().ToLower();
            });

            list = FilterEntityKeyList(list, textName);

            this.lstVwEntityKeys.Dispatcher.Invoke(() =>
            {
                foreach (var entityKey in list.OrderBy(s => s.LogicalName))
                {
                    _itemsSourceEntityKeyList.Add(entityKey);
                }

                if (this.lstVwEntityKeys.Items.Count == 1)
                {
                    this.lstVwEntityKeys.SelectedItem = this.lstVwEntityKeys.Items[0];
                }
                else
                {
                    var entityKey = list.FirstOrDefault(e => string.Equals(e.LogicalName, textName, StringComparison.InvariantCultureIgnoreCase));

                    if (entityKey != null)
                    {
                        this.lstVwEntityKeys.SelectedItem = entityKey;
                    }
                }
            });

            ToggleControls(connectionData, true, Properties.OutputStrings.LoadingEntityKeysCompletedFormat1, list.Count());
        }

        private static async Task GetEntityMetadataInformationAsync(IOrganizationServiceExtented service, string entityLogicalName, ConcurrentDictionary<string, IEnumerable<EntityKeyMetadataViewItem>> cacheEntityKey, ConcurrentDictionary<string, Task> cacheMetadataTask)
        {
            var repository = new EntityMetadataRepository(service);

            var metadata = await repository.GetEntityMetadataAttributesAsync(entityLogicalName, EntityFilters.All);

            if (metadata != null && metadata.Keys != null)
            {
                if (!cacheEntityKey.ContainsKey(entityLogicalName))
                {
                    cacheEntityKey.TryAdd(entityLogicalName, metadata.Keys.Select(e => new EntityKeyMetadataViewItem(e)).ToList());
                }
            }

            if (cacheMetadataTask.ContainsKey(entityLogicalName))
            {
                cacheMetadataTask.TryRemove(entityLogicalName, out _);
            }
        }

        private static IEnumerable<EntityKeyMetadataViewItem> FilterEntityKeyList(IEnumerable<EntityKeyMetadataViewItem> list, string textName)
        {
            if (!string.IsNullOrEmpty(textName))
            {
                textName = textName.ToLower();

                if (Guid.TryParse(textName, out Guid tempGuid))
                {
                    list = list.Where(ent => ent.EntityKeyMetadata.MetadataId == tempGuid);
                }
                else
                {
                    list = list
                    .Where(ent =>
                        ent.LogicalName.IndexOf(textName, StringComparison.InvariantCultureIgnoreCase) > -1
                        ||
                        (
                            ent.DisplayName != null
                            && ent.EntityKeyMetadata.DisplayName.LocalizedLabels
                                .Where(l => !string.IsNullOrEmpty(l.Label))
                                .Any(lbl => lbl.Label.IndexOf(textName, StringComparison.InvariantCultureIgnoreCase) > -1)
                        )

                    //|| (ent.Description != null && ent.Description.LocalizedLabels
                    //    .Where(l => !string.IsNullOrEmpty(l.Label))
                    //    .Any(lbl => lbl.Label.ToLower().Contains(textName)))

                    //|| (ent.DisplayCollectionName != null && ent.DisplayCollectionName.LocalizedLabels
                    //    .Where(l => !string.IsNullOrEmpty(l.Label))
                    //    .Any(lbl => lbl.Label.ToLower().Contains(textName)))
                    );
                }
            }

            return list;
        }

        private void UpdateStatus(ConnectionData connectionData, string format, params object[] args)
        {
            string message = format;

            if (args != null && args.Length > 0)
            {
                message = string.Format(format, args);
            }

            _iWriteToOutput.WriteToOutput(connectionData, message);

            this.stBIStatus.Dispatcher.Invoke(() =>
            {
                this.stBIStatus.Content = message;
            });
        }

        protected override void ToggleControls(ConnectionData connectionData, bool enabled, string statusFormat, params object[] args)
        {
            this.ChangeInitByEnabled(enabled);

            UpdateStatus(connectionData, statusFormat, args);

            ToggleControl(this.tSProgressBar, cmBCurrentConnection, btnSetCurrentConnection, mIClearEntityCacheAndRefresh, mIClearEntityKeyCacheAndRefresh);

            UpdateButtonsEnable();
        }

        private void UpdateButtonsEnable()
        {
            this.lstVwEntities.Dispatcher.Invoke(() =>
            {
                try
                {
                    bool enabled = this.IsControlsEnabled && this.lstVwEntities.SelectedItems.Count > 0;

                    UIElement[] list = { };

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
                await ShowExistingEntities();
            }
        }

        private async void txtBFilterEntityKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await ShowExistingEntityKeys();
            }
        }

        private EntityMetadataListViewItem GetSelectedEntity()
        {
            return this.lstVwEntities.SelectedItems.OfType<EntityMetadataListViewItem>().Count() == 1
                ? this.lstVwEntities.SelectedItems.OfType<EntityMetadataListViewItem>().SingleOrDefault() : null;
        }

        private List<EntityMetadataListViewItem> GetSelectedEntities()
        {
            List<EntityMetadataListViewItem> result = this.lstVwEntities.SelectedItems.OfType<EntityMetadataListViewItem>().ToList();

            return result;
        }

        private EntityKeyMetadataViewItem GetSelectedEntityKey()
        {
            return this.lstVwEntityKeys.SelectedItems.OfType<EntityKeyMetadataViewItem>().Count() == 1
                ? this.lstVwEntityKeys.SelectedItems.OfType<EntityKeyMetadataViewItem>().SingleOrDefault() : null;
        }

        private List<EntityKeyMetadataViewItem> GetSelectedEntityKeys()
        {
            List<EntityKeyMetadataViewItem> result = this.lstVwEntityKeys.SelectedItems.OfType<EntityKeyMetadataViewItem>().ToList();

            return result;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void lstVwEntities_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                EntityMetadataListViewItem entity = GetItemFromRoutedDataContext<EntityMetadataListViewItem>(e);
                ConnectionData connectionData = GetSelectedConnection();

                if (connectionData != null && entity != null)
                {
                    connectionData.OpenEntityMetadataInWeb(entity.EntityMetadata.MetadataId.Value);
                }
            }
        }

        private void lstVwEntityKeys_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                ConnectionData connectionData = GetSelectedConnection();

                var entity = GetSelectedEntity();

                EntityKeyMetadataViewItem entktyKey = GetItemFromRoutedDataContext<EntityKeyMetadataViewItem>(e);

                if (connectionData != null
                    && entity != null
                    && entktyKey != null
                )
                {
                    connectionData.OpenEntityKeyMetadataInWeb(entity.EntityMetadata.MetadataId.Value, entktyKey.EntityKeyMetadata.MetadataId.Value);
                }
            }
        }

        private async void btnPublishEntity_Click(object sender, RoutedEventArgs e)
        {
            var entityList = GetSelectedEntities();

            if (entityList == null || !entityList.Any())
            {
                return;
            }

            await PublishEntityAsync(GetSelectedConnection(), entityList.Select(item => item.LogicalName).ToList());
        }

        private async void lstVwEntities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ShowExistingEntityKeys();

            UpdateButtonsEnable();
        }

        protected override async Task OnRefreshList(ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            await ShowExistingEntities();
        }

        private void mIOpenEntityInWeb_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedEntity();

            if (entity == null)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                connectionData.OpenEntityMetadataInWeb(entity.EntityMetadata.MetadataId.Value);
            }
        }

        private void mIOpenEntityFetchXmlFile_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedEntity();

            if (entity == null)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                this._iWriteToOutput.OpenFetchXmlFile(connectionData, _commonConfig, entity.EntityMetadata.LogicalName);
            }
        }

        private void mIOpenEntityKeyInWeb_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedEntity();

            var entityKey = GetSelectedEntityKey();

            if (entity == null || entityKey == null)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                connectionData.OpenEntityKeyMetadataInWeb(entity.EntityMetadata.MetadataId.Value, entityKey.EntityKeyMetadata.MetadataId.Value);
            }
        }

        private void mIOpenEntityListInWeb_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedEntity();

            if (entity == null)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                connectionData.OpenEntityInstanceListInWeb(entity.LogicalName);
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

        private async Task AddToSolution(bool withSelect, string solutionUniqueName, SolutionComponent.Schema.OptionSets.rootcomponentbehavior rootComponentBehavior)
        {
            var entitiesList = GetSelectedEntities()
                .Select(item => item.EntityMetadata.MetadataId.Value);

            if (!entitiesList.Any())
            {
                return;
            }

            await AddEntityMetadataToSolution(
                GetSelectedConnection()
                , entitiesList
                , withSelect
                , solutionUniqueName
                , rootComponentBehavior
            );
        }

        private async void AddEntityKeyToCrmSolution_Click(object sender, RoutedEventArgs e)
        {
            await AddEntityKeyToSolution(true, null);
        }

        private async void AddEntityKeyToCrmSolutionLast_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
               && menuItem.Tag != null
               && menuItem.Tag is string solutionUniqueName
               )
            {
                await AddEntityKeyToSolution(false, solutionUniqueName);
            }
        }

        private async Task AddEntityKeyToSolution(bool withSelect, string solutionUniqueName)
        {
            var entityKeyList = GetSelectedEntityKeys()
                .Select(item => item.EntityKeyMetadata.MetadataId.Value)
                .ToList();

            if (!entityKeyList.Any())
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var descriptor = GetSolutionComponentDescriptor(service);

            _commonConfig.Save();

            try
            {
                this._iWriteToOutput.ActivateOutputWindow(service.ConnectionData);

                await SolutionController.AddSolutionComponentsGroupToSolution(_iWriteToOutput, service, descriptor, _commonConfig, solutionUniqueName, ComponentType.EntityKey, entityKeyList, null, withSelect);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);
            }
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu contextMenu)
            {
                var items = contextMenu.Items.OfType<Control>();

                ConnectionData connectionData = GetSelectedConnection();

                FillLastSolutionItems(connectionData, items, true, AddToCrmSolutionLastIncludeSubcomponents_Click, "contMnAddToSolutionLastIncludeSubcomponents");

                FillLastSolutionItems(connectionData, items, true, AddToCrmSolutionLastDoNotIncludeSubcomponents_Click, "contMnAddToSolutionLastDoNotIncludeSubcomponents");

                FillLastSolutionItems(connectionData, items, true, AddToCrmSolutionLastIncludeAsShellOnly_Click, "contMnAddToSolutionLastIncludeAsShellOnly");

                ActivateControls(items, connectionData.LastSelectedSolutionsUniqueName != null && connectionData.LastSelectedSolutionsUniqueName.Any(), "contMnAddToSolutionLast");
            }
        }

        private void ContextMenuEntityKey_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu contextMenu)
            {
                var items = contextMenu.Items.OfType<Control>();

                ConnectionData connectionData = GetSelectedConnection();

                FillLastSolutionItems(connectionData, items, true, AddEntityKeyToCrmSolutionLast_Click, "contMnAddToSolutionLast");
            }
        }

        private void mIOpenDependentComponentsInWeb_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedEntity();

            if (entity == null)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                connectionData.OpenSolutionComponentDependentComponentsInWeb(ComponentType.Entity, entity.EntityMetadata.MetadataId.Value);
            }
        }

        private async void mIOpenDependentComponentsInExplorer_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedEntity();

            if (entity == null)
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var descriptor = GetSolutionComponentDescriptor(service);

            _commonConfig.Save();

            WindowHelper.OpenSolutionComponentDependenciesExplorer(_iWriteToOutput, service, descriptor, _commonConfig, (int)ComponentType.Entity, entity.EntityMetadata.MetadataId.Value, null);
        }

        private async void mIOpenSolutionsContainingComponentInExplorer_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedEntity();

            if (entity == null)
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            _commonConfig.Save();

            WindowHelper.OpenExplorerSolutionExplorer(
                _iWriteToOutput
                , service
                , _commonConfig
                , (int)ComponentType.Entity
                , entity.EntityMetadata.MetadataId.Value
                , null
            );
        }

        private void mIEntityKeyOpenDependentComponentsInWeb_Click(object sender, RoutedEventArgs e)
        {
            var entityKey = GetSelectedEntityKey();

            if (entityKey == null)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                connectionData.OpenSolutionComponentDependentComponentsInWeb(ComponentType.EntityKey, entityKey.EntityKeyMetadata.MetadataId.Value);
            }
        }

        private async void mIEntityKeyOpenDependentComponentsInExplorer_Click(object sender, RoutedEventArgs e)
        {
            var entityKey = GetSelectedEntityKey();

            if (entityKey == null)
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            _commonConfig.Save();

            var descriptor = GetSolutionComponentDescriptor(service);

            WindowHelper.OpenSolutionComponentDependenciesExplorer(_iWriteToOutput, service, descriptor, _commonConfig, (int)ComponentType.EntityKey, entityKey.EntityKeyMetadata.MetadataId.Value, null);
        }

        private async void mIEntityKeyOpenSolutionsContainingComponentInExplorer_Click(object sender, RoutedEventArgs e)
        {
            var entityKey = GetSelectedEntityKey();

            if (entityKey == null)
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            _commonConfig.Save();

            WindowHelper.OpenExplorerSolutionExplorer(
                _iWriteToOutput
                , service
                , _commonConfig
                , (int)ComponentType.EntityKey
                , entityKey.EntityKeyMetadata.MetadataId.Value
                , null
            );
        }

        private async void cmBCurrentConnection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this._itemsSourceEntityList?.Clear();
                this._itemsSourceEntityKeyList?.Clear();
            });

            if (!this.IsControlsEnabled)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                UpdateButtonsEnable();

                await ShowExistingEntities();
            }
        }

        private async void mIClearEntityCacheAndRefresh_Click(object sender, RoutedEventArgs e)
        {
            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                _cacheEntityMetadata.Remove(connectionData.ConnectionId);

                UpdateButtonsEnable();

                await ShowExistingEntities();
            }
        }

        private async void mIClearEntityKeyCacheAndRefresh_Click(object sender, RoutedEventArgs e)
        {
            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                _cacheEntityKeyMetadata.Remove(connectionData.ConnectionId);

                UpdateButtonsEnable();

                await ShowExistingEntityKeys();
            }
        }

        private void btnSetCurrentConnection_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentConnection(_iWriteToOutput, GetSelectedConnection());
        }

        private async void hyperlinkPublishEntity_Click(object sender, RoutedEventArgs e)
        {
            EntityMetadataListViewItem entity = GetItemFromRoutedDataContext<EntityMetadataListViewItem>(e);

            if (entity == null)
            {
                return;
            }

            await PublishEntityAsync(GetSelectedConnection(), new[] { entity.LogicalName });
        }

        #region Clipboard EntityKey

        private void mIClipboardEntityKeyCopyName_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<EntityKeyMetadataViewItem>(e, ent => ent.LogicalName);
        }

        private void mIClipboardEntityKeyCopyDisplayName_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<EntityKeyMetadataViewItem>(e, ent => ent.DisplayName);
        }

        private void mIClipboardEntityKeyCopyKeyAttributes_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<EntityKeyMetadataViewItem>(e, ent => ent.KeyAttributesString);
        }

        private void mIClipboardEntityKeyCopyEntityKeyId_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<EntityKeyMetadataViewItem>(e, ent => ent.EntityKeyMetadata.MetadataId.ToString());
        }

        #endregion Clipboard EntityKey

        #region Clipboard Entity

        private void mIClipboardEntityCopyName_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<EntityMetadataListViewItem>(e, ent => ent.LogicalName);
        }

        private void mIClipboardEntityCopyDisplayName_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<EntityMetadataListViewItem>(e, ent => ent.DisplayName);
        }

        private void mIClipboardEntityCopyObjectTypeCode_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<EntityMetadataListViewItem>(e, ent => ent.ObjectTypeCode.ToString());
        }

        private void mIClipboardEntityCopyEntityMetadataId_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<EntityMetadataListViewItem>(e, ent => ent.EntityMetadata.MetadataId.ToString());
        }

        #endregion Clipboard Entity
    }
}