using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Nav.Common.VSPackages.CrmDeveloperHelper.UserControls.AttributeMetadataControls
{
    public partial class DoubleAttributeMetadataControl : UserControl, IAttributeMetadataControl<DoubleAttributeMetadata>
    {
        public DoubleAttributeMetadata AttributeMetadata { get; }

        private readonly double? _initialValue;

        private readonly bool _allwaysAddToEntity;

        public DoubleAttributeMetadataControl(DoubleAttributeMetadata attributeMetadata, double? initialValue, bool allwaysAddToEntity, bool showRestoreButton)
        {
            InitializeComponent();

            AttributeMetadataControlFactory.SetGroupBoxNameByAttributeMetadata(gbAttribute, attributeMetadata);

            this._initialValue = initialValue;
            this._allwaysAddToEntity = allwaysAddToEntity;
            this.AttributeMetadata = attributeMetadata;

            txtBValue.Text = _initialValue.ToString();

            Views.WindowBase.SetElementsEnabledAndVisible(allwaysAddToEntity, btnRemoveControl);

            Views.WindowBase.SetElementsEnabledAndVisible(showRestoreButton, btnRestore);

            Views.WindowBase.SetElementsVisible(showRestoreButton, chBChanged);
        }

        private void txtBValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            var currentValue = GetDoubleValue();

            chBChanged.IsChecked = currentValue != _initialValue;
        }

        private double? GetDoubleValue()
        {
            var value = txtBValue.Text.Trim();

            if (double.TryParse(value, out var result))
            {
                return result;
            }

            return null;
        }

        public void AddAttribute(Entity entity)
        {
            var currentValue = GetDoubleValue();

            entity.Attributes[AttributeMetadata.LogicalName] = currentValue;
        }

        public void AddChangedAttribute(Entity entity)
        {
            var currentValue = GetDoubleValue();

            if (this._allwaysAddToEntity || currentValue != _initialValue)
            {
                entity.Attributes[AttributeMetadata.LogicalName] = currentValue;
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            txtBValue.Focus();

            base.OnGotFocus(e);
        }

        public event EventHandler RemoveControlClicked;

        private void btnRemoveControl_Click(object sender, RoutedEventArgs e)
        {
            RemoveControlClicked?.Invoke(this, EventArgs.Empty);
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            txtBValue.Text = _initialValue.ToString();
        }
    }
}