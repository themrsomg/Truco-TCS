using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace TrucoClient.Properties.Langs
{
    [MarkupExtensionReturnType(typeof(string))]
    public class LocExtension : MarkupExtension, INotifyPropertyChanged
    {
        private readonly string key;
        private readonly ResourceManager resourceManager = Lang.ResourceManager;

        public event PropertyChangedEventHandler PropertyChanged;

        public LocExtension(string key)
        {
            this.key = key;
            LanguageManager.LanguageChanged += OnLanguageChanged;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget service &&
                service.TargetObject is DependencyObject targetObject &&
                service.TargetProperty is DependencyProperty targetProperty)
            {
                var binding = new System.Windows.Data.Binding(nameof(Value))
                {
                    Source = this
                };
                return binding.ProvideValue(serviceProvider);
            }

            return Value;
        }

        public string Value
        {
            get
            {
                return resourceManager.GetString(key, Thread.CurrentThread.CurrentUICulture) ?? $"[{key}]";
            }
        }

        private void OnLanguageChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }
}
