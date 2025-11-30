## [1.0.0] 2024-07-06
### First Release
## [1.0.1] 2024-07-10
### Meta files patch
## [1.1.0] 2024-07-10
### Now it is not required to define UI type in code.
Assign one of the next values:
 - PopAbove
 - OpenNewWindow

In UiType filed on view to define if opened Ui element should hide all previous views or only pop above them
## [1.1.1] 2024-07-10
### Minor fixes for UI View setup
## [1.1.2] 2024-07-10
### Added setting for UI parent transform for BaseUiViewManager
## [1.1.3] 2024-07-11
### Fixed Sequence contains no elements exception when opening first UI view in list
## [1.1.4] 2024-07-11
### Fixed sprite selection by SpriteContainerBinding if property value is null
## [1.1.5] 2024-07-11
### Fixed ui state handling
## [1.1.6] 2024-07-12
### Fixed view scale and position on open
## [1.1.7] 2024-07-12
### Minor implementation changes
## [1.2.0] 2024-10-18
### Changes to BaseUiViewManager
 - Replaced ResourceBasePath with more flexible ResourceSearchPattern with default value "Prefabs/UI/{UiViewType}/{UiViewType}" where {UiViewType} will be automatically replaced with string Enum value of UiView
 - Made GetResourcesUiPath method virtual to allow custom path generation
 - Full expanding each UiView RectTransform on every open - now every UiView will fully cover UiViewsContainer every time
## [1.2.1] 2024-10-18
### Improvements to ResourceSearchPattern parsing~~~~
## [1.2.2] 2024-10-19
### Now it is possible to open UIView with specific Context State
### Added method to validate last opened view
## [1.3.0] 2024-11-21
### Added MasterPathBinding. Now it is possible to specify same parent context for al child bindings not to write context name at the beginning several times
## [2.0.0] 2025-02-03
### Separated BaseUiViewManager and BaseViewModel. Added SelfContainedViewManager and SelfContainedViewModel to communicate through events
## [3.0.0] 2025-06-09
### Removed implementation as Submodule
## [3.0.1] 2025-06-09
### Fixed State is set after the View is enabled. Now it is possible to set State and instantly use it on enable
## [3.0.2] 2025-06-09
### Fixes for Context State setting
## [3.0.3] 2025-06-09
### Fixed MasterPath application and converted path resolving to Spans
## [3.0.5] 2025-06-10
### Fixed property path resolving
## [3.0.5] 2025-06-10
### Fixed command path resolving
## [3.1.0] 2025-06-10
### Instantiating views from resources in inactive state
## [3.2.0] 2025-07-16
### Added ColorByBoolBinding, added support for flags in ConditionBinding, added IInjectedContextState interface to inject Context to its CurrentState upon setting
## [3.2.1] 2025-07-17
### Fixed IInjectedContextState interface
## [3.2.2] 2025-07-17
### Improvement for IInjectedContextState interface
## [3.2.3] 2025-07-17
### Fixed ConditionBinding Enum evaluation
## [3.3.0] 2025-07-18
### Changes:
 - Added PropertyBatch to aggregate several properties into a group
 - Added PagedCollectionViewBinding and PagedCollectionProperty PropertyBatch
 - Added SingleNumberPropertyBinding to bind to any property type that can be converted or parsed to a double value
## [3.3.1] 2025-07-21
### Fixed BaseBinding exception when disabling binding in unbound state when it is not marked with AlwaysActiveForChange
## [3.3.2] 2025-07-21
### Added ActivityIsManagedByParent flag for IContext
## [4.0.0] 2025-07-23
### Removed DataView.Update method. Now updates will be called only if Context is implementing IContextUpdatable interface. This will trigger the lifecycle controlling component to be added by DataView
## [4.0.1] 2025-07-24
### Fixed Context type validation in DataView
## [4.0.2] 2025-07-25
### Injecting context before CurrentState is set
## [4.0.3] 2025-07-25
### Injecting context before CurrentState is set in all scenarios
## [4.0.4] 2025-07-27
### Improved reliability of ConditionalBindings
## [4.0.5] 2025-07-27
### Improved reliability of ConditionalBindings
## [4.0.6] 2025-07-27
### Improved reliability of ConditionalBindings
## [4.0.7] 2025-07-27
### Fixed pooled ui scale change
## [4.0.8] 2025-11-01
### Improved pooling reliability, pool cleaning on scene unload
## [4.0.9] 2025-11-01
### Improved error tracking for context state setting
## [4.0.10] 2025-11-15
### Introduced Context.Registrator adding code style sugar for property registration process
## [4.0.11] 2025-11-15
### Added GetRegistrator method to Context
## [4.0.12] 2025-11-30
### Added support for collection properties in ConditionBinding