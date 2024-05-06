using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FTPClientAvalonia.ViewModels;
using System;

namespace FTPClientAvalonia
{
    public class ViewLocator : IDataTemplate
    {

        public Control? Build(object? data)
        {
            if (data is null)
                return null;

            var name = data.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
            var type = Type.GetType(name);

            if (type != null)
            {
                var control = (Control)Activator.CreateInstance(type)!;
                control.DataContext = data;
                return control;
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
        
        /// <summary>
        /// Finds a view from a given ViewModel
        /// </summary>
        /// <param name="vm">The ViewModel representing a View</param>
        /// <returns>The View that matches the ViewModel. Null is no match found</returns>
        public static Window ResolveViewFromViewModel<T>(T vm) where T : ViewModelBase
        {
            var name = vm.GetType().AssemblyQualifiedName!.Replace("ViewModel", "View");
            var type = Type.GetType(name);
            return type != null ? (Window)Activator.CreateInstance(type)! : null;
        }
    }
}
