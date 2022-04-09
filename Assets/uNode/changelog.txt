# Changelog

## v2.1:
- Optimized State Graph code generation which will run more faster and has zero allocation after initialization.
- Now uNodeRuntime graph can execute OnDrawGizmos and OnDrawGizmosSelected when added to Game Object in the scene.
- Added UVS Parser add-ons: use to convert Unity Visual Scripting graphs into uNode graphs.
- Added smart connection features: hold 'shift' while moving nodes to use it.
- Added support for dragging port into Stackable node for creating block from a value port.
- Added new horizontal graph layout.
- Added Compilation Method for Runtime Graphs:
	- Unity: will save the generated script in 'Assets/uNode.Generated' folder and compiled natively with Unity.
  - Roslyn: will save the generated script in temporary folder and compiled using Roslyn Compiler, 
    - Advantage:	
      - Runtime Graphs can be generated & compiled in background.
      - Doesn't make Unity to Reload Domain each time c# scripts is generated.
      - Seamlessly integration with Fast Enter Play Mode.
      - Provides fast generation and compilation.
  	- Disadvantage: 
  		- Cannot debug with other IDE like Visual Studio for example.
- Added 'Add Node (Favorites)' menu for adding new nodes from favorites.
- Added support for the new Input System.
- Added First Item node
- Added Last Item node
- Added Count Items node
- Added On Button Input event
- Added On Keyboard Input event
- Added On Button Click event
- Added On Input Field Value Changed event
- Added On Input Field End Edit event
- Added On Dropdown Value Changed event
- Added On Toggle Value Changed event
- Added On Scrollbar Value Changed event
- Added On Scroll Rect Value Changed event
- Added On Slider Value Changed event
- Added On Pointer Click event
- Added On Pointer Down event
- Added On Pointer Enter event
- Added On Pointer Exit event
- Added On Pointer Move event
- Added On Pointer Up event
- Updated Odin Serialized to latest version.
- Updated Roslyn plugin to version 4.0.1
- Improved Code Generation: major refactoring, and optimizations.
- Improved Graph Event System
- Improved CSharpParser
- Improved Editor
- Fixed most of reported bugs

## v2.0.9:
- Added Exposed node ( can be created by right click on the output port )
- Improved CSharpParser
- Improved Code Generation
- Improved Editor
- Fixed most of reported bugs

## v2.0.8:
- Added new menu 'Script Documentation' to generate / update xml documentation for c# scripts / libraries with it's source code so you can see documentation for that code/libraries (required CSharpParser add- ons installed). menu location: 'Tools > uNode > Update > Script Documentation'
- Added new preference option 'Optimize Runtime Code' for optimizing Get / Set variable for Runtime Graphs when accessed from compatibility code.
- Added ability to edit getter/setter property modifier.
- Improved ItemSelector search: 
	- leveraging capitalization for quicker search e.g. writing 'GC' if you want 'GetComponent', or even allowing for partial words, such as 'GComInC' to find 'GetComponentInChildren' (thanks @EricRisbakk in discord for suggesting it)
	- Quick array search by typing with endings '[]' e.g. writing 'str[]' will show array of string, or writting 'str[] get' will show a Get method for array of string.
- Fixed most of reported bugs

## v2.0.7:
- Added auto convert when creating node from dragging a value port.
- Added 'Convert to GetComponent' node context menu for converting As / Convert node into GetComponent node.
- Added 'Reroute' node: use for re-route flow / value connection, can be created by double click on the edge / connection.
- Added 'Rename' context menu for macro node
- Added 'OnParticleCollision' event for runtime graph
- Improved Code Generation
- Improved Editor
- Improved ItemSelector
- Fixed cannot serialize Vector2Int
- Fixed cannot serialize Vector3Int
- Fixed most of reported bugs

## v2.0.6:
- Added 'Find All References' to find c# and graph: variable, property and function usages on all graph in the project.
- Added 'Find Node Usage' to find specific node usages in the project.
- Added 'Go to definition' from a graph type, field, property and function.
- Added 'Check All Graph Errors' for check all graph errors in the project.
- Added ability to move class variable to local variable and local variable to class variable.
- Added new preference for setting up new variable and function accessor ( public or private )
- Improved Code Generation
- Improved Editor

## v2.0.5:
- Added support for creating Generic Type for Runtime Graph Types in a graphs.
- Improved CSharp Parser
- Improved Editor
- Improved ItemSelector
- Fixed most of reported bugs

## v2.0.4:
- Added undo support when performing Place Fit Nodes
- Added support for debuging Macro for Runtime Graphs
- Added support for displaying State Graph in Graph Hierarchy
- Added visual debug values for proxy port
- Improved Editor
- Improved ItemSelector:
	- Added Favorites feature
	- Added ability to explore namespaces
	- Added ability to search item in all namespaces, specific namespaces, and on favorite namespaces
	- Added ability to navigate with keyboard
	- Improved search performance
- Fixed regions aren't moving nodes properly
- Fixed error on generating Set Value with debug mode
- Fixed some bug on performing Place Fit Nodes
- Fixed crash / freeze when patching graph
- Fixed graph attributes is not serialized properly
- Fixed annoying error message when deleting asset on newer unity version 
- Fixed most of others reported bugs

## v2.0.3:
- Added new Graph Hierarchy
- Improved CSharp Parser
- Improved Editor
- Changed initial node name from 'Start' to 'Entry'
- Fixed recursive detection bug
- Fixed visual debugging bug
- Fixed cannot using inherited protected members on c# graphs
- Fixed GUI tooltip is not displayed
- Fixed C# Graphs cannot add 'State Graph' events
- Fixed UI bug when using maximize and minimize

## v2.0.2:
- Now after importing CSharpParser add-ons, uNode will compile scripts using Roslyn Compiler
- Now uNode C# Graphs can use 'State Graph' if inherited from 'MonoBehaviour' or its sub classes
- Added misleading color
- Added support for fast Enter Play Mode 
- Added support for searcing deep member for Runtime Type ( Class Component, Class Asset, Class Singleton, and Graph Interface )
- Added option to auto compile graph on entering play mode ( the compiled graphs will not saved in your project 'Assets', it will be saved in temporary folder for debugging purpose )
- Added overflow / recursive node detection
- Improved Code Generator: optimization for state graph code generator which will produce better code in term of performance and better readable ( you can enable from preference )
- Improved Editor
- Changed port that's connected to nodes now doesn't display a name
- Fixed errors on generating script when 'Realtime C# Preview' window opened
- Fixed cannot build & run
- Fixed error when trying to clean temporary graphs on build
- Fixed error on generating project + scenes
- Fixed error when generating node convert to Runtime Type if using compatibility generation mode
- Fixed bug cannot convert int, byte, short, and long to System.Enum
- Fixed bug cannot create macro asset from project view
- Fixed bug cannot create node by drag & droping Graph Interface
- Fixed some UI bugs

## v2.0.1:
- Now change function return type, and modifying function parameters will update the node that's referencing the function
- Added finished flow output for node group
- Added context menu to open group node
- Fixed when converted to selection macros, nodes are sent off far from mouse position
- Fixed cannot open state node and group node by double click on the node
- Fixed adding function parameter doesn't update the c# preview
- Fixed error on converting multipurpose / reflection node to 'Condition' block

## v2.0:
- Stable version.

## v2.0b13:
- Added option to enable / disable: Auto C# Generator on Build in individual graph and global settings
- Changed the 'default' theme.
- Fixed bug on drag & droping graph asset to uNode Editor
- Fixed some UI bugs
- Fixed most of reported bugs

## v2.0b12:
- Added new theme "Dark Gray"
- Added ability to rename variable, property, and function with "alt + left click" in the graph panel 
- Improved Editor
- Improved Graph editor performance
- Changed double clicking variable, property, and function now show rename window instead of inspect
- Fixed most of reported bugs

## v2.0b11.1:
- Fixed bug cannot register or unregister event when a port connected to value node
- Fixed bug convert node error on generating convert code with string type
- Fixed bug error on creating constructor node
- Fixed bug error on displaying constructor node

## v2.0b11:
- Added ability to refactor missing c# member ( field, event, property, and function )
- Added ability to compile referenced c# graphs after renaming c# graph member ( variable, property, and function )
- Added ability to duplicate property, function and constructor in graph panel
- Added ability to compile all c# graphs in uNode Editor
- Improved Code Generation
- Improved ErrorChecker
- Improved Error Recovery
- Improved Editor
- Fixed some UI bug found on Unity 2019.4
- Fixed bug Console Integration not work for the generated c# scripts
- Fixed bug promoting port into variable or local variable doesn't repaint the graph panel
- Fixed state graph bug when run using native c# the flow node always wait one frame before executing next flow, this causes inconsistencies when running graphs between the native c# and reflections.
- Fixed other reported bugs

## v2.0b10:
- Added new window for easily creating new graphs: "Tools > uNode > Create New Graph"
- Added Freehand node selector:
	hotkey: "Shift + Left Click" to select nodes
	hotkey: "Shift + Alt + Left Click" to remove nodes and port connections
- Improved Node Snapping
- Improved Editor
- Fixed some bugs

## v2.0b9:
- Improved CSharpParser
- Improved Editor
- Changed API uNodeSpawner: InvokeFunction(name) to ExecuteEvent(name) this should fix annoying message when using AnimationEvent 
- Fixed most of knowed and reported bugs

## v2.0b8:
- Now uNode uses OdinSerializer for serializing the graphs
- Added support to parse event declaration
- Added support to parse event attributes
- Added local variable support for graph commands
- Added parameter support for graph commands
- Improved Reflection Performance
- Improved Runtime Graph Instantiate Performance
- Improved Code Generation
- Improved Editor
- Improved Graph Converters
- Changed API uNodeSpawner: ExecuteFunction(name) to InvokeFunction(name) 
- Fixed bug parsing nested type members
- Fixed bug error on generating variable with deep members
- Fixed bug error on creating node from parameter function with deep members
- Fixed bug when searching node, the highlight line is never hide
- Fixed bug sometimes the graph is not repaint after doing undo / redo
- Fixed bug renaming local variable doesn't repaint the graph and doesn't rename the whole reference.

## v2.0b7:
- Now graph namespace is now handled for searching member, so no need to add using namespace 'a' when you have graph with namespace 'a' or 'a.b' etc.
- Added support to parse 'nameof()' syntax
- Improved Editor
- Fixed insonsistent line endings warning 
- Fixed most warnings regarding USS found on Unity 2019.4

## v2.0b6:
- New Graph Interface: use for inheritance support for the runtime graph ( Class Component & Class Asset ) it is work just like c# interface
- Improved Editor
- Fixed Console Integration sometimes does not working
- Fixed bug after renaming function or property the graph panel is not refreshed
- Fixed bug after generating project script the graph still using reflection instead of native c#
- Fixed bug cannot edit using namespace in uNodeClass and uNodeStruct
- Fixed bug cannot save graph when opening uNode editor using 'Edit Target' from uNodeSpawner and uNodeAssetInstance
- Fixed error on creating node by drag & dropping local variable

## v2.0b5.1:
- Fixed 'Set as Start' menu couldn't update the graphs 
- Fixed renaming property and function don't update the graphs immediately
- Fixed error on build when using uNode Runtime, Class Component and Class Asset graphs
- Fixed property getter and setter can't access graph members ( variable, property, function, and its inherith members )
- Fixed graph 'using namespaces' is not handled by ItemSelector
- Fixed unassigned target bug on creating node by drag & drop property
- Fixed error on generating lambda code in state graph because the generator allowing yield statements
- Fixed inconsistency generated code on generating selector, sequence, radom selector, and random sequence nodes 
- Fixed bug after build an executable the graph still using reflection instead of native c#
- Fixed some UI bugs
- Fixed some Code Generation bugs

## v2.0b5:
- New High Level API for creating nodes, actions, and conditions
- Now Event in State Graph is displayed in the graph panel
- Added Lambda node: a simplified version of anonymous function which don't need to use return node and more easier to use
- Added namespace organization support for Class Component and Class Asset graphs
- Improved CSharp Generator
- Improved Editor
- Improved ItemSelector
- Fixed some bugs
- Fixed some warnings

## v2.0b4:
- New uNode Component Singleton: a new graph that's work just like Monobehaviour singleton in C# and all of its members (variables, properties and functions) can be accessed without assigning the instance
- Added support to edit Vector2Int, Vector3Int, Gradient, and LayerMask value
- Added support to use attribute to display or decorate a variable value the currently supported attribute are:
	- RangeAttribute for float and int
	- TextAreaAttribute for string
	- ColorUsageAttribute for Color
	- GradientUsageAttribute for Gradient
	- SpaceAttribute
	- HeaderAttribute
- Improved CSharp Preview window: It is now possible to check a errors or warnings by compiling the scripts and can highlight the node from error or warning information
- Improved CSharp Generator
- Improved Editor
- Improved ItemSelector
- Improved NodeBrowser
- Mark GlobalVariable to obsolete and will no longer use in newer version as it now replaced by the new Singleton graphs
- Fixed some bugs

## v2.0b3:
- Now renaming variable, property, and function will refactor all reference graphs so after renaming there is no missing member
- Improved AutoSave graph: adding / renaming variable, property, and function will auto save the graph
- Improved Editor
- Fixed some UI bug on Unity 2019.3+
- Fixed some bugs

## v2.0b2:
- New Unity Console Intergration: when running using generated script whatever with debug mode or not, clicking any log in the Console (info, warning, or error) will highlight the flow node and when running using runtime graph and an error was throw, clicking on the error log will highlight the node that is throw the errors
- Improved CSharpGenerator
- Improved C# Preview: It is now possible to click on the script and select & highlight the nodes, variables, properties, and functions that's generating the script
- Improved Graph Editor Performance: Now uNode will cache the node views instead of refreshing it every time when doing an action
- Fixed some bugs
- Fixed some warning

## v2.0b1:
- New uNode Class Asset a new graph that's work just like ScriptableObject and can be run without need to compile into script
- New uNode Asset Instance a instance of a new Class Asset graph just like asset of a ScriptableObject
- New uNode Class Component a new graph that's work just like MonoBehaviour and can be run without need to compile into script
- New Runtime Type that's created from a Class Component or Class Asset graph and can be used for referencing a graph
- Now uNode will auto generate c# script on build so the exported game will have native c# performance and supporting of all target platform
- Added ability to convert graph into another graph type
- Added ability to convert node into block by dragging the node into block nodes
- Added preferred display value to choose the display of input port inside or outside node
- Added preference to set up generation mode: Default, Performance, or Compatibility
	- Default: using individual graph setting and force to be pure when the graph use default setting
	- Performance: generate pure c# and get the best in term of performance but it may give errors when other graph is not compiled into script
	- Compatibility: generate c# script that's compatible with all graph even when other graph is not compiled into script
- Added new menu to compile all graphs into c# script
	- Tools > uNode > Generate C# Scripts - will compile all project graphs into c# scripts with exception manually compiled graph will be skipped
	- Tools > uNode > Generate C# including Scenes - compile all project graphs and will find all graph in all scene in build settings into c# scripts
- Improved CSharpParser
- Improved CSharpGenerator
- Improved Graph Inspector
- Improved Item Selector
- Improved uNode Editor
- Improved uNodeSpawner, now it can run graph using c# script when the target graph has been compiled into c# and supporting new Class Component graph
- Major code refactoring and its now include full source code
- Moved most c# generation setting from individual to global setting
- Moved the minimum required version of Unity to 2019.1
- Removed Horizontal / Native graph theme and makes the Vertical graph to be default graph
- Fixed most of knowed and reported bugs

## v1.8.7:
- Added ability to change node display ( Default, Partial, Full )
- Added ability to inspect any element using 'Shift' + 'Left Click'
- Improved Editor
- Changed Type Patcher now only work on hitting 'Compile' button instead of 'Save' in playmode
- Fixed graph can't keep changes on saving in play mode
- Fixed some bugs

## v1.8.6:
- Added ability to import graph from json
- Added ability to export graph to json
- Improved Editor
- Improved Type Builder
- Fixed variable node return type won't change after changing the variable type
- Fixed proxy dot won't show in the output port and input port from the block node
- Fixed can't add new attribute
- Fixed error on generating KeyValuePair type

## v1.8.5:
- Added support of displaying general node in node browser
- Added ability to drag & drop node browser type, variable, property, and function to block system
- Added ability to drag & drop graph explorer variable, property, and function to block system
- Added ability to exlude namespace & type in node browser
- Added ability to find node and pin type in node browser from a node on uNode editor
- Improved Graph Panel for Vertical Graph
- Improved Error checker
- Removed dependencies of uNode on generating Event node with multiple target
- Fixed bug can't change function type to 'void'
- Fixed some UX bug on Unity 2019.3
- Fixed some bugs

## v1.8.4:
- Added Node Browser allow to create node by drag & drop
- Added Graph Explorer allow to explore graph in the project
- Added Type Patcher allow to debug and patch (modify graph and compile without domain reload for prototyping purpose) generated script in playmode
- Added auto save graphs on rename (variable, property, and function)
- Added ability to create liked macro by drag & drop macro asset to graph
- Added ability to edit uNode when double clicking the uNode asset in the project panel
- Added ability to Take Screenshot of the Graph with high quality image (2X zoom)
- Changed auto save to only work in editor, use 'Save' button to save graph in the playmode
- Changed when editing graph in playmode any changed will be reverted on exiting playmode
- Fixed bug that makes documentation doesn't load Unity API
- Fixed bug that makes Realtime C# Preview doesn't repaint automatically on change value and connect node in the graph
- Fixed bug uNode Editor loses focus when entering / exiting playmode or closing uNode Editor window
- Fixed bug wrong position on creating linked node
- Fixed bug error generating code when linked node have variable which aren't initialized 
- Fixed error on removing node while in play mode
- Fixed some warning

## v1.8.3:
- Added Macro features
- Added ability to search graph nodes by:
	- name, 
	- type (block, event, variable, property, function, constructor, literal, value, and proxy), 
	- port name, 
	- port type
- Major code refactoring
- Fixed some bugs

## v1.8.2:
- Added ability to move graph canvas with right click
- Added ability to change function node overloads with right click
- Added EventHook node, used to easily hook C# Event / UnityEvent
- Improved C# Parser
- Improved CSharpGenerator
- Improved Editor
- Improved Vertical graph performances
- Fixed MemberData value are not being updated on Undo/Redo
- Fixed UI bug on Vertical graph

## v1.8.1:
- Improved Editor
- Added ability to add arithmetic & comparison node on dragging pin
- Added ability to add supported flow node on dragging pin
- Added ability to safe change operator type in Arithmetic node.
- Fixed some Vertical graph UI bugs in Unity 2019.2
- Fixed bug error on promoting null value to node

## v1.8:
- Improved Editor
- Added command to convert supported flow node to Action node
- Added command to convert supported value node to Condition node
- Added Place fit nodes features for Vertical graph
- Fixed some bugs

## v1.8b9:
- Added Welcome window
- Improved Editor
- Improved Graph Commands
- Fixed some bugs

## v1.8b8:
- Added minimap for Vertical graph
- Added debug visualization for Vertical graph
- Added support deep access for get, set, or invoke member of function parameters
- Improved Editor
- Fixed some bugs

## v1.8b7:
- Added ability to snap node to pin for Vertical graph, enable/disable it in preferences
- Added ability to find or edit block script by right click
- Added ability to add 'OR' block on condition by right click
- Added ability to expand/colapse all block by right click on Block node
- Added recently item in ItemSelector, only for Static types/members
- Added EqualityCompare, IsCompare, and ObjectCompare condition scripts
- Mark EqualityComparer, IsComparer and ObjectComparison to obsolete and will no longer use in newer version as it not support methods with many parameters
- Improved Editor
- Fixed some bugs

## v1.8b6:
- Now vertical graph is using asset file so users can makes new theme for vertical graph
- Added colored node title on Vertical graph
- Added colored block title for action & condition
- Added proxy name for transition in Vertical graph
- Improved Editor
- Removed Power operator, use UnityEngine.Mathf.Pow instead
- Fixed some bugs

## v1.8b5:
- Added Support region for Vertical graph
- Added ability to rename transition by double click on transition block (Vertical Graph)
- Added store parameter value for StateEventNode and TransitionEvent
- Added StickyNote
- Added Once node
- Added Timer node
- Added Multi AND node
- Added Multi Arithmetic node
- Added Multi OR mode
- Mark ANDNode, ArithmeticNode and ORNode to obsolete.
- Improved CSharpGenerator
- Improved Editor
- Fixed some bugs

## v1.8b4:
- Added Support State Machine for Vertical graph
- Added new block design for action & condition system in vertical graph
- Added automatic port order based on target position (only flow outputs) for vertical graph
- Added ability to convert connection to proxy for flow in vertical graph
- Improved C# Parser
- Improved Vertical graph
- Improved Editor
- Fixed inspector can't edit region node
- Fixed some bugs related to prefab
- Fixed some bugs

## v1.8b3:
- Added auto convert incorrect port for Vertical graph
- Added support State Flow graph type for Vertical graph
- Added ability to reorder variable, property, and function by drag & drop in uNode Editor
- Added error badge for Vertical graph
- Improved Vertical graph
- Improved Editor
- Fixed some bugs

## v1.8b2:
- New Vertical Graph theme
- Added ability to open recently files
- Improved C# Parser
- Improved Editor
- Improved ItemSelector
- Fixed some bugs

## v1.8b1:
- New EventSheet feature [BETA]
- Added C# Format features
- Added Control Point features
- Added Auto Convert value pin features, triggered on type from source pin is not match with type from target pin when making connection
- Added auto save graph (asset or prefab) on compile, preview c# script, and on closing Unity Editor
- Improved C# Parser
- Improved Editor
- Improved ErrorChecker
- Improved ItemSelector
- Fixed bugs when parsing c# script makes unity crash due to stack overflow
- Fixed bugs parsing for loop when using loop index from variable, or property
- Fixed bugs item not being sorted in ItemSelector
- Fixed bugs editor not refreshed on adding constructor
- Fixed bugs editor not refreshed on adding property
- Fixed bugs implicit operator doesn't work on set variable values
- Fixed some editor bugs

## v1.7.5:
- Added inspect context menu for variable, property, function, and nodes
- Added 'Show Pin Icon' preference for theme
- Improved C# Preview
- Improved Editor
- Improved ErrorChecker
- Improved ItemSelector
- Improved Editor Preferences
- Improved compatibility for Unity 2018.3+
- Fixed bugs on dragging object to canvas the menu doesn't show
- Fixed bugs need to save generated script manually for asset and prefab
- Fixed error on realtime preview c# script that's contains property
- Fixed bugs editor not refreshed on adding node using context menu
- Fixed bugs editor not refreshed on adding node by drag & drop functions
- Fixed bugs editor not refreshed on splitting nodes
- Fixed bugs native theme doesn't show region box correctly
- Fixed bugs value output has incorrect target on duplicating node

## v1.7.4:
- New command popup allow for find type or adding nodes
- New GraphAsset, allow saving uNode Graph in a .asset file (ScriptableObject)
- Improved CSharpGenerator
- Improved Editor
- Improved compatibility for Unity 2018.3
- Added support to edit uNode (prefab) for the new prefab system.
- Fixed bugs parsing enum values when enum type is within a class
- Fixed bugs parsing invoke field delegate
- Fixed bugs parsing Nullable<T> types
- Fixed bugs parsing nullable types (bool?, float?, int?, etc)
- Fixed bugs error on generate nodes get/set properties
- Fixed bugs error on generate SetValue node with null value
- Fixed some bugs on Unity 2018.3
- Fixed editor bugs initializer node always dimmed
- Fixed some bugs.

## v1.7.3:
- New uNode Hierarchy
- Improved CSharpGenerator
- Improved CSharpPreviewWindow
- Improved Editor
- Improved ErrorChecker
- Reorganize and restructure core scripts
- Removed legacy network event on uNodeRuntime which aren't work anymore on Unity 2018.3
- Fixed bugs uNode Runtime won't run
- Fixed bugs editor red text on Unity 2018.3
- Fixed bugs error on editing Struct type
- Fixed some bugs.

## v1.7.2:
- New realtime C# Preview Window.
- Added PerSecond value node
- Improved CSharpGenerator
- Improved CSharpPreviewWindow
- Improved Editor
- Improved ErrorChecker
- Fixed bugs parsing property set accessor
- Fixed bugs error on parsing enum type outside of source
- Fixed bugs ErrorChecker error on check divide operator
- Fixed bugs ErrorChecker error on check SetValue node
- Fixed bugs error on generating property and constructor
- Fixed warning related to null texture on draw node

## v1.7.1:
- Added support to generate code for EventNode with multiple target objects
- Improved CSharpGenerator
- Improved Editor
- Improved ErrorChecker
- Fixed parsing enum values
- Fixed bugs parsing SwitchStatement
- Fixed ErrorChecker bugs on NodeSetValue
- Fixed coroutine action can be added on non coroutine action
- Fixed some editor bug

## v1.7:
- Now coroutine is supported in action system
- Added AnimationPlay action
- Added AnimatorPlayAnimation action
- Added GameObjectFindClosestWithTag action
- Added NavMeshAgentFlee action
- Added NavMeshAgentFollow action
- Added NavMeshAgentPatrol action
- Added NavMeshAgentSeek action
- Added NavMeshAgentWander action
- Added YieldWaitForSeconds action
- Added YieldWaitForEndOfFrame action
- Added YieldWaitForFixedUpdate action
- Added New Dark and Light graph editor themes
- Improved Action System
- Improved CSharpGenerator
- Improved Editor
- Improved ItemSelector
- Improved ErrorChecker

## v1.6.7:
- Added preference to hide node comment
- Added FlowToggle.cs
- Improved CSharpGenerator
- Improved Editor
- Improved ErrorChecker
- Improved uNodePreference
- Improved core script for further development
- Moved node namespace from MaxyGames.uNode to MaxyGames.uNode.Nodes
- Fixed some editor bug

## v1.6.6:
- Added ability to craete variable by dragging UnityObject
- Added support to parse 'yield break' syntax
- Added support to parse LeftShiftExpression
- Added support to parse RightShiftExpression
- Added NodeYieldBreak.cs used to generate yield break
- Added Region node box used to organize the node
- Added commonly unused namespace to exclude it in item search by default (can be managed from preference)
- Added SmoothLookAt.cs action used to smooth look at object
- Added SmoothMove.cs action used to smooth move an object
- Added TransfromRotate.cs action used to rotate an object
- Added TransfromTranslate.cs action used to move an object
- Improved CSharpGenerator
- Improved Editor
- Improved ItemSelector
- Improved ErrorChecker
- Fixed bug parsing null values
- Fixed bug parsing commentaries
- Fixed bug parsing increment/decrement syntax
- Fixed bug parsing source generic method
- Fixed bug parsing type with full namespace
- Fixed bug RandomSelector won't call flow in random order
- Fixed bug could't invoke constructor with parameter
- Fixed bug on splitting node
- Fixed error on converting value between delegate type
- Fixed error on c# type, field, property or function name was changed (only for editor)
- Fixed some editor bug

## v1.6.5:
- Added new template feature.
- Added ability to create uNode class/struct from template
- Added ability to export uNode class/struct to template
- Added ability to make anonymous function by right- clicking on pin which has delegate type
- Added support to parse event
- Added support to parse using statement
- Added support to parse ref/out parameters
- Added support to parse method attributes
- Added support to generate ref/out parameters
- Added FlowInput class used to make custom flow input
- Improved Editor
- Improved core script for further development
- Improved node search
- Improved ItemSelector
- Improved CSharpGenerator
- Fixed some bugs

## v1.6.4:
- Added preference to prevent editing prefab on play (true by default)
- Added ability to change method target for MemberData
- Added ability to change constructor target for MemberData
- Added ability to set output pin value by right click the pin
- Added support to add or raise event
- Added NodeUsing.cs used to generate using statement
- Added Editor error checker used to predicted an error before generating C# or running uNode
- Improved Editor
- Improved various node to support error checker
- Fixed some bugs

## v1.6.3:
- Added ability to change method target for MultipurposeNode
- Added ability to change constructor target for MultipurposeNode
- Improved Editor
- Fixed some bugs

## v1.6.2:
- Added support to parse ThrowStatement syntax
- Added LoadScene action
- Added RestartScene action
- Improved Editor
- Fixed some bugs

## v1.6.1:
- Added F10 hotkey to compile
- Added F9 hotkey to preview generated code
- Added ability to disable auto hide inspector panel in preference
- Added Preview button to preview generated code
- Added description on some actions
- Improved Editor
- Fixed some bugs

## v1.6:
- Added support to generate generic classes and struct.
- Added support to parse Nested Types.
- Added ability to select type from generic parameter for types and functions.
- Added ability to save parsed uNode to prefab on CSharpParserWindow (only for parsing MonoScripts).
- Added ability to open menu by pressing Space key on the uNodeEditor canvas.
- Added NodeLock.cs used to generate lock statement.
- Added SelectNode.cs used to select one node from many values just like a switch but for value.
- Added about window.
- Improved Editor GUI
- Improved uNode Editor performance.
- Improved uNodeEditor
- Improved CSharpParser
- Improved CSharpGenerator
- Improved VariableData
- Improved MemberData
- Improved ItemSelector
- Fixed some bugs
- And much more

## v1.5.4:
- Added support to parse Generic MethodDeclaration syntax.
- Added support to parse typeof(T) syntax.
- Added support to parse Is expression.
- Improved CSharpGenerator, dramatically increase generating times.
- Fixed uNodeEditor auto focus on selection changed.
- Fixed wrong on displaying arrays of generic type.
- Fixed error on creating generic type with parameter more than one and using generic parameter.
- Fixed error on generating ISNode.
- Fixed some editor bugs

## v1.5.3:
- Added uNodeConsole which improve debugging by quickly jump to the error node.
- Added Succeeder node
- Added Failer node
- Added ability to edit function parameter type.
- Added ability to edit generic parameter type constraint.
- Added support to parse comment in statement expression.
- Improved JsonHelper
- Improved uNodeEditor
- Improved CSharpParser
- Improved CSharpGenerator
- Fixed parsing extension method with same name.
- Fixed editor not repaint when editing value in unity inspector.
- Fixed node hide when comment are too many.
- Fixed error on invoking some generic function.
- Fixed error on calling initializer when value created using MemberData.
- Fixed error on drawing generic function node.
- Fixed can't create array parameter using generic parameter.
- Fixed can't create generic type parameter node.
- Fixed error on generating typeof(T) code.
- Fixed some bugs

## v1.5.2:
- Added preference to hide object generated by uNode (true by default).
- Added preference to enable unity inspector integration (false by default), used to integrate uNode selection right to the unity inspector.
- Added support to generate initializer for Attribute
- Added support to generate initializer for Constructor
- Added support to parse Attribute named parameters
- Added support to parse ObjectCreation initializers
- Improved StateMachine editor connections.
- Improved ConstructorValueData
- Improved AttributeData
- Improved CSharpGenerator
- Fixed HashSet editor
- Fixed some editor bugs

## v1.5.1:
- Added preference to show Obsolete item (false by default).
- Added support to parse element access expression.
- Added support to parse array object creation.
- Added support to parse array initializer.
- Added support to parse using namespace.
- Added support to parse enum type.
- Added support to parse interface type.
- Added support to parse anonymous function ex: ('delegate() { someAction; }', '() => someAction;' with or without parameter).
- Added array length in MakeArrayNode for dynamically create array with varius length.
- Improved ValueData
- Improved ItemSelector
- Improved AttributeData
- Improved VariableEditor
- Improved CSharpGenerator
- Improved CSharpParser
- Improved uNodeEditor
- Fixed arithmetic operator error when handling different primitive type.
- Fixed conditional operator error when handling different primitive type.
- Fixed parsing inherith class/struct.
- Fixed parsing some generic method.
- Fixed parsing extension method.
- Fixed parsing generic extension method.
- Fixed parsing logical and.
- Fixed parsing logical or.
- Fixed parsing null value.
- Fixed parsing yield return.
- Fixed parsing empty statement.
- Fixed parsing return without parameter.
- Fixed parsing negative number.
- Fixed error on generating static field.
- Fixed error on generating array with "Get"/"Set" member.
- Fixed error when connecting pin to ouput value pins.
- Fixed editor error on drawing some generic method.
- Fixed editor wrong showing an attribute.
- Fixed node error when targeting deep member with multiple parameter.
- Fixed ItemSelector can't select get_Item member.
- Fixed ItemSelector can't select set_Item member.
- Fixed ItemSelector error on select some generic method.
- Fixed ForNumberLoop node gui bugs.
- Fixed MakeArrayNode error when element of the array are zero.
- Fixed generating scring with escape sequences character.
- Fixed error when using Add operator to add a string with another type.
- And much more

## v1.5:
- Added support to generating new enum type.
- Added support to generating new interface type (support property, function, and generic function).
- Added support to generating nested type (classes, struct, enums, and interfaces).
- Added StopFlowNode.cs used to force stop running nodes.
- Added InterfaceProperty
- Added InterfaceFunction
- Improved Editor
- Improved CSharpGenerator
- Improved FieldEditorWindow
- Changed uNodeInterface to InterfaceData
- Fixed Repeater error on generating c# output.
- Fixed ItemSelector search not auto focus on some unity version.
- Fixed some editor bugs

## v1.4.4:
- Added promote to local variable in input context menu.
- Added store instance to variable in node context menu.
- Added store instance to local variable in node context menu.
- Added auto convert delegate type when assign, add, remove a delegate.
- Removed Overflow checking for value pin.
- Fixed error on generating local variable.
- Fixed can't connect delegate type pin to delegate value when the type is not same.
- Fixed CSharpParser bugs parsing parameter with ref/out keyword.
- Fixed ItemSelector not show some item when you want to create new node from value pin.
- Fixed some bugs.

## v1.4.3:
- Now support nested StateMachine (State under State).
- Now StateMachine can be grouped using GroupNode.
- Added Split Node to split MultipurposeNode(deep member) into several node.
- Added ParallelControl.cs used to execute node without waiting the node (only on State Graphs).
- Added progress bar on generating script.
- Improved GroupNode.
- Improved StateNode.
- Improved some of editor script.
- Fixed ItemSelector when change the type of search some time make editor freeze.
- Fixed ItemSelector not show uNode member on TransitionEvent component.
- Fixed some gui bugs.

## v1.4.2:
- Added promote to node in input context menu.
- Added promote to variable in input context menu.
- Added NodeAnimateFloat.cs
- Added DefaultNode.cs used to generate default(T) keyword.
- Added ConditionalNode.cs used to generate conditional (?:) operator.
- Added CoalescingNode.cs used to generate coalescing (??) operator.
- Added support to parse default keyword.
- Added support to parse conditional (?:) operator.
- Added support to parse coalescing (??) operator.
- Fixed bugs on parsing method when parameter has keyword "params"
- Fixed bugs on parsing constructor when parameter has keyword "params"
- Fixed bugs on generating constructor
- Fixed bugs on generating AnimationCurve
- Fixed NodeWaitForSecond.cs won't generate next flow node.
- Fixed bug null value after creating value type variable
- Fixed GUI bugs can't assign member instance
- Fixed GUI bugs when entering play mode the uNode editor goes wrong
- Fixed GUI bugs after deleting StateEventNode

## v1.4.1:
- Added In Editor Documentation used to find xml documentation of member inside node and display it in inspector
- Added double click on variable button to rename variable
- Added double click on property button to rename property
- Added CTRL + C shortcut to copy node
- Added CTRL + V shortcut to paste node
- Improved Editor
- Improved uNode Editor Performance
- Improved uNodePreference
- Improved ItemSelector search filtering
- Improved ItemSelector type/member filtering
- Fixed CSharpGenerator error on generating for loop statement when the node generated from CSharpParser
- Fixed CSharpParser error on parsing for loop statement
- Fixed event line still show in another graph
- Fixed node always dim inside group
- Fixed some error when doing live editing
- Fixed some GUI bugs.

## v1.4:
- Brand new StateMachine System which more powerful and more easy to use.
- Added CSharpParser[BETA] which you can used to parse c# into uNode (required Unity 2018.1 with .Net Standard 2.0)
- Added support to define multiple target object for EventNode
- Added support to store parameter value from specific event for EventNode
- Added IncrementDecrementNode.cs used to generate pre Increment/Decrement or post Increment/Decrement.
- Added NegateNode.cs used to generate negation.
- Added StateEventNode.cs used to make event for StateMachine.
- Added uNodePreference
- Added Preference Editor Tool in uNodeEditor
- Added commentary for node.
- Added Condition Transition
- Added OnApplicationFocus Transition
- Added OnApplicationPause Transition
- Added OnApplicationQuit Transition
- Added OnBecameInvisible Transition
- Added OnBecameVisible Transition
- Added OnCollisionEnter Transition
- Added OnCollisionEnter2D Transition
- Added OnCollisionExit Transition
- Added OnCollisionExit2D Transition
- Added OnCollisionStay Transition
- Added OnCollisionStay2D Transition
- Added OnDestroy Transition
- Added OnDisable Transition
- Added OnEnable Transition
- Added OnMouseDown Transition
- Added OnMouseDrag Transition
- Added OnMouseEnter Transition
- Added OnMouseExit Transition
- Added OnMouseOver Transition
- Added OnMouseUp Transition
- Added OnMouseUpAsButton Transition
- Added OnTimerElapsed Transition
- Added OnTransformChildrenChanged Transition
- Added OnTransformParentChanged Transition
- Added OnTriggerEnter Transition
- Added OnTriggerEnter2D Transition
- Added OnTriggerExit Transition
- Added OnTriggerExit2D Transition
- Added OnTriggerStay Transition
- Added OnTriggerStay2D Transition
- Improved Editor
- Improved PlaceFit Tools
- Improved Support for Coroutine
- Improved C# Code Generation
- Improved uNode Runtime Performance
- Improved ItemSelector Performance
- Improved StateMachine for better understanding and fast creation of state
- Fixed some gui bugs when drawing MemberData that targeting Values
- Fixed error when using ISNode
- Fixed error when converting type using ASNode
- Fixed error when invoking generic method
- Fixed error when invoking constructor
- Fixed error when generating type conversions
- Fixed error when generating function/constructor parameter
- Fixed bug can't set value in field/property/uNodeVariable inside value type.
- Fixed bug incorrect some values after exporting uNode.
- Fixed ItemSelector incorrect select deep item.
- Fixed TypeBuilder can't build generic type when parameter more than 4.
- And much more

## v1.3:
- Now support move the canvas using right mouse button.
- Now support for generating summary for class, variable, property, method, and constructor.
- Now support for editing UnityEngine.Color32 just like UnityEngine.Color
- Added NodeAnonymousFunction.cs used for generating Anonymous Method and its supported at runtime
- Added MakeArrayNode.cs used for easy creating array.
- Improved Editor for showing the index of extension node and dim the node if its not connected.

## v1.2:
- Updated FullSerializer.dll to latest version.
- Added set value support from value node.
- Added NodeTry.cs
- Improved uNode Editor.
- Improved DoWhileLoop.cs  (Added gui in node and removed validation)
- Improved ForeachLoop.cs  (Added gui in node)
- Improved ForNumberLoop.cs (Added gui in node and removed validation)
- Improved WhileLoop.cs  (Added gui in node and removed validation)
- Improved ASNode.cs (Now support value type, value type will using convert operator)
- Fixed crash when doing make connection.
- Fixed FullSerializer can't serialize System.Char type.
- Fixed DoWhileLoop wrong generating code.
- And much more

## v1.1:
- Now Support Generating C# Properties(auto properties or with getter/setter) in both class or struct.
- Now Support Generating C# Constructors with/without parameters in both class or struct.
- Added NodeSwitch.cs
- Added LocalVariable Support for Function, Constructor, and Property(getter/setter).
- Added DragAndDrop variable, property, or local variable to canvas for fast creating node to get/set variable.
- Added DragAndDrop function to canvas for fast creating node to invoke function.
- Added DragAndDrop UnityEngine.Object to canvas for fast creating node to get or invoke value.
- Added DragAndDrop variable, property, or local variable to Action/Validation for fast creating event to get/set variable.
- Added DragAndDrop function to Action/Validation for fast creating event to invoke function.
- Added DragAndDrop UnityEngine.Object to Action/Validation for fast creating event to get, set, or invoke value.
- Added PlaceFit Tools used to auto arrange nodes position.
- Improved Documentation.
- Improved uNode Editor performance.
- Changed TypeSelectorWindow to TypeBuilderWindow.
- Moved EventCoroutine class to RuntimeSMHelper.cs file and remove EventCoroutine.cs file.
- Fixed bugs when generating abstract method.
- Fixed bugs when generating inherith member "this" will be "base" for all inherith member.
- Fixed bugs on creating function in prefab the modifier is not correct.
- Fixed gui bugs in some of node.
- Fixed bugs TypeBuilder showed null type.
- Fixed ItemSelector can select type when can only select instance item.
- Fixed ItemSelector can select custom item and this item that are not valid type/target.
- Fixed can't paste node when uNode object is not prefab.
- And much more

## v1.0:
- First Released