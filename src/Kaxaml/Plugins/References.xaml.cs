﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using JetBrains.Annotations;
using Kaxaml.Core;
using KaxamlPlugins;

namespace Kaxaml.Plugins
{
    /// <summary>
    /// Interaction logic for References.xaml
    /// </summary>
    public partial class References : UserControl
    {
        public References()
        {
            this.DataContext = new ReferencesViewModel();
            InitializeComponent();
        }

        public bool AddNewReferences(string fileName)
        {
            return ((ReferencesViewModel) this.DataContext).AddNewReferences(fileName);
        }
    }

    public class ReferencesViewModel : INotifyPropertyChanged
    {
        private readonly HashSet<Reference> _addedReferences = new HashSet<Reference>();
        private ObservableCollection<Reference> _allReferences;

        public ReferencesViewModel()
        {
            this.AllReferences = new ObservableCollection<Reference>();
        }

        public bool AddNewReferences(string fileName)
        {
            if (!File.Exists(fileName)) return false;

            try
            {
                var fileInfo = new FileInfo(fileName);
                var reference = new Reference(fileInfo);

                if (_addedReferences.Contains(reference)) return false;

                _addedReferences.Add(reference);
                this.AllReferences.Add(reference);
            }
            catch (Exception ex)
            {
                if (ex.IsCriticalException())
                {
                    throw;
                }
                else
                {
                    var window = KaxamlInfo.MainWindow as MainWindow;
                    window?.DocumentsView?.SelectedView?.ReportError(ex);
                }
            }

            return true;
        }

        public ObservableCollection<Reference> AllReferences
        {
            get { return _allReferences; }
            set
            {
                if (Equals(value, _allReferences)) return;
                _allReferences = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Reference
    {
        public Reference([NotNull] FileInfo fileInfo)
        {
            this.Name = fileInfo.Name;
            this.FullName = fileInfo.FullName;

            var bytes = File.ReadAllBytes(this.FullName);
            Assembly asm = Assembly.Load(bytes);
            this.AssemblyFileVersion = asm.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
        }

        public string Name { get; }

        public string FullName { get; }

        public string AssemblyFileVersion { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Reference) obj);
        }

        protected bool Equals(Reference other)
        {
            return string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase) && string.Equals(AssemblyFileVersion, other.AssemblyFileVersion, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(Name) : 0) * 397) ^ (AssemblyFileVersion != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(AssemblyFileVersion) : 0);
            }
        }
    }
}