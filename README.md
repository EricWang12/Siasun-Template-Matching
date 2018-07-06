# Siasun-Template-Matching
The template matching algorithm for Siasun Robotics

affiliate with dajuric's image processing library and algorthim at
https://github.com/dajuric/accord-net-extensions

 <a href="https://github.com/dajuric/accord-net-extensions/raw/master/Deployment/Documentation/Help/Accord.NET%20Extensions%20Documentation.chm"> 
 <img src="https://img.shields.io/badge/Help-download-yellow.svg?style=flat-square" alt="Help"/>  </a>

First use:
USING THE LIBRARIES: 
Three of the libraries are included----

* Image processing

 __Accord.Extensions.Imaging.Algorithms package__ 
 Implements image processing algorithms as .NET array extensions including the Accord.NET algorithms.

* Math libraries

 __Accord.Extensions.Math package__ 
  Fluent matrix extensions. Geometry and graph structures and extensions.

* Support libraries

 __Accord.Extensions.Imaging.AForgeInterop package__ 
 Interoperability extensions between .NET array and AForge's UnmanagedImage.

in order to use the libraries, TYPE in the package manager:

    PM> Install-Package Accord.Extensions.Imaging.Algorithms -Version 3.0.1
    PM> Install-Package Accord.Extensions.Math -Version 3.0.1
    PM> Install-Package Accord.Extensions.Imaging.AForgeInterop -Version 3.0.1


DEMO:

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

    TO BE UPDATED
