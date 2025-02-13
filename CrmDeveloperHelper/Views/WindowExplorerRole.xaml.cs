using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Nav.Common.VSPackages.CrmDeveloperHelper.Controllers;
using Nav.Common.VSPackages.CrmDeveloperHelper.Entities;
using Nav.Common.VSPackages.CrmDeveloperHelper.Helpers;
using Nav.Common.VSPackages.CrmDeveloperHelper.Interfaces;
using Nav.Common.VSPackages.CrmDeveloperHelper.Model;
using Nav.Common.VSPackages.CrmDeveloperHelper.Repository;
using Nav.Common.VSPackages.CrmDeveloperHelper.UserControls;
using System;
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
    public partial class WindowExplorerRole : WindowWithSolutionComponentDescriptor
    {
        private string _tabSpacer = "    ";

        private readonly Popup _popupEntityMetadataFilter;
        private readonly EntityMetadataFilter _entityMetadataFilter;

        private readonly Popup _popupPrivilegeFilter;
        private readonly PrivilegeFilter _privilegeFilter;

        private readonly ObservableCollection<SystemUser> _itemsSourceSystemUsers;

        private readonly ObservableCollection<Team> _itemsSourceTeams;
        private readonly ObservableCollection<Role> _itemsSourceRoles;

        private readonly ObservableCollection<RoleEntityPrivilegeViewItem> _itemsSourceEntityPrivileges;
        private readonly ObservableCollection<RoleOtherPrivilegeViewItem> _itemsSourceOtherPrivileges;

        private readonly List<RoleEntityPrivilegeViewItem> _currentRoleEntityPrivileges;
        private readonly List<RoleOtherPrivilegeViewItem> _currentRoleOtherPrivileges;

        private readonly Dictionary<Guid, IEnumerable<EntityMetadata>> _cacheEntityMetadata = new Dictionary<Guid, IEnumerable<EntityMetadata>>();
        private readonly Dictionary<Guid, IEnumerable<Privilege>> _cachePrivileges = new Dictionary<Guid, IEnumerable<Privilege>>();

        private readonly List<PrivilegeType> _privielgeTypesAll = Enum.GetValues(typeof(PrivilegeType)).Cast<PrivilegeType>().ToList();

        public WindowExplorerRole(
            IWriteToOutput iWriteToOutput
            , CommonConfiguration commonConfig
            , IOrganizationServiceExtented service
            , IEnumerable<EntityMetadata> entityMetadataList
            , IEnumerable<Privilege> privileges
            , string filterEntity
        ) : base(iWriteToOutput, commonConfig, service)
        {
            this.IncreaseInit();

            InitializeComponent();

            SetInputLanguageEnglish();

            if (entityMetadataList != null && entityMetadataList.Any(e => e.Privileges != null && e.Privileges.Any()))
            {
                _cacheEntityMetadata[service.ConnectionData.ConnectionId] = entityMetadataList;
            }

            if (privileges != null)
            {
                _cachePrivileges[service.ConnectionData.ConnectionId] = privileges;
            }

            _entityMetadataFilter = new EntityMetadataFilter();
            _entityMetadataFilter.CloseClicked += this.entityMetadataFilter_CloseClicked;
            this._popupEntityMetadataFilter = new Popup
            {
                Child = _entityMetadataFilter,

                PlacementTarget = lblEntityFilter,
                Placement = PlacementMode.Bottom,
                StaysOpen = false,
                Focusable = true,
            };
            _popupEntityMetadataFilter.Closed += this.popupEntityMetadataFilter_Closed;

            _privilegeFilter = new PrivilegeFilter();
            _privilegeFilter.CloseClicked += this.privilegeFilter_CloseClicked;
            this._popupPrivilegeFilter = new Popup
            {
                Child = _privilegeFilter,

                PlacementTarget = lblFilterOtherPrivileges,
                Placement = PlacementMode.Bottom,
                StaysOpen = false,
                Focusable = true,
            };
            _popupPrivilegeFilter.Closed += this.popupPrivilegeFilter_Closed;

            LoadFromConfig();

            txtBFilterRole.Text = filterEntity;
            txtBFilterRole.SelectionLength = 0;
            txtBFilterRole.SelectionStart = txtBFilterRole.Text.Length;

            txtBFilterRole.Focus();

            lstVwSecurityRoles.ItemsSource = _itemsSourceRoles = new ObservableCollection<Role>();

            lstVwSystemUsers.ItemsSource = _itemsSourceSystemUsers = new ObservableCollection<SystemUser>();
            lstVwTeams.ItemsSource = _itemsSourceTeams = new ObservableCollection<Team>();

            lstVwEntityPrivileges.ItemsSource = _itemsSourceEntityPrivileges = new ObservableCollection<RoleEntityPrivilegeViewItem>();
            lstVwOtherPrivileges.ItemsSource = _itemsSourceOtherPrivileges = new ObservableCollection<RoleOtherPrivilegeViewItem>();

            _currentRoleEntityPrivileges = new List<RoleEntityPrivilegeViewItem>();
            _currentRoleOtherPrivileges = new List<RoleOtherPrivilegeViewItem>();

            cmBCurrentConnection.ItemsSource = service.ConnectionData.ConnectionConfiguration.Connections;
            cmBCurrentConnection.SelectedItem = service.ConnectionData;

            FillExplorersMenuItems();

            this.DecreaseInit();

            var task = ShowExistingRoles();
        }

        private void FillExplorersMenuItems()
        {
            var explorersHelper = new ExplorersHelper(_iWriteToOutput, _commonConfig, GetService
                , getEntityName: GetEntityName
                , getOtherPrivilegeName: GetOtherPrivilegeName
                , getEntityMetadataList: GetEntityMetadataList
                , getOtherPrivilegesList: GetOtherPrivilegesList
            );

            var compareWindowsHelper = new CompareWindowsHelper(_iWriteToOutput, _commonConfig, () => Tuple.Create(GetSelectedConnection(), GetSelectedConnection())
                , getEntityName: GetEntityName
            );

            explorersHelper.FillExplorers(miExplorers);
            compareWindowsHelper.FillCompareWindows(miCompareOrganizations);

            if (this.Resources.Contains("listContextMenuEntityPrivileges")
                && this.Resources["listContextMenuEntityPrivileges"] is ContextMenu listContextMenuEntityPrivileges
            )
            {
                explorersHelper.FillExplorers(listContextMenuEntityPrivileges, nameof(miExplorers));

                compareWindowsHelper.FillCompareWindows(listContextMenuEntityPrivileges, nameof(miCompareOrganizations));

                AddMenuItemClickHandler(listContextMenuEntityPrivileges, explorersHelper.miEntityPrivilegesExplorer_Click, "miEntityPrivilegesExplorer");
            }

            if (this.Resources.Contains("listContextMenuOtherPrivileges")
                && this.Resources["listContextMenuOtherPrivileges"] is ContextMenu listContextMenuOtherPrivileges
            )
            {
                AddMenuItemClickHandler(listContextMenuOtherPrivileges, explorersHelper.miOtherPrivilegesExplorer_Click, "mIOpenOtherPrivilegeExplorer");
            }
        }

        private string GetEntityName()
        {
            var entity = GetSelectedEntity();

            return entity?.LogicalName;
        }

        private string GetOtherPrivilegeName()
        {
            var privilege = GetSelectedOtherPrivilege();

            return privilege?.Name;
        }

        private IEnumerable<EntityMetadata> GetEntityMetadataList(Guid connectionId)
        {
            if (_cacheEntityMetadata.ContainsKey(connectionId))
            {
                return _cacheEntityMetadata[connectionId];
            }

            return null;
        }

        private IEnumerable<Privilege> GetOtherPrivilegesList(Guid connectionId)
        {
            if (_cachePrivileges.ContainsKey(connectionId))
            {
                return _cachePrivileges[connectionId];
            }

            return null;
        }

        private void LoadFromConfig()
        {
            WindowSettings winConfig = GetWindowsSettings();

            LoadFormSettings(winConfig);
        }

        private void btnEntityMetadataFilter_Click(object sender, RoutedEventArgs e)
        {
            _popupEntityMetadataFilter.IsOpen = true;
            _popupEntityMetadataFilter.Child.Focus();
        }

        private void popupEntityMetadataFilter_Closed(object sender, EventArgs e)
        {
            if (_entityMetadataFilter.FilterChanged)
            {
                PerformFilterEntityPrivileges();
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

        private void btnPrivilegeFilter_Click(object sender, RoutedEventArgs e)
        {
            _popupPrivilegeFilter.IsOpen = true;
            _popupPrivilegeFilter.Child.Focus();
        }

        private void popupPrivilegeFilter_Closed(object sender, EventArgs e)
        {
            if (_privilegeFilter.FilterChanged)
            {
                PerformFilterOtherPrivileges();
            }
        }

        private void privilegeFilter_CloseClicked(object sender, EventArgs e)
        {
            if (_popupPrivilegeFilter.IsOpen)
            {
                _popupPrivilegeFilter.IsOpen = false;
                this.Focus();
            }
        }

        protected override void LoadConfigurationInternal(WindowSettings winConfig)
        {
            base.LoadConfigurationInternal(winConfig);

            LoadFormSettings(winConfig);
        }

        private const string paramColumnRoleWidth = "ColumnRoleWidth";

        private void LoadFormSettings(WindowSettings winConfig)
        {
            if (winConfig.DictDouble.ContainsKey(paramColumnRoleWidth))
            {
                columnRole.Width = new GridLength(winConfig.DictDouble[paramColumnRoleWidth]);
            }
        }

        protected override void SaveConfigurationInternal(WindowSettings winConfig)
        {
            base.SaveConfigurationInternal(winConfig);

            winConfig.DictDouble[paramColumnRoleWidth] = columnRole.Width.Value;
        }

        protected override void OnClosed(EventArgs e)
        {
            BindingOperations.ClearAllBindings(cmBCurrentConnection);

            cmBCurrentConnection.Items.DetachFromSourceCollection();

            cmBCurrentConnection.DataContext = null;
            cmBCurrentConnection.ItemsSource = null;

            base.OnClosed(e);
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

        private async Task ShowRoleSystemUsers()
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            ToggleControls(connectionData, false, Properties.OutputStrings.LoadingRoleSystemUsers);

            string textName = string.Empty;

            this.Dispatcher.Invoke(() =>
            {
                _itemsSourceSystemUsers.Clear();

                textName = txtBFilterSystemUser.Text.Trim().ToLower();
            });

            IEnumerable<SystemUser> list = Enumerable.Empty<SystemUser>();

            var role = GetSelectedRole();

            try
            {
                var service = await GetService();

                if (service != null && role != null)
                {
                    var repository = new SystemUserRepository(service);

                    list = await repository.GetUsersByRoleAsync(role.Id, textName, new ColumnSet(
                        SystemUser.Schema.Attributes.fullname
                        , SystemUser.Schema.Attributes.domainname
                        , SystemUser.Schema.Attributes.businessunitid
                        , SystemUser.Schema.Attributes.isdisabled
                    ));
                }
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(connectionData, ex); ;
            }

            this.lstVwSystemUsers.Dispatcher.Invoke(() =>
            {
                foreach (var entity in list.OrderBy(s => s.FullName))
                {
                    _itemsSourceSystemUsers.Add(entity);
                }

                if (this.lstVwSystemUsers.Items.Count == 1)
                {
                    this.lstVwSystemUsers.SelectedItem = this.lstVwSystemUsers.Items[0];
                }
            });

            ToggleControls(connectionData, true, Properties.OutputStrings.LoadingRoleSystemUsersCompletedFormat1, list.Count());
        }

        private async Task ShowExistingRoles()
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            ToggleControls(connectionData, false, Properties.OutputStrings.LoadingSecurityRoles);

            string filterRole = string.Empty;

            this.Dispatcher.Invoke(() =>
            {
                _itemsSourceTeams.Clear();
                _itemsSourceSystemUsers.Clear();
                _itemsSourceRoles.Clear();

                _itemsSourceEntityPrivileges.Clear();
                _itemsSourceOtherPrivileges.Clear();

                _currentRoleEntityPrivileges.Clear();
                _currentRoleOtherPrivileges.Clear();

                filterRole = txtBFilterRole.Text.Trim().ToLower();
            });

            IEnumerable<Role> list = Enumerable.Empty<Role>();

            try
            {
                var service = await GetService();

                if (service != null)
                {
                    RoleRepository repository = new RoleRepository(service);

                    list = await repository.GetListAsync(filterRole
                        , new ColumnSet
                        (
                            Role.Schema.Attributes.name
                            , Role.Schema.Attributes.businessunitid
                            , Role.Schema.Attributes.ismanaged
                            , Role.Schema.Attributes.iscustomizable
                        )
                    );
                }
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(connectionData, ex); ;
            }

            this.lstVwSecurityRoles.Dispatcher.Invoke(() =>
            {
                foreach (var entity in list.OrderBy(s => s.Name))
                {
                    _itemsSourceRoles.Add(entity);
                }

                if (this.lstVwSecurityRoles.Items.Count == 1)
                {
                    this.lstVwSecurityRoles.SelectedItem = this.lstVwSecurityRoles.Items[0];
                }
            });

            ToggleControls(connectionData, true, Properties.OutputStrings.LoadingSecurityRolesCompletedFormat1, list.Count());

            await RefreshRoleInfo();
        }

        private async Task ShowRoleTeams()
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            ToggleControls(connectionData, false, Properties.OutputStrings.LoadingRoleTeams);

            string filterTeam = string.Empty;

            this.Dispatcher.Invoke(() =>
            {
                _itemsSourceTeams.Clear();

                filterTeam = txtBFilterTeams.Text.Trim().ToLower();
            });

            var role = GetSelectedRole();

            IEnumerable<Team> list = Enumerable.Empty<Team>();

            try
            {
                var service = await GetService();

                if (service != null && role != null)
                {
                    var repository = new TeamRepository(service);

                    list = await repository.GetTeamsByRoleAsync(role.Id, filterTeam, new ColumnSet(
                        Team.Schema.Attributes.name
                        , Team.Schema.Attributes.businessunitid
                        , Team.Schema.Attributes.teamtype
                        , Team.Schema.Attributes.regardingobjectid
                        , Team.Schema.Attributes.teamtemplateid
                        , Team.Schema.Attributes.isdefault
                    ));
                }
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(connectionData, ex);
            }

            this.lstVwTeams.Dispatcher.Invoke(() =>
            {
                foreach (var entity in list.OrderBy(s => s.TeamType?.Value).ThenBy(s => s.Name))
                {
                    _itemsSourceTeams.Add(entity);
                }

                if (this.lstVwTeams.Items.Count == 1)
                {
                    this.lstVwTeams.SelectedItem = this.lstVwTeams.Items[0];
                }
            });

            ToggleControls(connectionData, true, Properties.OutputStrings.LoadingRoleTeamsCompletedFormat1, list.Count());
        }

        private async Task ShowRolePrivileges()
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            ToggleControls(connectionData, false, Properties.OutputStrings.LoadingEntities);

            this.Dispatcher.Invoke(() =>
            {
                _itemsSourceEntityPrivileges.Clear();
                _itemsSourceOtherPrivileges.Clear();

                _currentRoleEntityPrivileges.Clear();
                _currentRoleOtherPrivileges.Clear();
            });

            var role = GetSelectedRole();

            try
            {
                var service = await GetService();

                if (service != null)
                {
                    IEnumerable<Privilege> otherPrivileges = await GetOtherPrivileges(service);

                    IEnumerable<EntityMetadata> entityMetadataList = await GetEntityMetadataEnumerable(service);

                    entityMetadataList = entityMetadataList.Where(e => e.Privileges != null && e.Privileges.Any(p => p.PrivilegeType != PrivilegeType.None));

                    if (role != null)
                    {
                        if (entityMetadataList.Any() || otherPrivileges.Any())
                        {
                            var repository = new RolePrivilegesRepository(service);

                            var rolePrivileges = await repository.GetRolePrivilegesAsync(role.Id);

                            _currentRoleEntityPrivileges.AddRange(entityMetadataList.Select(e => new RoleEntityPrivilegeViewItem(e, rolePrivileges, (role.IsCustomizable?.Value).GetValueOrDefault())));

                            _currentRoleOtherPrivileges.AddRange(otherPrivileges.Select(e => new RoleOtherPrivilegeViewItem(e, rolePrivileges, (role.IsCustomizable?.Value).GetValueOrDefault())));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(connectionData, ex);
            }

            ToggleControls(connectionData, true, Properties.OutputStrings.LoadingEntitiesCompletedFormat1, _currentRoleEntityPrivileges.Count);

            PerformFilterEntityPrivileges();

            PerformFilterOtherPrivileges();
        }

        private void PerformFilterEntityPrivileges()
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            ToggleControls(connectionData, false, Properties.OutputStrings.FilteringEntities);

            string filterEntity = string.Empty;

            this.Dispatcher.Invoke(() =>
            {
                _itemsSourceEntityPrivileges.Clear();

                filterEntity = txtBEntityFilter.Text.Trim().ToLower();
            });

            IEnumerable<RoleEntityPrivilegeViewItem> listEntityPrivileges = FilterEntityList(_currentRoleEntityPrivileges, filterEntity);

            this.lstVwEntityPrivileges.Dispatcher.Invoke(() =>
            {
                foreach (var entity in listEntityPrivileges
                    .OrderBy(s => s.IsIntersect)
                    .ThenBy(s => s.LogicalName)
                )
                {
                    entity.PropertyChanged -= rolePrivilege_PropertyChanged;
                    entity.PropertyChanged -= rolePrivilege_PropertyChanged;
                    entity.PropertyChanged += rolePrivilege_PropertyChanged;

                    _itemsSourceEntityPrivileges.Add(entity);
                }

                if (this.lstVwEntityPrivileges.Items.Count == 1)
                {
                    this.lstVwEntityPrivileges.SelectedItem = this.lstVwEntityPrivileges.Items[0];
                }
            });

            ToggleControls(connectionData, true, Properties.OutputStrings.LoadingEntitiesCompletedFormat1, listEntityPrivileges.Count());
        }

        private void PerformFilterOtherPrivileges()
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            ToggleControls(connectionData, false, Properties.OutputStrings.FilteringOtherPrivileges);

            string filterOtherPrivilege = string.Empty;

            this.Dispatcher.Invoke(() =>
            {
                _itemsSourceOtherPrivileges.Clear();
                filterOtherPrivilege = txtBOtherPrivilegesFilter.Text.Trim().ToLower();
            });

            IEnumerable<RoleOtherPrivilegeViewItem> listOtherPrivileges = FilterPrivilegeList(_currentRoleOtherPrivileges, filterOtherPrivilege);

            this.lstVwOtherPrivileges.Dispatcher.Invoke(() =>
            {
                foreach (var otherPriv in listOtherPrivileges
                    .OrderBy(s => s.EntityLogicalName)
                    .ThenBy(s => s.Name, PrivilegeNameComparer.Comparer)
                )
                {
                    otherPriv.PropertyChanged -= rolePrivilege_PropertyChanged;
                    otherPriv.PropertyChanged -= rolePrivilege_PropertyChanged;
                    otherPriv.PropertyChanged += rolePrivilege_PropertyChanged;

                    _itemsSourceOtherPrivileges.Add(otherPriv);
                }

                if (this.lstVwOtherPrivileges.Items.Count == 1)
                {
                    this.lstVwOtherPrivileges.SelectedItem = this.lstVwOtherPrivileges.Items[0];
                }
            });

            ToggleControls(connectionData, true, Properties.OutputStrings.FilteringOtherPrivilegesCompletedFormat1, listOtherPrivileges.Count());
        }

        private async Task<IEnumerable<Privilege>> GetOtherPrivileges(IOrganizationServiceExtented service)
        {
            if (!_cachePrivileges.ContainsKey(service.ConnectionData.ConnectionId))
            {
                PrivilegeRepository repository = new PrivilegeRepository(service);

                var temp = await repository.GetListOtherPrivilegeAsync(new ColumnSet(
                    Privilege.Schema.Attributes.privilegeid
                    , Privilege.Schema.Attributes.name
                    , Privilege.Schema.Attributes.accessright

                    , Privilege.Schema.Attributes.canbebasic
                    , Privilege.Schema.Attributes.canbelocal
                    , Privilege.Schema.Attributes.canbedeep
                    , Privilege.Schema.Attributes.canbeglobal

                    , Privilege.Schema.Attributes.canbeentityreference
                    , Privilege.Schema.Attributes.canbeparententityreference
                ));

                _cachePrivileges.Add(service.ConnectionData.ConnectionId, temp);
            }

            return _cachePrivileges[service.ConnectionData.ConnectionId];
        }

        private async Task<IEnumerable<EntityMetadata>> GetEntityMetadataEnumerable(IOrganizationServiceExtented service)
        {
            if (!_cacheEntityMetadata.ContainsKey(service.ConnectionData.ConnectionId))
            {
                EntityMetadataRepository repository = new EntityMetadataRepository(service);

                var temp = await repository.GetEntitiesDisplayNameWithPrivilegesAsync();

                _cacheEntityMetadata.Add(service.ConnectionData.ConnectionId, temp);
            }

            return _cacheEntityMetadata[service.ConnectionData.ConnectionId];
        }

        private void rolePrivilege_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, nameof(SinglePrivilegeViewItem.IsChanged), StringComparison.InvariantCultureIgnoreCase))
            {
                UpdateSaveRoleChangesButtons();
            }
        }

        private IEnumerable<RoleEntityPrivilegeViewItem> FilterEntityList(IEnumerable<RoleEntityPrivilegeViewItem> list, string textName)
        {
            list = _entityMetadataFilter.FilterList(list, e => e.EntityMetadata);

            if (!string.IsNullOrEmpty(textName))
            {
                textName = textName.ToLower();

                if (int.TryParse(textName, out int tempInt))
                {
                    list = list.Where(ent => ent.ObjectTypeCode == tempInt);
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
                            ent.EntityMetadata != null
                            && ent.EntityMetadata.DisplayName != null
                            && ent.EntityMetadata.DisplayName.LocalizedLabels != null
                            && ent.EntityMetadata.DisplayName.LocalizedLabels
                                .Where(l => !string.IsNullOrEmpty(l.Label))
                                .Any(lbl => lbl.Label.IndexOf(textName, StringComparison.InvariantCultureIgnoreCase) > -1)
                        )
                    );
                }
            }

            return list;
        }

        private IEnumerable<RoleOtherPrivilegeViewItem> FilterPrivilegeList(IEnumerable<RoleOtherPrivilegeViewItem> list, string textName)
        {
            list = _privilegeFilter.FilterList(list, p => p.Privilege);

            if (!string.IsNullOrEmpty(textName))
            {
                textName = textName.ToLower();

                if (Guid.TryParse(textName, out Guid tempGuid))
                {
                    list = list.Where(ent => ent.Privilege.Id == tempGuid);
                }
                else
                {
                    list = list.Where(ent => ent.Name.IndexOf(textName, StringComparison.InvariantCultureIgnoreCase) != -1);
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

            ToggleControl(this.tSProgressBar

                , cmBCurrentConnection

                , btnSetCurrentConnection

                , btnRefreshEntites
                , btnRefreshRoles
                , btnRefreshSystemUsers
                , btnRefreshTeams
                , btnRefreshOtherPrivileges
            );

            UpdateTeamsButtons();

            UpdateSystemUsersButtons();

            UpdateSecurityRolesButtons();

            UpdateSaveRoleChangesButtons();
        }

        private async void txtBFilterSystemUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await ShowRoleSystemUsers();
            }
        }

        private void txtBOtherPrivilegesFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformFilterOtherPrivileges();
            }
        }

        private async void txtBFilterTeams_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await ShowRoleTeams();
            }
        }

        private async void txtBFilterRole_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await ShowExistingRoles();
            }
        }

        private Role GetSelectedRole()
        {
            Role result = null;

            lstVwSecurityRoles.Dispatcher.Invoke(() =>
            {
                result = this.lstVwSecurityRoles.SelectedItems.Cast<Role>().Count() == 1
                    ? this.lstVwSecurityRoles.SelectedItems.Cast<Role>().SingleOrDefault() : null;
            });

            return result;
        }

        private RoleEntityPrivilegeViewItem GetSelectedEntity()
        {
            RoleEntityPrivilegeViewItem result = null;

            lstVwEntityPrivileges.Dispatcher.Invoke(() =>
            {
                result = this.lstVwEntityPrivileges.SelectedItems.Cast<RoleEntityPrivilegeViewItem>().Count() == 1
                     ? this.lstVwEntityPrivileges.SelectedItems.Cast<RoleEntityPrivilegeViewItem>().SingleOrDefault() : null;
            });

            return result;
        }

        private List<RoleEntityPrivilegeViewItem> GetSelectedEntities()
        {
            List<RoleEntityPrivilegeViewItem> result = null;

            lstVwEntityPrivileges.Dispatcher.Invoke(() =>
            {
                result = this.lstVwEntityPrivileges.SelectedItems.Cast<RoleEntityPrivilegeViewItem>().ToList();
            });

            return result;
        }

        private List<Role> GetSelectedRoles()
        {
            List<Role> result = null;

            lstVwSecurityRoles.Dispatcher.Invoke(() =>
            {
                result = this.lstVwSecurityRoles.SelectedItems.Cast<Role>().ToList();
            });

            return result;
        }

        private List<SystemUser> GetSelectedUsers()
        {
            List<SystemUser> result = null;

            lstVwSystemUsers.Dispatcher.Invoke(() =>
            {
                result = this.lstVwSystemUsers.SelectedItems.Cast<SystemUser>().ToList();
            });

            return result;
        }

        private List<Team> GetSelectedTeams()
        {
            List<Team> result = null;

            lstVwTeams.Dispatcher.Invoke(() =>
            {
                result = this.lstVwTeams.SelectedItems.Cast<Team>().ToList();
            });

            return result;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void lstVwEntity_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Entity entity = GetItemFromRoutedDataContext<Entity>(e);

                if (entity != null)
                {
                    ConnectionData connectionData = GetSelectedConnection();

                    if (connectionData != null)
                    {
                        connectionData.OpenEntityInstanceInWeb(entity.LogicalName, entity.Id);
                    }
                }
            }
        }

        private void LstVwEntityPrivileges_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            IInputElement element = e.MouseDevice.DirectlyOver;
            if (element != null
                && element is FrameworkElement frameworkElement
                )
            {
                if (frameworkElement.Parent is DataGridCell cell)
                {
                    if (cell.Column == colEntityName
                        || cell.Column == colDisplayName
                        )
                    {
                        RoleEntityPrivilegeViewItem item = GetItemFromRoutedDataContext<RoleEntityPrivilegeViewItem>(e);

                        if (item != null)
                        {
                            ConnectionData connectionData = GetSelectedConnection();

                            if (connectionData != null)
                            {
                                connectionData.OpenEntityMetadataInWeb(item.EntityMetadata.MetadataId.Value);
                            }
                        }
                    }
                }
            }
        }

        private async Task ExecuteActionAsync(IEnumerable<string> entityNames, Func<IEnumerable<string>, Task> action)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            await action(entityNames);
        }

        private async void btnPublishEntity_Click(object sender, RoutedEventArgs e)
        {
            var entityList = GetSelectedEntities();

            if (entityList == null || !entityList.Any())
            {
                return;
            }

            await ExecuteActionAsync(entityList.Select(item => item.LogicalName).ToList(), PublishEntityAsync);
        }

        protected async Task PublishEntityAsync(IEnumerable<string> entityNames)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            await base.PublishEntityAsync(GetSelectedConnection(), entityNames);
        }

        private async void lstVwSecurityRoles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await RefreshRoleInfo();
        }

        private async Task RefreshRoleInfo()
        {
            ConnectionData connectionData = GetSelectedConnection();

            try
            {
                await ShowRoleSystemUsers();

                await ShowRoleTeams();

                await ShowRolePrivileges();
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(connectionData, ex);
            }
        }

        protected override async Task OnRefreshList(ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            await ShowExistingRoles();
        }

        #region Entity Operations

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
            await AddEntityToSolution(true, null, SolutionComponent.Schema.OptionSets.rootcomponentbehavior.Include_Subcomponents_0);
        }

        private async void AddToCrmSolutionDoNotIncludeSubcomponents_Click(object sender, RoutedEventArgs e)
        {
            await AddEntityToSolution(true, null, SolutionComponent.Schema.OptionSets.rootcomponentbehavior.Do_not_include_subcomponents_1);
        }

        private async void AddToCrmSolutionIncludeAsShellOnly_Click(object sender, RoutedEventArgs e)
        {
            await AddEntityToSolution(true, null, SolutionComponent.Schema.OptionSets.rootcomponentbehavior.Include_As_Shell_Only_2);
        }

        private async void AddToCrmSolutionLastIncludeSubcomponents_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
               && menuItem.Tag != null
               && menuItem.Tag is string solutionUniqueName
               )
            {
                await AddEntityToSolution(false, solutionUniqueName, SolutionComponent.Schema.OptionSets.rootcomponentbehavior.Include_Subcomponents_0);
            }
        }

        private async void AddToCrmSolutionLastDoNotIncludeSubcomponents_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
               && menuItem.Tag != null
               && menuItem.Tag is string solutionUniqueName
               )
            {
                await AddEntityToSolution(false, solutionUniqueName, SolutionComponent.Schema.OptionSets.rootcomponentbehavior.Do_not_include_subcomponents_1);
            }
        }

        private async void AddToCrmSolutionLastIncludeAsShellOnly_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
               && menuItem.Tag != null
               && menuItem.Tag is string solutionUniqueName
               )
            {
                await AddEntityToSolution(false, solutionUniqueName, SolutionComponent.Schema.OptionSets.rootcomponentbehavior.Include_As_Shell_Only_2);
            }
        }

        private async Task AddEntityToSolution(bool withSelect, string solutionUniqueName, SolutionComponent.Schema.OptionSets.rootcomponentbehavior rootComponentBehavior)
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

        private void ContextMenuEntityPrivileges_Opened(object sender, RoutedEventArgs e)
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

        #endregion Entity Operations

        private async void cmBCurrentConnection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                await ShowExistingRoles();
            }
        }

        private async void mIClearEntityCacheAndRefresh_Click(object sender, RoutedEventArgs e)
        {
            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                _cacheEntityMetadata.Remove(connectionData.ConnectionId);
                _cachePrivileges.Remove(connectionData.ConnectionId);

                await RefreshRoleInfo();
            }
        }

        #region Role Team User Common Operations

        private void mIOpenEntityInstanceInWeb_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
                )
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                connectionData.OpenEntityInstanceInWeb(entity.LogicalName, entity.Id);
            }
        }

        private void mICopyEntityInstanceIdToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
            )
            {
                return;
            }

            ClipboardHelper.SetText(entity.Id.ToString());
        }

        private void mICopyEntityInstanceUrlToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
            )
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                var url = connectionData.GetEntityInstanceUrl(entity.LogicalName, entity.Id);

                ClipboardHelper.SetText(url);
            }
        }

        private void mIOpenEntityInstanceListInWeb_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
                )
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                connectionData.OpenEntityInstanceListInWeb(entity.LogicalName);
            }
        }

        private async void mIOpenEntityExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
            )
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            IEnumerable<EntityMetadata> entityMetadataList = GetEntityMetadataList(service.ConnectionData.ConnectionId);
            IEnumerable<Privilege> privileges = GetOtherPrivilegesList(service.ConnectionData.ConnectionId);

            switch (entity)
            {
                case Role role:
                    WindowHelper.OpenRolesExplorer(_iWriteToOutput, service, _commonConfig, role.Name, entityMetadataList, privileges);
                    break;

                case SystemUser user:
                    WindowHelper.OpenSystemUsersExplorer(_iWriteToOutput, service, _commonConfig, user.FullName, entityMetadataList, privileges);
                    break;

                case Team team:
                    WindowHelper.OpenTeamsExplorer(_iWriteToOutput, service, _commonConfig, team.Name, entityMetadataList, privileges);
                    break;
            }
        }

        private async void mICreateEntityDescription_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
            )
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var entityFull = service.RetrieveByQuery<Entity>(entity.LogicalName, entity.Id, ColumnSetInstances.AllColumns);

            string fileName = EntityFileNameFormatter.GetEntityName(service.ConnectionData.Name, entityFull, EntityFileNameFormatter.Headers.EntityDescription, FileExtension.txt);
            string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

            await EntityDescriptionHandler.ExportEntityDescriptionAsync(filePath, entityFull, service.ConnectionData);

            _iWriteToOutput.WriteToOutput(
                service.ConnectionData
                , Properties.OutputStrings.InConnectionExportedEntityDescriptionFormat3
                , service.ConnectionData.Name
                , entityFull.LogicalName
                , filePath
            );

            _iWriteToOutput.PerformAction(service.ConnectionData, filePath);
        }

        #endregion Role Team User Common Operations

        #region Role Operations

        private async void mICreateRoleBackup_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
            )
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var role = entity.ToEntity<Role>();

            await PerformRoleBackup(service, role, true);
        }

        private void ContextMenuRole_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu contextMenu)
            {
                var items = contextMenu.Items.OfType<Control>();

                ConnectionData connectionData = GetSelectedConnection();

                FillLastSolutionItems(connectionData, items, true, AddRoleToCrmSolutionLast_Click, "contMnAddToSolutionLast");
            }
        }

        private async void AddRoleToCrmSolution_Click(object sender, RoutedEventArgs e)
        {
            await AddRoleToSolution(true, null);
        }

        private async void AddRoleToCrmSolutionLast_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
               && menuItem.Tag != null
               && menuItem.Tag is string solutionUniqueName
               )
            {
                await AddRoleToSolution(false, solutionUniqueName);
            }
        }

        private async Task AddRoleToSolution(bool withSelect, string solutionUniqueName)
        {
            var roleList = GetSelectedRoles()
                .Select(item => item.Id)
                .ToList();

            if (!roleList.Any())
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

                await SolutionController.AddSolutionComponentsGroupToSolution(_iWriteToOutput, service, descriptor, _commonConfig, solutionUniqueName, ComponentType.Role, roleList, null, withSelect);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);
            }
        }

        private async void mIRoleOpenSolutionsContainingComponentInExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
            )
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var role = entity.ToEntity<Role>();

            _commonConfig.Save();

            WindowHelper.OpenExplorerSolutionExplorer(
                _iWriteToOutput
                , service
                , _commonConfig
                , (int)ComponentType.Role
                , role.RoleId.Value
                , null
            );
        }

        private void mIRoleOpenDependentComponentsInWeb_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
                )
            {
                return;
            }

            var role = entity.ToEntity<Role>();

            _commonConfig.Save();

            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                connectionData.OpenSolutionComponentDependentComponentsInWeb(ComponentType.Role, role.RoleId.Value);
            }
        }

        private async void mIRoleOpenDependentComponentsInExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
                )
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var role = entity.ToEntity<Role>();

            _commonConfig.Save();

            WindowHelper.OpenSolutionComponentDependenciesExplorer(_iWriteToOutput, service, null, _commonConfig, (int)ComponentType.Role, role.RoleId.Value, null);
        }

        private async void btnAssignRoleToUser_Click(object sender, RoutedEventArgs e)
        {
            var roleList = GetSelectedRoles();

            if (roleList == null || !roleList.Any())
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var repository = new SystemUserRepository(service);

            Func<string, Task<IEnumerable<SystemUser>>> getter = null;

            var columnSet = new ColumnSet(
                SystemUser.Schema.Attributes.domainname
                , SystemUser.Schema.Attributes.fullname
                , SystemUser.Schema.Attributes.businessunitid
                , SystemUser.Schema.Attributes.isdisabled
            );

            if (roleList.Count == 1)
            {
                var role = roleList.First();

                getter = (string filter) => repository.GetAvailableUsersForRoleAsync(filter, role.Id, columnSet);
            }
            else
            {
                getter = (string filter) => repository.GetUsersAsync(filter, columnSet);
            }

            IEnumerable<DataGridColumn> columns = SystemUserRepository.GetDataGridColumn();

            var form = new WindowEntitySelect<SystemUser>(_iWriteToOutput, service.ConnectionData, SystemUser.EntityLogicalName, getter, columns);

            if (!form.ShowDialog().GetValueOrDefault())
            {
                return;
            }

            if (form.SelectedEntity == null)
            {
                return;
            }

            var user = form.SelectedEntity;

            string rolesName = string.Join(", ", roleList.Select(r => r.Name).OrderBy(s => s));
            string usersName = string.Format("{0} - {1}", user.DomainName, user.FullName);

            string operationName = string.Format(Properties.OperationNames.AssigningRolesToUsersFormat3, service.ConnectionData.Name, rolesName, usersName);

            _iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operationName);

            ToggleControls(service.ConnectionData, false, Properties.OutputStrings.InConnectionAssigningRolesToUsersFormat3, service.ConnectionData.Name, rolesName, usersName);

            try
            {
                var repositoryRolePrivileges = new RolePrivilegesRepository(service);

                await repositoryRolePrivileges.AssignRolesToUserAsync(user.Id, roleList.Select(r => r.Id));
            }
            catch (Exception ex)
            {
                _iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);
            }

            _iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operationName);

            ToggleControls(service.ConnectionData, true, Properties.OutputStrings.InConnectionAssigningRolesToUsersCompletedFormat3, service.ConnectionData.Name, rolesName, usersName);

            await RefreshRoleInfo();
        }

        private async void btnRemoveRoleFromUser_Click(object sender, RoutedEventArgs e)
        {
            var role = GetSelectedRole();

            var userList = GetSelectedUsers();

            if (role == null || userList == null || !userList.Any())
            {
                return;
            }

            string rolesName = role.Name;
            string usersName = string.Join(", ", userList.Select(u => string.Format("{0} - {1}", u.DomainName, u.FullName)).OrderBy(s => s));

            string message = string.Format(Properties.MessageBoxStrings.AreYouSureRemoveRolesFromUsersFormat2, rolesName, usersName);

            if (MessageBox.Show(message, Properties.MessageBoxStrings.QuestionTitle, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            string operationName = string.Format(Properties.OperationNames.RemovingRolesFromUsersFormat3, service.ConnectionData.Name, rolesName, usersName);

            _iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operationName);

            ToggleControls(service.ConnectionData, false, Properties.OutputStrings.InConnectionRemovingRolesFromUsersFormat3, service.ConnectionData.Name, rolesName, usersName);

            try
            {
                var repository = new RolePrivilegesRepository(service);

                var roleArray = new[] { role.Id };

                foreach (var user in userList)
                {
                    await repository.RemoveRolesFromUserAsync(user.Id, roleArray);
                }
            }
            catch (Exception ex)
            {
                _iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);
            }

            ToggleControls(service.ConnectionData, true, Properties.OutputStrings.InConnectionRemovingRolesFromUsersCompletedFormat3, service.ConnectionData.Name, rolesName, usersName);

            _iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operationName);

            await RefreshRoleInfo();
        }

        private async void btnAssignRoleToTeam_Click(object sender, RoutedEventArgs e)
        {
            var roleList = GetSelectedRoles();

            if (roleList == null || !roleList.Any())
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var repository = new TeamRepository(service);

            Func<string, Task<IEnumerable<Team>>> getter = null;

            var columnSet = new ColumnSet(
                Team.Schema.Attributes.name
                , Team.Schema.Attributes.businessunitid
                , Team.Schema.Attributes.isdefault
            );

            if (roleList.Count == 1)
            {
                var role = roleList.First();

                getter = (string filter) => repository.GetAvailableTeamsForRoleAsync(filter, role.Id, columnSet);
            }
            else
            {
                getter = (string filter) => repository.GetOwnerTeamsAsync(filter, columnSet);
            }

            IEnumerable<DataGridColumn> columns = TeamRepository.GetDataGridColumnOwner();

            var form = new WindowEntitySelect<Team>(_iWriteToOutput, service.ConnectionData, Team.EntityLogicalName, getter, columns);

            if (!form.ShowDialog().GetValueOrDefault())
            {
                return;
            }

            if (form.SelectedEntity == null)
            {
                return;
            }

            var team = form.SelectedEntity;

            string rolesName = string.Join(", ", roleList.Select(r => r.Name).OrderBy(s => s));
            string teamsName = team.Name;

            string operationName = string.Format(Properties.OperationNames.AssigningRolesToTeamsFormat3, service.ConnectionData.Name, rolesName, teamsName);

            _iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operationName);

            ToggleControls(service.ConnectionData, false, Properties.OutputStrings.InConnectionAssigningRolesToTeamsFormat3, service.ConnectionData.Name, rolesName, teamsName);

            try
            {
                var repositoryRolePrivileges = new RolePrivilegesRepository(service);

                await repositoryRolePrivileges.AssignRolesToTeamAsync(form.SelectedEntity.Id, roleList.Select(r => r.Id));
            }
            catch (Exception ex)
            {
                _iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);
            }

            ToggleControls(service.ConnectionData, true, Properties.OutputStrings.InConnectionAssigningRolesToTeamsCompletedFormat3, service.ConnectionData.Name, rolesName, teamsName);

            _iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operationName);

            await RefreshRoleInfo();
        }

        private async void btnRemoveRoleFromTeam_Click(object sender, RoutedEventArgs e)
        {
            var role = GetSelectedRole();

            var teamList = GetSelectedTeams();

            if (role == null || teamList == null || !teamList.Any())
            {
                return;
            }

            string rolesName = role.Name;
            string teamsName = string.Join(", ", teamList.Select(u => u.Name).OrderBy(s => s));

            string message = string.Format(Properties.MessageBoxStrings.AreYouSureRemoveRolesFromTeamsFormat2, rolesName, teamsName);

            if (MessageBox.Show(message, Properties.MessageBoxStrings.QuestionTitle, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            string operationName = string.Format(Properties.OperationNames.RemovingRolesFromTeamsFormat3, service.ConnectionData.Name, rolesName, teamsName);

            _iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operationName);

            ToggleControls(service.ConnectionData, false, Properties.OutputStrings.InConnectionRemovingRolesFromTeamsFormat3, service.ConnectionData.Name, rolesName, teamsName);

            try
            {
                var repository = new RolePrivilegesRepository(service);

                var roleArray = new[] { role.Id };

                foreach (var team in teamList)
                {
                    await repository.RemoveRolesFromTeamAsync(team.Id, roleArray);
                }
            }
            catch (Exception ex)
            {
                _iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);
            }

            ToggleControls(service.ConnectionData, true, Properties.OutputStrings.InConnectionRemovingRolesFromTeamsCompletedFormat3, service.ConnectionData.Name, rolesName, teamsName);

            _iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operationName);

            await RefreshRoleInfo();
        }

        #endregion Role Operations

        private void LstVwTeams_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTeamsButtons();
        }

        private void UpdateTeamsButtons()
        {
            this.lstVwTeams.Dispatcher.Invoke(() =>
            {
                try
                {
                    bool enabled = this.IsControlsEnabled
                        && this.lstVwTeams != null
                        && this.lstVwTeams.SelectedItems.Cast<Team>().Any();

                    UIElement[] list = { btnRemoveRoleFromTeam };

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

        private void lstVwSystemUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSystemUsersButtons();
        }

        private void UpdateSystemUsersButtons()
        {
            this.lstVwSystemUsers.Dispatcher.Invoke(() =>
            {
                try
                {
                    bool enabled = this.IsControlsEnabled && this.lstVwSystemUsers != null && this.lstVwSystemUsers.SelectedItems.Count > 0;

                    UIElement[] list = { btnRemoveRoleFromUser };

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

        private void UpdateSecurityRolesButtons()
        {
            this.lstVwSecurityRoles.Dispatcher.Invoke(() =>
            {
                try
                {
                    bool enabled = this.IsControlsEnabled && this.lstVwSecurityRoles != null && this.lstVwSecurityRoles.SelectedItems.Count > 0;

                    UIElement[] list = { btnAssignRoleToTeam, btnAssignRoleToUser };

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

        private async void btnRefreshSystemUsers_Click(object sender, RoutedEventArgs e)
        {
            await ShowRoleSystemUsers();
        }

        private async void btnRefreshRoles_Click(object sender, RoutedEventArgs e)
        {
            await ShowExistingRoles();
        }

        private async void btnRefreshTeams_Click(object sender, RoutedEventArgs e)
        {
            await ShowRoleTeams();
        }

        private async void btnRefreshEntites_Click(object sender, RoutedEventArgs e)
        {
            await ShowRolePrivileges();
        }

        private async void mISaveRoleChanges_Click(object sender, RoutedEventArgs e)
        {
            var role = GetSelectedRole();

            if (role == null)
            {
                return;
            }

            var changedEntitesList = _itemsSourceEntityPrivileges?.Where(en => en.IsChanged).ToList();
            var changedOtherList = _itemsSourceOtherPrivileges?.Where(en => en.IsChanged).ToList();

            if (!(changedEntitesList != null && changedEntitesList.Any())
                && !(changedOtherList != null && changedOtherList.Any())
            )
            {
                return;
            }

            string message = string.Format(Properties.MessageBoxStrings.SaveChangesToRolesFormat1, role.Name);

            if (MessageBox.Show(message, Properties.MessageBoxStrings.QuestionTitle, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            Dictionary<Guid, PrivilegeDepth> dictPrivilegesAdd = new Dictionary<Guid, PrivilegeDepth>();
            HashSet<Guid> hashPrivilegesRemove = new HashSet<Guid>();

            if (changedEntitesList != null)
            {
                foreach (var ent in changedEntitesList)
                {
                    ent.FillChangedPrivileges(dictPrivilegesAdd, hashPrivilegesRemove);
                }
            }

            if (changedOtherList != null)
            {
                foreach (var ent in changedOtherList)
                {
                    ent.FillChangedPrivileges(dictPrivilegesAdd, hashPrivilegesRemove);
                }
            }

            if (!dictPrivilegesAdd.Any()
                && !hashPrivilegesRemove.Any()
            )
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var privilegesAdd = dictPrivilegesAdd.Select(d => new RolePrivilege()
            {
                PrivilegeId = d.Key,
                Depth = d.Value,
            }).ToList();

            var privilegesRemove = hashPrivilegesRemove.Select(p => new RolePrivilege()
            {
                PrivilegeId = p,
            }).ToList();

            string operationName = string.Format(Properties.OperationNames.SavingChangesInRolesFormat1, service.ConnectionData.Name);

            _iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operationName);

            ToggleControls(service.ConnectionData, false, Properties.OutputStrings.InConnectionSavingChangesInRolesFormat1, service.ConnectionData.Name);

            var rolePrivileges = new RolePrivilegesRepository(service);

            _iWriteToOutput.WriteToOutput(service.ConnectionData, Properties.OutputStrings.SavingChangesInRoleFormat1, role.Name);
            _iWriteToOutput.WriteToOutputEntityInstance(service.ConnectionData, role);

            try
            {
                await rolePrivileges.ModifyRolePrivilegesAsync(role.Id, privilegesAdd, privilegesRemove);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);
            }

            ToggleControls(service.ConnectionData, true, Properties.OutputStrings.InConnectionSavingChangesInRolesCompletedFormat1, service.ConnectionData.Name);

            _iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operationName);

            await ShowRolePrivileges();
        }

        private void UpdateSaveRoleChangesButtons()
        {
            this.Dispatcher.Invoke(() =>
            {
                try
                {
                    bool enabled = this.IsControlsEnabled &&
                        (_itemsSourceEntityPrivileges.Any(e => e.IsChanged) || _itemsSourceOtherPrivileges.Any(e => e.IsChanged));

                    UIElement[] list = { btnSaveRoleChanges };

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

        private void btnSetCurrentConnection_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentConnection(_iWriteToOutput, GetSelectedConnection());
        }

        private async Task PerformRoleBackup(IOrganizationServiceExtented service, Role role, bool openFile)
        {
            string operationName = string.Format(Properties.OperationNames.CreatingRoleBackupFormat2, service.ConnectionData.Name, role.Name);

            _iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operationName);

            ToggleControls(service.ConnectionData, false, Properties.OutputStrings.InConnectionCreatingRoleBackupFormat2, service.ConnectionData.Name, role.Name);

            var repositoryRolePrivileges = new RolePrivilegesRepository(service);
            var repositoryPrivileges = new PrivilegeRepository(service);

            var rolePrivileges = await repositoryRolePrivileges.GetRolePrivilegesAsync(role.Id);
            var privileges = await repositoryPrivileges.GetListForRoleAsync(role.Id);

            Model.Backup.Role roleBackup = CreateRoleBackupByPrivileges(role, rolePrivileges, privileges);

            string fileName = EntityFileNameFormatter.GetRoleFileName(service.ConnectionData.Name, role.Name, EntityFileNameFormatter.Headers.Backup, FileExtension.xml);
            string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

            await roleBackup.SaveAsync(filePath);

            _iWriteToOutput.WriteToOutput(service.ConnectionData
                , Properties.OutputStrings.InConnectionExportedRoleBackupFormat3
                , service.ConnectionData.Name
                , role.Name
                , filePath
            );

            if (openFile)
            {
                _iWriteToOutput.PerformAction(service.ConnectionData, filePath);
            }
            else
            {
                _iWriteToOutput.WriteToOutputFilePathUri(service.ConnectionData, filePath);
            }

            ToggleControls(service.ConnectionData, true, Properties.OutputStrings.InConnectionCreatingRoleBackupCompletedFormat2, service.ConnectionData.Name, role.Name);

            _iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operationName);
        }

        private static Model.Backup.Role CreateRoleBackupByPrivileges(Role role, IEnumerable<RolePrivilege> rolePrivileges, List<Privilege> privileges)
        {
            var temp = new List<Model.Backup.RolePrivilege>();

            foreach (var rolePriv in rolePrivileges)
            {
                var priv = privileges.FirstOrDefault(p => p.Id == rolePriv.PrivilegeId);

                if (priv != null)
                {
                    temp.Add(new Model.Backup.RolePrivilege()
                    {
                        Name = priv.Name,
                        Level = rolePriv.Depth,
                    });
                }
            }

            Model.Backup.Role roleBackup = new Model.Backup.Role()
            {
                Id = role.Id,
                TemplateId = role.RoleTemplateId?.Id,
                Name = role.Name,
            };

            roleBackup.RolePrivileges.AddRange(temp.OrderBy(p => p.Name));
            return roleBackup;
        }

        #region Changing Role Privileges

        private async void mIAddUniquePrivilegesToRole_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
            )
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var sourceRole = entity.ToEntity<Role>();

            var repository = new RoleRepository(service);

            Func<string, Task<IEnumerable<Role>>> getter = (string filter) => repository.GetRolesForNotAnotherAsync(filter
                , sourceRole.Id
                , new ColumnSet(
                    Role.Schema.Attributes.name
                    , Role.Schema.Attributes.businessunitid
                    , Role.Schema.Attributes.ismanaged
                    , Role.Schema.Attributes.iscustomizable
                )
            );

            IEnumerable<DataGridColumn> columns = Helpers.SolutionComponentDescription.Implementation.RoleDescriptionBuilder.GetDataGridColumn();

            var form = new WindowEntitySelect<Role>(_iWriteToOutput, service.ConnectionData, Role.EntityLogicalName, getter, columns);

            if (!form.ShowDialog().GetValueOrDefault())
            {
                return;
            }

            if (form.SelectedEntity == null)
            {
                return;
            }

            var targetRole = form.SelectedEntity;

            await AddUniquePrivilegesFromSourceToTarget(service, sourceRole, targetRole);
        }

        private async void mIMergeCommonPrivilegesToMaxInRole_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
            )
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var sourceRole = entity.ToEntity<Role>();

            var repository = new RoleRepository(service);

            Func<string, Task<IEnumerable<Role>>> getter = (string filter) => repository.GetRolesForNotAnotherAsync(filter
                , sourceRole.Id
                , new ColumnSet(
                    Role.Schema.Attributes.name
                    , Role.Schema.Attributes.businessunitid
                    , Role.Schema.Attributes.ismanaged
                    , Role.Schema.Attributes.iscustomizable
                )
            );

            IEnumerable<DataGridColumn> columns = Helpers.SolutionComponentDescription.Implementation.RoleDescriptionBuilder.GetDataGridColumn();

            var form = new WindowEntitySelect<Role>(_iWriteToOutput, service.ConnectionData, Role.EntityLogicalName, getter, columns);

            if (!form.ShowDialog().GetValueOrDefault())
            {
                return;
            }

            if (form.SelectedEntity == null)
            {
                return;
            }

            var targetRole = form.SelectedEntity;

            await MergePrivilegesFromSourceToTarget(service, sourceRole, targetRole);
        }

        private async void mIReplacePrivilegesInRole_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
            )
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var sourceRole = entity.ToEntity<Role>();

            var repository = new RoleRepository(service);

            Func<string, Task<IEnumerable<Role>>> getter = (string filter) => repository.GetRolesForNotAnotherAsync(filter
                , sourceRole.Id
                , new ColumnSet(
                    Role.Schema.Attributes.name
                    , Role.Schema.Attributes.businessunitid
                    , Role.Schema.Attributes.ismanaged
                    , Role.Schema.Attributes.iscustomizable
                )
            );

            IEnumerable<DataGridColumn> columns = Helpers.SolutionComponentDescription.Implementation.RoleDescriptionBuilder.GetDataGridColumn();

            var form = new WindowEntitySelect<Role>(_iWriteToOutput, service.ConnectionData, Role.EntityLogicalName, getter, columns);

            if (!form.ShowDialog().GetValueOrDefault())
            {
                return;
            }

            if (form.SelectedEntity == null)
            {
                return;
            }

            var targetRole = form.SelectedEntity;

            await ReplacePrivilegesFromSourceToTarget(service, sourceRole, targetRole);
        }

        private async void mIClearRole_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
            )
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var role = entity.ToEntity<Role>();

            string question = string.Format(Properties.MessageBoxStrings.ClearRoleFormat1, role.Name);

            if (MessageBox.Show(question, Properties.MessageBoxStrings.QuestionTitle, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            var repositoryRolePrivileges = new RolePrivilegesRepository(service);
            IEnumerable<RolePrivilege> rolePrivileges = null;

            {
                string operationName = string.Format(Properties.OperationNames.CreatingRoleBackupFormat2, service.ConnectionData.Name, role.Name);

                _iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operationName);

                ToggleControls(service.ConnectionData, false, Properties.OutputStrings.InConnectionCreatingRoleBackupFormat2, service.ConnectionData.Name, role.Name);

                var repositoryPrivileges = new PrivilegeRepository(service);

                rolePrivileges = await repositoryRolePrivileges.GetRolePrivilegesAsync(role.Id);

                var privileges = await repositoryPrivileges.GetListForRoleAsync(role.Id);

                Model.Backup.Role roleBackup = CreateRoleBackupByPrivileges(role, rolePrivileges, privileges);

                string fileName = EntityFileNameFormatter.GetRoleFileName(service.ConnectionData.Name, role.Name, EntityFileNameFormatter.Headers.Backup, FileExtension.xml);
                string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

                await roleBackup.SaveAsync(filePath);

                _iWriteToOutput.WriteToOutput(service.ConnectionData
                    , Properties.OutputStrings.InConnectionExportedRoleBackupFormat3
                    , service.ConnectionData.Name
                    , role.Name
                    , filePath
                );

                _iWriteToOutput.PerformAction(service.ConnectionData, filePath);

                ToggleControls(service.ConnectionData, true, Properties.OutputStrings.InConnectionCreatingRoleBackupCompletedFormat2, service.ConnectionData.Name, role.Name);

                _iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operationName);
            }

            {
                string operationName = string.Format(Properties.OperationNames.ClearingRolePrivilegesFormat2, service.ConnectionData.Name, role.Name);

                _iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operationName);

                ToggleControls(service.ConnectionData, false, Properties.OutputStrings.InConnectionClearingRolePrivilegesFormat2, service.ConnectionData.Name, role.Name);

                await repositoryRolePrivileges.ModifyRolePrivilegesAsync(role.Id, null, rolePrivileges);

                ToggleControls(service.ConnectionData, true, Properties.OutputStrings.InConnectionClearingRolePrivilegesCompletedFormat2, service.ConnectionData.Name, role.Name);

                _iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operationName);
            }

            await ShowRolePrivileges();
        }

        private async void mIAddUniquePrivilegesFromRole_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
            )
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var targetRole = entity.ToEntity<Role>();

            var repository = new RoleRepository(service);

            Func<string, Task<IEnumerable<Role>>> getter = (string filter) => repository.GetRolesForNotAnotherAsync(filter
                , targetRole.Id
                , new ColumnSet(
                    Role.Schema.Attributes.name
                    , Role.Schema.Attributes.businessunitid
                    , Role.Schema.Attributes.ismanaged
                    , Role.Schema.Attributes.iscustomizable
                )
            );

            IEnumerable<DataGridColumn> columns = Helpers.SolutionComponentDescription.Implementation.RoleDescriptionBuilder.GetDataGridColumn();

            var form = new WindowEntitySelect<Role>(_iWriteToOutput, service.ConnectionData, Role.EntityLogicalName, getter, columns);

            if (!form.ShowDialog().GetValueOrDefault())
            {
                return;
            }

            if (form.SelectedEntity == null)
            {
                return;
            }

            var sourceRole = form.SelectedEntity;

            await AddUniquePrivilegesFromSourceToTarget(service, sourceRole, targetRole);

            await ShowRolePrivileges();
        }

        private async void mIMergeCommonPrivilegesToMaxFromRole_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
            )
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var targetRole = entity.ToEntity<Role>();

            var repository = new RoleRepository(service);

            Func<string, Task<IEnumerable<Role>>> getter = (string filter) => repository.GetRolesForNotAnotherAsync(filter
                , targetRole.Id
                , new ColumnSet(
                    Role.Schema.Attributes.name
                    , Role.Schema.Attributes.businessunitid
                    , Role.Schema.Attributes.ismanaged
                    , Role.Schema.Attributes.iscustomizable
                )
            );

            IEnumerable<DataGridColumn> columns = Helpers.SolutionComponentDescription.Implementation.RoleDescriptionBuilder.GetDataGridColumn();

            var form = new WindowEntitySelect<Role>(_iWriteToOutput, service.ConnectionData, Role.EntityLogicalName, getter, columns);

            if (!form.ShowDialog().GetValueOrDefault())
            {
                return;
            }

            if (form.SelectedEntity == null)
            {
                return;
            }

            var sourceRole = form.SelectedEntity;

            await MergePrivilegesFromSourceToTarget(service, sourceRole, targetRole);

            await ShowRolePrivileges();
        }

        private async void mIReplacePrivilegesFromRole_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
            )
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var targetRole = entity.ToEntity<Role>();

            var repository = new RoleRepository(service);

            Func<string, Task<IEnumerable<Role>>> getter = (string filter) => repository.GetRolesForNotAnotherAsync(filter
                , targetRole.Id
                , new ColumnSet(
                    Role.Schema.Attributes.name
                    , Role.Schema.Attributes.businessunitid
                    , Role.Schema.Attributes.ismanaged
                    , Role.Schema.Attributes.iscustomizable
                )
            );

            IEnumerable<DataGridColumn> columns = Helpers.SolutionComponentDescription.Implementation.RoleDescriptionBuilder.GetDataGridColumn();

            var form = new WindowEntitySelect<Role>(_iWriteToOutput, service.ConnectionData, Role.EntityLogicalName, getter, columns);

            if (!form.ShowDialog().GetValueOrDefault())
            {
                return;
            }

            if (form.SelectedEntity == null)
            {
                return;
            }

            var sourceRole = form.SelectedEntity;

            await ReplacePrivilegesFromSourceToTarget(service, sourceRole, targetRole);

            await ShowRolePrivileges();
        }

        private async Task AddUniquePrivilegesFromSourceToTarget(IOrganizationServiceExtented service, Role sourceRole, Role targetRole)
        {
            var repositoryRolePrivileges = new RolePrivilegesRepository(service);
            var repositoryPrivileges = new PrivilegeRepository(service);

            ToggleControls(service.ConnectionData, false, Properties.OutputStrings.AnalizingRolesFormat2, sourceRole.Name, targetRole.Name);

            IEnumerable<RolePrivilege> newRolePrivileges = Enumerable.Empty<RolePrivilege>();

            var targetRolePrivileges = await repositoryRolePrivileges.GetRolePrivilegesAsync(targetRole.Id);

            {
                var sourceRolePrivileges = await repositoryRolePrivileges.GetRolePrivilegesAsync(sourceRole.Id);

                newRolePrivileges = sourceRolePrivileges.Except(targetRolePrivileges, new RolePrivilegeComparer());
            }

            if (!newRolePrivileges.Any())
            {
                ToggleControls(service.ConnectionData, true, Properties.OutputStrings.NoPrivilegesToAdd);
                return;
            }

            await PerformRoleBackup(service, targetRole, false);

            {
                string operationName = string.Format(Properties.OperationNames.AddingNewPrivilegesFromRoleToRoleFormat3, service.ConnectionData.Name, targetRole.Name);

                _iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operationName);

                ToggleControls(service.ConnectionData, false, Properties.OutputStrings.InConnectionAddingNewPrivilegesFromRoleToRoleFormat3, service.ConnectionData.Name, targetRole.Name);

                var sourcePrivileges = await repositoryPrivileges.GetListForRoleAsync(sourceRole.Id);

                await repositoryRolePrivileges.ModifyRolePrivilegesAsync(targetRole.Id, newRolePrivileges, null);

                Model.Backup.Role roleBackup = CreateRoleBackupByPrivileges(targetRole, newRolePrivileges, sourcePrivileges);

                string fileName = EntityFileNameFormatter.GetRoleFileName(service.ConnectionData.Name, targetRole.Name, Role.Schema.Headers.AddedNewPrivileges, FileExtension.xml);
                string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

                await roleBackup.SaveAsync(filePath);

                _iWriteToOutput.WriteToOutput(service.ConnectionData
                    , Properties.OutputStrings.InConnectionAddedNewPrivilegesFormat3
                    , service.ConnectionData.Name
                    , targetRole.Name
                    , filePath
                );

                _iWriteToOutput.PerformAction(service.ConnectionData, filePath);

                ToggleControls(service.ConnectionData, true, Properties.OutputStrings.InConnectionAddingNewPrivilegesFromRoleToRoleCompletedFormat3, service.ConnectionData.Name, sourceRole.Name);

                _iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operationName);
            }
        }

        private async Task MergePrivilegesFromSourceToTarget(IOrganizationServiceExtented service, Role sourceRole, Role targetRole)
        {
            var repositoryRolePrivileges = new RolePrivilegesRepository(service);
            var repositoryPrivileges = new PrivilegeRepository(service);

            ToggleControls(service.ConnectionData, false, Properties.OutputStrings.AnalizingRolesFormat2, sourceRole.Name, targetRole.Name);

            IEnumerable<RolePrivilege> newRolePrivileges = Enumerable.Empty<RolePrivilege>();

            var targetRolePrivileges = await repositoryRolePrivileges.GetRolePrivilegesAsync(targetRole.Id);

            {
                var sourceRolePrivileges = await repositoryRolePrivileges.GetRolePrivilegesAsync(sourceRole.Id);

                newRolePrivileges = GetPrivilegesToMaximize(sourceRolePrivileges, targetRolePrivileges);
            }

            if (!newRolePrivileges.Any())
            {
                ToggleControls(service.ConnectionData, true, Properties.OutputStrings.NoPrivilegesToChange);
                return;
            }

            await PerformRoleBackup(service, targetRole, false);

            {
                string operationName = string.Format(Properties.OperationNames.MergingPrivilegesToMaximumFromRoleToRoleFormat3, service.ConnectionData.Name, targetRole.Name);

                _iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operationName);

                ToggleControls(service.ConnectionData, false, Properties.OutputStrings.InConnectionMergingPrivilegesToMaximumFromRoleToRoleFormat3, service.ConnectionData.Name, targetRole.Name);

                var sourcePrivileges = await repositoryPrivileges.GetListForRoleAsync(sourceRole.Id);

                await repositoryRolePrivileges.ModifyRolePrivilegesAsync(targetRole.Id, newRolePrivileges, null);

                Model.Backup.Role roleBackup = CreateRoleBackupByPrivileges(targetRole, newRolePrivileges, sourcePrivileges);

                string fileName = EntityFileNameFormatter.GetRoleFileName(service.ConnectionData.Name, targetRole.Name, Role.Schema.Headers.ChangedPrivileges, FileExtension.xml);
                string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

                await roleBackup.SaveAsync(filePath);

                _iWriteToOutput.WriteToOutput(service.ConnectionData
                    , Properties.OutputStrings.InConnectionChangedPrivilegesFormat3
                    , service.ConnectionData.Name
                    , targetRole.Name
                    , filePath
                );

                _iWriteToOutput.PerformAction(service.ConnectionData, filePath);

                ToggleControls(service.ConnectionData, true, Properties.OutputStrings.InConnectionMergingPrivilegesToMaximumFromRoleToRoleCompletedFormat3, service.ConnectionData.Name, sourceRole.Name);

                _iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operationName);
            }
        }

        private IEnumerable<RolePrivilege> GetPrivilegesToMaximize(IEnumerable<RolePrivilege> sourceRolePrivileges, IEnumerable<RolePrivilege> targetRolePrivileges)
        {
            List<RolePrivilege> result = new List<RolePrivilege>();

            foreach (var rolePrivilege in sourceRolePrivileges)
            {
                var targetRolePriv = targetRolePrivileges.FirstOrDefault(r => r.PrivilegeId == rolePrivilege.PrivilegeId);

                if (targetRolePriv != null && (int)targetRolePriv.Depth > (int)rolePrivilege.Depth)
                {
                    result.Add(rolePrivilege);
                }
            }

            return result;
        }

        private async Task ReplacePrivilegesFromSourceToTarget(IOrganizationServiceExtented service, Role sourceRole, Role targetRole)
        {
            await PerformRoleBackup(service, targetRole, false);

            {
                string operationName = string.Format(Properties.OperationNames.ReplacingPrivilegesFromRoleToRoleFormat3, service.ConnectionData.Name, targetRole.Name);

                _iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operationName);

                ToggleControls(service.ConnectionData, false, Properties.OutputStrings.InConnectionReplacingPrivilegesFromRoleToRoleFormat3, service.ConnectionData.Name, targetRole.Name);

                var repositoryRolePrivileges = new RolePrivilegesRepository(service);
                var repositoryPrivileges = new PrivilegeRepository(service);

                var rolePrivileges = await repositoryRolePrivileges.GetRolePrivilegesAsync(sourceRole.Id);
                var privileges = await repositoryPrivileges.GetListForRoleAsync(sourceRole.Id);

                Model.Backup.Role roleBackup = CreateRoleBackupByPrivileges(targetRole, rolePrivileges, privileges);

                await repositoryRolePrivileges.ReplaceRolePrivilegesAsync(targetRole.Id, rolePrivileges);

                string fileName = EntityFileNameFormatter.GetRoleFileName(service.ConnectionData.Name, targetRole.Name, Role.Schema.Headers.AllNewPrivileges, FileExtension.xml);
                string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

                await roleBackup.SaveAsync(filePath);

                _iWriteToOutput.WriteToOutput(service.ConnectionData
                    , Properties.OutputStrings.InConnectionAllNewPrivilegesFormat3
                    , service.ConnectionData.Name
                    , targetRole.Name
                    , filePath
                );

                _iWriteToOutput.PerformAction(service.ConnectionData, filePath);

                ToggleControls(service.ConnectionData, true, Properties.OutputStrings.InConnectionReplacingPrivilegesFromRoleToRoleCompletedFormat3, service.ConnectionData.Name, sourceRole.Name);

                _iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operationName);
            }
        }

        #endregion Changing Role Privileges

        #region Set Attribute

        private void mISetAttributeEntityPrivilegeAll_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
                && menuItem.Tag != null
                && menuItem.Tag is PrivilegeDepthExtended privilegeDepth
            )
            {
                SetSelectedRoleEntityPrivileges(privilegeDepth);
            }
        }

        private void mISetAttributeEntityPrivilegeCreate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
                && menuItem.Tag != null
                && menuItem.Tag is PrivilegeDepthExtended privilegeDepth
            )
            {
                SetSelectedRoleEntityPrivileges(PrivilegeType.Create, privilegeDepth);
            }
        }

        private void mISetAttributeEntityPrivilegeRead_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
                && menuItem.Tag != null
                && menuItem.Tag is PrivilegeDepthExtended privilegeDepth
            )
            {
                SetSelectedRoleEntityPrivileges(PrivilegeType.Read, privilegeDepth);
            }
        }

        private void mISetAttributeEntityPrivilegeUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
                && menuItem.Tag != null
                && menuItem.Tag is PrivilegeDepthExtended privilegeDepth
            )
            {
                SetSelectedRoleEntityPrivileges(PrivilegeType.Write, privilegeDepth);
            }
        }

        private void mISetAttributeEntityPrivilegeDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
                && menuItem.Tag != null
                && menuItem.Tag is PrivilegeDepthExtended privilegeDepth
            )
            {
                SetSelectedRoleEntityPrivileges(PrivilegeType.Delete, privilegeDepth);
            }
        }

        private void mISetAttributeEntityPrivilegeAppend_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
                && menuItem.Tag != null
                && menuItem.Tag is PrivilegeDepthExtended privilegeDepth
            )
            {
                SetSelectedRoleEntityPrivileges(PrivilegeType.Append, privilegeDepth);
            }
        }

        private void mISetAttributeEntityPrivilegeAppendTo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
                && menuItem.Tag != null
                && menuItem.Tag is PrivilegeDepthExtended privilegeDepth
            )
            {
                SetSelectedRoleEntityPrivileges(PrivilegeType.AppendTo, privilegeDepth);
            }
        }

        private void mISetAttributeEntityPrivilegeAssign_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
                && menuItem.Tag != null
                && menuItem.Tag is PrivilegeDepthExtended privilegeDepth
            )
            {
                SetSelectedRoleEntityPrivileges(PrivilegeType.Assign, privilegeDepth);
            }
        }

        private void mISetAttributeEntityPrivilegeShare_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
                && menuItem.Tag != null
                && menuItem.Tag is PrivilegeDepthExtended privilegeDepth
            )
            {
                SetSelectedRoleEntityPrivileges(PrivilegeType.Share, privilegeDepth);
            }
        }

        private void SetSelectedRoleEntityPrivileges(PrivilegeDepthExtended privilegeDepth)
        {
            var list = lstVwEntityPrivileges.SelectedItems.Cast<RoleEntityPrivilegeViewItem>().ToList();

            if (!list.Any())
            {
                return;
            }

            foreach (var privilegeType in _privielgeTypesAll)
            {
                foreach (var item in list)
                {
                    SetSelectedRoleEntityPrivileges(item, privilegeType, privilegeDepth);
                }
            }
        }

        private void SetSelectedRoleEntityPrivileges(PrivilegeType privilegeType, PrivilegeDepthExtended privilegeDepth)
        {
            var list = lstVwEntityPrivileges.SelectedItems.Cast<RoleEntityPrivilegeViewItem>().ToList();

            if (!list.Any())
            {
                return;
            }

            foreach (var item in list)
            {
                SetSelectedRoleEntityPrivileges(item, privilegeType, privilegeDepth);
            }
        }

        private static void SetSelectedRoleEntityPrivileges(RoleEntityPrivilegeViewItem item, PrivilegeType privilegeType, PrivilegeDepthExtended privilegeDepth)
        {
            switch (privilegeType)
            {
                case PrivilegeType.Create:
                    if (item.AvailableCreate && item.CreateOptions.Contains(privilegeDepth))
                    {
                        item.CreateRight = privilegeDepth;
                    }
                    break;

                case PrivilegeType.Read:
                    if (item.AvailableRead && item.ReadOptions.Contains(privilegeDepth))
                    {
                        item.ReadRight = privilegeDepth;
                    }
                    break;

                case PrivilegeType.Write:
                    if (item.AvailableUpdate && item.UpdateOptions.Contains(privilegeDepth))
                    {
                        item.UpdateRight = privilegeDepth;
                    }
                    break;

                case PrivilegeType.Delete:
                    if (item.AvailableDelete && item.DeleteOptions.Contains(privilegeDepth))
                    {
                        item.DeleteRight = privilegeDepth;
                    }
                    break;

                case PrivilegeType.Assign:
                    if (item.AvailableAssign && item.AssignOptions.Contains(privilegeDepth))
                    {
                        item.AssignRight = privilegeDepth;
                    }
                    break;

                case PrivilegeType.Share:
                    if (item.AvailableShare && item.ShareOptions.Contains(privilegeDepth))
                    {
                        item.ShareRight = privilegeDepth;
                    }
                    break;

                case PrivilegeType.Append:
                    if (item.AvailableAppend && item.AppendOptions.Contains(privilegeDepth))
                    {
                        item.AppendRight = privilegeDepth;
                    }
                    break;

                case PrivilegeType.AppendTo:
                    if (item.AvailableAppendTo && item.AppendToOptions.Contains(privilegeDepth))
                    {
                        item.AppendToRight = privilegeDepth;
                    }
                    break;

                case PrivilegeType.None:
                default:
                    break;
            }
        }

        private void mISetAttributeOtherPrivilegeRight_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
                && menuItem.Tag != null
                && menuItem.Tag is PrivilegeDepthExtended privilegeDepth
            )
            {
                SetSelectedRoleOtherPrivileges(privilegeDepth);
            }
        }

        private void SetSelectedRoleOtherPrivileges(PrivilegeDepthExtended privilegeDepth)
        {
            var list = lstVwOtherPrivileges.SelectedItems.Cast<RoleOtherPrivilegeViewItem>().ToList();

            if (!list.Any())
            {
                return;
            }

            foreach (var item in list)
            {
                if (item.RightOptions.Contains(privilegeDepth))
                {
                    item.Right = privilegeDepth;
                }
            }
        }

        #endregion Set Attribute

        #region Comparing Privileges

        private async void mIRolePrivilegesWithRolePrivileges_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
            )
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var role1 = entity.ToEntity<Role>();

            var repositoryRole = new RoleRepository(service);

            Func<string, Task<IEnumerable<Role>>> getter = (string filter) => repositoryRole.GetRolesForNotAnotherAsync(filter
                , role1.Id
                , new ColumnSet(
                    Role.Schema.Attributes.name
                    , Role.Schema.Attributes.businessunitid
                    , Role.Schema.Attributes.ismanaged
                    , Role.Schema.Attributes.iscustomizable
                )
            );

            IEnumerable<DataGridColumn> columns = Helpers.SolutionComponentDescription.Implementation.RoleDescriptionBuilder.GetDataGridColumn();

            var form = new WindowEntitySelect<Role>(_iWriteToOutput, service.ConnectionData, Role.EntityLogicalName, getter, columns);

            if (!form.ShowDialog().GetValueOrDefault())
            {
                return;
            }

            if (form.SelectedEntity == null)
            {
                return;
            }

            var role2 = form.SelectedEntity;

            string name1 = string.Format("Role {0}", role1.Name);
            string name2 = string.Format("Role {0}", role2.Name);

            StringBuilder content = new StringBuilder();

            content.AppendLine(Properties.OutputStrings.ConnectingToCRM);
            content.AppendLine(service.ConnectionData.GetConnectionDescription());
            content.AppendFormat(Properties.OutputStrings.CurrentServiceEndpointFormat1, service.CurrentServiceEndpoint).AppendLine();

            string operation = string.Format(Properties.OperationNames.ComparingEntitiesPrivilegesFormat3, service.ConnectionData.Name, name1, name2);

            content.AppendLine(_iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operation));

            ToggleControls(service.ConnectionData, false, Properties.OutputStrings.ComparingEntitiesPrivilegesFormat2, name1, name2);

            var repositoryRolePrivileges = new RolePrivilegesRepository(service);

            var rolePrivileges1 = await repositoryRolePrivileges.GetRolePrivilegesAsync(role1.Id);
            var rolePrivileges2 = await repositoryRolePrivileges.GetRolePrivilegesAsync(role2.Id);

            var hashPrivileges = new HashSet<Guid>(rolePrivileges1.Select(p => p.PrivilegeId).Union(rolePrivileges2.Select(p => p.PrivilegeId)));

            var repositoryPrivilege = new PrivilegeRepository(service);
            var privileges = await repositoryPrivilege.GetListByIdsAsync(hashPrivileges);

            var comparer = new RolePrivilegeComparerHelper(_tabSpacer, name1, name2);

            content.AppendLine();

            var difference = comparer.CompareRolePrivileges(rolePrivileges1, rolePrivileges2, privileges, PrivilegeNameComparer.Comparer);

            difference.ForEach(s => content.AppendLine(s));

            content.AppendLine();

            ToggleControls(service.ConnectionData, true, Properties.OutputStrings.ComparingEntitiesPrivilegesCompletedFormat2, name1, name2);

            content.AppendLine(_iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operation));

            string fileName = EntityFileNameFormatter.ComparingRolePrivilegesInEntitiesFileName(service.ConnectionData.Name, name1, name2);

            string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

            File.WriteAllText(filePath, content.ToString(), new UTF8Encoding(false));

            _iWriteToOutput.PerformAction(service.ConnectionData, filePath);
        }

        private async void mIRolePrivilegesWithUserPrivileges_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
            )
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var role1 = entity.ToEntity<Role>();

            var repositoryUser = new SystemUserRepository(service);

            Func<string, Task<IEnumerable<SystemUser>>> getter = (string filter) => repositoryUser.GetUsersAsync(filter, new ColumnSet(
                SystemUser.Schema.Attributes.domainname
                , SystemUser.Schema.Attributes.fullname
                , SystemUser.Schema.Attributes.businessunitid
                , SystemUser.Schema.Attributes.isdisabled
            ));

            IEnumerable<DataGridColumn> columns = SystemUserRepository.GetDataGridColumn();

            var form = new WindowEntitySelect<SystemUser>(_iWriteToOutput, service.ConnectionData, SystemUser.EntityLogicalName, getter, columns);

            if (!form.ShowDialog().GetValueOrDefault())
            {
                return;
            }

            if (form.SelectedEntity == null)
            {
                return;
            }

            var user2 = form.SelectedEntity;

            string name1 = string.Format("Role {0}", role1.Name);
            string name2 = string.Format("User {0}", user2.FullName);

            StringBuilder content = new StringBuilder();

            content.AppendLine(Properties.OutputStrings.ConnectingToCRM);
            content.AppendLine(service.ConnectionData.GetConnectionDescription());
            content.AppendFormat(Properties.OutputStrings.CurrentServiceEndpointFormat1, service.CurrentServiceEndpoint).AppendLine();

            string operation = string.Format(Properties.OperationNames.ComparingEntitiesPrivilegesFormat3, service.ConnectionData.Name, name1, name2);

            content.AppendLine(_iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operation));

            ToggleControls(service.ConnectionData, false, Properties.OutputStrings.ComparingEntitiesPrivilegesFormat2, name1, name2);

            var repositoryRolePrivileges = new RolePrivilegesRepository(service);

            var rolePrivileges1 = await repositoryRolePrivileges.GetRolePrivilegesAsync(role1.Id);
            var userPrivileges2 = await repositoryRolePrivileges.GetUserPrivilegesAsync(user2.Id);

            var hashPrivileges = new HashSet<Guid>(rolePrivileges1.Select(p => p.PrivilegeId).Union(userPrivileges2.Select(p => p.PrivilegeId)));

            var repositoryPrivilege = new PrivilegeRepository(service);
            var privileges = await repositoryPrivilege.GetListByIdsAsync(hashPrivileges);

            var comparer = new RolePrivilegeComparerHelper(_tabSpacer, name1, name2);

            content.AppendLine();

            var difference = comparer.CompareRolePrivileges(rolePrivileges1, userPrivileges2, privileges, PrivilegeNameComparer.Comparer);

            difference.ForEach(s => content.AppendLine(s));

            content.AppendLine();

            ToggleControls(service.ConnectionData, true, Properties.OutputStrings.ComparingEntitiesPrivilegesCompletedFormat2, name1, name2);

            content.AppendLine(_iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operation));

            string fileName = EntityFileNameFormatter.ComparingRolePrivilegesInEntitiesFileName(service.ConnectionData.Name, name1, name2);

            string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

            File.WriteAllText(filePath, content.ToString(), new UTF8Encoding(false));

            _iWriteToOutput.PerformAction(service.ConnectionData, filePath);
        }

        private async void mIRolePrivilegesWithTeamPrivileges_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
            )
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var role1 = entity.ToEntity<Role>();

            var repositoryTeam = new TeamRepository(service);

            Func<string, Task<IEnumerable<Team>>> getter = (string filter) => repositoryTeam.GetOwnerTeamsAsync(filter, new ColumnSet(
                Team.Schema.Attributes.name
                , Team.Schema.Attributes.businessunitid
                , Team.Schema.Attributes.isdefault
            ));

            IEnumerable<DataGridColumn> columns = TeamRepository.GetDataGridColumnOwner();

            var form = new WindowEntitySelect<Team>(_iWriteToOutput, service.ConnectionData, Team.EntityLogicalName, getter, columns);

            if (!form.ShowDialog().GetValueOrDefault())
            {
                return;
            }

            if (form.SelectedEntity == null)
            {
                return;
            }

            var team2 = form.SelectedEntity;

            string name1 = string.Format("Role {0}", role1.Name);
            string name2 = string.Format("Team {0}", team2.Name);

            StringBuilder content = new StringBuilder();

            content.AppendLine(Properties.OutputStrings.ConnectingToCRM);
            content.AppendLine(service.ConnectionData.GetConnectionDescription());
            content.AppendFormat(Properties.OutputStrings.CurrentServiceEndpointFormat1, service.CurrentServiceEndpoint).AppendLine();

            string operation = string.Format(Properties.OperationNames.ComparingEntitiesPrivilegesFormat3, service.ConnectionData.Name, name1, name2);

            content.AppendLine(_iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operation));

            ToggleControls(service.ConnectionData, false, Properties.OutputStrings.ComparingEntitiesPrivilegesFormat2, name1, name2);

            var repositoryRolePrivileges = new RolePrivilegesRepository(service);

            var rolePrivileges1 = await repositoryRolePrivileges.GetRolePrivilegesAsync(role1.Id);
            var teamPrivileges2 = await repositoryRolePrivileges.GetTeamPrivilegesAsync(team2.Id);

            var hashPrivileges = new HashSet<Guid>(rolePrivileges1.Select(p => p.PrivilegeId).Union(teamPrivileges2.Select(p => p.PrivilegeId)));

            var repositoryPrivilege = new PrivilegeRepository(service);
            var privileges = await repositoryPrivilege.GetListByIdsAsync(hashPrivileges);

            var comparer = new RolePrivilegeComparerHelper(_tabSpacer, name1, name2);

            content.AppendLine();

            var difference = comparer.CompareRolePrivileges(rolePrivileges1, teamPrivileges2, privileges, PrivilegeNameComparer.Comparer);

            difference.ForEach(s => content.AppendLine(s));

            content.AppendLine();

            ToggleControls(service.ConnectionData, true, Properties.OutputStrings.ComparingEntitiesPrivilegesCompletedFormat2, name1, name2);

            content.AppendLine(_iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operation));

            string fileName = EntityFileNameFormatter.ComparingRolePrivilegesInEntitiesFileName(service.ConnectionData.Name, name1, name2);

            string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

            File.WriteAllText(filePath, content.ToString(), new UTF8Encoding(false));

            _iWriteToOutput.PerformAction(service.ConnectionData, filePath);
        }

        #endregion Comparing Privileges

        #region Other Privilege

        private RoleOtherPrivilegeViewItem GetSelectedOtherPrivilege()
        {
            return this.lstVwOtherPrivileges.SelectedItems.Cast<RoleOtherPrivilegeViewItem>().Count() == 1
                ? this.lstVwOtherPrivileges.SelectedItems.Cast<RoleOtherPrivilegeViewItem>().SingleOrDefault() : null;
        }

        private List<RoleOtherPrivilegeViewItem> GetSelectedOtherPrivileges()
        {
            var result = this.lstVwOtherPrivileges.SelectedItems.Cast<RoleOtherPrivilegeViewItem>().ToList();

            return result;
        }

        private void ContextMenuOtherPrivilege_Opened(object sender, RoutedEventArgs e)
        {
            if (!(sender is ContextMenu contextMenu))
            {
                return;
            }

            var items = contextMenu.Items.OfType<Control>();

            ConnectionData connectionData = GetSelectedConnection();

            FillLastSolutionItems(connectionData, items, true, AddOtherPrivilegeToCrmSolutionLast_Click, "contMnAddOtherPrivilegeToSolutionLast");
        }

        private void mIOtherPrivilegeOpenDependentComponentsInWeb_Click(object sender, RoutedEventArgs e)
        {
            var privilege = GetSelectedOtherPrivilege();

            if (privilege == null)
            {
                return;
            }

            var connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                connectionData.OpenSolutionComponentDependentComponentsInWeb(ComponentType.Privilege, privilege.Privilege.Id);
            }
        }

        private async void mIOtherPrivilegeOpenDependentComponentsInExplorer_Click(object sender, RoutedEventArgs e)
        {
            var privilege = GetSelectedOtherPrivilege();

            if (privilege == null)
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            _commonConfig.Save();

            WindowHelper.OpenSolutionComponentDependenciesExplorer(_iWriteToOutput, service, null, _commonConfig, (int)ComponentType.Privilege, privilege.Privilege.Id, null);
        }

        private async void mIOtherPrivilegeOpenSolutionsContainingComponentInExplorer_Click(object sender, RoutedEventArgs e)
        {
            var privilege = GetSelectedOtherPrivilege();

            if (privilege == null)
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
                , (int)ComponentType.Privilege
                , privilege.Privilege.Id
                , null
            );
        }

        private async void AddOtherPrivilegeToCrmSolution_Click(object sender, RoutedEventArgs e)
        {
            await AddOtherPrivilegeToSolution(true, null);
        }

        private async void AddOtherPrivilegeToCrmSolutionLast_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
               && menuItem.Tag != null
               && menuItem.Tag is string solutionUniqueName
               )
            {
                await AddOtherPrivilegeToSolution(false, solutionUniqueName);
            }
        }

        private async Task AddOtherPrivilegeToSolution(bool withSelect, string solutionUniqueName)
        {
            var otherPrivilegesList = GetSelectedOtherPrivileges()
                .Select(item => item.Privilege.Id)
                .ToList();

            if (!otherPrivilegesList.Any())
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            _commonConfig.Save();

            try
            {
                this._iWriteToOutput.ActivateOutputWindow(service.ConnectionData);

                await SolutionController.AddSolutionComponentsGroupToSolution(_iWriteToOutput, service, null, _commonConfig, solutionUniqueName, ComponentType.Privilege, otherPrivilegesList, null, withSelect);
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);
            }
        }

        #endregion Other Privilege

        #region Clipboard Other Privilege

        private void mIClipboardPrivilegeCopyName_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<RoleOtherPrivilegeViewItem>(e, ent => ent.Name);
        }

        private void mIClipboardPrivilegeCopyType_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<RoleOtherPrivilegeViewItem>(e, ent => ent.PrivilegeType);
        }

        private void mIClipboardPrivilegeCopyLinkedEntity_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<RoleOtherPrivilegeViewItem>(e, ent => ent.EntityLogicalName);
        }

        private void mIClipboardPrivilegeCopyPrivilegeId_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<RoleOtherPrivilegeViewItem>(e, ent => ent.Privilege.Id.ToString());
        }

        #endregion Clipboard Other Privilege

        private class RolePrivilegeComparer : IEqualityComparer<RolePrivilege>
        {
            public bool Equals(RolePrivilege x, RolePrivilege y)
            {
                if (x == null && y == null)
                {
                    return true;
                }
                else if (x == null || y == null)
                {
                    return false;
                }
                else if (x.PrivilegeId == y.PrivilegeId)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public int GetHashCode(RolePrivilege obj)
            {
                int result = obj.PrivilegeId.GetHashCode();

                return result;
            }
        }

        #region Clipboard Entity

        private void mIClipboardEntityCopyName_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<RoleEntityPrivilegeViewItem>(e, ent => ent.LogicalName);
        }

        private void mIClipboardEntityCopyDisplayName_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<RoleEntityPrivilegeViewItem>(e, ent => ent.DisplayName);
        }

        private void mIClipboardEntityCopyObjectTypeCode_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<RoleEntityPrivilegeViewItem>(e, ent => ent.ObjectTypeCode.ToString());
        }

        private void mIClipboardEntityCopyEntityMetadataId_Click(object sender, RoutedEventArgs e)
        {
            GetEntityViewItemAndCopyToClipboard<RoleEntityPrivilegeViewItem>(e, ent => ent.EntityMetadata.MetadataId.ToString());
        }

        #endregion Clipboard Entity

        private async void btnChangeEntityInEditor_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Entity entity)
            )
            {
                return;
            }

            var service = await GetService();

            WindowHelper.OpenEntityEditor(_iWriteToOutput, service, _commonConfig, entity.LogicalName, entity.Id);
        }
    }
}