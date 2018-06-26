PluginBarCommand.cs, PluginBarDialog.cs, PluginBarPlugIn.cs are created by default 
as a starter of the plugin.

>>>> ToolbarControl.cs <<<<
The top bar provudes the menu of all operations that user can apply. User can select 
a portion of the model and convert it into ondule spring design, preview the simulation 
of the spring deformation behabviors, and export the STL file.

>>>> PopUpPropertyWindow.cs <<<<
The pop-up window includes all spring parameters that the user can adjust for their
expected deformation behaviors. To-Do: integrated into the current rhino interface 
as a side bar.

>>>> EventWatcherHandler.cs (No Need to Change) <<<<
Register the event handlers. 

>>>> OnduleUnit.cs <<<<
Ondule spring unit, including all spring parameters and the deformation meta data.

>>>> Controller.cs <<<<
Register all functions for the rihno model. Connecting the UI components and the rhino model 
components.

>>>> RhinoModel.cs <<<<
Implementation of each functions defined in the UI.
