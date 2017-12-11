﻿using AMIClient.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AMIClient.VML
{
    public class ViewModelLocator
    {
        public static bool GetAutoHookedUpViewModel(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoHookedUpViewModelProperty);
        }

        public static void SetAutoHookedUpViewModel(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoHookedUpViewModelProperty, value);
        }

        // Using a DependencyProperty as the backing store for AutoHookedUpViewModel. 

        //This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoHookedUpViewModelProperty =
           DependencyProperty.RegisterAttached("AutoHookedUpViewModel",
           typeof(bool), typeof(ViewModelLocator), new
           PropertyMetadata(false, AutoHookedUpViewModelChanged));

        private static void AutoHookedUpViewModelChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(d)) return;
            var viewType = d.GetType();

            string str = viewType.FullName;
            str = str.Replace(".View", ".ViewModels");

            var viewTypeName = str;
            var viewModelTypeName = viewTypeName.Replace("Control", "ViewModel");
            var viewModelType = Type.GetType(viewModelTypeName);
            var viewModel = Activator.CreateInstance(viewModelType);

            if (viewTypeName.Contains("NetworkPreviewViewModel"))
            {
                ((FrameworkElement)d).DataContext = NetworkPreviewViewModel.Instance;
            }
            else if (viewTypeName.Contains("AddCimXmlViewModel"))
            {
                ((FrameworkElement)d).DataContext = AddCimXmlViewModel.Instance;
            }
        }
    }
}
