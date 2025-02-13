﻿using Nav.Common.VSPackages.CrmDeveloperHelper.Intellisense;
using System.IO;

namespace Nav.Common.VSPackages.CrmDeveloperHelper.Model
{
    /// <summary>
    /// Описывает выбранный для публикации файл 
    /// </summary>
    public class SelectedFile
    {
        /// <summary>
        /// Имя файла без расширения
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Расширение файла
        /// </summary>
        public string Extension { get; private set; }

        /// <summary>
        /// Имя файла
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Полный путь к файлу
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Относительный путь к файлу
        /// </summary>
        public string FriendlyFilePath { get; private set; }

        public string UrlFriendlyFilePath { get; private set; }

        public string SolutionDirectoryPath { get; private set; }

        public EnvDTE.Document Document { get; set; }

        public SelectedFile(string filePath, string solutionDirectoryPath)
        {
            this.FilePath = filePath;
            this.SolutionDirectoryPath = solutionDirectoryPath;

            this.FileName = Path.GetFileName(filePath);
            this.Name = Path.GetFileNameWithoutExtension(filePath);

            string friendlyFilePath = GetFriendlyPath(filePath, solutionDirectoryPath);

            this.FriendlyFilePath = friendlyFilePath;

            this.UrlFriendlyFilePath = UrlCommandFilter.GetUriOpenInVisualStudioRelativePath(friendlyFilePath);

            this.Extension = Path.GetExtension(filePath).ToLower();
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.FriendlyFilePath))
            {
                return this.FriendlyFilePath;
            }
            else
            {
                return base.ToString();
            }
        }

        public static string GetFriendlyPath(string filePath, string solutionDirectoryPath)
        {
            if (!string.IsNullOrEmpty(solutionDirectoryPath)
                && !string.IsNullOrEmpty(filePath)
                && filePath.StartsWith(solutionDirectoryPath)
            )
            {
                return filePath.Replace(solutionDirectoryPath, string.Empty);
            }

            return filePath;
        }
    }
}