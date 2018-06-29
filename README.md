# Siasun-Template-Matching
The template matching algorithm for Siasun Robotics

affiliate with dajuric's image processing library and algorthim at
https://github.com/dajuric/accord-net-extensions

 <a href="https://github.com/dajuric/accord-net-extensions/raw/master/Deployment/Documentation/Help/Accord.NET%20Extensions%20Documentation.chm"> 
 <img src="https://img.shields.io/badge/Help-download-yellow.svg?style=flat-square" alt="Help"/>  </a>


For now it's a demo which is basically a state machine :
INIT -> BUILD TEMPLATE -> CALIBRATE THE TEMPLATE IMAGE -> ROTATE TO GET TEMPLATE IN DIFFERENT ANGLES -> DONE

For INIT State:
![init-illustration](https://user-images.githubusercontent.com/22462126/42083095-771595f2-7bbc-11e8-82bd-de8a05cd114a.PNG)

USER: put the object within the red circle and make sure the gap between red and green circle is clear, then you can hit the button!

BEHIND THE SENCE:
    The image is actually crop into square sized as the blue square in the image, 
    rotate then crop X 360 to get full degrees
    