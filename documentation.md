# Documentation
## Manifold Setup

### [Hardware](https://dl.djicdn.com/downloads/manifold-2/20190528/Manifold_2_User_Guide_v1.0_EN.pdf)

### [Software](https://github.com/dji-sdk/Onboard-SDK-ROS)
- On boot, a new Manifold requires a lot of updates (this happens automatically and will block apt-get along with get really hot for 15 or so minutes).
- ROS Kinetic should come pre-installed but feel free to follow the instructions on ROS's wiki.
- sudo apt install ros-kinetic-rosbridge-server
- Install [Onboard-SDK-ROS](https://github.com/dji-sdk/Onboard-SDK-ROS)
    - We are using Onboard-SDK-ROS commit f424a07, the commit to the master branch right before version 4.0.0 removed support for the M210 RTK v1.4
    - Create a Catkin workspace
        - mkdir catkin_ws
        - cd catkin_ws
        - catkin_init
    - clone the Onboard-SDK-ROS repo into catkin_ws/src
    - cd Onboard-SDK-ROS
    - git checkout f424a07
    - mkdir build
    - cd build
    - cmake ../cmake-modules
    - make
        - If there are any errors regarding LIBUSB, try installing it with `sudo apt-get install libusb-1.0-0-dev` 
    - cd ~/catkin_ws
    - catkin_make

Dependencies:
#### nema-comms
> $sudo apt install ros-{release}-nmea-comms  

__note:we only test on kinetic,but it should be support on other version.__
#### ffmpeg
> $sudo apt install ffmpeg  
#### libusb-1.0-0-dev
> $sudo apt install libusb-1.0-0-dev
#### libsdl2-dev
> $sudo apt install libsdl2-dev
#### opencv3.x
We use OpenCV to show images from camera stream. Download and install instructions can be found at: http://opencv.org. Tested with OpenCV 3.3.0.Suggest using 3.3.0+.
#### stereo-vision function
Follow the instruction of [here](https://developer.dji.com/onboard-sdk/documentation/sample-doc/advanced-sensing-stereo-depth-perception.html).

### 3.Permission
#### uart permission
You need to add your user to the dialout group to obtain read/write permissions for the uart communication.
>$sudo usermod -a -G dialout ${USER}  
>
Then log out of your user account and log in again for the permissions to take effect.

#### usb permission
You will need to add an udev file to allow your system to obtain permission and to identify DJI USB port.
>$cd /etc/udev/rules.d/  
>$sudo vi DJIDevice.rules

Then add these content into DJIDevice.rules.
>SUBSYSTEM=="usb", ATTRS{idVendor}=="2ca3", MODE="0666"

At last,you need to reboot your computer to make sure it works.


## Enabling Advanced Sensing:
    - On line 19 of Onboard-SDK-ROS/cmake-modules/External_djiosdkcore.cmake, either turn `-DADVANCED_SENSING=ON`(default) or `OFF`
