using System;
using System.Windows;

namespace TeaTime
{
    public static class DependencyProperty<TOwner, TProperty>
        where TOwner : FrameworkElement
    {
        public static DependencyProperty Register(string name, TProperty defaultValue)
        {
            return DependencyProperty.Register(name, typeof(TProperty), typeof(TOwner), new PropertyMetadata(defaultValue));
        }

        public static DependencyProperty Register(string name, TProperty defaultValue, PropertyChangedCallback propertyChangedCallback)
        {
            return DependencyProperty.Register(name, typeof(TProperty), typeof(TOwner), new PropertyMetadata(defaultValue, propertyChangedCallback));
        }
    }
}
