﻿using Microsoft.VisualStudio.Shell;
using Nav.Common.VSPackages.CrmDeveloperHelper.Helpers;
using Nav.Common.VSPackages.CrmDeveloperHelper.Interfaces;
using Nav.Common.VSPackages.CrmDeveloperHelper.Model;
using Nav.Common.VSPackages.CrmDeveloperHelper.Model.Sources;
using System.Collections.Generic;
using System.Linq;

namespace Nav.Common.VSPackages.CrmDeveloperHelper.Commands.WebResources
{
    internal class WebResourceOpenFilesByTypeCommand : AbstractDynamicCommandByOpenFilesType
    {
        private readonly bool _inTextEditor;
        private readonly ISourceSelectedFiles _sourceSelectedFiles;

        protected WebResourceOpenFilesByTypeCommand(
            OleMenuCommandService commandService
            , int baseIdStart
            , ISourceSelectedFiles sourceSelectedFiles
            , IList<OpenFilesType> sourceOpenTypes
            , bool inTextEditor
        ) : base(commandService, baseIdStart, sourceOpenTypes)
        {
            this._sourceSelectedFiles = sourceSelectedFiles;
            this._inTextEditor = inTextEditor;
        }

        public static WebResourceOpenFilesByTypeCommand InstanceFileOrdinal { get; private set; }

        public static WebResourceOpenFilesByTypeCommand InstanceFileChanges { get; private set; }

        public static WebResourceOpenFilesByTypeCommand InstanceFileMirror { get; private set; }

        public static WebResourceOpenFilesByTypeCommand InstanceFolderOrdinal { get; private set; }

        public static WebResourceOpenFilesByTypeCommand InstanceFolderChanges { get; private set; }

        public static WebResourceOpenFilesByTypeCommand InstanceFolderMirror { get; private set; }

        public static WebResourceOpenFilesByTypeCommand InstanceInTextEditorDocumentsOrdinal { get; private set; }

        public static WebResourceOpenFilesByTypeCommand InstanceInTextEditorDocumentsChanges { get; private set; }

        public static WebResourceOpenFilesByTypeCommand InstanceInTextEditorDocumentsMirror { get; private set; }

        public static WebResourceOpenFilesByTypeCommand InstanceInTextEditorFileOrdinal { get; private set; }

        public static WebResourceOpenFilesByTypeCommand InstanceInTextEditorFileChanges { get; private set; }

        public static WebResourceOpenFilesByTypeCommand InstanceInTextEditorFileMirror { get; private set; }

        public static WebResourceOpenFilesByTypeCommand InstanceInTextEditorFolderOrdinal { get; private set; }

        public static WebResourceOpenFilesByTypeCommand InstanceInTextEditorFolderChanges { get; private set; }

        public static WebResourceOpenFilesByTypeCommand InstanceInTextEditorFolderMirror { get; private set; }

        public static void Initialize(OleMenuCommandService commandService)
        {
            var sourceDocuments = DocumentsSourceSelectedFiles.CreateSource();

            var sourceFile = FileSourceSelectedFiles.CreateSource();

            var sourceFolder = FolderSourceSelectedFiles.CreateSource();

            InstanceFileOrdinal = new WebResourceOpenFilesByTypeCommand(commandService, PackageIds.guidDynamicOpenFilesTypeCommandSet.FileWebResourceOpenFilesByTypeOrdinalCommandId, sourceFile, _typesOrdinal, false);
            InstanceFileChanges = new WebResourceOpenFilesByTypeCommand(commandService, PackageIds.guidDynamicOpenFilesTypeCommandSet.FileWebResourceOpenFilesByTypeWithChangesCommandId, sourceFile, _typesChanges, false);
            InstanceFileMirror = new WebResourceOpenFilesByTypeCommand(commandService, PackageIds.guidDynamicOpenFilesTypeCommandSet.FileWebResourceOpenFilesByTypeWithMirrorCommandId, sourceFile, _typesMirror, false);

            InstanceFolderOrdinal = new WebResourceOpenFilesByTypeCommand(commandService, PackageIds.guidDynamicOpenFilesTypeCommandSet.FolderWebResourceOpenFilesByTypeOrdinalCommandId, sourceFolder, _typesOrdinal, false);
            InstanceFolderChanges = new WebResourceOpenFilesByTypeCommand(commandService, PackageIds.guidDynamicOpenFilesTypeCommandSet.FolderWebResourceOpenFilesByTypeWithChangesCommandId, sourceFolder, _typesChanges, false);
            InstanceFolderMirror = new WebResourceOpenFilesByTypeCommand(commandService, PackageIds.guidDynamicOpenFilesTypeCommandSet.FolderWebResourceOpenFilesByTypeWithMirrorCommandId, sourceFolder, _typesMirror, false);





            InstanceInTextEditorDocumentsOrdinal = new WebResourceOpenFilesByTypeCommand(commandService, PackageIds.guidDynamicOpenFilesTypeCommandSet.DocumentsWebResourceOpenFilesByTypeInTextEditorOrdinalCommandId, sourceDocuments, _typesOrdinal, true);
            InstanceInTextEditorDocumentsChanges = new WebResourceOpenFilesByTypeCommand(commandService, PackageIds.guidDynamicOpenFilesTypeCommandSet.DocumentsWebResourceOpenFilesByTypeInTextEditorWithChangesCommandId, sourceDocuments, _typesChanges, true);
            InstanceInTextEditorDocumentsMirror = new WebResourceOpenFilesByTypeCommand(commandService, PackageIds.guidDynamicOpenFilesTypeCommandSet.DocumentsWebResourceOpenFilesByTypeInTextEditorWithMirrorCommandId, sourceDocuments, _typesMirror, true);

            InstanceInTextEditorFileOrdinal = new WebResourceOpenFilesByTypeCommand(commandService, PackageIds.guidDynamicOpenFilesTypeCommandSet.FileWebResourceOpenFilesByTypeInTextEditorOrdinalCommandId, sourceFile, _typesOrdinal, true);
            InstanceInTextEditorFileChanges = new WebResourceOpenFilesByTypeCommand(commandService, PackageIds.guidDynamicOpenFilesTypeCommandSet.FileWebResourceOpenFilesByTypeInTextEditorWithChangesCommandId, sourceFile, _typesChanges, true);
            InstanceInTextEditorFileMirror = new WebResourceOpenFilesByTypeCommand(commandService, PackageIds.guidDynamicOpenFilesTypeCommandSet.FileWebResourceOpenFilesByTypeInTextEditorWithMirrorCommandId, sourceFile, _typesMirror, true);

            InstanceInTextEditorFolderOrdinal = new WebResourceOpenFilesByTypeCommand(commandService, PackageIds.guidDynamicOpenFilesTypeCommandSet.FolderWebResourceOpenFilesByTypeInTextEditorOrdinalCommandId, sourceFolder, _typesOrdinal, true);
            InstanceInTextEditorFolderChanges = new WebResourceOpenFilesByTypeCommand(commandService, PackageIds.guidDynamicOpenFilesTypeCommandSet.FolderWebResourceOpenFilesByTypeInTextEditorWithChangesCommandId, sourceFolder, _typesChanges, true);
            InstanceInTextEditorFolderMirror = new WebResourceOpenFilesByTypeCommand(commandService, PackageIds.guidDynamicOpenFilesTypeCommandSet.FolderWebResourceOpenFilesByTypeInTextEditorWithMirrorCommandId, sourceFolder, _typesMirror, true);
        }

        protected override void CommandAction(DTEHelper helper, OpenFilesType openFilesType)
        {
            var selectedFiles = _sourceSelectedFiles.GetSelectedFiles(helper, SelectedFileType.WebResourceText).ToList();

            if (selectedFiles.Any())
            {
                helper.HandleWebResourceOpenFilesCommand(selectedFiles, openFilesType, _inTextEditor);
            }
        }

        protected override void CommandBeforeQueryStatus(EnvDTE80.DTE2 applicationObject, OpenFilesType openFilesType, OleMenuCommand menuCommand)
        {
            if (_inTextEditor)
            {
                CommonHandlers.ActionBeforeQueryStatusTextEditorProgramExists(applicationObject, menuCommand);
            }

            _sourceSelectedFiles.CommandBeforeQueryStatus(applicationObject, menuCommand, SelectedFileType.WebResourceText);
        }
    }
}