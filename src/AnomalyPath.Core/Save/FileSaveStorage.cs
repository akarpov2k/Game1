#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Save
{
    /// <summary>Файловое хранилище слотов: JSON рядом с бэкапом, запись через временный файл.</summary>
    public sealed class FileSaveStorage : ISaveStorage
    {
        private const string Extension = ".json";
        private const string BackupExtension = ".bak";
        private const string TempExtension = ".tmp";
        private readonly string _root;

        /// <summary>Создаёт хранилище в папке.</summary>
        /// <param name="root">Папка сохранений; создаётся при первой записи.</param>
        /// <exception cref="ArgumentException">Путь не задан.</exception>
        public FileSaveStorage(string root)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                throw new ArgumentException(CoreText.Get(CoreErrors.SaveRootMissing), nameof(root));
            }

            _root = Path.GetFullPath(root);
        }

        /// <summary>Корневая папка сохранений.</summary>
        public string Root => _root;

        /// <inheritdoc />
        public bool Exists(string slot) => File.Exists(GetPath(slot));

        /// <inheritdoc />
        public string Read(string slot)
        {
            var path = GetPath(slot);
            if (!File.Exists(path))
            {
                throw new DataException(slot, CoreErrors.SaveNotFound, slot);
            }

            return File.ReadAllText(path, Encoding.UTF8);
        }

        /// <inheritdoc />
        public void Write(string slot, string text)
        {
            var path = GetPath(slot);
            var temp = path + TempExtension;
            try
            {
                Directory.CreateDirectory(_root);
                File.WriteAllText(temp, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                if (File.Exists(path))
                {
                    try
                    {
                        File.Replace(temp, path, path + BackupExtension);
                    }
                    catch (IOException)
                    {
                        ReplaceWithBackup(temp, path, path + BackupExtension);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        ReplaceWithBackup(temp, path, path + BackupExtension);
                    }
                }
                else
                {
                    File.Move(temp, path);
                }
            }
            catch (IOException error)
            {
                TryDelete(temp);
                throw new DataException(slot, CoreErrors.SaveWriteFailed, error.Message);
            }
            catch (UnauthorizedAccessException error)
            {
                TryDelete(temp);
                throw new DataException(slot, CoreErrors.SaveWriteFailed, error.Message);
            }
        }

        /// <inheritdoc />
        public void Delete(string slot)
        {
            var path = GetPath(slot);
            TryDelete(path);
            TryDelete(path + BackupExtension);
            TryDelete(path + TempExtension);
        }

        /// <inheritdoc />
        public IReadOnlyList<string> List()
        {
            var result = new List<string>();
            if (!Directory.Exists(_root))
            {
                return result;
            }

            foreach (var file in Directory.GetFiles(_root, "*" + Extension))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (!string.IsNullOrEmpty(name))
                {
                    result.Add(name);
                }
            }

            result.Sort(StringComparer.Ordinal);
            return result;
        }

        /// <summary>Возвращает путь к файлу слота.</summary>
        /// <param name="slot">Имя слота.</param>
        /// <returns>Абсолютный путь к JSON-файлу.</returns>
        /// <exception cref="DataException">Имя слота содержит недопустимые символы.</exception>
        public string GetPath(string slot)
        {
            if (string.IsNullOrWhiteSpace(slot) || slot.Length > 32 || slot == "." || slot == "..")
            {
                throw new DataException(slot, CoreErrors.SaveSlotInvalid, slot);
            }

            foreach (var character in slot)
            {
                var allowed = (character >= 'a' && character <= 'z') ||
                              (character >= 'A' && character <= 'Z') ||
                              (character >= '0' && character <= '9') ||
                              character == '_' ||
                              character == '-';
                if (!allowed)
                {
                    throw new DataException(slot, CoreErrors.SaveSlotInvalid, slot);
                }
            }

            var path = Path.GetFullPath(Path.Combine(_root, slot + Extension));
            var rootWithSeparator = _root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? _root
                : _root + Path.DirectorySeparatorChar;
            if (!path.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            {
                throw new DataException(slot, CoreErrors.SaveSlotInvalid, slot);
            }

            return path;
        }

        private static void ReplaceWithBackup(string temp, string path, string backup)
        {
            File.Copy(path, backup, overwrite: true);
            File.Delete(path);
            File.Move(temp, path);
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException)
            {
                // удаление служебного файла не должно блокировать сохранение
            }
        }
    }
}
