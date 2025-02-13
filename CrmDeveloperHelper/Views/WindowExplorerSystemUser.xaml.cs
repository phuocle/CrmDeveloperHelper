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
    public partial class WindowExplorerSystemUser : WindowWithSolutionComponentDescriptor
    {
        private string _tabSpacer = "    ";

        private readonly Popup _popupEntityMetadataFilter;
        private readonly EntityMetadataFilter _entityMetadataFilter;

        private readonly Popup _popupPrivilegeFilter;
        private readonly PrivilegeFilter _privilegeFilter;

        private readonly ObservableCollection<SystemUser> _itemsSourceSystemUsers;

        private readonly ObservableCollection<Team> _itemsSourceTeams;
        private readonly ObservableCollection<Role> _itemsSourceRoles;
        private readonly ObservableCollection<Role> _itemsSourceRolesByTeams;

        private readonly ObservableCollection<RoleEntityPrivilegeViewItem> _itemsSourceEntityPrivileges;
        private readonly ObservableCollection<RoleOtherPrivilegeViewItem> _itemsSourceOtherPrivileges;

        private readonly List<RoleEntityPrivilegeViewItem> _currentRoleEntityPrivileges;
        private readonly List<RoleOtherPrivilegeViewItem> _currentRoleOtherPrivileges;

        private readonly Dictionary<Guid, IEnumerable<EntityMetadata>> _cacheEntityMetadata = new Dictionary<Guid, IEnumerable<EntityMetadata>>();
        private readonly Dictionary<Guid, IEnumerable<Privilege>> _cachePrivileges = new Dictionary<Guid, IEnumerable<Privilege>>();

        public WindowExplorerSystemUser(
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

                PlacementTarget = lblFilterEntity,
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

            txtBFilterSystemUser.Text = filterEntity;
            txtBFilterSystemUser.SelectionLength = 0;
            txtBFilterSystemUser.SelectionStart = txtBFilterSystemUser.Text.Length;

            txtBFilterSystemUser.Focus();

            lstVwSystemUsers.ItemsSource = _itemsSourceSystemUsers = new ObservableCollection<SystemUser>();

            lstVwTeams.ItemsSource = _itemsSourceTeams = new ObservableCollection<Team>();
            lstVwSecurityRoles.ItemsSource = _itemsSourceRoles = new ObservableCollection<Role>();
            lstVwSecurityRolesByTeams.ItemsSource = _itemsSourceRolesByTeams = new ObservableCollection<Role>();
            lstVwEntityPrivileges.ItemsSource = _itemsSourceEntityPrivileges = new ObservableCollection<RoleEntityPrivilegeViewItem>();
            lstVwOtherPrivileges.ItemsSource = _itemsSourceOtherPrivileges = new ObservableCollection<RoleOtherPrivilegeViewItem>();

            _currentRoleEntityPrivileges = new List<RoleEntityPrivilegeViewItem>();
            _currentRoleOtherPrivileges = new List<RoleOtherPrivilegeViewItem>();

            UpdateSystemUsersButtons();

            cmBCurrentConnection.ItemsSource = service.ConnectionData.ConnectionConfiguration.Connections;
            cmBCurrentConnection.SelectedItem = service.ConnectionData;

            FillExplorersMenuItems();

            this.DecreaseInit();

            var task = ShowExistingSystemUsers();
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

        private const string paramColumnSystemUserWidth = "ColumnSystemUserWidth";

        private void LoadFormSettings(WindowSettings winConfig)
        {
            if (winConfig.DictDouble.ContainsKey(paramColumnSystemUserWidth))
            {
                columnSystemUser.Width = new GridLength(winConfig.DictDouble[paramColumnSystemUserWidth]);
            }
        }

        protected override void SaveConfigurationInternal(WindowSettings winConfig)
        {
            base.SaveConfigurationInternal(winConfig);

            winConfig.DictDouble[paramColumnSystemUserWidth] = columnSystemUser.Width.Value;
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

        private async Task ShowExistingSystemUsers()
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            ToggleControls(connectionData, false, Properties.OutputStrings.LoadingSystemUsers);

            string textName = string.Empty;

            this.Dispatcher.Invoke(() =>
            {
                _itemsSourceSystemUsers.Clear();

                _itemsSourceTeams.Clear();

                _itemsSourceRoles.Clear();
                _itemsSourceRolesByTeams.Clear();

                _itemsSourceEntityPrivileges.Clear();
                _itemsSourceOtherPrivileges.Clear();

                _currentRoleEntityPrivileges.Clear();
                _currentRoleOtherPrivileges.Clear();

                textName = txtBFilterSystemUser.Text.Trim().ToLower();
            });

            IEnumerable<SystemUser> list = Enumerable.Empty<SystemUser>();

            try
            {
                var service = await GetService();

                if (service != null)
                {
                    SystemUserRepository repository = new SystemUserRepository(service);

                    list = await repository.GetListAsync(textName, new ColumnSet(SystemUser.Schema.Attributes.fullname, SystemUser.Schema.Attributes.domainname, SystemUser.Schema.Attributes.businessunitid, SystemUser.Schema.Attributes.isdisabled));
                }
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(connectionData, ex);
            }

            this.lstVwSystemUsers.Dispatcher.Invoke(() =>
            {
                foreach (var entity in list.OrderBy(s => s.FullName).ThenBy(s => s.DomainName))
                {
                    _itemsSourceSystemUsers.Add(entity);
                }

                if (this.lstVwSystemUsers.Items.Count == 1)
                {
                    this.lstVwSystemUsers.SelectedItem = this.lstVwSystemUsers.Items[0];
                }
            });

            ToggleControls(connectionData, true, Properties.OutputStrings.LoadingSystemUsersCompletedFormat1, list.Count());

            await RefreshSystemUserInfo();
        }

        private async Task ShowSystemUserRoles()
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            ToggleControls(connectionData, false, Properties.OutputStrings.LoadingSystemUserSecurityRoles);

            string filterRole = string.Empty;

            this.Dispatcher.Invoke(() =>
            {
                _itemsSourceRoles.Clear();
                _itemsSourceRolesByTeams.Clear();

                filterRole = txtBFilterRole.Text.Trim().ToLower();
            });

            var user = GetSelectedSystemUser();

            IEnumerable<Role> list = Enumerable.Empty<Role>();
            IEnumerable<Role> listByTeams = Enumerable.Empty<Role>();

            try
            {
                var service = await GetService();

                if (service != null && user != null)
                {
                    RoleRepository repository = new RoleRepository(service);

                    list = await repository.GetUserRolesAsync(user.Id, filterRole, new ColumnSet(
                        Role.Schema.Attributes.name
                        , Role.Schema.Attributes.businessunitid
                        , Role.Schema.Attributes.ismanaged
                        , Role.Schema.Attributes.iscustomizable
                    ));

                    listByTeams = await repository.GetUserRolesByTeamsAsync(user.Id, filterRole, new ColumnSet(
                        Role.Schema.Attributes.name
                        , Role.Schema.Attributes.businessunitid
                        , Role.Schema.Attributes.ismanaged
                        , Role.Schema.Attributes.iscustomizable
                    ));
                }
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(connectionData, ex);
            }

            this.lstVwSecurityRoles.Dispatcher.Invoke(() =>
            {
                foreach (var entity in list.OrderBy(s => s.Name).ThenBy(s => s.BusinessUnitId?.Name))
                {
                    _itemsSourceRoles.Add(entity);
                }

                if (this.lstVwSecurityRoles.Items.Count == 1)
                {
                    this.lstVwSecurityRoles.SelectedItem = this.lstVwSecurityRoles.Items[0];
                }
            });

            this.lstVwSecurityRolesByTeams.Dispatcher.Invoke(() =>
            {
                foreach (var entity in listByTeams.OrderBy(s => s.Name).ThenBy(s => s.BusinessUnitId?.Name))
                {
                    _itemsSourceRolesByTeams.Add(entity);
                }

                if (this.lstVwSecurityRolesByTeams.Items.Count == 1)
                {
                    this.lstVwSecurityRolesByTeams.SelectedItem = this.lstVwSecurityRolesByTeams.Items[0];
                }
            });

            ToggleControls(connectionData, true, Properties.OutputStrings.LoadingSystemUserSecurityRolesCompleted);
        }

        private async Task ShowSystemUserTeams()
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            ToggleControls(connectionData, false, Properties.OutputStrings.LoadingSystemUserTeams);

            string filterTeam = string.Empty;

            this.Dispatcher.Invoke(() =>
            {
                _itemsSourceTeams.Clear();

                filterTeam = txtBFilterTeams.Text.Trim().ToLower();
            });

            var user = GetSelectedSystemUser();

            IEnumerable<Team> list = Enumerable.Empty<Team>();

            try
            {
                var service = await GetService();

                if (service != null && user != null)
                {
                    var repository = new TeamRepository(service);

                    list = await repository.GetUserTeamsAsync(user.Id, filterTeam, new ColumnSet(
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
                foreach (var entity in list
                    .OrderBy(s => s.TeamType?.Value)
                    .ThenBy(s => s.RegardingObjectId?.LogicalName)
                    .ThenBy(s => s.TeamTemplateName)
                    .ThenBy(s => s.Name)
                )
                {
                    _itemsSourceTeams.Add(entity);
                }

                if (this.lstVwTeams.Items.Count == 1)
                {
                    this.lstVwTeams.SelectedItem = this.lstVwTeams.Items[0];
                }
            });

            ToggleControls(connectionData, true, Properties.OutputStrings.LoadingSystemUserTeamsCompletedFormat1, list.Count());
        }

        private async Task ShowSystemUserEntityPrivileges()
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

            var user = GetSelectedSystemUser();

            try
            {
                var service = await GetService();

                if (service != null)
                {
                    var otherPrivileges = await GetPrivileges(service);

                    var entityMetadataList = await GetEntityMetadataEnumerable(service);

                    entityMetadataList = entityMetadataList.Where(e => e.Privileges != null && e.Privileges.Any(p => p.PrivilegeType != PrivilegeType.None));

                    if (user != null)
                    {
                        if (entityMetadataList.Any() || otherPrivileges.Any())
                        {
                            var repository = new RolePrivilegesRepository(service);

                            var userPrivileges = await repository.GetUserPrivilegesAsync(user.Id);

                            _currentRoleEntityPrivileges.AddRange(entityMetadataList.Select(e => new RoleEntityPrivilegeViewItem(e, userPrivileges)));

                            _currentRoleOtherPrivileges.AddRange(otherPrivileges.Select(e => new RoleOtherPrivilegeViewItem(e, userPrivileges)));
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
                filterEntity = txtBEntityFilter.Text.Trim().ToLower();
                _itemsSourceEntityPrivileges.Clear();
            });

            IEnumerable<RoleEntityPrivilegeViewItem> listEntityPrivileges = FilterEntityList(_currentRoleEntityPrivileges, filterEntity);

            this.lstVwEntityPrivileges.Dispatcher.Invoke(() =>
            {
                foreach (var entity in listEntityPrivileges
                    .OrderBy(s => s.IsIntersect)
                    .ThenBy(s => s.LogicalName)
                )
                {
                    _itemsSourceEntityPrivileges.Add(entity);
                }

                if (this.lstVwEntityPrivileges.Items.Count == 1)
                {
                    this.lstVwEntityPrivileges.SelectedItem = this.lstVwEntityPrivileges.Items[0];
                }
            });

            ToggleControls(connectionData, true, Properties.OutputStrings.FilteringEntitiesCompletedFormat1, listEntityPrivileges.Count());
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
                filterOtherPrivilege = txtBOtherPrivilegesFilter.Text.Trim().ToLower();
                _itemsSourceOtherPrivileges.Clear();
            });

            IEnumerable<RoleOtherPrivilegeViewItem> listOtherPrivileges = FilterPrivilegeList(_currentRoleOtherPrivileges, filterOtherPrivilege);

            this.lstVwOtherPrivileges.Dispatcher.Invoke(() =>
            {
                foreach (var otherPriv in listOtherPrivileges
                    .OrderBy(s => s.EntityLogicalName)
                    .ThenBy(s => s.Name, PrivilegeNameComparer.Comparer)
                )
                {
                    _itemsSourceOtherPrivileges.Add(otherPriv);
                }

                if (this.lstVwOtherPrivileges.Items.Count == 1)
                {
                    this.lstVwOtherPrivileges.SelectedItem = this.lstVwOtherPrivileges.Items[0];
                }
            });

            ToggleControls(connectionData, true, Properties.OutputStrings.FilteringOtherPrivilegesCompletedFormat1, listOtherPrivileges.Count());
        }

        private async Task<IEnumerable<Privilege>> GetPrivileges(IOrganizationServiceExtented service)
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
                    list = list.Where(ent =>
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

            ToggleControl(this.tSProgressBar, cmBCurrentConnection, btnSetCurrentConnection, btnRefreshEntites, btnRefreshRoles, btnRefreshSystemUsers, btnRefreshTeams, tSProgressBar);

            UpdateSystemUsersButtons();

            UpdateSecurityRolesButtons();

            UpdateTeamsButtons();
        }

        private void UpdateSystemUsersButtons()
        {
            this.lstVwSystemUsers.Dispatcher.Invoke(() =>
            {
                try
                {
                    bool enabled = this.IsControlsEnabled
                        && this.lstVwSystemUsers != null
                        && this.lstVwSystemUsers.SelectedItems.OfType<SystemUser>().Any();

                    UIElement[] list = { btnAssignRoleToUser, btnAddUserToTeam };

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

        private async void txtBFilterSystemUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await ShowExistingSystemUsers();
            }
        }

        private void txtBEntityFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformFilterEntityPrivileges();
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
                await ShowSystemUserTeams();
            }
        }

        private async void txtBFilterRole_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await ShowSystemUserRoles();
            }
        }

        private SystemUser GetSelectedSystemUser()
        {
            SystemUser result = null;

            lstVwSystemUsers.Dispatcher.Invoke(() =>
            {
                result = this.lstVwSystemUsers.SelectedItems.OfType<SystemUser>().Count() == 1
                    ? this.lstVwSystemUsers.SelectedItems.OfType<SystemUser>().SingleOrDefault() : null;
            });

            return result;
        }

        private List<SystemUser> GetSelectedSystemUsers()
        {
            List<SystemUser> result = null;

            lstVwSystemUsers.Dispatcher.Invoke(() =>
            {
                result = this.lstVwSystemUsers.SelectedItems.OfType<SystemUser>().ToList();
            });

            return result;
        }

        private List<Team> GetSelectedTeams()
        {
            List<Team> result = null;

            lstVwTeams.Dispatcher.Invoke(() =>
            {
                result = this.lstVwTeams.SelectedItems.OfType<Team>().Where(u => !u.IsDefault.GetValueOrDefault()).ToList();
            });

            return result;
        }

        private RoleEntityPrivilegeViewItem GetSelectedEntity()
        {
            RoleEntityPrivilegeViewItem result = null;

            lstVwEntityPrivileges.Dispatcher.Invoke(() =>
            {
                result = this.lstVwEntityPrivileges.SelectedItems.OfType<RoleEntityPrivilegeViewItem>().Count() == 1
                     ? this.lstVwEntityPrivileges.SelectedItems.OfType<RoleEntityPrivilegeViewItem>().SingleOrDefault() : null;
            });

            return result;
        }

        private List<RoleEntityPrivilegeViewItem> GetSelectedEntities()
        {
            List<RoleEntityPrivilegeViewItem> result = null;

            lstVwEntityPrivileges.Dispatcher.Invoke(() =>
            {
                result = this.lstVwEntityPrivileges.SelectedItems.OfType<RoleEntityPrivilegeViewItem>().ToList();
            });

            return result;
        }

        private List<Role> GetSelectedRoles()
        {
            List<Role> result = null;

            lstVwSecurityRoles.Dispatcher.Invoke(() =>
            {
                result = this.lstVwSecurityRoles.SelectedItems.OfType<Role>().ToList();
            });

            return result;
        }

        private List<Role> GetSelectedRolesByTeam()
        {
            List<Role> result = null;

            lstVwSecurityRolesByTeams.Dispatcher.Invoke(() =>
            {
                result = this.lstVwSecurityRolesByTeams.SelectedItems.OfType<Role>().ToList();
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
            if (e.ChangedButton == MouseButton.Left)
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

        private async void lstVwSystemUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await RefreshSystemUserInfo();
        }

        private async Task RefreshSystemUserInfo()
        {
            ConnectionData connectionData = GetSelectedConnection();

            try
            {
                await ShowSystemUserRoles();

                await ShowSystemUserTeams();

                await ShowSystemUserEntityPrivileges();
            }
            catch (Exception ex)
            {
                this._iWriteToOutput.WriteErrorToOutput(connectionData, ex);
            }
        }

        protected override async Task OnRefreshList(ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            await ShowExistingSystemUsers();
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

        private async void cmBCurrentConnection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsControlsEnabled)
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                await ShowExistingSystemUsers();
            }
        }

        private async void mIClearEntityCacheAndRefresh_Click(object sender, RoutedEventArgs e)
        {
            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                _cacheEntityMetadata.Remove(connectionData.ConnectionId);
                _cachePrivileges.Remove(connectionData.ConnectionId);

                await RefreshSystemUserInfo();
            }
        }

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
            GetEntityViewItemAndCopyToClipboard<Entity>(e, ent => ent.Id.ToString());
        }

        private void mICopyEntityInstanceUrlToClipboard_Click(object sender, RoutedEventArgs e)
        {
            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                GetEntityViewItemAndCopyToClipboard<Entity>(e, ent => connectionData.GetEntityInstanceUrl(ent.LogicalName, ent.Id));
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

            _iWriteToOutput.WriteToOutput(service.ConnectionData, Properties.OutputStrings.InConnectionExportedEntityDescriptionFormat3
                   , service.ConnectionData.Name
                   , entityFull.LogicalName
                   , filePath);

            _iWriteToOutput.PerformAction(service.ConnectionData, filePath);
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

        private void mIOpenRoleTeamInWeb_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Role role)
                || !role.TeamId.HasValue
                )
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                connectionData.OpenEntityInstanceInWeb(Team.EntityLogicalName, role.TeamId.Value);
            }
        }

        private void mICopyRoleTeamIdToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Role role)
                || !role.TeamId.HasValue
                )
            {
                return;
            }

            ClipboardHelper.SetText(role.TeamId.Value.ToString());
        }

        private void mICopyRoleTeamUrlToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Role role)
                || !role.TeamId.HasValue
                )
            {
                return;
            }

            ConnectionData connectionData = GetSelectedConnection();

            if (connectionData != null)
            {
                var url = connectionData.GetEntityInstanceUrl(Team.EntityLogicalName, role.TeamId.Value);

                ClipboardHelper.SetText(url);
            }
        }

        private async void mIOpenRoleTeamExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Role role)
                || !role.TeamId.HasValue
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

            WindowHelper.OpenTeamsExplorer(_iWriteToOutput, service, _commonConfig, role.TeamName, entityMetadataList, privileges);
        }

        private async void mICreateRoleTeamEntityDescription_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem))
            {
                return;
            }

            if (menuItem.DataContext == null
                || !(menuItem.DataContext is Role role)
                || !role.TeamId.HasValue
            )
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var entityFull = service.RetrieveByQuery<Entity>(Role.EntityLogicalName, role.TeamId.Value, ColumnSetInstances.AllColumns);

            string fileName = EntityFileNameFormatter.GetEntityName(service.ConnectionData.Name, entityFull, EntityFileNameFormatter.Headers.EntityDescription, FileExtension.txt);
            string filePath = Path.Combine(_commonConfig.FolderForExport, FileOperations.RemoveWrongSymbols(fileName));

            await EntityDescriptionHandler.ExportEntityDescriptionAsync(filePath, entityFull, service.ConnectionData);

            _iWriteToOutput.WriteToOutput(service.ConnectionData, Properties.OutputStrings.InConnectionExportedEntityDescriptionFormat3
                   , service.ConnectionData.Name
                   , entityFull.LogicalName
                   , filePath);

            DTEHelper.Singleton?.PerformAction(service.ConnectionData, filePath);
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

        private void ContextMenuRoleByTeam_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu contextMenu)
            {
                var items = contextMenu.Items.OfType<Control>();

                ConnectionData connectionData = GetSelectedConnection();

                FillLastSolutionItems(connectionData, items, true, AddRoleByTeamToCrmSolutionLast_Click, "contMnAddToSolutionLast");
            }
        }

        private async void AddRoleByTeamToCrmSolution_Click(object sender, RoutedEventArgs e)
        {
            await AddRoleByTeamToSolution(true, null);
        }

        private async void AddRoleByTeamToCrmSolutionLast_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
               && menuItem.Tag != null
               && menuItem.Tag is string solutionUniqueName
               )
            {
                await AddRoleByTeamToSolution(false, solutionUniqueName);
            }
        }

        private async Task AddRoleByTeamToSolution(bool withSelect, string solutionUniqueName)
        {
            var roleList = GetSelectedRolesByTeam()
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

        private void lstVwSecurityRoles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSecurityRolesButtons();
        }

        private void UpdateSecurityRolesButtons()
        {
            this.lstVwSecurityRoles.Dispatcher.Invoke(() =>
            {
                try
                {
                    bool enabled = this.IsControlsEnabled
                        && this.lstVwSecurityRoles != null
                        && this.lstVwSecurityRoles.SelectedItems.Count > 0;

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

        private async void btnAssignRoleToUser_Click(object sender, RoutedEventArgs e)
        {
            var userList = GetSelectedSystemUsers();

            if (userList == null || !userList.Any())
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            var repository = new RoleRepository(service);

            Func<string, Task<IEnumerable<Role>>> getter = null;

            var columnSet = new ColumnSet(
                Role.Schema.Attributes.name
                , Role.Schema.Attributes.businessunitid
                , Role.Schema.Attributes.ismanaged
                , Role.Schema.Attributes.iscustomizable
            );

            if (userList.Count() == 1)
            {
                var user = userList.First();

                getter = (string filter) => repository.GetAvailableRolesForUserAsync(filter, user.Id, columnSet);
            }
            else
            {
                getter = (string filter) => repository.GetListAsync(filter, columnSet);
            }

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

            var role = form.SelectedEntity;

            string usersName = string.Join(", ", userList.Select(r => string.Format("{0} - {1}", r.DomainName, r.FullName)).OrderBy(s => s));
            string rolesName = role.Name;

            string operationName = string.Format(Properties.OperationNames.AssigningRolesToUsersFormat3, service.ConnectionData.Name, rolesName, usersName);

            _iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operationName);

            ToggleControls(service.ConnectionData, false, Properties.OutputStrings.InConnectionAssigningRolesToUsersFormat3, service.ConnectionData.Name, rolesName, usersName);

            var repositoryRolePrivileges = new RolePrivilegesRepository(service);

            foreach (var user in userList)
            {
                try
                {
                    await repositoryRolePrivileges.AssignRolesToUserAsync(user.Id, new[] { role.Id });
                }
                catch (Exception ex)
                {
                    _iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);
                }
            }

            ToggleControls(service.ConnectionData, true, Properties.OutputStrings.InConnectionAssigningRolesToUsersCompletedFormat3, service.ConnectionData.Name, rolesName, usersName);

            _iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operationName);

            await RefreshSystemUserInfo();
        }

        private async void btnRemoveRoleFromUser_Click(object sender, RoutedEventArgs e)
        {
            var user = GetSelectedSystemUser();

            var roleList = GetSelectedRoles();

            if (user == null || roleList == null || !roleList.Any())
            {
                return;
            }

            string rolesName = string.Join(", ", roleList.Select(r => r.Name).OrderBy(s => s));
            string usersName = string.Format("{0} - {1}", user.DomainName, user.FullName);

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

                await repository.RemoveRolesFromUserAsync(user.Id, roleList.Select(r => r.Id));
            }
            catch (Exception ex)
            {
                _iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);
            }

            ToggleControls(service.ConnectionData, true, Properties.OutputStrings.InConnectionRemovingRolesFromUsersCompletedFormat3, service.ConnectionData.Name, rolesName, usersName);

            _iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operationName);

            await RefreshSystemUserInfo();
        }

        private async void btnAddUserToTeam_Click(object sender, RoutedEventArgs e)
        {
            var userList = GetSelectedSystemUsers();

            if (userList == null || !userList.Any())
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

            if (userList.Count == 1)
            {
                var user = userList.First();

                getter = (string filter) => repository.GetAvailableTeamsForUserAsync(filter, user.Id, columnSet);
            }
            else
            {
                getter = (string filter) => repository.GetNotDefaultTeamsAsync(filter, columnSet);
            }

            IEnumerable<DataGridColumn> columns = TeamRepository.GetDataGridColumn();

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

            string usersName = string.Join(", ", userList.Select(r => string.Format("{0} - {1}", r.DomainName, r.FullName)).OrderBy(s => s));
            string teamsName = team.Name;

            string operationName = string.Format(Properties.OperationNames.AddingUsersToTeamsFormat3, service.ConnectionData.Name, usersName, teamsName);

            _iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operationName);

            ToggleControls(service.ConnectionData, false, Properties.OutputStrings.InConnectionAddingUsersToTeamsFormat3, service.ConnectionData.Name, usersName, teamsName);

            try
            {
                await repository.AddUsersToTeamAsync(form.SelectedEntity.Id, userList.Select(r => r.Id));
            }
            catch (Exception ex)
            {
                _iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);
            }

            ToggleControls(service.ConnectionData, true, Properties.OutputStrings.InConnectionAddingUsersToTeamsCompletedFormat3, service.ConnectionData.Name, usersName, teamsName);

            _iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operationName);

            await RefreshSystemUserInfo();
        }

        private async void btnRemoveUserFromTeam_Click(object sender, RoutedEventArgs e)
        {
            var user = GetSelectedSystemUser();

            var teamList = GetSelectedTeams();

            if (user == null || teamList == null || !teamList.Any())
            {
                return;
            }

            string usersName = string.Format("{0} - {1}", user.DomainName, user.FullName);
            string teamsName = string.Join(", ", teamList.Select(r => r.Name).OrderBy(s => s));

            string message = string.Format(Properties.MessageBoxStrings.AreYouSureRemoveUsersFromTeamsFormat2, usersName, teamsName);

            if (MessageBox.Show(message, Properties.MessageBoxStrings.QuestionTitle, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            var service = await GetService();

            if (service == null)
            {
                return;
            }

            string operationName = string.Format(Properties.OperationNames.RemovingUsersFromTeamsFormat3, service.ConnectionData.Name, usersName, teamsName);

            _iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operationName);

            ToggleControls(service.ConnectionData, false, Properties.OutputStrings.InConnectionRemovingUsersFromTeamsFormat3, service.ConnectionData.Name, usersName, teamsName);

            try
            {
                var repository = new TeamRepository(service);

                await repository.RemoveUserFromTeamsAsync(user.Id, teamList.Select(r => r.Id));
            }
            catch (Exception ex)
            {
                _iWriteToOutput.WriteErrorToOutput(service.ConnectionData, ex);
            }

            ToggleControls(service.ConnectionData, true, Properties.OutputStrings.InConnectionRemovingUsersFromTeamsCompletedFormat3, service.ConnectionData.Name, usersName, teamsName);

            _iWriteToOutput.WriteToOutputEndOperation(service.ConnectionData, operationName);

            await RefreshSystemUserInfo();
        }

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
                        && this.lstVwTeams.SelectedItems.OfType<Team>().Any(t => !t.IsDefault.GetValueOrDefault());

                    UIElement[] list = { btnRemoveUserFromTeam };

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
            await ShowExistingSystemUsers();
        }

        private async void btnRefreshRoles_Click(object sender, RoutedEventArgs e)
        {
            await ShowSystemUserRoles();
        }

        private async void btnRefreshTeams_Click(object sender, RoutedEventArgs e)
        {
            await ShowSystemUserTeams();
        }

        private async void btnRefreshEntites_Click(object sender, RoutedEventArgs e)
        {
            await ShowSystemUserEntityPrivileges();
        }

        private void btnSetCurrentConnection_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentConnection(_iWriteToOutput, GetSelectedConnection());
        }

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

            var user1 = entity.ToEntity<SystemUser>();

            var repositoryRole = new RoleRepository(service);

            Func<string, Task<IEnumerable<Role>>> getter = (string filter) => repositoryRole.GetListAsync(filter, new ColumnSet(
                Role.Schema.Attributes.name
                , Role.Schema.Attributes.businessunitid
                , Role.Schema.Attributes.ismanaged
                , Role.Schema.Attributes.iscustomizable
            ));

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

            string name1 = string.Format("User {0}", user1.FullName);
            string name2 = string.Format("Role {0}", role2.Name);

            var content = new StringBuilder();

            var privilegeComparer = PrivilegeNameComparer.Comparer;

            content.AppendLine(Properties.OutputStrings.ConnectingToCRM);
            content.AppendLine(service.ConnectionData.GetConnectionDescription());
            content.AppendFormat(Properties.OutputStrings.CurrentServiceEndpointFormat1, service.CurrentServiceEndpoint).AppendLine();

            string operation = string.Format(Properties.OperationNames.ComparingEntitiesPrivilegesFormat3, service.ConnectionData.Name, name1, name2);

            content.AppendLine(_iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operation));

            ToggleControls(service.ConnectionData, false, Properties.OutputStrings.ComparingEntitiesPrivilegesFormat2, name1, name2);

            var repositoryRolePrivileges = new RolePrivilegesRepository(service);

            var userPrivileges1 = await repositoryRolePrivileges.GetUserPrivilegesAsync(user1.Id);
            var rolePrivileges2 = await repositoryRolePrivileges.GetRolePrivilegesAsync(role2.Id);

            var hashPrivileges = new HashSet<Guid>(userPrivileges1.Select(p => p.PrivilegeId).Union(rolePrivileges2.Select(p => p.PrivilegeId)));

            var repositoryPrivilege = new PrivilegeRepository(service);
            var privileges = await repositoryPrivilege.GetListByIdsAsync(hashPrivileges);

            var comparer = new RolePrivilegeComparerHelper(_tabSpacer, name1, name2);

            content.AppendLine();

            var difference = comparer.CompareRolePrivileges(userPrivileges1, rolePrivileges2, privileges, PrivilegeNameComparer.Comparer);

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

            var user1 = entity.ToEntity<SystemUser>();

            var repositoryUser = new SystemUserRepository(service);

            Func<string, Task<IEnumerable<SystemUser>>> getter = (string filter) => repositoryUser.GetUsersNotAnotherAsync(filter, user1.Id, new ColumnSet(
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

            string name1 = string.Format("User {0}", user1.FullName);
            string name2 = string.Format("User {0}", user2.FullName);

            StringBuilder content = new StringBuilder();

            var privilegeComparer = PrivilegeNameComparer.Comparer;

            content.AppendLine(Properties.OutputStrings.ConnectingToCRM);
            content.AppendLine(service.ConnectionData.GetConnectionDescription());
            content.AppendFormat(Properties.OutputStrings.CurrentServiceEndpointFormat1, service.CurrentServiceEndpoint).AppendLine();

            string operation = string.Format(Properties.OperationNames.ComparingEntitiesPrivilegesFormat3, service.ConnectionData.Name, name1, name2);

            content.AppendLine(_iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operation));

            ToggleControls(service.ConnectionData, false, Properties.OutputStrings.ComparingEntitiesPrivilegesFormat2, name1, name2);

            var repositoryRolePrivileges = new RolePrivilegesRepository(service);

            var rolePrivileges1 = await repositoryRolePrivileges.GetUserPrivilegesAsync(user1.Id);
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

            var user1 = entity.ToEntity<SystemUser>();

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

            string name1 = string.Format("User {0}", user1.FullName);
            string name2 = string.Format("Team {0}", team2.Name);

            StringBuilder content = new StringBuilder();

            var privilegeComparer = PrivilegeNameComparer.Comparer;

            content.AppendLine(Properties.OutputStrings.ConnectingToCRM);
            content.AppendLine(service.ConnectionData.GetConnectionDescription());
            content.AppendFormat(Properties.OutputStrings.CurrentServiceEndpointFormat1, service.CurrentServiceEndpoint).AppendLine();

            string operation = string.Format(Properties.OperationNames.ComparingEntitiesPrivilegesFormat3, service.ConnectionData.Name, name1, name2);

            content.AppendLine(_iWriteToOutput.WriteToOutputStartOperation(service.ConnectionData, operation));

            ToggleControls(service.ConnectionData, false, Properties.OutputStrings.ComparingEntitiesPrivilegesFormat2, name1, name2);

            var repositoryRolePrivileges = new RolePrivilegesRepository(service);

            var rolePrivileges1 = await repositoryRolePrivileges.GetUserPrivilegesAsync(user1.Id);
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

        #region Other Privilege

        private RoleOtherPrivilegeViewItem GetSelectedOtherPrivilege()
        {
            return this.lstVwOtherPrivileges.SelectedItems.OfType<RoleOtherPrivilegeViewItem>().Count() == 1
                ? this.lstVwOtherPrivileges.SelectedItems.OfType<RoleOtherPrivilegeViewItem>().SingleOrDefault() : null;
        }

        private List<RoleOtherPrivilegeViewItem> GetSelectedOtherPrivileges()
        {
            var result = this.lstVwOtherPrivileges.SelectedItems.OfType<RoleOtherPrivilegeViewItem>().ToList();

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