using Microsoft.Xrm.Sdk.Metadata;
using Nav.Common.VSPackages.CrmDeveloperHelper.Commands;
using Nav.Common.VSPackages.CrmDeveloperHelper.Helpers;
using Nav.Common.VSPackages.CrmDeveloperHelper.Helpers.ProxyClassGeneration.Implementations;
using Nav.Common.VSPackages.CrmDeveloperHelper.Helpers.ProxyClassGeneration.Interfaces;
using Nav.Common.VSPackages.CrmDeveloperHelper.Interfaces;
using Nav.Common.VSPackages.CrmDeveloperHelper.Model;
using Nav.Common.VSPackages.CrmDeveloperHelper.Repository;
using Nav.Common.VSPackages.CrmDeveloperHelper.UserControls;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Nav.Common.VSPackages.CrmDeveloperHelper.Views
{
    public partial class WindowOrganizationComparerEntityMetadata : WindowWithConnectionList
    {
        private readonly Popup _popupEntityMetadataOptions;
        private readonly Popup _popupFileGenerationEntityMetadataOptions;
        private readonly FileGenerationEntityMetadataCSharpOptionsControl _optionsControlFileGeneration;

        private readonly Dictionary<Guid, List<EntityMetadata>> _cacheEntityMetadata = new Dictionary<Guid, List<EntityMetadata>>();

        private readonly ObservableCollection<LinkedEntityMetadata> _itemsSource;

        public WindowOrganizationComparerEntityMetadata(
            IWriteToOutput iWriteToOutput
            , CommonConfiguration commonConfig
            , ConnectionData connection1
            , ConnectionData connection2
            , string entityFilter
        ) : base(iWriteToOutput, commonConfig, connection1)
        {
            this.IncreaseInit();

            InitializeComponent();

            SetInputLanguageEnglish();

            var child = new ExportEntityMetadataOptionsControl(_commonConfig);
            child.CloseClicked += optionsEntityMetadata_CloseClicked;
            this._popupEntityMetadataOptions = new Popup
            {
                Child = child,

                PlacementTarget = toolBarHeader,
                Placement = PlacementMode.Bottom,
                StaysOpen = false,
                Focusable = true,
            };

            _optionsControlFileGeneration = new FileGenerationEntityMetadataCSharpOptionsControl();
            _optionsControlFileGeneration.CloseClicked += this.optionsControlFileGeneration_CloseClicked;
            this._popupFileGenerationEntityMetadataOptions = new Popup
            {
                Child = _optionsControlFileGeneration,

                PlacementTarget = toolBarHeader,
                Placement = PlacementMode.Bottom,
                StaysOpen = false,
                Focusable = true,
            };

            this.Resources["ConnectionName1"] = connection1.Name;
            this.Resources["ConnectionName2"] = connection2.Name;

            FillRoleEditorLayoutTabs();

            LoadFromConfig();

            txtBFilter.Text = entityFilter;

            txtBFilter.SelectionLength = 0;
            txtBFilter.SelectionStart = txtBFilter.Text.Length;

            txtBFilter.Focus();

            this._itemsSource = new ObservableCollection<LinkedEntityMetadata>();

            this.lstVwEntities.ItemsSource = _itemsSource;

            cmBConnection1.ItemsSource = connection1.ConnectionConfiguration.Connections;
            cmBConnection1.SelectedItem = connection1;

            cmBConnection2.ItemsSource = connection1.ConnectionConfiguration.Connections;
            cmBConnection2.SelectedItem = connection2;

            FillExplorersMenuItems();

            this.DecreaseInit();

            var task = ShowExistingEntities();
        }

        private void FillExplorersMenuItems()
        {
            var explorersHelper1 = new ExplorersHelper(_iWriteToOutput, _commonConfig, GetService1
                , getEntityName: GetEntityName
                , getEntityMetadataList: GetEntityMetadataList
            );

            var explorersHelper2 = new ExplorersHelper(_iWriteToOutput, _commonConfig, GetService2
                , getEntityName: GetEntityName
                , getEntityMetadataList: GetEntityMetadataList
            );

            var compareWindowsHelper = new CompareWindowsHelper(_iWriteToOutput, _commonConfig, () => Tuple.Create(GetConnection1(), GetConnection2())
                , getEntityName: GetEntityName
            );

            explorersHelper1.FillExplorers(miExplorers1);
            explorersHelper2.FillExplorers(miExplorers2);

            compareWindowsHelper.FillCompareWindows(miCompareOrganizations);
            compareWindowsHelper.FillCompareWindows(miCompareOrganizations2);

            if (this.Resources.Contains("listContextMenu")
                && this.Resources["listContextMenu"] is ContextMenu listContextMenu
            )
            {
                explorersHelper1.FillExplorers(listContextMenu, "miExplorers1");
                explorersHelper2.FillExplorers(listContextMenu, "miExplorers2");
                compareWindowsHelper.FillCompareWindows(listContextMenu, "miCompareOrganizations");
            }
        }

        private string GetEntityName()
        {
            var entity = GetSelectedLinkedEntityMetadata();

            return entity?.LogicalName ?? txtBFilter.Text.Trim();
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
            cmBFileAction.DataContext = _commonConfig;
        }

        private void FillRoleEditorLayoutTabs()
        {
            cmBRoleEditorLayoutTabs.Items.Clear();

            cmBRoleEditorLayoutTabs.Items.Add("All");

            var tabs = RoleEditorLayoutTab.GetTabs();

            foreach (var tab in tabs)
            {
                cmBRoleEditorLayoutTabs.Items.Add(tab);
            }

            cmBRoleEditorLayoutTabs.SelectedIndex = 0;
        }

        protected override void OnClosed(EventArgs e)
        {
            _commonConfig.Save();
            FileGenerationConfiguration.SaveConfiguration();

            (cmBConnection1.SelectedItem as ConnectionData)?.Save();
            (cmBConnection2.SelectedItem as ConnectionData)?.Save();

            BindingOperations.ClearAllBindings(cmBConnection1);
            cmBConnection1.Items.DetachFromSourceCollection();
            cmBConnection1.DataContext = null;
            cmBConnection1.ItemsSource = null;

            BindingOperations.ClearAllBindings(cmBConnection2);
            cmBConnection2.Items.DetachFromSourceCollection();
            cmBConnection2.DataContext = null;
            cmBConnection2.ItemsSource = null;

            base.OnClosed(e);
        }

        private ConnectionData GetConnection1()
        {
            ConnectionData connectionData = null;

            cmBConnection1.Dispatcher.Invoke(() =>
            {
                connectionData = cmBConnection1.SelectedItem as ConnectionData;
            });

            return connectionData;
        }

        private ConnectionData GetConnection2()
        {
            ConnectionData connectionData = null;

            cmBConnection2.Dispatcher.Invoke(() =>
            {
                connectionData = cmBConnection2.SelectedItem as ConnectionData;
            });

            return connectionData;
        }

        private Task<IOrganizationServiceExtented> GetService1()
        {
            return GetOrganizationService(GetConnection1());
        }

        private Task<IOrganizationServiceExtented> GetService2()
        {
            return GetOrganizationService(GetConnection2());
        }

        private async Task ShowExistingEntities()
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            ToggleControls(false, Properties.OutputStrings.LoadingEntities);

            this.Dispatcher.Invoke(() =>
            {
                this._itemsSource.Clear();
            });

            IEnumerable<LinkedEntityMetadata> list = Enumerable.Empty<LinkedEntityMetadata>();

            try
            {
                var service1 = await GetService1();
                var service2 = await GetService2();

                if (service1 != null && service2 != null)
                {
                    var temp = new List<LinkedEntityMetadata>();

                    List<EntityMetadata> list1;
                    List<EntityMetadata> list2;

                    if (!_cacheEntityMetadata.ContainsKey(service1.ConnectionData.ConnectionId))
                    {
                        EntityMetadataRepository repository1 = new EntityMetadataRepository(service1);

                        var task1 = repository1.GetEntitiesDisplayNameAsync();

                        _cacheEntityMetadata.Add(service1.ConnectionData.ConnectionId, await task1);
                    }

                    if (!_cacheEntityMetadata.ContainsKey(service2.ConnectionData.ConnectionId))
                    {
                        EntityMetadataRepository repository2 = new EntityMetadataRepository(service2);

                        var task2 = repository2.GetEntitiesDisplayNameAsync();

                        _cacheEntityMetadata.Add(service2.ConnectionData.ConnectionId, await task2);
                    }

                    list1 = _cacheEntityMetadata[service1.ConnectionData.ConnectionId];
                    list2 = _cacheEntityMetadata[service2.ConnectionData.ConnectionId];

                    if (service1.ConnectionData.ConnectionId != service2.ConnectionData.ConnectionId)
                    {
                        foreach (var entityMetadata1 in list1)
                        {
                            var entityMetadata2 = list2.FirstOrDefault(e => e.LogicalName == entityMetadata1.LogicalName);

                            if (entityMetadata2 == null)
                            {
                                continue;
                            }

                            temp.Add(new LinkedEntityMetadata(entityMetadata1.LogicalName, entityMetadata1, entityMetadata2));
                        }
                    }
                    else
                    {
                        foreach (var entityMetadata1 in list1)
                        {
                            temp.Add(new LinkedEntityMetadata(entityMetadata1.LogicalName, entityMetadata1, null));
                        }
                    }

                    list = temp;
                }
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(null, ex);
            }

            string textName = string.Empty;
            RoleEditorLayoutTab selectedTab = null;

            this.Dispatcher.Invoke(() =>
            {
                textName = txtBFilter.Text.Trim().ToLower();
                selectedTab = cmBRoleEditorLayoutTabs.SelectedItem as RoleEditorLayoutTab;
            });

            list = FilterList(list, textName, selectedTab);

            LoadEntities(list);
        }

        private static IEnumerable<LinkedEntityMetadata> FilterList(IEnumerable<LinkedEntityMetadata> list, string textName, RoleEditorLayoutTab selectedTab)
        {
            if (selectedTab != null)
            {
                list = list.Where(ent => ent.EntityMetadata1.ObjectTypeCode.HasValue && selectedTab.EntitiesHash.Contains(ent.EntityMetadata1.ObjectTypeCode.Value));
            }

            if (!string.IsNullOrEmpty(textName))
            {
                textName = textName.ToLower();

                if (int.TryParse(textName, out int tempInt))
                {
                    list = list.Where(ent => ent.EntityMetadata1?.ObjectTypeCode == tempInt || ent.EntityMetadata2?.ObjectTypeCode == tempInt);
                }
                else if (Guid.TryParse(textName, out Guid tempGuid))
                {
                    list = list.Where(ent =>
                        ent.EntityMetadata1?.MetadataId == tempGuid
                        || ent.EntityMetadata2?.MetadataId == tempGuid
                    );
                }
                else
                {
                    list = list.Where(ent =>
                        ent.LogicalName.IndexOf(textName, StringComparison.InvariantCultureIgnoreCase) > -1

                        ||
                        (
                            ent.EntityMetadata1 != null
                            && ent.EntityMetadata1.DisplayName != null
                            && ent.EntityMetadata1.DisplayName.LocalizedLabels != null
                            && ent.EntityMetadata1.DisplayName.LocalizedLabels
                                .Where(l => !string.IsNullOrEmpty(l.Label))
                                .Any(lbl => lbl.Label.IndexOf(textName, StringComparison.InvariantCultureIgnoreCase) > -1)
                        )

                        ||
                        (
                            ent.EntityMetadata2 != null
                            && ent.EntityMetadata2.DisplayName != null
                            && ent.EntityMetadata2.DisplayName.LocalizedLabels != null
                            && ent.EntityMetadata2.DisplayName.LocalizedLabels
                                .Where(l => !string.IsNullOrEmpty(l.Label))
                                .Any(lbl => lbl.Label.IndexOf(textName, StringComparison.InvariantCultureIgnoreCase) > -1)
                        )
                    );
                }
            }

            return list;
        }

        private void LoadEntities(IEnumerable<LinkedEntityMetadata> results)
        {
            this.lstVwEntities.Dispatcher.Invoke(() =>
            {
                foreach (var entity in results)
                {
                    _itemsSource.Add(entity);
                }

                if (this.lstVwEntities.Items.Count == 1)
                {
                    this.lstVwEntities.SelectedItem = this.lstVwEntities.Items[0];
                }
            });

            ToggleControls(true, Properties.OutputStrings.LoadingEntitiesCompletedFormat1, results.Count());
        }

        private void UpdateStatus(string format, params object[] args)
        {
            string message = format;

            if (args != null && args.Length > 0)
            {
                message = string.Format(format, args);
            }

            _iWriteToOutput.WriteToOutput(null, message);

            this.stBIStatus.Dispatcher.Invoke(() =>
            {
                this.stBIStatus.Content = message;
            });
        }

        protected override void ToggleControls(ConnectionData connectionData, bool enabled, string statusFormat, params object[] args)
        {
            ToggleControls(enabled, statusFormat, args);
        }

        protected void ToggleControls(bool enabled, string statusFormat, params object[] args)
        {
            this.ChangeInitByEnabled(enabled);

            UpdateStatus(statusFormat, args);

            ToggleControl(this.tSProgressBar, this.cmBConnection1, this.cmBConnection2);

            UpdateButtonsEnable();
        }

        private void UpdateButtonsEnable()
        {
            this.lstVwEntities.Dispatcher.Invoke(() =>
            {
                try
                {
                    bool enabled = this.IsControlsEnabled && this.lstVwEntities.SelectedItems.Count > 0;

                    var item = (this.lstVwEntities.SelectedItems[0] as LinkedEntityMetadata);

                    tSDDBShowDifference.IsEnabled = enabled && item.EntityMetadata1 != null && item.EntityMetadata2 != null;

                    tSDDBConnection1.IsEnabled = enabled && item.EntityMetadata1 != null;
                    tSDDBConnection2.IsEnabled = enabled && item.EntityMetadata2 != null;
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

        private async void cmBRoleEditorLayoutTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ShowExistingEntities();
        }

        private LinkedEntityMetadata GetSelectedLinkedEntityMetadata()
        {
            return this.lstVwEntities.SelectedItems.OfType<LinkedEntityMetadata>().Count() == 1
                ? this.lstVwEntities.SelectedItems.OfType<LinkedEntityMetadata>().SingleOrDefault() : null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void lstVwEntities_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                LinkedEntityMetadata item = GetItemFromRoutedDataContext<LinkedEntityMetadata>(e);

                if (item != null)
                {
                    await ExecuteDifferenceCSharpSchema(item);
                }
            }
        }

        private void lstVwEntities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonsEnable();
        }

        private async void btnDifferenceCSharpFileSchema_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null
                || string.IsNullOrEmpty(entity.LogicalName)
            )
            {
                return;
            }

            await ExecuteDifferenceCSharpSchema(entity);
        }

        private async Task ExecuteDifferenceCSharpSchema(LinkedEntityMetadata linkedEntityMetadata)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            var service1 = await GetService1();
            var service2 = await GetService2();

            if (service1 == null || service2 == null)
            {
                return;
            }

            _commonConfig.CheckFolderForExportExists(_iWriteToOutput);

            this._iWriteToOutput.WriteToOutputStartOperation(null, Properties.OperationNames.CreatingFileWithEntityMetadataForEntityConnectionsFormat3, linkedEntityMetadata.LogicalName, service1.ConnectionData.Name, service2.ConnectionData.Name);

            ToggleControls(false, Properties.OutputStrings.ShowingDifferenceEntityMetadataCSharpForEntityFormat1, linkedEntityMetadata.LogicalName);

            var fileGenerationOptions = FileGenerationConfiguration.GetFileGenerationOptions();

            CreateFileCSharpConfiguration config = CreateFileCSharpConfiguration.CreateForSchemaEntity(fileGenerationOptions);

            string fileName1 = CreateFileWithEntityMetadataCSharpHandler.CreateFileNameForSchema(service1.ConnectionData, linkedEntityMetadata.EntityMetadata1.SchemaName, false);
            string fileName2 = CreateFileWithEntityMetadataCSharpHandler.CreateFileNameForSchema(service2.ConnectionData, linkedEntityMetadata.EntityMetadata2.SchemaName, false);

            string filePath1 = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName1));
            string filePath2 = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName2));

            var repository1 = new EntityMetadataRepository(service1);
            var repository2 = new EntityMetadataRepository(service2);

            ICodeGenerationService codeGenerationService = new CodeGenerationService(config);

            ICodeWriterFilterService codeWriterFilterService = new CodeWriterFilterService(config);

            INamingService namingService1 = new NamingService(service1.ConnectionData.ServiceContextName, config);
            INamingService namingService2 = new NamingService(service2.ConnectionData.ServiceContextName, config);

            ITypeMappingService typeMappingService = new TypeMappingService(fileGenerationOptions.NamespaceClassesCSharp);

            IMetadataProviderService metadataProviderService1 = new MetadataProviderService(repository1);
            IMetadataProviderService metadataProviderService2 = new MetadataProviderService(repository2);

            ICodeGenerationServiceProvider codeGenerationServiceProvider1 = new CodeGenerationServiceProvider(typeMappingService, codeGenerationService, codeWriterFilterService, metadataProviderService1, namingService1);
            ICodeGenerationServiceProvider codeGenerationServiceProvider2 = new CodeGenerationServiceProvider(typeMappingService, codeGenerationService, codeWriterFilterService, metadataProviderService2, namingService2);

            var stringBuilder1 = new StringBuilder();

            using (var stringWriter1 = new StringWriter(stringBuilder1))
            {
                var handler1 = new CreateFileWithEntityMetadataCSharpHandler(stringWriter1, config, service1, _iWriteToOutput, codeGenerationServiceProvider1);

                var task1 = handler1.CreateFileAsync(linkedEntityMetadata.LogicalName);

                if (service1.ConnectionData.ConnectionId != service2.ConnectionData.ConnectionId)
                {
                    var stringBuilder2 = new StringBuilder();

                    using (var stringWriter2 = new StringWriter(stringBuilder2))
                    {
                        var handler2 = new CreateFileWithEntityMetadataCSharpHandler(stringWriter2, config, service2, _iWriteToOutput, codeGenerationServiceProvider2);

                        await handler2.CreateFileAsync(linkedEntityMetadata.LogicalName);
                    }

                    File.WriteAllText(filePath2, stringBuilder2.ToString(), new UTF8Encoding(false));
                }

                await task1;
            }

            File.WriteAllText(filePath1, stringBuilder1.ToString(), new UTF8Encoding(false));

            this._iWriteToOutput.WriteToOutput(null, Properties.OutputStrings.InConnectionCreatedEntityMetadataFileFormat3, service1.ConnectionData.Name, linkedEntityMetadata.LogicalName, filePath1);

            if (service1.ConnectionData.ConnectionId != service2.ConnectionData.ConnectionId)
            {
                this._iWriteToOutput.WriteToOutput(null, Properties.OutputStrings.InConnectionCreatedEntityMetadataFileFormat3, service2.ConnectionData.Name, linkedEntityMetadata.LogicalName, filePath2);
            }

            if (File.Exists(filePath1) && File.Exists(filePath2))
            {
                await this._iWriteToOutput.ProcessStartProgramComparerAsync(service1.ConnectionData, filePath1, filePath2, Path.GetFileName(filePath1), Path.GetFileName(filePath2), service2.ConnectionData);
            }
            else
            {
                this._iWriteToOutput.PerformAction(service1.ConnectionData, filePath1);

                this._iWriteToOutput.PerformAction(service2.ConnectionData, filePath2);
            }

            ToggleControls(true, Properties.OutputStrings.ShowingDifferenceEntityMetadataCSharpForEntityCompletedFormat1, linkedEntityMetadata.LogicalName);

            this._iWriteToOutput.WriteToOutputEndOperation(null, Properties.OperationNames.CreatingFileWithEntityMetadataForEntityConnectionsFormat3, linkedEntityMetadata.LogicalName, service1.ConnectionData.Name, service2.ConnectionData.Name);
        }

        private async void btnDifferenceCSharpFileProxyClass_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null
                || string.IsNullOrEmpty(entity.LogicalName)
            )
            {
                return;
            }

            await ExecuteDifferenceCSharpProxyClass(entity);
        }

        private async Task ExecuteDifferenceCSharpProxyClass(LinkedEntityMetadata linkedEntityMetadata)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            var service1 = await GetService1();
            var service2 = await GetService2();

            if (service1 == null || service2 == null)
            {
                return;
            }

            _commonConfig.CheckFolderForExportExists(_iWriteToOutput);

            this._iWriteToOutput.WriteToOutputStartOperation(null, Properties.OperationNames.CreatingFileWithEntityMetadataForEntityConnectionsFormat3, linkedEntityMetadata.LogicalName, service1.ConnectionData.Name, service2.ConnectionData.Name);

            ToggleControls(false, Properties.OutputStrings.ShowingDifferenceEntityMetadataCSharpForEntityFormat1, linkedEntityMetadata.LogicalName);

            var fileGenerationOptions = FileGenerationConfiguration.GetFileGenerationOptions();

            CreateFileCSharpConfiguration config = CreateFileCSharpConfiguration.CreateForProxyClass(fileGenerationOptions);

            string fileName1 = CreateFileWithEntityMetadataCSharpHandler.CreateFileNameForProxy(service1.ConnectionData, linkedEntityMetadata.EntityMetadata1.SchemaName, false);
            string fileName2 = CreateFileWithEntityMetadataCSharpHandler.CreateFileNameForProxy(service2.ConnectionData, linkedEntityMetadata.EntityMetadata2.SchemaName, false);

            string filePath1 = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName1));
            string filePath2 = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName2));

            var repository1 = new EntityMetadataRepository(service1);
            var repository2 = new EntityMetadataRepository(service2);

            ICodeGenerationService codeGenerationService = new CodeGenerationService(config);

            ICodeWriterFilterService codeWriterFilterService = new CodeWriterFilterService(config);

            INamingService namingService1 = new NamingService(service1.ConnectionData.ServiceContextName, config);
            INamingService namingService2 = new NamingService(service2.ConnectionData.ServiceContextName, config);

            ITypeMappingService typeMappingService = new TypeMappingService(fileGenerationOptions.NamespaceClassesCSharp);

            IMetadataProviderService metadataProviderService1 = new MetadataProviderService(repository1);
            IMetadataProviderService metadataProviderService2 = new MetadataProviderService(repository2);

            ICodeGenerationServiceProvider codeGenerationServiceProvider1 = new CodeGenerationServiceProvider(typeMappingService, codeGenerationService, codeWriterFilterService, metadataProviderService1, namingService1);
            ICodeGenerationServiceProvider codeGenerationServiceProvider2 = new CodeGenerationServiceProvider(typeMappingService, codeGenerationService, codeWriterFilterService, metadataProviderService2, namingService2);

            var entityMetadataFull1 = await repository1.GetEntityMetadataAsync(linkedEntityMetadata.LogicalName);
            var entityMetadataFull2 = await repository2.GetEntityMetadataAsync(linkedEntityMetadata.LogicalName);

            CodeGeneratorOptions options = new CodeGeneratorOptions
            {
                BlankLinesBetweenMembers = true,
                BracingStyle = "C",
                VerbatimOrder = true,
            };

            var task1 = codeGenerationService.WriteEntityFileAsync(entityMetadataFull1, filePath1, fileGenerationOptions.NamespaceClassesCSharp, options);

            if (service1.ConnectionData.ConnectionId != service2.ConnectionData.ConnectionId)
            {
                await codeGenerationService.WriteEntityFileAsync(entityMetadataFull2, filePath2, fileGenerationOptions.NamespaceClassesCSharp, options);
            }

            this._iWriteToOutput.WriteToOutput(null, Properties.OutputStrings.InConnectionCreatedEntityMetadataFileFormat3, service1.ConnectionData.Name, linkedEntityMetadata.LogicalName, filePath1);

            if (service1.ConnectionData.ConnectionId != service2.ConnectionData.ConnectionId)
            {
                this._iWriteToOutput.WriteToOutput(null, Properties.OutputStrings.InConnectionCreatedEntityMetadataFileFormat3, service2.ConnectionData.Name, linkedEntityMetadata.LogicalName, filePath2);
            }

            if (File.Exists(filePath1) && File.Exists(filePath2))
            {
                await this._iWriteToOutput.ProcessStartProgramComparerAsync(service1.ConnectionData, filePath1, filePath2, Path.GetFileName(filePath1), Path.GetFileName(filePath2), service2.ConnectionData);
            }
            else
            {
                this._iWriteToOutput.PerformAction(service1.ConnectionData, filePath1);

                this._iWriteToOutput.PerformAction(service2.ConnectionData, filePath2);
            }

            ToggleControls(true, Properties.OutputStrings.ShowingDifferenceEntityMetadataCSharpForEntityCompletedFormat1, linkedEntityMetadata.LogicalName);

            this._iWriteToOutput.WriteToOutputEndOperation(null, Properties.OperationNames.CreatingFileWithEntityMetadataForEntityConnectionsFormat3, linkedEntityMetadata.LogicalName, service1.ConnectionData.Name, service2.ConnectionData.Name);
        }

        private async void btnDifferenceJavaScriptFile_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null
                || string.IsNullOrEmpty(entity.LogicalName)
            )
            {
                return;
            }

            await ExecuteDifferenceJavaScript(entity);
        }

        private async Task ExecuteDifferenceJavaScript(LinkedEntityMetadata linkedEntityMetadata)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            var service1 = await GetService1();
            var service2 = await GetService2();

            if (service1 == null || service2 == null)
            {
                return;
            }

            _commonConfig.CheckFolderForExportExists(_iWriteToOutput);

            this._iWriteToOutput.WriteToOutputStartOperation(null, Properties.OperationNames.CreatingFileWithEntityMetadataForEntityConnectionsFormat3, linkedEntityMetadata.LogicalName, service1.ConnectionData.Name, service2.ConnectionData.Name);

            ToggleControls(false, Properties.OutputStrings.ShowingDifferenceEntityMetadataJavaScriptForEntityFormat1, linkedEntityMetadata.LogicalName);

            string filename1 = string.Format("{0}.{1}.entitymetadata.generated.js", service1.ConnectionData.Name, linkedEntityMetadata.LogicalName);
            string filename2 = string.Format("{0}.{1}.entitymetadata.generated.js", service2.ConnectionData.Name, linkedEntityMetadata.LogicalName);

            var fileGenerationOptions = FileGenerationConfiguration.GetFileGenerationOptions();

            CreateFileJavaScriptConfiguration config = GetJavaScriptConfig(fileGenerationOptions);

            string filePath1 = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(filename1));
            string filePath2 = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(filename2));

            var stringBuilder1 = new StringBuilder();

            using (var stringWriter1 = new StringWriter(stringBuilder1))
            {
                var handler1 = new CreateFileWithEntityMetadataJavaScriptHandler(stringWriter1, config, service1, _iWriteToOutput);

                var task1 = handler1.CreateFileAsync(linkedEntityMetadata.LogicalName);

                if (service1.ConnectionData.ConnectionId != service2.ConnectionData.ConnectionId)
                {
                    var stringBuilder2 = new StringBuilder();

                    using (var stringWriter2 = new StringWriter(stringBuilder2))
                    {
                        var handler2 = new CreateFileWithEntityMetadataJavaScriptHandler(stringWriter2, config, service2, _iWriteToOutput);

                        await handler2.CreateFileAsync(linkedEntityMetadata.LogicalName);
                    }

                    File.WriteAllText(filePath2, stringBuilder2.ToString(), new UTF8Encoding(false));
                }

                await task1;
            }

            File.WriteAllText(filePath1, stringBuilder1.ToString(), new UTF8Encoding(false));

            using (var memoryStream1 = new MemoryStream())
            {
                using (var streamWriter1 = new StreamWriter(memoryStream1, new UTF8Encoding(false)))
                {


                    try
                    {
                        await streamWriter1.FlushAsync();
                        await memoryStream1.FlushAsync();

                        memoryStream1.Seek(0, SeekOrigin.Begin);

                        var fileBody = memoryStream1.ToArray();

                        File.WriteAllBytes(filePath1, fileBody);
                    }
                    catch (Exception ex)
                    {
                        DTEHelper.WriteExceptionToOutput(service1.ConnectionData, ex);
                    }
                }
            }

            this._iWriteToOutput.WriteToOutput(null, Properties.OutputStrings.InConnectionCreatedEntityMetadataFileFormat3, service1.ConnectionData.Name, linkedEntityMetadata.LogicalName, filePath1);

            if (service1.ConnectionData.ConnectionId != service2.ConnectionData.ConnectionId)
            {
                this._iWriteToOutput.WriteToOutput(null, Properties.OutputStrings.InConnectionCreatedEntityMetadataFileFormat3, service2.ConnectionData.Name, linkedEntityMetadata.LogicalName, filePath2);
            }

            if (File.Exists(filePath1) && File.Exists(filePath2))
            {
                await this._iWriteToOutput.ProcessStartProgramComparerAsync(service1.ConnectionData, filePath1, filePath2, Path.GetFileName(filePath1), Path.GetFileName(filePath2), service2.ConnectionData);
            }
            else
            {
                this._iWriteToOutput.PerformAction(service1.ConnectionData, filePath1);

                this._iWriteToOutput.PerformAction(service2.ConnectionData, filePath2);
            }

            ToggleControls(true, Properties.OutputStrings.ShowingDifferenceEntityMetadataJavaScriptForEntityCompletedFormat1, linkedEntityMetadata.LogicalName);

            this._iWriteToOutput.WriteToOutputEndOperation(null, Properties.OperationNames.CreatingFileWithEntityMetadataForEntityConnectionsFormat3, linkedEntityMetadata.LogicalName, service1.ConnectionData.Name, service2.ConnectionData.Name);
        }

        private CreateFileJavaScriptConfiguration GetJavaScriptConfig(FileGenerationOptions fileGenerationOptions)
        {
            var result = new CreateFileJavaScriptConfiguration(fileGenerationOptions);

            return result;
        }

        private async void btnConnection1CSharpSchema_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null
                || string.IsNullOrEmpty(entity.LogicalName)
            )
            {
                return;
            }

            await CreateEntityMetadataFileCSharpSchema(GetService1, entity.EntityMetadata1);
        }

        private async void btnConnection2CSharpSchema_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null
                || string.IsNullOrEmpty(entity.LogicalName)
            )
            {
                return;
            }

            await CreateEntityMetadataFileCSharpSchema(GetService2, entity.EntityMetadata2);
        }

        private async Task CreateEntityMetadataFileCSharpSchema(Func<Task<IOrganizationServiceExtented>> getService, EntityMetadata entityMetadata)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            var service = await getService();

            if (service == null)
            {
                return;
            }

            _commonConfig.CheckFolderForExportExists(_iWriteToOutput);

            this._iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, Properties.OperationNames.CreatingFileWithEntityMetadataForEntityFormat2, service.ConnectionData.Name, entityMetadata.LogicalName);

            ToggleControls(false, Properties.OutputStrings.CreatingFileForEntityFormat1, entityMetadata.LogicalName);

            var fileGenerationOptions = FileGenerationConfiguration.GetFileGenerationOptions();

            var config = CreateFileCSharpConfiguration.CreateForSchemaEntity(fileGenerationOptions);

            string fileName = CreateFileWithEntityMetadataCSharpHandler.CreateFileNameForSchema(service.ConnectionData, entityMetadata.SchemaName, false);
            string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

            try
            {
                var repository = new EntityMetadataRepository(service);

                ICodeGenerationService codeGenerationService = new CodeGenerationService(config);
                INamingService namingService = new NamingService(service.ConnectionData.ServiceContextName, config);
                ITypeMappingService typeMappingService = new TypeMappingService(fileGenerationOptions.NamespaceClassesCSharp);
                ICodeWriterFilterService codeWriterFilterService = new CodeWriterFilterService(config);
                IMetadataProviderService metadataProviderService = new MetadataProviderService(repository);

                ICodeGenerationServiceProvider codeGenerationServiceProvider = new CodeGenerationServiceProvider(typeMappingService, codeGenerationService, codeWriterFilterService, metadataProviderService, namingService);

                var stringBuilder = new StringBuilder();

                using (var stringWriter = new StringWriter(stringBuilder))
                {
                    var handler = new CreateFileWithEntityMetadataCSharpHandler(stringWriter, config, service, _iWriteToOutput, codeGenerationServiceProvider);

                    await handler.CreateFileAsync(entityMetadata.LogicalName);
                }

                File.WriteAllText(filePath, stringBuilder.ToString(), new UTF8Encoding(false));

                this._iWriteToOutput.WriteToOutput(service.ConnectionData, Properties.OutputStrings.InConnectionCreatedEntityMetadataFileFormat3, service.ConnectionData.Name, entityMetadata.LogicalName, filePath);

                this._iWriteToOutput.PerformAction(service.ConnectionData, filePath);

                ToggleControls(true, Properties.OutputStrings.CreatingFileForEntityCompletedFormat1, entityMetadata.LogicalName);
            }
            catch (Exception ex)
            {
                _iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.CreatingFileForEntityFailedFormat1, entityMetadata.LogicalName);
            }

            this._iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, Properties.OperationNames.CreatingFileWithEntityMetadataForEntityFormat2, service.ConnectionData.Name, entityMetadata.LogicalName);
        }

        private async void btnConnection1CSharpProxyClass_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null
                || string.IsNullOrEmpty(entity.LogicalName)
            )
            {
                return;
            }

            await CreateEntityMetadataFileCSharpProxyClass(GetService1, entity.EntityMetadata1);
        }

        private async void btnConnection2CSharpProxyClass_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null
                || string.IsNullOrEmpty(entity.LogicalName)
            )
            {
                return;
            }

            await CreateEntityMetadataFileCSharpProxyClass(GetService2, entity.EntityMetadata2);
        }

        private async Task CreateEntityMetadataFileCSharpProxyClass(Func<Task<IOrganizationServiceExtented>> getService, EntityMetadata entityMetadata)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            var service = await getService();

            if (service == null)
            {
                return;
            }

            _commonConfig.CheckFolderForExportExists(_iWriteToOutput);

            this._iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, Properties.OperationNames.CreatingFileWithEntityMetadataForEntityFormat2, service.ConnectionData.Name, entityMetadata.LogicalName);

            ToggleControls(false, Properties.OutputStrings.CreatingFileForEntityFormat1, entityMetadata.LogicalName);

            var fileGenerationOptions = FileGenerationConfiguration.GetFileGenerationOptions();

            var config = CreateFileCSharpConfiguration.CreateForProxyClass(fileGenerationOptions);

            string fileName = CreateFileWithEntityMetadataCSharpHandler.CreateFileNameForProxy(service.ConnectionData, entityMetadata.SchemaName, false);
            string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

            try
            {
                var repository = new EntityMetadataRepository(service);

                ICodeGenerationService codeGenerationService = new CodeGenerationService(config);
                INamingService namingService = new NamingService(service.ConnectionData.ServiceContextName, config);
                ITypeMappingService typeMappingService = new TypeMappingService(fileGenerationOptions.NamespaceClassesCSharp);
                ICodeWriterFilterService codeWriterFilterService = new CodeWriterFilterService(config);
                IMetadataProviderService metadataProviderService = new MetadataProviderService(repository);

                ICodeGenerationServiceProvider codeGenerationServiceProvider = new CodeGenerationServiceProvider(typeMappingService, codeGenerationService, codeWriterFilterService, metadataProviderService, namingService);

                var entityMetadataFull = await repository.GetEntityMetadataAsync(entityMetadata.LogicalName);

                CodeGeneratorOptions options = new CodeGeneratorOptions
                {
                    BlankLinesBetweenMembers = true,
                    BracingStyle = "C",
                    VerbatimOrder = true,
                };

                await codeGenerationService.WriteEntityFileAsync(entityMetadataFull, filePath, fileGenerationOptions.NamespaceClassesCSharp, options);

                this._iWriteToOutput.WriteToOutput(service.ConnectionData, Properties.OutputStrings.InConnectionCreatedEntityMetadataFileFormat3, service.ConnectionData.Name, entityMetadata.LogicalName, filePath);

                this._iWriteToOutput.PerformAction(service.ConnectionData, filePath);

                ToggleControls(true, Properties.OutputStrings.CreatingFileForEntityCompletedFormat1, entityMetadata.LogicalName);
            }
            catch (Exception ex)
            {
                _iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.CreatingFileForEntityFailedFormat1, entityMetadata.LogicalName);
            }

            this._iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, Properties.OperationNames.CreatingFileWithEntityMetadataForEntityFormat2, service.ConnectionData.Name, entityMetadata.LogicalName);
        }

        private async void btnConnection1JavaScript_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (string.IsNullOrEmpty(entity?.LogicalName))
            {
                return;
            }

            await CreateEntityMetadataFileJavaScript(GetService1, entity?.LogicalName);
        }

        private async void btnConnection2JavaScript_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (string.IsNullOrEmpty(entity?.LogicalName))
            {
                return;
            }

            await CreateEntityMetadataFileJavaScript(GetService2, entity?.LogicalName);
        }

        private async Task CreateEntityMetadataFileJavaScript(Func<Task<IOrganizationServiceExtented>> getService, string entityName)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            var service = await getService();

            if (service == null)
            {
                return;
            }

            _commonConfig.CheckFolderForExportExists(_iWriteToOutput);

            this._iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, Properties.OperationNames.CreatingFileWithEntityMetadataForEntityFormat2, service.ConnectionData.Name, entityName);

            ToggleControls(false, Properties.OutputStrings.CreatingFileForEntityFormat1, entityName);

            var fileGenerationOptions = FileGenerationConfiguration.GetFileGenerationOptions();

            var config = GetJavaScriptConfig(fileGenerationOptions);

            string filename = string.Format("{0}.{1}.entitymetadata.generated.js", service.ConnectionData.Name, entityName);
            string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(filename));

            try
            {
                var stringBuilder = new StringBuilder();

                using (var stringWriter = new StringWriter(stringBuilder))
                {
                    var handler = new CreateFileWithEntityMetadataJavaScriptHandler(stringWriter, config, service, _iWriteToOutput);

                    await handler.CreateFileAsync(entityName);
                }

                File.WriteAllText(filePath, stringBuilder.ToString(), new UTF8Encoding(false));

                this._iWriteToOutput.WriteToOutput(service.ConnectionData, Properties.OutputStrings.InConnectionCreatedEntityMetadataFileFormat3, service.ConnectionData.Name, entityName, filePath);

                this._iWriteToOutput.PerformAction(service.ConnectionData, filePath);

                ToggleControls(true, Properties.OutputStrings.CreatingFileForEntityCompletedFormat1, entityName);
            }
            catch (Exception ex)
            {
                _iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.CreatingFileForEntityFailedFormat1, entityName);
            }

            this._iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, Properties.OperationNames.CreatingFileWithEntityMetadataForEntityFormat2, service.ConnectionData.Name, entityName);
        }

        protected override async Task OnRefreshList(ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            await ShowExistingEntities();
        }

        protected override bool CanCloseWindow(KeyEventArgs e)
        {
            Popup[] _popupArray = new Popup[] { _popupEntityMetadataOptions, _popupFileGenerationEntityMetadataOptions };

            foreach (var popup in _popupArray)
            {
                if (popup.IsOpen)
                {
                    popup.IsOpen = false;
                    e.Handled = true;

                    return false;
                }
            }

            return true;
        }

        private void cmBCurrentConnection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            foreach (var removed in e.RemovedItems.OfType<ConnectionData>())
            {
                removed.Save();
            }

            this.Dispatcher.Invoke(() =>
            {
                this._itemsSource?.Clear();

                ConnectionData connection1 = cmBConnection1.SelectedItem as ConnectionData;
                ConnectionData connection2 = cmBConnection2.SelectedItem as ConnectionData;

                if (connection1 != null && connection2 != null)
                {
                    this.Resources["ConnectionName1"] = connection1.Name;
                    this.Resources["ConnectionName2"] = connection2.Name;

                    UpdateButtonsEnable();

                    var task = ShowExistingEntities();
                }
            });
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (!(sender is ContextMenu contextMenu))
            {
                return;
            }

            LinkedEntityMetadata linkedEntityMetadata = GetItemFromRoutedDataContext<LinkedEntityMetadata>(e);

            var items = contextMenu.Items.OfType<Control>();

            foreach (var menuContextDifference in items.Where(i =>
                string.Equals(i.Uid, "menuContextDifference", StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(i.Uid, "miCompareOrganizations", StringComparison.InvariantCultureIgnoreCase)
            ))
            {
                menuContextDifference.IsEnabled = false;
                menuContextDifference.Visibility = Visibility.Collapsed;

                if (linkedEntityMetadata != null
                     && linkedEntityMetadata.EntityMetadata1 != null
                     && linkedEntityMetadata.EntityMetadata2 != null
                )
                {
                    menuContextDifference.IsEnabled = true;
                    menuContextDifference.Visibility = Visibility.Visible;
                }
            }

            foreach (var menuContextConnection2 in items.Where(i =>
                string.Equals(i.Uid, "menuContextConnection2", StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(i.Uid, "miExplorers2", StringComparison.InvariantCultureIgnoreCase)
            ))
            {
                menuContextConnection2.IsEnabled = false;
                menuContextConnection2.Visibility = Visibility.Collapsed;

                if (linkedEntityMetadata != null
                    && linkedEntityMetadata.EntityMetadata2 != null
                )
                {
                    menuContextConnection2.IsEnabled = true;
                    menuContextConnection2.Visibility = Visibility.Visible;
                }
            }
        }

        private async void mIDifferenceEntityRibbon_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null || string.IsNullOrEmpty(entity.LogicalName))
            {
                return;
            }

            await ExecuteDifferenceEntityRibbon(entity.LogicalName);
        }

        private async Task ExecuteDifferenceEntityRibbon(string entityName)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            var service1 = await GetService1();
            var service2 = await GetService2();

            if (service1 == null || service2 == null)
            {
                return;
            }

            _commonConfig.CheckFolderForExportExists(_iWriteToOutput);

            this._iWriteToOutput.WriteToOutputStartOperation(null, Properties.OperationNames.ExportingEntityRibbonConnectionFormat3, entityName, service1.ConnectionData.Name, service2.ConnectionData.Name);

            ToggleControls(false, Properties.OutputStrings.ShowingDifferenceRibbonForEntityFormat1, entityName);

            try
            {
                string filePath1 = string.Empty;
                string filePath2 = string.Empty;

                var filter = _commonConfig.GetRibbonLocationFilters();

                var repository1 = new RibbonCustomizationRepository(service1);

                Task<string> task1 = repository1.ExportEntityRibbonAsync(entityName, filter);
                Task<string> task2 = null;

                if (service1.ConnectionData.ConnectionId != service2.ConnectionData.ConnectionId)
                {
                    var repository2 = new RibbonCustomizationRepository(service2);

                    task2 = repository2.ExportEntityRibbonAsync(entityName, filter);
                }

                if (task1 != null)
                {
                    string ribbonXml = await task1;

                    ribbonXml = ContentComparerHelper.FormatXmlByConfiguration(
                        ribbonXml
                        , _commonConfig
                        , XmlOptionsControls.RibbonXmlOptions
                        , entityName: entityName
                    );

                    string fileName = EntityFileNameFormatter.GetEntityRibbonFileName(service1.ConnectionData.Name, entityName);
                    filePath1 = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

                    File.WriteAllText(filePath1, ribbonXml, new UTF8Encoding(false));
                }

                if (task2 != null)
                {
                    string ribbonXml = await task2;

                    ribbonXml = ContentComparerHelper.FormatXmlByConfiguration(
                        ribbonXml
                        , _commonConfig
                        , XmlOptionsControls.RibbonXmlOptions
                        , entityName: entityName
                    );

                    string fileName = EntityFileNameFormatter.GetEntityRibbonFileName(service2.ConnectionData.Name, entityName);
                    filePath2 = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

                    File.WriteAllText(filePath2, ribbonXml, new UTF8Encoding(false));
                }

                this._iWriteToOutput.WriteToOutput(null, Properties.OutputStrings.InConnectionExportedEntityRibbonFormat3, service1.ConnectionData.Name, entityName, filePath1);
                if (service1.ConnectionData.ConnectionId != service2.ConnectionData.ConnectionId)
                {
                    this._iWriteToOutput.WriteToOutput(null, Properties.OutputStrings.InConnectionExportedEntityRibbonFormat3, service2.ConnectionData.Name, entityName, filePath2);
                }

                if (File.Exists(filePath1) && File.Exists(filePath2))
                {
                    await this._iWriteToOutput.ProcessStartProgramComparerAsync(service1.ConnectionData, filePath1, filePath2, Path.GetFileName(filePath1), Path.GetFileName(filePath2), service2.ConnectionData);
                }
                else
                {
                    this._iWriteToOutput.PerformAction(service1.ConnectionData, filePath1);

                    this._iWriteToOutput.PerformAction(service2.ConnectionData, filePath2);
                }

                ToggleControls(true, Properties.OutputStrings.ShowingDifferenceRibbonForEntityCompletedFormat1, entityName);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(null, ex);

                ToggleControls(true, Properties.OutputStrings.ShowingDifferenceRibbonForEntityFailedFormat1, entityName);
            }

            this._iWriteToOutput.WriteToOutputEndOperation(null, Properties.OperationNames.ExportingEntityRibbonConnectionFormat3, entityName, service1.ConnectionData.Name, service2.ConnectionData.Name);
        }

        private async void mIDifferenceEntityRibbonDiffXml_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null || string.IsNullOrEmpty(entity.LogicalName))
            {
                return;
            }

            await ExecuteDifferenceEntityRibbonDiffXml(entity);
        }

        private async Task ExecuteDifferenceEntityRibbonDiffXml(LinkedEntityMetadata entity)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            var service1 = await GetService1();
            var service2 = await GetService2();

            if (service1 == null || service2 == null)
            {
                return;
            }

            _commonConfig.CheckFolderForExportExists(_iWriteToOutput);

            this._iWriteToOutput.WriteToOutputStartOperation(null, Properties.OperationNames.ExportingEntityRibbonDiffXmlConnectionFormat3, entity.LogicalName, service1.ConnectionData.Name, service2.ConnectionData.Name);

            ToggleControls(false, Properties.OutputStrings.ShowingDifferenceRibbonDiffXmlForEntityFormat1, entity.LogicalName);

            try
            {
                string filePath1 = string.Empty;
                string filePath2 = string.Empty;

                Task<string> task1 = null;
                Task<string> task2 = null;

                {
                    var repositoryRibbonCustomization = new RibbonCustomizationRepository(service1);

                    task1 = repositoryRibbonCustomization.GetRibbonDiffXmlAsync(_iWriteToOutput, entity.EntityMetadata1, null);
                }

                if (service1.ConnectionData.ConnectionId != service2.ConnectionData.ConnectionId)
                {
                    var repositoryRibbonCustomization = new RibbonCustomizationRepository(service2);

                    task2 = repositoryRibbonCustomization.GetRibbonDiffXmlAsync(_iWriteToOutput, entity.EntityMetadata2, null);
                }

                if (task1 != null)
                {
                    string ribbonDiffXml = await task1;

                    if (!string.IsNullOrEmpty(ribbonDiffXml))
                    {
                        ribbonDiffXml = ContentComparerHelper.FormatXmlByConfiguration(
                            ribbonDiffXml
                            , _commonConfig
                            , XmlOptionsControls.RibbonXmlOptions
                            , schemaName: AbstractDynamicCommandXsdSchemas.RibbonSchema
                            , entityName: entity.LogicalName
                        );

                        string fileName1 = EntityFileNameFormatter.GetEntityRibbonDiffXmlFileName(service1.ConnectionData.Name, entity.LogicalName);
                        filePath1 = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName1));

                        File.WriteAllText(filePath1, ribbonDiffXml, new UTF8Encoding(false));
                    }
                }

                if (task2 != null)
                {
                    string ribbonDiffXml = await task2;

                    if (!string.IsNullOrEmpty(ribbonDiffXml))
                    {
                        ribbonDiffXml = ContentComparerHelper.FormatXmlByConfiguration(
                            ribbonDiffXml
                            , _commonConfig
                            , XmlOptionsControls.RibbonXmlOptions
                            , schemaName: AbstractDynamicCommandXsdSchemas.RibbonSchema
                            , entityName: entity.LogicalName
                        );

                        string fileName2 = EntityFileNameFormatter.GetEntityRibbonDiffXmlFileName(service2.ConnectionData.Name, entity.LogicalName);
                        filePath2 = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName2));

                        File.WriteAllText(filePath2, ribbonDiffXml, new UTF8Encoding(false));
                    }
                }

                this._iWriteToOutput.WriteToOutput(null, Properties.OutputStrings.InConnectionExportedEntityRibbonDiffXmlFormat3, service1.ConnectionData.Name, entity.LogicalName, filePath1);
                if (service1.ConnectionData.ConnectionId != service2.ConnectionData.ConnectionId)
                {
                    this._iWriteToOutput.WriteToOutput(null, Properties.OutputStrings.InConnectionExportedEntityRibbonDiffXmlFormat3, service2.ConnectionData.Name, entity.LogicalName, filePath2);
                }

                if (File.Exists(filePath1) && File.Exists(filePath2))
                {
                    await this._iWriteToOutput.ProcessStartProgramComparerAsync(service1.ConnectionData, filePath1, filePath2, Path.GetFileName(filePath1), Path.GetFileName(filePath2), service2.ConnectionData);
                }
                else
                {
                    this._iWriteToOutput.PerformAction(service1.ConnectionData, filePath1);

                    this._iWriteToOutput.PerformAction(service2.ConnectionData, filePath2);
                }

                ToggleControls(true, Properties.OutputStrings.ShowingDifferenceRibbonDiffXmlForEntityCompletedFormat1, entity.LogicalName);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(null, ex);

                ToggleControls(true, Properties.OutputStrings.ShowingDifferenceRibbonDiffXmlForEntityFailedFormat1, entity.LogicalName);
            }

            this._iWriteToOutput.WriteToOutputEndOperation(null, Properties.OperationNames.ExportingEntityRibbonDiffXmlConnectionFormat3, entity.LogicalName, service1.ConnectionData.Name, service2.ConnectionData.Name);
        }

        private async void mIConnection1EntityRibbon_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null || string.IsNullOrEmpty(entity.LogicalName))
            {
                return;
            }

            await ExecuteCreatingEntityRibbon(GetService1, entity.LogicalName);
        }

        private async void mIConnection2EntityRibbon_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null || string.IsNullOrEmpty(entity.LogicalName))
            {
                return;
            }

            await ExecuteCreatingEntityRibbon(GetService2, entity.LogicalName);
        }

        private async Task ExecuteCreatingEntityRibbon(Func<Task<IOrganizationServiceExtented>> getService, string entityName)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            var service = await getService();

            if (service == null)
            {
                return;
            }

            _commonConfig.CheckFolderForExportExists(_iWriteToOutput);

            this._iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, Properties.OperationNames.ExportingRibbonForEntityFormat2, service.ConnectionData.Name, entityName);

            ToggleControls(false, Properties.OutputStrings.ExportingRibbonForEntityFormat1, entityName);

            try
            {
                var repository = new RibbonCustomizationRepository(service);

                string ribbonXml = await repository.ExportEntityRibbonAsync(entityName, _commonConfig.GetRibbonLocationFilters());

                ribbonXml = ContentComparerHelper.FormatXmlByConfiguration(
                    ribbonXml
                    , _commonConfig
                    , XmlOptionsControls.RibbonXmlOptions
                    , entityName: entityName
                );

                string fileName = EntityFileNameFormatter.GetEntityRibbonFileName(service.ConnectionData.Name, entityName);
                string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

                File.WriteAllText(filePath, ribbonXml, new UTF8Encoding(false));

                this._iWriteToOutput.WriteToOutput(service.ConnectionData, Properties.OutputStrings.InConnectionExportedEntityRibbonFormat3, service.ConnectionData.Name, entityName, filePath);

                this._iWriteToOutput.PerformAction(service.ConnectionData, filePath);

                ToggleControls(true, Properties.OutputStrings.ExportingRibbonForEntityCompletedFormat1, entityName);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.ExportingRibbonForEntityFailedFormat1, entityName);
            }

            this._iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, Properties.OperationNames.ExportingRibbonForEntityFormat2, service.ConnectionData.Name, entityName);
        }

        private async void mIConnection1EntityRibbonArchive_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null || string.IsNullOrEmpty(entity.LogicalName))
            {
                return;
            }

            await ExecuteCreatingEntityRibbonArchive(GetService1, entity.LogicalName);
        }

        private async void mIConnection2EntityRibbonArchive_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null || string.IsNullOrEmpty(entity.LogicalName))
            {
                return;
            }

            await ExecuteCreatingEntityRibbonArchive(GetService2, entity.LogicalName);
        }

        private async Task ExecuteCreatingEntityRibbonArchive(Func<Task<IOrganizationServiceExtented>> getService, string entityName)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            var service = await getService();

            if (service == null)
            {
                return;
            }

            _commonConfig.CheckFolderForExportExists(_iWriteToOutput);

            this._iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, Properties.OperationNames.ExportingRibbonForEntityFormat2, service.ConnectionData.Name, entityName);

            ToggleControls(false, Properties.OutputStrings.ExportingRibbonForEntityFormat1, entityName);

            try
            {
                var repository = new RibbonCustomizationRepository(service);

                var ribbonBody = await repository.ExportEntityRibbonByteArrayAsync(entityName, _commonConfig.GetRibbonLocationFilters());

                string fileName = EntityFileNameFormatter.GetEntityRibbonFileName(service.ConnectionData.Name, entityName, FileExtension.zip);
                string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

                File.WriteAllBytes(filePath, ribbonBody);

                this._iWriteToOutput.WriteToOutput(service.ConnectionData, Properties.OutputStrings.InConnectionExportedEntityRibbonArchiveFormat3, service.ConnectionData.Name, entityName, filePath);

                this._iWriteToOutput.PerformAction(service.ConnectionData, filePath);

                ToggleControls(true, Properties.OutputStrings.ExportingRibbonForEntityCompletedFormat1, entityName);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.ExportingRibbonForEntityFailedFormat1, entityName);
            }

            this._iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, Properties.OperationNames.ExportingRibbonForEntityFormat2, service.ConnectionData.Name, entityName);
        }

        private async void mIConnection1EntityRibbonDiffXml_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null || string.IsNullOrEmpty(entity.LogicalName))
            {
                return;
            }

            await ExecuteCreatingEntityRibbonDiffXml(GetService1, entity.EntityMetadata1);
        }

        private async void mIConnection2EntityRibbonDiffXml_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null || string.IsNullOrEmpty(entity.LogicalName))
            {
                return;
            }

            await ExecuteCreatingEntityRibbonDiffXml(GetService2, entity.EntityMetadata2);
        }

        private async Task ExecuteCreatingEntityRibbonDiffXml(Func<Task<IOrganizationServiceExtented>> getService, EntityMetadata entityMetadata)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            var service = await getService();

            if (service == null)
            {
                return;
            }

            _commonConfig.CheckFolderForExportExists(_iWriteToOutput);

            this._iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, Properties.OperationNames.ExportingRibbonDiffXmlForEntityFormat2, service.ConnectionData.Name, entityMetadata.LogicalName);

            ToggleControls(false, Properties.OutputStrings.ExportingRibbonDiffXmlForEntityFormat1, entityMetadata.LogicalName);

            var repositoryRibbonCustomization = new RibbonCustomizationRepository(service);

            try
            {
                string ribbonDiffXml = await repositoryRibbonCustomization.GetRibbonDiffXmlAsync(_iWriteToOutput, entityMetadata, null);

                ribbonDiffXml = ContentComparerHelper.FormatXmlByConfiguration(
                    ribbonDiffXml
                    , _commonConfig
                    , XmlOptionsControls.RibbonXmlOptions
                    , schemaName: AbstractDynamicCommandXsdSchemas.RibbonSchema
                    , entityName: entityMetadata.LogicalName
                );

                {
                    string fileName = EntityFileNameFormatter.GetEntityRibbonDiffXmlFileName(service.ConnectionData.Name, entityMetadata.LogicalName);
                    string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

                    File.WriteAllText(filePath, ribbonDiffXml, new UTF8Encoding(false));

                    this._iWriteToOutput.WriteToOutput(service.ConnectionData, Properties.OutputStrings.InConnectionExportedEntityRibbonDiffXmlFormat3, service.ConnectionData.Name, entityMetadata.LogicalName, filePath);

                    this._iWriteToOutput.PerformAction(service.ConnectionData, filePath);
                }

                ToggleControls(true, Properties.OutputStrings.ExportingRibbonDiffXmlForEntityCompletedFormat1, entityMetadata.LogicalName);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);

                ToggleControls(true, Properties.OutputStrings.ExportingRibbonDiffXmlForEntityFailedFormat1, entityMetadata.LogicalName);
            }

            this._iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, Properties.OperationNames.ExportingRibbonDiffXmlForEntityFormat2, service.ConnectionData.Name, entityMetadata.LogicalName);
        }

        private void miExportEntityMetadataOptions_Click(object sender, RoutedEventArgs e)
        {
            _popupEntityMetadataOptions.IsOpen = true;
            _popupEntityMetadataOptions.Child.Focus();
        }

        private void optionsEntityMetadata_CloseClicked(object sender, EventArgs e)
        {
            if (_popupEntityMetadataOptions.IsOpen)
            {
                _popupEntityMetadataOptions.IsOpen = false;
                this.Focus();
            }
        }

        private void miFileGenerationEntityMetadataOptions_Click(object sender, RoutedEventArgs e)
        {
            var fileGenerationOptions = FileGenerationConfiguration.GetFileGenerationOptions();

            this._optionsControlFileGeneration.BindFileGenerationOptions(fileGenerationOptions);

            _popupFileGenerationEntityMetadataOptions.IsOpen = true;
            _popupFileGenerationEntityMetadataOptions.Child.Focus();
        }

        private void optionsControlFileGeneration_CloseClicked(object sender, EventArgs e)
        {
            if (_popupFileGenerationEntityMetadataOptions.IsOpen)
            {
                _popupFileGenerationEntityMetadataOptions.IsOpen = false;
                this.Focus();
            }
        }

        private async void miConnection1OpenEntityMetadataInWeb_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null)
            {
                return;
            }

            var service = await GetService1();

            if (service == null)
            {
                return;
            }

            service.ConnectionData.OpenEntityMetadataInWeb(entity.EntityMetadata1.MetadataId.Value);
        }

        private async void miConnection2OpenEntityMetadataInWeb_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null)
            {
                return;
            }

            var service = await GetService2();

            if (service == null)
            {
                return;
            }

            service.ConnectionData.OpenEntityMetadataInWeb(entity.EntityMetadata2.MetadataId.Value);
        }

        private async void miConnection1OpenEntityFetchXmlFile_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null)
            {
                return;
            }

            var service = await GetService1();

            if (service == null)
            {
                return;
            }

            this._iWriteToOutput.OpenFetchXmlFile(service.ConnectionData, _commonConfig, entity.EntityMetadata1.LogicalName);
        }

        private async void miConnection2OpenEntityFetchXmlFile_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null)
            {
                return;
            }

            var service = await GetService2();

            if (service == null)
            {
                return;
            }

            this._iWriteToOutput.OpenFetchXmlFile(service.ConnectionData, _commonConfig, entity.EntityMetadata2.LogicalName);
        }

        private async void miConnection1OpenEntityInstanceListInWeb_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null)
            {
                return;
            }

            var service = await GetService1();

            if (service == null)
            {
                return;
            }

            service.ConnectionData.OpenEntityInstanceListInWeb(entity.LogicalName);
        }

        private async void miConnection2OpenEntityInstanceListInWeb_Click(object sender, RoutedEventArgs e)
        {
            var entity = GetSelectedLinkedEntityMetadata();

            if (entity == null)
            {
                return;
            }

            var service = await GetService2();

            if (service == null)
            {
                return;
            }

            service.ConnectionData.OpenEntityInstanceListInWeb(entity.LogicalName);
        }
    }
}