# MstfRhinoPlugin
**Massive Surface Thickness Faker Plugin**
I have developed this RhinoCeros plugin to add several functionalities to working with RhinoCerso on Naval Architecture Projects; especially for weigth estimation and nesting processes.

This plugin has the following commands:

**mstf_SetThicknesToSurface :** sets thicknes value to selected surfaces. Caution! The unit of the thickness should be same as the unit system of the model. If the model is milimetric then the thickness also should be milimeter.

**mstf_MassCalculate :** for now just calculates volume and prompts the result to console. Mass calculation option will be available soon.

**mstf_GetCenterOfMAss :** calculates the center of the volume and adds a point on the location, also promts the value of the volume.  Mass calculation option will be available soon. 

**Mstf_NameSerialObjects :** Seçilen nesneleri seçildikleri sıraya göre bir ön isime eklenen tam sayılarla isimlendirir. Örn. Fr-1 Fr-2 Fr-3...

**Mstf_NameSerialObjects :** Seçilen ünesnelerin merkez noktasında nesnenin ismini gösteren bir TextDot nesnesini etiket olarak ekler

**Mstf_Help :** Shows the information about the plugin

## Adding Plugin to RhinoCeros

Insert the command "PluginManager" into the RhinoCeros command console

![image](https://user-images.githubusercontent.com/46689277/177102507-ac7f5b13-2206-4d55-939d-c803ab006ce1.png)

Click onto the Install button under the PluginManager window.

![RhinoManager_3](https://user-images.githubusercontent.com/46689277/177102729-f5fb7db8-f2e7-4903-be24-4cbbb52ab8f8.PNG)

Find the MstfRhinoPlugin1.rhp file and select, then quit by pressing OK button. The plugin methods should be ready to be used.
