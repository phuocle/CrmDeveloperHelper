﻿using Nav.Common.VSPackages.CrmDeveloperHelper.Entities;
using Nav.Common.VSPackages.CrmDeveloperHelper.Helpers;
using Nav.Common.VSPackages.CrmDeveloperHelper.Interfaces;
using Nav.Common.VSPackages.CrmDeveloperHelper.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nav.Common.VSPackages.CrmDeveloperHelper.Controllers
{
    /// <summary>
    /// Основной контроллер
    /// </summary>
    public class MainController
    {
        private readonly IWriteToOutputAndPublishList _iWriteToOutputAndPublishList;

        private readonly PublishController _publishController;
        private readonly ExplorerController _explorerController;
        private readonly CheckController _checkController;
        private readonly FindsController _findsController;
        private readonly ExportXmlController _exportXmlController;
        private readonly PluginConfigurationController _pluginConfigurationController;
        private readonly SolutionController _solutionController;
        private readonly EntityMetadataController _entityMetadataController;
        private readonly ExportPluginConfigurationController _exportPluginConfigurationController;
        private readonly CheckPluginController _checkPluginController;
        private readonly PluginController _pluginController;
        private readonly CheckManagedEntitiesController _checkManagedEntitiesController;
        private readonly ReportController _reportController;
        private readonly SecurityController _securityController;
        private readonly WebResourceController _webResourceController;

        /// <summary>
        /// Конструктор контроллера
        /// </summary>
        /// <param name="outputWindow"></param>
        public MainController(IWriteToOutputAndPublishList outputWindow)
        {
            this._iWriteToOutputAndPublishList = outputWindow;

            this._publishController = new PublishController(outputWindow);
            this._explorerController = new ExplorerController(outputWindow);
            this._checkController = new CheckController(outputWindow);
            this._findsController = new FindsController(outputWindow);
            this._exportXmlController = new ExportXmlController(outputWindow);
            this._pluginConfigurationController = new PluginConfigurationController(outputWindow);
            this._solutionController = new SolutionController(outputWindow);
            this._entityMetadataController = new EntityMetadataController(outputWindow);
            this._exportPluginConfigurationController = new ExportPluginConfigurationController(outputWindow);
            this._checkPluginController = new CheckPluginController(outputWindow);
            this._pluginController = new PluginController(outputWindow);
            this._checkManagedEntitiesController = new CheckManagedEntitiesController(outputWindow);
            this._reportController = new ReportController(outputWindow);
            this._securityController = new SecurityController(outputWindow);
            this._webResourceController = new WebResourceController(outputWindow);
        }

        #region ExecuteInThread Methods

        private void ExecuteWithConnectionInThreadVoid<T1>(ConnectionData connectionData, Action<ConnectionData, T1> action, T1 arg1)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(connectionData, arg1);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(connectionData, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteWithConnectionInThreadVoid<T1, T2>(ConnectionData connectionData, Action<ConnectionData, T1, T2> action, T1 arg1, T2 arg2)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(connectionData, arg1, arg2);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(connectionData, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteWithConnectionInThread(ConnectionData connectionData, Func<ConnectionData, Task> action)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(connectionData);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(connectionData, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteWithConnectionInThread<T1>(ConnectionData connectionData, Func<ConnectionData, T1, Task> action, T1 arg1)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(connectionData, arg1);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(connectionData, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteWithConnectionInThread<T1, T2>(ConnectionData connectionData, Func<ConnectionData, T1, T2, Task> action, T1 arg1, T2 arg2)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(connectionData, arg1, arg2);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(connectionData, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteWithConnectionInThread<T1, T2, T3>(ConnectionData connectionData, Func<ConnectionData, T1, T2, T3, Task> action, T1 arg1, T2 arg2, T3 arg3)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(connectionData, arg1, arg2, arg3);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(connectionData, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteWithConnectionInThread<T1, T2, T3, T4>(ConnectionData connectionData, Func<ConnectionData, T1, T2, T3, T4, Task> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(connectionData, arg1, arg2, arg3, arg4);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(connectionData, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteWithConnectionInThread<T1, T2, T3, T4, T5>(ConnectionData connectionData, Func<ConnectionData, T1, T2, T3, T4, T5, Task> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(connectionData, arg1, arg2, arg3, arg4, arg5);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(connectionData, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteWithConnectionInThread<T1, T2, T3, T4, T5, T6>(ConnectionData connectionData, Func<ConnectionData, T1, T2, T3, T4, T5, T6, Task> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(connectionData, arg1, arg2, arg3, arg4, arg5, arg6);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(connectionData, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteInThreadVoid(Action action)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(null, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteInThreadVoid<T1>(Action<T1> action, T1 art1)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(art1);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(null, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteInThreadVoid<T1, T2>(Action<T1, T2> action, T1 art1, T2 arg2)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(art1, arg2);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(null, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteInThreadVoid<T1, T2, T3>(Action<T1, T2, T3> action, T1 art1, T2 arg2, T3 arg3)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(art1, arg2, arg3);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(null, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteInThreadVoid<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 art1, T2 arg2, T3 arg3, T4 arg4)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(art1, arg2, arg3, arg4);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(null, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteInThread<T1>(Func<T1, Task> action, T1 arg1)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(arg1);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(null, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteInThread<T1, T2>(Func<T1, T2, Task> action, T1 arg1, T2 arg2)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(arg1, arg2);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(null, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteInThread<T1, T2, T3>(Func<T1, T2, T3, Task> action, T1 arg1, T2 arg2, T3 arg3)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(arg1, arg2, arg3);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(null, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteInThread<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(arg1, arg2, arg3, arg4);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(null, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteInThread<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(arg1, arg2, arg3, arg4, arg5);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(null, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteInThread<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, Task> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(arg1, arg2, arg3, arg4, arg5, arg6);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(null, ex);
                }
            });

            thread.Start();
        }

        private void ExecuteInThread<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, Task> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
                }
                catch (Exception ex)
                {
                    DTEHelper.WriteExceptionToOutput(null, ex);
                }
            });

            thread.Start();
        }

        #endregion ExecuteInThread Methods

        #region Explorers

        public void StartOpenReportExplorer(ConnectionData connectionData, CommonConfiguration commonConfig, string selection)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningReportExplorer, commonConfig, selection);

        public void StartOpenWebResourceExplorer(ConnectionData connectionData, CommonConfiguration commonConfig, string selection)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningWebResourceExplorer, commonConfig, selection);

        public void StartOpenApplicationRibbonExplorer(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningApplicationRibbonExplorer, commonConfig);

        public void StartOpenPluginTypeExplorer(ConnectionData connectionData, CommonConfiguration commonConfig, string selection)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningPluginTypeExplorer, commonConfig, selection);

        public void StartOpenPluginAssemblyExplorer(ConnectionData connectionData, CommonConfiguration commonConfig, string selection)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningPluginAssemblyExplorer, commonConfig, selection);

        public void StartOpeningEntityMetadataExplorer(ConnectionData connectionData, CommonConfiguration commonConfig, string selection, EnvDTE.SelectedItem selectedItem)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningEntityMetadataExplorer, commonConfig, selection, selectedItem);

        public void StartOpeningEntityMetadataFileGenerationOptions()
            => ExecuteInThreadVoid(this._explorerController.ExecuteOpeningEntityMetadataFileGenerationOptions);

        public void StartOpeningJavaScriptFileGenerationOptions()
            => ExecuteInThreadVoid(this._explorerController.ExecuteOpeningJavaScriptFileGenerationOptions);

        public void StartOpeningGlobalOptionSetsMetadataFileGenerationOptions()
            => ExecuteInThreadVoid(this._explorerController.ExecuteOpeningGlobalOptionSetsMetadataFileGenerationOptions);

        public void StartOpeningFileGenerationOptions()
            => ExecuteInThreadVoid(this._explorerController.ExecuteOpeningFileGenerationOptions);

        public void StartOpeningFileGenerationConfiguration()
            => ExecuteInThreadVoid(this._explorerController.ExecuteOpeningFileGenerationConfiguration);

        public void StartExplorerEntityAttribute(ConnectionData connectionData, CommonConfiguration commonConfig, string selection)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningEntityAttributeExplorer, commonConfig, selection);

        public void StartExplorerEntityKey(ConnectionData connectionData, CommonConfiguration commonConfig, string selection)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningEntityKeyExplorer, commonConfig, selection);

        public void StartExplorerEntityRelationshipOneToMany(ConnectionData connectionData, CommonConfiguration commonConfig, string selection)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningEntityRelationshipOneToManyExplorer, commonConfig, selection);

        public void StartExplorerEntityRelationshipManyToMany(ConnectionData connectionData, CommonConfiguration commonConfig, string selection)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningEntityRelationshipManyToManyExplorer, commonConfig, selection);

        public void StartExplorerEntityPrivileges(ConnectionData connectionData, CommonConfiguration commonConfig, string selection)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningEntityPrivilegesExplorer, commonConfig, selection);

        public void StartExplorerOtherPrivileges(ConnectionData connectionData, CommonConfiguration commonConfig, string selection)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningOtherPrivilegesExplorer, commonConfig, selection);

        public void StartCreatingFileWithGlobalOptionSets(ConnectionData connectionData, CommonConfiguration commonConfig, string selection, EnvDTE.SelectedItem selectedItem)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteCreatingFileWithGlobalOptionSets, commonConfig, selection, selectedItem);

        public void StartExplorerSiteMapXml(ConnectionData connectionData, CommonConfiguration commonConfig, string filter)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningSiteMapExplorer, commonConfig, filter);

        public void StartExplorerOrganizationInformation(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningOrganizationExplorer, commonConfig);

        public void StartExplorerSystemSavedQueryXml(ConnectionData connectionData, CommonConfiguration commonConfig, string selection)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningSystemSavedQueryExplorer, commonConfig, selection);

        public void StartExplorerSystemSavedQueryVisualization(ConnectionData connectionData, CommonConfiguration commonConfig, string selection)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningSystemSavedQueryVisualizationExplorer, commonConfig, selection);

        public void StartExplorerSystemForm(ConnectionData connectionData, CommonConfiguration commonConfig, string entityName, string selection, EnvDTE.SelectedItem selectedItem)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningSystemFormExplorer, commonConfig, entityName, selection, selectedItem);

        public void StartExplorerCustomControl(ConnectionData connectionData, CommonConfiguration commonConfig, string selection)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningCustomControlExplorer, commonConfig, selection);

        public void StartExplorerWorkflow(ConnectionData connectionData, CommonConfiguration commonConfig, string selection)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningWorkflowExplorer, commonConfig, selection);

        public void StartOpenSolutionExplorerWindow(ConnectionData connectionData, CommonConfiguration commonConfig, EnvDTE.SelectedItem selectedItem)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningSolutionExlorerWindow, commonConfig, selectedItem);

        public void StartOpenImportJobExplorerWindow(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningImportJobExlorerWindow, commonConfig);

        public void StartShowingPluginTree(ConnectionData connectionData, CommonConfiguration commonConfig, string entityFilter, string pluginTypeFilter, string messageFilter)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteShowingPluginTree, commonConfig, entityFilter, pluginTypeFilter, messageFilter);

        public void StartShowingPluginStepsExplorer(ConnectionData connectionData, CommonConfiguration commonConfig, string entityFilter, string pluginTypeFilter, string messageFilter)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteShowingPluginStepsExplorer, commonConfig, entityFilter, pluginTypeFilter, messageFilter);

        public void StartShowingSdkMessageExplorer(ConnectionData connectionData, CommonConfiguration commonConfig, string messageFilter)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteShowingSdkMessageExplorer, commonConfig, messageFilter);

        public void StartShowingSdkMessageFilterExplorer(ConnectionData connectionData, CommonConfiguration commonConfig, string messageFilter)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteShowingSdkMessageFilterExplorer, commonConfig, messageFilter);

        public void StartShowingSdkMessageFilterTree(ConnectionData connectionData, CommonConfiguration commonConfig, string entityFilter, string messageFilter)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteShowingSdkMessageFilterTree, commonConfig, entityFilter, messageFilter);

        public void StartShowingSdkMessageRequestTree(ConnectionData connectionData, CommonConfiguration commonConfig, string entityFilter, string messageFilter, EnvDTE.SelectedItem selectedItem)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteShowingSdkMessageRequestTree, commonConfig, entityFilter, messageFilter, selectedItem);

        public void StartShowingSystemUserExplorer(ConnectionData connectionData, CommonConfiguration commonConfig, string filter)
            => ExecuteWithConnectionInThread(connectionData, this._securityController.ExecuteShowingSystemUserExplorer, commonConfig, filter);

        public void StartShowingTeamsExplorer(ConnectionData connectionData, CommonConfiguration commonConfig, string filter)
            => ExecuteWithConnectionInThread(connectionData, this._securityController.ExecuteShowingTeamsExplorer, commonConfig, filter);

        public void StartShowingSecurityRolesExplorer(ConnectionData connectionData, CommonConfiguration commonConfig, string filter)
            => ExecuteWithConnectionInThread(connectionData, this._securityController.ExecuteShowingSecurityRolesExplorer, commonConfig, filter);

        public void StartTraceReaderOpenWindow(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._explorerController.ExecuteOpeningTraceReader, commonConfig);

        public void StartOpenSolutionImageWindow(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThreadVoid(connectionData, this._explorerController.ExecuteOpeningSolutionImageWindow, commonConfig);

        public void StartOpenSolutionDifferenceImageWindow(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThreadVoid(connectionData, this._explorerController.ExecuteOpeningSolutionDifferenceImageWindow, commonConfig);

        public void StartOpenOrganizationDifferenceImageWindow(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThreadVoid(connectionData, this._explorerController.ExecuteOpeningOrganizationDifferenceImageWindow, commonConfig);

        public void StartOrganizationComparer(ConnectionConfiguration crmConfig, CommonConfiguration commonConfig)
            => ExecuteInThreadVoid(this._explorerController.ExecuteOrganizationComparer, crmConfig, commonConfig);

        public void OpenWebResourceOrganizationComparer(ConnectionData connectionData1, ConnectionData connectionData2, CommonConfiguration commonConfig, string filter)
            => ExecuteInThreadVoid(this._explorerController.OpenWebResourceOrganizationComparer, connectionData1, connectionData2, commonConfig, filter);

        public void OpenWorkflowOrganizationComparer(ConnectionData connectionData1, ConnectionData connectionData2, CommonConfiguration commonConfig, string filter)
            => ExecuteInThreadVoid(this._explorerController.OpenWorkflowOrganizationComparer, connectionData1, connectionData2, commonConfig, filter);

        public void OpenSystemFormOrganizationComparer(ConnectionData connectionData1, ConnectionData connectionData2, CommonConfiguration commonConfig, string filter)
            => ExecuteInThreadVoid(this._explorerController.OpenSystemFormOrganizationComparer, connectionData1, connectionData2, commonConfig, filter);

        public void OpenSiteMapOrganizationComparer(ConnectionData connectionData1, ConnectionData connectionData2, CommonConfiguration commonConfig, string filter)
            => ExecuteInThreadVoid(this._explorerController.OpenSiteMapOrganizationComparer, connectionData1, connectionData2, commonConfig, filter);

        public void OpenSavedQueryOrganizationComparer(ConnectionData connectionData1, ConnectionData connectionData2, CommonConfiguration commonConfig, string filter)
            => ExecuteInThreadVoid(this._explorerController.OpenSavedQueryOrganizationComparer, connectionData1, connectionData2, commonConfig, filter);

        public void OpenEntityMetadataOrganizationComparer(ConnectionData connectionData1, ConnectionData connectionData2, CommonConfiguration commonConfig, string filter)
            => ExecuteInThreadVoid(this._explorerController.OpenEntityMetadataOrganizationComparer, connectionData1, connectionData2, commonConfig, filter);

        public void OpenApplicationRibbonOrganizationComparer(ConnectionData connectionData1, ConnectionData connectionData2, CommonConfiguration commonConfig)
            => ExecuteInThreadVoid(this._explorerController.OpenApplicationRibbonOrganizationComparer, connectionData1, connectionData2, commonConfig);

        #endregion Explorers

        #region Xml Files

        #region SiteMap

        public void StartSiteMapDifference(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteDifferenceSiteMap, commonConfig, selectedFile);

        public void StartSiteMapDifference(ConnectionData connectionData, CommonConfiguration commonConfig, XDocument doc, string filePath)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteDifferenceSiteMap, commonConfig, doc, filePath);

        public void StartSiteMapUpdate(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteUpdateSiteMap, commonConfig, selectedFile);

        public void StartSiteMapUpdate(ConnectionData connectionData, CommonConfiguration commonConfig, XDocument doc, string filePath)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteUpdateSiteMap, commonConfig, doc, filePath);

        public void StartSiteMapOpenInWeb(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteOpenInWebSiteMap, commonConfig, selectedFile);

        public void StartSiteMapGetCurrent(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteGetSiteMapCurrentXml, commonConfig, selectedFile);

        #endregion SiteMap

        #region SystemForm

        public void StartSystemFormDifference(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteDifferenceSystemForm, commonConfig, selectedFile);

        public void StartSystemFormDifference(ConnectionData connectionData, CommonConfiguration commonConfig, XDocument doc, string filePath)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteDifferenceSystemForm, commonConfig, doc, filePath);

        public void StartSystemFormUpdate(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteUpdateSystemForm, commonConfig, selectedFile);

        public void StartSystemFormUpdate(ConnectionData connectionData, CommonConfiguration commonConfig, XDocument doc, string filePath)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteUpdateSystemForm, commonConfig, doc, filePath);

        public void StartSystemFormOpenInWeb(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteOpenInWebSystemForm, commonConfig, selectedFile);

        public void StartSystemFormGetCurrentFormXml(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteGetSystemFormCurrentXml, commonConfig, selectedFile);

        public void StartSystemFormGetCurrentAttribute(ConnectionData connectionData, CommonConfiguration commonConfig, Guid formId, ActionOnComponent actionOnComponent, string fieldName, string fieldTitle)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteGetSystemFormCurrentAttribute, commonConfig, formId, actionOnComponent, fieldName, fieldTitle);

        public void StartOpeningLinkedSystemForm(ConnectionData connectionData, CommonConfiguration commonConfig, ActionOnComponent actionOnComponent, string entityName, Guid formId, int formType)
             => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteOpeningLinkedSystemForm, commonConfig, actionOnComponent, entityName, formId, formType);

        public void StartLinkedSystemFormChangeInEntityEditor(ConnectionData connectionData, CommonConfiguration commonConfig, string entityName, Guid formId, int formType)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteChangingLinkedSystemFormInEntityEditor, commonConfig, entityName, formId, formType);

        public void StartSystemFormCopyToClipboardTabsAndSections(ConnectionData connectionData, CommonConfiguration commonConfig, JavaScriptObjectType jsObjectType, string entityName, Guid formId, int formType)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteCopingToClipboardSystemFormCurrentTabsAndSections, commonConfig, jsObjectType, entityName, formId, formType);

        #endregion SystemForm

        #region SavedQuery

        public void StartSavedQueryDifference(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteDifferenceSavedQuery, commonConfig, selectedFile);

        public void StartSavedQueryDifference(ConnectionData connectionData, CommonConfiguration commonConfig, XDocument doc, string filePath)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteDifferenceSavedQuery, commonConfig, doc, filePath);

        public void StartSavedQueryUpdate(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteUpdateSavedQuery, commonConfig, selectedFile);

        public void StartSavedQueryUpdate(ConnectionData connectionData, CommonConfiguration commonConfig, XDocument doc, string filePath)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteUpdateSavedQuery, commonConfig, doc, filePath);

        public void StartSavedQueryOpenInWeb(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteOpenInWebSavedQuery, commonConfig, selectedFile);

        public void StartSavedQueryGetCurrent(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteGetSavedQueryCurrentXml, commonConfig, selectedFile);

        #endregion SavedQuery

        #region Workflow

        public void StartWorkflowDifference(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteDifferenceWorkflow, commonConfig, selectedFile);

        public void StartWorkflowDifference(ConnectionData connectionData, CommonConfiguration commonConfig, XDocument doc, string filePath)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteDifferenceWorkflow, commonConfig, doc, filePath);

        public void StartWorkflowUpdate(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteUpdateWorkflow, commonConfig, selectedFile);

        public void StartWorkflowUpdate(ConnectionData connectionData, CommonConfiguration commonConfig, XDocument doc, string filePath)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteUpdateWorkflow, commonConfig, doc, filePath);

        public void StartWorkflowOpenInWeb(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteOpenInWebWorkflow, commonConfig, selectedFile);

        public void StartWorkflowGetCurrent(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteGetWorkflowCurrentXaml, commonConfig, selectedFile);

        #endregion Workflow

        #region PluginTypeCustomWorkflowActivityInfo

        public void StartPluginTypeCustomWorkflowActivityInfoDifference(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteDifferencePluginTypeCustomWorkflowActivityInfo, commonConfig, selectedFile);

        public void StartPluginTypeCustomWorkflowActivityInfoDifference(ConnectionData connectionData, CommonConfiguration commonConfig, XDocument doc, string filePath)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteDifferencePluginTypeCustomWorkflowActivityInfo, commonConfig, doc, filePath);

        public void StartPluginTypeCustomWorkflowActivityInfoGetCurrent(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteGetPluginTypeCurrentCustomWorkflowActivityInfo, commonConfig, selectedFile);

        #endregion PluginTypeCustomWorkflowActivityInfo

        #region WebResource DependencyXml

        public void StartWebResourceDependencyXmlDifference(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteDifferenceWebResourceDependencyXml, commonConfig, selectedFile);

        public void StartWebResourceDependencyXmlDifference(ConnectionData connectionData, CommonConfiguration commonConfig, XDocument doc, string filePath)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteDifferenceWebResourceDependencyXml, commonConfig, doc, filePath);

        public void StartWebResourceDependencyXmlUpdate(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteUpdateWebResourceDependencyXml, commonConfig, selectedFile);

        public void StartWebResourceDependencyXmlUpdate(ConnectionData connectionData, CommonConfiguration commonConfig, XDocument doc, string filePath)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteUpdateWebResourceDependencyXml, commonConfig, doc, filePath);

        public void StartWebResourceDependencyXmlOpenInWeb(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteOpenInWebWebResourceDependencyXml, commonConfig, selectedFile);

        public void StartWebResourceDependencyXmlGetCurrent(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteGetWebResourceCurrentDependencyXml, commonConfig, selectedFile);

        #endregion WebResource DependencyXml

        #endregion Xml Files

        #region RibbonDiff

        public void StartRibbonDiffXmlDifference(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteRibbonDiffXmlDifference, commonConfig, selectedFile);

        public void StartRibbonDiffXmlDifference(ConnectionData connectionData, CommonConfiguration commonConfig, XDocument doc, string filePath)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteRibbonDiffXmlDifference, commonConfig, doc, filePath);

        public void StartRibbonDiffXmlUpdate(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteRibbonDiffXmlUpdate, commonConfig, selectedFile);

        public void StartRibbonDiffXmlUpdate(ConnectionData connectionData, CommonConfiguration commonConfig, XDocument doc, string filePath)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteRibbonDiffXmlUpdate, commonConfig, doc, filePath);

        public void StartRibbonDiffXmlGetCurrent(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteRibbonDiffXmlGetCurrent, commonConfig, selectedFile);

        public void StartOpeningEntityMetadataInWeb(ConnectionData connectionData, CommonConfiguration commonConfig, string entityName, int? entityTypeCode)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteOpeningEntityMetadataInWeb, commonConfig, entityName, entityTypeCode);

        public void StartOpeningEntityListInWeb(ConnectionData connectionData, CommonConfiguration commonConfig, string entityName, int? entityTypeCode)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteOpeningEntityListInWeb, commonConfig, entityName, entityTypeCode);

        public void StartOpeningEntityInstanceByIdInWeb(ConnectionData connectionData, CommonConfiguration commonConfig, string entityName, int? entityTypeCode, Guid entityId)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteOpeningEntityInstanceByIdInWeb, commonConfig, entityName, entityTypeCode, entityId);

        public void StartOpeningEntityFetchXmlFile(ConnectionData connectionData, CommonConfiguration commonConfig, string entityName, int? entityTypeCode)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteOpeningEntityFetchXmlFile, commonConfig, entityName, entityTypeCode);

        #endregion RibbonDiff

        #region Ribbon

        public void StartRibbonDifference(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteRibbonDifference, commonConfig, selectedFile);

        public void StartRibbonDifference(ConnectionData connectionData, CommonConfiguration commonConfig, XDocument doc, string filePath)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteRibbonDifference, commonConfig, doc, filePath);

        public void StartEntityRibbonOpenInWeb(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteEntityRibbonOpenInWeb, commonConfig, selectedFile);

        public void StartRibbonExplorer(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteOpenRibbonExplorer, commonConfig, selectedFile);

        public void StartRibbonGetCurrent(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteRibbonGetCurrent, commonConfig, selectedFile);

        #endregion Ribbon

        public void StartRibbonAndRibbonDiffXmlGetCurrent(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteRibbonAndRibbonDiffXmlGetCurrent, commonConfig, selectedFile);

        public void StartEntityMetadataOpenInWeb(ConnectionData connectionData, CommonConfiguration commonConfig, string entityName, ActionOnComponent actionOnComponent)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteEntityMetadataOpenInWeb, commonConfig, entityName, actionOnComponent);

        public void StartPublishEntityMetadata(ConnectionData connectionData, CommonConfiguration commonConfig, string entityName, int? entityTypeCode)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecutePublishEntity, commonConfig, entityName, entityTypeCode);

        public void StartGlobalOptionSetMetadataOpenInWeb(ConnectionData connectionData, CommonConfiguration commonConfig, string optionSetName, ActionOnComponent actionOnComponent)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteGlobalOptionSetMetadataOpenInWeb, commonConfig, optionSetName, actionOnComponent);

        public void StartPublishGlobalOptionSetMetadata(ConnectionData connectionData, CommonConfiguration commonConfig, string optionSetName)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecutePublishGlobalOptionSet, commonConfig, optionSetName);

        #region WebResource

        public void StartUpdateContentAndPublish(ConnectionData connectionData, List<SelectedFile> selectedFiles)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteUpdateContentAndPublishAsync, selectedFiles);

        public void StartUpdateContentAndPublishEqualByText(ConnectionData connectionData, List<SelectedFile> selectedFiles)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteUpdateContentAndPublishEqualByTextAsync, selectedFiles);

        public void StartIncludeReferencesToDependencyXml(ConnectionData connectionData, CommonConfiguration commonConfig, List<SelectedFile> selectedFiles)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteIncludeReferencesToDependencyXmlAsync, commonConfig, selectedFiles);

        public void StartUpdateContentIncludeReferencesToDependencyXml(ConnectionData connectionData, CommonConfiguration commonConfig, List<SelectedFile> selectedFiles)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteUpdateContentIncludeReferencesToDependencyXmlAsync, commonConfig, selectedFiles);

        public void StartUpdateEqualByTextContentContentIncludeReferencesToDependencyXml(ConnectionData connectionData, CommonConfiguration commonConfig, List<SelectedFile> selectedFiles)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteUpdateEqualByTextContentIncludeReferencesToDependencyXmlAsync, commonConfig, selectedFiles);

        public void StartIncludeReferencesToLinkedSystemFormsLibraries(ConnectionData connectionData, CommonConfiguration commonConfig, List<SelectedFile> selectedFiles)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteIncludeReferencesToLinkedSystemFormsLibrariesAsync, commonConfig, selectedFiles);

        public void StartWebResourceComparing(ConnectionData connectionData, List<SelectedFile> selectedFiles, bool withDetails)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteComparingFilesAndWebResourcesAsync, selectedFiles, withDetails);

        public void StartComparingFilesWithWrongEncoding(ConnectionData connectionData, List<SelectedFile> selectedFiles, bool withDetails)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteComparingFilesWithWrongEncodingAsync, selectedFiles, withDetails);

        public void ShowingWebResourcesDependentComponents(ConnectionData connectionData, CommonConfiguration commonConfig, List<SelectedFile> selectedFiles)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteShowingWebResourcesDependentComponentsAsync, commonConfig, selectedFiles);

        public void StartWebResourceDifference(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile, bool withSelect)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteDifferenceWebResourcesAsync, commonConfig, selectedFile, withSelect);

        public void StartWebResourceDifferenceReferencesAndDependencyXml(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteDifferenceReferencesAndDependencyXmlAsync, commonConfig, selectedFile);

        public void StartWebResourceCreateEntityDescription(ConnectionData connectionData, CommonConfiguration commonConfig, IEnumerable<SelectedFile> selectedFiles)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteCreatingWebResourceEntityDescriptionAsync, commonConfig, selectedFiles);

        public void StartWebResourceGetAttribute(ConnectionData connectionData, CommonConfiguration commonConfig, IEnumerable<SelectedFile> selectedFiles, string fieldName, string fieldTitle)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteWebResourceGettingAttributeAsync, commonConfig, selectedFiles, fieldName, fieldTitle);

        public void StartWebResourceChangeInEntityEditor(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteChangingWebResourceInEntityEditorAsync, commonConfig, selectedFile);

        public void StartWebResourceThreeFileDifference(ConnectionData connectionData1, ConnectionData connectionData2, CommonConfiguration commonConfig, SelectedFile selectedFile, ShowDifferenceThreeFileType differenceType)
            => ExecuteInThread(this._webResourceController.ExecuteThreeFileDifferenceWebResourcesAsync, connectionData1, connectionData2, commonConfig, selectedFile, differenceType);

        public void StartWebResourceMultiDifferenceFiles(ConnectionData connectionData, CommonConfiguration commonConfig, IEnumerable<SelectedFile> selectedFiles, OpenFilesType openFilesType)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteWebResourceMultiDifferenceFilesAsync, commonConfig, selectedFiles, openFilesType);

        public void StartWebResourceCreatingLastLinkMultiple(ConnectionData connectionData, List<SelectedFile> selectedFiles)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteCreatingLastLinkWebResourceMultipleAsync, selectedFiles);

        public void StartClearingLastLink(ConnectionData connectionData, List<SelectedFile> selectedFiles)
            => ExecuteWithConnectionInThreadVoid(connectionData, this._webResourceController.ExecuteClearingWebResourcesLinks, selectedFiles);

        public void StartWebResourcesGetContent(ConnectionData connectionData, CommonConfiguration commonConfig, List<SelectedFile> selectedFiles)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteGettingContentAsync, commonConfig, selectedFiles);

        public void StartOpeningWebResourceInExplorer(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile, ActionOnComponent actionOnComponent)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteOpeningWebResourceInExplorerAsync, commonConfig, selectedFile, actionOnComponent);

        public void StartOpeningWebResourceInWeb(ConnectionData connectionData, CommonConfiguration commonConfig, ActionOnComponent actionOnComponent, IEnumerable<SelectedFile> selectedFiles)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteOpeningWebResourceInWebAsync, commonConfig, actionOnComponent, selectedFiles);

        public void StartAddingIntoPublishListFilesByType(ConnectionData connectionData, CommonConfiguration commonConfig, IEnumerable<SelectedFile> selectedFiles, OpenFilesType openFilesType)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteAddingIntoPublishListFilesByTypeAsync, commonConfig, selectedFiles, openFilesType);

        public void StartRemovingFromPublishListFilesByType(ConnectionData connectionData, CommonConfiguration commonConfig, IEnumerable<SelectedFile> selectedFiles, OpenFilesType openFilesType)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteRemovingIntoPublishListFilesByTypeAsync, commonConfig, selectedFiles, openFilesType);

        public void StartWebResourceCheckFileEncoding(List<SelectedFile> selectedFiles)
            => ExecuteInThreadVoid(this._webResourceController.ExecuteCheckingFilesEncoding, selectedFiles);

        public void StartWebResourceOpenFilesWithouUTF8Encoding(List<SelectedFile> selectedFiles)
            => ExecuteInThreadVoid(this._webResourceController.ExecuteOpenFilesWithoutUTF8Encoding, selectedFiles);

        public void StartWebResourceOpeningFiles(ConnectionData connectionData, CommonConfiguration commonConfig, List<SelectedFile> selectedFiles, OpenFilesType openFilesType, bool isTextEditor)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteOpenFilesAsync, commonConfig, selectedFiles, openFilesType, isTextEditor);

        public void StartWebResourceCopyToClipboardRibbonObjects(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile, RibbonPlacement ribbonPlacement)
            => ExecuteWithConnectionInThread(connectionData, this._webResourceController.ExecuteCopyToClipboardRibbonObjectsAsync, commonConfig, selectedFile, ribbonPlacement);

        #endregion WebResource

        #region JavaScript Files

        public void StartJavaScriptEntityMetadataFileUpdatingSchema(ConnectionData connectionData, CommonConfiguration commonConfig, List<SelectedFile> selectedFiles, bool selectEntity, bool openOptions)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteUpdateFileWithEntityMetadataJavaScript, commonConfig, selectedFiles, selectEntity, openOptions);

        public void StartJavaScriptGlobalOptionSetFileUpdatingSingle(ConnectionData connectionData, CommonConfiguration commonConfig, List<SelectedFile> selectedFiles, bool selectEntity, bool openOptions)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteUpdatingFileWithGlobalOptionSetSingleJavaScript, commonConfig, selectedFiles, selectEntity, openOptions);

        public void StartJavaScriptGlobalOptionSetFileUpdatingAll(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile, bool openOptions)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteUpdatingFileWithGlobalOptionSetAllJavaScript, commonConfig, selectedFile, openOptions);

        #endregion JavaScript Files

        #region Report

        /// <summary>
        /// Старт различия отчетов
        /// </summary>
        /// <param name="selectedFile"></param>
        /// <param name="withSelect"></param>
        /// <param name="crmConfig"></param>
        /// <param name="commonConfig"></param>
        public void StartReportDifference(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile, string fieldName, string fieldTitle, bool withSelect)
            => ExecuteWithConnectionInThread(connectionData, this._reportController.ExecuteDifferenceReport, commonConfig, selectedFile, fieldName, fieldTitle, withSelect);

        public void StartReportThreeFileDifference(ConnectionData connectionData1, ConnectionData connectionData2, CommonConfiguration commonConfig, SelectedFile selectedFile, string fieldName, string fieldTitle, ShowDifferenceThreeFileType differenceType)
            => ExecuteInThread(this._reportController.ExecuteThreeFileDifferenceReport, connectionData1, connectionData2, commonConfig, selectedFile, fieldName, fieldTitle, differenceType);

        public void StartReportUpdate(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._reportController.ExecuteUpdatingReport, commonConfig, selectedFile);

        public void StartReportCreatingLastLink(ConnectionData connectionData, SelectedFile selectedFile)
            => ExecuteWithConnectionInThread(connectionData, this._reportController.ExecuteCreatingLastLinkReport, selectedFile);

        public void StartOpeningReport(ConnectionData connectionData, CommonConfiguration commonConfig, SelectedFile selectedFile, ActionOnComponent actionOnComponent)
            => ExecuteWithConnectionInThread(connectionData, this._reportController.ExecuteOpeningReport, commonConfig, selectedFile, actionOnComponent);

        #endregion Report

        #region Solutions

        public void StartAddingEntityToSolution(ConnectionData connectionData, CommonConfiguration commonConfig, string solutionUniqueName, bool withSelect, string entityName, SolutionComponent.Schema.OptionSets.rootcomponentbehavior rootComponentBehavior)
            => ExecuteWithConnectionInThread(connectionData, this._solutionController.ExecuteAddingEntityToSolution, commonConfig, solutionUniqueName, withSelect, entityName, rootComponentBehavior);

        public void StartAddingGlobalOptionSetToSolution(ConnectionData connectionData, CommonConfiguration commonConfig, string solutionUniqueName, bool withSelect, IEnumerable<string> optionSetNames)
            => ExecuteWithConnectionInThread(connectionData, this._solutionController.ExecuteAddingGlobalOptionSetToSolution, commonConfig, solutionUniqueName, withSelect, optionSetNames);

        public void StartAddingWebResourcesToSolution(ConnectionData connectionData, CommonConfiguration commonConfig, string solutionUniqueName, IEnumerable<SelectedFile> selectedFiles, bool withSelect)
            => ExecuteWithConnectionInThread(connectionData, this._solutionController.ExecuteAddingWebResourcesToSolution, commonConfig, solutionUniqueName, selectedFiles, withSelect);

        public void StartAddingLinkedSystemFormToSolution(ConnectionData connectionData, CommonConfiguration commonConfig, string solutionUniqueName, bool withSelect, IEnumerable<Guid> formIdList)
            => ExecuteWithConnectionInThread(connectionData, this._solutionController.ExecuteAddingLinkedSystemFormToSolution, commonConfig, solutionUniqueName, withSelect, formIdList);

        public void StartReportAddingToSolution(ConnectionData connectionData, CommonConfiguration commonConfig, string solutionUniqueName, IEnumerable<SelectedFile> selectedFiles, bool withSelect)
            => ExecuteWithConnectionInThread(connectionData, this._solutionController.ExecuteAddingReportsToSolution, commonConfig, solutionUniqueName, selectedFiles, withSelect);

        public void StartPluginAssemblyAddingToSolution(ConnectionData connectionData, CommonConfiguration commonConfig, IEnumerable<string> projectNames, string solutionUniqueName, bool withSelect)
            => ExecuteWithConnectionInThread(connectionData, this._solutionController.ExecuteAddingPluginAssemblyToSolution, commonConfig, projectNames, solutionUniqueName, withSelect);

        public void StartPluginAssemblyAddingProcessingStepsToSolution(ConnectionData connectionData, CommonConfiguration commonConfig, IEnumerable<string> projectNames, string solutionUniqueName, bool withSelect)
            => ExecuteWithConnectionInThread(connectionData, this._solutionController.ExecuteAddingPluginAssemblyProcessingStepsToSolution, commonConfig, projectNames, solutionUniqueName, withSelect);

        public void StartPluginTypeAddingProcessingStepsToSolution(ConnectionData connectionData, CommonConfiguration commonConfig, IEnumerable<string> pluginTypeNames, string solutionUniqueName, bool withSelect)
            => ExecuteWithConnectionInThread(connectionData, this._solutionController.ExecuteAddingPluginTypeProcessingStepsToSolution, commonConfig, pluginTypeNames, solutionUniqueName, withSelect);

        public void StartSolutionOpening(ConnectionData connectionData, CommonConfiguration commonConfig, string solutionUniqueName, ActionOnComponent actionOnComponent)
            => ExecuteWithConnectionInThread(connectionData, this._solutionController.ExecuteOpeningSolutionAsync, commonConfig, solutionUniqueName, actionOnComponent);

        public void StartSolutionOpeningWebResources(ConnectionData connectionData, CommonConfiguration commonConfig, string solutionUniqueName, bool inTextEditor, OpenFilesType openFilesType)
            => ExecuteWithConnectionInThread(connectionData, this._solutionController.ExecuteOpeningSolutionWebResourcesAsync, commonConfig, solutionUniqueName, inTextEditor, openFilesType);

        #endregion Solutions

        #region VisualStudio Projects, Plugin Assemblies, Types, Steps

        public void StartAddPluginStep(ConnectionData connectionData, CommonConfiguration commonConfig, string pluginTypeName)
            => ExecuteWithConnectionInThread(connectionData, this._pluginController.ExecuteAddingPluginStepForType, commonConfig, pluginTypeName);

        public void StartPluginAssemblyComparingPluginTypesWithLocalAssembly(ConnectionData connectionData, CommonConfiguration commonConfig, IEnumerable<EnvDTE.Project> projectList)
            => ExecuteWithConnectionInThread(connectionData, this._pluginController.ExecuteComparingPluginTypesLocalAssemblyAndPluginAssembly, commonConfig, projectList);

        public void StartComparingByteArrayLocalAssemblyAndPluginAssembly(ConnectionData connectionData, CommonConfiguration commonConfig, IEnumerable<EnvDTE.Project> projectList)
            => ExecuteWithConnectionInThread(connectionData, this._pluginController.ExecuteComparingByteArrayLocalAssemblyAndPluginAssembly, commonConfig, projectList);

        public void StartPluginAssemblyRegister(ConnectionData connectionData, CommonConfiguration commonConfig, IEnumerable<EnvDTE.Project> projectList)
            => ExecuteWithConnectionInThread(connectionData, this._pluginController.ExecuteRegisterPluginAssembly, commonConfig, projectList);

        public void StartPluginAssemblyUpdatingInWindow(ConnectionData connectionData, CommonConfiguration commonConfig, IEnumerable<EnvDTE.Project> projectList)
            => ExecuteWithConnectionInThread(connectionData, this._pluginController.ExecuteUpdatingPluginAssembliesInWindow, commonConfig, projectList);

        public void StartPluginAssemblyBuildProjectUpdate(ConnectionData connectionData, CommonConfiguration commonConfig, IEnumerable<EnvDTE.Project> projectList, bool registerPlugins)
            => ExecuteWithConnectionInThread(connectionData, this._pluginController.ExecuteBuildingProjectAndUpdatingPluginAssembly, commonConfig, projectList, registerPlugins);

        public void StartActionOnPluginAssembly(ConnectionData connectionData, CommonConfiguration commonConfig, IEnumerable<EnvDTE.Project> projectList, ActionOnComponent actionOnComponent)
            => ExecuteWithConnectionInThread(connectionData, this._pluginController.ExecuteActionOnProjectPluginAssembly, commonConfig, projectList, actionOnComponent);

        public void StartActionOnPluginTypes(ConnectionData connectionData, CommonConfiguration commonConfig, IEnumerable<string> pluginTypeNames, ActionOnComponent actionOnComponent, string fieldName, string fieldTitle)
            => ExecuteWithConnectionInThread(connectionData, this._pluginController.ExecuteActionOnPluginTypes, commonConfig, pluginTypeNames, actionOnComponent, fieldName, fieldTitle);

        #endregion VisualStudio Projects, Plugin Assemblies, Types, Steps

        #region PluginConfiguration

        public void StartExportPluginConfiguration(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._exportPluginConfigurationController.ExecuteExportingPluginConfigurationXml, commonConfig);

        public void StartExportPluginConfigurationIntoFolder(ConnectionData connectionData, CommonConfiguration commonConfig, EnvDTE.SelectedItem selectedItem)
            => ExecuteWithConnectionInThread(connectionData, this._exportPluginConfigurationController.ExecuteExportingPluginConfigurationIntoFolder, commonConfig, selectedItem);

        public void StartShowingPluginConfigurationTree(ConnectionData connectionData, CommonConfiguration commonConfig, string filePath)
            => ExecuteWithConnectionInThreadVoid(connectionData, this._pluginConfigurationController.ExecuteShowingPluginConfigurationTree, commonConfig, filePath);

        public void StartShowingPluginConfigurationAssemblyDescriptionWindow(CommonConfiguration commonConfig, string filePath)
            => ExecuteInThreadVoid(this._pluginConfigurationController.ExecuteShowingPluginConfigurationAssemblyDescriptionWindow, commonConfig, filePath);

        public void StartShowingPluginConfigurationTypeDescriptionWindow(CommonConfiguration commonConfig, string filePath)
            => ExecuteInThreadVoid(this._pluginConfigurationController.ExecuteShowingPluginConfigurationTypeDescriptionWindow, commonConfig, filePath);

        public void StartShowingPluginConfigurationComparer(CommonConfiguration commonConfig, string filePath)
            => ExecuteInThreadVoid(this._pluginConfigurationController.ExecuteShowingPluginConfigurationComparer, commonConfig, filePath);

        #endregion PluginConfiguration

        #region C# Files

        public void StartCSharpEntityMetadataUpdatingFileWithSchema(ConnectionData connectionData, CommonConfiguration commonConfig, List<SelectedFile> selectedFiles, bool selectEntity, bool openOptions)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteUpdateFileWithEntityMetadataCSharpSchema, commonConfig, selectedFiles, selectEntity, openOptions);

        public void StartCSharpEntityMetadataUpdatingFileWithProxyClassOrSchema(ConnectionData connectionData, CommonConfiguration commonConfig, List<SelectedFile> selectedFiles, bool selectEntity, bool openOptions)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteUpdateFileWithEntityMetadataCSharpProxyClassOrSchema, commonConfig, selectedFiles, selectEntity, openOptions);

        public void StartCSharpEntityMetadataUpdatingFileWithProxyClass(ConnectionData connectionData, CommonConfiguration commonConfig, List<SelectedFile> selectedFiles, bool selectEntity, bool openOptions)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteUpdateFileWithEntityMetadataCSharpProxyClass, commonConfig, selectedFiles, selectEntity, openOptions);

        public void StartCSharpGlobalOptionSetsFileUpdatingSchema(ConnectionData connectionData, CommonConfiguration commonConfig, IEnumerable<SelectedFile> selectedFiles, bool withSelect, bool openOptions)
            => ExecuteWithConnectionInThread(connectionData, this._entityMetadataController.ExecuteUpdatingFileWithGlobalOptionSetCSharp, commonConfig, selectedFiles, withSelect, openOptions);

        #endregion C# Files

        #region Finds

        /// <summary>
        /// Проверка CRM на существование сущностей с префиксом new_.
        /// </summary>
        public void StartFindEntityObjectsByPrefix(ConnectionData connectionData, CommonConfiguration commonConfig, string prefix)
            => ExecuteWithConnectionInThread(connectionData, this._findsController.ExecuteFindingEntityObjectsByPrefix, commonConfig, prefix);

        /// <summary>
        /// Проверка CRM на существование сущностей с префиксом new_.
        /// </summary>
        public void StartFindEntityObjectsByPrefixInExplorer(ConnectionData connectionData, CommonConfiguration commonConfig, string prefix)
            => ExecuteWithConnectionInThread(connectionData, this._findsController.ExecuteFindingEntityObjectsByPrefixInExplorer, commonConfig, prefix);

        public void StartFindEntityObjectsByPrefixAndShowDependentComponents(ConnectionData connectionData, CommonConfiguration commonConfig, string prefix)
            => ExecuteWithConnectionInThread(connectionData, this._findsController.ExecuteFindingEntityObjectsByPrefixAndShowDependentComponents, commonConfig, prefix);

        public void StartFindMarkedToDeleteAndShowDependentComponents(ConnectionData connectionData, CommonConfiguration commonConfig, string prefix)
            => ExecuteWithConnectionInThread(connectionData, this._findsController.ExecuteFindingMarkedToDeleteAndShowDependentComponents, commonConfig, prefix);

        public void StartFindMarkedToDeleteInExplorer(ConnectionData connectionData, CommonConfiguration commonConfig, string prefix)
            => ExecuteWithConnectionInThread(connectionData, this._findsController.ExecuteFindingMarkedToDeleteInExplorer, commonConfig, prefix);

        public void StartFindEntityObjectsByName(ConnectionData connectionData, CommonConfiguration commonConfig, string name)
            => ExecuteWithConnectionInThread(connectionData, this._findsController.ExecuteFindEntityElementsByName, commonConfig, name);

        public void StartFindEntityObjectsByNameInExplorer(ConnectionData connectionData, CommonConfiguration commonConfig, string name)
            => ExecuteWithConnectionInThread(connectionData, this._findsController.ExecuteFindEntityElementsByNameInExplorer, commonConfig, name);

        public void StartFindEntityObjectsContainsString(ConnectionData connectionData, CommonConfiguration commonConfig, string name)
            => ExecuteWithConnectionInThread(connectionData, this._findsController.ExecuteFindEntityElementsContainsString, commonConfig, name);

        public void StartFindEntityObjectsContainsStringInExplorer(ConnectionData connectionData, CommonConfiguration commonConfig, string name)
            => ExecuteWithConnectionInThread(connectionData, this._findsController.ExecuteFindEntityElementsContainsStringInExplorer, commonConfig, name);

        public void StartFindEntityById(ConnectionData connectionData, CommonConfiguration commonConfig, string entityName, int? entityTypeCode, Guid entityId)
            => ExecuteWithConnectionInThread(connectionData, this._findsController.ExecuteFindEntityById, commonConfig, entityName, entityTypeCode, entityId);

        public void StartEditEntityById(ConnectionData connectionData, CommonConfiguration commonConfig, string entityName, int? entityTypeCode, Guid entityId)
            => ExecuteWithConnectionInThread(connectionData, this._findsController.ExecuteEditEntityById, commonConfig, entityName, entityTypeCode, entityId);

        public void StartFindEntityByUniqueidentifier(ConnectionData connectionData, CommonConfiguration commonConfig, string entityName, int? entityTypeCode, Guid entityId)
            => ExecuteWithConnectionInThread(connectionData, this._findsController.ExecuteFindEntityByUniqueidentifier, commonConfig, entityName, entityTypeCode, entityId);

        #endregion Finds

        #region Checks

        public void StartCheckPluginImages(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._checkPluginController.ExecuteCheckingPluginImages, commonConfig);

        public void StartCheckGlobalOptionSetDuplicates(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._checkController.ExecuteCheckingGlobalOptionSetDuplicates, commonConfig);

        public void StartCheckComponentTypeEnum(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._checkController.ExecuteCheckingComponentTypeEnum, commonConfig);

        public void StartCheckUnknownFormControlTypes(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._checkController.ExecuteCheckingUnknownFormControlType, commonConfig);

        public void StartCreateMissingTeamTemplatesInSystemForms(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._checkController.ExecuteCreatingMissingTeamTemplatesInSystemForms, commonConfig);

        public void StartCheckTeamTemplates(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._checkController.ExecuteCheckingTeamTemplates, commonConfig);

        public void StartCreateAllDependencyNodesDescription(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._checkController.ExecuteCreatingAllDependencyNodesDescription, commonConfig);

        public void StartCheckPluginImagesRequiredComponents(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._checkPluginController.ExecuteCheckingPluginImagesRequiredComponents, commonConfig);

        public void StartCheckPluginStepsRequiredComponents(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._checkPluginController.ExecuteCheckingPluginStepsRequiredComponents, commonConfig);

        public void StartCheckPluginSteps(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._checkPluginController.ExecuteCheckingPluginSteps, commonConfig);

        public void StartCheckEntitiesOwnership(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._checkController.ExecuteCheckingEntitiesOwnership, commonConfig);

        public void StartCheckingWorkflowsUsedEntities(ConnectionData connectionData, CommonConfiguration commonConfig, bool openExplorer)
            => ExecuteWithConnectionInThread(connectionData, this._checkController.ExecuteCheckingWorkflowsUsedEntities, commonConfig, openExplorer);

        public void ExecuteCheckingWorkflowsNotExistingUsedEntities(ConnectionData connectionData, CommonConfiguration commonConfig, bool openExplorer)
            => ExecuteWithConnectionInThread(connectionData, this._checkController.ExecuteCheckingWorkflowsNotExistingUsedEntities, commonConfig, openExplorer);

        public void ExecuteCheckingWorkflowsWithEntityFieldStrings(ConnectionData connectionData, CommonConfiguration commonConfig, bool openExplorer)
            => ExecuteWithConnectionInThread(connectionData, this._checkController.ExecuteCheckingWorkflowsWithEntityFieldStrings, commonConfig, openExplorer);

        public void StartCheckManagedEntities(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._checkManagedEntitiesController.ExecuteCheckingManagedEntities, commonConfig);

        #endregion Checks

        public void StartExportingFormEvents(ConnectionData connectionData, CommonConfiguration commonConfig)
            => ExecuteWithConnectionInThread(connectionData, this._exportXmlController.ExecuteExportingFormsEvents, commonConfig);

        public void StartPublishAll(ConnectionData connectionData)
            => ExecuteWithConnectionInThread(connectionData, this._publishController.ExecutePublishingAll);
    }
}
