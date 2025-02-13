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
    ///     (English - United States - 1033): App Module Component
    ///     (Russian - 1049): Компонент модуля приложения
    /// 
    /// DisplayCollectionName:
    ///     (English - United States - 1033): App Module Components
    ///     (Russian - 1049): Компоненты модуля приложения
    /// 
    /// Description:
    ///     (English - United States - 1033): A component available in a business app such as entity, dashboard, form, view, chart, and business process.
    ///     (Russian - 1049): Компонент, доступный в бизнес-приложении, например сущность, панель мониторинга, форма, представление, диаграмма и бизнес-процесс.
    /// </summary>
    [System.Runtime.Serialization.DataContractAttribute()]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute(AppModuleComponent.EntityLogicalName)]
    [System.ComponentModel.DescriptionAttribute("App Module Component")]
    public partial class AppModuleComponent : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
    {
        
        public const string EntityLogicalName = "appmodulecomponent";
        
        public const string EntitySchemaName = "AppModuleComponent";
        
        public const int EntityTypeCode = 9007;
        
        public const string EntityPrimaryIdAttribute = "appmodulecomponentid";
        
        /// <summary>
        /// Default Constructor appmodulecomponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public AppModuleComponent() : 
                base(EntityLogicalName)
        {
        }
        
        /// <summary>
        /// Constructor appmodulecomponent for populating via LINQ queries given a LINQ anonymous type object
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public AppModuleComponent(object anonymousObject) : 
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
        /// DisplayName:
        ///     (English - United States - 1033): App Module Component
        ///     (Russian - 1049): Компонент модуля приложения
        /// 
        /// Description:
        ///     (English - United States - 1033): Unique identifier for entity instances
        ///     (Russian - 1049): Уникальный код для экземпляров сущности
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(EntityPrimaryIdAttribute)]
        [System.ComponentModel.DescriptionAttribute("App Module Component")]
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
                this.AppModuleComponentId = value;
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): App Module Component
        ///     (Russian - 1049): Компонент модуля приложения
        /// 
        /// Description:
        ///     (English - United States - 1033): Unique identifier for entity instances
        ///     (Russian - 1049): Уникальный код для экземпляров сущности
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(EntityPrimaryIdAttribute)]
        [System.ComponentModel.DescriptionAttribute("App Module Component")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<System.Guid> AppModuleComponentId
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>(EntityPrimaryIdAttribute);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(AppModuleComponentId));
                this.SetAttributeValue(EntityPrimaryIdAttribute, value);
                if (value.HasValue)
                {
                    base.Id = value.Value;
                }
                else
                {
                    base.Id = System.Guid.Empty;
                }
                this.OnPropertyChanged(nameof(AppModuleComponentId));
            }
        }
        

        #endregion
        
        #region Attributes

        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Application Component Unique Id
        ///     (Russian - 1049): Уникальный идентификатор компонента приложения
        /// 
        /// Description:
        ///     (English - United States - 1033): Unique identifier of the Application Component used when synchronizing customizations for the Microsoft Dynamics 365 client for Outlook
        ///     (Russian - 1049): Уникальный идентификатор компонента приложения, используемого при синхронизации настроек для клиента Microsoft Dynamics 365 для Outlook.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.appmodulecomponentidunique)]
        [System.ComponentModel.DescriptionAttribute("Application Component Unique Id")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<System.Guid> AppModuleComponentIdUnique
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.appmodulecomponentidunique);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(AppModuleComponentIdUnique));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.appmodulecomponentidunique, value);
                this.OnPropertyChanged(nameof(AppModuleComponentIdUnique));
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): AppModule
        /// 
        /// Description:
        ///     (English - United States - 1033): The App Module Id Unique
        ///     (Russian - 1049): Уникальный идентификатор модуля приложения
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.appmoduleidunique)]
        [System.ComponentModel.DescriptionAttribute("AppModule")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public Microsoft.Xrm.Sdk.EntityReference AppModuleIdUnique
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.appmoduleidunique);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(AppModuleIdUnique));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.appmoduleidunique, value);
                this.OnPropertyChanged(nameof(AppModuleIdUnique));
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Object Type Code
        ///     (Russian - 1049): Код типа объекта
        /// 
        /// Description:
        ///     (English - United States - 1033): The object type code of the component.
        ///     (Russian - 1049): Код типа объекта компонента.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.componenttype)]
        [System.ComponentModel.DescriptionAttribute("Object Type Code")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public Microsoft.Xrm.Sdk.OptionSetValue ComponentType
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.componenttype);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(ComponentType));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.componenttype, value);
                this.OnPropertyChanged(nameof(ComponentType));
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Object Type Code
        ///     (Russian - 1049): Код типа объекта
        /// 
        /// Description:
        ///     (English - United States - 1033): The object type code of the component.
        ///     (Russian - 1049): Код типа объекта компонента.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.componenttype)]
        [System.ComponentModel.DescriptionAttribute("Object Type Code")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.OptionSets.componenttype> ComponentTypeEnum
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue optionSetValue = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.componenttype);
                if (((optionSetValue != null) 
                            && System.Enum.IsDefined(typeof(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.OptionSets.componenttype), optionSetValue.Value)))
                {
                    return ((Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.OptionSets.componenttype)(System.Enum.ToObject(typeof(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.OptionSets.componenttype), optionSetValue.Value)));
                }
                else
                {
                    return null;
                }
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(ComponentTypeEnum));
                this.OnPropertyChanging(nameof(ComponentType));
                if ((value == null))
                {
                    this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.componenttype, null);
                }
                else
                {
                    this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.componenttype, new Microsoft.Xrm.Sdk.OptionSetValue(((int)(value))));
                }
                this.OnPropertyChanged(nameof(ComponentType));
                this.OnPropertyChanged(nameof(ComponentTypeEnum));
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Created By
        ///     (Russian - 1049): Кем создано
        /// 
        /// Description:
        ///     (English - United States - 1033): Unique identifier of the user who created the record.
        ///     (Russian - 1049): Уникальный идентификатор пользователя, создавшего запись.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.createdby)]
        [System.ComponentModel.DescriptionAttribute("Created By")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public Microsoft.Xrm.Sdk.EntityReference CreatedBy
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.createdby);
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Created On
        ///     (Russian - 1049): Дата и время создания
        /// 
        /// Description:
        ///     (English - United States - 1033): Date and time when the record was created.
        ///     (Russian - 1049): Дата и время создания записи.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.createdon)]
        [System.ComponentModel.DescriptionAttribute("Created On")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<System.DateTime> CreatedOn
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.createdon);
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Created By (Delegate)
        ///     (Russian - 1049): Кем создано (представитель)
        /// 
        /// Description:
        ///     (English - United States - 1033): Unique identifier of the delegate user who created the record.
        ///     (Russian - 1049): Уникальный идентификатор пользователя-представителя, создавшего запись.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.createdonbehalfby)]
        [System.ComponentModel.DescriptionAttribute("Created By (Delegate)")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public Microsoft.Xrm.Sdk.EntityReference CreatedOnBehalfBy
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.createdonbehalfby);
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): ExchangeRate
        /// 
        /// Description:
        ///     (English - United States - 1033): Exchange rate for the currency associated with the Application Component with respect to the base currency.
        ///     (Russian - 1049): Обменный курс валюты, связанной с компонентом приложения, по отношению к базовой валюте.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.exchangerate)]
        [System.ComponentModel.DescriptionAttribute("ExchangeRate")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<decimal> ExchangeRate
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<decimal>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.exchangerate);
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Introduced Version
        ///     (Russian - 1049): Введенная версия
        /// 
        /// Description:
        ///     (English - United States - 1033): Version in which the application component record is introduced.
        ///     (Russian - 1049): Версия, в которой была введена запись компонента приложения.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.introducedversion)]
        [System.ComponentModel.DescriptionAttribute("Introduced Version")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string IntroducedVersion
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<string>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.introducedversion);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(IntroducedVersion));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.introducedversion, value);
                this.OnPropertyChanged(nameof(IntroducedVersion));
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Is Default
        ///     (Russian - 1049): По умолчанию
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.isdefault)]
        [System.ComponentModel.DescriptionAttribute("Is Default")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<bool> IsDefault
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<bool>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.isdefault);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(IsDefault));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.isdefault, value);
                this.OnPropertyChanged(nameof(IsDefault));
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Is Metadata
        ///     (Russian - 1049): Является метаданными
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.ismetadata)]
        [System.ComponentModel.DescriptionAttribute("Is Metadata")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<bool> IsMetadata
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<bool>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.ismetadata);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(IsMetadata));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.ismetadata, value);
                this.OnPropertyChanged(nameof(IsMetadata));
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Modified By
        ///     (Russian - 1049): Кем изменено
        /// 
        /// Description:
        ///     (English - United States - 1033): Unique identifier of the user who modified the record.
        ///     (Russian - 1049): Уникальный идентификатор пользователя, изменившего запись.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.modifiedby)]
        [System.ComponentModel.DescriptionAttribute("Modified By")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public Microsoft.Xrm.Sdk.EntityReference ModifiedBy
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.modifiedby);
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Modified On
        ///     (Russian - 1049): Дата изменения
        /// 
        /// Description:
        ///     (English - United States - 1033): Date and time when the record was modified.
        ///     (Russian - 1049): Дата и время изменения записи.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.modifiedon)]
        [System.ComponentModel.DescriptionAttribute("Modified On")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<System.DateTime> ModifiedOn
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.modifiedon);
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Modified By (Delegate)
        ///     (Russian - 1049): Изменено (представитель)
        /// 
        /// Description:
        ///     (English - United States - 1033): Unique identifier of the delegate user who modified the record.
        ///     (Russian - 1049): Уникальный идентификатор пользователя-представителя, изменившего запись.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.modifiedonbehalfby)]
        [System.ComponentModel.DescriptionAttribute("Modified By (Delegate)")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public Microsoft.Xrm.Sdk.EntityReference ModifiedOnBehalfBy
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.modifiedonbehalfby);
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Object
        ///     (Russian - 1049): Объект
        /// 
        /// Description:
        ///     (English - United States - 1033): Object Id
        ///     (Russian - 1049): Идентификатор объекта
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.objectid)]
        [System.ComponentModel.DescriptionAttribute("Object")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<System.Guid> ObjectId
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.objectid);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(ObjectId));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.objectid, value);
                this.OnPropertyChanged(nameof(ObjectId));
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Created On
        ///     (Russian - 1049): Когда создано
        /// 
        /// Description:
        ///     (English - United States - 1033): Date and time when the record was created.
        ///     (Russian - 1049): Дата и время, когда была создана запись.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.overwritetime)]
        [System.ComponentModel.DescriptionAttribute("Created On")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<System.DateTime> OverwriteTime
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.overwritetime);
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Root App Module Component
        ///     (Russian - 1049): Корневой компонент модуля приложения
        /// 
        /// Description:
        ///     (English - United States - 1033): The parent ID of the subcomponent, which will be a root
        ///     (Russian - 1049): Родительский идентификатор подкомпонента, который будет корневым
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.rootappmodulecomponentid)]
        [System.ComponentModel.DescriptionAttribute("Root App Module Component")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<System.Guid> RootAppModuleComponentId
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.rootappmodulecomponentid);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(RootAppModuleComponentId));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.rootappmodulecomponentid, value);
                this.OnPropertyChanged(nameof(RootAppModuleComponentId));
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Root Component Behavior
        ///     (Russian - 1049): Поведение корневого компонента
        /// 
        /// Description:
        ///     (English - United States - 1033): Indicates the include behavior of the root component.
        ///     (Russian - 1049): Указывает на поведение включения корневого компонента.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.rootcomponentbehavior)]
        [System.ComponentModel.DescriptionAttribute("Root Component Behavior")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public Microsoft.Xrm.Sdk.OptionSetValue RootComponentBehavior
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.rootcomponentbehavior);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(RootComponentBehavior));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.rootcomponentbehavior, value);
                this.OnPropertyChanged(nameof(RootComponentBehavior));
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Root Component Behavior
        ///     (Russian - 1049): Поведение корневого компонента
        /// 
        /// Description:
        ///     (English - United States - 1033): Indicates the include behavior of the root component.
        ///     (Russian - 1049): Указывает на поведение включения корневого компонента.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.rootcomponentbehavior)]
        [System.ComponentModel.DescriptionAttribute("Root Component Behavior")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.OptionSets.rootcomponentbehavior> RootComponentBehaviorEnum
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue optionSetValue = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.rootcomponentbehavior);
                if (((optionSetValue != null) 
                            && System.Enum.IsDefined(typeof(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.OptionSets.rootcomponentbehavior), optionSetValue.Value)))
                {
                    return ((Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.OptionSets.rootcomponentbehavior)(System.Enum.ToObject(typeof(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.OptionSets.rootcomponentbehavior), optionSetValue.Value)));
                }
                else
                {
                    return null;
                }
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(RootComponentBehaviorEnum));
                this.OnPropertyChanging(nameof(RootComponentBehavior));
                if ((value == null))
                {
                    this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.rootcomponentbehavior, null);
                }
                else
                {
                    this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.rootcomponentbehavior, new Microsoft.Xrm.Sdk.OptionSetValue(((int)(value))));
                }
                this.OnPropertyChanged(nameof(RootComponentBehavior));
                this.OnPropertyChanged(nameof(RootComponentBehaviorEnum));
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): Time Zone Rule Version Number
        ///     (Russian - 1049): Номер версии правила часового пояса
        /// 
        /// Description:
        ///     (English - United States - 1033): For internal use only.
        ///     (Russian - 1049): Только для внутреннего использования.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.timezoneruleversionnumber)]
        [System.ComponentModel.DescriptionAttribute("Time Zone Rule Version Number")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<int> TimeZoneRuleVersionNumber
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.timezoneruleversionnumber);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(TimeZoneRuleVersionNumber));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.timezoneruleversionnumber, value);
                this.OnPropertyChanged(nameof(TimeZoneRuleVersionNumber));
            }
        }
        
        /// <summary>
        /// DisplayName:
        ///     (English - United States - 1033): UTC Conversion Time Zone Code
        ///     (Russian - 1049): Идентификатор часового пояса (преобразование в UTC)
        /// 
        /// Description:
        ///     (English - United States - 1033): Time zone code that was in use when the record was created.
        ///     (Russian - 1049): Идентификатор часового пояса, использовавшийся при создании записи.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.utcconversiontimezonecode)]
        [System.ComponentModel.DescriptionAttribute("UTC Conversion Time Zone Code")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<int> UTCConversionTimeZoneCode
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.utcconversiontimezonecode);
            }
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            set
            {
                this.OnPropertyChanging(nameof(UTCConversionTimeZoneCode));
                this.SetAttributeValue(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.utcconversiontimezonecode, value);
                this.OnPropertyChanged(nameof(UTCConversionTimeZoneCode));
            }
        }
        
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.versionnumber)]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Nullable<long> VersionNumber
        {
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            get
            {
                return this.GetAttributeValue<System.Nullable<long>>(Nav.Common.VSPackages.CrmDeveloperHelper.Entities.AppModuleComponent.Schema.Attributes.versionnumber);
            }
        }
        

        #endregion
    }
}
