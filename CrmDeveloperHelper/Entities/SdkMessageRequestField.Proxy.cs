//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Nav.Common.VSPackages.CrmDeveloperHelper.Entities
{
    
    
    /// <summary>
    /// DisplayName:
    ///     (English - United States - 1033): Sdk Message Request Field
    ///     (Russian - 1049): Поле запроса сообщения SDK
    /// 
    /// DisplayCollectionName:
    ///     (English - United States - 1033): Sdk Message Request Fields
    ///     (Russian - 1049): Поля запроса сообщения SDK
    /// 
    /// Description:
    ///     (English - United States - 1033): For internal use only.
    ///     (Russian - 1049): Только для внутреннего использования.
    /// </summary>
    [System.Runtime.Serialization.DataContractAttribute()]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute(SdkMessageRequestField.EntityLogicalName)]
    [System.ComponentModel.DescriptionAttribute("Sdk Message Request Field")]
    public partial class SdkMessageRequestField : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
    {
        
        public const string EntityLogicalName = "sdkmessagerequestfield";
        
        public const string EntitySchemaName = "SdkMessageRequestField";
        
        public const int EntityTypeCode = 4614;
        
        public const string EntityPrimaryIdAttribute = "sdkmessagerequestfieldid";
        
        /// <summary>
        /// Default Constructor sdkmessagerequestfield
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public SdkMessageRequestField() : 
                base(EntityLogicalName)
        {
        }
        
        /// <summary>
        /// Constructor sdkmessagerequestfield for populating via LINQ queries given a LINQ anonymous type object
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public SdkMessageRequestField(object anonymousObject) : 
                this()
        {
            if (anonymousObject == null)
            {
                return;
            }

            System.Type anonymousObjectType = anonymousObject.GetType();

            if (!anonymousObjectType.Name.StartsWith("<>")
                || anonymousObjectType.Name.IndexOf("AnonymousType", System.StringComparison.InvariantCultureIgnoreCase) == -1
            )
            {
                return;
            }

            foreach (var prop in anonymousObjectType.GetProperties())
            {
                var value = prop.GetValue(anonymousObject, null);
                var name = prop.Name.ToLower();

                switch (name)
                {
                    case "id":
                    case EntityPrimaryIdAttribute:
                        if (value is System.Guid idValue)
                        {
                            Attributes[EntityPrimaryIdAttribute] = base.Id = idValue;
                        }
                        break;

                    default:
                        if (value is Microsoft.Xrm.Sdk.FormattedValueCollection formattedValueCollection)
                        {
                            FormattedValues.AddRange(formattedValueCollection);
                        }
                        else
                        {
                            Attributes[name] = value;
                        }
                        break;
                }
            }
        }
        
        #region NotifyProperty Events

        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private void OnPropertyChanged(string propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private void OnPropertyChanging(string propertyName)
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, new System.ComponentModel.PropertyChangingEventArgs(propertyName));
            }
        }
        

        #endregion
        
        #region Primary Attributes

        
        /// <summary>
        /// Description:
        ///     (English - United States - 1033): Unique identifier of the SDK message request field entity.
        ///     (Russian - 1049): Уникальный идентификатор сущности поля запроса сообщения SDK.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(EntityPrimaryIdAttribute)]
        [System.ComponentModel.DescriptionAttribute("Unique identifier of the SDK message request field entity.")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public override System.Guid Id
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return base.Id;
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.SdkMessageRequestFieldId = value;
            }
        }
        
        /// <summary>
        /// Description:
        ///     (English - United States - 1033): Unique identifier of the SDK message request field entity.
        ///     (Russian - 1049): Уникальный идентификатор сущности поля запроса сообщения SDK.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(EntityPrimaryIdAttribute)]
        [System.ComponentModel.DescriptionAttribute("Unique identifier of the SDK message request field entity.")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<System.Guid> SdkMessageRequestFieldId
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>(EntityPrimaryIdAttribute);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(SdkMessageRequestFieldId));
                this.SetAttributeValue(EntityPrimaryIdAttribute, value);
                if (value.HasValue)
                {
                    base.Id = value.Value;
                }
                else
                {
                    base.Id = System.Guid.Empty;
                }
                this.OnPropertyChanged(nameof(SdkMessageRequestFieldId));
            }
        }
        

        #endregion
        
        #region Attributes

        
        /// <summary>
        /// Description:
        ///     (English - United States - 1033): Common language runtime (CLR)-based parser for the SDK message request field.
        ///     (Russian - 1049): Парсер на основе CLR для поля запроса сообщения SDK.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.clrparser)]
        [System.ComponentModel.DescriptionAttribute("Common language runtime (CLR)-based parser for the SDK message request field.")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string ClrParser
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<string>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.clrparser);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(ClrParser));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.clrparser, value);
                this.OnPropertyChanged(nameof(ClrParser));
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Component State
        ///     (Russian - 1049): Состояние компонента
        /// 
        /// Description:
        ///     (English - United States - 1033): For internal use only.
        ///     (Russian - 1049): Только для внутреннего использования.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.componentstate)]
        [System.ComponentModel.DescriptionAttribute("Component State")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public Microsoft.Xrm.Sdk.OptionSetValue ComponentState
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.componentstate);
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Component State
        ///     (Russian - 1049): Состояние компонента
        /// 
        /// Description:
        ///     (English - United States - 1033): For internal use only.
        ///     (Russian - 1049): Только для внутреннего использования.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.componentstate)]
        [System.ComponentModel.DescriptionAttribute("Component State")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<Nav.Common.VSPackages.CrmDeveloperHelper.Entities.GlobalOptionSets.componentstate> ComponentStateEnum
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue optionSetValue = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.componentstate);
                if (((optionSetValue != null) 
                            && System.Enum.IsDefined(typeof(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.GlobalOptionSets.componentstate), optionSetValue.Value)))
                {
                    return ((Nav.Common.VSPackages.CrmDeveloperHelper.Entities.GlobalOptionSets.componentstate)(System.Enum.ToObject(typeof(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.GlobalOptionSets.componentstate), optionSetValue.Value)));
                }
                else
                {
                    return null;
                }
            }
        }
        
        /// <summary>
        /// Description:
        ///     (English - United States - 1033): Unique identifier of the user who created the SDK message request field.
        ///     (Russian - 1049): Уникальный идентификатор пользователя, создавшего поле запроса сообщения SDK.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.createdby)]
        [System.ComponentModel.DescriptionAttribute("Unique identifier of the user who created the SDK message request field.")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public Microsoft.Xrm.Sdk.EntityReference CreatedBy
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.createdby);
            }
        }
        
        /// <summary>
        /// Description:
        ///     (English - United States - 1033): Date and time when the SDK message request field was created.
        ///     (Russian - 1049): Дата и время создания поля запроса сообщения SDK.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.createdon)]
        [System.ComponentModel.DescriptionAttribute("Date and time when the SDK message request field was created.")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<System.DateTime> CreatedOn
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.createdon);
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Created By (Delegate)
        ///     (Russian - 1049): Кем создано (делегат)
        /// 
        /// Description:
        ///     (English - United States - 1033): Unique identifier of the delegate user who created the sdkmessagerequestfield.
        ///     (Russian - 1049): Уникальный идентификатор делегата, создавшего поле запроса сообщения SDK.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.createdonbehalfby)]
        [System.ComponentModel.DescriptionAttribute("Created By (Delegate)")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public Microsoft.Xrm.Sdk.EntityReference CreatedOnBehalfBy
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.createdonbehalfby);
            }
        }
        
        /// <summary>
        /// Description:
        ///     (English - United States - 1033): Customization level of the SDK message request field.
        ///     (Russian - 1049): Уровень настройки  поля запроса сообщения SDK.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.customizationlevel)]
        [System.ComponentModel.DescriptionAttribute("Customization level of the SDK message request field.")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<int> CustomizationLevel
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.customizationlevel);
            }
        }
        
        /// <summary>
        /// Description:
        ///     (English - United States - 1033): Indicates how field contents are used during message processing. 1 - Primary entity, 2- Secondary entity
        ///     (Russian - 1049): Указывает на порядок использования содержимого поля в ходе обработки сообщения. 1 - основная сущность, 2 - дополнительная сущность.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.fieldmask)]
        [System.ComponentModel.DescriptionAttribute("Indicates how field contents are used during message processing. 1 - Primary enti" +
            "ty, 2- Secondary entity")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<int> FieldMask
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.fieldmask);
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Introduced Version
        ///     (Russian - 1049): Версия добавления
        /// 
        /// Description:
        ///     (English - United States - 1033): Version in which the component is introduced.
        ///     (Russian - 1049): Версия, в которой был введен компонент.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.introducedversion)]
        [System.ComponentModel.DescriptionAttribute("Introduced Version")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string IntroducedVersion
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<string>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.introducedversion);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(IntroducedVersion));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.introducedversion, value);
                this.OnPropertyChanged(nameof(IntroducedVersion));
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): State
        ///     (Russian - 1049): Состояние
        /// 
        /// Description:
        ///     (English - United States - 1033): Information that specifies whether this component is managed.
        ///     (Russian - 1049): Сведения о том, является ли компонент управляемым.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.ismanaged)]
        [System.ComponentModel.DescriptionAttribute("State")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<bool> IsManaged
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<bool>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.ismanaged);
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Modified By
        ///     (Russian - 1049): Изменено
        /// 
        /// Description:
        ///     (English - United States - 1033): Unique identifier of the user who last modified the SDK message request field.
        ///     (Russian - 1049): Уникальный идентификатор последнего пользователя, изменившего поле запроса сообщения SDK.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.modifiedby)]
        [System.ComponentModel.DescriptionAttribute("Modified By")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public Microsoft.Xrm.Sdk.EntityReference ModifiedBy
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.modifiedby);
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Modified On
        ///     (Russian - 1049): Дата изменения
        /// 
        /// Description:
        ///     (English - United States - 1033): Date and time when the SDK message request field was last modified.
        ///     (Russian - 1049): Дата и время последнего изменения поля запроса сообщения SDK.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.modifiedon)]
        [System.ComponentModel.DescriptionAttribute("Modified On")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<System.DateTime> ModifiedOn
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.modifiedon);
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Modified By (Delegate)
        ///     (Russian - 1049): Кем изменено (делегат)
        /// 
        /// Description:
        ///     (English - United States - 1033): Unique identifier of the delegate user who last modified the sdkmessagerequestfield.
        ///     (Russian - 1049): Уникальный идентификатор делегата, внесшего последнее изменение в поле запроса сообщения SDK.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.modifiedonbehalfby)]
        [System.ComponentModel.DescriptionAttribute("Modified By (Delegate)")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public Microsoft.Xrm.Sdk.EntityReference ModifiedOnBehalfBy
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.modifiedonbehalfby);
            }
        }
        
        /// <summary>
        /// Description:
        ///     (English - United States - 1033): Name of the SDK message request field.
        ///     (Russian - 1049): Имя поля запроса сообщения SDK.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.name)]
        [System.ComponentModel.DescriptionAttribute("Name of the SDK message request field.")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string Name
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<string>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.name);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(Name));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.name, value);
                this.OnPropertyChanged(nameof(Name));
            }
        }
        
        /// <summary>
        /// Description:
        ///     (English - United States - 1033): Information about whether SDK message request field is optional.
        ///     (Russian - 1049): Сведения о том, является ли поле запроса сообщения SDK необязательным.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.optional)]
        [System.ComponentModel.DescriptionAttribute("Information about whether SDK message request field is optional.")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<bool> Optional
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<bool>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.optional);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(Optional));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.optional, value);
                this.OnPropertyChanged(nameof(Optional));
            }
        }
        
        /// <summary>
        /// Description:
        ///     (English - United States - 1033): Unique identifier of the organization with which the SDK message request field is associated.
        ///     (Russian - 1049): Уникальный идентификатор организации, с которой связано поле запроса сообщения SDK.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.organizationid)]
        [System.ComponentModel.DescriptionAttribute("Unique identifier of the organization with which the SDK message request field is" +
            " associated.")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public Microsoft.Xrm.Sdk.EntityReference OrganizationId
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.organizationid);
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Record Overwrite Time
        ///     (Russian - 1049): Время замены записи
        /// 
        /// Description:
        ///     (English - United States - 1033): For internal use only.
        ///     (Russian - 1049): Только для внутреннего использования.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.overwritetime)]
        [System.ComponentModel.DescriptionAttribute("Record Overwrite Time")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<System.DateTime> OverwriteTime
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.overwritetime);
            }
        }
        
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.parameterbindinginformation)]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string ParameterBindingInformation
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<string>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.parameterbindinginformation);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(ParameterBindingInformation));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.parameterbindinginformation, value);
                this.OnPropertyChanged(nameof(ParameterBindingInformation));
            }
        }
        
        /// <summary>
        /// Description:
        ///     (English - United States - 1033): Parser for the SDK message request field.
        ///     (Russian - 1049): Парсер для поля запроса сообщения SDK.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.parser)]
        [System.ComponentModel.DescriptionAttribute("Parser for the SDK message request field.")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string Parser
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<string>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.parser);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(Parser));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.parser, value);
                this.OnPropertyChanged(nameof(Parser));
            }
        }
        
        /// <summary>
        /// Description:
        ///     (English - United States - 1033): Position of the Sdk message request field
        ///     (Russian - 1049): Положение поля запроса сообщения SDK
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.position)]
        [System.ComponentModel.DescriptionAttribute("Position of the Sdk message request field")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<int> Position
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.position);
            }
        }
        
        /// <summary>
        /// Description:
        ///     (English - United States - 1033): Public name of the SDK message request field.
        ///     (Russian - 1049): Внешнее имя поля запроса сообщения SDK.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.publicname)]
        [System.ComponentModel.DescriptionAttribute("Public name of the SDK message request field.")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string PublicName
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<string>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.publicname);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(PublicName));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.publicname, value);
                this.OnPropertyChanged(nameof(PublicName));
            }
        }
        
        /// <summary>
        /// Description:
        ///     (English - United States - 1033): Entity identifier of the SDK message request field.
        ///     (Russian - 1049): ИД сущности поля запроса сообщения SDK.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.sdkmessagerequestfieldidunique)]
        [System.ComponentModel.DescriptionAttribute("Entity identifier of the SDK message request field.")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<System.Guid> SdkMessageRequestFieldIdUnique
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.sdkmessagerequestfieldidunique);
            }
        }
        
        /// <summary>
        /// Description:
        ///     (English - United States - 1033): Unique identifier of the message request with which the SDK message request field is associated.
        ///     (Russian - 1049): Уникальный идентификатор запроса сообщения, с которым связано поле запроса сообщения SDK.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.sdkmessagerequestid)]
        [System.ComponentModel.DescriptionAttribute("Unique identifier of the message request with which the SDK message request field" +
            " is associated.")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public Microsoft.Xrm.Sdk.EntityReference SdkMessageRequestId
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.sdkmessagerequestid);
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Solution
        ///     (Russian - 1049): Решение
        /// 
        /// Description:
        ///     (English - United States - 1033): Unique identifier of the associated solution.
        ///     (Russian - 1049): Уникальный идентификатор связанного решения.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.solutionid)]
        [System.ComponentModel.DescriptionAttribute("Solution")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<System.Guid> SolutionId
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.solutionid);
            }
        }
        
        /// <summary>
        /// Description:
        ///     (English - United States - 1033): For internal use only.
        ///     (Russian - 1049): Только для внутреннего использования.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.versionnumber)]
        [System.ComponentModel.DescriptionAttribute("For internal use only.")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<long> VersionNumber
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<long>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.SdkMessageRequestField.Schema.Attributes.versionnumber);
            }
        }
        

        #endregion
    }
}
