using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace BaconBinary.ObjectEditor.UI.Controls
{
    public partial class EnumComboBox : UserControl
    {
        public static readonly StyledProperty<Type> EnumTypeProperty =
            AvaloniaProperty.Register<EnumComboBox, Type>(nameof(EnumType));

        public static readonly StyledProperty<object> SelectedItemProperty =
            AvaloniaProperty.Register<EnumComboBox, object>(nameof(SelectedItem), defaultBindingMode: BindingMode.TwoWay);

        private static readonly DirectProperty<EnumComboBox, IEnumerable<object>> ItemsProperty =
            AvaloniaProperty.RegisterDirect<EnumComboBox, IEnumerable<object>>(nameof(Items), o => o.Items);

        public Type EnumType
        {
            get => GetValue(EnumTypeProperty);
            set => SetValue(EnumTypeProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        private IEnumerable<object> _items;
        public IEnumerable<object> Items
        {
            get => _items;
            private set => SetAndRaise(ItemsProperty, ref _items, value);
        }

        public EnumComboBox()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == EnumTypeProperty && EnumType != null && EnumType.IsEnum)
            {
                Items = System.Enum.GetValues(EnumType).Cast<object>().ToList();
            }
        }
    }
}
