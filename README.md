# Siasun-Template-Matching
The template matching algorithm for Siasun Robotics

affiliate with dajuric's image processing library and algorthim at
https://github.com/dajuric/accord-net-extensions

 <a href="https://github.com/dajuric/accord-net-extensions/raw/master/Deployment/Documentation/Help/Accord.NET%20Extensions%20Documentation.chm"> 
 <img src="https://img.shields.io/badge/Help-download-yellow.svg?style=flat-square" alt="Help"/>  </a>

First things first:
USING THE LIBRARIES: 
Three of the libraries are included----

### Image processing

* __Accord.Extensions.Imaging.Algorithms package__      
 Implements image processing algorithms as .NET array extensions including the Accord.NET algorithms.

### Math libraries

* __Accord.Extensions.Math package__    
  Fluent matrix extensions. Geometry and graph structures and extensions.

### Support libraries

* __Accord.Extensions.Imaging.AForgeInterop package__     
 Interoperability extensions between .NET array and AForge's UnmanagedImage.

 
  
### __in order to use the libraries, TYPE in the package manager:__

    PM> Install-Package Accord.Extensions.Imaging.Algorithms -Version 3.0.1
    PM> Install-Package Accord.Extensions.Math -Version 3.0.1
    PM> Install-Package Accord.Extensions.Imaging.AForgeInterop -Version 3.0.1

## Basic usage:

In the `Basic Methods` section, two main functionalities are implemented:

### #1 build templates:

* __build template from a image__

  ```C#
          public static List<TemplatePyramid> buildTemplate(Gray<byte>[,] image, int Width, int Height, bool buildXMLTemplateFile = false, int angles = 360, int sizes = 1, float minRatio = 0.6f, int[] maxFeaturesPerLevel = null)
  ```
  build the template with a specific image
  
* __build template from file(s)__

  ```C#
          public static List<TemplatePyramid> fromFiles(String[] files, bool buildXMLTemplateFile = false, int angles = 360, int sizes = 1, int[] maxFeaturesPerLevel = null)
  ```
  build the templates with files in the list
  
  **OR simpler:**
  
  ```C#
   public static void buildTemplate(string[] fileNames, ref List<TemplatePyramid> templPyrs, bool saveToXml = false)
   ```
  
* __build template from a proper XML file__

  ```C#
          public static  List<TemplatePyramid> fromXML(String fileName)
  ```
  build the templates with an XML that maybe created in the previous stages.
  
*NOTE: if needed to build __one specific__ template to the templateList, use:*
     
 ```C#
          TemplatePyramid newTemp = TemplatePyramid.CreatePyramidFromPreparedBWImage(
                        preparedBWImage, templateName, ImageAngle, maxNumberOfFeaturesPerLevel: maxFeaturesPerLevel);     
          templateList.Add(newTemp);
 ```
    
     
### #2 find templates:

find the object through the template list with/without a process time measured

```C#
public static List<Match> findObjects(Bgr<byte>[,] image, List<TemplatePyramid> templPyrs, int Threshold = 80, String[] labels = null, int minDetectionsPerGroup = 0, Func<List<Match>, List<Match>> userFunc = null)
```
```C#
public static List<Match> findObjects(Bgr<byte>[,] image, List<TemplatePyramid> templPyrs,  out long preprocessTime, out long matchTime, int Threshold = 80, String[] labels = null, int minDetectionsPerGroup = 0, Func<List<Match>, List<Match>> userFunc = null)
```


## DEMO:

For now it's a demo which is basically a state machine :
INIT -> BUILD TEMPLATE -> CALIBRATE THE TEMPLATE IMAGE -> ROTATE TO GET TEMPLATE IN DIFFERENT ANGLES -> DONE

For INIT State:
![init-illustration](https://user-images.githubusercontent.com/22462126/42083095-771595f2-7bbc-11e8-82bd-de8a05cd114a.PNG)

USER: put the object within the red circle and make sure the gap between red and green circle is clear, then you can hit the button!

BEHIND THE SENCE:
    The image is actually crop into square sized as the blue square in the image, 
    rotate then crop X 360 to get full degrees
    
AFTER PRESSING THE BUTTON:

![template-preview2](https://user-images.githubusercontent.com/22462126/42362241-f9a27b80-8124-11e8-955e-c5c7b7dc5ed3.PNG)


the system build a template based on this frame and adjust it to fit the framework and display the features (of templates) superimposed on the frame work 

Then the program will ask you to confirm the template, after making sure that no blue dot is in between two circles, press yes then it would start building the templates in different angles and sizes (only 1 size is used in the demo)

Then if there is no intent to build another template, confirm the process and the program would go to the last state in which it would continuously find the template in the frame captured by the camera and output `name, size, angle, X, Y ` of any found object.

Multi-Template scanned simultaneously :

![multi-template](https://user-images.githubusercontent.com/22462126/42547047-a965a6ca-84f2-11e8-8226-9e23845176a8.PNG)


    TO BE UPDATED
