﻿using Microsoft.VisualStudio.Shell;
using Nav.Common.VSPackages.CrmDeveloperHelper.Helpers;
using Nav.Common.VSPackages.CrmDeveloperHelper.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;

namespace Nav.Common.VSPackages.CrmDeveloperHelper.Commands
{
    internal abstract class AbstractOutputWindowDynamicCommand<T> : AbstractCommand
    {
        protected readonly int _baseIdStart;

        protected AbstractOutputWindowDynamicCommand(
            OleMenuCommandService commandService
            , Guid commandGroupId
            , int baseIdStart
            , int commandsCount
        )
        {
            this._baseIdStart = baseIdStart;

            for (int i = 0; i < commandsCount; i++)
            {
                var menuCommandID = new CommandID(commandGroupId, _baseIdStart + i);

                var menuCommand = new OleMenuCommand(this.menuItemCallback, menuCommandID);

                menuCommand.Enabled = menuCommand.Visible = false;

                menuCommand.BeforeQueryStatus += menuItem_BeforeQueryStatus;

                commandService.AddCommand(menuCommand);
            }
        }

        private void menuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (sender is OleMenuCommand menuCommand)
                {
                    menuCommand.Enabled = menuCommand.Visible = false;

                    var applicationObject = CrmDeveloperHelperPackage.Singleton?.ApplicationObject;
                    if (applicationObject == null)
                    {
                        return;
                    }

                    var helper = DTEHelper.Create(applicationObject);

                    var connectionData = helper.GetOutputWindowConnection();

                    if (connectionData == null)
                    {
                        return;
                    }

                    var index = menuCommand.CommandID.ID - _baseIdStart;

                    var elementsList = GetElementSourceCollection(connectionData);

                    if (0 <= index && index < elementsList.Count)
                    {
                        var element = elementsList.ElementAt(index);

                        menuCommand.Text = GetElementName(connectionData, element);

                        menuCommand.Enabled = menuCommand.Visible = true;

                        CommandBeforeQueryStatus(applicationObject, connectionData, element, menuCommand);
                    }
                }
            }
            catch (Exception ex)
            {
                DTEHelper.WriteExceptionToOutput(null, ex);
            }
        }

        private void menuItemCallback(object sender, EventArgs e)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                OleMenuCommand menuCommand = sender as OleMenuCommand;
                if (menuCommand == null)
                {
                    return;
                }

                var applicationObject = CrmDeveloperHelperPackage.Singleton?.ApplicationObject;
                if (applicationObject == null)
                {
                    return;
                }

                var helper = DTEHelper.Create(applicationObject);

                var connectionData = helper.GetOutputWindowConnection();

                if (connectionData == null)
                {
                    return;
                }

                var index = menuCommand.CommandID.ID - _baseIdStart;

                var elementsList = GetElementSourceCollection(connectionData);

                if (0 <= index && index < elementsList.Count)
                {
                    var element = elementsList.ElementAt(index);

                    CommandAction(helper, connectionData, element);
                }
            }
            catch (Exception ex)
            {
                DTEHelper.WriteExceptionToOutput(null, ex);
            }
        }

        protected abstract ICollection<T> GetElementSourceCollection(ConnectionData connectionData);

        protected abstract string GetElementName(ConnectionData connectionData, T element);

        protected abstract void CommandAction(DTEHelper helper, ConnectionData connectionData, T element);

        protected virtual void CommandBeforeQueryStatus(EnvDTE80.DTE2 applicationObject, ConnectionData connectionData, T element, OleMenuCommand menuCommand)
        {

        }
    }
}
